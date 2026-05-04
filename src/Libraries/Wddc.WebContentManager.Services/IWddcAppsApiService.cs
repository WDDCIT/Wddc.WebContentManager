using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services
{
    /// <summary>
    /// Generic API-key based HTTP service for the secondary apps API.
    /// Provides typed + raw methods with CancellationToken support.
    /// </summary>
    public interface IWddcAppsApiService
    {
        HttpClient Client { get; }

        // Raw
        Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object body = null, CancellationToken ct = default);

        // Non-generic (raw content as string)
        Task<string> GetStringAsync(string path, CancellationToken ct = default);

        // Generic (JSON)
        Task<T> GetAsync<T>(string path, object query = null, CancellationToken ct = default);
        Task<T> PostAsync<T>(string path, object body = null, CancellationToken ct = default);
        Task<T> PutAsync<T>(string path, object body = null, CancellationToken ct = default);
        Task<T> DeleteAsync<T>(string path, object body = null, CancellationToken ct = default);
        Task DeleteAsync(string path, object body = null, CancellationToken ct = default);
    }
}
