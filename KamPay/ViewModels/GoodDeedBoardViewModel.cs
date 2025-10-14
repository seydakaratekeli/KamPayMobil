using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using KamPay.Models;
using KamPay.Services;

namespace KamPay.ViewModels;

// ===== �Y�L�K PANOSU ViewModel =====
public partial class GoodDeedBoardViewModel : ObservableObject
{
    private readonly IGoodDeedService _goodDeedService;
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private PostType selectedType;

    public ObservableCollection<GoodDeedPost> Posts { get; } = new();

    public List<PostType> PostTypes { get; } = Enum.GetValues(typeof(PostType)).Cast<PostType>().ToList();

    public GoodDeedBoardViewModel(IGoodDeedService goodDeedService, IAuthenticationService authService)
    {
        _goodDeedService = goodDeedService;
        _authService = authService;
        LoadPostsAsync();
    }

    [RelayCommand]
    private async Task LoadPostsAsync()
    {
        try
        {
            IsLoading = true;

            var result = await _goodDeedService.GetPostsAsync();

            if (result.Success && result.Data != null)
            {
                Posts.Clear();
                foreach (var post in result.Data)
                {
                    Posts.Add(post);
                }
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
    private async Task CreatePostAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Uyar�", "Ba�l�k ve a��klama gerekli", "Tamam");
                return;
            }

            IsLoading = true;

            var currentUser = await _authService.GetCurrentUserAsync();

            var post = new GoodDeedPost
            {
                UserId = currentUser.UserId,
                UserName = currentUser.FullName,
                Type = SelectedType,
                Title = Title,
                Description = Description
            };

            var result = await _goodDeedService.CreatePostAsync(post);

            if (result.Success)
            {
                Posts.Insert(0, result.Data);
                Title = string.Empty;
                Description = string.Empty;

                await Application.Current.MainPage.DisplayAlert("Ba�ar�l�", "�lan payla��ld�!", "Tamam");
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
    private async Task LikePostAsync(GoodDeedPost post)
    {
        try
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            await _goodDeedService.LikePostAsync(post.PostId, currentUser.UserId);
            post.LikeCount++;
        }
        catch { }
    }

    [RelayCommand]
    private async Task DeletePostAsync(GoodDeedPost post)
    {
        try
        {
            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Sil",
                "Bu ilan� silmek istedi�inize emin misiniz?",
                "Evet",
                "Hay�r"
            );

            if (!confirm) return;

            var currentUser = await _authService.GetCurrentUserAsync();
            var result = await _goodDeedService.DeletePostAsync(post.PostId, currentUser.UserId);

            if (result.Success)
            {
                Posts.Remove(post);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}
