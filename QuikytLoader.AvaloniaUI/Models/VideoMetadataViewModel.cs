using CommunityToolkit.Mvvm.ComponentModel;

namespace QuikytLoader.AvaloniaUI.Models;

public partial class VideoMetadataViewModel : ObservableObject
{
    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _videoId = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _channel = string.Empty;

    [ObservableProperty]
    private string _duration = string.Empty;

    [ObservableProperty]
    private string _thumbnailUrl = string.Empty;

    [ObservableProperty]
    private bool _isAvailable;

    [ObservableProperty]
    private string _unavailableReason = string.Empty;

    [ObservableProperty]
    private bool _isLoaded;

    public override string ToString() =>
        $"{Title} ({Channel}, {Duration}) - {Url}";
}
