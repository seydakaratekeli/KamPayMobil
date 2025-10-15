using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using KamPay.Helpers;
using KamPay.Models;
using KamPay.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using KamPay.Views;

namespace KamPay.ViewModels
{
    public partial class OffersViewModel : ObservableObject, IDisposable, IQueryAttributable
    {
        private readonly ITransactionService _transactionService;
        private readonly IAuthenticationService _authService;
        private readonly IQRCodeService _qrCodeService;
        private IDisposable _incomingOffersSubscription;
        private IDisposable _outgoingOffersSubscription;
        private readonly FirebaseClient _firebaseClient = new(Constants.FirebaseRealtimeDbUrl);

        public ObservableCollection<Transaction> IncomingOffers { get; } = new();
        public ObservableCollection<Transaction> OutgoingOffers { get; } = new();

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private bool isIncomingSelected = true;

        [ObservableProperty]
        private bool isOutgoingSelected = false;

        public OffersViewModel(ITransactionService transactionService, IAuthenticationService authService, IQRCodeService qrCodeService)
        {
            _transactionService = transactionService;
            _authService = authService;
            _qrCodeService = qrCodeService;
            StartListeningForOffers();
        }

        private async void StartListeningForOffers()
        {
            IsLoading = true;

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                IsLoading = false;
                return;
            }

            IncomingOffers.Clear();
            OutgoingOffers.Clear();

            // 🔹 Gelen teklifler
            _incomingOffersSubscription = _firebaseClient
                .Child(Constants.TransactionsCollection)
                .OrderBy("SellerId")
                .EqualTo(currentUser.UserId)
                .AsObservable<Transaction>()
                .Subscribe(e => UpdateCollection(IncomingOffers, e));

            // 🔹 Giden teklifler
            _outgoingOffersSubscription = _firebaseClient
                .Child(Constants.TransactionsCollection)
                .OrderBy("BuyerId")
                .EqualTo(currentUser.UserId)
                .AsObservable<Transaction>()
                .Subscribe(e => UpdateCollection(OutgoingOffers, e));

            IsLoading = false;
        }

        private void UpdateCollection(ObservableCollection<Transaction> collection, FirebaseEvent<Transaction> e)
        {
            if (e.Object == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var transaction = e.Object;
                transaction.TransactionId = e.Key;

                var existing = collection.FirstOrDefault(t => t.TransactionId == transaction.TransactionId);

                switch (e.EventType)
                {
                    case FirebaseEventType.InsertOrUpdate:
                        if (existing != null)
                        {
                            var index = collection.IndexOf(existing);
                            collection[index] = transaction;
                        }
                        else
                        {
                            collection.Insert(0, transaction);
                        }
                        break;

                    case FirebaseEventType.Delete:
                        if (existing != null)
                            collection.Remove(existing);
                        break;
                }
            });
        }

        [RelayCommand]
        private void SelectIncoming()
        {
            IsIncomingSelected = true;
            IsOutgoingSelected = false;
        }

        [RelayCommand]
        private void SelectOutgoing()
        {
            IsIncomingSelected = false;
            IsOutgoingSelected = true;
        }

        [RelayCommand]
        private async Task AcceptOfferAsync(Transaction transaction)
        {
            await RespondToOfferInternalAsync(transaction, true);
        }

        [RelayCommand]
        private async Task RejectOfferAsync(Transaction transaction)
        {
            await RespondToOfferInternalAsync(transaction, false);
        }

        private async Task RespondToOfferInternalAsync(Transaction transaction, bool accept)
        {
            if (transaction == null) return;

            var result = await _transactionService.RespondToOfferAsync(transaction.TransactionId, accept);
            if (result.Success)
            {
                var offerInList = IncomingOffers.FirstOrDefault(o => o.TransactionId == transaction.TransactionId);
                if (offerInList != null)
                {
                    offerInList.Status = result.Data.Status;
                    OnPropertyChanged(nameof(IncomingOffers));
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Hata", result.Message, "Tamam");
            }
        }
        // YENİ: QRScannerPage'den gelen sonucu yakalamak için
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("ScanResult"))
            {
                string scannedData = query["ScanResult"].ToString();
                ProcessScannedCode(scannedData);
            }
        }
        // YENİ: Taranan kodu işleyen metot
        private async void ProcessScannedCode(string qrCodeData)
        {
            IsLoading = true;
            var validateResult = await _qrCodeService.ValidateQRCodeAsync(qrCodeData);
            if (!validateResult.Success)
            {
                await Application.Current.MainPage.DisplayAlert("Geçersiz Kod", validateResult.Message, "Tamam");
                IsLoading = false;
                return;
            }

            var confirm = await Application.Current.MainPage.DisplayAlert("Teslimatı Onayla", "Bu ürünün teslimatını aldığınızı onaylıyor musunuz? Bu işlem geri alınamaz.", "Evet, Onayla", "Hayır");
            if (confirm)
            {
                var completeResult = await _qrCodeService.CompleteDeliveryAsync(validateResult.Data.QRCodeId);
                if (completeResult.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Başarılı", "Teslimat tamamlandı!", "Tamam");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Hata", completeResult.Message, "Tamam");
                }
            }
            IsLoading = false;
        }
        // YENİ: Alıcı için QR kod gösterme komutu
        [RelayCommand]
        private async Task ShowQRCodeAsync(Transaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.DeliveryQRCodeId)) return;

            // QR kod bilgisini veritabanından çekmemiz gerekiyor.
            var qrCode = await _firebaseClient
                .Child(Constants.DeliveryQRCodesCollection)
                .Child(transaction.DeliveryQRCodeId)
                .OnceSingleAsync<DeliveryQRCode>();

            if (qrCode != null)
            {
                await Shell.Current.GoToAsync($"{nameof(QRCodeDisplayPage)}?qrCodeData={qrCode.QRCodeData}");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Hata", "Teslimat kodu bulunamadı.", "Tamam");
            }
        }

        // YENİ: Satıcı için tarayıcıyı açma komutu
        [RelayCommand]
        private async Task ScanToDeliverAsync()
        {
            await Shell.Current.GoToAsync(nameof(QRScannerPage));
        }
        public void Dispose()
        {
            _incomingOffersSubscription?.Dispose();
            _outgoingOffersSubscription?.Dispose();
        }
    }
}
