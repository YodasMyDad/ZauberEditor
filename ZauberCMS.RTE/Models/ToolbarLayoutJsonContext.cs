using System.Text.Json.Serialization;

namespace ZauberCMS.RTE.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ToolbarLayout))]
internal sealed partial class ToolbarLayoutJsonContext : JsonSerializerContext;
