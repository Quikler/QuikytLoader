# QuikytLoader.Domain

Core domain layer with no external dependencies. Contains entities, value objects, and common types.

## Entities

**DownloadResultEntity** - Result of a Youtube download operation
- Record type with YoutubeVideoId, VideoTitle, TempMediaFilePath, TempThumbnailPath
- Represents temporary files that should be cleaned up after use

**DownloadHistoryEntity** - Download history record for persistence
- Record type with YoutubeVideoId, VideoTitle, DownloadedAt (ISO 8601 UTC timestamp)
- Factory method `Create()` returning `Result<DownloadHistoryEntity>`
- Propagates failure from YoutubeVideoId validation if invalid

## Common Types

**Result / Result&lt;T&gt;** - Railway-oriented error handling
- `IsSuccess` property with `MemberNotNullWhen` attributes for null-state analysis
- `Error` property contains failure reason
- Implicit conversions from value (success) and Error (failure)
- Usage: `return new Error("message")` for failure, `return value` for success

**Error** - Error representation
- Simple record type with Message property

**Errors** - Predefined error constants
- Static class with common error definitions
