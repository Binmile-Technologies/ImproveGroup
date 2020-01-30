using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ImproveGroup
{
    public class OpportunityPriceList : IPlugin
    {
        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Setup
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            #endregion

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "opportunity")
                {
                    return;
                }
                try
                {
                    if (entity.Attributes.Contains("transactioncurrencyid"))
                    {
                        var transactionCurrency = (EntityReference)entity.Attributes["transactioncurrencyid"];
                        var currencyId = (Guid)transactionCurrency.Id;

                        Entity priceList = new Entity("pricelevel");
                        priceList["name"] = entity.Attributes["name"] + entity.Id.ToString();
                        priceList["transactioncurrencyid"] = new EntityReference(transactionCurrency.LogicalName, currencyId);
                        //Changes made to fix the price list issues...
                        //service.Create(priceList);
                        var priceListId = service.Create(priceList);
                        UpdateOpportunityPriceList(priceListId, entity.Id);
                    }
                }
                catch (Exception ex)
                {
                    IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);
                    Entity errorLog = new Entity("ig1_pluginserrorlogs");
                    errorLog["ig1_name"] = "Error";
                    errorLog["ig1_errormessage"] = ex.Message;
                    errorLog["ig1_errordescription"] = ex.InnerException;
                    serviceAdmin.Create(errorLog);
                    throw;
                }
            }
        }
        protected void UpdateOpportunityPriceList(Guid priceListId, Guid opportunityId)
        {
            try
            {
                Entity entity = service.Retrieve("opportunity", opportunityId, new ColumnSet("pricelevelid"));
                entity["pricelevelid"] = new EntityReference("pricelevel", priceListId);
                service.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
