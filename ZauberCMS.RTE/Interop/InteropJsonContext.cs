using System.Text.Json;
using System.Text.Json.Serialization;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SelectionInfo))]
[JsonSerializable(typeof(LinkInfo))]
[JsonSerializable(typeof(ImageInfo))]
internal sealed partial class InteropJsonContext : JsonSerializerContext;
