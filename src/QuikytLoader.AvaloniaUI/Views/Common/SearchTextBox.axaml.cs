using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace QuikytLoader.AvaloniaUI.Views.Common;

public partial class SearchTextBox : UserControl
{
    public static readonly StyledProperty<int> CurrentOccurrenceIndexProperty =
        AvaloniaProperty.Register<SearchTextBox, int>(
            nameof(CurrentOccurrenceIndex));

    public int CurrentOccurrenceIndex
    {
        get => GetValue(CurrentOccurrenceIndexProperty);
        set => SetValue(CurrentOccurrenceIndexProperty, value);
    }

    public static readonly StyledProperty<int> OccurrencesCountProperty =
        AvaloniaProperty.Register<SearchTextBox, int>(
            nameof(OccurrencesCount));

    public int OccurrencesCount
    {
        get => GetValue(OccurrencesCountProperty);
        set => SetValue(OccurrencesCountProperty, value);
    }

    public static readonly StyledProperty<string?> FindTextProperty =
        AvaloniaProperty.Register<SearchTextBox, string?>(
            nameof(FindText));

    public string? FindText
    {
        get => GetValue(FindTextProperty);
        set => SetValue(FindTextProperty, value);
    }

    public static readonly StyledProperty<ICommand> GoToTheNextOccurrenceCommandProperty =
        AvaloniaProperty.Register<SearchTextBox, ICommand>(
            nameof(GoToTheNextOccurrenceCommand));

    public ICommand GoToTheNextOccurrenceCommand
    {
        get => GetValue(GoToTheNextOccurrenceCommandProperty);
        set => SetValue(GoToTheNextOccurrenceCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> GoToThePreviousOccurrenceCommandProperty =
        AvaloniaProperty.Register<SearchTextBox, ICommand>(
            nameof(GoToThePreviousOccurrenceCommand));

    public ICommand GoToThePreviousOccurrenceCommand
    {
        get => GetValue(GoToThePreviousOccurrenceCommandProperty);
        set => SetValue(GoToThePreviousOccurrenceCommandProperty, value);
    }

    public SearchTextBox() => InitializeComponent();

    public new bool Focus(
        NavigationMethod method = NavigationMethod.Unspecified,
        KeyModifiers keyModifiers = KeyModifiers.None)
            => TextBoxToFocus.Focus(method, keyModifiers);
}
