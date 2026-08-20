using System.Diagnostics;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Infrastructure.Youtube.YtDlp;

internal sealed class YtDlpProcessClient : IYtDlpProcessClient
{
    public async Task<Result<string>> RunCaptureAsync(
        IReadOnlyList<string> args,
        CancellationToken ct = default)
    {
        try
        {
            using var process = Process.Start(CreatePsi(args));

            if (process is null)
                return Errors.Youtube.YtDlpStartFailed();

            var outputTask = process.StandardOutput.ReadToEndAsync(ct);

            await WaitForProcessExit(process, ct);

            if (process.ExitCode != 0)
                return Errors.Youtube.YtDlpFailed(process.ExitCode);

            return Result<string>.Success(await outputTask);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Youtube.YtDlpException(ex.GetType().Name);
        }
    }

    public async Task<Result> RunStreamingAsync(
        IReadOnlyList<string> args,
        Action<string>? onOutputLine = null,
        CancellationToken ct = default)
    {
        try
        {
            using var process = Process.Start(CreatePsi(args));

            if (process is null)
                return Errors.Youtube.YtDlpStartFailed();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                    onOutputLine?.Invoke(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await WaitForProcessExit(process, ct);

            return process.ExitCode == 0
                ? Result.Success()
                : Errors.Youtube.YtDlpFailed(process.ExitCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Youtube.YtDlpException(ex.GetType().Name);
        }
    }

    private static ProcessStartInfo CreatePsi(IReadOnlyList<string> arguments)
    {
        var fileName = "yt-dlp";
        var s = Path.DirectorySeparatorChar;
        // If yt-dlp is installed with pipx, pipx creates a symlink $HOME/.local/bin/yt-dlp
        var ytDlpLocalBinFile = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, $".local{s}bin{s}{fileName}");

        if (File.Exists(ytDlpLocalBinFile))
            fileName = ytDlpLocalBinFile;

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            psi.ArgumentList.Add(argument);

        return psi;
    }

    private static async Task WaitForProcessExit(
        Process process,
        CancellationToken ct)
    {
        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { } // If process died in the nanosecond gap between `HasExited` and `Kill`
            }

            await process.WaitForExitAsync(CancellationToken.None);
            throw;
        }
    }
}
