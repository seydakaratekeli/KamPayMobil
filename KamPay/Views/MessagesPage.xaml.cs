using KamPay.ViewModels;
using CommunityToolkit.Mvvm.Messaging; 

namespace KamPay.Views;

public partial class MessagesPage : ContentPage
{
    public MessagesPage(MessagesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Bu sayfa g�r�nd���nde, okunmam�� mesaj rozetini gizlemesi i�in sinyal g�nder.
        WeakReferenceMessenger.Default.Send(new UnreadMessageStatusMessage(false));
    }
}