using MyBandApp.Models.Presentation;
using MyBandApp.ViewModels;

namespace MyBandApp.Views;

public partial class SetlistEditPage : ContentPage
{
    public SetlistEditViewModel ViewModel { get; }

    public SetlistEditPage(SetlistEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = ViewModel = viewModel;
    }

    private void OnDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is SelectableSongModel model)
            ViewModel.StartDragging(model);
    }

    private void OnDrop(object? sender, DropEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is SelectableSongModel model)
            ViewModel.ReorderSelectedSongs(model);
    }
}
