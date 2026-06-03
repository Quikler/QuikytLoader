using System;
using System.Collections.Generic;
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
public partial class QueueGroupViewModel : ViewModelBase, IQueueItemsViewModel
{
    public string Id { get; }

    public IReadOnlyList<DownloadQueueItem> Items { get; } = [];

    public string PlaylistTitle { get; }

    /// <summary>
    /// Number of items the user has selected (excluding disabled items).
    /// </summary>
    [ObservableProperty]
    private int _selectedCount;

    /// <summary>
    /// Number of selectable items (not disabled).
    /// </summary>
    [ObservableProperty]
    private int _selectableCount;

    /// <summary>
    /// Whether at least one selected item is eligible to start downloading.
    /// </summary>
    [ObservableProperty]
    private bool _canProceedAll;

    private readonly Action<string> _proceedGroupCallback;

    public QueueGroupViewModel(string id, string playlistTitle, DownloadQueueItem[] items, Action<string> proceedGroupCallback)
    {
        Id = id;
        PlaylistTitle = playlistTitle;

        _proceedGroupCallback = proceedGroupCallback;

        List<DownloadQueueItem> list = [];
        foreach (var item in items)
        {
            item.GroupId = id;
            item.IsInPlaylist = true;
            item.IsSelected = true;
            item.PropertyChanged += OnItemPropertyChanged;
            list.Add(item);
        }

        Items = list;
        RecomputeCounts();
    }

    [RelayCommand]
    private void ProceedAll() => _proceedGroupCallback(Id);

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
        status is DownloadStatus.Queued
            or DownloadStatus.Failed
            or DownloadStatus.Cancelled
            or DownloadStatus.Editing;

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
