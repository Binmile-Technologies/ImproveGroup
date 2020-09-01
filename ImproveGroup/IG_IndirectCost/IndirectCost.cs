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
                service = serviceFactory.CreateOrganizationService(null);

                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    return;
                }

                var entity = (Entity)context.InputParameters["Target"];
                Guid bidsheetid = Guid.Empty;
                Guid categoryid = Guid.Empty;

                if (entity.LogicalName == "ig1_bidsheetpricelistitem")
                {
                    Entity bsLineItems = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_bidsheet", "ig1_category"));
                    if (bsLineItems.Attributes.Contains("ig1_bidsheet") && bsLineItems.Attributes["ig1_bidsheet"] != null)
                    {
                        EntityReference entityReference = (EntityReference)bsLineItems.Attributes["ig1_bidsheet"];
                        bidsheetid = entityReference.Id;
                    }
                    if (bsLineItems.Attributes.Contains("ig1_category") && bsLineItems.Attributes["ig1_category"]!=null)
                    {
                        EntityReference entityReference = (EntityReference)bsLineItems.Attributes["ig1_category"];
                        categoryid = entityReference.Id;
                    }
                    if (bidsheetid != Guid.Empty && categoryid != Guid.Empty)
                    {
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
                foreach (var bscategory in entityCollection.Entities)
                {
                    decimal margin = Convert.ToDecimal(0);
                    AttributeCollection result = bscategory.Attributes;
                    if (result.Contains("defaultMargin") && result["defaultMargin"] != null)
                    {
                       var defaultMargin  = (AliasedValue)result["defaultMargin"];
                        margin = Convert.ToDecimal(defaultMargin.Value);
                    }
                    if (result.Contains("ig1_category") && result["ig1_category"] != null)
                    {
                        var category = (EntityReference)result["ig1_category"];
                        Guid associatedcostid = GetAssociatedCost(bidsheetid, category.Id);
                        if (associatedcostid == Guid.Empty)
                        {
                            CreateAssociatedCost(bidsheetid, category.Id, margin);
                        }
                        else
                        {
                            UpdateAssociatedCost(associatedcostid, bidsheetid, category.Id);
                        }
                    }
                }
            }
        }

        protected void CreateAssociatedCost(Guid bidsheetid, Guid categoryid, decimal margin)
        {
            decimal[] arr = BidSheetLineItems(bidsheetid, categoryid);
            decimal materialCost = arr[0];
            decimal luExtend = arr[1];
            decimal freighttotal = arr[2];
            decimal freightsell = arr[3];
            decimal designFactor = Convert.ToDecimal(0);
            decimal salesFactor = Convert.ToDecimal(0);
            decimal salesLaborRate = Convert.ToDecimal(0);
            decimal designLaborRate = Convert.ToDecimal(0);
            decimal laborRate = Convert.ToDecimal(0);
            decimal lodging = Convert.ToDecimal(0);
            decimal perDiem = Convert.ToDecimal(0);
            decimal salesCost = Convert.ToDecimal(0);
            decimal designCost = Convert.ToDecimal(0);
            decimal travelCost = Convert.ToDecimal(0);
            decimal baseSales = Convert.ToDecimal(0);
            decimal baseDesign = Convert.ToDecimal(0);
            decimal baseTrave = Convert.ToDecimal(0);
            decimal baseLabor = Convert.ToDecimal(0);
            decimal laborMargin = Convert.ToDecimal(0);
            decimal laborCost = Convert.ToDecimal(0);
            decimal designMargin = Convert.ToDecimal(0);
            decimal salesMargin = Convert.ToDecimal(0);
            decimal baseTravel = Convert.ToDecimal(0);
            decimal sellPrice = Convert.ToDecimal(0);
            decimal totalMaterialCost = Convert.ToDecimal(0);

            AttributeCollection projectCostAllowances = GetDefaults();


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

            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_defaultlaborrate") && projectCostAllowances["ig1_defaultlaborrate"] != null)
            {
                laborRate = Convert.ToDecimal(projectCostAllowances["ig1_defaultlaborrate"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_lodging") && projectCostAllowances["ig1_lodging"] != null)
            {
                lodging = Convert.ToDecimal(projectCostAllowances["ig1_lodging"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_perdiem") && projectCostAllowances["ig1_perdiem"] != null)
            {
                Money money = (Money)projectCostAllowances["ig1_perdiem"];
                perDiem = Convert.ToDecimal(money.Value);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_labormargin") && projectCostAllowances["ig1_labormargin"] != null)
            {
                laborMargin = Convert.ToDecimal(projectCostAllowances["ig1_labormargin"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_designfactor") && projectCostAllowances["ig1_designfactor"] != null)
            {
                designFactor = Convert.ToDecimal(projectCostAllowances["ig1_designfactor"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_designlaborrate") && projectCostAllowances["ig1_designlaborrate"] != null)
            {
                Money money = (Money)projectCostAllowances["ig1_designlaborrate"];
                designLaborRate = Convert.ToDecimal(money.Value);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_designmargin") && projectCostAllowances["ig1_designmargin"] != null)
            {
                designMargin = Convert.ToDecimal(projectCostAllowances["ig1_designmargin"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_salesfactor") && projectCostAllowances["ig1_salesfactor"] != null)
            {
                salesFactor = Convert.ToDecimal(projectCostAllowances["ig1_salesfactor"]);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_saleslaborrate") && projectCostAllowances["ig1_saleslaborrate"] != null)
            {
                Money money = (Money)projectCostAllowances["ig1_saleslaborrate"];
                salesLaborRate = Convert.ToDecimal(money.Value);
            }
            if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_salesmargin") && projectCostAllowances["ig1_salesmargin"] != null)
            {
                salesMargin = Convert.ToDecimal(projectCostAllowances["ig1_salesmargin"]);
            }
            entity.Attributes["ig1_laborrate"] = laborRate;
            entity.Attributes["ig1_lodgingrate"] = lodging;
            entity.Attributes["ig1_perdiem"] = new Money(perDiem);
            entity.Attributes["ig1_lodgingtotal"] = Convert.ToDecimal(0);
            entity.Attributes["ig1_margin"] = margin;
            entity.Attributes["ig1_freight"] =new Money(freighttotal);
            entity.Attributes["ig1_freightsell"] = new Money(freightsell);
            entity.Attributes["ig1_totalmaterialcost"] = new Money(materialCost);

            laborCost = laborRate * luExtend;
            entity.Attributes["ig1_pmlaborsme"] = laborCost;
            entity.Attributes["ig1_baselabor"] =new Money(laborCost * laborMargin);

            entity.Attributes["ig1_designfactor"] = designFactor;
            entity.Attributes["ig1_designlaborrate"] = new Money(designLaborRate);
            designCost = designFactor * luExtend * designLaborRate;
            entity.Attributes["ig1_designcost"] = new Money(designCost);
            entity.Attributes["ig1_basedesign"] = new Money(designCost * designMargin);

            entity.Attributes["ig1_salesfactor"] = salesFactor;
            entity.Attributes["ig1_saleslaborrate"] = new Money(salesLaborRate);
            salesCost = salesFactor * luExtend * salesLaborRate;
            entity.Attributes["ig1_salescost"] = new Money(salesCost);
            entity.Attributes["ig1_basesales"] = new Money(salesCost * salesMargin);

            entity.Attributes["ig1_airfaretrans"] = new Money(0);
            entity.Attributes["ig1_days"] = Convert.ToDecimal(0);
            entity.Attributes["ig1_travelcost"] = new Money(0);

            entity.Attributes["ig1_materialcost"] = new Money(materialCost);
            entity.Attributes["ig1_totaldirectcost"] = Convert.ToDecimal(freighttotal + materialCost);
            entity.Attributes["ig1_baseindirect"] = new Money(baseSales + baseDesign + baseTravel + baseLabor);

            if (margin >= 0 && margin < 100)
            {
                totalMaterialCost = materialCost / (1 - margin / 100);
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

            service.Create(entity);
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
                decimal materialCost = arr[0];
                decimal luExtend = arr[1];
                decimal freighttotal = arr[2];
                decimal freightsell = arr[3];
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

                AttributeCollection projectCostAllowances = GetDefaults();
                if (result.Contains("ig1_designfactor") && result["ig1_designfactor"] != null)
                {
                    designFactor = Convert.ToDecimal(result["ig1_designfactor"]);
                }
                if (result.Contains("ig1_designlaborrate") && result["ig1_designlaborrate"] != null)
                {
                    Money money = (Money)result["ig1_designlaborrate"];
                    designLaborRate = Convert.ToDecimal(money.Value);
                }
                if (result.Contains("ig1_salesfactor") && result["ig1_salesfactor"] != null)
                {
                    salesFactor = Convert.ToDecimal(result["ig1_salesfactor"]);
                }
                if (result.Contains("ig1_saleslaborrate") && result["ig1_saleslaborrate"] != null)
                {
                    Money money = (Money)result["ig1_saleslaborrate"];
                    salesLaborRate = Convert.ToDecimal(money.Value);
                }
                if (result.Contains("ig1_laborrate") && result["ig1_laborrate"] != null)
                {
                    laborRate = Convert.ToDecimal(result["ig1_laborrate"]);
                }
                if (result.Contains("ig1_margin") && result["ig1_margin"] != null)
                {
                    margin = Convert.ToDecimal(result["ig1_margin"]);
                }
                if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_labormargin") && projectCostAllowances["ig1_labormargin"] != null)
                {
                    laborMargin = Convert.ToDecimal(projectCostAllowances["ig1_labormargin"]);
                }
                if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_designmargin") && projectCostAllowances["ig1_designmargin"] != null)
                {
                    designMargin = Convert.ToDecimal(projectCostAllowances["ig1_designmargin"]);
                }
                if (projectCostAllowances.Count > 0 && projectCostAllowances.Contains("ig1_salesmargin") && projectCostAllowances["ig1_salesmargin"] != null)
                {
                    salesMargin = Convert.ToDecimal(projectCostAllowances["ig1_salesmargin"]);
                }
                if (result.Contains("ig1_numberoftrip") && result["ig1_numberoftrip"] != null)
                {
                    number_of_trip = Convert.ToDecimal(result["ig1_numberoftrip"]);
                }
                if (result.Contains("ig1_peoplepertrip") && result["ig1_peoplepertrip"] != null)
                {
                    people_per_trip = Convert.ToInt32(result["ig1_peoplepertrip"]);
                }
                if (result.Contains("ig1_days") && result["ig1_days"] != null)
                {
                    days_per_trip = Convert.ToDecimal(result["ig1_days"]);
                }
                if (result.Contains("ig1_perdiem") && result["ig1_perdiem"] != null)
                {
                    Money money = (Money)result["ig1_perdiem"];
                    perDiam = Convert.ToDecimal(money.Value);
                }
                if (result.Contains("ig1_lodgingrate") && result["ig1_lodgingrate"] != null)
                {
                    lodgingRate = Convert.ToDecimal(result["ig1_lodgingrate"]);
                }
                if (result.Contains("ig1_airfaretrans") && result["ig1_airfaretrans"] != null)
                {
                    Money money = (Money)result["ig1_airfaretrans"];
                    airFare = Convert.ToDecimal(money.Value);
                }

                associatedCost.Attributes["ig1_materialcost"] = new Money(materialCost);
                associatedCost.Attributes["ig1_luextend"] = luExtend;
                associatedCost.Attributes["ig1_freight"] = new Money(freighttotal);
                associatedCost.Attributes["ig1_freightsell"] = new Money(freightsell);

                designCost = designFactor * luExtend * designLaborRate;
                baseDesign = designCost * designMargin;
                associatedCost.Attributes["ig1_designcost"] = new Money(designCost);
                associatedCost.Attributes["ig1_basedesign"] = new Money(baseDesign);

                salesCost = salesFactor * luExtend * salesLaborRate;
                baseSales = salesCost * salesMargin;
                associatedCost.Attributes["ig1_salescost"] = new Money(salesCost);
                associatedCost.Attributes["ig1_basesales"] = new Money(baseSales);

                designHours = designFactor * luExtend;
                salesHours = salesFactor * luExtend;
                associatedCost.Attributes["ig1_designhours"] = designHours;
                associatedCost.Attributes["ig1_saleshours"] = salesHours;

                laborCost = laborRate * luExtend;
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
                    
                    totalMaterialCost = materialCost / (1 - margin / 100);
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
                associatedCost.Attributes["ig1_totaldirectsell"] =new Money(freightsell + totalMaterialCost);
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
        protected AttributeCollection GetDefaults()
        {
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_projectcostallowances'>
                                <attribute name='ig1_designfactor' />
                                <attribute name='ig1_salesfactor' />
                                <attribute name='ig1_designmargin' />
                                <attribute name='ig1_designlaborrate' />
                                <attribute name='ig1_perdiem' />
                                <attribute name='ig1_saleslaborrate' />
                                <attribute name='ig1_corpgna' />
                                <attribute name='ig1_salesmargin' />
                                <attribute name='ig1_effectivedate' />
                                <attribute name='ig1_labormargin' />
                                <attribute name='ig1_margin' />
                                <attribute name='ig1_defaultlaborrate' />
                                <attribute name='ig1_lodging' />
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                return entityCollection.Entities[0].Attributes;
            }
            else
            {
                return null;
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
                
                AttributeCollection projectCostAllowances = GetDefaults();
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
            decimal totalSellPrice = Convert.ToDecimal(0);

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
                    if (result.Contains("ig1_freight") && result["ig1_freight"]!=null)
                    {
                        Money money = (Money)result["ig1_freight"];
                        freightCost += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_totaldirectcost") && result["ig1_totaldirectcost"]!=null)
                    {
                        directPrice += Convert.ToDecimal(result["ig1_totaldirectcost"]);
                    }
                    if (result.Contains("ig1_totalindirectcost") && result["ig1_totalindirectcost"] != null)
                    {
                        indirectPrice += Convert.ToDecimal(result["ig1_totalindirectcost"]);
                    }
                    if (result.Contains("ig1_saleshours") && result["ig1_saleshours"]!=null)
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
                    if (result.Contains("ig1_pmlaborsme") && result["ig1_pmlaborsme"]!=null)
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
                        totalSellPrice += Convert.ToDecimal(money.Value);
                    }
                }

                Entity bidsheet = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_name"));

                bidsheet.Attributes["ig1_directcost"] = directPrice;
                bidsheet.Attributes["ig1_indirectcost"] = indirectPrice;
                bidsheet.Attributes["ig1_totalhours"] = salesHours + designHours + laborHours;
                bidsheet.Attributes["ig1_totalcost"] = new Money(directPrice + indirectPrice);
                bidsheet.Attributes["ig1_materialcost"] = materialCost;
                bidsheet.Attributes["ig1_freightamount"] = new Money(freightCost);
                bidsheet.Attributes["ig1_pmhours"] = laborHours;
                bidsheet.Attributes["ig1_pmcost"] = laborPrice;
                bidsheet.Attributes["ig1_designhours"] = designHours;
                bidsheet.Attributes["ig1_designcost"] = new Money(designPrice);
                bidsheet.Attributes["ig1_saleshours"] = salesHours;
                bidsheet.Attributes["ig1_salescost"] = new Money(salesPrice);
                bidsheet.Attributes["ig1_lodgingtotal"] = lodgingTotal;
                bidsheet.Attributes["ig1_transtotal"] = transTotal;
                bidsheet.Attributes["ig1_perdiem"] = new Money(perDiamTotal);
                bidsheet.Attributes["ig1_totaltravel"] = new Money(totalTravel);

                decimal contingencyCost = GetContingency(bidsheetid);
                bidsheet.Attributes["ig1_totalbomcost"] = materialCost + freightCost;
                bidsheet.Attributes["ig1_sellprice"] = new Money(totalSellPrice + contingencyCost);
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
    }
}
