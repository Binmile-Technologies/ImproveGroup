using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace ImproveGroup
{
    public class GetOpportunityDetailsToQuote : IPlugin
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
                service = serviceFactory.CreateOrganizationService(null);
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "opportunity")
                    {
                        QueryExpression query = new QueryExpression("quote");
                        query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                        query.Criteria.AddCondition("opportunityid", ConditionOperator.Equal, entity.Id);
                        EntityCollection entityCollection = service.RetrieveMultiple(query);

                        if (entityCollection.Entities.Count > 0)
                        {
                            var oppDetails = GetOppDetails(entity.Id);
                            foreach (var quote in entityCollection.Entities)
                            {
                                if (oppDetails[0] != -1)
                                {
                                    quote.Attributes["ig1_leadtimeinstallation"] = oppDetails[0];
                                }
                                else
                                {
                                    quote.Attributes["ig1_leadtimeinstallation"] = 0;
                                }
                                if (oppDetails[1] != -1)
                                {
                                    quote.Attributes["ig1_leadtimematerials"] = oppDetails[1];
                                }
                                else
                                {
                                    quote.Attributes["ig1_leadtimematerials"] = 0;
                                }
                                service.Update(quote);
                            }
                        }

                    }
                    else if (entity.LogicalName == "quote")
                    {
                        Entity quote= service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("opportunityid"));
                        if (quote.Attributes.Contains("opportunityid") && quote.Attributes["opportunityid"] != null)
                        {
                            EntityReference opportunity = (EntityReference)quote.Attributes["opportunityid"];
                            if (opportunity != null)
                            {
                                var oppDetails = GetOppDetails(opportunity.Id);
                                if (oppDetails[0] != -1)
                                {
                                    quote.Attributes["ig1_leadtimeinstallation"] = oppDetails[0];
                                }
                                else
                                {
                                    quote.Attributes["ig1_leadtimeinstallation"] = 0;
                                }
                                if (oppDetails[1] != -1)
                                {
                                    quote.Attributes["ig1_leadtimematerials"] = oppDetails[1];
                                }
                                else
                                {
                                    quote.Attributes["ig1_leadtimematerials"] = 0;
                                }
                                service.Update(quote);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in GetOpportunityDetailsToQuote Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected int[] GetOppDetails(Guid oppId)
        {
            int[] arr = new int[2];
            var fetchData = new
            {
                statecode = "0",
                opportunityid = oppId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='opportunity'>
                                <attribute name='ig1_leadtimeinstallation' />
                                <attribute name='ig1_leadtimematerials' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b922f690-976e-491f-8088-071cf0a89110*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (var entity in entityCollection.Entities)
                {
                    var result = entity.Attributes;
                    if (result.Contains("ig1_leadtimeinstallation") && result["ig1_leadtimeinstallation"] != null)
                    {
                        arr[0] = Convert.ToInt32(result["ig1_leadtimeinstallation"]);
                    }
                    else
                    {
                        arr[0] = -1;
                    }
                    if (result.Contains("ig1_leadtimematerials") && result["ig1_leadtimematerials"] != null)
                    {
                        arr[1] = Convert.ToInt32(result["ig1_leadtimematerials"]);
                    }
                    else
                    {
                        arr[1] = -1;
                    }
                }
            }
            return arr;
        }
    }
}
