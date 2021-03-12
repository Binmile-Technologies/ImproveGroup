using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdateProjectRecordToEntities
{
    public class UpdateProjectRecordToEntities : IPlugin
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
                    if (entity.LogicalName == "ig1_bidsheet")
                    {
                        Entity bidsheet = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunitytitle"));
                        if (bidsheet.Attributes.Contains("ig1_opportunitytitle") && bidsheet.Attributes["ig1_opportunitytitle"] != null)
                        {
                            Guid opportunityid = bidsheet.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
                            Guid projectid = GetProjectRecordId(opportunityid);
                            if (projectid != Guid.Empty)
                            {
                                UpdateRecord(projectid, bidsheet);
                            }
                        }
                    }
                    else if (entity.LogicalName == "quote")
                    {
                        Entity quote = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("opportunityid"));
                        if (quote.Attributes.Contains("opportunityid") && quote.Attributes["opportunityid"] != null)
                        {
                            Guid opportunityid = quote.GetAttributeValue<EntityReference>("opportunityid").Id;
                            Guid projectid = GetProjectRecordId(opportunityid);
                            if (projectid != Guid.Empty)
                            {
                                UpdateRecord(projectid, quote);
                            }
                        }
                    }
                    else if (entity.LogicalName == "salesorder")
                    {
                        Entity order = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("opportunityid"));
                        if (order.Attributes.Contains("opportunityid") && order.Attributes["opportunityid"] != null)
                        {
                            Guid opportunityid = order.GetAttributeValue<EntityReference>("opportunityid").Id;
                            Guid projectid = GetProjectRecordId(opportunityid);
                            if (projectid != Guid.Empty)
                            {
                                UpdateRecord(projectid, order);
                            }
                        }
                    }
                    else if (entity.LogicalName == "invoice")
                    {
                        Entity invoice = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("opportunityid"));
                        if (invoice.Attributes.Contains("opportunityid") && invoice.Attributes["opportunityid"] != null)
                        {
                            Guid opportunityid = invoice.GetAttributeValue<EntityReference>("opportunityid").Id;
                            Guid projectid = GetProjectRecordId(opportunityid);
                            if (projectid != Guid.Empty)
                            {
                                UpdateRecord(projectid, invoice);
                            }
                        }
                    }
                    else if (entity.LogicalName == "msdyn_workorder")
                    {
                        Entity wo = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_opportunityid"));
                        if (wo.Attributes.Contains("msdyn_opportunityid") && wo.Attributes["msdyn_opportunityid"] != null)
                        {
                            Guid opportunityid = wo.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                            Guid projectid = GetProjectRecordId(opportunityid);
                            if (projectid != Guid.Empty)
                            {
                                UpdateRecord(projectid, wo);
                            }
                        }
                    }
                    else if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        Guid opportunityid = Guid.Empty;
                        Guid projectid = Guid.Empty;
                        Entity po = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_workorder", "ig1_opportunityid"));
                        if (po.Attributes.Contains("ig1_opportunityid") && po.Attributes["ig1_opportunityid"] != null)
                        {
                            opportunityid = po.GetAttributeValue<EntityReference>("ig1_opportunityid").Id;
                            projectid = GetProjectRecordId(opportunityid);
                        }
                        else if (po.Attributes.Contains("msdyn_workorder") && po.Attributes["msdyn_workorder"] != null)
                        {
                            EntityReference entityReference = (EntityReference)po.Attributes["msdyn_workorder"];
                            Entity wo = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet("msdyn_opportunityid"));
                            if (wo.Attributes.Contains("msdyn_opportunityid") && wo.Attributes["msdyn_opportunityid"] != null)
                            {
                                opportunityid = wo.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                                projectid = GetProjectRecordId(opportunityid);
                            }
                        }
                        if (projectid != Guid.Empty)
                        {
                            UpdateRecord(projectid, entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in UpdateProjectRecordToEntities Plugin " + ex);
            }
        }
        protected Guid GetProjectRecordId(Guid opportunityid)
        {
            Guid projectrecordid = Guid.Empty;
            var fetchData = new
            {
                statecode = "0",
                ig1_opportunity = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectnumber' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                projectrecordid = entity.Id;
            }
            return projectrecordid;
        }

        protected void UpdateRecord(Guid projectid, Entity entity)
        {
            Entity record = new Entity(entity.LogicalName, entity.Id);
            record.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectid);
            service.Update(record);

        }
    }
}
