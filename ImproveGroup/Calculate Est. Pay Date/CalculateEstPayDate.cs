/*
 * This plugins will calculate the Est. Pay Date and will update the Project Record(ig1_projectrecord) entity..
 * 
 * 
 *DATE:04-03-2020
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
            service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    return;
                }
                var entity = (Entity)context.InputParameters["Target"];
                Guid opportunityId = Guid.Empty;
                if (entity.LogicalName == "oppoertunity")
                {
                    opportunityId = entity.Id;

                }
                else if (entity.LogicalName == "msdyn_workorder")
                {
                    if (entity.Attributes.Contains("msdyn_opportunityid") && entity.Attributes["msdyn_opportunityid"] != null)
                    {
                        EntityReference opportunity = (EntityReference)entity.Attributes["msdyn_opportunityid"];
                        opportunityId = opportunity.Id;
                    }
                }
            }
            catch (Exception ex)
            { 

            }
        }
        protected void UpdateEstPayDate(Entity entity, Guid oppId)
        {
            Guid projectRecordId = FetchProjectRecordId(oppId);
            if(projectRecordId!=Guid.Empty)
            {
                Entity projectRecord = service.Retrieve(entity.LogicalName, projectRecordId, new ColumnSet("ig1_estpaydate"));
                if (projectRecord.Attributes.Contains("ig1_estpaydate"))
                {
                    Entity estPaydateLog = new Entity("");
                }
            }
        }
        protected Guid FetchProjectRecordId(Guid oppId)
        {
            Guid projectRecordId = Guid.Empty;
            var fetchData = new
            {
                statecode = "0",
                ig1_opportunity = oppId
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectrecordid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*619cdd20-20b4-4d67-bc48-3d84557616dc*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                projectRecordId = ec.Entities[0].Id;
            }

                return projectRecordId;
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
                ig1_workorderstatus = "286150006"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='msdyn_workorder'>
                                <attribute name='ig1_requiredcompletiondate' />
                                <attribute name='ig1_dateofcustomersignoff' />
                                <attribute name='ig1_workorderstatus' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*76e041d1-fad2-e911-a967-000d3a1d57cf*/}'/>
                                  <condition attribute='ig1_workorderstatus' operator='neq' value='{fetchData.ig1_workorderstatus/*286150006*/}'/>
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
                        if (record.Attributes.Contains("ig1_workorderstatus") && record.Attributes["ig1_workorderstatus"] != null)
                        {
                            OptionSetValue workOrderStatus = (OptionSetValue)record.Attributes["ig1_workorderstatus"];
                            if ((workOrderStatus.Value == 286150005 || workOrderStatus.Value == 286150004) && dateTime==null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 286150002 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 286150001 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                            else if (workOrderStatus.Value == 286150000 && dateTime == null)
                            {
                                dateTime = (DateTime)record.Attributes["ig1_dateofcustomersignoff"];
                            }
                        }
                    }
                }
            }
            return dateTime;
        }
    }
}
