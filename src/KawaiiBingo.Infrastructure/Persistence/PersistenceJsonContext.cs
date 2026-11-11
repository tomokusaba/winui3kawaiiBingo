using System.Text.Json.Serialization;

namespace KawaiiBingo.Infrastructure.Persistence;

/// <summary>保存データの JSON シリアル化情報を提供します。</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(BingoGameDocument))]
internal sealed partial class PersistenceJsonContext : JsonSerializerContext;
