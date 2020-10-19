using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
namespace IG_ActivateBidSheet
{
    public class ActivateBidSheet : IPlugin
    {
        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        Guid bidsheetid = Guid.Empty;
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Setup
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            #endregion

            if (context.InputParameters!=null && context.InputParameters.Contains("bidSheetId"))
            {
                bidsheetid = new Guid(context.InputParameters["bidSheetId"].ToString());
                GetBidsheetLineItems(bidsheetid);

            }
        }

        protected void GetBidsheetLineItems(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_productname = "PM Labor",
                ig1_categoryname = "Labor",
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_product' />
                                <attribute name='ig1_category' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*ig1_bidsheet*/}'/>
                                  <condition attribute='ig1_productname' operator='neq' value='{fetchData.ig1_productname/*PM Labor*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                </filter>
                                <link-entity name='product' from='productid' to='ig1_product' link-type='inner'>
                                  <attribute name='defaultuomid' alias='uom'/>
                                  <filter>
                                    <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                EntityReference opportunity = null;
                Entity entity = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_opportunitytitle"));
                if (entity.Attributes.Contains("ig1_opportunitytitle") && entity.Attributes["ig1_opportunitytitle"] != null)
                {
                    opportunity = (EntityReference)entity.Attributes["ig1_opportunitytitle"];
                }
                decimal totalLaborCost = Convert.ToDecimal(0);
                Guid[] category = new Guid[entityCollection.Entities.Count];
                int i = 0;
                foreach (var item in entityCollection.Entities)
                {
                    decimal materialCost = Convert.ToDecimal(0);
                    decimal margin = Convert.ToDecimal(0);
                    decimal totalMaterialCost = Convert.ToDecimal(0);
                    Guid productid = Guid.Empty;
                    Guid categoryid = Guid.Empty;
                    Guid defaultUnit = Guid.Empty;
                    decimal salesCost = Convert.ToDecimal(0);
                    decimal designCost = Convert.ToDecimal(0);
                    decimal travelCost = Convert.ToDecimal(0);
                    decimal laborCost = Convert.ToDecimal(0);
                    decimal categorysdt = Convert.ToDecimal(0);
                    decimal categoryMaterialCost = Convert.ToDecimal(0);
                    decimal productsdt = Convert.ToDecimal(0);

                    AttributeCollection associatedCost = null;
                    var result = item.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"] != null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_product") && result["ig1_product"]!=null)
                    {
                        EntityReference entityReference = (EntityReference)result["ig1_product"];
                        productid = entityReference.Id;
                    }
                    if (result.Contains("ig1_category") && result["ig1_category"] != null)
                    {
                        EntityReference entityReference = (EntityReference)result["ig1_category"];
                        categoryid = entityReference.Id;
                        associatedCost = IndirectCost(bidsheetid, categoryid);
                    }
                    if (result.Contains("uom") && result["uom"] != null)
                    {
                        AliasedValue aliasedValue = (AliasedValue)result["uom"];
                        EntityReference entityReference = (EntityReference)aliasedValue.Value;
                        defaultUnit = entityReference.Id;
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_materialcost") && associatedCost["ig1_materialcost"] != null)
                    {
                        Money money = (Money)associatedCost["ig1_materialcost"];
                        categoryMaterialCost = Convert.ToDecimal(money.Value);
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_margin") && associatedCost["ig1_margin"] != null)
                    {
                        margin = Convert.ToDecimal(associatedCost["ig1_margin"]);
                        if (margin > 0 && margin < 100)
                        {
                            totalMaterialCost = Math.Round((materialCost / (1 - margin / 100)), 2);
                        }
                        else
                        {
                            totalMaterialCost = materialCost;
                        }
                    }
                    else if (item.Attributes.Contains("ig1_category") && item.Attributes["ig1_category"] != null)
                    {
                        EntityReference entityReference = (EntityReference)item.Attributes["ig1_category"];
                        if (entityReference.Name == "Contingency")
                        {
                            totalMaterialCost = materialCost;
                        }
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_salescost") && associatedCost["ig1_salescost"] != null)
                    {
                        Money money = (Money)associatedCost["ig1_salescost"];
                        salesCost = Convert.ToDecimal(money.Value); 
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_designcost") && associatedCost["ig1_designcost"] != null)
                    {
                        Money money = (Money)associatedCost["ig1_designcost"];
                        designCost = Convert.ToDecimal(money.Value);
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_travelcost") && associatedCost["ig1_travelcost"] != null)
                    {
                        Money money = (Money)associatedCost["ig1_travelcost"];
                        travelCost = Convert.ToDecimal(money.Value);
                    }
                    if (associatedCost != null && associatedCost.Count > 0 && associatedCost.Contains("ig1_pmlaborsme") && associatedCost["ig1_pmlaborsme"] != null && !category.Contains(categoryid))
                    {
                        laborCost = Convert.ToDecimal(associatedCost["ig1_pmlaborsme"]);
                        totalLaborCost += laborCost;
                        category[i] = categoryid;
                        i++;
                    }
                    categorysdt = salesCost + designCost + travelCost;
                    if (categoryMaterialCost > 0)
                    {
                        productsdt = (materialCost / categoryMaterialCost) * categorysdt;
                    }
                    totalMaterialCost = totalMaterialCost + productsdt;
                    CreatePriceListItem(bidsheetid, productid, defaultUnit, totalMaterialCost);
                    CreateOpportunityLine(productid, opportunity.Id, defaultUnit);
                }
                i = 0;
                Array.Clear(category, i, category.Length);
                if (totalLaborCost > 0)
                {
                    Guid defaultUnit = Guid.Empty;
                    Guid productid = Guid.Empty;
                    AttributeCollection result = GetPMLabor();
                    if (result != null && result.Count > 0 && result.Contains("defaultuomid") && result["defaultuomid"] != null)
                    {
                        EntityReference entityReference = (EntityReference)result["defaultuomid"];
                        defaultUnit = entityReference.Id;
                    }
                    if (result != null && result.Count > 0 && result.Contains("productid") && result["productid"] != null)
                    {
                        productid = (Guid)result["productid"];
                    }
                    CreatePriceListItem(bidsheetid, productid, defaultUnit, totalLaborCost);
                    CreateOpportunityLine(productid, opportunity.Id, defaultUnit);
                }
            }
        }
        protected AttributeCollection IndirectCost(Guid bidsheetid, Guid categoryid)
        {
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_bidsheetcategory = categoryid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_materialcost'/>
                                <attribute name='ig1_margin' />
                                <attribute name='ig1_salescost' />
                                <attribute name='ig1_designcost' />
                                <attribute name='ig1_travelcost' />
                                <attribute name='ig1_pmlaborsme' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*912743e1-d823-4017-a7f8-93d0ac4a9298*/}'/>
                                  <condition attribute='ig1_bidsheetcategory' operator='eq' value='{fetchData.ig1_bidsheetcategory/*b55a0caf-bd8b-ea11-a812-000d3a55dce2*/}'/>
                                </filter>
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
        protected AttributeCollection GetPMLabor()
        {
            var fetchData = new
            {
                productnumber = "PML001",
                statecode = "0",
                ig1_bidsheetcategoryname = "Labor"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='product'>
                                <attribute name='productid'/>
                                <attribute name='name'/>
                                <attribute name='productnumber'/>
                                <attribute name='defaultuomid'/>
                                <filter type='and'>
                                  <condition attribute='productnumber' operator='eq' value='{fetchData.productnumber/*PML001*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_bidsheetcategoryname' operator='eq' value='{fetchData.ig1_bidsheetcategoryname/*Labor*/}'/>
                                </filter>
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
        protected void CreatePriceListItem(Guid bidsheetid, Guid productId, Guid unitId, decimal materialCost)
        {
            var fetchData = new
            {
                ig1_bidsheetid = bidsheetid
            };
            var fetchXml = $@"
                                      <fetch>
                                      <entity name='ig1_bidsheet'>
                                      <attribute name='ig1_upperrevisionid' />
                                      <attribute name='ig1_projectnumber' />
                                      <attribute name='ig1_pricelist' />
                                      <filter type='and'>
                                     <condition attribute='ig1_bidsheetid' operator='eq' value='{fetchData.ig1_bidsheetid/*31156121-3df3-ea11-a815-000d3a98d873*/}'/>
                                          </filter>
                                        </entity>
                                         </fetch>";

            EntityCollection entitycollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entitycollection.Entities.Count > 0)
            {
                String projectnumber = entitycollection.Entities[0].GetAttributeValue<string>("ig1_projectnumber").ToString();
                //  String pricelistname= priceLevelData.Entities[0].GetAttributeValue<string>("ig1_pricelist").ToString();
                int uppreviseid = entitycollection.Entities[0].GetAttributeValue<int>("ig1_upperrevisionid");


                if (entitycollection.Entities[0].Attributes.Contains("ig1_pricelist") && entitycollection.Entities[0].Attributes["ig1_pricelist"] != null)
                {
                    Guid priceList = entitycollection.Entities[0].GetAttributeValue<EntityReference>("ig1_pricelist").Id;

                    Entity priceListItem = new Entity("productpricelevel");
                    priceListItem["pricelevelid"] = new EntityReference("pricelevel", priceList);
                    priceListItem["productid"] = new EntityReference("product", productId);
                    priceListItem["uomid"] = new EntityReference("uom", unitId);
                    priceListItem["amount"] = new Money(materialCost);
                    service.Create(priceListItem);

                    Entity opportunity = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_opportunitytitle"));
                    Guid oppid = opportunity.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;


                    Entity opportunityEntity = service.Retrieve("opportunity", oppid, new ColumnSet("pricelevelid"));
                    opportunityEntity["pricelevelid"] = new EntityReference("pricelevel", priceList);
                    service.Update(opportunityEntity);

                }
                else
                {
                    var priceList = CreatePriceList(projectnumber, uppreviseid);
                    Entity priceListItem = new Entity("productpricelevel");
                    priceListItem["pricelevelid"] = new EntityReference("pricelevel", priceList);
                    priceListItem["productid"] = new EntityReference("product", productId);
                    priceListItem["uomid"] = new EntityReference("uom", unitId);
                    priceListItem["amount"] = new Money(materialCost);
                    service.Create(priceListItem);

                
                }

            }
        }
        protected void CreateOpportunityLine(Guid productid, Guid opportunityId, Guid unitId)
        {
            Entity opportunityLine = new Entity("opportunityproduct");
            opportunityLine["productid"] = new EntityReference("product", productid);
            opportunityLine["opportunityid"] = new EntityReference("opportunity", opportunityId);
            opportunityLine["uomid"] = new EntityReference("uom", unitId);
            opportunityLine["quantity"] = new decimal(1);
            service.Create(opportunityLine);
        }
        protected Guid CreatePriceList(string priceprojectnumber,int upperreviseid)
        {
            var existingPriceList = GetExistingPriceListByName(priceprojectnumber, upperreviseid);
            if (existingPriceList != null && existingPriceList != Guid.Empty)
            {
                Entity opportunity=service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_opportunitytitle"));
                Guid oppid = opportunity.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;


                Entity opportunityEntity = service.Retrieve("opportunity", oppid, new ColumnSet("pricelevelid"));
                opportunityEntity["pricelevelid"] = new EntityReference("pricelevel", existingPriceList);
                service.Update(opportunityEntity);

                return existingPriceList;
            }
            
            else
            {
                return Guid.Empty; 
            }

        }
        protected Guid GetExistingPriceListByName(string prjnumber,int id)
        {
            //Entity transid = service.Retrieve("opportunity", bidsheetid, new ColumnSet("transactioncurrencyid"));
            Entity opptitle= service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_opportunitytitle"));
            Guid oppid = opptitle.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
            Entity transid = service.Retrieve("opportunity", oppid, new ColumnSet("transactioncurrencyid"));
            
            var transactionCurrency = (EntityReference)transid.Attributes["transactioncurrencyid"];
            var currencyId = (Guid)transactionCurrency.Id;
            var priceListName = prjnumber + " - " + id.ToString();
            Entity priceList = new Entity("pricelevel");
            priceList.Attributes["name"] = priceListName;
            priceList.Attributes["transactioncurrencyid"] = new EntityReference(transactionCurrency.LogicalName, currencyId);

            Guid pricelistid = service.Create(priceList);
            Entity objbidsheet = new Entity("ig1_bidsheet", bidsheetid);
            objbidsheet["ig1_pricelist"] = new EntityReference("pricelevel",pricelistid);
            service.Update(objbidsheet);
            return pricelistid;


        }
        protected Guid GetDefaultCurrencyId()
        {
            var fetchData = new
            {
                isocurrencycode = "USD",
                statecode = "0"
            };
            var fetchXml = $@"
                                <fetch mapping='logical' version='1.0'>
                                  <entity name='transactioncurrency'>
                                    <attribute name='transactioncurrencyid' />
                                    <attribute name='currencyname' />
                                    <filter type='and'>
                                      <condition attribute='isocurrencycode' operator='eq' value='{fetchData.isocurrencycode/*USD*/}'/>
                                      <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                    </filter>
                                  </entity>
                                </fetch>";
            EntityCollection currency = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (currency.Entities.Count > 0)
            {
                return (currency.Entities[0].Id);
            }
            else
            {
                return Guid.Empty;
            }

        }
    }
}
