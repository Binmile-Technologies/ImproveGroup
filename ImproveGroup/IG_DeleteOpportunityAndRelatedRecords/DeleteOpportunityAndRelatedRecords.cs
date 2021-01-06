using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_DeleteOpportunityAndRelatedRecords
{
    public class DeleteOpportunityAndRelatedRecords : IPlugin
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
                service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.MessageName == "ig1_DeleteOpportunityAndRelatedRecords" && context.InputParameters.Contains("selectedOpportunity") && !string.IsNullOrEmpty(context.InputParameters["selectedOpportunity"].ToString()))
                {
                    Guid opportunityid = new Guid("d21b8536-5818-4682-baab-ac708095940f");//new Guid(context.InputParameters["selectedOpportunity"].ToString());
                    if (opportunityid != Guid.Empty)
                    {
                        DeleteRelatedInvoices(opportunityid);
                        DeleteRelatedOrders(opportunityid);
                        DeleteRelatedQuotes(opportunityid);
                        DeleteRelatedBidsheets(opportunityid);
                        DeleteRelatedProjectRecord(opportunityid);
                        service.Delete("opportunity", opportunityid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in DeleteOpportunityAndRelatedRecords Plugin "+ex);
            }
        }
        protected void DeleteRelatedInvoices(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='invoice'>
                                <attribute name='invoiceid' />
                                <attribute name='ig1_projectnumber' />
                                <filter>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*opportunity*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    DeleteInvoiceDetails(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteInvoiceDetails(Guid invoiceid)
        {
            var fetchData = new
            {
                invoiceid = invoiceid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='invoicedetail'>
                                <attribute name='invoicedetailid' />
                                <filter>
                                  <condition attribute='invoiceid' operator='eq' value='{fetchData.invoiceid/*invoice*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteRelatedOrders(Guid oppotunityid)
        {
            var fetchData = new
            {
                opportunityid = oppotunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='salesorderid' />
                                <filter>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*opportunity*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    DeleteOrdersDetails(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteOrdersDetails(Guid orderid)
        {
            var fetchData = new
            {
                salesorderid = orderid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorderdetail'>
                                <attribute name='salesorderid' />
                                <filter>
                                  <condition attribute='salesorderid' operator='eq' value='{fetchData.salesorderid/*salesorder*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }

        }
        protected void DeleteRelatedQuotes(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quote'>
                                <attribute name='quoteid' />
                                <filter>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    DeleteQuoteDetails(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteQuoteDetails(Guid quoteid)
        {
            var fetchData = new
            {
                quoteid = quoteid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quotedetail'>
                                <attribute name='quotedetailid' />
                                <filter>
                                  <condition attribute='quoteid' operator='eq' value='{fetchData.quoteid/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteRelatedBidsheets(Guid opportunityid)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_bidsheetid' />
                                <filter>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    DeleteBidsheetLineItems(entity.Id);
                    DeleteBidsheetIndirectCosts(entity.Id);
                    DeleteBidsheetCategories(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteBidsheetLineItems(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_bidsheetpricelistitemid' />
                                <filter>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteBidsheetIndirectCosts(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_bidsheet' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteBidsheetCategories(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bscategoryvendor'>
                                <attribute name='ig1_bscategoryvendorid' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
        protected void DeleteRelatedProjectRecord(Guid opportunityid)
        {
            var fetchData = new
            {
                ig1_opportunity = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectrecordid' />
                                <filter type='and'>
                                    <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }
        }
    }
}
