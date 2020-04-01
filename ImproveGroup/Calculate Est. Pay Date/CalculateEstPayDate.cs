/*
 * This plugins will calculate the Est. Pay Date and will update the Project Record(ig1_projectrecord) entity..
 * 
 * 
 *DATE:31-03-2020
 *WRITTEN BY: Mohd Nazish
 */

using System;
using System.Collections;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace Calculate_Est.Pay_Date
{
    public class CalculateEstPayDate : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracingService;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);

            try
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    return;
                }
                var entity = (Entity)context.InputParameters["Target"];
                Guid opportunityId = Guid.Empty;
                if (entity.LogicalName == "opportunity")
                {
                    opportunityId = entity.Id;
                    if (opportunityId != Guid.Empty)
                    {
                        UpdateEstPayDate_ProjectRecord(opportunityId);
                    }

                }
                else if (entity.LogicalName == "msdyn_workorder")
                {
                    var wo = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_opportunityid"));
                    if (wo.Attributes.Contains("msdyn_opportunityid") && wo.Attributes["msdyn_opportunityid"] != null)
                    {
                        EntityReference opportunity = (EntityReference)wo.Attributes["msdyn_opportunityid"];
                        if (opportunity != null)
                        {
                            opportunityId = opportunity.Id;
                        }
                        if (opportunityId != Guid.Empty)
                        {
                            UpdateEstPayDate_ProjectRecord(opportunityId);
                        }
                    }
                }
                else if (entity.LogicalName == "ig1_projectrecord")
                {
                    var pr = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunity"));
                    if (pr.Attributes.Contains("ig1_opportunity") && pr.Attributes["ig1_opportunity"] != null)
                    {
                        EntityReference opportunity = (EntityReference)pr.Attributes["ig1_opportunity"];
                        if (opportunity != null)
                        {
                            opportunityId = opportunity.Id;
                        }
                        if (opportunityId != Guid.Empty)
                        {
                            UpdateEstPayDate_ProjectRecord(opportunityId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in CalculateEstPayDate Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected void UpdateEstPayDate_ProjectRecord(Guid oppId)
        {
            DateTime? estPayDate = CalculateEstmatedPayDate(oppId);
            if (estPayDate != null)
            {
                var fetchData = new
                {
                    statecode = "0",
                    ig1_opportunity = oppId
                };
                var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectrecordid'/>
                                <attribute name='ig1_projectnumber'/>
                                <attribute name='ig1_estpaydate' />
                                <attribute name='ig1_actualestpaydate'/>
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*619cdd20-20b4-4d67-bc48-3d84557616dc*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (ec.Entities.Count > 0)
                {
                    UpdateEstPayDateLog(ec.Entities[0]);
                    Entity entity = ec.Entities[0];
                    entity.Attributes["ig1_estpaydate"] = Convert.ToDateTime(estPayDate);
                    if (!entity.Attributes.Contains("ig1_actualestpaydate") || entity.Attributes["ig1_actualestpaydate"] == null)
                    {
                        entity.Attributes["ig1_actualestpaydate"] = Convert.ToDateTime(estPayDate);
                    }
                    service.Update(entity);
                }
            }
        }
        protected DateTime? CalculateEstmatedPayDate(Guid oppId)
        {
            DateTime? EstPayDate = null;
            DateTime? actualCompletionDate = FetchActualComplettionDate(oppId);
            if (actualCompletionDate != null)
            {
                EstPayDate = actualCompletionDate.Value.AddDays(30);
            }
            else
            {
                DateTime? committedCompletionOrEstProjetcCompletionDate = FetchCommittedCompletionOrEstProjetcCompletionDate(oppId);
                if (committedCompletionOrEstProjetcCompletionDate != null)
                {
                    EstPayDate = committedCompletionOrEstProjetcCompletionDate.Value.AddDays(30);
                }
            }
            return EstPayDate;
        }
        protected DateTime? FetchCommittedCompletionOrEstProjetcCompletionDate(Guid oppId)
        {
            DateTime? dateTime = null;
            var fetchData = new
            {
                statecode = "2",
                opportunityid = oppId
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='opportunity'>
                                <attribute name='ig1_requestedprojectcompletiondate' />
                                <attribute name='new_estprojectcompletion' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='neq' value='{fetchData.statecode/*2*/}'/>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*91740044-B63D-EA11-A812-000D3A55DD4E*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                var result = ec.Entities[0].Attributes;
                if (result.Contains("ig1_requestedprojectcompletiondate") && result["ig1_requestedprojectcompletiondate"] != null)
                {
                    dateTime = (DateTime)result["ig1_requestedprojectcompletiondate"];
                }
                else if (result.Contains("new_estprojectcompletion") && result["new_estprojectcompletion"] != null)
                {
                    dateTime = (DateTime)result["new_estprojectcompletion"];
                }
            }
            return dateTime;
        }
        protected DateTime? FetchActualComplettionDate(Guid oppId)
        {
            DateTime? dateTime = null;
            var fetchData = new
            {
                statecode = "0",
                msdyn_opportunityid = oppId,
                msdyn_systemstatus = "690970005"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='msdyn_workorder'>
                                <attribute name='ig1_requiredcompletiondate' />
                                <attribute name='ig1_dateofcustomersignoff' />
                                <attribute name='msdyn_systemstatus' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*76e041d1-fad2-e911-a967-000d3a1d57cf*/}'/>
                                  <condition attribute='msdyn_systemstatus' operator='neq' value='{fetchData.msdyn_systemstatus/*286150006*/}'/>
                                </filter>
                                <order attribute='createdon' descending='true' />
                              </entity>
                            </fetch>";

            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                foreach (var record in ec.Entities)
                {
                    if (record.Attributes.Contains("ig1_dateofcustomersignoff") && record.Attributes["ig1_dateofcustomersignoff"] != null)
                    {
                        if (record.Attributes.Contains("msdyn_systemstatus") && record.Attributes["msdyn_systemstatus"] != null)
                        {
                            OptionSetValue workOrderStatus = (OptionSetValue)record.Attributes["msdyn_systemstatus"];
                            if ((workOrderStatus.Value == 690970004 || workOrderStatus.Value == 690970003) && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 690970002 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 690970001 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 690970000 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                        }
                    }
                }
            }
            return dateTime;
        }
        protected void UpdateEstPayDateLog(Entity projectRecord)
        {
            if (projectRecord.Attributes.Contains("ig1_estpaydate") && projectRecord.Attributes["ig1_estpaydate"] != null)
            {
                if (projectRecord != null)
                {
                    Entity entity = new Entity("ig1_projectrecordlog");
                    if (projectRecord.Id != Guid.Empty)
                    {
                        entity.Attributes["ig1_projectrecord"] = new EntityReference(projectRecord.LogicalName, projectRecord.Id);
                    }
                    if (projectRecord.Attributes.Contains("ig1_projectnumber") && !string.IsNullOrEmpty(projectRecord.Attributes["ig1_projectnumber"].ToString()))
                    {
                        entity.Attributes["ig1_name"] = projectRecord.Attributes["ig1_projectnumber"].ToString();
                    }
                    entity.Attributes["ig1_date"] = Convert.ToDateTime(projectRecord.Attributes["ig1_estpaydate"]);
                    entity.Attributes["ig1_datetype"] = new OptionSetValue(10);
                    service.Create(entity);
                }
            }
        }
    }
}
