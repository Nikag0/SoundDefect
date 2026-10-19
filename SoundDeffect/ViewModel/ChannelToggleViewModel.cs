using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundDeffect.ViewModels;

public class ChannelToggleViewModel : INotifyPropertyChanged
{
    private bool _isVisible = true;

    public int ChannelId { get; }
    public string Name => $"Канал {ChannelId}";

    public event Action? IsVisibleChanged;

    public ChannelToggleViewModel(int channelId)
    {
        ChannelId = channelId;
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChanged();
                IsVisibleChanged?.Invoke();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
