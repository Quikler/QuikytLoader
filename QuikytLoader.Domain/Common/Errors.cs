
namespace QuikytLoader.Domain.Common;

public static class Errors
{
    public static class Youtube
    {
        public static Error InvalidUrl(string url) => new(
            $"The provided URL '{url}' is not a valid Youtube URL");

        public static Error DownloadFailed(string youtubeVideoId, int exitCode) => new(
            $"Failed to download video from '{youtubeVideoId}' (yt-dlp exit code: {exitCode})");

        public static Error FileNotFound(string directory) => new(
            $"Downloaded file not found in directory: {directory}");

        public static Error YtDlpStartFailed() => new(
            "Failed to start yt-dlp process");

        public static Error YtDlpException(string id, string exceptionType) => new(
            $"Unexpected error running yt-dlp for '{id}': {exceptionType}");

        public static Error MetadataFetchFailed(string youtubeVideoId) => new(
            $"Failed to fetch video metadata from '{youtubeVideoId}'");

        public static Error PlaylistMetadataFetchFailed(string youtubePlaylistId) => new(
            $"Failed to fetch playlist metadata from '{youtubePlaylistId}'");

        public static Error YtDlpFailed(int exitCode) => new(
            $"yt-dlp failed with exit code: {exitCode}");
    }

    public static class Telegram
    {
        public static Error BotTokenNotConfigured() => new(
            "Telegram bot token is not configured. Please set it in Settings.");

        public static Error InvalidChatIdFormat(string? chatId) => new(
            string.IsNullOrWhiteSpace(chatId)
                ? "Telegram chat ID is not configured. Please set it in Settings."
                : $"Chat ID is not a valid number: {chatId}");

        public static Error AudioFileNotFound(string path) => new(
            $"Audio file not found at path: {path}");

        public static Error SendFailed(string errorMessage) => new(
            $"Failed to send audio to Telegram: {errorMessage}");

        public static Error InitializationFailed(string errorMessage) => new(
            $"Failed to initialize Telegram bot: {errorMessage}");

        public static Error FileReadError(string audioPath, string thumbnailPath, string errorMessage) => new(
            $"Failed to read file '{audioPath}' for upload (thumbnail: {thumbnailPath}): {errorMessage}");
    }

    public static class Thumbnail
    {
        public static Error ProcessingFailed(string errorMessage) => new(
            $"Failed to process thumbnail: {errorMessage}");

        public static Error FileNotFound(string path) => new(
            $"Thumbnail file not found at path: {path}");
    }
}
