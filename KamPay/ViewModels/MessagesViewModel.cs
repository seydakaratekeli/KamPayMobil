using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KamPay.Models;
using KamPay.Services;
using KamPay.Views;

namespace KamPay.ViewModels
{
    // Mesajlar Listesi ViewModel
    public partial class MessagesViewModel : ObservableObject
    {
        private readonly IMessagingService _messagingService;
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private int unreadCount;

        [ObservableProperty]
        private string emptyMessage = "Hen�z mesaj�n�z yok";

        public ObservableCollection<Conversation> Conversations { get; } = new();

        public MessagesViewModel(IMessagingService messagingService, IAuthenticationService authService)
        {
            _messagingService = messagingService;
            _authService = authService;

            LoadConversationsAsync();
        }

        [RelayCommand]
        private async Task LoadConversationsAsync()
        {
            try
            {
                IsLoading = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null) return;

                var result = await _messagingService.GetUserConversationsAsync(currentUser.UserId);

                if (result.Success && result.Data != null)
                {
                    Conversations.Clear();
                    foreach (var conversation in result.Data)
                    {
                        // ===== YEN� EKLENEN MANTIK =====
                        // Her konu�ma i�in "di�er kullan�c�n�n" bilgilerini ayarla
                        conversation.OtherUserName = conversation.GetOtherUserName(currentUser.UserId);
                        conversation.OtherUserPhotoUrl = conversation.GetOtherUserPhotoUrl(currentUser.UserId);
                        conversation.UnreadCount = conversation.GetUnreadCount(currentUser.UserId);
                        // ===============================

                        Conversations.Add(conversation);
                    }

                    EmptyMessage = Conversations.Any()
                        ? string.Empty
                        : "Hen�z mesaj�n�z yok\nBir �r�n sahibiyle ileti�ime ge�in!";

                    // Okunmam�� mesaj say�s�n� g�ncelle
                    UnreadCount = Conversations.Sum(c => c.UnreadCount);
                    WeakReferenceMessenger.Default.Send(new UnreadMessageStatusMessage(UnreadCount > 0));


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
        private async Task RefreshConversationsAsync()
        {
            IsRefreshing = true;
            await LoadConversationsAsync();
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task ConversationTappedAsync(Conversation conversation)
        {
            if (conversation == null) return;

            await Shell.Current.GoToAsync($"{nameof(ChatPage)}?conversationId={conversation.ConversationId}");
        }

        [RelayCommand]
        private async Task DeleteConversationAsync(Conversation conversation)
        {
            if (conversation == null) return;

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Onay",
                "Bu konu�may� silmek istedi�inize emin misiniz?",
                "Evet",
                "Hay�r"
            );

            if (!confirm) return;

            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                var result = await _messagingService.DeleteConversationAsync(conversation.ConversationId, currentUser.UserId);

                if (result.Success)
                {
                    Conversations.Remove(conversation);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }


}