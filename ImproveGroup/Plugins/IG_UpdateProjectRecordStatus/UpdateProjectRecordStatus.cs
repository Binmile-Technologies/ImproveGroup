using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdateProjectRecordStatus
{
    public class UpdateProjectRecordStatus : IPlugin
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
                service = serviceFactory.CreateOrganizationService(null);

                Guid opportunityid = Guid.Empty;
                string entityName = string.Empty;
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName == "msdyn_workorder")
                    {
                        Entity wo = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_opportunityid"));
                        if (wo.Attributes.Contains("msdyn_opportunityid") && wo.Attributes["msdyn_opportunityid"] != null)
                        {
                            opportunityid = wo.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                        }
                    }
                    else if (entity.LogicalName == "opportunity")
                    {
                        opportunityid = entity.Id;
                    }
                    else if (entity.LogicalName == "ig1_bidsheet")
                    {
                        entityName = entity.LogicalName;
                        Entity bidsheet = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_opportunitytitle"));
                        if (bidsheet.Attributes.Contains("ig1_opportunitytitle") && bidsheet.Attributes["ig1_opportunitytitle"] != null)
                        {
                            opportunityid = bidsheet.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
                        }
                    }
                }
                else if (context.MessageName == "Delete" && context.PreEntityImages.Contains("Image") && context.PreEntityImages["Image"] is Entity)
                {
                    Entity bidsheet = (Entity)context.PreEntityImages["Image"];
                    if (bidsheet.LogicalName == "ig1_bidsheet")
                    {
                        entityName = bidsheet.LogicalName;
                        if (bidsheet.Attributes.Contains("ig1_opportunitytitle") && bidsheet.Attributes["ig1_opportunitytitle"] != null)
                        {
                            opportunityid = bidsheet.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
                        }
                    }
                }
                if (opportunityid != Guid.Empty)
                {
                    bool Daterfqprompurchasing = GetDateRfqfromopportunity(opportunityid);
                    string woStatus1 = GetWOStatus(opportunityid);
                    if (Daterfqprompurchasing== true && String.IsNullOrEmpty(woStatus1))
                    {
                        UpdateProjectRecord(opportunityid, 286150011, woStatus1);
                    }

                    else
                    {
                        string woStatus = GetWOStatus(opportunityid);
                        string opportunityStatus = GetOpportunityStatus(opportunityid);
                        if (woStatus == "Installing" && opportunityStatus != "Lost")
                        {
                            UpdateProjectRecord(opportunityid, 286150002, woStatus);
                        }
                        else if (woStatus == "Completed" && opportunityStatus != "Lost")
                        {
                            UpdateProjectRecord(opportunityid, 286150003, woStatus);
                        }


                        else
                        {
                            if (opportunityStatus == "Lost")
                            {
                                UpdateProjectRecord(opportunityid, 286150006, woStatus);
                            }
                            else if (opportunityStatus == "Won")
                            {
                                UpdateProjectRecord(opportunityid, 286150001, woStatus);
                            }
                            else if (opportunityStatus == "Open")
                            {
                                string bidsheetStatus = GetBidSheetStatus(opportunityid);
                                if (bidsheetStatus == "Priced")
                                {
                                    UpdateProjectRecord(opportunityid, 286150005, woStatus);
                                }
                                else if (bidsheetStatus == "Designing")
                                {
                                    UpdateProjectRecord(opportunityid, 286150010, woStatus);

                                    if (opportunityid != Guid.Empty)
                                    {
                                        Entity opportunity = new Entity("opportunity", opportunityid);
                                        opportunity.Attributes["ig1_bidsheetcreated"] = true;
                                        service.Update(opportunity);
                                    }

                                }
                                else
                                {
                                    UpdateProjectRecord(opportunityid, 286150000, woStatus);

                                    if (opportunityid != Guid.Empty)
                                    {
                                        Entity opportunity = new Entity("opportunity", opportunityid);
                                        opportunity.Attributes["ig1_bidsheetcreated"] = false;
                                        service.Update(opportunity);
                                    }
                                }
                            }
                        }
                        if (opportunityid != Guid.Empty && entityName == "ig1_bidsheet")
                        {
                            QueryExpression queryExpression = new QueryExpression();
                            queryExpression.EntityName = "ig1_bidsheet";
                            queryExpression.ColumnSet.AddColumn("ig1_projectnumber");
                            queryExpression.Criteria.AddCondition("ig1_opportunitytitle", ConditionOperator.Equal, opportunityid);
                            EntityCollection entityCollection = service.RetrieveMultiple(queryExpression);

                            Entity opportunity = new Entity("opportunity", opportunityid);
                            if (entityCollection.Entities.Count > 0)
                            {
                                opportunity.Attributes["ig1_bidsheetcreated"] = true;
                                service.Update(opportunity);
                            }
                            else
                            {
                                opportunity.Attributes["ig1_bidsheetcreated"] = false;
                                service.Update(opportunity);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in UpdateProjectRecordStatus Plugin " + ex);
            }
        }
        protected string GetWOStatus(Guid opportunityid)
        {
            string status = string.Empty;
            var fetchData = new
            {
                msdyn_opportunityid = opportunityid,
                ig1_workorderstatus = "286150006"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_workorder'>
                                <attribute name='ig1_workorderstatus' />
                                <filter type='and'>
                                  <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*00000000-0000-0000-0000-000000000000*/}'/>
                                  <condition attribute='ig1_workorderstatus' operator='neq' value='{fetchData.ig1_workorderstatus/*286150006*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("ig1_workorderstatus") && entity.Attributes["ig1_workorderstatus"] != null)
                    {
                        int statusValue = entity.GetAttributeValue<OptionSetValue>("ig1_workorderstatus").Value;
                        if (statusValue == 286150000 || statusValue == 286150001 || statusValue == 286150002)
                        {
                            status = "Installing";
                        }
                        else if (statusValue == 286150004 || statusValue == 286150005)
                        {
                            status = "Completed";
                            break;
                        }
                    }
                }
            }
            return status;
        }
        protected string GetOpportunityStatus(Guid opportunityid)
        {
            string status = string.Empty;
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='opportunity'>
                                <attribute name='statecode' />
                                <filter>
                                  <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                if (entity.Attributes.Contains("statecode") && entity.Attributes["statecode"] != null)
                {
                    int statusValue = entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                    if (statusValue == 0)
                    {
                        status = "Open";
                    }
                    else if (statusValue == 1)
                    {
                        status = "Won";
                    }
                    else if (statusValue == 2)
                    {
                        status = "Lost";
                    }
                }
            }
            return status;
        }
        protected string GetBidSheetStatus(Guid opportunityid)
        {
            string status = string.Empty;
            var fetchData = new
            {
                ig1_opportunitytitle = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_bidsheet'>
                                <attribute name='ig1_associated' />
                                <attribute name='ig1_opportunitytitle' />
                                <filter>
                                  <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*00000000-0000-0000-0000-000000000000*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                status = "Designing";
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("ig1_associated") && entity.Attributes["ig1_associated"] != null)
                    {
                        Boolean isAssociated = Convert.ToBoolean(entity.Attributes["ig1_associated"]);
                        if (isAssociated)
                        {
                            status = "Priced";
                            break;
                        }
                    }
                }
            }
            return status;
        }
        protected bool GetDateRfqfromopportunity(Guid opportunityid)
        {
            string oppstate = GetOpportunityStatus(opportunityid);
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='opportunity'>
                            <attribute name='ig1_daterfqfrompurchasing' />
                            <filter>
                              <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*9e7ba460-da55-4a01-a94d-a05d74d0d6c7*/}'/>
                            </filter>
                          </entity>
                        </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    if (entity.Attributes.Contains("ig1_daterfqfrompurchasing") && entity.Attributes["ig1_daterfqfrompurchasing"] != null && oppstate== "Open")
                    {                                                                     
                            return true;
                                                
                    }
                    return false;
                }


            }
            return false;
        }
        protected void UpdateProjectRecord(Guid opportunityid, int status, string woStatus)
        {
            string oppst = GetOpportunityStatus(opportunityid);
            var fetchData = new
            {
                ig1_opportunity = opportunityid,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='ig1_projectrecord'>
                               <attribute name='ig1_holdpreviousstatus1' />
                                <attribute name='ig1_projectstatus' />
                                <filter type='and'>
                                  <condition attribute='ig1_opportunity' operator='eq' value='{fetchData.ig1_opportunity/*00000000-0000-0000-0000-000000000000*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            { 
                Entity entity = entityCollection.Entities[0];
                int preprojectStatus=0;
               
                if(entity.Attributes.Contains("ig1_holdpreviousstatus1") && entity.Attributes["ig1_holdpreviousstatus1"] != null)
                {
                     preprojectStatus = entity.GetAttributeValue<OptionSetValue>("ig1_holdpreviousstatus1").Value;
                }
               
                if (entity.Attributes.Contains("ig1_projectstatus") && entity.Attributes["ig1_projectstatus"] != null)
                {
                    int projectStatus = entity.GetAttributeValue<OptionSetValue>("ig1_projectstatus").Value;
                    
                    if (oppst != "Lost")
                    {
                        if ((preprojectStatus == 286150007 || preprojectStatus == 286150008 || preprojectStatus == 286150009) && woStatus == "Completed")
                        {
                            status = preprojectStatus;
                        }

                       else if ((projectStatus == 286150007 || projectStatus == 286150008 || projectStatus == 286150009) && woStatus == "Completed")
                        {
                            return;
                        }
                    }                 
                }
                Entity projectRecord = new Entity(entity.LogicalName, entity.Id);
                projectRecord.Attributes["ig1_projectstatus"] = new OptionSetValue(status);
                service.Update(projectRecord);

            }
        }
    }
}
