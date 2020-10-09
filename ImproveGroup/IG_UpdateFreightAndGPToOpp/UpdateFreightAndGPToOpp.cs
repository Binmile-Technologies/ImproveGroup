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
                            Entity opp = service.Retrieve(oppRef.LogicalName, oppRef.Id, new ColumnSet("ig1_totalgrossprofit"));
                            Money totalGP = TotalGP(oppRef.Id);
                            if (totalGP != null)
                            {
                                opp.Attributes["ig1_totalgrossprofit"] = totalGP;
                            }
                            else
                            {
                                opp.Attributes["ig1_totalgrossprofit"] = null;
                            }
                            Money freightTotal = FreightTotal(entity.Id);
                            if (freightTotal != null)
                            {
                                opp.Attributes["freightamount"] = freightTotal;
                            }
                            else
                            {
                                opp.Attributes["freightamount"] = null;
                            }
                            decimal bidSheetTotal = BidSheetTotal(oppRef.Id);
                            opp.Attributes["ig1_bidsheettotal"] = bidSheetTotal;
                            service.Update(opp);
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in BidSheetGrossProfitToOpportunity Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected Money TotalGP(Guid oppId)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = oppId,
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
                return new Money(totalGP);
            }
            else
            {
                return new Money(0);
            }
        }
        protected Money FreightTotal(Guid bidSheetId)
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
                return new Money(freightTotal);
            }
            else
            {
                return new Money(0);
            }
        }
        protected decimal BidSheetTotal(Guid oppId)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = oppId,
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
                return bidsheetTotal;
            }
            else
            {
                return Convert.ToDecimal(0);
            }
        }
    }
}
