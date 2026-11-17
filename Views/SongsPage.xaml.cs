using MyBandApp.ViewModels;

namespace MyBandApp.Views;

public partial class SongsPage : ContentPage
{
    private readonly SongsViewModel _viewModel;

    public SongsPage(SongsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSongsCommand.ExecuteAsync(null);
    }
}