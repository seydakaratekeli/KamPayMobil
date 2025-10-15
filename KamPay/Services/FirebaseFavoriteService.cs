using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using KamPay.Helpers;
using KamPay.Models;

namespace KamPay.Services
{
    public class FirebaseFavoriteService : IFavoriteService
    {
        private readonly FirebaseClient _firebaseClient;
    

        private readonly INotificationService _notificationService;


        public FirebaseFavoriteService(INotificationService notificationService)
        {
            _firebaseClient = new FirebaseClient(Constants.FirebaseRealtimeDbUrl);
            _notificationService = notificationService; 
        }

        public async Task<ServiceResult<Favorite>> AddToFavoritesAsync(string userId, string productId)
        {
            try
            {
                // 1. Zaten favorilerde mi kontrol et
                var existingFavorites = await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .OnceAsync<Favorite>();

                var existing = existingFavorites
                    .Select(f => f.Object)
                    .FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

                if (existing != null)
                {
                    return ServiceResult<Favorite>.FailureResult("�r�n zaten favorilerde");
                }

                // 2. �r�n bilgilerini al
                var product = await _firebaseClient
                    .Child(Constants.ProductsCollection)
                    .Child(productId)
                    .OnceSingleAsync<Product>();

                if (product == null)
                {
                    return ServiceResult<Favorite>.FailureResult("�r�n bulunamad�");
                }

                // 3. Favori nesnesini olu�tur
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductTitle = product.Title,
                    ProductThumbnail = product.ThumbnailUrl,
                    ProductPrice = product.Price,
                    ProductType = product.Type
                };

                // 4. Favoriyi veritaban�na kaydet
                await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .Child(favorite.FavoriteId)
                    .PutAsync(favorite);

                // 5. �r�n�n favori say�s�n� art�r
                product.FavoriteCount++;
                await _firebaseClient
                    .Child(Constants.ProductsCollection)
                    .Child(productId)
                    .PutAsync(product);

                // 6. Bildirim mant���n� uygula
                var isOwner = product.UserId == userId;

                // Sadece �r�n�n sahibi olmayan biri favoriye ekledi�inde bildirim g�nder
                if (!isOwner)
                {
                    var currentUser = await _firebaseClient.Child(Constants.UsersCollection).Child(userId).OnceSingleAsync<User>();
                    var notification = new Notification
                    {
                        UserId = product.UserId, // Bildirimi alacak ki�i (�R�N SAH�B�)
                        Type = NotificationType.NewFavorite,
                        Title = "Yeni Favori!",
                        Message = $"{currentUser.FullName}, '{product.Title}' adl� �r�n�n� favorilerine ekledi.",
                        RelatedEntityId = product.ProductId,
                        RelatedEntityType = "Product",
                        ActionUrl = $"ProductDetailPage?productId={product.ProductId}"
                    };

                    // Bildirimi olu�tur (Bu metot art�k anl�k sinyal g�ndermiyor, sadece kaydeder)
                    await _notificationService.CreateNotificationAsync(notification);
                }

                return ServiceResult<Favorite>.SuccessResult(favorite, "Favorilere eklendi");
            }
            catch (Exception ex)
            {
                return ServiceResult<Favorite>.FailureResult("Favorilere eklenirken bir hata olu�tu", ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> RemoveFromFavoritesAsync(string userId, string productId)
        {
            try
            {
                var allFavorites = await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .OnceAsync<Favorite>();

                var favorite = allFavorites
                    .FirstOrDefault(f => f.Object.UserId == userId && f.Object.ProductId == productId);

                if (favorite == null)
                {
                    return ServiceResult<bool>.FailureResult("Favoride bulunamad�");
                }

                // Favoriyi sil
                await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .Child(favorite.Object.FavoriteId)
                    .DeleteAsync();

                // �r�n�n favori say�s�n� azalt
                var product = await _firebaseClient
                    .Child(Constants.ProductsCollection)
                    .Child(productId)
                    .OnceSingleAsync<Product>();

                if (product != null && product.FavoriteCount > 0)
                {
                    product.FavoriteCount--;
                    await _firebaseClient
                        .Child(Constants.ProductsCollection)
                        .Child(productId)
                        .PutAsync(product);
                }

                return ServiceResult<bool>.SuccessResult(true, "Favorilerden ��kar�ld�");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("��kar�lamad�", ex.Message);
            }
        }

        public async Task<ServiceResult<List<Favorite>>> GetUserFavoritesAsync(string userId)
        {
            try
            {
                var allFavorites = await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .OnceAsync<Favorite>();

                var favorites = allFavorites
                    .Select(f => f.Object)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToList();

                return ServiceResult<List<Favorite>>.SuccessResult(favorites);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Favorite>>.FailureResult("Y�klenemedi", ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> IsFavoriteAsync(string userId, string productId)
        {
            try
            {
                var allFavorites = await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .OnceAsync<Favorite>();

                var isFavorite = allFavorites
                    .Select(f => f.Object)
                    .Any(f => f.UserId == userId && f.ProductId == productId);

                return ServiceResult<bool>.SuccessResult(isFavorite);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult("Kontrol edilemedi", ex.Message);
            }
        }

        public async Task<ServiceResult<int>> GetProductFavoriteCountAsync(string productId)
        {
            try
            {
                var allFavorites = await _firebaseClient
                    .Child(Constants.FavoritesCollection)
                    .OnceAsync<Favorite>();

                var count = allFavorites
                    .Select(f => f.Object)
                    .Count(f => f.ProductId == productId);

                return ServiceResult<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult("Say�lamad�", ex.Message);
            }
        }
    }
}