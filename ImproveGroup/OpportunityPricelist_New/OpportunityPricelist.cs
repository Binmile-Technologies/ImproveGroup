using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpportunityPricelist_New
{
    public class OpportunityPricelist :IPlugin
    {



        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;



        public void Execute(IServiceProvider serviceProvider)
        {

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);


            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName == "opportunity")
                {
                    try
                    {
                        if (entity.Attributes.Contains("transactioncurrencyid") && entity.Attributes["transactioncurrencyid"] != null)
                        {
                            var transactionCurrency = (EntityReference)entity.Attributes["transactioncurrencyid"];
                            var currencyId = (Guid)transactionCurrency.Id;
                            Entity projectnumber = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectnumber"));

                            Entity priceList = new Entity("pricelevel");
                            priceList["name"] = projectnumber.GetAttributeValue<string>("ig1_projectnumber").ToString() + " - 1";
                            priceList["transactioncurrencyid"] = new EntityReference(transactionCurrency.LogicalName, currencyId);
                            //Changes made to fix the price list issues...
                            //service.Create(priceList);
                            var priceListId = service.Create(priceList);
                            UpdateOpportunityPriceList(priceListId, entity.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                        trace.Trace("OpportunityPriceList Plug-in");
                        throw new InvalidPluginExecutionException("Error " + ex);
                    }
                }
                else if (entity.LogicalName == "ig1_bidsheet")
                {
                    Guid id = Guid.Empty;
                    Entity fetchentity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunitytitle", "ig1_status"));
                    if (fetchentity.Attributes.Contains("ig1_opportunitytitle") && fetchentity.Attributes["ig1_opportunitytitle"] != null)
                    {
                        id = fetchentity.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
                    }
                    var fetchData = new
                    {
                        ig1_bidsheetid = entity.Id,
                        ig1_status = "286150003"
                    };
                    var fetchXml = $@"
                                   <fetch>
                                    <entity name='ig1_bidsheet'>
                                       <attribute name='ig1_upperrevisionid' />
                                       <attribute name='ig1_projectnumber' />
                                       <filter type='and'>
                                         <condition attribute='ig1_bidsheetid' operator='eq' value='{fetchData.ig1_bidsheetid/*a43f327c-7434-4c43-ad1e-da3929406fd4*/}'/>
                                         <condition attribute='ig1_status' operator='neq' value='{fetchData.ig1_status/*286150003*/}'/>
                                       </filter>
                                     </entity>
                                   </fetch>";
                    EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (result.Entities.Count > 0 && result.Entities != null)
                    {
                        var attrributes = result.Entities[0];
                        string projectnumber = string.Empty;
                        string uppreviseid = string.Empty;
                        string pricelist = string.Empty;

                        if (attrributes.Attributes.Contains("ig1_projectnumber") && attrributes.Attributes["ig1_projectnumber"] != null)
                        {
                            projectnumber = attrributes.GetAttributeValue<string>("ig1_projectnumber").ToString();
                        }
                        if (attrributes.Attributes.Contains("ig1_upperrevisionid") && attrributes.Attributes["ig1_upperrevisionid"] != null)
                        {
                            uppreviseid = result.Entities[0].GetAttributeValue<int>("ig1_upperrevisionid").ToString();
                        }
                        if (projectnumber != string.Empty && uppreviseid != string.Empty)
                        {
                            pricelist = projectnumber + " - " + uppreviseid;
                        }

                        Guid pricedlistidforbs = GetPricelist(pricelist, id);
                        if (pricedlistidforbs != Guid.Empty)
                        {
                            Entity objentity = new Entity(entity.LogicalName, entity.Id);
                            objentity["ig1_pricelist"] = new EntityReference("pricelevel", pricedlistidforbs);
                            service.Update(objentity);
                        }
                    }
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

         protected Guid GetPricelist(string priceListName,Guid id)
        {
            Guid pricelistid = Guid.Empty;
            var fetchData = new
            {
                name = priceListName
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='pricelevel'>
                                <attribute name='name' />
                                <filter>
                                    <condition attribute='name' operator='eq' value='{fetchData.name/*60617 -2*/}'/>
                                </filter>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                pricelistid = entityCollection.Entities[0].Id;
            }
            else
            {
                Entity transid = service.Retrieve("opportunity", id, new ColumnSet("transactioncurrencyid"));
                var transactionCurrency = (EntityReference)transid.Attributes["transactioncurrencyid"];
                var currencyId = (Guid)transactionCurrency.Id;
                Entity entity = new Entity("pricelevel");
                entity.Attributes["name"] = priceListName;
                entity.Attributes["transactioncurrencyid"] = new EntityReference(transactionCurrency.LogicalName, currencyId);

                pricelistid = service.Create(entity);
            }
            return pricelistid;

        }

       

    }
}
