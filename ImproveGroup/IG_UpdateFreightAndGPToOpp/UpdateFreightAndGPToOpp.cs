using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdateFreightAndGPToOpp
{
    public class UpdateFreightAndGPToOpp : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.UserId);
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "ig1_bidsheet")
                    {
                        return;
                    }
                    Entity bs = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunitytitle"));
                    if (bs.Attributes.Contains("ig1_opportunitytitle") && bs.Attributes["ig1_opportunitytitle"] != null)
                    {
                        EntityReference oppRef = (EntityReference)bs.Attributes["ig1_opportunitytitle"];
                        if (oppRef.Id != Guid.Empty)
                        {
                            Entity opportunity = service.Retrieve(oppRef.LogicalName, oppRef.Id, new ColumnSet("ig1_totalgrossprofit"));
                            FreightTotal(entity.Id, opportunity);
                            TotalGP(opportunity);
                            BidSheetTotal(opportunity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                trace.Trace("UpdateFreightAndGPToOpp lugin Exception");
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }
        protected void TotalGP(Entity opportunity)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = opportunity.Id,
                ig1_associated = "1"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_anticipatedgp' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*76e041d1-fad2-e911-a967-000d3a1d57cf*/}'/>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                decimal totalGP = Convert.ToDecimal(0);
                foreach (var entity in entityCollection.Entities)
                {
                    var result = entity.Attributes;
                    if (result.Contains("ig1_anticipatedgp") && result["ig1_anticipatedgp"] != null)
                    {
                        Money gp = (Money)result["ig1_anticipatedgp"];
                        totalGP += Convert.ToDecimal(gp.Value);
                    }
                }
                opportunity.Attributes["ig1_totalgrossprofit"] = new Money(totalGP);
                service.Update(opportunity);
            }
            else
            {
                opportunity.Attributes["ig1_totalgrossprofit"] = new Money(0);
                service.Update(opportunity);
            }
        }
        protected void FreightTotal(Guid bidSheetId, Entity opportunity)
        {
            var fetchData = new
            {
                ig1_categoryname = "Labor",
                ig1_categoryname2 = "Contingency",
                ig1_status = "286150000",
                ig1_bidsheetid = bidSheetId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheetpricelistitem'>
                                <attribute name='ig1_freighttotal' />
                                <filter type='and'>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname/*Labor*/}'/>
                                  <condition attribute='ig1_categoryname' operator='neq' value='{fetchData.ig1_categoryname2/*Contingency*/}'/>
                                </filter>
                                <link-entity name='ig1_bidsheet' from='ig1_bidsheetid' to='ig1_bidsheet'>
                                  <filter type='and'>
                                    <condition attribute='ig1_status' operator='eq' value='{fetchData.ig1_status/*286150000*/}'/>
                                    <condition attribute='ig1_bidsheetid' operator='eq' value='{fetchData.ig1_bidsheetid/*29e4477b-1295-ea11-a811-000d3a98d1ad*/}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                decimal freightTotal = Convert.ToDecimal(0);
                foreach (var entity in entityCollection.Entities)
                {
                    var result = entity.Attributes;
                    if (result.Contains("ig1_freighttotal") && result["ig1_freighttotal"] != null)
                    {
                        Money money = (Money)result["ig1_freighttotal"];
                        freightTotal += Convert.ToDecimal(money.Value);
                    }
                }
                opportunity.Attributes["freightamount"] = new Money(freightTotal);
                service.Update(opportunity);
            }
        }
        protected void BidSheetTotal(Entity opportunity)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = opportunity.Id,
                ig1_associated = "1"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_sellprice' />
                                <attribute name='ig1_associated' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*a50869b6-e606-ea11-a811-000d3a55d2c3*/}'/>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                decimal bidsheetTotal = Convert.ToDecimal(0);
                foreach (var entity in entityCollection.Entities)
                {
                    var result = entity.Attributes;
                    if (result.Contains("ig1_sellprice") && result["ig1_sellprice"] != null)
                    {
                        Money money = (Money)result["ig1_sellprice"];
                        bidsheetTotal += Convert.ToDecimal(money.Value);
                    }
                }
                opportunity.Attributes["ig1_bidsheettotal"] = bidsheetTotal;
                service.Update(opportunity);
            }
            else
            {
                opportunity.Attributes["ig1_bidsheettotal"] = Convert.ToDecimal(0);
                service.Update(opportunity);
            }
        }
    }
}
