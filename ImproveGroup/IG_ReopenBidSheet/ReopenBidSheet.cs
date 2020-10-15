using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_ReopenBidSheet
{
    public class ReopenBidSheet : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = ServiceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.MessageName == "ig1_ReopenBidsheet" && context.InputParameters != null && context.InputParameters.Count > 0)
                {
                    Guid opportunityid = Guid.Empty;
                    Guid bidsheetid = Guid.Empty;
                    Guid pricelistid = Guid.Empty;
                    string upperRevision = string.Empty;

                    if (context.InputParameters.Contains("opportunityid") && !string.IsNullOrEmpty(context.InputParameters["opportunityid"].ToString()))
                    {
                        opportunityid = new Guid(context.InputParameters["opportunityid"].ToString());
                    }
                    if (context.InputParameters.Contains("bidsheetid") && !string.IsNullOrEmpty(context.InputParameters["bidsheetid"].ToString()))
                    {
                        bidsheetid = new Guid(context.InputParameters["bidsheetid"].ToString());
                    }
                    if (context.InputParameters.Contains("pricelistid") && !string.IsNullOrEmpty(context.InputParameters["pricelistid"].ToString()))
                    {
                        pricelistid = new Guid(context.InputParameters["pricelistid"].ToString());
                    }
                    if (context.InputParameters.Contains("upperRevision") && !string.IsNullOrEmpty(context.InputParameters["upperRevision"].ToString()))
                    {
                        upperRevision = context.InputParameters["upperRevision"].ToString();
                    }
                    if (opportunityid != Guid.Empty && bidsheetid != Guid.Empty)
                    {
                        DeleteOpportunityProducts(opportunityid);
                        DeletePriceListItems(pricelistid);
                        UpdateBidSheetsStatus(opportunityid, bidsheetid, upperRevision);
                    }
                }
            }
            catch (Exception ex)
            {
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                trace.Trace("Exception in ReopenBidSheet Plugin");
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }
        protected void DeleteOpportunityProducts(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='opportunityproduct'>
                                <attribute name='productname' />
                                <attribute name='opportunityproductname' />
                                <filter type='and'>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*opportunity*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                }
            }
        }
        protected void DeletePriceListItems(Guid pricelistid)
        {
            var fetchData = new
            {
                pricelevelid = pricelistid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='productpricelevel'>
                                <attribute name='productpricelevelid' />
                                <attribute name='productidname' />
                                <filter type='and'>
                                  <condition attribute='pricelevelid' operator='eq' value='{fetchData.pricelevelid/*9e1ee952-010b-eb11-a813-000d3a98d873*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                }
            }
        }
        protected void UpdateBidSheetsStatus(Guid opportunityid, Guid bidsheetid, string upperRevision)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityid,
                ig1_upperrevisionid = upperRevision
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_bidsheetid' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*opportunity*/}'/>
                                  <condition attribute='ig1_upperrevisionid' operator='eq' value='{fetchData.ig1_upperrevisionid/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Id == bidsheetid)
                    {
                        item.Attributes["ig1_status"] = new OptionSetValue(286150001);
                        item.Attributes["ig1_associated"] = false;
                        service.Update(item);
                    }
                    else
                    {
                        item.Attributes["ig1_status"] = new OptionSetValue(286150002);
                        item.Attributes["ig1_associated"] = false;
                        service.Update(item);
                    }
                }
            }
        }
    }
}
