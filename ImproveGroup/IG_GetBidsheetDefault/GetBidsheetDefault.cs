using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace IG_GetBidsheetDefault
{
    public class GetBidsheetDefault : IPlugin
    {
        IOrganizationService service;
        IOrganizationServiceFactory serviceFactory;
        IPluginExecutionContext context;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
                if(context.MessageName=="Create" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "ig1_bidsheet")
                    {
                        FetchAndSaveBidsheetDefaultToRecord(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in GetBidsheetDefault plugin " + ex);
            }
        }
        protected void FetchAndSaveBidsheetDefaultToRecord(Entity entity)
        {
            decimal designFactor = Convert.ToDecimal(0);
            decimal designLaborRate = Convert.ToDecimal(0);
            decimal designMargin = Convert.ToDecimal(0);

            decimal salesFactor = Convert.ToDecimal(0);
            decimal salesLaborRate = Convert.ToDecimal(0);
            decimal salesMargin = Convert.ToDecimal(0);

            decimal laborRate = Convert.ToDecimal(0);
            decimal laborMargin = Convert.ToDecimal(0);
            decimal margin = Convert.ToDecimal(0);

            decimal travelMargin = Convert.ToDecimal(0);
            decimal lodging = Convert.ToDecimal(0);
            decimal perDiem = Convert.ToDecimal(0);

            decimal corpGNA = FetchCorpGNA();

            var fetchData = new
            {
                statecode = "0"
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='ig1_projectcostallowances'>
                            <attribute name='ig1_designfactor' />
                            <attribute name='ig1_salesfactor' />
                            <attribute name='ig1_labormargin' />
                            <attribute name='ig1_travelmargin' />
                            <attribute name='ig1_designlaborrate' />
                            <attribute name='ig1_designmargin' />
                            <attribute name='ig1_perdiem' />
                            <attribute name='ig1_saleslaborrate' />
                            <attribute name='ig1_salesmargin' />
                            <attribute name='ig1_margin' />
                            <attribute name='ig1_defaultlaborrate' />
                            <attribute name='ig1_lodging' />
                            <filter type='and'>
                              <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                            </filter>
                          </entity>
                        </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                var result = entityCollection.Entities[0].Attributes;

                if (result.Contains("ig1_designfactor") && result["ig1_designfactor"] != null)
                {
                    designFactor = Math.Round(Convert.ToDecimal(result["ig1_designfactor"]), 2);
                }
                if (result.Contains("ig1_designlaborrate") && result["ig1_designlaborrate"] != null)
                {
                    Money money = (Money)result["ig1_designlaborrate"];
                    designLaborRate = Math.Round(Convert.ToDecimal(money.Value), 2);
                }
                if (result.Contains("ig1_designmargin") && result["ig1_designmargin"] != null)
                {
                    designMargin = Math.Round(Convert.ToDecimal(result["ig1_designmargin"]), 2);
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
                if (result.Contains("ig1_salesmargin") && result["ig1_salesmargin"] != null)
                {
                    salesMargin = Math.Round(Convert.ToDecimal(result["ig1_salesmargin"]), 2);
                }
                if (result.Contains("ig1_defaultlaborrate") && result["ig1_defaultlaborrate"] != null)
                {
                    laborRate = Math.Round(Convert.ToDecimal(result["ig1_defaultlaborrate"]), 2);
                }
                if (result.Contains("ig1_labormargin") && result["ig1_labormargin"] != null)
                {
                    laborMargin = Math.Round(Convert.ToDecimal(result["ig1_labormargin"]), 2);
                }
                if (result.Contains("ig1_margin") && result["ig1_margin"] != null)
                {
                    margin = Math.Round(Convert.ToDecimal(result["ig1_margin"]), 2);
                }
                if (result.Contains("ig1_travelmargin") && result["ig1_travelmargin"] != null)
                {
                    travelMargin = Math.Round(Convert.ToDecimal(result["ig1_travelmargin"]), 2);
                }
                if (result.Contains("ig1_lodging") && result["ig1_lodging"] != null)
                {
                    lodging = Math.Round(Convert.ToDecimal(result["ig1_lodging"]));
                }
                if (result.Contains("ig1_perdiem") && result["ig1_perdiem"] != null)
                {
                    Money money = (Money)result["ig1_perdiem"];
                    perDiem = Math.Round(Convert.ToDecimal(money.Value), 2);
                }
            }

            entity.Attributes["ig1_defaultdesignfactor"] = designFactor;
            entity.Attributes["ig1_defaultdesignlaborrate"] = new Money(designLaborRate);
            entity.Attributes["ig1_defaultdesignmargin"] = designMargin;

            entity.Attributes["ig1_defaultsalesfactor"] = salesFactor;
            entity.Attributes["ig1_defaultsaleslaborrate"] = new Money(salesLaborRate);
            entity.Attributes["ig1_defaultsalesmargin"] = salesMargin;

            entity.Attributes["ig1_defaultlaborrate"] = laborRate;
            entity.Attributes["ig1_defaultlabormargin"] = laborMargin;

            entity.Attributes["ig1_defaulttravelmargin"] = travelMargin;
            entity.Attributes["ig1_defaultlodging"] = lodging;
            entity.Attributes["ig1_defaultperdiem"] = new Money(lodging);

            entity.Attributes["ig1_defaultmargin"] = margin;
            entity.Attributes["ig1_defaultcorpgna"] = corpGNA;
            service.Update(entity);
        }
        protected decimal FetchCorpGNA()
        {
            decimal corpGNA = Convert.ToDecimal(0);
            var fetchData = new
            {
                ig1_year = DateTime.Now.Year,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_corpgna'>
                                <attribute name='ig1_corpgnavalue' />
                                <filter type='and'>
                                  <condition attribute='ig1_year' operator='this-year'>
                                    <value>{fetchData.ig1_year/*2020*/}</value>
                                  </condition>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                var result = entityCollection.Entities[0].Attributes;
                if (result.Contains("ig1_corpgnavalue") && result["ig1_corpgnavalue"] != null)
                {
                    corpGNA = Math.Round(Convert.ToDecimal(result["ig1_corpgnavalue"]), 2); 
                }
            }
            return corpGNA;  
        }
    }
}
