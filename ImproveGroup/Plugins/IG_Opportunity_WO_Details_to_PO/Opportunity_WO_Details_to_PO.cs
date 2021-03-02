using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_Opportunity_WO_Details_to_PO
{
    public class Opportunity_WO_Details_to_PO : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationService service;
        IOrganizationServiceFactory serviceFactory;
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
                    if (entity.LogicalName == "opportunity")
                    {
                        Entity opp = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ownerid"));
                        if (opp.Attributes.Contains("ownerid") && opp.Attributes["ownerid"] != null)
                        {
                            EntityReference opportunityOwner = (EntityReference)opp.Attributes["ownerid"];
                            UpdateOpportunityOwner(entity.Id, opportunityOwner);
                        }
                    }
                    else if (entity.LogicalName == "msdyn_workorder")
                    {
                        Entity wo = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ownerid"));
                        if (wo.Attributes.Contains("ownerid") && wo.Attributes["ownerid"] != null)
                        {

                            EntityReference workOrderOwner = (EntityReference)wo.Attributes["ownerid"];
                            UpdateWorkOrderOwner(entity.Id, workOrderOwner);
                        }
                    }
                    else if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        Entity po = new Entity(entity.LogicalName, entity.Id);
                        EntityReference opportunityOwner = GetOpportunityOwner(entity.Id);
                        EntityReference workOrderOwner = GetWorkOrderOwner(entity.Id);
                        po.Attributes["ig1_opportunityowner"] = opportunityOwner;
                        po.Attributes["ig1_workorderowner"] = workOrderOwner;
                        service.Update(po);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Erron in Opportunity_WO_Details_to_PO Plugin " + ex);
            }
        }
        protected void UpdateOpportunityOwner(Guid opportunityid, EntityReference opportunityOwner)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorder'>
                                <attribute name='msdyn_purchaseorderid' />
                                <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                                  <link-entity name='opportunity' from='opportunityid' to='msdyn_opportunityid'>
                                    <filter>
                                      <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*00000000-0000-0000-0000-000000000000*/}'/>
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (opportunityOwner != null)
                    {
                        entity.Attributes["ig1_opportunityowner"] = opportunityOwner;
                        service.Update(entity);
                    }
                }
            }
        }
        protected void UpdateWorkOrderOwner(Guid workorderid, EntityReference workOrderOwner)
        {
            var fetchData = new
            {
                msdyn_workorderid = workorderid
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='msdyn_purchaseorder'>
                                <attribute name='msdyn_purchaseorderid' />
                                <attribute name='ownerid' />
                                <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                                    <filter>
                                    <condition attribute='msdyn_workorderid' operator='eq' value='{fetchData.msdyn_workorderid/*00000000-0000-0000-0000-000000000000*/}'/>
                                    </filter>
                                </link-entity>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (workOrderOwner != null)
                    {
                        entity.Attributes["ig1_workorderowner"] = workOrderOwner;
                        service.Update(entity);
                    }
                }
            }
        }
        protected EntityReference GetOpportunityOwner(Guid poid)
        {
            EntityReference owner = null;
            var fetchData = new
            {
                msdyn_purchaseorderid = poid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='opportunity'>
                                <attribute name='ownerid' />
                                <link-entity name='msdyn_workorder' from='msdyn_opportunityid' to='opportunityid'>
                                  <link-entity name='msdyn_purchaseorder' from='msdyn_workorder' to='msdyn_workorderid'>
                                    <filter>
                                      <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*00000000-0000-0000-0000-000000000000*/}'/>
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                if (entity.Attributes.Contains("ownerid") && entity.Attributes["ownerid"] != null)
                {
                    owner = (EntityReference)entity.Attributes["ownerid"];
                }
            }
            return owner;
        }
        protected EntityReference GetWorkOrderOwner(Guid poid)
        {
            EntityReference owner = null;
            var fetchData = new
            {
                msdyn_purchaseorderid = poid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_workorder'>
                                <attribute name='ownerid' />
                                <link-entity name='msdyn_purchaseorder' from='msdyn_workorder' to='msdyn_workorderid'>
                                  <filter>
                                    <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*00000000-0000-0000-0000-000000000000*/}'/>
                                  </filter>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                if (entity.Attributes.Contains("ownerid") && entity.Attributes["ownerid"] != null)
                {
                    owner = (EntityReference)entity.Attributes["ownerid"];
                }
            }
            return owner;
        }
    }

}
