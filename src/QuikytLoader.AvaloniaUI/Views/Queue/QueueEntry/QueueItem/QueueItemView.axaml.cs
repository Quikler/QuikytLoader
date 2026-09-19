using System.Diagnostics;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry;

namespace QuikytLoader.AvaloniaUI.Views.Queue.QueueEntry.QueueItem;

public partial class QueueItemView : QueueEntryView
{
    public QueueItemView()
    {
        InitializeComponent();

        AddHandler(PointerPressedEvent, (_, _) =>
        {
            if (ViewModel is not QueueItemViewModel vm)
                throw new UnreachableException();

            vm.SelectInComboBoxCommand.Execute(null);
        }, handledEventsToo: true);
    }
}
