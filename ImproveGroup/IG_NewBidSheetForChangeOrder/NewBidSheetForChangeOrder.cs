﻿using System;
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
            service = servicefactory.CreateOrganizationService(context.UserId);
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "ig1_bidsheet")
                    {
                        return;
                    }
                    if (entity.Attributes.Contains("ig1_opportunitytitle"))
                    {
                        var opportunity = (EntityReference)entity.Attributes["ig1_opportunitytitle"];
                        var opportunityId = (Guid)opportunity.Id;
                        UpdateUpperRevisionId(opportunityId, entity.Id);
                        UpdateProjectNumber(opportunityId, entity.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "Error";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.InnerException;
                service.Create(errorLog);
                throw;
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
                Entity entity = service.Retrieve("ig1_bidsheet", bidSheetId, new ColumnSet("ig1_upperrevisionid"));
                if (!entity.Attributes.Contains("ig1_upperrevisionid"))
                {
                    entity["ig1_upperrevisionid"] = maxUpperRevisionId + 1;
                    service.Update(entity);
                }
        }
        protected void UpdateProjectNumber(Guid opportunityId, Guid bidSheetId)
        {
            string projectNumber = GetProjectNumber(opportunityId);
            Entity entity = service.Retrieve("ig1_bidsheet", bidSheetId, new ColumnSet("ig1_projectnumber"));
            if (!entity.Attributes.Contains("ig1_projectnumber") && projectNumber!="")
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
    }
}