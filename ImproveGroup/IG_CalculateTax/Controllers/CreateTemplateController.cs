using System;
using System.Web.Http;
using IG_ImproveGroup_Web_API.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_ImproveGroup_Web_API.Controllers
{
    public class CreateTemplateController : ApiController
    {
        [HttpGet]
        public void CreateExcelTemplate(Guid BidSheetId)
        {
            try
            {
                var service = Authentication.OrgService();
                if (service != null)
                {
                    Guid userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
                    if (userid != Guid.Empty)
                    {
                        QueryExpression query = new QueryExpression()
                        {
                            EntityName = "ig1_bscategoryvendor",
                            ColumnSet = new ColumnSet(true)
                        };
                        EntityCollection ec= service.RetrieveMultiple(query);
                        if (ec.Entities.Count > 0)
                        {
                            
                        }
                    }
                }
            }
            catch
            { 
            }
        }
    }
}
