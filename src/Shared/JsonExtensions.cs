using System.Text.Json;
using System.Text.Json.Nodes;

namespace Swashbuckle.AspNetCore;

internal static class JsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
#if NET9_0_OR_GREATER
        NewLine = "\n",
#endif
        WriteIndented = true,
    };

    public static string ToJson(this JsonNode value)
        => value.ToJsonString(Options);
}
