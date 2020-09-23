using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_CalculateProjectRecordTotals
{
    public class CalculateProjectRecordTotals : IPlugin
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

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        Entity po = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (po.Attributes.Contains("ig1_projectrecord") && po.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)po.Attributes["ig1_projectrecord"];
                            CalculatePOTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "invoice")
                    {
                        Entity invoice = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (invoice.Attributes.Contains("ig1_projectrecord") && invoice.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)invoice.Attributes["ig1_projectrecord"];
                            CalculateInvoiceTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "salesorder")
                    {
                        Entity salesorder = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (salesorder.Attributes.Contains("ig1_projectrecord") && salesorder.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)salesorder.Attributes["ig1_projectrecord"];
                            CalculateOrderTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "quote")
                    {
                        Entity quote = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (quote.Attributes.Contains("ig1_projectrecord") && quote.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)quote.Attributes["ig1_projectrecord"];
                            CalculateQuotesTotals(projectRecord);
                        }

                    }
                    if (entity.LogicalName == "ig1_bidsheet")
                    {
                        Entity bidsheet = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord", "ig1_projectnumber"));
                        if (bidsheet.Attributes.Contains("ig1_projectrecord") && bidsheet.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)bidsheet.Attributes["ig1_projectrecord"];
                            CalculateBidsheetTotals(projectRecord);
                        }
                    }
                }
                else if (context.MessageName.ToLower() == "delete" && context.PreEntityImages.Contains("Image"))
                {
                    Entity entity = (Entity)context.PreEntityImages["Image"];
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        if (entity.Attributes.Contains("ig1_projectrecord") && entity.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            CalculatePOTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "invoice")
                    {
                        if (entity.Attributes.Contains("ig1_projectrecord") && entity.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            CalculateInvoiceTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "salesorder")
                    {
                        if (entity.Attributes.Contains("ig1_projectrecord") && entity.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            CalculateOrderTotals(projectRecord);
                        }
                    }
                    if (entity.LogicalName == "quote")
                    {
                        if (entity.Attributes.Contains("ig1_projectrecord") && entity.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            CalculateQuotesTotals(projectRecord);
                        }

                    }
                    if (entity.LogicalName == "ig1_bidsheet")
                    {
                        if (entity.Attributes.Contains("ig1_projectrecord") && entity.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            CalculateBidsheetTotals(projectRecord);
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
            }
        }
        protected void CalculatePOTotals(EntityReference project)
        {
            decimal billedTotal = Convert.ToDecimal(0);
            decimal total = Convert.ToDecimal(0);
            int count = 0;
            var fetchData = new
            {
                statecode = "0",
                msdyn_systemstatus = "690970002",
                ig1_projectrecord = project.Id
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorder'>
                                <attribute name='msdyn_totalamount' />
                                <attribute name='msdyn_amountbilled' />
                                <attribute name='msdyn_systemstatus' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='msdyn_systemstatus' operator='neq' value='{fetchData.msdyn_systemstatus/*690970002*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                count = Convert.ToInt32(entityCollection.Entities.Count);
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("msdyn_amountbilled") && item.Attributes["msdyn_amountbilled"] != null)
                    {
                        Money money = (Money)item.Attributes["msdyn_amountbilled"];
                        billedTotal += Convert.ToDecimal(money.Value);
                    }
                    if (item.Attributes.Contains("msdyn_totalamount") && item.Attributes["msdyn_totalamount"] != null)
                    {
                        Money money = (Money)item.Attributes["msdyn_totalamount"];
                        total += Convert.ToDecimal(money.Value);
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_totalnumberofpos"] = Convert.ToInt32(count);
            entity.Attributes["ig1_totalamountofpos"] = new Money(total);
            entity.Attributes["ig1_totalamountbilled"] = new Money(billedTotal);
            service.Update(entity);
        }
        protected void CalculateInvoiceTotals(EntityReference project)
        {
            decimal totalAmount = Convert.ToDecimal(0);
            int count = 0;
            var fetchData = new
            {
                statecode = "3",
                ig1_projectrecord = project.Id
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='invoice'>
                                <attribute name='totalamount' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='neq' value='{fetchData.statecode/*3*/}'/>
                                   <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                count = Convert.ToInt32(entityCollection.Entities.Count);
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("totalamount") && item.Attributes["totalamount"] != null)
                    {
                        Money money = (Money)item.Attributes["totalamount"];
                        totalAmount += Convert.ToDecimal(money.Value);
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_totalnumberofinvoices"] = count;
            entity.Attributes["ig1_totalamountofinvoices"] = new Money(totalAmount);
            service.Update(entity);
        }
        protected void CalculateQuotesTotals(EntityReference project)
        {
            decimal totalAmount = Convert.ToDecimal(0);
            int count = 0;
            var fetchData = new
            {
                statecode = "2",
                ig1_projectrecord = project.Id
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quote'>
                                <attribute name='totalamount' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*2*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                 count = Convert.ToInt32(entityCollection.Entities.Count);
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("totalamount") && item.Attributes["totalamount"] != null)
                    {
                        Money money = (Money)item.Attributes["totalamount"]; ;
                        totalAmount += Convert.ToDecimal(money.Value);
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_totalnumberofwonquotes"] = count;
            entity.Attributes["ig1_totalamountofwonquotes"] = new Money(totalAmount);
            service.Update(entity);
        }
        protected void CalculateOrderTotals(EntityReference project)
        {
            decimal totalAmount = Convert.ToDecimal(0);
            int count = 0;
            var fetchData = new
            {
                statecode = "2",
                ig1_projectrecord = project.Id
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='totalamount' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='neq' value='{fetchData.statecode/*2*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                 count = Convert.ToInt32(entityCollection.Entities.Count);
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("totalamount") && item.Attributes["totalamount"] != null)
                    {
                        Money money = (Money)item.Attributes["totalamount"];
                        totalAmount += Convert.ToDecimal(money.Value);
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_totalnumberoforders"] = count;
            entity.Attributes["ig1_totalamountoforders"] = new Money(totalAmount);
            service.Update(entity);
        }
        protected void CalculateBidsheetTotals(EntityReference project)
        {
            decimal totalAmount = Convert.ToDecimal(0);
            int count = 0;
            var fetchData = new
            {
                statecode = "0",
                ig1_associated = "1",
                ig1_projectrecord = project.Id
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_sellprice' />
                                <attribute name='ig1_associated' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                 count = Convert.ToInt32(entityCollection.Entities.Count);
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("ig1_sellprice") && item.Attributes["ig1_sellprice"] != null)
                    {
                        Money money = (Money)item.Attributes["ig1_sellprice"];
                        totalAmount += Convert.ToDecimal(money.Value);
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_totalnumberofassociatedbidsheets"] = count;
            entity.Attributes["ig1_totalamountofassociatedbidsheets"] = new Money(totalAmount);
            service.Update(entity);
        }
    }
}
