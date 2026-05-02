using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.AvaloniaUI.ViewModels;

/// <summary>
/// Represents a playlist group in the download queue. Owns its items collection,
/// selection counts, and the batch "Proceed all" command.
/// </summary>
public partial class QueueGroupViewModel : ViewModelBase
{
    public string GroupId { get; }
    public string PlaylistTitle { get; }

    /// <summary>
    /// True when this group represents a YouTube playlist (header + batch controls shown).
    /// False for a single standalone video.
    /// </summary>
    public bool IsPlaylist { get; }

    public ObservableCollection<DownloadQueueItem> Items { get; } = [];

    /// <summary>
    /// Group expanded by default when first added to the queue.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// Number of items the user has selected (excluding disabled items).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderCountText))]
    [NotifyPropertyChangedFor(nameof(ProceedAllLabel))]
    private int _selectedCount;

    /// <summary>
    /// Number of selectable items (not disabled).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeaderCountText))]
    [NotifyPropertyChangedFor(nameof(ProceedAllLabel))]
    private int _selectableCount;

    /// <summary>
    /// Whether at least one selected item is eligible to start downloading.
    /// </summary>
    [ObservableProperty]
    private bool _canProceedAll;

    /// <summary>
    /// "N/M" shown in the group header.
    /// </summary>
    public string HeaderCountText => $"{SelectedCount}/{SelectableCount}";

    /// <summary>
    /// Button label including the count.
    /// </summary>
    public string ProceedAllLabel => $"Proceed all ({SelectedCount}/{SelectableCount})";

    private readonly Action<string> _proceedGroupCallback;

    public QueueGroupViewModel(string groupId, string playlistTitle, bool isPlaylist, Action<string> proceedGroupCallback)
    {
        GroupId = groupId;
        PlaylistTitle = playlistTitle;
        IsPlaylist = isPlaylist;
        _proceedGroupCallback = proceedGroupCallback;
        Items.CollectionChanged += OnItemsChanged;
    }

    [RelayCommand]
    private void ToggleCollapse() => IsExpanded = !IsExpanded;

    [RelayCommand]
    private void ProceedAll() => _proceedGroupCallback(GroupId);

    /// <summary>
    /// Recomputes selection counts and CanProceedAll. Call whenever an item's
    /// IsSelected, Status, or DisabledReason changes.
    /// </summary>
    public void RecomputeCounts()
    {
        var selectable = 0;
        var selected = 0;
        var hasEligible = false;

        foreach (var item in Items)
        {
            if (item.Status == DownloadStatus.Disabled) continue;
            selectable++;
            if (!item.IsSelected) continue;
            selected++;
            if (IsEligibleForBatch(item.Status))
                hasEligible = true;
        }

        SelectableCount = selectable;
        SelectedCount = selected;
        CanProceedAll = hasEligible;
    }

    private static bool IsEligibleForBatch(DownloadStatus status) =>
        status is DownloadStatus.Pending
            or DownloadStatus.Failed
            or DownloadStatus.Cancelled
            or DownloadStatus.Editing;

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (DownloadQueueItem item in e.OldItems)
                item.PropertyChanged -= OnItemPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (DownloadQueueItem item in e.NewItems)
                item.PropertyChanged += OnItemPropertyChanged;
        }

        RecomputeCounts();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DownloadQueueItem.IsSelected)
            or nameof(DownloadQueueItem.Status)
            or nameof(DownloadQueueItem.DisabledReason))
        {
            RecomputeCounts();
        }
    }
}
