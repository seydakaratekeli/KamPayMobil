using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KamPay.Models;
using KamPay.Services;
using System.Linq;
using KamPay.Views;

namespace KamPay.ViewModels
{
    // Favoriler ViewModel
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IProductService _productService;
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string emptyMessage = "Hen�z favori �r�n�n�z yok";

        public ObservableCollection<Favorite> FavoriteItems { get; } = new();

        public FavoritesViewModel(IFavoriteService favoriteService, IProductService productService, IAuthenticationService authService)
        {
            _favoriteService = favoriteService;
            _productService = productService;
            _authService = authService;

            LoadFavoritesAsync();
        }

        [RelayCommand]
        private async Task LoadFavoritesAsync()
        {
            try
            {
                IsLoading = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return;

                // 1. Sadece favori listesini �ek (Bu art�k yeterli!)
                var favoritesResult = await _favoriteService.GetUserFavoritesAsync(currentUser.UserId);

                if (favoritesResult.Success && favoritesResult.Data != null)
                {
                    FavoriteItems.Clear();
                    foreach (var favorite in favoritesResult.Data)
                    {
                        // 2. Art�k _productService'i �a��rmaya GEREK YOK!
                        FavoriteItems.Add(favorite);
                    }

                    EmptyMessage = FavoriteItems.Any()
                        ? string.Empty
                        : "Hen�z favori �r�n�n�z yok\nBe�endi�iniz �r�nleri favorilere ekleyin!";
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshFavoritesAsync()
        {
            IsRefreshing = true;
            await LoadFavoritesAsync();
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task ProductTappedAsync(Favorite favorite)
        {
            if (favorite == null) return;
            await Shell.Current.GoToAsync($"ProductDetailPage?productId={favorite.ProductId}");
        }

        [RelayCommand]
        private async Task RemoveFavoriteAsync(Favorite favorite)
        {
            if (favorite == null) return;

            var currentUser = await _authService.GetCurrentUserAsync();
            var result = await _favoriteService.RemoveFromFavoritesAsync(currentUser.UserId, favorite.ProductId);

            if (result.Success)
            {
                FavoriteItems.Remove(favorite);
            }
        }
    }
}