using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace Swashbuckle.AspNetCore.ApiTesting;

public static class HttpHeadersExtensions
{
    internal static NameValueCollection ToNameValueCollection(this HttpHeaders httpHeaders)
    {
        var headerNameValues = new NameValueCollection();

        foreach (var entry in httpHeaders)
        {
#if NET8_0_OR_GREATER
            headerNameValues.Add(entry.Key, string.Join(',', entry.Value));
#else
            headerNameValues.Add(entry.Key, string.Join(",", entry.Value));
#endif
        }

        return headerNameValues;
    }
}
