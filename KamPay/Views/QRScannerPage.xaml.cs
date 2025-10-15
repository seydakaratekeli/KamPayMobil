
namespace KamPay.Views;

public partial class QRScannerPage : ContentPage
{
    public QRScannerPage()
    {
        InitializeComponent();
    }

    private async void BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
    {
        if (e.Results == null || !e.Results.Any())
            return;

        // Tarayýcýyý durdur ve titreþimle geri bildirim ver
        barcodeReader.IsDetecting = false;
        Vibration.Vibrate(TimeSpan.FromMilliseconds(500));

        var result = e.Results.First();

        // Sonucu bir önceki sayfaya "ScanResult" anahtarýyla göndererek geri dön
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "ScanResult", result.Value }
            });
        });
    }
}