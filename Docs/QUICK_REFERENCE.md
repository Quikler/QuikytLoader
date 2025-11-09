# 🔍 Quick Reference Guide - Key Patterns & Code

## 1. MVVM Command Pattern

### ViewModel Command Declaration
```csharp
[RelayCommand(CanExecute = nameof(CanExecuteDownload))]
private async Task DownloadAndSendAsync()
{
    // Command logic here
}

private bool CanExecuteDownload()
{
    return !IsProcessing && HasValidUrl();
}
```

### XAML Command Binding
```xml
<Button Command="{Binding DownloadAndSendCommand}">
    Download & Send
</Button>
```

**Benefits:**
- No code-behind
- Automatic UI state management
- Testable logic
- Clear separation

---

## 2. Observable Properties

### ViewModel Property
```csharp
[ObservableProperty]
private string _youtubeUrl = string.Empty;
// Generates: public string YoutubeUrl { get; set; }
```

### With Change Notification
```csharp
partial void OnYoutubeUrlChanged(string value)
{
    DownloadAndSendCommand.NotifyCanExecuteChanged();
}
```

**Benefits:**
- Less boilerplate
- Type-safe
- Automatic INotifyPropertyChanged
- Clean syntax

---

## 3. Dependency Injection

### Service Registration (App.axaml.cs)
```csharp
services.AddTransient<MainWindowViewModel>();
services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
services.AddSingleton<ITelegramBotService, TelegramBotService>();
```

### Constructor Injection
```csharp
public MainWindowViewModel(
    IYouTubeDownloadService youtubeService,
    ITelegramBotService telegramService)
{
    _youtubeService = youtubeService;
    _telegramService = telegramService;
}
```

**Benefits:**
- Loose coupling
- Easy testing (mock services)
- Centralized configuration
- Lifetime management

---

## 4. Law of Demeter - Good Examples

### ❌ BAD (Violates Law of Demeter)
```csharp
var result = viewModel.Service.Client.Connection.Send();
```

### ✅ GOOD (Respects Law of Demeter)
```csharp
// In ViewModel
await _youtubeService.DownloadAsync(url);

// In Service
public async Task DownloadAsync(string url)
{
    // Internal complexity hidden
    var client = _clientFactory.Create();
    await client.DownloadAsync(url);
}
```

**Principle:** Only talk to immediate friends, not friends of friends.

---

## 5. DRY - Extracted Methods

### ❌ REPEATED CODE
```csharp
IsProcessing = true;
DownloadAndSendCommand.NotifyCanExecuteChanged();

// ... later ...

IsProcessing = false;
DownloadAndSendCommand.NotifyCanExecuteChanged();
```

### ✅ DRY SOLUTION
```csharp
private void SetProcessingState(bool isProcessing)
{
    IsProcessing = isProcessing;
    DownloadAndSendCommand.NotifyCanExecuteChanged();
}

// Usage:
SetProcessingState(true);
// ... work ...
SetProcessingState(false);
```

**Benefits:**
- Change once, effect everywhere
- Consistent behavior
- Easier maintenance

---

## 6. Single Responsibility Methods

Each method does ONE thing:

```csharp
// ✅ Validates URL only
private bool ValidateUrl()
{
    if (string.IsNullOrWhiteSpace(YoutubeUrl))
        return false;
    return IsYouTubeUrl(YoutubeUrl);
}

// ✅ Updates status only
private void UpdateStatus(string message)
{
    StatusMessage = message;
}

// ✅ Downloads only
private async Task DownloadFromYouTubeAsync()
{
    UpdateStatus("Downloading...");
    await _youtubeService.DownloadAsync(YoutubeUrl);
}

// ✅ Sends only
private async Task SendToTelegramAsync()
{
    UpdateStatus("Sending...");
    await _telegramService.SendAsync(filePath);
}
```

---

## 7. Async/Await Pattern

```csharp
[RelayCommand]
private async Task DownloadAndSendAsync()
{
    try
    {
        await DownloadFromYouTubeAsync();
        await SendToTelegramAsync();
        HandleSuccess();
    }
    catch (Exception ex)
    {
        HandleError(ex.Message);
    }
}
```

