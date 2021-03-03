using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace IG_CreateProjectRecord
{
    public class CreateProjectRecord : IPlugin
    {
        IOrganizationService service;
        IOrganizationServiceFactory serviceFactory;
        IPluginExecutionContext context;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(null);
                Guid opportunityid = Guid.Empty;
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (context.MessageName == "Create" && entity.LogicalName == "opportunity")
                    {
                        opportunityid = entity.Id;
                        Entity opportunity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("parentaccountid", "ig1_projectnumber", "ownerid", "ig1_istestproject", "name", "statecode", "ig1_forecaststatus"));
                        
                        Entity projectRecord = new Entity("ig1_projectrecord");

                        projectRecord.Attributes["ig1_opportunity"] = new EntityReference(entity.LogicalName, entity.Id);
                        if (opportunity.Attributes.Contains("ig1_projectnumber") && opportunity.Attributes["ig1_projectnumber"] != null)
                        {
                            projectRecord.Attributes["ig1_projectnumber"] = Convert.ToString(opportunity.Attributes["ig1_projectnumber"]);
                        }
                        if (opportunity.Attributes.Contains("name") && opportunity.Attributes["name"] != null)
                        {
                            projectRecord.Attributes["ig1_name"] = Convert.ToString(opportunity.Attributes["name"]);
                        }
                        if (opportunity.Attributes.Contains("ownerid") && opportunity.Attributes["ownerid"] != null)
                        {
                            EntityReference opportunityOwner = (EntityReference)opportunity.Attributes["ownerid"];
                            projectRecord.Attributes["ig1_opportunityowner"] = opportunityOwner;
                        }
                        if (entity.Attributes.Contains("parentaccountid") && entity.Attributes["parentaccountid"] != null)
                        {
                            EntityReference parentAccount = (EntityReference)entity.Attributes["parentaccountid"];
                            projectRecord.Attributes["ig1_opportunityprimaryaccount"] = parentAccount;
                        }
                        if (opportunity.Attributes.Contains("ig1_istestproject") && opportunity.Attributes["ig1_istestproject"] != null)
                        {
                            projectRecord.Attributes["ig1_istestproject"] = Convert.ToBoolean(opportunity.Attributes["ig1_istestproject"]);
                        }
                        if (opportunity.Attributes.Contains("statecode") && opportunity.Attributes["statecode"] != null)
                        {
                            OptionSetValue stateCode = (OptionSetValue)opportunity.Attributes["statecode"];
                            if (stateCode.Value == Convert.ToInt32(0))
                            {
                                projectRecord.Attributes["ig1_opportunitystatus"] = new OptionSetValue(286150000);
                            }
                            else if (stateCode.Value == Convert.ToInt32(1))
                            {
                                projectRecord.Attributes["ig1_opportunitystatus"] = new OptionSetValue(286150001);
                            }
                            else if (stateCode.Value == Convert.ToInt32(2))
                            {
                                projectRecord.Attributes["ig1_opportunitystatus"] = new OptionSetValue(286150002);
                            }
                        }
                        if (opportunity.Attributes.Contains("ig1_forecaststatus") && opportunity.Attributes["ig1_forecaststatus"] != null)
                        {
                            OptionSetValue forecastStatus = (OptionSetValue)opportunity.Attributes["ig1_forecaststatus"];
                            if (forecastStatus.Value == Convert.ToInt32(286150000))
                            {
                                projectRecord.Attributes["ig1_opportunityforecaststatus"] = new OptionSetValue(286150000);
                            }
                            else if (forecastStatus.Value == Convert.ToInt32(286150001))
                            {
                                projectRecord.Attributes["ig1_opportunityforecaststatus"] = new OptionSetValue(286150001);
                            }
                            else if (forecastStatus.Value == Convert.ToInt32(286150002))
                            {
                                projectRecord.Attributes["ig1_opportunityforecaststatus"] = new OptionSetValue(286150002);
                            }
                            else if (forecastStatus.Value == Convert.ToInt32(286150003))
                            {
                                projectRecord.Attributes["ig1_opportunityforecaststatus"] = new OptionSetValue(286150003);
                            }
                        }
                        Guid projectRecordid = service.Create(projectRecord);

                        if (projectRecordid != Guid.Empty)
                        {
                            Entity opp = new Entity("opportunity", opportunityid);
                            opp.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecordid);
                            service.Update(opp);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in CreateProjectRecord Plugin " + ex);
            }
        }
    }
}
