using CommunityToolkit.Maui.Views;
using KamPay.ViewModels;

namespace KamPay.Views;

public partial class TradeOfferView : Popup
{
    public TradeOfferView(TradeOfferViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Pop-up'�n sonucunu ViewModel'e bildirmek i�in
     //   vm.ClosePopupAction = async () => await CloseAsync();
    }
}