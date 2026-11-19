using MyBandApp.Services;
using ReactiveUI;
using System.Windows.Input;

namespace MyBandApp.ViewModels;

public partial class DashboardViewModel : ReactiveObject
{
    private readonly IAppNavigationService _navigation;

    public ICommand GoToSongsCommand { get; }
    public ICommand GoToSetlistsCommand { get; }

    public DashboardViewModel(IAppNavigationService navigation)
    {
        _navigation = navigation;
        GoToSongsCommand = ReactiveCommand.CreateFromTask(_navigation.NavigateToSongsAsync);
        GoToSetlistsCommand = ReactiveCommand.CreateFromTask(_navigation.NavigateToSetlistsAsync);
    }

}
