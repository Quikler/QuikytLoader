using Avalonia.Controls;
using Avalonia.Interactivity;
using QuikytLoader.AvaloniaUI.ViewModels;

namespace QuikytLoader.AvaloniaUI.Views;

public partial class QueueItemView : UserControl
{
    public QueueItemView() => InitializeComponent();

    private void OnSubtitlesClick(object? sender, RoutedEventArgs e)
    {
        SubtitlesGrid.IsVisible = !SubtitlesGrid.IsVisible;
        if (SubtitlesGrid.IsVisible)
        {
            SubtitlesIcon.Symbol = FluentAvalonia.UI.Controls.FASymbol.ClosedCaptionFilled;
            SubtitlesChevron.Symbol = FluentAvalonia.UI.Controls.FASymbol.ChevronUp;

            var vm = (DataContext as QueueItemViewModel)!;
            if (vm.FetchSubtitlesCommand.CanExecute(null))
                vm.FetchSubtitlesCommand.Execute(null);
        }
        else
        {
            SubtitlesIcon.Symbol = FluentAvalonia.UI.Controls.FASymbol.ClosedCaption;
            SubtitlesChevron.Symbol = FluentAvalonia.UI.Controls.FASymbol.ChevronDown;
        }
    }
}
