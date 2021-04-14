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
        Boolean outparameterflag = false;
        String entityname = "";
        bool invoiceflagst = false;
        bool orderstatus = false;
        bool checkquote = false;
        bool workorderst = false;
        bool purchorderst = false;
        bool purchasestat = false;
        bool bidsheetstatus = false;
        bool oppstat = false;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(null);


                if (context.MessageName == "ig1_DeleteOpportunityAndRelatedRecords" && context.InputParameters.Contains("selectedOpportunity") && !string.IsNullOrEmpty(context.InputParameters["selectedOpportunity"].ToString()))
                {

                    Guid opportunityid = new Guid(context.InputParameters["selectedOpportunity"].ToString()); //new Guid("26e0028c-6c62-ea11-a811-000d3a55dce2");//
                    if (opportunityid != Guid.Empty)
                    {
                        CheckStatus(opportunityid);
                        if (invoiceflagst || orderstatus || checkquote || workorderst || purchorderst || purchasestat || bidsheetstatus || oppstat)
                        {

                            return;
                        }
                        DeleteRelatedInvoices(opportunityid);
                        DeleteRelatedOrders(opportunityid);
                        DeleteRelatedQuotes(opportunityid);
                        DeleteWorkOrder(opportunityid);
                        DeleteRelatedBidsheets(opportunityid);
                        DeleteRelatedProjectRecord(opportunityid);
                        Entity priceentity = service.Retrieve("opportunity", opportunityid, new ColumnSet("pricelevelid"));
                        if (priceentity.Attributes.Contains("pricelevelid") && priceentity.Attributes["pricelevelid"] != null)
                        {
                            Guid pricelistid = priceentity.GetAttributeValue<EntityReference>("pricelevelid").Id;

                            if (pricelistid != Guid.Empty)
                            {
                                try
                                {
                                    DeletePricelistItem(pricelistid);
                                    service.Delete("pricelevel", pricelistid);
                                    
                                }

                                catch
                                {

                                }

                                finally
                                {
                                    service.Delete("opportunity", opportunityid);
                                }

                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in DeleteOpportunityAndRelatedRecords Plugin " + ex);
            }
        }
        protected void DeleteRelatedInvoices(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid1 = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='invoice'>
                                <attribute name='invoiceid' />
                                <attribute name='ig1_projectnumber' />
                                <attribute name='statecode' />
                                <filter>
                                <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid1/*opportunity*/}'/>
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
                                <attribute name='statecode' />
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
                                <attribute name='statecode' />
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
                                <attribute name='ig1_status' />
                                 <attribute name='ig1_pricelist' />
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
                    if (entity.Attributes.Contains("ig1_pricelist") && entity.Attributes["ig1_pricelist"] != null)
                    {
                        Guid pricelistid = entity.GetAttributeValue<EntityReference>("ig1_pricelist").Id;

                        if (pricelistid != Guid.Empty)
                        {
                            DeletePricelistItem(pricelistid);

                        }

                    }

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
        protected void DeleteWorkOrder(Guid opportunityid)
        {

            var fetchData = new
            {
                msdyn_opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_workorder'>
                                <attribute name='msdyn_workorderid' />
                                <attribute name='ig1_workorderstatus' />                               
                                <filter>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*cfedbe4f-8cc6-4e3c-b3fd-f7027393faa2*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {
                    DeletePurchaseOrder(entity.Id);
                    DeleteResourceRequirement(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);

                }


            }

        }
        protected void DeletePurchaseOrder(Guid Workorderid)
        {

            var fetchData = new
            {
                msdyn_workorder = Workorderid
            };
            var fetchXml = $@"
                                <fetch>
                                  <entity name='msdyn_purchaseorder'>
                                    <attribute name='msdyn_purchaseorderid' />
                                    <attribute name='statecode' />
                                    <filter>
                                      <condition attribute='msdyn_workorder' operator='eq' value='{fetchData.msdyn_workorder/*4c507cde-8f04-4050-a651-4bb769e39e64*/}'/>
                                    </filter>
                                  </entity>
                                </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {
                    DeletePurchaseOrderBill(entity.Id);
                    service.Delete(entity.LogicalName, entity.Id);

                }


            }

        }
        protected void DeletePurchaseOrderBill(Guid PurchaseOrderid)
        {

            var fetchData = new
            {
                msdyn_purchaseorder = PurchaseOrderid
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='msdyn_purchaseorderbill'>
                        <attribute name='msdyn_purchaseorderbillid' />
                        <attribute name='statecode' />
                        <filter>
                          <condition attribute='msdyn_purchaseorder' operator='eq' value='{fetchData.msdyn_purchaseorder/*a63219cd-c7ae-47d7-bdcf-c02837be523c*/}'/>
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
        protected void DeleteResourceRequirement(Guid Workorderid)
        {
            var fetchData = new
            {
                msdyn_workorder = Workorderid
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='msdyn_resourcerequirement'>
                            <attribute name='msdyn_resourcerequirementid' />
                            <attribute name='msdyn_workorder' />
                            <filter>
                              <condition attribute='msdyn_workorder' operator='eq' value='{fetchData.msdyn_workorder/*48cec1c6-87b7-4ffa-b047-8f44d3a7595c*/}'/>
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
        protected void DeletePricelistItem(Guid pricelistid)
        {

            var fetchData = new
            {
                pricelevelid = pricelistid
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='productpricelevel'>
                            <attribute name='productpricelevelid' />
                            <filter>
                              <condition attribute='pricelevelid' operator='eq' value='{fetchData.pricelevelid/*c87bfd26-4579-4daa-b03b-2ab8e5a3e4a7*/}'/>
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
        protected void CheckStatus(Guid opportunityid)
        {
            oppstat = checkopportunity(opportunityid);
            if (oppstat)
            {
                return;
            }
            invoiceflagst = checkinvoice(opportunityid);
            if (invoiceflagst)
            {
                return;
            }
            orderstatus = CheckOrderStatus(opportunityid);
            {
                if (orderstatus)
                {
                    return;
                }

            }
            checkquote = CheckRelatedQuote(opportunityid);
            if (checkquote)
            {
                return;

            }
            workorderst = checkWorkorderstatus(opportunityid);
            if (workorderst)
            {

                return;
            }
            //purchorderst = CheckPurchaseOrderstatus(opportunityid);
            //if (purchorderst)
            //{

            //    return;
            //}
            //purchasestat = CheckPurchaseOrderBillstatus(opportunityid);
            //if (purchasestat)
            //{

            //    return;
            //}
            // bidsheetstatus= checkBidsheetstaus(opportunityid);
            //if (bidsheetstatus)
            //{
            //    return;
            //}

        }

        protected bool checkinvoice(Guid opportunityid)
        {

            var fetchData = new
            {
                opportunityid = opportunityid              
            };
            var fetchXml = $@"
            <fetch>
              <entity name='invoice'>
                <attribute name='statecode' />
                <filter>
                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*1fb75bf6-a54e-41b2-927b-21aa128bd103*/}'/>                  
                </filter>
              </entity>
            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("statecode") && entity.Attributes["statecode"] != null)
                    {
                        int invoicestatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                        if (invoicestatus == 2)
                        {
                            outparameterflag = true;
                            entityname = "Cannot Delete Opportunity due to the Associated Invoice being in a Paid Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            context.OutputParameters["Checkstatus"] = outparameterflag;

                            return true;

                        }
                    }
                }
            }
            return false;
        }

        protected bool CheckOrderStatus(Guid oppotunityid)
        {
            var fetchData = new
            {
                opportunityid = oppotunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='salesorder'>
                                <attribute name='salesorderid' />
                                <attribute name='statecode' />
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
                    if (entity.Attributes.Contains("statecode") && entity.Attributes["statecode"] != null)
                    {
                        int salesstatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                        if (salesstatus == 1 || salesstatus == 3 || salesstatus == 4)
                        {
                            outparameterflag = true;
                            entityname = "Cannot Delete Opportunity due to the Associated Order Being in a Fulfilled, Invoiced or Submitted Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        protected bool CheckRelatedQuote(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quote'>
                                <attribute name='quoteid' />
                                <attribute name='statecode' />
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
                    if (entity.Attributes.Contains("statecode") && entity.Attributes["statecode"] != null)
                    {

                        int status = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                        if (status == 2)
                        {
                            outparameterflag = true;
                            entityname = "Cannot Delete Opportunity due to the Associated Quote being in a Won Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            return true;
                        }

                    }


                }
            }
            return false;
        }

        protected bool checkWorkorderstatus(Guid opportunityid)
        {

            var fetchData = new
            {
                msdyn_opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_workorder'>
                                <attribute name='msdyn_workorderid' />
                                <attribute name='ig1_workorderstatus' />                               
                                <filter>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*cfedbe4f-8cc6-4e3c-b3fd-f7027393faa2*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("ig1_workorderstatus") && entity.Attributes["ig1_workorderstatus"] != null)
                    {
                        Guid woid = entity.GetAttributeValue<Guid>("msdyn_workorderid");
                        int Workorderstaus = entity.GetAttributeValue<OptionSetValue>("ig1_workorderstatus").Value;
                        purchorderst = CheckPurchaseOrderstatus(woid);
                        if (purchorderst)
                        {
                            return true;
                        }
                        if (Workorderstaus == 286150004 || Workorderstaus == 286150005)
                        {
                            outparameterflag = true;
                            entityname = "Cannot Delete Opportunity due to the Associated Work Order Being in a Closed-Billed and Open-Completed Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            return true;

                        }
                       
                    }



                }


            }
            return false;
        }

        protected bool CheckPurchaseOrderstatus(Guid Workorderid)
        {

            var fetchData = new
            {
                msdyn_workorder = Workorderid
            };
            var fetchXml = $@"
                                <fetch>
                                  <entity name='msdyn_purchaseorder'>
                                    <attribute name='msdyn_purchaseorderid' />
                                    <attribute name='msdyn_systemstatus' />
                                    <filter>
                                      <condition attribute='msdyn_workorder' operator='eq' value='{fetchData.msdyn_workorder/*4c507cde-8f04-4050-a651-4bb769e39e64*/}'/>
                                    </filter>
                                  </entity>
                                </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("msdyn_systemstatus") && entity.Attributes["msdyn_systemstatus"] != null)
                    {
                        int postatus = entity.GetAttributeValue<OptionSetValue>("msdyn_systemstatus").Value;

                        purchasestat = CheckPurchaseOrderBillstatus(entity.Id);
                        if (purchasestat && postatus == 690970000)
                        {
                            outparameterflag = true;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            entityname = "Opportunity cannot be deleted When an Associated Purchase Order Bill Exists.";
                            context.OutputParameters["EntityName"] = entityname;
                            return true;
                        }
                        if (postatus == 690970004 || postatus == 690970001 || postatus == 286150000)
                        {
                              purchasestat = CheckPurchaseOrderBillstatus(entity.Id);

                            if (purchasestat)
                            {
                                return true;
                            }

                            outparameterflag = true;
                            entityname = "Cannot Delete Opportunity due to the Associated Purchase Order(s) Being in an Acknowledge, Billed or Submitted Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            return true;

                        }

                       
                       
                    }



                }


            }
            return false;

        }

        protected bool CheckPurchaseOrderBillstatus(Guid PurchaseOrderid)
        {

            var fetchData = new
            {
                msdyn_purchaseorder = PurchaseOrderid
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='msdyn_purchaseorderbill'>
                        <attribute name='msdyn_purchaseorderbillid' />
                        <attribute name='statecode' />
                        <filter>
                          <condition attribute='msdyn_purchaseorder' operator='eq' value='{fetchData.msdyn_purchaseorder/*a63219cd-c7ae-47d7-bdcf-c02837be523c*/}'/>
                        </filter>
                      </entity>
                    </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("statecode") && entity.Attributes["statecode"] != null)
                    {
                        int pobillstatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;                      
                        
                            outparameterflag = true;
                            context.OutputParameters["Checkstatus"] = outparameterflag;
                            entityname = "Cannot Delete Opportunity due to the Associated Purchase Order Bill(s) Being in an Active Status.";
                            context.OutputParameters["EntityName"] = entityname;
                            return true;
                        

                    }
                }


            }
            return false;
        }

        //protected bool checkBidsheetstaus(Guid opportunityid)
        //{

        //    var fetchData = new
        //    {
        //        ig1_opportunitytitle = opportunityid
        //    };
        //    var fetchXml = $@"
        //                    <fetch>
        //                      <entity name='ig1_bidsheet'>
        //                        <attribute name='ig1_bidsheetid' />
        //                        <attribute name='ig1_status' />
        //                         <attribute name='ig1_pricelist' />
        //                          <attribute name='ig1_associated' />
        //                        <filter>
        //                          <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*00000000-0000-0000-0000-000000000000*/}'/>
        //                        </filter>
        //                      </entity>
        //                    </fetch>";
        //    EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
        //    if (entityCollection.Entities.Count > 0)
        //    {
        //        foreach (Entity entity in entityCollection.Entities)
        //        {
        //            if (entity.Attributes.Contains("ig1_pricelist") && entity.Attributes["ig1_pricelist"] != null)
        //            {
        //                Guid pricelistid = entity.GetAttributeValue<EntityReference>("ig1_pricelist").Id;
        //                int bidstatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
        //                bool associated = entity.GetAttributeValue<bool>("ig1_associated");

        //                 if(bidstatus== 286150000 && associated == true)
        //                {
        //                    outparameterflag = true;
        //                    context.OutputParameters["Checkstatus"] = outparameterflag;
        //                    entityname = "You have Activated and Associated Bidsheet can not delete opportunity Please Deactivate";
        //                    context.OutputParameters["EntityName"] = entityname;
        //                    return true;
        //                }
        //                else  if (pricelistid != Guid.Empty)
        //                {
        //                 bool pricelis =  checkPriceliststatus(pricelistid);
        //                    return pricelis;
        //                }

        //            }


        //        }
        //    }
        //    return false;
        //}

        protected bool checkPriceliststatus(Guid pricelistid)
        {

            var fetchData = new
            {
                pricelevelid = pricelistid
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='pricelevel'>
                        <attribute name='statecode' />
                        <attribute name='name' />
                        <filter>
                          <condition attribute='pricelevelid' operator='eq' value='{fetchData.pricelevelid/*64ec28b1-66d7-4aa4-bd51-f23f9121a9a1*/}'/>
                        </filter>
                      </entity>
                    </fetch>";



            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)

            {
                outparameterflag = true;
                context.OutputParameters["Checkstatus"] = outparameterflag;
                entityname = "You have Associated pricelist with Bidsheet can not delete opportunity Please Deactivate";
                context.OutputParameters["EntityName"] = entityname;
                return true;

            }
            return false;
        }

        protected bool checkopportunity(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='opportunity'>
                        <attribute name='statecode' />
                        <attribute name='pricelevelid' />
                        <filter>
                          <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*770d41a7-d591-4890-98ac-cacb479477c1*/}'/>
                        </filter>
                      </entity>
                    </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                foreach (Entity entity in entityCollection.Entities)
                {

                    int opportunitystatus = entity.GetAttributeValue<OptionSetValue>("statecode").Value;

                  //  Guid pricelistid = entity.GetAttributeValue<EntityReference>("ig1_pricelist").Id;

                    if (opportunitystatus == 1)
                    {

                        outparameterflag = true;
                        context.OutputParameters["Checkstatus"] = outparameterflag;
                        entityname = "Cannot Delete Opportunity While in a Won Status";
                        context.OutputParameters["EntityName"] = entityname;
                        return true;
                    }
                  
                }


            }

            return false;
        }
    }
}