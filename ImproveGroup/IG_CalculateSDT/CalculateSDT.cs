using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace IG_CalculateSDT
{
    public class CalculateSDT : IPlugin
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

                if (context.InputParameters.Equals(null) && !context.InputParameters.Contains("bidsheetId"))
                {
                    return;
                }

                string bidsheetId = context.InputParameters["bidsheetId"].ToString();
                if (!string.IsNullOrEmpty(bidsheetId))
                {
                   CalculateCategorySDT(new Guid(bidsheetId));
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        protected void CalculateCategorySDT(Guid bidsheetId)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_designfactor' />
                                <attribute name='ig1_designlaborrate' />
                                <attribute name='ig1_salesfactor' />
                                <attribute name='ig1_saleslaborrate' />
                                <attribute name='ig1_bidsheetcategory' />
                                <attribute name='ig1_numberoftrip' />
                                <attribute name='ig1_days' />
                                <attribute name='ig1_airfaretrans' />
                                <attribute name='ig1_perdiem' />
                                <attribute name='ig1_peoplepertrip' />
                                <attribute name='ig1_lodgingrate' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var associatedItems in entityCollection.Entities)
                {
                    decimal salesfactor = Convert.ToDecimal(0);
                    decimal saleslaborrate = Convert.ToDecimal(0);
                    decimal designfactor = Convert.ToDecimal(0);
                    decimal designlaborrate = Convert.ToDecimal(0);
                    decimal number_of_trip = Convert.ToDecimal(0);
                    decimal peaple_per_trip = Convert.ToDecimal(0);
                    decimal airFare = Convert.ToDecimal(0);
                    decimal days_per_trip = Convert.ToDecimal(0);
                    decimal perDiem = Convert.ToDecimal(0);
                    decimal lodging = Convert.ToDecimal(0);
                    decimal materialCost = Convert.ToDecimal(0);
                    decimal laborHours = Convert.ToDecimal(0);
                    decimal salesCost = Convert.ToDecimal(0);
                    decimal designCost = Convert.ToDecimal(0);
                    decimal travelCost = Convert.ToDecimal(0);
                    
                    decimal[] categoryValues = new decimal[2];
                    Guid categoryId = Guid.Empty;
                    var result = associatedItems.Attributes;
                    if (result.Contains("ig1_designfactor") && result["ig1_designfactor"] != null)
                    {
                        designfactor = Convert.ToDecimal(result["ig1_designfactor"]);
                    }
                    if (result.Contains("ig1_designlaborrate") && result["ig1_designlaborrate"] != null)
                    {
                        Money money = (Money)result["ig1_designlaborrate"];
                        designlaborrate = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_salesfactor") && result["ig1_salesfactor"] != null)
                    {
                        salesfactor = Convert.ToDecimal(result["ig1_salesfactor"]);
                    }
                    if (result.Contains("ig1_saleslaborrate") && result["ig1_saleslaborrate"] != null)
                    {
                        Money money = (Money)result["ig1_saleslaborrate"];
                        saleslaborrate = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_numberoftrip") && result["ig1_numberoftrip"] != null)
                    {
                        number_of_trip = Convert.ToDecimal(result["ig1_numberoftrip"]);
                    }
                    if (result.Contains("ig1_days") && result["ig1_days"] != null)
                    {
                        days_per_trip = Convert.ToDecimal(result["ig1_days"]);
                    }
                    if (result.Contains("ig1_airfaretrans") && result["ig1_airfaretrans"] != null)
                    {
                        Money money = (Money)result["ig1_airfaretrans"];
                        airFare = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_perdiem") && result["ig1_perdiem"] != null)
                    {
                        Money money = (Money)result["ig1_perdiem"];
                        perDiem = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_peoplepertrip") && result["ig1_peoplepertrip"] != null)
                    {
                        peaple_per_trip = Convert.ToDecimal(result["ig1_peoplepertrip"]);
                    }
                    if (result.Contains("ig1_lodgingrate") && result["ig1_lodgingrate"]!=null)
                    {
                        lodging = Convert.ToDecimal(result["ig1_lodgingrate"]);
                    }
                    if (result.Contains("ig1_bidsheetcategory") && result["ig1_bidsheetcategory"] != null)
                    {
                        EntityReference entityReference = (EntityReference)result["ig1_bidsheetcategory"];
                        categoryId = entityReference.Id;
                    }
                    if (categoryId != Guid.Empty)
                    {
                        categoryValues = MaterialCost_LaborHours(bidsheetId, categoryId);
                        materialCost = categoryValues[0];
                        laborHours = categoryValues[1];
                        
                    }

                    salesCost = salesfactor* saleslaborrate * laborHours;
                    designCost = designfactor * designlaborrate * laborHours;
                    travelCost = (number_of_trip * peaple_per_trip * airFare) + (number_of_trip * peaple_per_trip * days_per_trip) * (perDiem + lodging);
                    
                    decimal categorySDT = salesCost + designCost + travelCost;
                    CalculateProductSDT(categoryId, bidsheetId, materialCost, categorySDT);
                }
            }
        }
        protected void CalculateProductSDT(Guid categoryId, Guid bidsheetId, decimal categoryMaterialCost, decimal categorySDT)
        {
            var fetchData = new
            {
                ig1_category = categoryId,
                ig1_bidsheet = bidsheetId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_materialcost' />
                                <filter type='and'>
                                  <condition attribute='ig1_category' operator='eq' value='{fetchData.ig1_category}'/>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var lineItem in entityCollection.Entities)
                {
                    var result = lineItem.Attributes;
                    decimal sdt = Convert.ToDecimal(0);
                    decimal materialCost = Convert.ToDecimal(0);

                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost = Convert.ToDecimal(money.Value);
                    }
                    sdt = (materialCost / categoryMaterialCost) * categorySDT;

                    lineItem.Attributes["ig1_sdt"] = new Money(sdt);
                    service.Update(lineItem);
                }
            }
        }

        protected decimal[] MaterialCost_LaborHours(Guid bidsheetId, Guid categoryId)
        {
            decimal[] categoryValues = new decimal[2];
            decimal categoryMaterialCost = Convert.ToDecimal(0);
            decimal categoryLaborHours = Convert.ToDecimal(0);
            var fetchData = new
            {
                ig1_bidsheet = bidsheetId,
                ig1_category = categoryId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_luextend'/>
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet}'/>
                                  <condition attribute='ig1_category' operator='eq' value='{fetchData.ig1_category}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var materialCost in entityCollection.Entities)
                {
                    var result = materialCost.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        categoryMaterialCost += Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_luextend") && result["ig1_luextend"] != null)
                    {
                        categoryLaborHours += Convert.ToDecimal(result["ig1_luextend"]);
                    }
                }
                categoryValues[0] = categoryMaterialCost;
                categoryValues[1] = categoryLaborHours;
            }
            return categoryValues;
        }
    }
}
