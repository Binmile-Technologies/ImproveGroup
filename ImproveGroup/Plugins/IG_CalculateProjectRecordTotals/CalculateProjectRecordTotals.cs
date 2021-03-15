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
                    if (entity.LogicalName == "opportunity")
                    {
                        EntityReference projectRecord = null;
                        decimal bidsheettotal = Convert.ToDecimal(0);

                        Entity opportunity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord", "ig1_bidsheettotal"));
                        if (opportunity.Attributes.Contains("ig1_projectrecord") && opportunity.Attributes["ig1_projectrecord"] != null)
                        {
                           projectRecord  = (EntityReference)opportunity.Attributes["ig1_projectrecord"];
                        }
                        if (opportunity.Attributes.Contains("ig1_bidsheettotal") && opportunity.Attributes["ig1_bidsheettotal"] != null)
                        {
                            bidsheettotal = Convert.ToDecimal(opportunity.Attributes["ig1_bidsheettotal"]);
                        }
                    }
                    if (entity.LogicalName == "ig1_projectrecordcost")
                    {
                        Entity actualCost = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord", "ig1_name"));
                        if (actualCost.Attributes.Contains("ig1_projectrecord") && actualCost.Attributes["ig1_projectrecord"] !=null)
                        {
                            EntityReference projectRecord = (EntityReference)actualCost.Attributes["ig1_projectrecord"];
                            QuickBooksTotals(projectRecord);
                            //TimeTraxCostsTotals(projectRecord);
                        } 
                    }
                    if (entity.LogicalName== "ig1_projectrecordhours")
                    {
                        Entity actulaHours = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (actulaHours.Attributes.Contains("ig1_projectrecord") && actulaHours.Attributes["ig1_projectrecord"] != null)
                        {
                            EntityReference projectRecord = (EntityReference)actulaHours.Attributes["ig1_projectrecord"];
                            //TimeTraxHoursTotls(projectRecord);
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
            entity.Attributes["ig1_bestsalesestimate"] = new Money(totalAmount);
            service.Update(entity);
        }
        protected void QuickBooksTotals(EntityReference project)
        {
            decimal PO_Bills = Convert.ToDecimal(0);
            decimal IG_Install_Po_Bills = Convert.ToDecimal(0);
            decimal Travel = Convert.ToDecimal(0);


        var fetchData = new
        {
            statecode = "0",
            ig1_projectrecord = project.Id,
            ig1_amount = "0"
        };
        var fetchXml = $@"
                        <fetch>
                            <entity name='ig1_projectrecordcost'>
                            <attribute name='ig1_amount' />
                            <attribute name='ig1_expensetype' />
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                <condition attribute='ig1_txnid' operator='not-null' />
                                <condition attribute='ig1_txnlineid' operator='not-null' />
                                <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*bd47d04f-cf32-ea11-a813-000d3a55d0f0*/}'/>
                                <condition attribute='ig1_amount' operator='gt' value='{fetchData.ig1_amount/*0*/}'/>
                            </filter>
                            </entity>
                        </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("ig1_amount") && item.Attributes["ig1_amount"] != null)
                    {
                        Money money = (Money)item.Attributes["ig1_amount"];

                        if (item.Attributes.Contains("ig1_expensetype") && item.Attributes["ig1_expensetype"] != null)
                        {
                            EntityReference entityReference = (EntityReference)item.Attributes["ig1_expensetype"];

                            if (entityReference.Name == "PO Bill")
                            {
                                PO_Bills += Convert.ToDecimal(money.Value);
                            }
                            else if (entityReference.Name == "IG Install PO Bill")
                            {
                                IG_Install_Po_Bills += Convert.ToDecimal(money.Value);
                            }
                            else if (entityReference.Name == "Travel Cost")
                            {
                                Travel += Convert.ToDecimal(money.Value);
                            }
                        }
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_pobills"] = new Money(PO_Bills);
            entity.Attributes["ig1_iginstallpobill"] = new Money(IG_Install_Po_Bills);
            entity.Attributes["ig1_travel"] = new Money(Travel);
            service.Update(entity);
        }
        protected void TimeTraxCostsTotals(EntityReference project)
        {
            decimal salesCost = Convert.ToDecimal(0);
            decimal designCost = Convert.ToDecimal(0);
            decimal pmCost = Convert.ToDecimal(0);
            decimal installerCost = Convert.ToDecimal(0);
            var fetchData = new
            {
                statecode = "0",
                ig1_projectrecord = project.Id,
                ig1_amount = "0"
            };
            var fetchXml = $@"
                            <fetch distinct='true'>
                              <entity name='ig1_projectrecordcost'>
                                <attribute name='ig1_expensetype' />
                                <attribute name='ig1_amount' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*ig1_projectrecord*/}'/>
                                  <condition attribute='ig1_amount' operator='gt' value='{fetchData.ig1_amount/*0*/}'/>
                                  <condition attribute='ig1_txnid' operator='null' />
                                  <condition attribute='ig1_txnlineid' operator='null' />
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("ig1_amount") && item.Attributes["ig1_amount"] != null)
                    {
                        Money money = (Money)item.Attributes["ig1_amount"];
                        if (item.Attributes.Contains("ig1_expensetype") && item.Attributes["ig1_expensetype"] != null)
                        {
                            EntityReference type = (EntityReference)item.Attributes["ig1_expensetype"];
                            if (type.Name == "Sales Cost")
                            {
                                salesCost += Convert.ToDecimal(money.Value);
                            }
                            else if (type.Name == "Design Cost")
                            {
                                designCost += Convert.ToDecimal(money.Value);
                            }
                            else if (type.Name == "PM Cost")
                            {
                                pmCost += Convert.ToDecimal(money.Value);
                            }
                            else if (type.Name == "Installer Cost")
                            {
                                installerCost += Convert.ToDecimal(money.Value);
                            }
                        }
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_actualsalescost"] = new Money(salesCost);
            entity.Attributes["ig1_actualdesigncost"] = new Money(designCost);
            entity.Attributes["ig1_actualpmcost"] = new Money(pmCost);
            entity.Attributes["ig1_installercost"] = new Money(installerCost);
            service.Update(entity);
        }
        protected void TimeTraxHoursTotls(EntityReference project)
        {
            decimal salesHours = Convert.ToDecimal(0);
            decimal designHours = Convert.ToDecimal(0);
            decimal pmHours = Convert.ToDecimal(0);
            decimal installerHours = Convert.ToDecimal(0);

            var fetchData = new
            {
                statecode = "0",
                ig1_projectrecord = project.Id,
                ig1_duration = "0"
            };
            var fetchXml = $@"
                            <fetch distinct='true'>
                              <entity name='ig1_projectrecordhours'>
                                <attribute name='ig1_duration' />
                                <attribute name='ig1_type' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*ig1_projectrecord*/}'/>
                                  <condition attribute='ig1_duration' operator='gt' value='{fetchData.ig1_duration/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count>0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    if (item.Attributes.Contains("ig1_duration") && item.Attributes["ig1_duration"] != null)
                    {
                        decimal duration = Convert.ToDecimal(item.Attributes["ig1_duration"]);
                        if (item.Attributes.Contains("ig1_type") && item.Attributes["ig1_type"] != null)
                        {
                            OptionSetValue type = (OptionSetValue)item.Attributes["ig1_type"];
                            if (type.Value == Convert.ToInt32(0))
                            {
                                salesHours += duration;
                            }
                            else if (type.Value == Convert.ToInt32(1))
                            {
                                designHours += duration;
                            }
                            else if (type.Value == Convert.ToInt32(2))
                            {
                                pmHours += duration;
                            }
                            else if (type.Value == Convert.ToInt32(3))
                            {
                                installerHours += duration;
                            }
                        }
                    }
                }
            }
            Entity entity = service.Retrieve(project.LogicalName, project.Id, new ColumnSet("ig1_projectrecordid"));
            entity.Attributes["ig1_saleshours"] = salesHours;
            entity.Attributes["ig1_actualdesignhours"] = designHours;
            entity.Attributes["ig1_actualpmhours"] = pmHours;
            entity.Attributes["ig1_installerhours"] = installerHours;
            service.Update(entity);
        }
    }
}
