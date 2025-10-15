using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using KamPay.Services; 
using KamPay.Helpers; 
using Firebase.Database;
using Firebase.Database.Query; 
using System.Reactive.Linq; 
using KamPay.Models; 


namespace KamPay.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool hasUnreadNotifications;

        [ObservableProperty]
        private bool hasUnreadMessages;

        private readonly IAuthenticationService _authService;
        private readonly IMessagingService _messagingService;
        private IDisposable? _messageSubscription;

        public AppShellViewModel(IAuthenticationService authService, IMessagingService messagingService)
        {
            _authService = authService;
            _messagingService = messagingService;

            // Genel bildirimleri dinle 
            WeakReferenceMessenger.Default.Register<UnreadGeneralNotificationStatusMessage>(this, (r, m) =>
            {
                HasUnreadNotifications = m.Value;
            });

            // Mesaj bildirimlerini dinle
            WeakReferenceMessenger.Default.Register<UnreadMessageStatusMessage>(this, (r, m) =>
            {
                HasUnreadMessages = m.Value;
            });

            // Kullan�c� giri� / ��k�� yapt���nda dinleyiciyi ba�lat / durdur
            WeakReferenceMessenger.Default.Register<UserSessionChangedMessage>(this, (r, m) =>
            {
                if (m.Value) // Giri� yap�ld�
                {
                    StartListeningForMessages();
                }
                else // ��k�� yap�ld�
                {
                    StopListeningForMessages();
                    HasUnreadMessages = false;
                }
            });
        }
        private async void StartListeningForMessages()
        {
            StopListeningForMessages(); // �nceki dinleyiciyi durdur

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null) return;

            // Uygulama a��ld���nda ilk kontrol� yap
            var initialCheckResult = await _messagingService.GetTotalUnreadMessageCountAsync(currentUser.UserId);
            if (initialCheckResult.Success && initialCheckResult.Data > 0)
            {
                HasUnreadMessages = true;
            }

            // Ger�ek zamanl� dinleyiciyi ba�lat
            var firebaseClient = new FirebaseClient(Constants.FirebaseRealtimeDbUrl);
            _messageSubscription = firebaseClient
                .Child(Constants.ConversationsCollection)
                .AsObservable<Conversation>()
                .Where(e => e.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate &&
                            e.Object != null &&
                            (e.Object.User1Id == currentUser.UserId || e.Object.User2Id == currentUser.UserId))
                .Subscribe(async entry =>
                {
                    // Kullan�c�ya ait bir konu�ma g�ncellendi�inde, toplam okunmam�� say�s�n� yeniden kontrol et
                    var result = await _messagingService.GetTotalUnreadMessageCountAsync(currentUser.UserId);
                    if (result.Success)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            HasUnreadMessages = result.Data > 0;
                        });
                    }
                });
        }

        private void StopListeningForMessages()
        {
            _messageSubscription?.Dispose();
            _messageSubscription = null;
        }
    }

        public class UnreadGeneralNotificationStatusMessage : CommunityToolkit.Mvvm.Messaging.Messages.ValueChangedMessage<bool>
    {
        public UnreadGeneralNotificationStatusMessage(bool value) : base(value) { }
    }

    public class UnreadMessageStatusMessage : CommunityToolkit.Mvvm.Messaging.Messages.ValueChangedMessage<bool>
    {
        public UnreadMessageStatusMessage(bool value) : base(value) { }
    }
    public class UserSessionChangedMessage : CommunityToolkit.Mvvm.Messaging.Messages.ValueChangedMessage<bool>
    {
        public UserSessionChangedMessage(bool isLoggedIn) : base(isLoggedIn) { }
    }
}