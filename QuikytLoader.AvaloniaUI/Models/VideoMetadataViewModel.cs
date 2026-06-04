using CommunityToolkit.Mvvm.ComponentModel;

namespace QuikytLoader.AvaloniaUI.Models;

public partial class VideoMetadataViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _videoId;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _channel;

    [ObservableProperty]
    private string? _duration;

    [ObservableProperty]
    private string? _thumbnailUrl;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private string? _unavailableReason;

    [ObservableProperty]
    private bool _isLoaded;

    public override string ToString() =>
        $"{Title ?? "<No Title>"} ({Channel ?? "Unknown Channel"}, {Duration ?? "Unknown Duration"})";
}

