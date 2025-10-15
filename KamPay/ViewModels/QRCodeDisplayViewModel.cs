using CommunityToolkit.Mvvm.ComponentModel;
using KamPay.Models;
using KamPay.Services;

namespace KamPay.ViewModels
{
    [QueryProperty(nameof(QrCodeData), "qrCodeData")]
    public partial class QRCodeDisplayViewModel : ObservableObject
    {
        [ObservableProperty]
        private string qrCodeData;
    }
}