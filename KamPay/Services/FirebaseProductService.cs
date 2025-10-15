using Firebase.Database;
using Firebase.Database.Query;
using KamPay.Helpers;
using KamPay.Models;

namespace KamPay.Services;
public class FirebaseProductService : IProductService
{
    private readonly FirebaseClient _firebaseClient;
    private readonly IStorageService _storageService;

    public FirebaseProductService(IStorageService storageService)
    {
        _firebaseClient = new FirebaseClient(Constants.FirebaseRealtimeDbUrl);
        _storageService = storageService;
    }

    public async Task<ServiceResult<Product>> AddProductAsync(ProductRequest request, User currentUser)
    {
        try
        {
            // 1. Validasyon
            var validation = ValidateProduct(request);
            if (!validation.IsValid)
            {
                return ServiceResult<Product>.FailureResult(
                    "�r�n bilgileri ge�ersiz",
                    validation.Errors.ToArray()
                );
            }

            // 2. �r�n nesnesi olu�tur
            var product = new Product
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                CategoryId = request.CategoryId,
                Condition = request.Condition,
                Type = request.Type,
                Price = request.Price,
                Location = request.Location?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                UserId = currentUser.UserId,
                UserName = currentUser.FullName,
                UserEmail = currentUser.Email,
                UserPhotoUrl = currentUser.ProfileImageUrl,
                ExchangePreference = request.ExchangePreference?.Trim()
            };

            // 3. Kategori ad�n� al
            var categories = await GetCategoriesAsync();
            var category = categories.Data?.FirstOrDefault(c => c.CategoryId == request.CategoryId);
            if (category != null)
            {
                product.CategoryName = category.Name;
            }

            // 4. G�rselleri y�kle
            if (request.ImagePaths != null && request.ImagePaths.Any())
            {
                var imageUrls = new List<string>();

                for (int i = 0; i < Math.Min(request.ImagePaths.Count, Constants.MaxProductImages); i++)
                {
                    var uploadResult = await _storageService.UploadProductImageAsync(
                        request.ImagePaths[i],
                        product.ProductId,
                        i
                    );

                    if (uploadResult.Success)
                    {
                        imageUrls.Add(uploadResult.Data);
                    }
                }

                product.ImageUrls = imageUrls;
                if (imageUrls.Any())
                {
                    product.ThumbnailUrl = imageUrls.First();
                }
            }

            // 5. Firebase'e kaydet
            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(product.ProductId)
                .PutAsync(product);

            return ServiceResult<Product>.SuccessResult(
                product,
                "�r�n ba�ar�yla eklendi!"
            );
        }
        catch (Exception ex)
        {
            return ServiceResult<Product>.FailureResult(
                "�r�n eklenirken hata olu�tu",
                ex.Message
            );
        }
    }

    public async Task<ServiceResult<Product>> UpdateProductAsync(string productId, ProductRequest request)
    {
        try
        {
            // Validasyon
            var validation = ValidateProduct(request);
            if (!validation.IsValid)
            {
                return ServiceResult<Product>.FailureResult(
                    "�r�n bilgileri ge�ersiz",
                    validation.Errors.ToArray()
                );
            }

            // Mevcut �r�n� al
            var existingProduct = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (existingProduct == null)
            {
                return ServiceResult<Product>.FailureResult("�r�n bulunamad�");
            }

            // G�ncelle
            existingProduct.Title = request.Title.Trim();
            existingProduct.Description = request.Description.Trim();
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.Condition = request.Condition;
            existingProduct.Type = request.Type;
            existingProduct.Price = request.Price;
            existingProduct.Location = request.Location?.Trim();
            existingProduct.Latitude = request.Latitude;
            existingProduct.Longitude = request.Longitude;
            existingProduct.ExchangePreference = request.ExchangePreference?.Trim();
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Kategori ad�n� g�ncelle
            var categories = await GetCategoriesAsync();
            var category = categories.Data?.FirstOrDefault(c => c.CategoryId == request.CategoryId);
            if (category != null)
            {
                existingProduct.CategoryName = category.Name;
            }

            // Yeni g�rseller varsa y�kle
            if (request.ImagePaths != null && request.ImagePaths.Any())
            {
                var newImageUrls = new List<string>();

                for (int i = 0; i < Math.Min(request.ImagePaths.Count, Constants.MaxProductImages); i++)
                {
                    var uploadResult = await _storageService.UploadProductImageAsync(
                        request.ImagePaths[i],
                        productId,
                        i
                    );

                    if (uploadResult.Success)
                    {
                        newImageUrls.Add(uploadResult.Data);
                    }
                }

                existingProduct.ImageUrls = newImageUrls;
                if (newImageUrls.Any())
                {
                    existingProduct.ThumbnailUrl = newImageUrls.First();
                }
            }

            // Firebase'e kaydet
            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .PutAsync(existingProduct);

            return ServiceResult<Product>.SuccessResult(
                existingProduct,
                "�r�n g�ncellendi"
            );
        }
        catch (Exception ex)
        {
            return ServiceResult<Product>.FailureResult(
                "G�ncelleme hatas�",
                ex.Message
            );
        }
    }

    public async Task<ServiceResult<bool>> DeleteProductAsync(string productId)
    {
        try
        {
            var product = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("�r�n bulunamad�");
            }

            // 1. �r�n�n g�rsellerini sil
            if (product.ImageUrls != null && product.ImageUrls.Any())
            {
                foreach (var imageUrl in product.ImageUrls)
                {
                    await _storageService.DeleteImageAsync(imageUrl);
                }
            }

            //  �L��K�L� FAVOR�LER� S�LME 
            // Bu �r�ne ait t�m favori kay�tlar�n� bul
            var allFavorites = await _firebaseClient
                .Child(Constants.FavoritesCollection)
                .OrderBy("ProductId")
                .EqualTo(productId)
                .OnceAsync<Favorite>();

            if (allFavorites.Any())
            {
                // Bulunan her favori kayd�n� sil
                foreach (var favoriteEntry in allFavorites)
                {
                    await _firebaseClient
                        .Child(Constants.FavoritesCollection)
                        .Child(favoriteEntry.Key) // .Key kullanarak favorinin kendi ID'sini al�yoruz
                        .DeleteAsync();
                }
            }

            // 2. �r�n�n kendisini sil
            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .DeleteAsync();

            return ServiceResult<bool>.SuccessResult(true, "�r�n ve ili�kili favoriler silindi");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult("Silme hatas�", ex.Message);
        }
    }
    public async Task<ServiceResult<Product>> GetProductByIdAsync(string productId)
    {
        try
        {
            var product = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (product == null)
            {
                return ServiceResult<Product>.FailureResult("�r�n bulunamad�");
            }

            return ServiceResult<Product>.SuccessResult(product);
        }
        catch (Exception ex)
        {
            return ServiceResult<Product>.FailureResult("Hata olu�tu", ex.Message);
        }
    }

    public async Task<ServiceResult<List<Product>>> GetAllProductsAsync(ProductFilter filter = null)
    {
        try
        {
            var allProducts = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .OnceAsync<Product>();

            var products = allProducts.Select(p => p.Object).ToList();

            // Filtreleme
            if (filter != null)
            {
                // Sadece aktif �r�nler
                if (filter.OnlyActive)
                {
                    products = products.Where(p => p.IsActive).ToList();
                }

                // Sat�lmam�� �r�nler
                if (filter.ExcludeSold)
                {
                    products = products.Where(p => !p.IsSold).ToList();
                }

                // Arama metni
                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    var searchLower = filter.SearchText.ToLower();
                    products = products.Where(p =>
                        p.Title.ToLower().Contains(searchLower) ||
                        p.Description.ToLower().Contains(searchLower)
                    ).ToList();
                }

                // Kategori
                if (!string.IsNullOrWhiteSpace(filter.CategoryId))
                {
                    products = products.Where(p => p.CategoryId == filter.CategoryId).ToList();
                }

                // Tip
                if (filter.Type.HasValue)
                {
                    products = products.Where(p => p.Type == filter.Type.Value).ToList();
                }

                // Durum
                if (filter.Condition.HasValue)
                {
                    products = products.Where(p => p.Condition == filter.Condition.Value).ToList();
                }

                // Fiyat aral���
                if (filter.MinPrice.HasValue)
                {
                    products = products.Where(p => p.Price >= filter.MinPrice.Value).ToList();
                }

                if (filter.MaxPrice.HasValue)
                {
                    products = products.Where(p => p.Price <= filter.MaxPrice.Value).ToList();
                }

                // Konum
                if (!string.IsNullOrWhiteSpace(filter.Location))
                {
                    var locationLower = filter.Location.ToLower();
                    products = products.Where(p =>
                        p.Location != null && p.Location.ToLower().Contains(locationLower)
                    ).ToList();
                }

                // S�ralama
                products = filter.SortBy switch
                {
                    ProductSortOption.Newest => products.OrderByDescending(p => p.CreatedAt).ToList(),
                    ProductSortOption.Oldest => products.OrderBy(p => p.CreatedAt).ToList(),
                    ProductSortOption.PriceAsc => products.OrderBy(p => p.Price).ToList(),
                    ProductSortOption.PriceDesc => products.OrderByDescending(p => p.Price).ToList(),
                    ProductSortOption.MostViewed => products.OrderByDescending(p => p.ViewCount).ToList(),
                    ProductSortOption.MostFavorited => products.OrderByDescending(p => p.FavoriteCount).ToList(),
                    _ => products.OrderByDescending(p => p.CreatedAt).ToList()
                };
            }

            return ServiceResult<List<Product>>.SuccessResult(products);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<Product>>.FailureResult("�r�nler y�klenemedi", ex.Message);
        }
    }
    public async Task<ServiceResult<List<Product>>> GetUserProductsAsync(string userId)
    {
        try
        {
            var allProducts = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .OrderBy("UserId")
                .EqualTo(userId)
                .OnceAsync<Product>();

            // �r�n ID'lerini Firebase Key'lerinden al�p nesnelere ata
            foreach (var product in allProducts)
            {
                product.Object.ProductId = product.Key;
            }

         

            // Sadece aktif, sat�lmam�� VE rezerve edilmemi� �r�nleri filtrele
            var products = allProducts
                .Select(p => p.Object)
                .Where(p => p.IsActive && !p.IsSold && !p.IsReserved) // Filtreleme burada yap�l�yor
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

        

            return ServiceResult<List<Product>>.SuccessResult(products);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<Product>>.FailureResult("Kullan�c�n�n �r�nleri al�namad�.", ex.Message);
        }
    }
    public async Task<ServiceResult<bool>> MarkAsSoldAsync(string productId)
    {
        try
        {
            var product = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("�r�n bulunamad�");
            }

            product.IsSold = true;
            product.SoldAt = DateTime.UtcNow;
            product.IsActive = false;

            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .PutAsync(product);

            return ServiceResult<bool>.SuccessResult(true, "�r�n sat�ld� olarak i�aretlendi");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult("��lem ba�ar�s�z", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> MarkAsReservedAsync(string productId, bool isReserved)
    {
        try
        {
            var product = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("�r�n bulunamad�");
            }

            product.IsReserved = isReserved;

            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .PutAsync(product);

            var message = isReserved ? "�r�n rezerve edildi" : "Rezervasyon kald�r�ld�";
            return ServiceResult<bool>.SuccessResult(true, message);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult("��lem ba�ar�s�z", ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> IncrementViewCountAsync(string productId)
    {
        try
        {
            var product = await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .OnceSingleAsync<Product>();

            if (product == null)
            {
                return ServiceResult<bool>.FailureResult("�r�n bulunamad�");
            }

            product.ViewCount++;

            await _firebaseClient
                .Child(Constants.ProductsCollection)
                .Child(productId)
                .PutAsync(product);

            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult("Hata", ex.Message);
        }
    }

    public async Task<ServiceResult<List<Category>>> GetCategoriesAsync()
    {
        try
        {
            // �nce Firebase'den kategorileri almay� dene
            var firebaseCategories = await _firebaseClient
                .Child(Constants.CategoriesCollection)
                .OnceAsync<Category>();

            if (firebaseCategories.Any())
            {
                var categories = firebaseCategories.Select(c => c.Object).ToList();
                return ServiceResult<List<Category>>.SuccessResult(categories);
            }

            // E�er kategori yoksa, varsay�lanlar� ekle
            var defaultCategories = Category.GetDefaultCategories();
            foreach (var category in defaultCategories)
            {
                await _firebaseClient
                    .Child(Constants.CategoriesCollection)
                    .Child(category.CategoryId)
                    .PutAsync(category);
            }

            return ServiceResult<List<Category>>.SuccessResult(defaultCategories);
        }
        catch (Exception ex)
        {
            // Hata durumunda varsay�lan kategorileri d�nd�r
            return ServiceResult<List<Category>>.SuccessResult(
                Category.GetDefaultCategories(),
                "Kategoriler yerel olarak y�klendi"
            );
        }
    }

    public ValidationResult ValidateProduct(ProductRequest request)
    {
        var result = new ValidationResult();

        // Ba�l�k kontrol�
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            result.AddError("�r�n ba�l��� bo� olamaz");
        }
        else if (request.Title.Length > Constants.MaxProductTitleLength)
        {
            result.AddError($"Ba�l�k en fazla {Constants.MaxProductTitleLength} karakter olabilir");
        }

        // A��klama kontrol�
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            result.AddError("�r�n a��klamas� bo� olamaz");
        }
        else if (request.Description.Length > Constants.MaxProductDescriptionLength)
        {
            result.AddError($"A��klama en fazla {Constants.MaxProductDescriptionLength} karakter olabilir");
        }

        // Kategori kontrol�
        if (string.IsNullOrWhiteSpace(request.CategoryId))
        {
            result.AddError("Kategori se�ilmelidir");
        }

        // Fiyat kontrol� (Sat�l�k �r�nler i�in)
        if (request.Type == ProductType.Satis)
        {
            if (request.Price <= 0)
            {
                result.AddError("Sat�� fiyat� 0'dan b�y�k olmal�d�r");
            }
            else if (request.Price > 999999)
            {
                result.AddError("Fiyat �ok y�ksek");
            }
        }

        // G�rsel kontrol�
        if (request.ImagePaths != null && request.ImagePaths.Count > Constants.MaxProductImages)
        {
            result.AddError($"En fazla {Constants.MaxProductImages} g�rsel eklenebilir");
        }

        // Takas i�in tercih kontrol�
        if (request.Type == ProductType.Takas && string.IsNullOrWhiteSpace(request.ExchangePreference))
        {
            result.AddError("Takas i�in tercih belirtilmelidir");
        }

        return result;
    }
}
