using MyBandApp.ViewModels;

namespace MyBandApp.Views;

public partial class SongDetailPopup : ContentPage
{
    public SongDetailViewModel ViewModel { get; }

    public SongDetailPopup(SongDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }
}
