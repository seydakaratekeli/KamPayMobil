
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

        // Taray�c�y� durdur ve titre�imle geri bildirim ver
        barcodeReader.IsDetecting = false;
        Vibration.Vibrate(TimeSpan.FromMilliseconds(500));

        var result = e.Results.First();

        // Sonucu bir �nceki sayfaya "ScanResult" anahtar�yla g�ndererek geri d�n
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "ScanResult", result.Value }
            });
        });
    }
}