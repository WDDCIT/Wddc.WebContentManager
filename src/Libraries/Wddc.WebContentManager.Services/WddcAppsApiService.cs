using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Wddc.WebContentManager.Core.Configuration;

namespace Wddc.WebContentManager.Services
{
    /// <summary>
    /// API-key service using the named HttpClient "wddc_apps_api".
    /// Targets the secondary apps API at https://apps-api.wddc.com/
    /// </summary>
    public sealed class WddcAppsApiService : IWddcAppsApiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        private static readonly JsonSerializerSettings NewtonsoftSettings = new()
        {
            ContractResolver = new DerivedFirstContractResolver()
        };

        public HttpClient Client { get; }

        public WddcAppsApiService(IHttpClientFactory factory, IRootConfiguration rootConfig)
        {
            Client = factory.CreateClient("wddc_apps_api");
            if (Client.BaseAddress is null && rootConfig?.SecondaryApiConfiguration?.BaseUrl is { Length: > 0 } baseUrl)
                Client.BaseAddress = new Uri(baseUrl);
        }

        #region Public API

        public Task<string> GetStringAsync(string path, CancellationToken ct = default)
            => SendCoreAsync<string>(HttpMethod.Get, path, null, ct, rawString: true);

        public Task<T> GetAsync<T>(string path, object query = null, CancellationToken ct = default)
            => SendCoreAsync<T>(HttpMethod.Get, AppendQuery(path, query), null, ct);

        public Task<T> PostAsync<T>(string path, object body = null, CancellationToken ct = default)
            => SendCoreAsync<T>(HttpMethod.Post, path, body, ct);

        public Task<T> PutAsync<T>(string path, object body = null, CancellationToken ct = default)
            => SendCoreAsync<T>(HttpMethod.Put, path, body, ct);

        public async Task<T> DeleteAsync<T>(string path, object body = null, CancellationToken ct = default)
            => await SendCoreAsync<T>(HttpMethod.Delete, path, body, ct);

        public async Task DeleteAsync(string path, object body = null, CancellationToken ct = default)
            => _ = await SendCoreAsync<object>(HttpMethod.Delete, path, body, ct, allowNoContent: true);

        public Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object body = null, CancellationToken ct = default)
            => SendRawAsync(method, path, body, ct);

        #endregion

        #region Core

        private async Task<T> SendCoreAsync<T>(HttpMethod method, string path, object body, CancellationToken ct,
                                               bool rawString = false, bool allowNoContent = false)
        {
            using var response = await SendRawAsync(method, path, body, ct);
            if (response.StatusCode == HttpStatusCode.NoContent && allowNoContent)
                return default;

            if (!response.IsSuccessStatusCode)
                await ThrowForError(response);

            if (response.Content.Headers.ContentLength == 0)
                return default;

            var raw = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrEmpty(raw))
                return default;

            if (rawString && typeof(T) == typeof(string))
                return (T)(object)raw;

            try
            {
                return JsonConvert.DeserializeObject<T>(raw, NewtonsoftSettings);
            }
            catch (JsonSerializationException) when (typeof(T) == typeof(string))
            {
                return (T)(object)raw;
            }
            catch (JsonReaderException) when (typeof(T) == typeof(string))
            {
                return (T)(object)raw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize '{typeof(T).Name}' from '{path}'. Raw: {raw}", ex);
            }
        }

        private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string path, object body, CancellationToken ct)
        {
            var request = new HttpRequestMessage(method, ResolveUri(path));
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            return response;
        }

        #endregion

        #region Helpers

        private Uri ResolveUri(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var abs))
                return abs;

            if (Client.BaseAddress == null)
                throw new InvalidOperationException("HttpClient.BaseAddress is not set and relative path provided.");
            path = path.TrimStart('/');
            return new Uri(Client.BaseAddress, path);
        }

        private static string AppendQuery(string path, object query)
        {
            if (query == null) return path;

            var sb = new StringBuilder();
            sb.Append(path);
            sb.Append(path.Contains("?") ? "&" : "?");

            bool first = true;
            foreach (var kv in ToQueryPairs(query))
            {
                if (!first) sb.Append('&');
                first = false;
                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(kv.Value ?? string.Empty));
            }
            return sb.ToString();
        }

        private static IEnumerable<KeyValuePair<string, string>> ToQueryPairs(object query)
        {
            switch (query)
            {
                case IEnumerable<KeyValuePair<string, string>> kvps:
                    foreach (var kv in kvps) yield return kv;
                    yield break;
                case null:
                    yield break;
                default:
                    var props = query.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var p in props)
                    {
                        var val = p.GetValue(query);
                        if (val == null) continue;
                        if (val is DateTime dt) yield return new(p.Name, dt.ToString("O"));
                        else yield return new(p.Name, val.ToString());
                    }
                    break;
            }
        }

        private static async Task ThrowForError(HttpResponseMessage response)
        {
            var status = (int)response.StatusCode;
            var payload = await response.Content.ReadAsStringAsync();

            string msg;
            try
            {
                var problem = System.Text.Json.JsonSerializer.Deserialize<MiniProblem>(payload, JsonOptions);
                if (problem?.Title != null)
                    msg = $"{problem.Title} ({problem.Status ?? status}) :: {problem.Detail ?? payload}";
                else
                    msg = payload;
            }
            catch
            {
                msg = payload;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedAccessException(msg);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ArgumentException($"BadRequest: {msg}");

            response.Content?.Dispose();
            throw new HttpRequestException($"HTTP {status}: {msg}", null, response.StatusCode);
        }

        private sealed class MiniProblem
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public int? Status { get; set; }
            public string Detail { get; set; }
            public string Instance { get; set; }
        }

        private sealed class DerivedFirstContractResolver : DefaultContractResolver
        {
            private static readonly Type StjIgnoreAttr = typeof(System.Text.Json.Serialization.JsonIgnoreAttribute);
            private static readonly Type StjPropertyNameAttr = typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute);

            protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);

                var filtered = new List<Newtonsoft.Json.Serialization.JsonProperty>(props.Count);
                foreach (var prop in props)
                {
                    var attrs = prop.AttributeProvider?.GetAttributes(StjIgnoreAttr, true);
                    if (attrs != null && attrs.Count > 0)
                        continue;

                    var nameAttrs = prop.AttributeProvider?.GetAttributes(StjPropertyNameAttr, true);
                    if (nameAttrs != null && nameAttrs.Count > 0)
                    {
                        var nameAttr = (System.Text.Json.Serialization.JsonPropertyNameAttribute)nameAttrs[0];
                        prop.PropertyName = nameAttr.Name;
                    }

                    filtered.Add(prop);
                }

                return filtered
                    .GroupBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.Count() == 1
                        ? g.First()
                        : g.OrderByDescending(p => GetInheritanceDepth(p.DeclaringType, type)).First())
                    .ToList();
            }

            private static int GetInheritanceDepth(Type declaring, Type concrete)
            {
                int depth = 0;
                var t = concrete;
                while (t != null)
                {
                    if (t == declaring) return depth;
                    t = t.BaseType;
                    depth++;
                }
                return int.MaxValue;
            }
        }

        #endregion
    }
}
