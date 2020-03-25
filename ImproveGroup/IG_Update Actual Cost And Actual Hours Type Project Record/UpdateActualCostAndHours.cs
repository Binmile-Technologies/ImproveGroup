using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_Update_Actual_Cost_And_Actual_Hours_Type_Project_Record
{
    public class UpdateActualCostAndHours: IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracing;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);
            try
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    return;
                }
                var entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName == "ig1_projectrecordcost")
                {
                    if ((entity.Attributes.Contains("ig1_actualdate") && !string.IsNullOrEmpty(entity.Attributes["ig1_actualdate"].ToString()))
                        || (entity.Attributes.Contains("ig1_costtype") && !string.IsNullOrEmpty(entity.Attributes["ig1_costtype"].ToString())))
                    {
                        Entity actualCost = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_name"));
                        if (entity.Attributes.Contains("ig1_actualdate") && !string.IsNullOrEmpty(entity.Attributes["ig1_actualdate"].ToString()))
                        {
                            actualCost.Attributes["ig1_date"] = Convert.ToDateTime(entity.Attributes["ig1_actualdate"]);
                        }
                        if (entity.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(entity.Attributes["ig1_name"].ToString()))
                        {
                            Guid projectRecord = Guid.Empty;
                            projectRecord = GetProjectRecord(entity.Attributes["ig1_name"].ToString());
                            if (projectRecord != Guid.Empty)
                            {
                                actualCost.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecord);
                            }
                        }
                        if (entity.Attributes.Contains("ig1_costtype") && !string.IsNullOrEmpty(entity.Attributes["ig1_costtype"].ToString()))
                        {
                            Guid expenseType = Guid.Empty;
                            string type = entity.Attributes["ig1_costtype"].ToString();
                            if (type != "Sales Cost" && type != "Design Cost" && type != "PM Cost")
                            {
                                expenseType = GetExpenseType("Miscellaneous");
                            }
                            else
                            {
                                expenseType = GetExpenseType(type);
                            }
                            if (expenseType != Guid.Empty)
                            {
                                actualCost.Attributes["ig1_expensetype"] = new EntityReference("ig1_expensecategories", expenseType);
                            }
                        }
                        service.Update(actualCost);
                    }
                }
                else if (entity.LogicalName == "ig1_projectrecordhours")
                {
                    if (entity.Attributes.Contains("ig1_actualdate") && !string.IsNullOrEmpty(entity.Attributes["ig1_actualdate"].ToString()))
                    {
                        var actualHours = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_name"));
                        actualHours.Attributes["ig1_date"] = Convert.ToDateTime(entity.Attributes["ig1_actualdate"].ToString());

                        if (entity.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(entity.Attributes["ig1_name"].ToString()))
                        {
                            Guid projectRecord = Guid.Empty;
                            projectRecord = GetProjectRecord(entity.Attributes["ig1_name"].ToString());
                            if (projectRecord != Guid.Empty)
                            {
                                actualHours.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecord);
                            }
                        }
                        service.Update(actualHours);
                    }
                }
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in UpdateActualCostAndHours Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected Guid GetProjectRecord(string projectNumber)
        {
            var fetchData = new
            {
                statecode = "0",
                ig1_projectnumber = projectNumber
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectrecordid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_projectnumber' operator='eq' value='{fetchData.ig1_projectnumber/*40450*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                return ec.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }
        protected Guid GetExpenseType(string expenseType)
        {
            var fetchData = new
            {
                statecode = "0",
                ig1_name = expenseType
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_expensecategories'>
                                <attribute name='ig1_expensecategoriesid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_name' operator='eq' value='{fetchData.ig1_name/*Sales Cost*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                return ec.Entities[0].Id;
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}
