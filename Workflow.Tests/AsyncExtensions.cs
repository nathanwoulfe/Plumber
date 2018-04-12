using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Workflow.Tests
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Execute the response to access Content as a dynamic
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task<dynamic> GetContent(this IHttpActionResult result)
        {
            HttpResponseMessage response = await result.ExecuteAsync(CancellationToken.None);
            dynamic content = await response.Content.ReadAsAsync<dynamic>();

            return content;
        }
    }
}
