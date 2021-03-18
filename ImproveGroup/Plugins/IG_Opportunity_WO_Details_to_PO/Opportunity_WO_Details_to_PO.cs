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
                            UpdateOpportunityOwnerInPObill(entity.Id, opportunityOwner);
                        }
                    }
                    else if (entity.LogicalName == "msdyn_workorder")
                    {
                        Entity wo = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ownerid"));
                        if (wo.Attributes.Contains("ownerid") && wo.Attributes["ownerid"] != null)
                        {

                            EntityReference workOrderOwner = (EntityReference)wo.Attributes["ownerid"];
                            UpdateWorkOrderOwner(entity.Id, workOrderOwner);
                            UpdateWorkOrderOwnerInPoBill(entity.Id, workOrderOwner);
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

                    else if (entity.LogicalName == "msdyn_purchaseorderbill")
                    {
                        Entity pobill = new Entity(entity.LogicalName, entity.Id);
                        EntityReference opportunityOwner = GetOpportunityOwnerforPobill(entity.Id);
                        EntityReference workOrderOwner = GetWorkOwderOwnerforPobill(entity.Id);
                        pobill.Attributes["ig1_opportunityowner"] = opportunityOwner;
                        pobill.Attributes["ig1_workorderowner"] = workOrderOwner;
                        service.Update(pobill);
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
        protected void UpdateOpportunityOwnerInPObill(Guid opportunityid, EntityReference opportunityOwner)
        {

            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorderbill'>
                                <attribute name='msdyn_purchaseorderbillid' />
                                <attribute name='ownerid' />
                                <attribute name='msdyn_name' />
                                <link-entity name='msdyn_purchaseorder' from='msdyn_purchaseorderid' to='msdyn_purchaseorder'>
                                  <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                                    <link-entity name='opportunity' from='opportunityid' to='msdyn_opportunityid'>
                                      <filter>
                                        <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*20df5ea4-6244-47b5-9be2-8220d7c749fb*/}'/>
                                      </filter>
                                    </link-entity>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach(Entity entitypobill in entityCollection.Entities)
                {

                    if(opportunityOwner != null)
                    {
                        entitypobill.Attributes["ig1_opportunityowner"] = opportunityOwner;
                        service.Update(entitypobill);
                    }
                }

            }

        }
        protected void UpdateWorkOrderOwnerInPoBill(Guid workorderid, EntityReference workOrderOwner)
        {

            var fetchData = new
            {
                msdyn_workorderid = workorderid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorderbill'>
                                <attribute name='msdyn_purchaseorderbillid' />
                                <attribute name='ownerid' />
                                <attribute name='msdyn_name' />
                                <link-entity name='msdyn_purchaseorder' from='msdyn_purchaseorderid' to='msdyn_purchaseorder'>
                                  <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                                    <filter>
                                      <condition attribute='msdyn_workorderid' operator='eq' value='{fetchData.msdyn_workorderid/*47c0161d-f682-eb11-a812-000d3a4fe50b*/}'/>
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach(Entity entitypo in entityCollection.Entities)
                {

                    if(workOrderOwner != null)
                    {
                        entitypo.Attributes["ig1_workorderowner"] = workOrderOwner;
                        service.Update(entitypo);
                    }
                }
            }

        }
        protected EntityReference GetOpportunityOwnerforPobill(Guid pobillid)
        {
            EntityReference oppowner=null;
            var fetchData = new
            {
                msdyn_purchaseorderbillid = pobillid
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='opportunity'>
                        <attribute name='ownerid' />
                        <attribute name='owneridname' />
                        <link-entity name='msdyn_workorder' from='msdyn_opportunityid' to='opportunityid'>
                          <link-entity name='msdyn_purchaseorder' from='msdyn_workorder' to='msdyn_workorderid'>
                            <link-entity name='msdyn_purchaseorderbill' from='msdyn_purchaseorder' to='msdyn_purchaseorderid'>
                              <filter>
                                <condition attribute='msdyn_purchaseorderbillid' operator='eq' value='{fetchData.msdyn_purchaseorderbillid/*7f415be4-1a83-eb11-a812-000d3a4fe50b*/}'/>
                              </filter>
                            </link-entity>
                          </link-entity>
                        </link-entity>
                      </entity>
                    </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                Entity poentity = entityCollection.Entities[0];
                if(poentity.Attributes.Contains("ownerid") && poentity.Attributes["ownerid"] != null)
                {

                    oppowner = (EntityReference)poentity.Attributes["ownerid"];

                }
            }
            return oppowner;
        }
        protected EntityReference GetWorkOwderOwnerforPobill(Guid Pobillid)
        {
            EntityReference Woowner = null;
            var fetchData = new
            {
                msdyn_purchaseorderbillid = Pobillid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_workorder'>
                                <attribute name='ownerid' />
                                <link-entity name='msdyn_purchaseorder' from='msdyn_workorder' to='msdyn_workorderid'>
                                  <link-entity name='msdyn_purchaseorderbill' from='msdyn_purchaseorder' to='msdyn_purchaseorderid'>
                                    <filter>
                                      <condition attribute='msdyn_purchaseorderbillid' operator='eq' value='{fetchData.msdyn_purchaseorderbillid/*7f415be4-1a83-eb11-a812-000d3a4fe50b*/}'/>
                                    </filter>
                                  </link-entity>
                                </link-entity>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {

                Entity pobillentity = entityCollection.Entities[0];
                if(pobillentity.Attributes.Contains("ownerid") && pobillentity.Attributes["ownerid"] != null)
                {
                    Woowner = (EntityReference)pobillentity.Attributes["ownerid"];
                }
            };

             return Woowner;
        }
    }

}
