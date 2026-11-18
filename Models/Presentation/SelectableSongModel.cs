using MyBandApp.Models.Entities;

namespace MyBandApp.Models.Presentation;

public class SelectableSongModel : BindableObject
{
    public Song Song { get; set; } = new();

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    private int _displayIndex;
    public int DisplayIndex
    {
        get => _displayIndex;
        set
        {
            if (_displayIndex != value)
            {
                _displayIndex = value;
                OnPropertyChanged();
            }
        }
    }
}