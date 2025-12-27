namespace QuikytLoader.Domain.Common;

public static class Errors
{
    public static class YouTube
    {
        public static Error InvalidUrl(string url) => new(
            $"The provided URL '{url}' is not a valid YouTube URL");

        public static Error DownloadFailed(string url, int exitCode) => new(
            $"Failed to download video from '{url}' (yt-dlp exit code: {exitCode})");

        public static Error TitleFetchFailed(string url) => new(
            $"Failed to fetch video title from '{url}'");

        public static Error FileNotFound(string directory) => new(
            $"Downloaded file not found in directory: {directory}");

        public static Error YtDlpStartFailed() => new(
            "Failed to start yt-dlp process");

        public static Error YtDlpExtractionFailed(string url, int exitCode) => new(
            $"yt-dlp failed to extract video ID from '{url}' (exit code: {exitCode})");

        public static Error InvalidIdLength(string url, string id, int length) => new(
            $"yt-dlp returned invalid ID length: {length} (expected 11) for URL '{url}', ID: {id}");

        public static Error YtDlpException(string url, string exceptionType) => new(
            $"Unexpected error running yt-dlp for '{url}': {exceptionType}");
    }

    public static class Telegram
    {
        public static Error BotTokenNotConfigured() => new(
            "Telegram bot token is not configured. Please set it in Settings.");

        public static Error ChatIdNotConfigured() => new(
            "Telegram chat ID is not configured. Please set it in Settings.");

        public static Error InvalidChatIdFormat(string chatId) => new(
            $"Chat ID is not a valid number: {chatId}");

        public static Error AudioFileNotFound(string path) => new(
            $"Audio file not found at path: {path}");

        public static Error SendFailed(string errorMessage) => new(
            $"Failed to send audio to Telegram: {errorMessage}");

        public static Error InitializationFailed(string errorMessage) => new(
            $"Failed to initialize Telegram bot: {errorMessage}");

        public static Error FileReadError(string audioPath, string? thumbnailPath, string errorMessage) => new(
            $"Failed to read file '{audioPath}' for upload (thumbnail: {thumbnailPath ?? "none"}): {errorMessage}");
    }

    public static class Thumbnail
    {
        public static Error ProcessingFailed(string errorMessage) => new(
            $"Failed to process thumbnail: {errorMessage}");

        public static Error FileNotFound(string path) => new(
            $"Thumbnail file not found at path: {path}");
    }
}
