using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KamPay.Models;
using KamPay.Services;

namespace KamPay.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string firstName;

        [ObservableProperty]
        private string lastName;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string passwordConfirm;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool showVerificationSection;

        [ObservableProperty]
        private string verificationCode;

        public RegisterViewModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var request = new RegisterRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = Password,
                    PasswordConfirm = PasswordConfirm
                };

                var result = await _authService.RegisterAsync(request);

                if (result.Success)
                {
                    // Kay�t ba�ar�l� -> Do�rulama k�sm�n� g�ster
                    ShowVerificationSection = true;
                    VerificationCode = string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Ba�ar�l�", result.Message ?? "Kay�t ba�ar�l�. Do�rulama kodu g�nderildi.", "Tamam");
                }
                else
                {
                    // Hata mesaj� g�ster
                    if (result.Errors != null && result.Errors.Any())
                        ErrorMessage = string.Join("\n", result.Errors);
                    else
                        ErrorMessage = result.Message ?? "Kay�t yap�lamad�.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Beklenmeyen hata: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task VerifyEmailAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var vreq = new VerificationRequest
                {
                    Email = Email,
                    VerificationCode = VerificationCode
                };

                var result = await _authService.VerifyEmailAsync(vreq);

                if (result.Success)
                {
                    ShowVerificationSection = false;
                    await Application.Current.MainPage.DisplayAlert("Do�ruland�", result.Message ?? "E-posta do�ruland�.", "Tamam");
                    
                    // Kaydolan kullan�c�y� giri� sayfas�na y�nlendir
                    //  await Shell.Current.GoToAsync("//LoginPage");

                    // =====  OTOMAT�K G�R�� YAP VE Y�NLEND�R =====
                    // Do�rulama ba�ar�l� oldu�u i�in art�k kullan�c�y� otomatik olarak i�eri alabiliriz.
                    var loginRequest = new LoginRequest { Email = Email, Password = Password, RememberMe = true };
                    var loginResult = await _authService.LoginAsync(loginRequest);

                   
                    if (loginResult.Success)
                    {
                        // Ana uygulama ekran�na y�nlendir
                        await Shell.Current.GoToAsync("//MainApp");
                    }
                    else
                    {
                        // Bir sorun olursa login sayfas�na y�nlendir
                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                }
                else
                {
                    if (result.Errors != null && result.Errors.Any())
                        ErrorMessage = string.Join("\n", result.Errors);
                    else
                        ErrorMessage = result.Message ?? "Do�rulama ba�ar�s�z.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Beklenmeyen hata: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ResendVerificationAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "E-posta alan� bo� olamaz.";
                    return;
                }

                var result = await _authService.SendVerificationCodeAsync(Email);

                if (result.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Ba�ar�l�", result.Message ?? "Do�rulama kodu yeniden g�nderildi.", "Tamam");
                }
                else
                {
                    if (result.Errors != null && result.Errors.Any())
                        ErrorMessage = string.Join("\n", result.Errors);
                    else
                        ErrorMessage = result.Message ?? "Kod g�nderilemedi.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Beklenmeyen hata: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CancelVerificationAsync()
        {
            // Do�rulama i�lemini iptal et -> kullan�c�y� login sayfas�na g�t�rebiliriz ya da ShowVerificationSection = false
            ShowVerificationSection = false;
            VerificationCode = string.Empty;
            await Task.CompletedTask;
        }

        // RegisterViewModel.cs i�inde
        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            // Bir �nceki sayfaya (LoginPage) geri d�n
            await Shell.Current.GoToAsync("..");
        }
    }
}