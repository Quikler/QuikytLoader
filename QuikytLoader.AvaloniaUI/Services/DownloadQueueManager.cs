using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.Services;

public class DownloadQueueManager
{
    private readonly IDownloadQueue _queue;
    private readonly IDownloadQueueProcessor _queueProcessor;

    private readonly FetchManualSubtitlesUseCase _fetchManualSubtitlesUseCase;
    private readonly FetchAutoSubtitlesUseCase _fetchAutoSubtitlesUseCase;
    private readonly CancelSubtitlesUseCase _cancelSubtitlesUseCase;

    private readonly SettingsViewModel _settingsViewModel;

    private readonly Dictionary<Guid, QueueItemViewModel> _itemViewModels = [];

    /// <summary>
    /// All queue entries. Can be one queue item and a group item.
    /// </summary>
    public ObservableCollection<QueueEntryViewModel> QueueEntries { get; } = [];

    public DownloadQueueManager(
        IDownloadQueue queue,
        IDownloadQueueProcessor queueProcessor,
        FetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
        FetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
        CancelSubtitlesUseCase cancelSubtitlesUseCase,
        SettingsViewModel settingsViewModel)
    {
        _queue = queue;
        _queue.Changed += OnQueueChanged;

        _queueProcessor = queueProcessor;

        _fetchManualSubtitlesUseCase = fetchManualSubtitlesUseCase;
        _fetchAutoSubtitlesUseCase = fetchAutoSubtitlesUseCase;
        _cancelSubtitlesUseCase = cancelSubtitlesUseCase;

        _settingsViewModel = settingsViewModel;
    }

    private void OnQueueChanged(QueueEvent evt)
    {
        switch (evt)
        {
            case QueueEvent.ItemAdded { Item: var item }:
                AddItem(item);
                break;

            case QueueEvent.GroupAdded { Group: var group }:
                AddGroup(group);
                break;

            case QueueEvent.ItemUpdated { ItemId: var itemId }:
                UpdateItem(itemId);
                break;
        }
    }

    private void AddItem(QueueItem item)
    {
        var vm = CreateItemVm(item);

        RegisterItem(vm);
        AddToUi(vm);

        _queueProcessor.Enqueue(item.Id);
    }

    private void AddGroup(QueueGroup group)
    {
        var itemVms = group.ItemIds
            .Select(_queue.GetItem)
            .Select(CreateGroupItemVm)
            .ToArray();

        foreach (var vm in itemVms)
        {
            RegisterItem(vm);
        }

        var groupVm = new QueueGroupViewModel(group, itemVms, ProceedGroup);
        AddToUi(groupVm);

        // should not queue here as in `AddItem` because it's a group
        // and it requires user to manually click 
        // "Proceed all" in order to queue the queueItem
    }

    private void UpdateItem(Guid itemId)
    {
        if (_itemViewModels.TryGetValue(itemId, out var vm))
            vm.Refresh();
    }

    // Single items — no selection needed
    private QueueItemViewModel CreateItemVm(QueueItem item)
        => new(item,
            ProceedItem,
            CancelItem,
            _fetchManualSubtitlesUseCase,
            _fetchAutoSubtitlesUseCase,
            _cancelSubtitlesUseCase,
            _settingsViewModel);

    // Group items — selectable subtype
    private SelectableQueueItemViewModel CreateGroupItemVm(QueueItem item)
        => new(item,
            ProceedItem,
            CancelItem,
            _fetchManualSubtitlesUseCase,
            _fetchAutoSubtitlesUseCase,
            _cancelSubtitlesUseCase,
            _settingsViewModel);

    private void RegisterItem(QueueItemViewModel vm)
        => _itemViewModels[vm.QueueItemId] = vm;

    private void AddToUi(QueueEntryViewModel vm) => QueueEntries.Add(vm);

    private void ProceedItem(Guid itemId) => _queueProcessor.Proceed(itemId);

    private void ProceedGroup(IEnumerable<Guid> itemIds)
    {
        foreach (var itemId in itemIds)
        {
            ProceedItem(itemId);
        }
    }

    private void CancelItem(Guid itemId) => _queueProcessor.Cancel(itemId);
}
