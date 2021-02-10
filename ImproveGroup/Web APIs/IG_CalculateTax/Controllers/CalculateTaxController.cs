using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IG_ImproveGroup_Web_API.Controllers
{
    public class CalculateTaxController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage GetCalculatedTax([FromBody]JObject body, [FromUri]string url, [FromUri] string auth)
        {
            try
            {
                string req = body.ToString(Newtonsoft.Json.Formatting.None);

                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", auth);
                request.AddParameter("application/json", req, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, JObject.Parse(response.Content));
                }
                else
                {
                    return Request.CreateResponse(response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException);
            }
            
        }
    }
}
