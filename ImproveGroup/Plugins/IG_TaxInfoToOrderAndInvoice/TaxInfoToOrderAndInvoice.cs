using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_TaxInfoToOrderAndInvoice
{
    public class TaxInfoToOrderAndInvoice : IPlugin
    {
        IExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (context.MessageName == "Create" && entity.LogicalName == "salesorder")
                    {
                        EntityReference entityReference = (EntityReference)entity.Attributes["quoteid"];
                        UpdateSalesOrder(entityReference.Id, entity);
                    }
                    else if (context.MessageName == "Create" && entity.LogicalName == "invoice")
                    {
                        EntityReference entityReference = (EntityReference)entity.Attributes["salesorderid"];
                        UpdateInvoice(entityReference.Id, entity);
                    }
                    else if (context.MessageName == "Create" && entity.LogicalName == "salesorderdetail")
                    {
                            UpdateSalesOrderDetail(entity);
                    }
                    else if (context.MessageName == "Create" && entity.LogicalName == "invoicedetail")
                    {
                        UpdateInvoiceDetail(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Erron In TaxInfoToOrderAndInvoice Plugin " + ex);
            }
        }
        protected void UpdateSalesOrder(Guid quoteid, Entity salesOrder)
        {
            var fetchData = new
            {
                quoteid = quoteid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quote'>
                                <attribute name='ig1_cityfreighttaxrate' />
                                <attribute name='ig1_countyfreighttaxrate' />
                                <attribute name='ig1_statefreighttaxrate' />
                                <attribute name='ig1_freighttax' />
                                <attribute name='ig1_freightamount'/>
                                <filter>
                                  <condition attribute='quoteid' operator='eq' value='{fetchData.quoteid/*657cf1e2-d3ea-e911-a964-000d3a1d5d22*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];

                if (entity.Attributes.Contains("ig1_statefreighttaxrate") && entity.Attributes["ig1_statefreighttaxrate"] != null)
                {
                    salesOrder.Attributes["ig1_statefreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_statefreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_countyfreighttaxrate") && entity.Attributes["ig1_countyfreighttaxrate"] != null)
                {
                    salesOrder.Attributes["ig1_countyfreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_countyfreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_cityfreighttaxrate") && entity.Attributes["ig1_cityfreighttaxrate"] != null)
                {
                    salesOrder.Attributes["ig1_cityfreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_cityfreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_freighttax") && entity.Attributes["ig1_freighttax"] != null)
                {
                    salesOrder.Attributes["ig1_freighttax"] = Convert.ToDecimal(((Money)entity.Attributes["ig1_freighttax"]).Value);
                }
                if (entity.Attributes.Contains("ig1_freightamount") && entity.Attributes["ig1_freightamount"] != null)
                {
                    salesOrder.Attributes["ig1_freightamount"] = (Money)entity.Attributes["ig1_freightamount"];
                }
                service.Update(salesOrder);
            }
        }
        protected void UpdateInvoice(Guid orderid, Entity invoice)
        {
            var fetchData = new
            {
                salesorderid = orderid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='ig1_cityfreighttaxrate' />
                                <attribute name='ig1_freightamount' />
                                <attribute name='ig1_countyfreighttaxrate' />
                                <attribute name='ig1_statefreighttaxrate' />
                                <attribute name='ig1_freighttax' />
                                <filter>
                                  <condition attribute='salesorderid' operator='eq' value='{fetchData.salesorderid/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];

                if (entity.Attributes.Contains("ig1_statefreighttaxrate") && entity.Attributes["ig1_statefreighttaxrate"] != null)
                {
                    invoice.Attributes["ig1_statefreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_statefreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_countyfreighttaxrate") && entity.Attributes["ig1_countyfreighttaxrate"] != null)
                {
                    invoice.Attributes["ig1_countyfreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_countyfreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_cityfreighttaxrate") && entity.Attributes["ig1_cityfreighttaxrate"] != null)
                {
                    invoice.Attributes["ig1_cityfreighttaxrate"] = Convert.ToDecimal(entity.Attributes["ig1_cityfreighttaxrate"]);
                }
                if (entity.Attributes.Contains("ig1_freighttax") && entity.Attributes["ig1_freighttax"] != null)
                {
                    invoice.Attributes["ig1_freighttax"] = Convert.ToDecimal(((Money)entity.Attributes["ig1_freighttax"]).Value);
                }
                if (entity.Attributes.Contains("ig1_freightamount") && entity.Attributes["ig1_freightamount"] != null)
                {
                    invoice.Attributes["ig1_freightamount"] = (Money)entity.Attributes["ig1_freightamount"];
                }
                service.Update(invoice);
            }
        }
        protected void UpdateSalesOrderDetail(Entity orderDetails)
        {
            Guid quotedetailid = Guid.Empty;
            if (orderDetails.Attributes.Contains("quotedetailid") && orderDetails.Attributes["quotedetailid"] != null)
            {
                EntityReference entityReference = (EntityReference)orderDetails.Attributes["quotedetailid"];
                quotedetailid = entityReference.Id;
            }
            var fetchData = new
            {
                quotedetailid = quotedetailid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quotedetail'>
                                <attribute name='ig1_statetaxrate' />
                                <attribute name='ig1_citytaxrate' />
                                <attribute name='ig1_countytaxrate' />
                                <filter type='and'>
                                  <condition attribute='quotedetailid' operator='eq' value='{fetchData.quotedetailid/*737cf1e2-d3ea-e911-a964-000d3a1d5d22*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = new Entity(orderDetails.LogicalName, orderDetails.Id);
                Entity quotedetails = entityCollection.Entities[0];
                if (quotedetails.Attributes.Contains("ig1_statetaxrate") && quotedetails.Attributes["ig1_statetaxrate"] != null)
                {
                    entity.Attributes["ig1_statetaxrate"] = Convert.ToDecimal(quotedetails.Attributes["ig1_statetaxrate"]);
                }
                if (quotedetails.Attributes.Contains("ig1_countytaxrate") && quotedetails.Attributes["ig1_countytaxrate"] != null)
                {
                    entity.Attributes["ig1_countytaxrate"] = Convert.ToDecimal(quotedetails.Attributes["ig1_countytaxrate"]);
                }
                if (quotedetails.Attributes.Contains("ig1_citytaxrate") && quotedetails.Attributes["ig1_citytaxrate"] != null)
                {
                    entity.Attributes["ig1_citytaxrate"] = Convert.ToDecimal(quotedetails.Attributes["ig1_citytaxrate"]);
                }
                service.Update(entity);
            }
        }
        protected void UpdateInvoiceDetail(Entity invoiceDetail)
        {
            Guid salesorderdetailid = Guid.Empty;
            if (invoiceDetail.Attributes.Contains("salesorderdetailid") && invoiceDetail.Attributes["salesorderdetailid"] != null)
            {
                EntityReference entityReference = (EntityReference)invoiceDetail.Attributes["salesorderdetailid"];
                salesorderdetailid = entityReference.Id;
            }

            var fetchData = new
            {
                salesorderdetailid = salesorderdetailid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorderdetail'>
                                <attribute name='ig1_statetaxrate' />
                                <attribute name='ig1_citytaxrate' />
                                <attribute name='ig1_countytaxrate' />
                                <filter>
                                  <condition attribute='salesorderdetailid' operator='eq' value='{fetchData.salesorderdetailid/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = new Entity(invoiceDetail.LogicalName, invoiceDetail.Id);

                Entity salesOrderDetail = entityCollection.Entities[0];
                if (salesOrderDetail.Attributes.Contains("ig1_statetaxrate") && salesOrderDetail.Attributes["ig1_statetaxrate"] != null)
                {
                    entity.Attributes["ig1_statetaxrate"] = Convert.ToDecimal(salesOrderDetail.Attributes["ig1_statetaxrate"]);
                }
                if (salesOrderDetail.Attributes.Contains("ig1_countytaxrate") && salesOrderDetail.Attributes["ig1_countytaxrate"] != null)
                {
                    entity.Attributes["ig1_countytaxrate"] = Convert.ToDecimal(salesOrderDetail.Attributes["ig1_countytaxrate"]);
                }
                if (salesOrderDetail.Attributes.Contains("ig1_citytaxrate") && salesOrderDetail.Attributes["ig1_citytaxrate"] != null)
                {
                    entity.Attributes["ig1_citytaxrate"] = Convert.ToDecimal(salesOrderDetail.Attributes["ig1_citytaxrate"]);
                }
                service.Update(entity);
            }
        }
    }
}
