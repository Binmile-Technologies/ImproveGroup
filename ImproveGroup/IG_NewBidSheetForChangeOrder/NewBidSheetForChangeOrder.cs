using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_NewBidSheetForChangeOrder
{
    public class NewBidSheetForChangeOrder : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracing;
        IOrganizationServiceFactory servicefactory;
        IOrganizationService service;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = servicefactory.CreateOrganizationService(context.InitiatingUserId);
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "ig1_bidsheet")
                    {
                        return;
                    }

                    Entity bs = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_status", "ig1_opportunitytitle"));
                    EntityReference opportunity = null;
                    OptionSetValue status = null;
                    if (bs.Attributes.Contains("ig1_opportunitytitle") && bs.Attributes["ig1_opportunitytitle"] != null)
                    {
                        opportunity = (EntityReference)entity.Attributes["ig1_opportunitytitle"];
                    }
                    if (bs.Attributes.Contains("ig1_status") && bs.Attributes["ig1_status"] != null)
                    {
                        status = (OptionSetValue)bs.Attributes["ig1_status"];
                    }
                    if (opportunity != null && status.Value != Convert.ToInt32("286150003"))
                    {
                        if (opportunity.Id != Guid.Empty)
                        {
                            UpdateUpperRevisionId(opportunity.Id, entity.Id);
                            UpdateProjectNumber(opportunity.Id, entity.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                trace.Trace("NewBidSheetForChangeOrderP lugin Exception");
                throw new InvalidPluginExecutionException("Error " + ex);
            }
        }

        protected int MaxUpperRevisionId(Guid opportunityId)
        {
            int maxUpperRevisionId = 0;
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityId
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_upperrevisionid'/>
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*8b4029e1-1a06-ea11-a811-000d3a55d0f0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                foreach (var result in ec.Entities)
                {
                    if (result.Attributes.Contains("ig1_upperrevisionid") && result.Attributes["ig1_upperrevisionid"]!=null)
                    {
                        if ((int)result.Attributes["ig1_upperrevisionid"] > maxUpperRevisionId)
                        {
                            maxUpperRevisionId = (int)result.Attributes["ig1_upperrevisionid"];
                        }
                    }
                }
            }

            return maxUpperRevisionId;
        }
        protected void UpdateUpperRevisionId(Guid opportunityId, Guid bidSheetId)
        {
            var maxUpperRevisionId = MaxUpperRevisionId(opportunityId);
            var maxRevisionId = MaxRevisionId(opportunityId, maxUpperRevisionId);
            Entity entity = service.Retrieve("ig1_bidsheet", bidSheetId, new ColumnSet("ig1_upperrevisionid", "ig1_iscreateorder"));
            bool par = entity.GetAttributeValue<bool>("ig1_iscreateorder");
            if (!entity.Attributes.Contains("ig1_upperrevisionid") || entity.Attributes["ig1_upperrevisionid"] == null)
            {
                if (par == true)
                {
                    entity["ig1_upperrevisionid"] = maxUpperRevisionId + 1;
                    entity["ig1_revisionid"] = 0;
                    service.Update(entity);
                }
                else
                {
                    entity["ig1_upperrevisionid"] = maxUpperRevisionId;
                    entity["ig1_revisionid"] = maxRevisionId + 1;
                    service.Update(entity);
                }
            }
        }
        protected void UpdateProjectNumber(Guid opportunityId, Guid bidSheetId)
        {
            string projectNumber = GetProjectNumber(opportunityId);
            Entity entity = service.Retrieve("ig1_bidsheet", bidSheetId, new ColumnSet("ig1_projectnumber"));
            if (projectNumber!="" && projectNumber!=null)
            {
                entity["ig1_projectnumber"] = projectNumber;
                service.Update(entity);
            }
        }
        protected string GetProjectNumber(Guid opportunityId)
        {
            string projectNumber = "";
            Entity entity = service.Retrieve("opportunity", opportunityId, new ColumnSet("ig1_projectnumber"));
            if (entity.Attributes.Contains("ig1_projectnumber"))
            {
                projectNumber = Convert.ToString(entity["ig1_projectnumber"]);
            }
            return projectNumber;
        }

        protected int MaxRevisionId(Guid opportunityId,int upperid)
        {
            int revise = 0;
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityId,
                ig1_upperrevisionid = upperid
            };
            var fetchXml = $@"
                             <fetch>
                            <entity name='ig1_bidsheet'>
                             <attribute name='ig1_revisionid' />
                              <attribute name='ig1_upperrevisionid' />
                              <filter type='and'>
                              <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*12a9e283-5608-41db-97b6-90526fd0b5a4*/}'/>
                              <condition attribute='ig1_upperrevisionid' operator='eq' value='{fetchData.ig1_upperrevisionid/*2*/}'/>
                                </filter>
                              </entity>
                               </fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            foreach (var bidsheet in result.Entities)
            {
                if (bidsheet.Attributes.Contains("ig1_revisionid") && bidsheet.Attributes["ig1_revisionid"] != null)
                {
                    var revisionId = (int)bidsheet.Attributes["ig1_revisionid"];
                    if (revisionId > revise)
                    {
                        revise = revisionId;
                    }
                }
            }
            return revise;
        }
    }
}
