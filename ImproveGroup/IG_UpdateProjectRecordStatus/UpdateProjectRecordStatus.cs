using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
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
                        //OpportunityId = GetOpportunityIdByWOId(WorkOrderId, service);
                        OpportunityId = ((EntityReference)entity.Attributes["msdyn_opportunityid"]).Id;
                    }
                    else
                    {
                        //Bidsheet
                        Guid BidSheetId = entity.Id;
                        //Get Opportunity Id by Bidsheet ID
                        //OpportunityId = GetOpportunityIdByBSId(BidSheetId, service);
                        OpportunityId = ((EntityReference)entity.Attributes["ig1_opportunitytitle"]).Id;
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

        public int CheckWorkOrderExistsOrNot(Guid OppId, IOrganizationService service)
        {
            int[] arrOpen = { 690970000, 690970001, 690970002, 690970003 };
            int inResult = 0;
            var fetchData = new
            {
                msdyn_opportunityid = OppId
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
            return inResult;
        }

        public int GetOpportunityStatusByOppId(Guid OppId, IOrganizationService service)
        {
            int inResult = 0;
            var fetchData = new
            {
                opportunityid = OppId
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
                        inResult = 0;
                    }
                    else
                    {
                        inResult = 1;
                    }
                }
            }
            return inResult;
        }

        public Guid GetProjectRecordId(Guid OppId, IOrganizationService service)
        {
            Guid retval = Guid.Empty;
            var fetchData = new
            {
                ig1_opportunity = OppId
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
            return retval;
        }

        public Guid GetActiveBidSheetIdByOpportunityID(Guid OppId, IOrganizationService service)
        {
            Guid retval = Guid.Empty;
            var fetchData = new
            {
                ig1_opportunitytitle = OppId,
                ig1_status = "286150000"
            };
            var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='ig1_bidsheet'>
                                <filter type='and'>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*a1980122-a02b-ea11-a810-000d3a55dd4e*/}'/>
                                  <condition attribute='ig1_status' operator='eq' value='{fetchData.ig1_status/*286150000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (result.Entities[0].Attributes.Contains("ig1_bidsheetid"))
                {
                    retval = (Guid)result.Entities[0].Attributes["ig1_bidsheetid"];
                }
            }
            return retval;
        }

        //Not in Use
        public string GetWOStatusIdByWOId(Guid WOId, IOrganizationService service)
        {
            string retval = "";
            string[] arrOpen = { "690970000", "690970001", "690970002", "690970003" };
            var fetchData = new
            {
                msdyn_workorderid = WOId
            };
            var fetchXml = $@"
                        <fetch attribute='statuscodename' operator='eq'>
                          <entity name='msdyn_workorder'>
                            <attribute name='msdyn_systemstatus' />
                            <filter type='and'>
                              <condition attribute='msdyn_workorderid' operator='eq' value='{fetchData.msdyn_workorderid/*3544909f-e873-40c4-9137-e12ced016e1c*/}'/>
                            </filter>
                          </entity>
                        </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (arrOpen.Contains(Convert.ToString(result.Entities[0].Attributes["msdyn_systemstatus"])))
                {
                    retval = "Open";
                }
                else
                {
                    retval = "Closed";
                }
            }
            return retval;
        }

        public string GetBSStatusIdByBSId(Guid BSId, IOrganizationService service)
        {
            string retval = "";
            var fetchData = new
            {
                ig1_bidsheetid = BSId
            };
            var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_status' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheetid' operator='eq' value='{fetchData.ig1_bidsheetid/*f8d726fd-9521-ea11-a810-000d3a4e6fff*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (result.Entities[0].Attributes.Contains("ig1_status"))
                {
                    retval = Convert.ToString(result.Entities[0].Attributes["ig1_status"]);
                }
            }
            return retval;
        }

        public Guid GetOpportunityIdByWOId(Guid WOId, IOrganizationService service)
        {
            Guid retval = Guid.Empty;
            var fetchData = new
            {
                msdyn_workorderid = WOId
            };
            var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='msdyn_workorder'>
                                <attribute name='msdyn_opportunityid' />
                                <filter type='and'>
                                  <condition attribute='msdyn_workorderid' operator='eq' value='{fetchData.msdyn_workorderid/*2b615065-6404-ea11-a811-000d3a55dd4e*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (result.Entities[0].Attributes.Contains("msdyn_opportunityid"))
                {
                    var opp = (EntityReference)result.Entities[0].Attributes["msdyn_opportunityid"];
                    retval = opp.Id;
                    //retval = ((EntityReference)result.Entities[0].Attributes["msdyn_opportunityid"]).Id;
                }
            }
            return retval;
        }

        public Guid GetOpportunityIdByBSId(Guid BSId, IOrganizationService service)
        {
            Guid retval = Guid.Empty;
            var fetchData = new
            {
                ig1_bidsheetid = BSId
            };
            var fetchXml = $@"
                            <fetch attribute='statuscodename' operator='eq'>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_opportunitytitle' />
                                <filter type='and'>
                                  <condition attribute='ig1_bidsheetid' operator='eq' value='{fetchData.ig1_bidsheetid/*f8d726fd-9521-ea11-a810-000d3a4e6fff*/}'/>
                                </filter>
                              </entity>
                            </fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                if (result.Entities[0].Attributes.Contains("ig1_opportunitytitle"))
                {
                    var opp = (EntityReference)result.Entities[0].Attributes["ig1_opportunitytitle"];
                    retval = opp.Id;
                    //retval = (Guid)result.Entities[0].Attributes["ig1_opportunitytitle"];
                }
            }
            return retval;
        }

        //This function will Update Project Status By Opportunity ID
        //public void UpdateProjectRecord(Guid OppId, IOrganizationService service, int StatusCode)
        public void UpdateProjectRecord(IOrganizationService service)
        {
            int inResult = CheckWorkOrderExistsOrNot(OpportunityId, service);

            if (inResult == 1)
            {
                //Work order exists for given opportunity with Open status.
                //Set Status of project record to Installing(286150002)
                StatusCode = 286150002;
            }
            else if (inResult == 2)
            {
                //Work order exists for given opportunity with Closed status
                //Set Status of project record to Completed(286150003)
                StatusCode = 286150003;
            }
            else
            {
                //Work order doesn't exists for given opportunity, So check Status of Opportunity whether it is Closed won or Open
                Guid ActiveBidSheetId = GetActiveBidSheetIdByOpportunityID(OpportunityId, service);
                if (ActiveBidSheetId != Guid.Empty)
                {
                    //Active bidsheet exists for given opportunity
                    //Set Status of project record to Priced(286150005)
                    StatusCode = 286150005;
                }
                else
                {
                    //Get Opportunity status by Opportunity Id
                    int oppStatus = GetOpportunityStatusByOppId(OpportunityId, service);
                    if (oppStatus == 1)
                    {
                        // If Opportunity's status is Won, Then set Status of project record to Booked(286150001)
                        StatusCode = 286150001;
                    }
                    else if (oppStatus == 0)
                    {
                        // If Opportunity's status is Open, Then set Status of project record to Estimated(286150000)
                        StatusCode = 286150000;
                    }
                }
                // Get ProjectRecordID row by providing opportunity id
                Guid ProRecId = GetProjectRecordId(OpportunityId, service);
                // Get ProjectRecord row by providing ProjectRecord ID
                Entity ProjectRecord = service.Retrieve("ig1_projectrecord", ProRecId, new ColumnSet("ig1_projectstatus"));
                ProjectRecord["ig1_projectstatus"] = new OptionSetValue(StatusCode);
                service.Update(ProjectRecord);
            }
        }
    }
}
