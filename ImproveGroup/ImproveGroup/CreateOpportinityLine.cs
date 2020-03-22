using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
namespace ImproveGroup
{
    public class CreateOpportinityLine : IPlugin
    {
        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Setup
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            //service = serviceFactory.CreateOrganizationService(new Guid("EFA54FBF-71A2-E911-A962-000D3A1D5D97"));
            #endregion

            if (context.InputParameters.Equals(null))
            {
                return;
            }
            try
            {
                string bidSheetId = context.InputParameters["bidSheetId"].ToString();
                var opportunityId = context.InputParameters["opportunityId"].ToString();
                var opportunityName = context.InputParameters["opportunityName"].ToString();
                if (opportunityId.Equals("") || opportunityId.Equals(null) || bidSheetId.Equals("") || bidSheetId.Equals(null) || opportunityName.Equals(null) || opportunityName.Equals(""))
                {
                    return;
                }

                //Nazish - Fetching all products which are associated to Bill of Material
                var fetchData = new
                {
                    ig1_bidsheet = bidSheetId
                };
                var fetchXmlBOMProducts = $@"
                                <fetch>
                                  <entity name='product'>
                                    <attribute name='defaultuomid' />
                                    <attribute name='productid' />
                                    <attribute name='name' />
                                    <link-entity name='ig1_bidsheetpricelistitem' from='ig1_product' to='productid'>
                                      <attribute name='ig1_materialcost' alias='materialCost'/>
                                      <attribute name='ig1_sdt' alias='sdt'/>
                                      <filter type='and'>
                                        <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet}'/>
                                      </filter>
                                      <link-entity name='ig1_associatedcost' from='ig1_bidsheetcategory' to='ig1_category' link-type='outer'>
                                        <attribute name='ig1_margin' alias='margin'/>
                                        <filter type='and'>
                                          <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet}'/>
                                        </filter>
                                      </link-entity>
                                    </link-entity>
                                  </entity>
                                </fetch>";
                EntityCollection BOMProductsData = service.RetrieveMultiple(new FetchExpression(fetchXmlBOMProducts));
                var unitId = new Guid();
                if (BOMProductsData.Entities.Count > 0)
                {
                    foreach (var item in BOMProductsData.Entities)
                    {
                        var productId = new Guid();
                        var materialCost = new Money(0);
                        var totalMaterialCost = new Money(0);
                        var margin = new decimal(0);

                        if (item.Attributes.Contains("productid"))
                        {
                            productId = (Guid)item.Attributes["productid"];
                        }
                        if (item.Attributes.Contains("defaultuomid"))
                        {
                            var unit = (EntityReference)item.Attributes["defaultuomid"];
                            unitId = (Guid)unit.Id;
                        }
                        if (item.Attributes.Contains("materialCost"))
                        {
                            materialCost = (Money)((AliasedValue)item.Attributes["materialCost"]).Value;
                        }
                        if (item.Attributes.Contains("margin"))
                        {
                            margin = (decimal)((AliasedValue)item.Attributes["margin"]).Value;
                        }
                        if (margin > 0 && margin<100)
                        {
                            totalMaterialCost = new Money(materialCost.Value / (1 - margin / 100));
                        }
                        else
                        {
                            totalMaterialCost = new Money(materialCost.Value);
                        }
                        if (item.Attributes.Contains("sdt"))
                        {
                            totalMaterialCost = new Money((decimal)totalMaterialCost.Value + (decimal)((Money)((AliasedValue)item.Attributes["sdt"]).Value).Value);
                        }
                        CreatePriceListItem(opportunityId, opportunityName, productId, unitId, totalMaterialCost);
                        CreateOpportunityLine(productId, opportunityId, unitId);
                    }
                }
            }
            catch (Exception ex)
            {
                IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in CreateOpportunityLIne Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                serviceAdmin.Create(errorLog);
            }
        }
        protected void CreatePriceListItem(string opportunityId, string opportunityName, Guid productId, Guid unitId, Money materialCost)
        {
            var fetchDataOpportunity = new
            {
                PriceLevelopportunityid = opportunityId,
            };
            var PriceLevelFetch = $@"
                                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                        <entity name='pricelevel'>
                                        <attribute name='transactioncurrencyid' />
                                        <attribute name='name' />
                                        <attribute name='pricelevelid' />
                                        <link-entity name='opportunity' from='pricelevelid' to='pricelevelid'>
                                            <filter type='and'>
                                            <condition attribute='opportunityid' operator='eq' value='{fetchDataOpportunity.PriceLevelopportunityid}'/>
                                            </filter>
                                        </link-entity>
                                        </entity>
                                    </fetch>";
            EntityCollection priceLevelData = service.RetrieveMultiple(new FetchExpression(PriceLevelFetch));
            if (priceLevelData.Entities.Count > 0)
            {
                Guid priceList = priceLevelData.Entities[0].Id;
                AddProductToPriceList(priceList, productId, unitId, materialCost);
            }
            else
            {
                var priceList = CreatePriceList(opportunityId, opportunityName);
                AddProductToPriceList(priceList, productId, unitId, materialCost);
            }

        }

        protected void CreateOpportunityLine(Guid productid, string opportunityId, Guid unitId)
        {
            var isProductNotExist = true;
            var fetchData = new
            {
                opportunityid = opportunityId,
                productid = productid
            };
            var fetchXml = $@"
                            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='opportunityproduct'>
                                <attribute name='opportunityproductid' />
                                <attribute name='opportunityid' />
                                <attribute name='uomid' />
                                <attribute name='productid' />
                                <attribute name='quantity' />
                                <filter type='and'>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid}'/>
                                  <condition attribute='productid' operator='eq' value='{fetchData.productid}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            var recordCount = result.Entities.Count;
            var productId = new Guid();
            var opportunityProductId = new Guid();
            if (recordCount > 0)
            {
                productId = (Guid)((EntityReference)result.Entities[0].Attributes["productid"]).Id;
                opportunityProductId = (Guid)(result.Entities[0].Attributes["opportunityproductid"]);
                isProductNotExist = false;
            }

            if (isProductNotExist)
            {
                Entity opportunityLine = new Entity("opportunityproduct");
                opportunityLine["productid"] = new EntityReference("product", productid);
                opportunityLine["opportunityid"] = new EntityReference("opportunity", new Guid(opportunityId));
                opportunityLine["uomid"] = new EntityReference("uom", unitId);
                opportunityLine["quantity"] = new decimal(1);
                service.Create(opportunityLine);
            }
            else
            {
                Entity opportunityLine = service.Retrieve("opportunityproduct", opportunityProductId, new ColumnSet("productid", "opportunityid", "uomid", "quantity"));
                opportunityLine["productid"] = new EntityReference("product", productId);
                opportunityLine["opportunityid"] = new EntityReference("opportunity", new Guid(opportunityId));
                opportunityLine["uomid"] = new EntityReference("uom", unitId);
                opportunityLine["quantity"] = new decimal(1);
                service.Update(opportunityLine);
            }
        }

        protected void AddProductToPriceList(Guid priceList, Guid productId, Guid unitId, Money materialCost)
        {

            var productPriceLevelData = new
            {
                productid = productId,
                pricelevelid = priceList

            };
            var productPriceLevelfetchXml = $@"
                                                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                        <entity name='productpricelevel'>
                                                        <attribute name='productid' />
                                                        <attribute name='pricelevelid' />
                                                        <attribute name='productpricelevelid' />
                                                        <attribute name='productidname' />
                                                        <filter type='and'>
                                                            <condition attribute='productid' operator='eq' value='{productPriceLevelData.productid}'/>
                                                            <condition attribute='pricelevelid' operator='eq' value='{productPriceLevelData.pricelevelid}'/>
                                                            </filter>
                                                        </entity>
                                                    </fetch>";
            var check = true;
            var productPriceLevelId = new Guid();
            EntityCollection productPriceLevel = service.RetrieveMultiple(new FetchExpression(productPriceLevelfetchXml));
            foreach (var product in productPriceLevel.Entities)
            {
                var proId = (EntityReference)(product.Attributes["productid"]);
                if (proId.Id.Equals(productId) && !productPriceLevel.Equals(null))
                {
                    check = false;
                    productPriceLevelId = (Guid)product.Attributes["productpricelevelid"];
                }
            }
            if (check)
            {
                Entity priceListItem = new Entity("productpricelevel");
                priceListItem["pricelevelid"] = new EntityReference("pricelevel", priceList);
                priceListItem["productid"] = new EntityReference("product", productId);
                priceListItem["uomid"] = new EntityReference("uom", unitId);
                priceListItem["amount"] = materialCost;
                service.Create(priceListItem);
            }
            else
            {

                Entity priceListItem = service.Retrieve("productpricelevel", productPriceLevelId, new ColumnSet("pricelevelid", "productid", "uomid", "amount"));
                priceListItem["pricelevelid"] = new EntityReference("pricelevel", priceList);
                priceListItem["productid"] = new EntityReference("product", productId);
                priceListItem["uomid"] = new EntityReference("uom", unitId);
                priceListItem["amount"] = materialCost;
                service.Update(priceListItem);
            }
        }
        protected Guid CreatePriceList(string opportunityId, string opportunityName)
        {
            var existingPriceList = GetExistingPriceListByName(opportunityId, opportunityName);
            if (existingPriceList != null && existingPriceList != Guid.Empty)
            {
                Entity opportunityEntity = service.Retrieve("opportunity", new Guid(opportunityId), new ColumnSet("pricelevelid"));
                opportunityEntity["pricelevelid"] = new EntityReference("pricelevel", existingPriceList);
                service.Update(opportunityEntity);

                return existingPriceList;
            }
            else
            {
                Entity entity = service.Retrieve("opportunity", new Guid(opportunityId), new ColumnSet("transactioncurrencyid"));

                Entity priceList = new Entity("pricelevel");
                if (opportunityName != null && opportunityName != "")
                {
                    priceList["name"] = opportunityName + opportunityId;
                }
                if (entity.Attributes["transactioncurrencyid"] != null)
                {
                    EntityReference transactionCurrency = (EntityReference)entity.Attributes["transactioncurrencyid"];
                    priceList["transactioncurrencyid"] = transactionCurrency;
                }
                else
                {
                    var transactionCurrencyId = GetDefaultCurrencyId();
                    if (transactionCurrencyId != null && transactionCurrencyId != Guid.Empty)
                    {
                        priceList["transactioncurrencyid"] = new EntityReference("transactioncurrency", transactionCurrencyId);
                    }
                }

                var priceListId = service.Create(priceList);

                if (priceList != null)
                {
                    Entity opportunityEntity = service.Retrieve("opportunity", new Guid(opportunityId), new ColumnSet("pricelevelid"));
                    opportunityEntity["pricelevelid"] = new EntityReference("pricelevel", priceListId);
                    service.Update(opportunityEntity);
                }
                return priceListId;
            }
            
        }
        protected Guid GetExistingPriceListByName(string opportunityId, string opportunityName)
        {
                var fetchData = new
                {
                    name = opportunityName+ opportunityId
                };
                var fetchXml = $@"
                                <fetch mapping='logical' version='1.0'>
                                  <entity name='pricelevel'>
                                    <attribute name='pricelevelid' />
                                    <filter>
                                      <condition attribute='name' operator='eq' value='{fetchData.name/*Price List Issue4d87e32b-0f02-ea11-a811-000d3a55d2c3*/}'/>
                                    </filter>
                                  </entity>
                                </fetch>";

            EntityCollection existingPriceList = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (existingPriceList.Entities.Count > 0)
            {
                return existingPriceList.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
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