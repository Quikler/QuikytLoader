using System.Text.Json.Serialization;
using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Infrastructure.Persistence.Json;

/// <summary>
/// Source-generated JSON serialization context for AOT compatibility
/// Eliminates reflection-based serialization when app is published with trimming/NativeAOT
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AppSettingsDto))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
