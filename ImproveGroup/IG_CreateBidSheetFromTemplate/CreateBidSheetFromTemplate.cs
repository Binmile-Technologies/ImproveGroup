using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_CreateBidSheetFromTemplate
{
    public class CreateBidSheetFromTemplate : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.UserId);
                if (context.InputParameters.Equals(null) || string.IsNullOrEmpty(context.InputParameters["templateId"].ToString()) || string.IsNullOrEmpty(context.InputParameters["opportunityId"].ToString()) || string.IsNullOrEmpty(context.InputParameters["topic"].ToString()))
                {
                    return;
                }
                
                string templateId = context.InputParameters["templateId"].ToString();
                string topic = context.InputParameters["topic"].ToString();
                string opportunityId = context.InputParameters["opportunityId"].ToString();
                bool keepPricing = Convert.ToBoolean(context.InputParameters["keepPricing"]);
                bool isChangeOrder = Convert.ToBoolean(context.InputParameters["isChangeOrder"]);
                Entity template = service.Retrieve("ig1_bidsheet", new Guid(templateId), new ColumnSet(true));
                Guid bidSheetId = CreateBidSheet(template, opportunityId, topic, keepPricing, isChangeOrder);
                if (bidSheetId != Guid.Empty)
                {
                    CreateBidSheetCategories(template.Id, bidSheetId);
                    CreateBidSheetProducts(template.Id, bidSheetId, keepPricing);
                    CreateBidSheetLineItems(template.Id, bidSheetId, keepPricing);
                    UpdateTemplate(template);

                }
            }
            catch (Exception ex)
            {
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                trace.Trace("Throwing CreateBidSheetFromTemplate Plugin");
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }
        protected Guid CreateBidSheet(Entity template, string opportunityId, string topic, bool keepPricing, bool isChangeOrder)
        {
            Entity bidSheet = new Entity("ig1_bidsheet");

            foreach (var attribute in template.Attributes)
            {
                if (attribute.Key == "ig1_bidsheetid" || attribute.Key == "ig1_upperrevisionid" || attribute.Key== "ig1_revisionid" || attribute.Key == "ig1_projectnumber" || attribute.Key== "ig1_pricelist")
                {
                    continue;
                }
                else if (attribute.Key == "ig1_status")
                {
                    bidSheet[attribute.Key] = new OptionSetValue(286150001);
                }
                else if (attribute.Key == "ig1_associated")
                {
                    bidSheet[attribute.Key] = false;
                }
                else if (attribute.Key == "ig1_name")
                {
                    bidSheet[attribute.Key] = topic;
                }
                else
                {
                    Type type = attribute.Value.GetType();

                    if (type.Name == "Money" && !keepPricing)
                    {
                        bidSheet[attribute.Key] = new Money(0);
                    }
                    else if (type.Name == "Decimal" && !keepPricing)
                    {
                        bidSheet[attribute.Key] = Convert.ToDecimal(0);
                    }
                    else
                    {
                        bidSheet[attribute.Key] = attribute.Value;
                    }
                }
            }
            if (!string.IsNullOrEmpty(opportunityId))
            {
                bidSheet["ig1_opportunitytitle"] = new EntityReference("opportunity", new Guid(opportunityId));
            }
            bidSheet["ig1_iscreateorder"] = isChangeOrder;
            Guid bidSheetId = service.Create(bidSheet);
            return bidSheetId;
        }
        protected void CreateBidSheetCategories(Guid templateId, Guid bidSheetId)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bscategoryvendor");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(templateId);

            EntityCollection categories = service.RetrieveMultiple(query);
            foreach (var category in categories.Entities)
            {
                Entity bidSheetCategory = new Entity("ig1_bscategoryvendor");
                foreach (var attribute in category.Attributes)
                {
                    if (attribute.Key == "ig1_bscategoryvendorid")
                    {
                        continue;
                    }
                    else if (attribute.Key== "ig1_bidsheet")
                    {
                        bidSheetCategory["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", bidSheetId);
                    }
                    else
                    {
                        bidSheetCategory[attribute.Key] = attribute.Value;
                    }
                }
                service.Create(bidSheetCategory);
            }
        }
        protected void CreateBidSheetProducts(Guid templateId, Guid bidSheetId, bool keepPricing)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bidsheetproduct");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(templateId);
            EntityCollection templateProducts = service.RetrieveMultiple(query);

            foreach (Entity products in templateProducts.Entities)
            {
                Entity bidSheetProducts = new Entity("ig1_bidsheetproduct");
                foreach (var attribute in products.Attributes)
                {
                    if (attribute.Key == "ig1_bidsheetproductid")
                    {
                        continue;
                    }
                    else if (attribute.Key== "ig1_bidsheet")
                    {
                        bidSheetProducts["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", bidSheetId);
                    }
                    else
                    {
                        Type type = attribute.Value.GetType();
                        if (type.Name == "Money" && !keepPricing)
                        {
                            bidSheetProducts[attribute.Key] = new Money(0);
                        }
                        else if (type.Name == "Decimal" && !keepPricing)
                        {
                            bidSheetProducts[attribute.Key] = Convert.ToDecimal(0);
                        }
                        else
                        {
                            bidSheetProducts[attribute.Key] = attribute.Value;
                        }
                    }
                }
                service.Create(bidSheetProducts);
            }
        }
        protected void CreateBidSheetLineItems(Guid templateId, Guid bidSheetId, bool keepPricing)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bidsheetpricelistitem");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(templateId);

            EntityCollection templateLineItems = service.RetrieveMultiple(query);
            foreach (Entity items in templateLineItems.Entities)
            {
                Entity bidSheetLineItems = new Entity("ig1_bidsheetpricelistitem");
                foreach (var attribute in items.Attributes)
                {
                    if (attribute.Key == "ig1_bidsheetpricelistitemid" || attribute.Key == "ig1_associatedcostid")
                    {
                        continue;
                    }
                    else if (attribute.Key== "ig1_bidsheet")
                    {
                        bidSheetLineItems["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", bidSheetId);
                    }
                    /*else if (attribute.Key == "ig1_associatedcostid")
                    {
                        EntityReference bsAssociatedCost = (EntityReference)attribute.Value;
                        Guid bidSheetAssociatedCostId = GetBidsheetAssociatedCost(bsAssociatedCost.Id, bidSheetId);
                        if (bidSheetAssociatedCostId != null && bidSheetAssociatedCostId != Guid.Empty)
                        {
                            bidSheetLineItems[attribute.Key] = new EntityReference("ig1_associatedcost", bidSheetAssociatedCostId);
                        }
                    }*/
                    else
                    {
                        Type type = attribute.Value.GetType();
                        if (type.Name == "Money" && !keepPricing)
                        {
                            bidSheetLineItems[attribute.Key] = new Money(0);
                        }
                        else if (type.Name == "Decimal" && !keepPricing)
                        {
                            bidSheetLineItems[attribute.Key] = Convert.ToDecimal(0);
                        }
                        else
                        {
                            bidSheetLineItems[attribute.Key] = attribute.Value;
                        }
                    }
                }
                service.Create(bidSheetLineItems);
            }

        }
        /*protected Guid GetBidsheetAssociatedCost(Guid associatedCostId, Guid bidSheetId)
        {
            Entity templateAssociatedCost = service.Retrieve("ig1_associatedcost", associatedCostId, new ColumnSet(true));
            Entity bsAssociatedCost = new Entity("ig1_associatedcost");
            foreach (var associatedCost in templateAssociatedCost.Attributes)
            {
                if (associatedCost.Key == "ig1_associatedcostid")
                {
                    continue;
                }
                else if (associatedCost.Key == "ig1_bidsheet")
                {
                    bsAssociatedCost[associatedCost.Key] = new EntityReference("ig1_bidsheet", bidSheetId);
                }
                else
                {
                    bsAssociatedCost[associatedCost.Key] = associatedCost.Value;
                }
            }
            Guid bidSheetAssociatedCostId = service.Create(bsAssociatedCost);

            return bidSheetAssociatedCostId;
        }*/
        protected void UpdateTemplate(Entity template)
        {
            Entity entity = service.Retrieve(template.LogicalName, template.Id, new ColumnSet("ig1_createdbidsheets"));
            if (entity.Attributes.Contains("ig1_createdbidsheets") && entity.Attributes["ig1_createdbidsheets"] != null)
            {
                entity.Attributes["ig1_createdbidsheets"] = Convert.ToInt32(entity.Attributes["ig1_createdbidsheets"]) + Convert.ToInt32(1);
                service.Update(entity);
            }
        }
    }
}
