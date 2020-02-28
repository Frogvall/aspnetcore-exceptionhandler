using System.Text.Json;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;

namespace System.Net.Http
{ 
    public static class HttpResponseMessageExtensions
    {
        public static async Task<ApiError> ParseApiErrorAsync(this HttpResponseMessage httpResponseMessage)
        {
            //Not an error if successful
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var options = new JsonSerializerOptions{
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync()) {
                    return await JsonSerializer.DeserializeAsync<ApiError>(stream, options);
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool TryParseApiError(this HttpResponseMessage httpResponseMessage, out ApiError error)
        {
            error = null;
            //Not an error if successful
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            var options = new JsonSerializerOptions{
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                var responseResult = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                error = JsonSerializer.Deserialize<ApiError>(responseResult, options);
                return error != null;
            }
            catch
            {
                return false;
            }
        }
    }
}