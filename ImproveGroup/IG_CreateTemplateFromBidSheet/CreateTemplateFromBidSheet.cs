using System;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_CreateTemplateFromBidSheet
{
    public class CreateTemplateFromBidSheet : IPlugin
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
                service = serviceFactory.CreateOrganizationService(null);
                if (context.InputParameters.Equals(null) || string.IsNullOrEmpty(context.InputParameters["bidSheetId"].ToString()))
                {
                    return;
                }

                string bidSheetId = context.InputParameters["bidSheetId"].ToString();
                bool keepPricing = Convert.ToBoolean(context.InputParameters["keepPricing"]);
                Entity bidSheet = service.Retrieve("ig1_bidsheet", new Guid(bidSheetId), new ColumnSet(true));
                Guid templateId = CreateTemplate(bidSheet, keepPricing);
                if (templateId != Guid.Empty)
                {
                    CreateTemplateCategories(bidSheet.Id, templateId);
                    CreateTemplateProducts(bidSheet.Id, templateId, keepPricing);
                    CreateTemplateLineItems(bidSheet.Id, templateId, keepPricing);

                }
            }
            catch (Exception ex)
            {
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                trace.Trace("Throwing CreateTemplateFromBidSheet Plugin");
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }
        protected Guid CreateTemplate(Entity bidSheet, bool keepPricing)
        {
            string projectNumber = string.Empty;
            string revisionId = string.Empty;
            string bidSheetTitle = string.Empty;

            Entity template = new Entity("ig1_bidsheet");

            foreach (var attribute in bidSheet.Attributes)
            {
                if (attribute.Key == "ig1_bidsheetid" || attribute.Key== "ig1_opportunitytitle")
                {
                    continue;
                }
                else if (attribute.Key == "ig1_name")
                {
                    bidSheetTitle = attribute.Value.ToString();
                }
                else if (attribute.Key == "ig1_projectnumber")
                {
                    projectNumber = attribute.Value.ToString();
                    template[attribute.Key] = attribute.Value;
                }
                else if (attribute.Key == "ig1_upperrevisionid")
                {
                    template[attribute.Key] = Convert.ToInt32(1);
                }
                else if (attribute.Key == "ig1_revisionid")
                {
                    template[attribute.Key] = Convert.ToInt32(0);
                }
                else if (attribute.Key == "ig1_version")
                {
                    revisionId = attribute.Value.ToString();
                    template[attribute.Key] = Convert.ToString("1.0");
                }
                else if (attribute.Key == "ig1_status")
                {
                    template[attribute.Key] = new OptionSetValue(286150003);
                }
                else if (attribute.Key == "ig1_associated")
                {
                    template[attribute.Key] = false;
                }
                else
                {
                    Type fieldType = attribute.Value.GetType();
                    if (fieldType.Name == "Money")
                    {
                        template[attribute.Key] = new Money(0);
                    }
                    else if (fieldType.Name == "Decimal")
                    {
                        template[attribute.Key] = new Decimal(0);
                    }
                    else
                    {
                        template[attribute.Key] = attribute.Value;
                    }
                }
            }
            template["ig1_name"] = Convert.ToString(projectNumber + "-" + bidSheetTitle + "-" + revisionId);
            template["ig1_createdbidsheets"] = Convert.ToInt32(0);
            Guid templateId = service.Create(template);

            return templateId;
        }
        protected void CreateTemplateCategories(Guid bidSheetId, Guid templateId)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bscategoryvendor");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(bidSheetId);

            EntityCollection categories = service.RetrieveMultiple(query);
            foreach (var category in categories.Entities)
            {
                Entity templateCategory = new Entity("ig1_bscategoryvendor");
                foreach (var attribute in category.Attributes)
                {
                    if (attribute.Key == "ig1_bscategoryvendorid")
                    {
                        continue;
                    }
                    else if (attribute.Key== "ig1_bidsheet")
                    {
                        templateCategory["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", templateId);
                    }
                    else
                    {
                        templateCategory[attribute.Key] = attribute.Value;
                    }
                }
                service.Create(templateCategory);
            }
        }
        protected void CreateTemplateProducts(Guid bidSheetId, Guid templateId, bool keepPricing)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bidsheetproduct");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(bidSheetId);
            EntityCollection bidSheetProducts = service.RetrieveMultiple(query);

            foreach (Entity products in bidSheetProducts.Entities)
            {
                Entity templateProducts = new Entity("ig1_bidsheetproduct");
                foreach (var attribute in products.Attributes)
                {
                    if (attribute.Key == "ig1_bidsheetproductid")
                    {
                        continue;
                    }
                    else if (attribute.Key == "ig1_unitprice")
                    {
                        templateProducts[attribute.Key] = Convert.ToDecimal(0);
                    }
                    else if (attribute.Key == "ig1_quantity")
                    {
                        templateProducts[attribute.Key] = Convert.ToInt32(1);
                    }
                    else if (attribute.Key == "ig1_bidsheet")
                    {
                        templateProducts["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", templateId);
                    }
                    else
                    {
                        Type type = attribute.Value.GetType();
                        if (type.Name == "Money")
                        {
                            templateProducts[attribute.Key] = new Money(0);
                        }
                        else
                        {
                            templateProducts[attribute.Key] = attribute.Value;
                        }
                    }
                }
                service.Create(templateProducts);
            }
        }
        protected void CreateTemplateLineItems(Guid bidSheetId, Guid templateId, bool keepPricing)
        {
            QueryByAttribute query = new QueryByAttribute("ig1_bidsheetpricelistitem");
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("ig1_bidsheet");
            query.Values.AddRange(bidSheetId);

            EntityCollection bidSheetLineItems = service.RetrieveMultiple(query);
            foreach (Entity lineItems in bidSheetLineItems.Entities)
            {
                Entity templateLineItems = new Entity("ig1_bidsheetpricelistitem");
                foreach (var attribute in lineItems.Attributes)
                {
                    if (attribute.Key == "ig1_bidsheetpricelistitemid")
                    {
                        continue;
                    }
                    else if (attribute.Key == "ig1_associatedcostid")
                    {
                        EntityReference bsAssociatedCost = (EntityReference)attribute.Value;
                        Guid templateAssociatedCostId = CreateTemplateAssociatedCost(bsAssociatedCost.Id, templateId, keepPricing);
                        if (templateAssociatedCostId != null && templateAssociatedCostId != Guid.Empty)
                        {
                            templateLineItems[attribute.Key] = new EntityReference("ig1_associatedcost", templateAssociatedCostId);
                        }
                    }
                    else if (attribute.Key == "ig1_unitprice")
                    {
                        templateLineItems[attribute.Key] = Convert.ToDecimal(0);
                    }
                    else if (attribute.Key == "ig1_quantity")
                    {
                        templateLineItems[attribute.Key] = Convert.ToInt32(1);
                    }
                    else if (attribute.Key== "ig1_bidsheet")
                    {
                        templateLineItems["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", templateId);
                    }
                    else
                    {
                        Type type = attribute.Value.GetType();
                        if (type.Name == "Money")
                        {
                            templateLineItems[attribute.Key] = new Money(0);
                        }
                        else if (type.Name=="Decimal")
                        {
                            templateLineItems[attribute.Key] = new Decimal(0);
                        }
                        else
                        {
                            templateLineItems[attribute.Key] = attribute.Value;
                        }
                    }
                }
                service.Create(templateLineItems);
            }

        }
        protected Guid CreateTemplateAssociatedCost(Guid associatedCostId, Guid templateId, bool keepPricing)
        {
            Entity bsAssocaitedCost = service.Retrieve("ig1_associatedcost", associatedCostId, new ColumnSet(true));
            Entity templateAssociatedCost = new Entity("ig1_associatedcost");
            foreach (var associatedCost in bsAssocaitedCost.Attributes)
            {
                if (associatedCost.Key == "ig1_associatedcostid")
                {
                    continue;
                }
                else if (associatedCost.Key == "ig1_bidsheet")
                {
                    templateAssociatedCost[associatedCost.Key] = new EntityReference("ig1_bidsheet", templateId);
                }
                else
                {
                    Type type = associatedCost.Value.GetType();
                    if (type.Name == "Money")
                    {
                        templateAssociatedCost[associatedCost.Key] = new Money(0);
                    }
                    else if (type.Name == "Decimal")
                    {
                        templateAssociatedCost[associatedCost.Key] = new Decimal(0);
                    }
                    else
                    {
                        templateAssociatedCost[associatedCost.Key] = associatedCost.Value;
                    }
                }
            }
            Guid templateAssociatedCostId = service.Create(templateAssociatedCost);

            return templateAssociatedCostId;
        }
    }
}
