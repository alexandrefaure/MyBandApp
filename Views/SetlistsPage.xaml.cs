using MyBandApp.ViewModels;

namespace MyBandApp.Views;

public partial class SetlistsPage : ContentPage
{
    private readonly SetlistsViewModel _viewModel;

    public SetlistsPage(SetlistsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSetlistsCommand.ExecuteAsync(null);
    }
}