**Best Practices:**
- Always use `async Task` for commands
- Handle exceptions at command level
- Use `ConfigureAwait(false)` for library code
- Keep UI responsive

---

## 8. XAML Behaviors (No Code-Behind)

### Command on Enter Key
```xml
<TextBox Text="{Binding YoutubeUrl}">
    <i:Interaction.Behaviors>
        <ia:EventTriggerBehavior EventName="KeyDown">
            <ia:InvokeCommandAction Command="{Binding DownloadAndSendCommand}"/>
        </ia:EventTriggerBehavior>
    </i:Interaction.Behaviors>
</TextBox>
```

**Benefits:**
- No code-behind event handlers
- Declarative behavior
- Reusable
- Testable

---

## 9. FluentAvalonia UI Components

### Modern Card Design
```xml
<Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{DynamicResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="8"
        Padding="24">
    <!-- Content -->
</Border>
```

### Symbol Icons
```xml
<ui:SymbolIcon Symbol="Download" FontSize="20"/>
```

The Symbol enum can be found here ```FluentAvalonia.UI.Controls.Symbol```

**Benefits:**
- Consistent theming
- Dark/light mode support
- Modern look
- Accessible

---

## 10. Error Handling Pattern

```csharp
private async Task ProcessDownloadAndSendAsync()
{
    SetProcessingState(true);
    ShowProgress();

    try
    {
        await DownloadFromYouTubeAsync();
        await SendToTelegramAsync();
        HandleSuccess();
    }
    catch (Exception ex)
    {
        HandleError(ex.Message);
    }
    finally
    {
        SetProcessingState(false);
        HideProgress();
    }
}

private void HandleSuccess()
{
    UpdateStatus("✓ Successfully sent!");
    ClearUrl();
}

private void HandleError(string message)
{
    UpdateStatus($"Error: {message}");
    ResetProgress();
}
```

**Pattern:**
1. Set state (processing)
2. Try operation
3. Handle success
4. Catch and handle errors
5. Always cleanup in finally

---

## 11. Validation Pattern

```csharp
// Multi-level validation
private bool ValidateUrl()
{
    if (string.IsNullOrWhiteSpace(YoutubeUrl))
        return false;
    
    return IsYouTubeUrl(YoutubeUrl);
}

private bool IsYouTubeUrl(string url)
{
    return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
           url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
}

private bool HasValidUrl()
{
    return !string.IsNullOrWhiteSpace(YoutubeUrl) && 
           IsYouTubeUrl(YoutubeUrl);
}
```

**Layers:**
1. Null/empty check
2. Format validation
3. Combined validation for commands

---

## 12. Progress Indication

```csharp
private void ShowProgress()
{
    IsProgressVisible = true;
    ResetProgress();
}

private void UpdateProgress(double value)
{
    ProgressValue = value;
}

private void HideProgress()
{
    IsProgressVisible = false;
}
```

**Usage:**
```csharp
ShowProgress();
UpdateProgress(30);  // 30%
// ... work ...
UpdateProgress(60);  // 60%
// ... work ...
UpdateProgress(100); // Done
HideProgress();
```

---

## 🎓 Key Takeaways

1. **Commands** replace event handlers
2. **Observable properties** replace INotifyPropertyChanged boilerplate
3. **DI** provides loose coupling
4. **Small methods** improve readability
5. **Law of Demeter** reduces dependencies
6. **DRY** eliminates duplication
7. **Async/await** keeps UI responsive
8. **Try-finally** ensures cleanup
9. **FluentAvalonia** provides modern UI
10. **MVVM** separates concerns

---

## 📖 Further Reading

- **CommunityToolkit.Mvvm**: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/
- **Avalonia Docs**: https://docs.avaloniaui.net/
- **FluentAvalonia**: https://github.com/amwx/FluentAvalonia
- **MVVM Pattern**: https://en.wikipedia.org/wiki/Model–view–viewmodel
- **Law of Demeter**: https://en.wikipedia.org/wiki/Law_of_Demeter

---

**Use this guide as a reference when implementing new features!** 🚀
