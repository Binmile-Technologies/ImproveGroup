using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_CalculateLaborCost
{
    public class CalculateLaborCost : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracingService;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity =(Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "ig1_bidsheet")
                    {
                        CreateUpdateLaborCost(entity.Id);
                    }
                    else if (entity.LogicalName == "ig1_associatedcost")
                    {
                        Entity entity1 = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_bidsheet"));
                        if (entity1.Attributes.Contains("ig1_bidsheet") && entity1.Attributes["ig1_bidsheet"] != null)
                        {
                            EntityReference entityReference = (EntityReference)entity1.Attributes["ig1_bidsheet"];
                            CreateUpdateLaborCost(entityReference.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in CalculateLaborCost Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                serviceAdmin.Create(errorLog);
            }
        }

        protected void CreateUpdateLaborCost(Guid bidsheetid)
        {
            var fetchData = new
            {
                ig1_productname = "PM Labor",
                ig1_categoryname = "Labor",
                ig1_bidsheet = bidsheetid
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_product' />
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_category' />
                                <filter type='and'>
                                    <condition attribute='ig1_productname' operator='eq' value='{fetchData.ig1_productname/*PM Labor*/}'/>
                                    <condition attribute='ig1_categoryname' operator='eq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                    <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d02e6d25-d104-ea11-a811-000d3a55dce2*/}'/>
                                </filter>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            decimal laborCost = CalculateLabor(bidsheetid);
            if (entityCollection.Entities.Count > 0)
            {
                Entity record = entityCollection.Entities[0];
                if (laborCost > 0)
                {
                    record.Attributes["ig1_unitprice"] = laborCost;
                    record.Attributes["ig1_quantity"] = Convert.ToInt32(1);
                    record.Attributes["ig1_materialcost"] = new Money(laborCost);
                    service.Update(record);
                }
                else
                {
                    service.Delete(record.LogicalName, record.Id);
                }
            }
            else
            {
                Guid categoryid = LaborCategory();
                Guid productid = LaborProduct();
                if (categoryid != Guid.Empty && productid != Guid.Empty && laborCost>0)
                {
                    Entity record = new Entity("ig1_bidsheetpricelistitem");
                    record.Attributes["ig1_bidsheet"] = new EntityReference("ig1_bidsheet", bidsheetid);
                    record.Attributes["ig1_category"] = new EntityReference("ig1_bidsheetcategory", categoryid);
                    record.Attributes["ig1_product"] = new EntityReference("product", productid);

                    Entity entity1 = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_opportunitytitle"));
                    if (entity1.Attributes.Contains("ig1_opportunitytitle") && entity1.Attributes["ig1_opportunitytitle"] != null)
                    {
                        EntityReference entityReference = (EntityReference)entity1.Attributes["ig1_opportunitytitle"];
                        record.Attributes["ig1_opportunity"] = new EntityReference("opportunity", entityReference.Id);
                    }
                    record.Attributes["ig1_quantity"] = Convert.ToInt32(1);
                    record.Attributes["ig1_unitprice"] = Convert.ToDecimal(laborCost);
                    record.Attributes["ig1_materialcost"] = new Money(laborCost);
                    service.Create(record);
                }
            }

            Entity entity = service.Retrieve("ig1_bidsheet", bidsheetid, new ColumnSet("ig1_laborcost"));
            entity.Attributes["ig1_laborcost"] = new Money(laborCost);
            service.Update(entity);
        }
        protected decimal CalculateLabor(Guid bidsheetid)
        {
            decimal laborCost = Convert.ToDecimal(0);
            var fetchData = new
            {
                ig1_bidsheet = bidsheetid,
                ig1_categoryname = "Labor",
                ig1_productname = "PM Labor",
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_projectlu' />
                                <attribute name='ig1_materialcost' />
                                <attribute name='ig1_category' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d02e6d25-d104-ea11-a811-000d3a55dce2*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                  <condition attribute='ig1_productname' operator='neq' value='{fetchData.ig1_productname/*PM Labor*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var item in entityCollection.Entities)
                {
                    decimal materialCost = Convert.ToDecimal(0);
                    decimal labotUnit = Convert.ToDecimal(0);
                    decimal laborRate = Convert.ToDecimal(0);

                    var result = item.Attributes;
                    if (result.Contains("ig1_materialcost") && result["ig1_materialcost"]!=null)
                    {
                        Money money = (Money)result["ig1_materialcost"];
                        materialCost = Convert.ToDecimal(money.Value);
                    }
                    if (result.Contains("ig1_projectlu") && result["ig1_projectlu"] != null)
                    {
                        labotUnit = Convert.ToDecimal(result["ig1_projectlu"]);
                    }
                    if (result.Contains("ig1_category") && result["ig1_category"]!=null)
                    {
                        EntityReference entityReference = (EntityReference)result["ig1_category"];
                        laborRate = LaborRate(bidsheetid, entityReference.Id);
                    }

                    laborCost += materialCost * labotUnit * laborRate;
                }
            }
            return laborCost;
        }
        protected decimal LaborRate(Guid bidsheet, Guid categoryid)
        {
            decimal laborRate = Convert.ToDecimal(0);
            var fetchData = new
            {
                statecode = "0",
                ig1_bidsheet = bidsheet,
                ig1_bidsheetcategory = categoryid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_associatedcost'>
                                <attribute name='ig1_laborrate' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_bidsheet' operator='eq' value='{fetchData.ig1_bidsheet/*d02e6d25-d104-ea11-a811-000d3a55dce2*/}'/>
                                  <condition attribute='ig1_bidsheetcategory' operator='eq' value='{fetchData.ig1_bidsheetcategory/*ig1_bidsheetcategory*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                var result = entityCollection.Entities[0].Attributes;
                if (result.Contains("ig1_laborrate") && result["ig1_laborrate"] != null)
                {
                    laborRate = Convert.ToDecimal(result["ig1_laborrate"]);
                }
            }
            else
            {
                laborRate = DefaultLaborRate();
            }
            return laborRate;
        }
        protected Guid LaborCategory()
        {
            Guid categoryid = Guid.Empty;
            var fetchData = new
            {
                statecode = "0",
                ig1_name = "Labor"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetcategory'>
                                <attribute name='ig1_bidsheetcategoryid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_name' operator='eq' value='{fetchData.ig1_name/*Labor*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                categoryid = entityCollection.Entities[0].Id;
            }
            else
            {
                Entity record = new Entity("ig1_bidsheetcategory");
                record.Attributes["ig1_name"] = "Labor";
                record.Attributes["ig1_defaultmatcostmargin"] = Convert.ToDecimal(0);
                record.Attributes["ig1_laborunit"] = Convert.ToDecimal(0);
                categoryid = service.Create(record);
            }
            return categoryid;
        }
        protected Guid LaborProduct()
        {
            Guid productid = Guid.Empty;
            var fetchData = new
            {
                productnumber = "PML001",
                ig1_bidsheetcategoryname = "Labor",
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='product'>
                                <attribute name='productid' />
                                <attribute name='productnumber' />
                                <filter type='and'>
                                  <condition attribute='productnumber' operator='eq' value='{fetchData.productnumber/*PML001*/}'/>
                                  <condition attribute='ig1_bidsheetcategoryname' operator='eq' value='{fetchData.ig1_bidsheetcategoryname/*Labor*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                productid = entityCollection.Entities[0].Id;
            }
            else
            {
                Guid categoryid = LaborCategory();
                Entity product = new Entity("product");
                product.Attributes["defaultuomid"] = new EntityReference("uom", new Guid("d8e7bc9c-47cb-49ce-b226-4fa95e52da93"));
                product.Attributes["defaultuomscheduleid"] = new EntityReference("uomschedule", new Guid("ef1c2bdc-945b-41e7-84b3-fb8ed610c5bd"));
                if (categoryid != Guid.Empty)
                {
                    product.Attributes["ig1_bidsheetcategory"] = new EntityReference("ig1_bidsheetcategory", categoryid);
                }
                product.Attributes["name"] = "PM Labor";
                product.Attributes["productnumber"] = "PML001";
                product.Attributes["ig1_projectlu"] = Convert.ToDecimal(0);
                product.Attributes["ig1_freight"] = new Money(0);
                product.Attributes["ig1_taxcode"] = "SP156226";
                productid = service.Create(product);
            }
            return productid;
        }

        protected decimal DefaultLaborRate()
        {
            decimal defaultLaborRate = Convert.ToDecimal(0);
            var fetchData = new
            {
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_projectcostallowances'>
                                <attribute name='ig1_defaultlaborrate' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                var result = entityCollection.Entities[0].Attributes;
                if (result.Contains("ig1_defaultlaborrate") && result["ig1_defaultlaborrate"] != null)
                {
                    defaultLaborRate = Convert.ToDecimal(result["ig1_defaultlaborrate"]);
                }
            }
            return defaultLaborRate;
        }
    }
}
