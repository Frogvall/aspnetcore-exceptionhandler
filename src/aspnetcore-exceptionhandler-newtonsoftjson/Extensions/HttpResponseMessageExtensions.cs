using Newtonsoft.Json;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;

namespace System.Net.Http
{ 
    public static class HttpResponseMessageExtensions
    {
        public static async Task<ApiError> ParseApiErrorUsingNewtonsoftJsonAsync(this HttpResponseMessage httpResponseMessage)
        {
            //Not an error if successful
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            try
            {
                var responseResult = await httpResponseMessage.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ApiError>(responseResult);
                return error;
            }
            catch
            {
                return null;
            }
        }

        public static bool TryParseApiErrorUsingNewtonsoftJson(this HttpResponseMessage httpResponseMessage, out ApiError error)
        {
            error = null;
            //Not an error if successful
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return false;
            }

            try
            {
                var responseResult = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                error = JsonConvert.DeserializeObject<ApiError>(responseResult);
                return error != null;
            }
            catch
            {
                return false;
            }
        }
    }
}