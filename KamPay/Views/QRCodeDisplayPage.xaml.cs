using KamPay.ViewModels;

namespace KamPay.Views;

public partial class QRCodeDisplayPage : ContentPage
{
    public QRCodeDisplayPage(QRCodeDisplayViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}