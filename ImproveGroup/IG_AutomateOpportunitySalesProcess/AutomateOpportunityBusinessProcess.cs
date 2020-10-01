using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_AutomateOpportunitySalesProcess
{
    public class AutomateOpportunityBusinessProcess : IPlugin
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

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];

                    if (entity.LogicalName == "opportunity")
                    {
                        Entity opportunity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_customerponumber"));
                        if (opportunity.Attributes.Contains("ig1_customerponumber") && opportunity.Attributes["ig1_customerponumber"] != null)
                        {
                            entity.Attributes["new_porecieved"] = true;
                            service.Update(entity);
                        }
                        else
                        {
                            entity.Attributes["new_porecieved"] = false;
                            service.Update(entity);
                        }
                    }
                    else if (entity.LogicalName == "ig1_bidsheet")
                    {
                        Entity bidsheet = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunitytitle"));
                        if (bidsheet.Attributes.Contains("ig1_opportunitytitle") && bidsheet.Attributes["ig1_opportunitytitle"] != null)
                        {
                            EntityReference entityReference = (EntityReference)bidsheet.Attributes["ig1_opportunitytitle"];
                            Guid opportunityid = entityReference.Id;
                            BidSheet(opportunityid);
                        }
                    }
                    else if (entity.LogicalName == "quote")
                    {
                        Entity quote = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("opportunityid"));
                        if (quote.Attributes.Contains("opportunityid") && quote.Attributes["opportunityid"] != null)
                        {
                            EntityReference entityReference = (EntityReference)quote.Attributes["opportunityid"];
                            Guid opportunityid = entityReference.Id;
                            Quote(opportunityid);
                        }
                    }
                }
                else if (context.MessageName.ToLower() == "delete" && context.PreEntityImages.Contains("Image"))
                {
                    Entity entity = (Entity)context.PreEntityImages["Image"];
                    if (entity.LogicalName == "quote")
                    {
                        if (entity.Attributes.Contains("opportunityid") && entity.Attributes["opportunityid"] != null)
                        {
                            EntityReference entityReference = (EntityReference)entity.Attributes["opportunityid"];
                            Guid opportunityid = entityReference.Id;
                            Quote(opportunityid);

                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in AutomateOpportunityBusinessProcess Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected void BidSheet(Guid opportunityid)
        {
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityid,
                ig1_associated = "1"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_status' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*58709a69-0861-42e1-b5fc-392213d2f683*/}'/>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity entity = service.Retrieve("opportunity", opportunityid, new ColumnSet("new_designready"));
            if (entityCollection.Entities.Count > 0)
            {
                entity.Attributes["new_designready"] = true;
                service.Update(entity);
            }
            else
            {
                entity.Attributes["new_designready"] = false;
                service.Update(entity);
            }
        }
        protected void Quote(Guid opportunityid)
        {
            var fetchData = new
            {
                opportunityid = opportunityid,
                statecode = "1",
                statecode2 = "2"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='quote'>
                                <attribute name='name' />
                                <filter type='and'>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*opportunity*/}'/>
                                  <filter type='or'>
                                    <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*1*/}'/>
                                    <condition attribute='statecode' operator='eq' value='{fetchData.statecode2/*2*/}'/>
                                  </filter>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Entity entity = service.Retrieve("opportunity", opportunityid, new ColumnSet("developproposal"));
            if (entityCollection.Entities.Count > 0)
            {
                entity.Attributes["developproposal"] = true;
                service.Update(entity);
            }
            else
            {
                entity.Attributes["developproposal"] = false;
                service.Update(entity);
            }
        }
    }
}
