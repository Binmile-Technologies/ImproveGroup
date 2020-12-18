using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdateDetailsFromAccountToOpportunity
{
    public class UpdateDetailsFromAccountToOpportunity : IPlugin
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
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "account")
                    {
                        EntityReference baseLocation = null;
                        if (entity.Attributes.Contains("ig1_baselocation") && entity.Attributes["ig1_baselocation"] != null)
                        {
                            baseLocation = (EntityReference)entity.Attributes["ig1_baselocation"];
                        }
                        UpdateOpportunityBaseLocation(entity.Id, baseLocation);
                    }
                    else if (entity.LogicalName == "opportunity")
                    {
                        if (entity.Attributes.Contains("parentaccountid") && entity.Attributes["parentaccountid"] != null)
                        {
                            EntityReference entityReference = (EntityReference)entity.Attributes["parentaccountid"];
                            EntityReference baseLocation = GetAccountBseLocation(entityReference.Id);
                            entity.Attributes["ig1_baselocation"] = baseLocation;
                            service.Update(entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in UpdateDetailsFromAccountToOpportunity Plug-in " + ex);
            }
        }
        protected void UpdateOpportunityBaseLocation(Guid accountid, EntityReference baseLocation)
        {
            var fetchData = new
            {
                statecode = "0",
                statecode2 = "0",
                accountid = accountid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='opportunity'>
                                <attribute name='ig1_projectnumber' />
                                <attribute name='ig1_baselocation' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                                <link-entity name='account' from='accountid' to='parentaccountid'>
                                  <filter type='and'>
                                    <condition attribute='statecode' operator='eq' value='{fetchData.statecode2/*0*/}'/>
                                    <condition attribute='accountid' operator='eq' value='{fetchData.accountid/*824859ec-efce-e911-a95e-000d3a110bbd*/}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    entity.Attributes["ig1_baselocation"] =new EntityReference(baseLocation.LogicalName, baseLocation.Id);
                    service.Update(entity);
                }
            }
        }
        protected EntityReference GetAccountBseLocation(Guid accountid)
        {
            EntityReference baseLocation = null;
            var fetchData = new
            {
                statecode = "0",
                accountid = accountid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='account'>
                                <attribute name='ig1_baselocation' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='accountid' operator='eq' value='{fetchData.accountid/*824859ec-efce-e911-a95e-000d3a110bbd*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                if (entity.Attributes.Contains("ig1_baselocation") && entity.Attributes["ig1_baselocation"] != null)
                {
                    EntityReference entityrence = (EntityReference)entity.Attributes["ig1_baselocation"];
                    baseLocation = new EntityReference(entityrence.LogicalName, entityrence.Id);
                }
            }
            return baseLocation;
        }
    }
}
