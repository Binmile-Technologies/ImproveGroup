using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ImproveGroup
{
    public class ReviseBidSheet : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracing;
        IOrganizationServiceFactory servicefactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = servicefactory.CreateOrganizationService(context.UserId);
            try
            {

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    EntityReference entity = (EntityReference)context.InputParameters["Target"];
                    if (entity.LogicalName != "ig1_bidsheet")
                    {
                        return;
                    }

                    var bidSheetId = string.Empty;
                    bidSheetId = entity.Id.ToString().Replace("{", "").Replace("}", "");

                    Entity existingBidSheet = service.Retrieve("ig1_bidsheet", new Guid(bidSheetId), new ColumnSet(true));
                    Entity bidSheet = new Entity("ig1_bidsheet");
                    foreach (KeyValuePair<string, object> attr in existingBidSheet.Attributes)
                    {
                        if (attr.Key == "statecode" || attr.Key == "statuscode" || attr.Key == "ig1_bidsheetid")
                            continue;
                        if (attr.Key == "ig1_status")
                        {
                            bidSheet[attr.Key] = new OptionSetValue(Convert.ToInt32(286150002));
                        }
                        else
                        {
                            bidSheet[attr.Key] = attr.Value;
                        }
                    }
                    Guid newBidSheetId = service.Create(bidSheet);

                    //Nazish - 10-07-2019 - Cloning the child records from exiating Bid Sheet.
                    CloneBidSheetCategoryVendors(newBidSheetId, existingBidSheet);
                    CloneBidSheetProducts(newBidSheetId, existingBidSheet);
                    //CloneBidSheetLineItems(newBidSheetId, existingBidSheet);
                    CloneAssociatedCost(newBidSheetId, existingBidSheet);

                    //Nazish - 10-07-2019 - Updating the Revision Id
                    UpdateRevisionId(existingBidSheet);

                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "Error";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.InnerException;
                service.Create(errorLog);
                throw;
            }
        }
        protected void CloneBidSheetCategoryVendors(Guid newBidSheetId, Entity existingBidSheet)
        {
            try
            {
                QueryByAttribute query = new QueryByAttribute("ig1_bscategoryvendor");
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("ig1_bidsheet");
                query.Values.AddRange(existingBidSheet.Id);
                List<Entity> existingBidSheetCategoryVendor = service.RetrieveMultiple(query).Entities.ToList();

                foreach (Entity bidSheetCategoryVendor in existingBidSheetCategoryVendor)
                {
                    Entity newBidSheetCategoryVendor = new Entity("ig1_bscategoryvendor");
                    foreach (KeyValuePair<string, object> attr in bidSheetCategoryVendor.Attributes)
                    {
                        if (attr.Key == "ig1_bscategoryvendorid")
                            continue;

                        newBidSheetCategoryVendor[attr.Key] = attr.Value;
                    }

                    newBidSheetCategoryVendor["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", newBidSheetId);

                    service.Create(newBidSheetCategoryVendor);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected void CloneBidSheetProducts(Guid newBidSheetId, Entity existingBidSheet)
        {
            try
            {
                QueryByAttribute query = new QueryByAttribute("ig1_bidsheetproduct");
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("ig1_bidsheet");
                query.Values.AddRange(existingBidSheet.Id);
                List<Entity> existingBidSheetProducts = service.RetrieveMultiple(query).Entities.ToList();

                foreach (Entity bidSheetProducts in existingBidSheetProducts)
                {
                    Entity newBidSheetProducts = new Entity("ig1_bidsheetproduct");
                    foreach (KeyValuePair<string, object> attr in bidSheetProducts.Attributes)
                    {
                        if (attr.Key == "ig1_bidsheetproductid")
                            continue;

                        newBidSheetProducts[attr.Key] = attr.Value;
                    }

                    newBidSheetProducts["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", newBidSheetId);

                    service.Create(newBidSheetProducts);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected void CloneBidSheetLineItems(Guid newBidSheetId, Entity existingBidSheet, string existingAssociatedCostId, string newAssociatedCostId)
        {
            try
            {
                QueryByAttribute query = new QueryByAttribute("ig1_bidsheetpricelistitem");
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("ig1_bidsheet");
                query.Values.AddRange(existingBidSheet.Id);

                List<Entity> existingBidSheetLineItems = service.RetrieveMultiple(query).Entities.ToList();

                foreach (Entity bidSheetLineItem in existingBidSheetLineItems)
                {
                    EntityReference associatedCost=null;
                    string associatedCostId=null;
                    if (bidSheetLineItem.Attributes.Contains("ig1_associatedcostid"))
                    {
                        associatedCost = (EntityReference)bidSheetLineItem.Attributes["ig1_associatedcostid"];
                        associatedCostId = associatedCost.Id.ToString();
                    }
                     
                    Entity newBidSheetLineItems = new Entity("ig1_bidsheetpricelistitem");
                    if (existingAssociatedCostId != null && newAssociatedCostId != null && existingAssociatedCostId == associatedCostId)
                    {
                        foreach (KeyValuePair<string, object> attr in bidSheetLineItem.Attributes)
                        {
                            if (attr.Key == "ig1_bidsheetpricelistitemid")
                                continue;

                            if (attr.Key == "ig1_associatedcostid")
                                newBidSheetLineItems[attr.Key] = new EntityReference("ig1_associatedcost", new Guid(newAssociatedCostId));

                            else
                                newBidSheetLineItems[attr.Key] = attr.Value;
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> attr in bidSheetLineItem.Attributes)
                        {
                            if (attr.Key == "ig1_bidsheetpricelistitemid")
                                continue;

                            newBidSheetLineItems[attr.Key] = attr.Value;
                        }
                    }
                    newBidSheetLineItems["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", newBidSheetId);
                    service.Create(newBidSheetLineItems);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected void CloneAssociatedCost(Guid newBidSheetId, Entity existingBidSheet)
        {
            try
            {
                QueryByAttribute query = new QueryByAttribute("ig1_associatedcost");
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("ig1_bidsheet");
                query.Values.AddRange(existingBidSheet.Id);

                string newAssociatedCostId=null;
                string existingAssociatedCostId=null;

                List<Entity> existingAssociatedCost = service.RetrieveMultiple(query).Entities.ToList();
                if (existingAssociatedCost.Count > 0)
                {
                    foreach (Entity associatedCost in existingAssociatedCost)
                    {
                        Entity newAssociatedCost = new Entity("ig1_associatedcost");
                        foreach (KeyValuePair<string, object> attr in associatedCost.Attributes)
                        {
                            if (attr.Key == "ig1_associatedcostid")
                                continue;

                            newAssociatedCost[attr.Key] = attr.Value;
                        }
                        newAssociatedCost["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", newBidSheetId);
                         newAssociatedCostId = service.Create(newAssociatedCost).ToString();
                         existingAssociatedCostId = associatedCost.Attributes["ig1_associatedcostid"].ToString();
                        CloneBidSheetLineItems(newBidSheetId, existingBidSheet, existingAssociatedCostId, newAssociatedCostId);
                    }
                }
                else
                    CloneBidSheetLineItems(newBidSheetId, existingBidSheet, existingAssociatedCostId, newAssociatedCostId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected void UpdateRevisionId(Entity existingBidSheet)
        {
            Entity entity = service.Retrieve("ig1_bidsheet", existingBidSheet.Id, new ColumnSet("ig1_revisionid", "ig1_status", "ig1_opportunitytitle"));
            try
            {
                var opportunityTitle = entity.GetAttributeValue<EntityReference>("ig1_opportunitytitle");
                var opportunityId = opportunityTitle.Id;
                var maxRevisionId = GetMaxRevisionId(opportunityId);
                //var revisionID = entity.GetAttributeValue<int>("ig1_revisionid");
                var status = entity.GetAttributeValue<OptionSetValue>("ig1_status");
                if (!maxRevisionId.Equals(null) && !maxRevisionId.Equals(""))
                {
                    maxRevisionId++;
                    entity["ig1_revisionid"] = maxRevisionId;
                }
                entity["ig1_status"] = new OptionSetValue(Convert.ToInt32(286150001));
                service.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public int GetMaxRevisionId(Guid OpportunityId)
        {
            try
            {
                var maxRevisionId = 0;
                var fetchData = new
                {
                    ig1_opportunitytitle = OpportunityId,
                    statecode = "0"
                };
                var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_revisionid' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*0cd068b0-a1cf-e911-a960-000d3a1d52e7*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

                EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                foreach (var bidsheet in result.Entities)
                {
                    var revisionId = (int)bidsheet.Attributes["ig1_revisionid"];
                    if (revisionId > maxRevisionId)
                        maxRevisionId = revisionId;
                }
                return maxRevisionId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}