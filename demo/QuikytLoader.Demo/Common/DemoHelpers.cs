namespace QuikytLoader.Demo.Common;

public static class DemoHelpers
{
    public static async Task SimulateProgress(
        int inclusiveIterationsCount,
        int millisecondsDelay,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        for (var i = 0; i <= inclusiveIterationsCount; i++)
        {
            progress?.Report(i);

            await Task.Delay(
                millisecondsDelay,
                ct);
        }
    }
}
