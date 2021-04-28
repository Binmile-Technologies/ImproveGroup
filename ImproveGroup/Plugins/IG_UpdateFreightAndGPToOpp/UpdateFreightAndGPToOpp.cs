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
                if (context.MessageName == "Update" && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
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
                            UpdateBidSheetDetailsToOpportunity(entity, oppRef);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }

        protected void UpdateBidSheetDetailsToOpportunity(Entity bidsheet, EntityReference oppRef)
        {
            decimal freightTotal = FreightTotal(bidsheet.Id);
            decimal totalGP = Convert.ToDecimal(0);
            decimal bidsheetTotal = Convert.ToDecimal(0);

            var fetchData = new
            {
                ig1_opportunitytitle = oppRef.Id,
                ig1_associated = "1"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_anticipatedgp' />
                                <attribute name='ig1_sellprice' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle}'/>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("ig1_anticipatedgp") && entity.Attributes["ig1_anticipatedgp"] != null)
                    {
                        totalGP += Convert.ToDecimal(entity.GetAttributeValue<Money>("ig1_anticipatedgp").Value);
                    }
                    if (entity.Attributes.Contains("ig1_sellprice") && entity.Attributes["ig1_sellprice"] != null)
                    {
                        bidsheetTotal += Convert.ToDecimal(entity.GetAttributeValue<Money>("ig1_sellprice").Value);
                    }
                }
            }
            Entity opportunity = new Entity(oppRef.LogicalName, oppRef.Id);
            opportunity.Attributes["freightamount"] = new Money(freightTotal);
            opportunity.Attributes["ig1_totalgrossprofit"] = new Money(totalGP);
            opportunity.Attributes["ig1_totalsellprice"] = new Money(bidsheetTotal);
            service.Update(opportunity);
        }
        protected decimal FreightTotal(Guid bidSheetId)
        {
            decimal freightTotal = Convert.ToDecimal(0);
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
                foreach (var entity in entityCollection.Entities)
                {
                    var result = entity.Attributes;
                    if (result.Contains("ig1_freighttotal") && result["ig1_freighttotal"] != null)
                    {
                        Money money = (Money)result["ig1_freighttotal"];
                        freightTotal += Convert.ToDecimal(money.Value);
                    }
                }
            }
            return freightTotal;
        }
    }
}
