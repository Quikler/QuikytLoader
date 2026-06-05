using System.Text.Json.Serialization;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Infrastructure.YouTube;

namespace QuikytLoader.Infrastructure.Persistence.Json;

/// <summary>
/// Source-generated JSON serialization context for AOT compatibility
/// Eliminates reflection-based serialization when app is published with trimming/NativeAOT
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    RespectNullableAnnotations = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(UserSettingsDto))]
[JsonSerializable(typeof(YtDlpPlaylistJson))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
