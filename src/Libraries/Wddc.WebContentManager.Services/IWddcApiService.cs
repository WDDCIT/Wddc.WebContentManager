using System.Net.Http;
using System.Threading.Tasks;
using Wddc.Core;

namespace Wddc.WebContentManager.Services
{
    public interface IWddcApiService
    {
        Task<HttpResponseMessage> DeleteAsync(string path, object body = null);
        Task<HttpResponseMessage> GetAsync(string path, object body = null);
        Task<T> PostAsync<T>(string path, object body = null);
        Task<T> GetAsync<T>(string path, object body = null, ListOptions listOptions = null);
        HttpClient GetHttpClient();
        Task<HttpResponseMessage> PostAsync(string path, object body = null);
        Task<HttpResponseMessage> PutAsync(string path, object body = null);
        Task<T> PutAsync<T>(string path, object body = null);
        Task<HttpResponseMessage> SendAsync(string path, HttpMethod httpMethod, object body = null);
        //Task<T> DeleteAsync<T>(string v);
    }
}