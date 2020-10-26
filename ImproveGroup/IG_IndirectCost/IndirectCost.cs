using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace IG_IndirectCost
{
    public class IndirectCost : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracingService;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.MessageName.ToLower() == "delete" && context.PreEntityImages.Contains("Image"))
                {
                    Guid bidsheetid = Guid.Empty;
                    Guid categoryid = Guid.Empty;
                    Entity entity = (Entity)context.PreEntityImages["Image"];
                    if (entity.Attributes.Contains("ig1_bidsheet") && entity.Attributes["ig1_bidsheet"] != null)
                    {
                        EntityReference entityReference = (EntityReference)entity.Attributes["ig1_bidsheet"];
                        bidsheetid = entityReference.Id;
                    }
                    if (entity.Attributes.Contains("ig1_category") && entity.Attributes["ig1_category"] != null)
                    {
                        EntityReference entityReference = (EntityReference)entity.Attributes["ig1_category"];
                        categoryid = entityReference.Id;
                    }
                    if (bidsheetid != Guid.Empty && categoryid != Guid.Empty)
                    {
                        DeleteIndirectCost(bidsheetid);
                        CreateUpdateAssociatedCost(bidsheetid, categoryid);
                        IndirectCostTotals(bidsheetid);
                    }
                }
                else if (context.MessageName == "ig1_ReCalculateIndirectCost" && context.InputParameters != null && context.InputParameters.Contains("bidsheetid") && !string.IsNullOrEmpty(context.InputParameters["bidsheetid"].ToString()))
                {
                    Guid bidsheetid = new Guid(context.InputParameters["bidsheetid"].ToString());
                    DeleteIndirectCost(bidsheetid);
                    CreateUpdateAssociatedCostSync(bidsheetid);
                    IndirectCostTotals(bidsheetid);
                }
                else if (context.InputParameters.Contains("Target") && (context.InputParameters["Target"] is Entity))
                {
                    Guid bidsheetid = Guid.Empty;
                    Guid categoryid = Guid.Empty;
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "ig1_bidsheetpricelistitem")
                    {
                        Entity bsLineItems = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_bidsheet", "ig1_category"));
                        if (bsLineItems.Attributes.Contains("ig1_bidsheet") && bsLineItems.Attributes["ig1_bidsheet"] != null)
                        {
                            EntityReference entityReference = (EntityReference)bsLineItems.Attributes["ig1_bidsheet"];
                            bidsheetid = entityReference.Id;
                        }
                        if (bsLineItems.Attributes.Contains("ig1_category") && bsLineItems.Attributes["ig1_category"] != null)
                        {
                            EntityReference entityReference = (EntityReference)bsLineItems.Attributes["ig1_category"];
                            categoryid = entityReference.Id;
                        }
                        if (bidsheetid != Guid.Empty && categoryid != Guid.Empty)
                        {
                            DeleteIndirectCost(bidsheetid);
                            CreateUpdateAssociatedCost(bidsheetid, categoryid);
                            IndirectCostTotals(bidsheetid);
                        }
                    }
                    else if (entity.LogicalName == "ig1_associatedcost")
                    {
                        Entity associatedCost = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_bidsheet", "ig1_bidsheetcategory"));
                        if (associatedCost.Attributes.Contains("ig1_bidsheet") && associatedCost.Attributes["ig1_bidsheet"] != null)
                        {
                            EntityReference entityReference = (EntityReference)associatedCost.Attributes["ig1_bidsheet"];
                            bidsheetid = entityReference.Id;
                        }
                        if (associatedCost.Attributes.Contains("ig1_bidsheetcategory") && associatedCost.Attributes["ig1_bidsheetcategory"] != null)
                        {
                            EntityReference entityReference = (EntityReference)associatedCost.Attributes["ig1_bidsheetcategory"];
                            categoryid = entityReference.Id;
                        }
                        if (bidsheetid != Guid.Empty && categoryid != Guid.Empty)
                        {
                            UpdateAssociatedCost(entity.Id, bidsheetid, categoryid);
                            IndirectCostTotals(bidsheetid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        protected void CreateUpdateAssociatedCost(Guid bidsheetid, Guid categoryid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_category = categoryid,
                ig1_categoryname = "Labor",
                ig1_categoryname2 = "Contingency"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_category' />
                                <attribute name='ig1_associatedcostid'/>
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_category' operator='eq' value='{fetchData.ig1_category/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname2/*Contingency*/}'/>
                                </filter>
                                <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category'>
                                  <attribute name='ig1_defaultmatcostmargin' alias='defaultMargin'/>
                                  <attribute name='ig1_name' />
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var lineitem in entityCollection.Entities)
                {
                    decimal margin = Convert.ToDecimal(0);
                    AttributeCollection result = lineitem.Attributes;
                    if (result.Contains("defaultMargin") && result["defaultMargin"] != null)
                    {
                        var defaultMargin = (AliasedValue)result["defaultMargin"];
                        margin = Convert.ToDecimal(defaultMargin.Value);
                    }
                    if (result.Contains("ig1_category") && result["ig1_category"] != null)
                    {
                        var category = (EntityReference)result["ig1_category"];
                        Guid associatedcostid = GetAssociatedCost(bidsheetid, category.Id);
                        if (associatedcostid == Guid.Empty)
                        {
                            CreateAssociatedCost(bidsheetid, category.Id, lineitem, margin);
                        }
                        else
                        {
                            UpdateAssociatedCost(associatedcostid, bidsheetid, category.Id);
                        }
                    }
                }
            }
        }

        protected void CreateAssociatedCost(Guid bidsheetid, Guid categoryid, Entity lineitem, decimal margin)
        {
            decimal[] arr = BidSheetLineItems(bidsheetid, categoryid);
            decimal materialCost = Math.Round(arr[0], 2);
            decimal luExtend = arr[1];
            decimal freighttotal = Math.Round(arr[2], 2);
            decimal freightsell = Math.Round(arr[3], 2);
            decimal designFactor = Convert.ToDecimal(0);
            decimal salesFactor = Convert.ToDecimal(0);
            decimal salesLaborRate = Convert.ToDecimal(0);
            decimal designLaborRate = Convert.ToDecimal(0);
            decimal laborRate = Convert.ToDecimal(0);
            decimal lodging = Convert.ToDecimal(0);
            decimal perDiem = Convert.ToDecimal(0);
            decimal salesCost = Convert.ToDecimal(0);
            decimal designCost = Convert.ToDecimal(0);
            decimal travelCost = Math.Round(Convert.ToDecimal(0), 2);
            decimal baseSales = Convert.ToDecimal(0);
            decimal baseDesign = Convert.ToDecimal(0);
            decimal baseLabor = Convert.ToDecimal(0);
            decimal laborMargin = Convert.ToDecimal(0);
            decimal laborCost = Convert.ToDecimal(0);
            decimal designMargin = Convert.ToDecimal(0);
            decimal salesMargin = Convert.ToDecimal(0);
            decimal baseTravel = Convert.ToDecimal(0);
            decimal sellPrice = Convert.ToDecimal(0);
            decimal totalMaterialCost = Convert.ToDecimal(0);

            Entity defaults = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_defaultdesignfactor", "ig1_defaultdesignlaborrate", "ig1_defaultdesignmargin", "ig1_defaultsalesfactor", "ig1_defaultsaleslaborrate", "ig1_defaultsalesmargin", "ig1_defaultlaborrate", "ig1_defaultlabormargin", "ig1_defaulttravelmargin", "ig1_defaultlodging", "ig1_defaultperdiem", "ig1_defaultmargin", "ig1_defaultcorpgna"));

            Entity entity = new Entity("ig1_associatedcost");
            if (bidsheetid != Guid.Empty)
            {
                entity.Attributes["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", bidsheetid);
            }
            if (categoryid != Guid.Empty)
            {
                entity.Attributes["ig1_bidsheetcategory"] = new EntityReference("ig1_bidsheetcategory", categoryid);
            }

            entity.Attributes["ig1_luextend"] = luExtend;

            if (defaults.Attributes.Contains("ig1_defaultlaborrate") && defaults.Attributes["ig1_defaultlaborrate"] != null)
            {
                Money money = (Money)defaults.Attributes["ig1_defaultlaborrate"];
                laborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultlodging") && defaults.Attributes["ig1_defaultlodging"] != null)
            {
                lodging = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultlodging"]), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultperdiem") && defaults.Attributes["ig1_defaultperdiem"] != null)
            {
                Money money = (Money)defaults.Attributes["ig1_defaultperdiem"];
                perDiem = Math.Round(Convert.ToDecimal(money.Value), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultlabormargin") && defaults.Attributes["ig1_defaultlabormargin"] != null)
            {
                laborMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultlabormargin"]), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultdesignfactor") && defaults.Attributes["ig1_defaultdesignfactor"] != null)
            {
                designFactor = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultdesignfactor"]), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultdesignlaborrate") && defaults.Attributes["ig1_defaultdesignlaborrate"] != null)
            {
                Money money = (Money)defaults.Attributes["ig1_defaultdesignlaborrate"];
                designLaborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultdesignmargin") && defaults.Attributes["ig1_defaultdesignmargin"] != null)
            {
                designMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultdesignmargin"]), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultsalesfactor") && defaults.Attributes["ig1_defaultsalesfactor"] != null)
            {
                salesFactor = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultsalesfactor"]), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultsaleslaborrate") && defaults.Attributes["ig1_defaultsaleslaborrate"] != null)
            {
                Money money = (Money)defaults.Attributes["ig1_defaultsaleslaborrate"];
                salesLaborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
            }
            if (defaults.Attributes.Contains("ig1_defaultsalesmargin") && defaults.Attributes["ig1_defaultsalesmargin"] != null)
            {
                salesMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultsalesmargin"]), 2);
            }
            entity.Attributes["ig1_laborrate"] = laborRate;
            entity.Attributes["ig1_lodgingrate"] = lodging;

            entity.Attributes["ig1_perdiem"] = new Money(perDiem);
            entity.Attributes["ig1_lodgingtotal"] = Convert.ToDecimal(0);

            entity.Attributes["ig1_margin"] = margin;
            entity.Attributes["ig1_freight"] = new Money(freighttotal);
            entity.Attributes["ig1_freightsell"] = new Money(freightsell);
            entity.Attributes["ig1_totalmaterialcost"] = new Money(materialCost);

            laborCost = Math.Round((laborRate * luExtend), 2);
            entity.Attributes["ig1_pmlaborsme"] = laborCost;
            entity.Attributes["ig1_baselabor"] = new Money(laborCost * laborMargin);

            entity.Attributes["ig1_designfactor"] = designFactor;
            entity.Attributes["ig1_designlaborrate"] = new Money(designLaborRate);
            designCost = Math.Round((designFactor * luExtend * designLaborRate), 2);
            baseDesign = Math.Round(designCost * designMargin);
            entity.Attributes["ig1_designcost"] = new Money(designCost);
            entity.Attributes["ig1_basedesign"] = new Money(baseDesign);

            entity.Attributes["ig1_salesfactor"] = salesFactor;
            entity.Attributes["ig1_saleslaborrate"] = new Money(salesLaborRate);
            salesCost = Math.Round((salesFactor * luExtend * salesLaborRate), 2);
            baseSales = Math.Round((salesCost * salesMargin), 2);
            entity.Attributes["ig1_salescost"] = new Money(salesCost);
            entity.Attributes["ig1_basesales"] = new Money(baseSales);

            entity.Attributes["ig1_airfaretrans"] = new Money(0);
            entity.Attributes["ig1_days"] = Convert.ToDecimal(0);
            entity.Attributes["ig1_travelcost"] = new Money(0);

            entity.Attributes["ig1_materialcost"] = new Money(materialCost);
            entity.Attributes["ig1_totaldirectcost"] = Convert.ToDecimal(freighttotal + materialCost);
            entity.Attributes["ig1_baseindirect"] = new Money(baseSales + baseDesign + baseTravel + baseLabor);

            if (margin >= 0 && margin < 100)
            {
                totalMaterialCost = Math.Round((materialCost / (1 - margin / 100)), 2);
                sellPrice = salesCost + designCost + travelCost + freightsell + totalMaterialCost + laborCost;
            }
            else
            {
                totalMaterialCost = materialCost;
                sellPrice = salesCost + designCost + travelCost + freighttotal + totalMaterialCost + laborCost;
            }
            entity.Attributes["ig1_totalcost"] = new Money(materialCost + baseSales + baseDesign + baseTravel + freighttotal + baseLabor);
            entity.Attributes["ig1_totalsellprice"] = new Money(sellPrice);
            entity.Attributes["ig1_totaldirectsell"] = new Money(freightsell + totalMaterialCost);

            Guid indirectCostId = service.Create(entity);

            if (indirectCostId != Guid.Empty)
            {
                lineitem.Attributes["ig1_associatedcostid"] = new EntityReference("ig1_associatedcost", indirectCostId);
                service.Update(lineitem);
            }
        }
        protected void UpdateAssociatedCost(Guid associatedcostid, Guid bidsheetid, Guid categoryid)
        {
            var fetchData = new
            {
                ig1_associatedcostid = associatedcostid,
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_associatedcostid' />
                                <attribute name='ig1_bidsheetcategory' />
                                <attribute name='ig1_bidsheet' />
                                <attribute name='ig1_designfactor' />
                                <attribute name='ig1_designlaborrate' />
                                <attribute name='ig1_salesfactor' />
                                <attribute name='ig1_saleslaborrate' />
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_totalmaterialcost' />
                                <attribute name='ig1_totalsellprice' />
                                <attribute name='ig1_margin' />
                                <attribute name='ig1_travelcost' />
                                <attribute name='ig1_lodgingrate' />
                                <attribute name='ig1_numberoftrip' />
                                <attribute name='ig1_peoplepertrip' />
                                <attribute name='ig1_perdiem' />
                                <attribute name='ig1_airfaretrans' />
                                <attribute name='ig1_days' />
                                <attribute name='ig1_laborrate' />
                                <attribute name='ig1_pmlaborsme' />
                                <filter type='and'>
                                  <condition attribute='ig1_associatedcostid' operator='eq' value='{fetchData.ig1_associatedcostid/*ig1_bidsheet*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity associatedCost = entityCollection.Entities[0];
                var result = associatedCost.Attributes;

                decimal[] arr = BidSheetLineItems(bidsheetid, categoryid);
                decimal materialCost = Math.Round(arr[0], 2);
                decimal luExtend = arr[1];
                decimal freighttotal = Math.Round(arr[2], 2);
                decimal freightsell = Math.Round(arr[3], 2);
                decimal designFactor = Convert.ToDecimal(0);
                decimal salesFactor = Convert.ToDecimal(0);
                decimal salesLaborRate = Convert.ToDecimal(0);
                decimal designLaborRate = Convert.ToDecimal(0);
                decimal laborRate = Convert.ToDecimal(0);
                decimal margin = Convert.ToDecimal(0);
                decimal designMargin = Convert.ToDecimal(0);
                decimal salesMargin = Convert.ToDecimal(0);
                decimal laborMargin = Convert.ToDecimal(0);
                decimal number_of_trip = Convert.ToDecimal(0);
                int people_per_trip = Convert.ToInt32(0);
                decimal days_per_trip = Convert.ToDecimal(0);
                decimal perDiam = Convert.ToDecimal(0);
                decimal lodgingRate = Convert.ToDecimal(0);
                decimal airFare = Convert.ToDecimal(0);
                decimal designCost = Convert.ToDecimal(0);
                decimal salesCost = Convert.ToDecimal(0);
                decimal travelCost = Convert.ToDecimal(0);
                decimal laborCost = Convert.ToDecimal(0);
                decimal baseSales = Convert.ToDecimal(0);
                decimal baseDesign = Convert.ToDecimal(0);
                decimal baseLabor = Convert.ToDecimal(0);
                decimal salesHours = Convert.ToDecimal(0);
                decimal designHours = Convert.ToDecimal(0);
                decimal totalMaterialCost = Convert.ToDecimal(0);

                Entity defaults = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_defaultdesignfactor", "ig1_defaultdesignlaborrate", "ig1_defaultdesignmargin", "ig1_defaultsalesfactor", "ig1_defaultsaleslaborrate", "ig1_defaultsalesmargin", "ig1_defaultlaborrate", "ig1_defaultlabormargin", "ig1_defaulttravelmargin", "ig1_defaultlodging", "ig1_defaultperdiem", "ig1_defaultmargin", "ig1_defaultcorpgna"));
                if (result.Contains("ig1_designfactor") && result["ig1_designfactor"] != null)
                {
                    designFactor = Math.Round(Convert.ToDecimal(result["ig1_designfactor"]), 2);
                }
                if (result.Contains("ig1_designlaborrate") && result["ig1_designlaborrate"] != null)
                {
                    Money money = (Money)result["ig1_designlaborrate"];
                    designLaborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
                }
                if (result.Contains("ig1_salesfactor") && result["ig1_salesfactor"] != null)
                {
                    salesFactor = Math.Round(Convert.ToDecimal(result["ig1_salesfactor"]), 2);
                }
                if (result.Contains("ig1_saleslaborrate") && result["ig1_saleslaborrate"] != null)
                {
                    Money money = (Money)result["ig1_saleslaborrate"];
                    salesLaborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
                }
                if (result.Contains("ig1_laborrate") && result["ig1_laborrate"] != null)
                {
                    laborRate = Math.Round(Convert.ToDecimal(result["ig1_laborrate"]), 2);
                }
                if (result.Contains("ig1_margin") && result["ig1_margin"] != null)
                {
                    margin = Math.Round(Convert.ToDecimal(result["ig1_margin"]), 2);
                }
                if (defaults.Attributes.Contains("ig1_defaultlabormargin") && defaults["ig1_defaultlabormargin"] != null)
                {
                    laborMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultlabormargin"]), 2);
                }
                if (defaults.Attributes.Contains("ig1_defaultdesignmargin") && defaults.Attributes["ig1_defaultdesignmargin"] != null)
                {
                    designMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultdesignmargin"]), 2);
                }
                if (defaults.Attributes.Contains("ig1_defaultsalesmargin") && defaults.Attributes["ig1_defaultsalesmargin"] != null)
                {
                    salesMargin = Math.Round(Convert.ToDecimal(defaults.Attributes["ig1_defaultsalesmargin"]), 2);
                }
                if (result.Contains("ig1_numberoftrip") && result["ig1_numberoftrip"] != null)
                {
                    number_of_trip = Math.Round(Convert.ToDecimal(result["ig1_numberoftrip"]), 2);
                }
                if (result.Contains("ig1_peoplepertrip") && result["ig1_peoplepertrip"] != null)
                {
                    people_per_trip = Convert.ToInt32(result["ig1_peoplepertrip"]);
                }
                if (result.Contains("ig1_days") && result["ig1_days"] != null)
                {
                    days_per_trip = Math.Round(Convert.ToDecimal(result["ig1_days"]), 2);
                }
                if (result.Contains("ig1_perdiem") && result["ig1_perdiem"] != null)
                {
                    Money money = (Money)result["ig1_perdiem"];
                    perDiam = Math.Round(Convert.ToDecimal(money.Value), 2);
                }
                if (result.Contains("ig1_lodgingrate") && result["ig1_lodgingrate"] != null)
                {
                    lodgingRate = Math.Round(Convert.ToDecimal(result["ig1_lodgingrate"]), 2);
                }
                if (result.Contains("ig1_airfaretrans") && result["ig1_airfaretrans"] != null)
                {
                    Money money = (Money)result["ig1_airfaretrans"];
                    airFare = Math.Round(Convert.ToDecimal(money.Value), 2);
                }

                associatedCost.Attributes["ig1_materialcost"] = new Money(materialCost);
                associatedCost.Attributes["ig1_luextend"] = luExtend;
                associatedCost.Attributes["ig1_freight"] = new Money(freighttotal);
                associatedCost.Attributes["ig1_freightsell"] = new Money(freightsell);

                designCost = Math.Round((designFactor * luExtend * designLaborRate), 2);
                baseDesign = designCost * designMargin;
                associatedCost.Attributes["ig1_designcost"] = new Money(designCost);
                associatedCost.Attributes["ig1_basedesign"] = new Money(baseDesign);

                salesCost = Math.Round((salesFactor * luExtend * salesLaborRate), 2);
                baseSales = salesCost * salesMargin;
                associatedCost.Attributes["ig1_salescost"] = new Money(salesCost);
                associatedCost.Attributes["ig1_basesales"] = new Money(baseSales);

                designHours = designFactor * luExtend;
                salesHours = salesFactor * luExtend;
                associatedCost.Attributes["ig1_designhours"] = designHours;
                associatedCost.Attributes["ig1_saleshours"] = salesHours;

                laborCost = Math.Round((laborRate * luExtend), 2);
                baseLabor = laborCost * laborMargin;
                associatedCost.Attributes["ig1_pmlaborsme"] = laborCost;
                associatedCost.Attributes["ig1_baselabor"] = new Money(baseLabor);

                travelCost = (number_of_trip * people_per_trip * airFare) + (number_of_trip * people_per_trip * days_per_trip) * (perDiam + lodgingRate);
                associatedCost.Attributes["ig1_travelcost"] = new Money(travelCost);
                associatedCost.Attributes["ig1_lodgingtotal"] = number_of_trip * people_per_trip * days_per_trip * lodgingRate;
                associatedCost.Attributes["ig1_perdiemtotal"] = new Money(number_of_trip * people_per_trip * days_per_trip * perDiam);
                associatedCost.Attributes["ig1_transporttotal"] = new Money(number_of_trip * people_per_trip * airFare);

                associatedCost.Attributes["ig1_totaldirectcost"] = materialCost + freighttotal;
                associatedCost.Attributes["ig1_totalindirectcost"] = designCost + laborCost + salesCost + travelCost;
                associatedCost.Attributes["ig1_baseindirect"] = baseSales + baseDesign + travelCost + baseLabor;

                if (margin > 0 && margin < 100)
                {

                    totalMaterialCost = Math.Round((materialCost / (1 - margin / 100)), 2);
                    associatedCost.Attributes["ig1_totalmaterialcost"] = new Money(totalMaterialCost);
                    associatedCost.Attributes["ig1_totalsellprice"] = salesCost + designCost + travelCost + laborCost + totalMaterialCost + freightsell;
                }
                else
                {
                    totalMaterialCost = materialCost;
                    associatedCost.Attributes["ig1_totalmaterialcost"] = totalMaterialCost; //If no margin then total material cost is same as material cost
                    associatedCost.Attributes["ig1_totalsellprice"] = salesCost + designCost + travelCost + laborCost + totalMaterialCost + freightsell;
                }

                associatedCost.Attributes["ig1_totalcost"] = new Money(salesCost + designCost + travelCost + laborCost + materialCost + freighttotal);
                associatedCost.Attributes["ig1_totalprojecthours"] = salesHours + designHours + luExtend;
                associatedCost.Attributes["ig1_totaldirectsell"] = new Money(freightsell + totalMaterialCost);
                service.Update(associatedCost);
            }

        }
        protected Guid GetAssociatedCost(Guid bidsheetid, Guid categoryid)
        {
            Guid associatedCostId = Guid.Empty;
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_bidsheetcategory = categoryid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_associatedcostid' />
                                <attribute name='ig1_bidsheetcategory' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*b2170d6f-f935-4d40-96e6-24e9bab814ca*/}'/>
                                  <condition attribute='ig1_bidsheetcategory' operator='eq' value='{fetchData.ig1_bidsheetcategory/*75c6d32a-22cf-e911-a969-000d3a1d578c*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                return entityCollection.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }
        protected decimal[] BidSheetLineItems(Guid bidsheetid, Guid categoryid)
        {
            decimal materialCost = Convert.ToDecimal(0);
            decimal luExtend = Convert.ToDecimal(0);
            decimal freighttotal = Convert.ToDecimal(0);
            decimal freightsell = Convert.ToDecimal(0);

            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_categoryid = categoryid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_freightamount' />
                                <attribute name='ig1_markup' />
                                <attribute name='ig1_projectlu' />
                                <attribute name='ig1_luextend' />
                                <attribute name='ig1_projecthours' />
                                <attribute name='ig1_projectcost' />
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_freighttotal' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_category' operator='eq' value='{fetchData.ig1_categoryid/*Labor*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (entityCollection.Entities.Count > 0)
            {
                foreach (var lineItem in entityCollection.Entities)
                {
                    var result = lineItem.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_luextend") && result["ig1_luextend"] != null)
                    {
                        luExtend += Convert.ToDecimal(result["ig1_luextend"]);
                    }
                    if (result.Contains("ig1_freightamount") && result["ig1_freightamount"] != null)
                    {
                        Money money = (Money)result["ig1_freightamount"];
                        freighttotal += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_freighttotal") && result["ig1_freighttotal"] != null)
                    {
                        Money money = (Money)result["ig1_freighttotal"];
                        freightsell += Convert.ToDecimal(money.Value);
                    }
                }
            }

            decimal[] arr = new decimal[4];
            arr[0] = materialCost;
            arr[1] = luExtend;
            arr[2] = freighttotal;
            arr[3] = freightsell;

            return arr;
        }
        protected void IndirectCostTotals(Guid bidsheetid)
        {
            decimal directPrice = Convert.ToDecimal(0);
            decimal indirectPrice = Convert.ToDecimal(0);
            decimal materialCost = Convert.ToDecimal(0);
            decimal freightCost = Convert.ToDecimal(0);
            decimal laborHours = Convert.ToDecimal(0);
            decimal laborPrice = Convert.ToDecimal(0);
            decimal designHours = Convert.ToDecimal(0);
            decimal designPrice = Convert.ToDecimal(0);
            decimal salesHours = Convert.ToDecimal(0);
            decimal salesPrice = Convert.ToDecimal(0);
            decimal lodgingTotal = Convert.ToDecimal(0);
            decimal transTotal = Convert.ToDecimal(0);
            decimal perDiamTotal = Convert.ToDecimal(0);
            decimal totalTravel = Convert.ToDecimal(0);
            decimal sellPrice = Convert.ToDecimal(0);
            decimal totalSellPrice = Convert.ToDecimal(0);
            decimal anticipatedGP = Convert.ToDecimal(0);
            decimal anticipatedGPPercent = Convert.ToDecimal(0);
            decimal anticipatedNet = Convert.ToDecimal(0);
            decimal anticipatedNetPercent = Convert.ToDecimal(0);
            decimal anticipatedNetPreCommissionper = Convert.ToDecimal(0);
            decimal anticipatedCommissionableValue = Convert.ToDecimal(0);
            decimal net_netamt = Convert.ToDecimal(0);
            decimal net_netper = Convert.ToDecimal(0);
            decimal corpGNA = Convert.ToDecimal(0);


            var fetchData = new
            {
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_luextend' />
                                <attribute name='ig1_saleshours' />
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_freight' />
                                <attribute name='ig1_totalindirectcost' />
                                <attribute name='ig1_totaldirectsell' />
                                <attribute name='ig1_designhours' />
                                <attribute name='ig1_totaldirectcost' />
                                <attribute name='ig1_perdiemtotal' />
                                <attribute name='ig1_travelcost' />
                                <attribute name='ig1_pmlaborsme' />
                                <attribute name='ig1_transporttotal' />
                                <attribute name='ig1_perdiemtotal' />
                                <attribute name='ig1_designcost' />
                                <attribute name='ig1_lodgingtotal' />
                                <attribute name='ig1_salescost' />
                                <attribute name='ig1_totalsellprice' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    var result = item.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_freight") && result["ig1_freight"] != null)
                    {
                        Money money = (Money)result["ig1_freight"];
                        freightCost += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_totaldirectcost") && result["ig1_totaldirectcost"] != null)
                    {
                        directPrice += Convert.ToDecimal(result["ig1_totaldirectcost"]);
                    }
                    if (result.Contains("ig1_totalindirectcost") && result["ig1_totalindirectcost"] != null)
                    {
                        indirectPrice += Convert.ToDecimal(result["ig1_totalindirectcost"]);
                    }
                    if (result.Contains("ig1_saleshours") && result["ig1_saleshours"] != null)
                    {
                        salesHours += Convert.ToDecimal(result["ig1_saleshours"]);
                    }
                    if (result.Contains("ig1_designhours") && result["ig1_designhours"] != null)
                    {
                        designHours += Convert.ToDecimal(result["ig1_designhours"]);
                    }
                    if (result.Contains("ig1_luextend") && result["ig1_luextend"] != null)
                    {
                        laborHours += Convert.ToDecimal(result["ig1_luextend"]);
                    }
                    if (result.Contains("ig1_pmlaborsme") && result["ig1_pmlaborsme"] != null)
                    {
                        laborPrice += Convert.ToDecimal(result["ig1_pmlaborsme"]);
                    }
                    if (result.Contains("ig1_salescost") && result["ig1_salescost"] != null)
                    {
                        Money money = (Money)result["ig1_salescost"];
                        salesPrice += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_designcost") && result["ig1_designcost"] != null)
                    {
                        Money money = (Money)result["ig1_designcost"];
                        designPrice += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_lodgingtotal") && result["ig1_lodgingtotal"] != null)
                    {
                        lodgingTotal += Convert.ToDecimal(result["ig1_lodgingtotal"]);
                    }
                    if (result.Contains("ig1_transporttotal") && result["ig1_transporttotal"] != null)
                    {
                        Money money = (Money)result["ig1_transporttotal"];
                        transTotal += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_perdiemtotal") && result["ig1_perdiemtotal"] != null)
                    {
                        Money money = (Money)result["ig1_perdiemtotal"];
                        perDiamTotal += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_travelcost") && result["ig1_travelcost"] != null)
                    {
                        Money money = (Money)result["ig1_travelcost"];
                        totalTravel += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_totalsellprice") && result["ig1_totalsellprice"] != null)
                    {
                        Money money = (Money)result["ig1_totalsellprice"];
                        sellPrice += Convert.ToDecimal(money.Value);
                    }
                }

                Entity bidsheet = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_name", "ig1_defaultcorpgna"));
                if (bidsheet.Attributes.Contains("ig1_defaultcorpgna") && bidsheet.Attributes["ig1_defaultcorpgna"] != null)
                {
                    corpGNA = Math.Round(Convert.ToDecimal(bidsheet.Attributes["ig1_defaultcorpgna"]), 2);
                }
                bidsheet.Attributes["ig1_directcost"] = Math.Round(directPrice, 2);
                bidsheet.Attributes["ig1_indirectcost"] = Math.Round(indirectPrice, 2);

                bidsheet.Attributes["ig1_totalhours"] = Math.Round((salesHours + designHours + laborHours), 2);
                bidsheet.Attributes["ig1_totalcost"] = new Money(Math.Round((directPrice + indirectPrice), 2));

                bidsheet.Attributes["ig1_materialcost"] = Math.Round(materialCost, 2);
                bidsheet.Attributes["ig1_freightamount"] = new Money(Math.Round(freightCost, 2));
                bidsheet.Attributes["ig1_freightcost"] = Math.Round(freightCost, 2);

                bidsheet.Attributes["ig1_pmhours"] = laborHours;
                bidsheet.Attributes["ig1_pmcost"] = Math.Round(laborPrice, 2);

                bidsheet.Attributes["ig1_designhours"] = Math.Round(designHours, 2);
                bidsheet.Attributes["ig1_designcost"] = new Money(Math.Round(designPrice, 2));

                bidsheet.Attributes["ig1_saleshours"] = Math.Round(salesHours, 2);
                bidsheet.Attributes["ig1_salescost"] = new Money(Math.Round(salesPrice, 2));

                bidsheet.Attributes["ig1_lodgingtotal"] = Math.Round(lodgingTotal, 2);
                bidsheet.Attributes["ig1_transtotal"] = Math.Round(transTotal, 2);
                bidsheet.Attributes["ig1_perdiem"] = new Money(Math.Round(perDiamTotal, 2));
                bidsheet.Attributes["ig1_totaltravel"] = new Money(Math.Round(totalTravel));

                decimal contingencyCost = GetContingency(bidsheetid);
                totalSellPrice = Math.Round((sellPrice + contingencyCost), 2);
                bidsheet.Attributes["ig1_totalbomcost"] = Math.Round((materialCost + freightCost), 2);
                bidsheet.Attributes["ig1_sellprice"] = new Money(Math.Round(totalSellPrice, 2));

                //Project Finance Projection
                anticipatedGP = totalSellPrice - directPrice - Convert.ToDecimal(0.7) * (designPrice + salesPrice + laborPrice) - totalTravel;
                if (totalSellPrice > 0)
                {
                    anticipatedGPPercent = (anticipatedGP * 100) / totalSellPrice;
                }
                anticipatedNetPreCommissionper = anticipatedGPPercent - corpGNA;
                anticipatedNet = (anticipatedNetPreCommissionper / 100) * totalSellPrice;
                anticipatedCommissionableValue = anticipatedNet * Convert.ToDecimal(0.3);
                net_netamt = anticipatedNet - anticipatedCommissionableValue;
                if (totalSellPrice != 0)
                {
                    net_netper = (net_netamt * 100) / totalSellPrice;
                }
                else
                {
                    net_netper = 0;
                }
                bidsheet.Attributes["ig1_anticipatedgp"] = new Money(anticipatedGP);
                bidsheet.Attributes["ig1_anticipatedgpinpercent"] = anticipatedGPPercent;
                bidsheet.Attributes["ig1_anticipatednetinpercent"] = anticipatedNetPreCommissionper;
                bidsheet.Attributes["ig1_anticipatednet"] = new Money(anticipatedNet);
                bidsheet.Attributes["ig1_anticipatedcommissionablevalue"] = new Money(anticipatedCommissionableValue);
                bidsheet.Attributes["ig1_netnet"] = new Money(net_netamt);
                bidsheet.Attributes["ig1_anticipatedcommissionablevalueinpercent"] = net_netper;
                bidsheet.Attributes["ig1_isbidsheetcalculated"] = true;

                service.Update(bidsheet);
            }

        }
        protected decimal GetContingency(Guid bidsheetid)
        {
            decimal materialCost = Convert.ToDecimal(0);
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_categoryname = "Contingency"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_materialcost' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_categoryname' operator='eq' value='{fetchData.ig1_categoryname/*Contingency*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    var result = item.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost += Convert.ToDecimal(money.Value);
                    }
                }
            }
            return materialCost;
        }
        protected void DeleteIndirectCost(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_associatedcostid' />
                                <attribute name='ig1_bidsheetcategory' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*41b91ca9-c901-49ef-b55f-b35a624a1451*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                foreach (var item in entityCollection.Entities)
                {
                    Guid categoryid = Guid.Empty;
                    if (item.Attributes.Contains("ig1_bidsheetcategory") && item.Attributes["ig1_bidsheetcategory"] != null)
                    {
                        EntityReference entityReference = (EntityReference)item.Attributes["ig1_bidsheetcategory"];
                        categoryid = entityReference.Id;
                        bool isLineItemExist = IsBidsheetLineItemExist(bidsheetid, categoryid);
                        if (!isLineItemExist)
                        {
                            service.Delete(entity.LogicalName, entity.Id);
                        }
                    }
                }

            }
        }
        protected bool IsBidsheetLineItemExist(Guid bidsheetid, Guid categoryid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_category = categoryid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_bidsheetpricelistitemid' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*41b91ca9-c901-49ef-b55f-b35a624a1451*/}'/>
                                  <condition attribute='ig1_category' operator='eq' value='{fetchData.ig1_category/*ig1_bidsheetcategory*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected void CreateUpdateAssociatedCostSync(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_categoryname = "Labor",
                ig1_categoryname2 = "Contingency"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_category' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname2/*Contingency*/}'/>
                                </filter>
                                <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category'>
                                  <attribute name='ig1_defaultmatcostmargin' alias='defaultMargin'/>
                                  <attribute name='ig1_name' />
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var lineitem in entityCollection.Entities)
                {
                    decimal margin = Convert.ToDecimal(0);
                    AttributeCollection result = lineitem.Attributes;
                    if (result.Contains("defaultMargin") && result["defaultMargin"] != null)
                    {
                        var defaultMargin = (AliasedValue)result["defaultMargin"];
                        margin = Convert.ToDecimal(defaultMargin.Value);
                    }
                    if (result.Contains("ig1_category") && result["ig1_category"] != null)
                    {
                        var category = (EntityReference)result["ig1_category"];
                        Guid associatedcostid = GetAssociatedCost(bidsheetid, category.Id);
                        if (associatedcostid == Guid.Empty)
                        {
                            CreateAssociatedCost(bidsheetid, category.Id, lineitem, margin);
                        }
                        else
                        {
                            UpdateAssociatedCost(associatedcostid, bidsheetid, category.Id);
                        }
                    }
                }
            }
        }

    }
}
