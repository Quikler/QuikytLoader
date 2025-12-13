namespace QuikytLoader.Domain.Common;

/// <summary>
/// Centralized catalog of all domain errors.
/// Provides type-safe error definitions with consistent codes.
/// </summary>
public static class Errors
{
    public static class YouTube
    {
        public static Error InvalidUrl(string url) => Error.Validation(
            "YouTube.InvalidUrl",
            "The provided URL is not a valid YouTube URL",
            new() { ["Url"] = url }
        );

        public static Error VideoIdExtractionFailed(string url) => Error.Failure(
            "YouTube.VideoIdExtractionFailed",
            "Failed to extract video ID from URL",
            new() { ["Url"] = url }
        );

        public static Error DownloadFailed(string url, int exitCode) => Error.ExternalService(
            "YouTube.DownloadFailed",
            $"Failed to download video (yt-dlp exit code: {exitCode})",
            new() { ["Url"] = url, ["ExitCode"] = exitCode }
        );

        public static Error TitleFetchFailed(string url) => Error.ExternalService(
            "YouTube.TitleFetchFailed",
            "Failed to fetch video title",
            new() { ["Url"] = url }
        );

        public static Error FileNotFound(string directory) => Error.NotFound(
            "YouTube.FileNotFound",
            "Downloaded file not found in expected directory",
            new() { ["Directory"] = directory }
        );

        public static Error ProcessStartFailed() => Error.Failure(
            "YouTube.ProcessStartFailed",
            "Failed to start yt-dlp process"
        );

        public static Error YtDlpExtractionFailed(string url, int exitCode) => Error.ExternalService(
            "YouTube.YtDlpExtractionFailed",
            $"yt-dlp failed to extract video ID (exit code: {exitCode})",
            new() { ["Url"] = url, ["ExitCode"] = exitCode }
        );

        public static Error InvalidIdLength(string url, string id, int length) => Error.ExternalService(
            "YouTube.InvalidIdLength",
            $"yt-dlp returned invalid ID length: {length} (expected 11)",
            new() { ["Url"] = url, ["Id"] = id, ["Length"] = length }
        );

        public static Error YtDlpException(string url, string exceptionType) => Error.ExternalService(
            "YouTube.YtDlpException",
            $"Unexpected error running yt-dlp: {exceptionType}",
            new() { ["Url"] = url, ["Exception"] = exceptionType }
        );
    }

    public static class Telegram
    {
        public static Error BotTokenNotConfigured() => Error.Configuration(
            "Telegram.BotTokenNotConfigured",
            "Telegram bot token is not configured. Please set it in Settings."
        );

        public static Error ChatIdNotConfigured() => Error.Configuration(
            "Telegram.ChatIdNotConfigured",
            "Telegram chat ID is not configured. Please set it in Settings."
        );

        public static Error InvalidChatIdFormat(string chatId) => Error.Configuration(
            "Telegram.InvalidChatIdFormat",
            $"Chat ID is not a valid number: {chatId}",
            new() { ["ChatId"] = chatId }
        );

        public static Error AudioFileNotFound(string path) => Error.NotFound(
            "Telegram.AudioFileNotFound",
            $"Audio file not found at path: {path}",
            new() { ["Path"] = path }
        );

        public static Error SendFailed(string errorMessage) => Error.ExternalService(
            "Telegram.SendFailed",
            $"Failed to send audio to Telegram: {errorMessage}",
            new() { ["TelegramError"] = errorMessage }
        );

        public static Error InitializationFailed(string maskedToken, string errorMessage) => Error.ExternalService(
            "Telegram.InitializationFailed",
            $"Failed to initialize Telegram bot: {errorMessage}",
            new() { ["BotToken"] = maskedToken }
        );

        public static Error FileReadError(string audioPath, string? thumbnailPath, string errorMessage) => Error.Failure(
            "Telegram.FileReadError",
            $"Failed to read file for upload: {errorMessage}",
            new() { ["AudioPath"] = audioPath, ["ThumbnailPath"] = thumbnailPath ?? "none" }
        );
    }

    public static class History
    {
        public static Error DuplicateVideo(string youtubeId, string previousTitle, string downloadedAt)
            => Error.Conflict(
                "History.DuplicateVideo",
                "This video has already been downloaded",
                new()
                {
                    ["YouTubeId"] = youtubeId,
                    ["PreviousTitle"] = previousTitle,
                    ["DownloadedAt"] = downloadedAt
                }
            );

        public static Error RecordNotFound(string youtubeId) => Error.NotFound(
            "History.RecordNotFound",
            $"Download record not found for YouTube ID: {youtubeId}",
            new() { ["YouTubeId"] = youtubeId }
        );
    }

    public static class Common
    {
        public static Error UnexpectedError(string message, Exception? exception = null)
        {
            var metadata = new Dictionary<string, object> { ["Message"] = message };
            if (exception != null)
            {
                metadata["ExceptionType"] = exception.GetType().Name;
                metadata["StackTrace"] = exception.StackTrace ?? "";
            }

            return Error.Failure("Common.UnexpectedError", message, metadata);
        }
    }
}
