using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SoundDeffect.ViewModels;

public class ChannelToggleViewModel : INotifyPropertyChanged
{
    private bool _isVisible = true;

    public int ChannelId { get; }
    public string Name => $"Канал {ChannelId}";
    public Brush ChannelBrush { get; }

    public event Action? IsVisibleChanged;

    public ChannelToggleViewModel(int channelId)
    {
        ChannelId = channelId;
        ChannelBrush = GetFixedBrushForChannel(channelId);
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

    private Brush GetFixedBrushForChannel(int id)
    {
        Color[] baseColors = new Color[]
        {
            Color.FromRgb(31, 119, 180),  // Синий
            Color.FromRgb(255, 127, 14),  // Оранжевый
            Color.FromRgb(44, 160, 44),   // Зеленый
            Color.FromRgb(214, 39, 40),   // Красный
            Color.FromRgb(148, 103, 189), // Фиолетовый
            Color.FromRgb(140, 86, 75),   // Коричневый
            Color.FromRgb(227, 119, 194), // Розовый
            Color.FromRgb(127, 127, 127), // Серый
            Color.FromRgb(188, 189, 34),  // Болотный
            Color.FromRgb(23, 190, 207)   // Голубой
        };

        // Зацикливаем выбор, если каналов больше 10
        Color targetColor = baseColors[Math.Abs(id) % baseColors.Length];

        var brush = new SolidColorBrush(targetColor);
        brush.Freeze(); // Оптимизация WPF для неизменяемых ресурсов
        return brush;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
