using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_DeleteBidSheet
{
    public class DeleteBidSheet : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(null);

                if (context.InputParameters.Equals(null) || string.IsNullOrEmpty(context.InputParameters["recordId"].ToString()))
                {
                    return;
                }

                Guid recordId = new Guid(context.InputParameters["recordId"].ToString());
                if (recordId == Guid.Empty)
                {
                    return;
                }
                string[] entities = { "ig1_bidsheetpricelistitem", "ig1_associatedcost", "ig1_bidsheetproduct", "ig1_bscategoryvendor" };
                for (int i = 0; i < entities.Length; i++)
                {
                    DeleteAssociatedRecords(recordId, entities[i]);
                }
                service.Delete("ig1_bidsheet", recordId);
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in DeleteBidSheet Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected void DeleteAssociatedRecords(Guid recordId, string entityname)
        {
            QueryByAttribute query = new QueryByAttribute(entityname);
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(recordId);
            EntityCollection ec = service.RetrieveMultiple(query);
            foreach (var rec in ec.Entities)
            {
                service.Delete(entityname, rec.Id);
            }
        }
    }
}
