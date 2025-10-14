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

    // Chat (Sohbet) ViewModel
    [QueryProperty(nameof(ConversationId), "conversationId")]

    public partial class ChatViewModel : ObservableObject
    {
        private readonly IMessagingService _messagingService;
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string conversationId;

        [ObservableProperty]
        private Conversation conversation;

        [ObservableProperty]
        private string otherUserName;

        [ObservableProperty]
        private string otherUserPhoto;

        [ObservableProperty]
        private string messageText;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isSending;

        public ObservableCollection<Message> Messages { get; } = new();

        // Bu Action, View'a (ChatPage.xaml.cs) son mesaja kayd�rmas� i�in sinyal g�nderecek.
      //  public Action<Message> OnMessageSent { get; set; }

        private User _currentUser;

        public ChatViewModel(IMessagingService messagingService, IAuthenticationService authService)
        {
            _messagingService = messagingService;
            _authService = authService;
        }

        partial void OnConversationIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _ = LoadChatAsync();
            }
        }

        [RelayCommand]
        private async Task LoadChatAsync()
        {
            try
            {
                IsLoading = true;
                Messages.Clear();

                _currentUser = await _authService.GetCurrentUserAsync();
                if (_currentUser == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Giri� yapm�� kullan�c� bulunamad�.", "Tamam");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Konu�ma bilgilerini al
                var conversations = await _messagingService.GetUserConversationsAsync(_currentUser.UserId);
                Conversation = conversations.Data?.FirstOrDefault(c => c.ConversationId == ConversationId);

                if (Conversation != null)
                {
                    OtherUserName = Conversation.GetOtherUserName(_currentUser.UserId);
                    OtherUserPhoto = Conversation.GetOtherUserPhotoUrl(_currentUser.UserId);
                }

                // Mesajlar� y�kle
                var messagesResult = await _messagingService.GetConversationMessagesAsync(ConversationId);

                if (messagesResult.Success && messagesResult.Data != null)
                {
                    Messages.Clear();
                    foreach (var msg in messagesResult.Data)
                    {
                        //  Her mesaj i�in IsSentByMe �zelli�ini ayarl�yoruz
                        msg.IsSentByMe = msg.SenderId == _currentUser.UserId;
                        Messages.Add(msg);
                    }
                }

                // Mesajlar� okundu i�aretle
                await _messagingService.MarkMessagesAsReadAsync(ConversationId, _currentUser.UserId);
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
        private async Task SendMessageAsync()
        {
            // --- KONTROL 1: Gerekli bilgiler y�klenmi� mi veya mesaj bo� mu? ---
            if (IsSending || string.IsNullOrWhiteSpace(MessageText) || _currentUser == null || Conversation == null)
            {
                // E�er kullan�c� veya sohbet bilgisi hen�z y�klenmediyse veya kutu bo�sa hi�bir �ey yapma.
                return;
            }

            // 2. Mesaj i�eri�ini ge�ici bir de�i�kene kaydet
            var messageContent = MessageText;

            // 3. UI'daki metin kutusunu ANINDA temizle (kullan�c� deneyimi i�in)
            MessageText = string.Empty;

            try
            {
                IsSending = true;
               

                var request = new SendMessageRequest
                {
                    ReceiverId = Conversation.GetOtherUserId(_currentUser.UserId),
                    Content = messageContent.Trim(), // Ge�ici de�i�kenden oku
                    Type = MessageType.Text,
                    ProductId = Conversation.ProductId // Konu�madan �r�n ID'sini al

                };
                // Servise g�ndermeden �nce son bir kontrol yapal�m
                if (string.IsNullOrEmpty(request.ReceiverId) || _currentUser == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", "Al�c� veya g�nderen bilgisi bulunamad�.", "Tamam");
                    MessageText = messageContent; // Mesaj� geri y�kle
                    IsSending = false;
                    return;
                }

                var result = await _messagingService.SendMessageAsync(request, _currentUser);

                if (result.Success)
                {
                    //  Yeni g�nderilen mesaj�n da IsSentByMe �zelli�ini ayarl�yoruz
                    var sentMessage = result.Data;
                    sentMessage.IsSentByMe = true;
                    Messages.Add(sentMessage);

                    // View'a "Mesaj g�nderildi, son ��eye kayd�r" sinyalini g�nder.
                    WeakReferenceMessenger.Default.Send(new ScrollToChatMessage(sentMessage));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", result.Message, "Tamam");
                    MessageText = messageContent; // Hata durumunda yaz�y� geri getir
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Hata", ex.Message, "Tamam");
            }
            finally
            {
                IsSending = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}