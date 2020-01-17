using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdateProjectRecordStatus
{
    public class UpdateProjectRecordStatus : IPlugin
    {
        Guid OpportunityId;
        int StatusCode;
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Buisness logic
                    string entityName = entity.LogicalName;
                    if (entityName == "opportunity")
                    {
                        OpportunityId = entity.Id;
                    }
                    else if (entityName == "msdyn_workorder")
                    {
                        Guid WorkOrderId = entity.Id;
                        //Get Opportunity Id by Work Order ID
                        if (entity.Attributes.Contains("msdyn_opportunityid"))
                        {
                            OpportunityId = ((EntityReference)entity.Attributes["msdyn_opportunityid"]).Id;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if(entityName=="ig1_bidsheet")
                    {
                        //Bidsheet
                        Guid BidSheetId = entity.Id;
                        //Get Opportunity Id by Bidsheet ID
                        if (entity.Attributes.Contains("ig1_opportunitytitle"))
                        {
                            OpportunityId = ((EntityReference)entity.Attributes["ig1_opportunitytitle"]).Id;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //UpdateProjectRecord(OpportunityId, service, StatusCode);
                    UpdateProjectRecord(service);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Status Update Plugin(Update_Project_Record_Status_Plugin): {0}", ex.ToString());
                    throw;
                }
            }
        }

        public int CheckWorkOrderExistsOrNot(IOrganizationService service)
        {
            int[] arrOpen = { 690970000, 690970001, 690970002, 690970003 }; //Open statuses of Work Order for ex.(Open-Scheduled 690970001,Open-Inprogress 690970002, Open-UnScheduled 690970000,Open-Completed 690970003)
            int inResult = 0;
            if (OpportunityId != Guid.Empty)
            {
                var fetchData = new
                {
                    msdyn_opportunityid = OpportunityId
                };
                var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='msdyn_workorder'>
                                <attribute name='msdyn_systemstatus' />
                                <attribute name='msdyn_opportunityid' />
                                <filter>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*a1980122-a02b-ea11-a810-000d3a55dd4e*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
                EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result.Entities.Count > 0)
                {
                    for (int i = 0; i < result.Entities.Count; i++)
                    {
                        if (result.Entities[i].Attributes.Contains("msdyn_systemstatus"))
                        {
                            var woStatus = (OptionSetValue)result.Entities[i].Attributes["msdyn_systemstatus"];
                            if (arrOpen.Contains(woStatus.Value))
                            {
                                inResult = 1;
                                break;
                            }
                            else
                            {
                                inResult = 2;
                            }
                        }

                    }
                }
            }
            return inResult;
        }

        public int GetOpportunityStatusByOppId(IOrganizationService service)
        {
            int inResult = 2;
            var fetchData = new
            {
                opportunityid = OpportunityId
            };
            var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='opportunity'>
                                <attribute name='statecode' />
                                <filter>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*a1980122-a02b-ea11-a810-000d3a55dd4e*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (result.Entities[0].Attributes.Contains("statecode"))
                {
                    var woStatus = (OptionSetValue)result.Entities[0].Attributes["statecode"];
                    if (Convert.ToInt32(woStatus.Value) == 0)
                    {
                        inResult = 0; //Open Opportunity
                    }
                    else if (Convert.ToInt32(woStatus.Value) == 1)
                    {
                        inResult = 1; //Close as won Opportunity
                    }
                }
            }
            return inResult;
        }

        public Guid GetProjectRecordId(IOrganizationService service)
        {
            Guid retval = Guid.Empty;
            if (OpportunityId != Guid.Empty)
            {
                var fetchData = new
                {
                    ig1_opportunity = OpportunityId
                };
                var fetchXml = $@"
                        <fetch attribute='statuscodename' operator='eq'>
                          <entity name='ig1_projectrecord'>
                            <attribute name='ig1_projectrecordid' />
                            <filter type='and'>
                              <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*a1980122-a02b-ea11-a810-000d3a55dd4e*/}'/>
                            </filter>
                          </entity>
                        </fetch>";
                EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (result.Entities.Count > 0)
                {
                    if (result.Entities[0].Attributes.Contains("ig1_projectrecordid"))
                    {
                        retval = (Guid)result.Entities[0].Attributes["ig1_projectrecordid"];
                    }
                }
            }
            return retval;
        }

        public int CheckIfAssociatedeBidSheetIdExistsByOpportunityID(IOrganizationService service)
        {
            int inResult = 0;
            Guid retval = Guid.Empty;
            var fetchData = new
            {
                ig1_associated = "1", // Status of Bidsheet is Associated
                ig1_opportunitytitle = OpportunityId
            };
            var fetchXml = $@"
                            <fetch attribute='ig1_opportunitytitle ' operator='eq'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_opportunitytitle' />
                                <attribute name='ig1_associated' />
                                <filter type='and'>
                                  <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                                </filter>
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*88e041d1-fad2-e911-a967-000d3a1d57cf*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                inResult = 1;
                //if (result.Entities[0].Attributes.Contains("ig1_bidsheetid"))
                //{
                //    retval = (Guid)result.Entities[0].Attributes["ig1_bidsheetid"];
                //}
            }
            //return retval;
            return inResult;
        }

        //This function will Update Project Status By Opportunity ID
        public void UpdateProjectRecord(IOrganizationService service)
        {
            int inResult = CheckWorkOrderExistsOrNot(service);
            int inResultOpportunity = GetOpportunityStatusByOppId(service);
            int inResultBidsheet;
            if (inResult == 1 && inResultOpportunity != 2)               //Work order status is Open and Opportunity status should not be Close as Lost i.e. inResultOpportunity !=2
            {
                //Work order exists for given opportunity with Open status.
                //Set Status of project record to Installing(286150002)
                StatusCode = 286150002;
            }
            else if (inResult == 2 && inResultOpportunity != 2)          //Work order status is Closed and Opportunity status should not be Close as Lost i.e. inResultOpportunity !=2
            {
                //Work order exists for given opportunity with Closed status
                //Set Status of project record to Completed(286150003)
                StatusCode = 286150003;
            }
            else
            {
                //Work order doesn't exists for given opportunity, So check Status of Opportunity whether it is Closed won or Open
                inResultBidsheet = CheckIfAssociatedeBidSheetIdExistsByOpportunityID(service);
                if (inResultBidsheet == 1 && inResultOpportunity != 2)   // Opportunity Status  should not be Close as Lost i.e. inResultOpportunity !=2 and Associated Bidsheet exists
                {
                    //Status of project record to Priced(286150005)
                    StatusCode = 286150005;
                }
                else
                {
                    if (inResultOpportunity == 1 && inResultBidsheet == 0)      // Opportunity Status is Won and Associated Bidsheet doesn't exists
                    {
                        //Status of project record to Booked(286150001)
                        StatusCode = 286150001;
                    }
                    else if (inResultOpportunity == 0 && inResultBidsheet == 0)     // Opportunity Status is Open and Associated Bidsheet doesn't exists
                    {
                        //Status of project record to Estimated(286150000)
                        StatusCode = 286150000;
                    }
                    else
                    {
                        StatusCode = 286150004; // For now Opprotunity having Close as Lost Then it's status has been set to Finalised (286150004)
                    }
                }
            }
            // Get ProjectRecordID row by providing opportunity id
            Guid ProRecId = GetProjectRecordId(service);
            // Get ProjectRecord row by providing ProjectRecord ID
            if (ProRecId != Guid.Empty)
            {
                Entity ProjectRecord = service.Retrieve("ig1_projectrecord", ProRecId, new ColumnSet("ig1_projectstatus"));
                ProjectRecord["ig1_projectstatus"] = new OptionSetValue(StatusCode);
                service.Update(ProjectRecord);
            }
        }
    }
}
