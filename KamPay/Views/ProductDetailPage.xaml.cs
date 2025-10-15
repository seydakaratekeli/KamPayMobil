using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using KamPay.Services;
using KamPay.ViewModels;

namespace KamPay.Views;

public partial class ProductDetailPage : ContentPage
{

    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;


    }
}