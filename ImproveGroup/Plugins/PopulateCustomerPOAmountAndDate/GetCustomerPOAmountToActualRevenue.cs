using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PopulateCustomerPOAmountAndDate
{
    public class GetCustomerPOAmountToActualRevenue : IPlugin
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

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    
                    Guid opportunityId = Guid.Empty;
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "opportunityclose")
                    {
                        return;
                    }

                    Entity opportunityClose = service.Retrieve("opportunityclose", entity.Id, new ColumnSet("opportunityid"));
                    if (opportunityClose.Attributes.Contains("opportunityid") && opportunityClose.Attributes["opportunityid"]!=null)
                    {
                        EntityReference opportunity = (EntityReference)opportunityClose.Attributes["opportunityid"];
                        opportunityId = opportunity.Id;
                    }

                    UpdateOppClose(opportunityId, opportunityClose);

                }
            }

            catch (Exception e)
            {

                throw e;
            }
        }

        protected void UpdateOppClose(Guid opportunityid, Entity opportunityClose)
        {
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                                     <fetch>
                                     <entity name='opportunity'>
                                     <attribute name='ig1_customerpodate' />
                                     <attribute name='ig1_customerpoamount' />
                                     <filter type='and'>
                                     <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid}'/>
                                     </filter>
                                     </entity>
                                     </fetch>";
            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            
            if (result.Entities.Count>0)
            {
                var record = result.Entities[0].Attributes;
                
                if (record.Contains("ig1_customerpoamount") && record["ig1_customerpoamount"] !=null)
                {
                    Money money = (Money)record["ig1_customerpoamount"];

                    opportunityClose.Attributes["actualrevenue"] = money;
                }
                if (record.Contains("ig1_customerpodate") && record["ig1_customerpodate"]!=null)
                {
                    opportunityClose.Attributes["actualend"] = Convert.ToDateTime(record["ig1_customerpodate"]);
                }
                service.Update(opportunityClose);      
            }
        }
    }
}
