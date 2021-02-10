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

                if (context.InputParameters.Equals(null) || !context.InputParameters.Contains("recordId") || string.IsNullOrEmpty(context.InputParameters["recordId"].ToString()))
                {
                    return;
                }
                bool lineItemsDeleted = false;
                bool indirectCostDeleted = false;
                bool bsProductsDeleted = false;
                bool categoriesDeleted = false;
                string[] recordsids =Convert.ToString(context.InputParameters["recordId"]).Split(',');
                for (int i = 0; i < recordsids.Length; i++)
                {
                    Guid recordid = new Guid(recordsids[i]);
                    if (recordid != Guid.Empty)
                    {
                        lineItemsDeleted = BidSheetLineItems(recordid);
                        indirectCostDeleted = IndirectCosts(recordid);
                        bsProductsDeleted = BidsheetProducts(recordid);
                        categoriesDeleted = Categories(recordid);
                        if (lineItemsDeleted && indirectCostDeleted && bsProductsDeleted && categoriesDeleted)
                        {
                            service.Delete("ig1_bidsheet", recordid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }
        protected bool BidSheetLineItems(Guid recordid)
        {
            bool bidsheetlineItemsDeleted = false;
            var fetchData = new
            {
                ig1_bidsheet = recordid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_product' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d9c97819-7813-eb11-a813-000d3a98d1ad*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                int count = 0;
                int totalRecords = entityCollection.Entities.Count;
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                    count++;
                }
                if (totalRecords == count)
                {
                    bidsheetlineItemsDeleted = true;
                }
            }
            else
            {
                bidsheetlineItemsDeleted = true;
            }
            return bidsheetlineItemsDeleted;
        }
        protected bool IndirectCosts(Guid recordid)
        {
            bool indirectCostsDeleted = false;
            var fetchData = new
            {
                ig1_bidsheet = recordid
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='ig1_associatedcost'>
                                <attribute name='ig1_bidsheetcategory' />
                                <filter type='and'>
                                    <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d9c97819-7813-eb11-a813-000d3a98d1ad*/}'/>
                                </filter>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                int count = 0;
                int totalRecords = entityCollection.Entities.Count;
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                    count++;
                }
                if (totalRecords == count)
                {
                    indirectCostsDeleted = true;
                }
            }
            else
            {
                indirectCostsDeleted = true;
            }
            return indirectCostsDeleted;
        }
        protected bool BidsheetProducts(Guid recordid)
        {
            bool bsProductsDeleted = false;
            var fetchData = new
            {
                ig1_bidsheet = recordid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetproduct'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d9c97819-7813-eb11-a813-000d3a98d1ad*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                int count = 0;
                int totalRecords = entityCollection.Entities.Count;
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                    count++;
                }
                if (count == totalRecords)
                {
                    bsProductsDeleted = true;
                }
            }
            else
            {
                bsProductsDeleted = true;
            }
            return bsProductsDeleted;
        }
        protected bool Categories(Guid recordid)
        {
            bool categoriesDeleted = false;
            var fetchData = new
            {
                ig1_bidsheet = recordid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bscategoryvendor'>
                                <attribute name='ig1_category' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                int count = 0;
                int totalRecords = entityCollection.Entities.Count;
                foreach (var item in entityCollection.Entities)
                {
                    service.Delete(item.LogicalName, item.Id);
                    count++;
                }
                if (count == totalRecords)
                {
                    categoriesDeleted = true;
                }
            }
            else
            {
                categoriesDeleted = true;
            }
            return categoriesDeleted;
        }
    }
}
