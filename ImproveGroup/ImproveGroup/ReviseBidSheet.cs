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
        IOrganizationService service, serviceAdmin;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = servicefactory.CreateOrganizationService(context.UserId);
            serviceAdmin = servicefactory.CreateOrganizationService(null);
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
                        {
                            continue;
                        }
                        else if (attr.Key == "ig1_status")
                        {
                            bidSheet[attr.Key] = new OptionSetValue(Convert.ToInt32(286150002));
                        }
                        else if (attr.Key == "ig1_associated")
                        {
                            bidSheet[attr.Key] = false;
                        }
                        else
                        {
                            bidSheet[attr.Key] = attr.Value;
                        }
                    }
                    //Guid newBidSheetId = service.Create(bidSheet);
                    Guid newBidSheetId = serviceAdmin.Create(bidSheet);

                    //Nazish - 10-07-2019 - Cloning the child records from exiating Bid Sheet.
                    CloneBidSheetCategoryVendors(newBidSheetId, existingBidSheet);
                    CloneBidSheetProducts(newBidSheetId, existingBidSheet);
                    CloneBidSheetLineItems(newBidSheetId, existingBidSheet);
                    //CloneAssociatedCost(newBidSheetId, existingBidSheet);

                    //Nazish - 10-07-2019 - Updating the Revision Id
                    UpdateRevisionId(existingBidSheet);

                }
            }
            catch (Exception ex)
            {
                IOrganizationService serviceAdmin = servicefactory.CreateOrganizationService(null);
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in ReviseBidSheet Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                serviceAdmin.Create(errorLog);
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
        protected void CloneBidSheetLineItems(Guid newBidSheetId, Entity existingBidSheet)
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
                    Entity newBidSheetLineItems = new Entity("ig1_bidsheetpricelistitem");
                    foreach (KeyValuePair<string, object> attr in bidSheetLineItem.Attributes)
                    {
                        if (attr.Key == "ig1_bidsheetpricelistitemid")
                            continue;
                        if (attr.Key == "ig1_associatedcostid")
                        {
                            EntityReference category = (EntityReference)bidSheetLineItem.Attributes["ig1_category"];
                            string newAssociatedCostId = CloneAssociatedCost(newBidSheetId, existingBidSheet, category);
                            if (newAssociatedCostId != null && newAssociatedCostId != "")
                            {
                                newBidSheetLineItems[attr.Key] = new EntityReference("ig1_associatedcost", new Guid(newAssociatedCostId));
                            }
                        }
                        else
                        {
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
        protected string CloneAssociatedCost(Guid newBidSheetId, Entity existingBidSheet, EntityReference category)
        {
            try
            {
                string newAssociatedCostId = null;
                QueryByAttribute query = new QueryByAttribute("ig1_associatedcost");
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("ig1_bidsheet");
                query.Values.AddRange(existingBidSheet.Id);
                List<Entity> existingAssociatedCost = service.RetrieveMultiple(query).Entities.ToList();

                foreach (Entity associatedCost in existingAssociatedCost)
                {
                    var associatedCostCategory = (EntityReference)associatedCost.Attributes["ig1_bidsheetcategory"];
                    bool isCategoryExist = checkDuplicateAssociatedCost(newBidSheetId, category.Id);
                    if (category.Id == associatedCostCategory.Id && !isCategoryExist)
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
                    }
                }
                return newAssociatedCostId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected bool checkDuplicateAssociatedCost(Guid newBidSheetId, Guid categoryId)
        {
            try
            {
                var isCategoryExist = false;
                var fetchData = new
                {
                    ig1_bidsheet = newBidSheetId,
                    ig1_bidsheetcategory = categoryId
                };
                var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_bidsheetcategory' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*61b01ec3-fefa-e911-a812-000d3a4e6fff*/}'/>
                                  <condition attribute='ig1_bidsheetcategory' operator='eq' value='{fetchData.ig1_bidsheetcategory/*7067b58b-c5ee-4a14-8cb5-fe03e79b5b08*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                foreach (var bidsheet in result.Entities)
                {
                    EntityReference category = (EntityReference)bidsheet.Attributes["ig1_bidsheetcategory"];
                    if (category != null && category.Id == categoryId)
                        isCategoryExist = true;
                }
                return isCategoryExist;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        protected void UpdateRevisionId(Entity existingBidSheet)
        {
            Entity entity = service.Retrieve("ig1_bidsheet", existingBidSheet.Id, new ColumnSet("ig1_revisionid", "ig1_status", "ig1_opportunitytitle", "ig1_associated"));
            try
            {
                var opportunityTitle = entity.GetAttributeValue<EntityReference>("ig1_opportunitytitle");
                var opportunityId = opportunityTitle.Id;

                //Changes made to get max revision id with same upper revision id.
                //var maxRevisionId = GetMaxRevisionId(opportunityId);
                var maxRevisionId = GetMaxRevisionId(opportunityId, existingBidSheet);

                //var revisionID = entity.GetAttributeValue<int>("ig1_revisionid");
                var status = entity.GetAttributeValue<OptionSetValue>("ig1_status");
                if (!maxRevisionId.Equals(null) && !maxRevisionId.Equals(""))
                {
                    maxRevisionId++;
                    entity["ig1_revisionid"] = maxRevisionId;
                }
                entity["ig1_status"] = new OptionSetValue(Convert.ToInt32(286150001));
                entity["ig1_associated"] = false;
                serviceAdmin.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //public int GetMaxRevisionId(Guid OpportunityId)
        public int GetMaxRevisionId(Guid OpportunityId, Entity existingBidSheet)
        {
            try
            {
                var upperRevisionId = GetUpperRevisionId(existingBidSheet);
                var maxRevisionId = 0;
                var fetchData = new
                {
                    ig1_opportunitytitle = OpportunityId,
                    statecode = "0",
                    ig1_upperrevisionid = upperRevisionId
                };
                var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_revisionid' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*0cd068b0-a1cf-e911-a960-000d3a1d52e7*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                   
                                  <condition attribute='ig1_upperrevisionid' operator='eq' value='{fetchData.ig1_upperrevisionid/*0*/}'/>
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
        //Getting upper revision id to increase revision id for same version
        protected int GetUpperRevisionId(Entity existingBidSheet)
        {
            int upperRevisionId = 0;
            Entity entity = service.Retrieve(existingBidSheet.LogicalName, existingBidSheet.Id, new ColumnSet("ig1_upperrevisionid"));
            if (entity.Attributes.Contains("ig1_upperrevisionid"))
            {
                upperRevisionId = (int)entity["ig1_upperrevisionid"];
            }
            
            return upperRevisionId;
        }
    }
}