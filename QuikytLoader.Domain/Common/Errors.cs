namespace QuikytLoader.Domain.Common;

public static class Errors
{
    public static class YouTube
    {
        public static Error InvalidUrl(string url) => Error.Validation(
            "YouTube.InvalidUrl",
            $"The provided URL '{url}' is not a valid YouTube URL"
        );

        public static Error DownloadFailed(string url, int exitCode) => Error.ExternalService(
            "YouTube.DownloadFailed",
            $"Failed to download video from '{url}' (yt-dlp exit code: {exitCode})"
        );

        public static Error TitleFetchFailed(string url) => Error.ExternalService(
            "YouTube.TitleFetchFailed",
            $"Failed to fetch video title from '{url}'"
        );

        public static Error FileNotFound(string directory) => Error.NotFound(
            "YouTube.FileNotFound",
            $"Downloaded file not found in directory: {directory}"
        );

        public static Error YtDlpStartFailed() => Error.Failure(
            "YouTube.ProcessStartFailed",
            "Failed to start yt-dlp process"
        );

        public static Error YtDlpExtractionFailed(string url, int exitCode) => Error.ExternalService(
            "YouTube.YtDlpExtractionFailed",
            $"yt-dlp failed to extract video ID from '{url}' (exit code: {exitCode})"
        );

        public static Error InvalidIdLength(string url, string id, int length) => Error.ExternalService(
            "YouTube.InvalidIdLength",
            $"yt-dlp returned invalid ID length: {length} (expected 11) for URL '{url}', ID: {id}"
        );

        public static Error YtDlpException(string url, string exceptionType) => Error.ExternalService(
            "YouTube.YtDlpException",
            $"Unexpected error running yt-dlp for '{url}': {exceptionType}"
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
            $"Chat ID is not a valid number: {chatId}"
        );

        public static Error AudioFileNotFound(string path) => Error.NotFound(
            "Telegram.AudioFileNotFound",
            $"Audio file not found at path: {path}"
        );

        public static Error SendFailed(string errorMessage) => Error.ExternalService(
            "Telegram.SendFailed",
            $"Failed to send audio to Telegram: {errorMessage}"
        );

        public static Error InitializationFailed(string errorMessage) => Error.ExternalService(
            "Telegram.InitializationFailed",
            $"Failed to initialize Telegram bot: {errorMessage}"
        );

        public static Error FileReadError(string audioPath, string? thumbnailPath, string errorMessage) => Error.Failure(
            "Telegram.FileReadError",
            $"Failed to read file '{audioPath}' for upload (thumbnail: {thumbnailPath ?? "none"}): {errorMessage}"
        );
    }

    public static class Thumbnail
    {
        public static Error ProcessingFailed(string errorMessage) => Error.Failure(
            "Thumbnail.ProcessingFailed",
            $"Failed to process thumbnail: {errorMessage}"
        );

        public static Error FileNotFound(string path) => Error.NotFound(
            "Thumbnail.FileNotFound",
            $"Thumbnail file not found at path: {path}"
        );
    }
}
