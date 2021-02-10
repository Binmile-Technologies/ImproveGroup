using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace UpdateQuoteAmountForOrderEntry
{

    public class Updatequoteamountfromquote : IPlugin
    {
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;

        public void Execute(IServiceProvider serviceProvider)
        {

            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);

            try
            {
                   if (context.MessageName.ToLower() == "delete")
                     {
                    if (context.PreEntityImages.Contains("Image"))
                    {
                        Entity entity = (Entity)context.PreEntityImages["Image"];

                        if (entity.LogicalName == "quote")
                           {

                            EntityReference er = (EntityReference)entity.Attributes["opportunityid"];
                            Guid oppid = er.Id;
                            GetActivatedquoteamount(oppid);
                                   }
                           }
                    }

                   else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                     {


                    Entity entity = (Entity)context.InputParameters["Target"];

                    string entityName = entity.LogicalName;

                    if (entityName == "quote")
                    {
                        Entity fetchoppid = service.Retrieve(entityName, entity.Id, new ColumnSet("opportunityid"));
                        if (fetchoppid.Attributes.Contains("opportunityid") && fetchoppid.Attributes["opportunityid"] != null)
                        {

                            Guid oppid = fetchoppid.GetAttributeValue<EntityReference>("opportunityid").Id;

                            GetActivatedquoteamount(oppid);

                        }
                    }

                }


            }

            catch (Exception ex)
            {

                throw ex;
            }

        }


        public void GetActivatedquoteamount(Guid id)
        {

            var fetchData = new
            {
                opportunityid = id,
                statecode = "2",
                statecode2 = "1"
            };
            var fetchXml = $@"
                         <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                 <entity name='quote'>
                                 <attribute name='statecode' />
                                 <attribute name='totalamount' />
                                 <filter type='and'>
                                <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
                                <filter type='or'>
                                <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*2*/}'/>
                              <condition attribute='statecode' operator='eq' value='{fetchData.statecode2/*1*/}'/>
                        </filter>
                       </filter>
                       </entity>
                       </fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            decimal calculateamount = 0;
            Entity entobj = new Entity("opportunity", id);
            if (result.Entities.Count>0 && result.Entities != null)
              {
 
                for(int i=0;i< result.Entities.Count; i++)
                {

                    Money totalamount = new Money(0);

                    totalamount = result.Entities[i].GetAttributeValue<Money>("totalamount");
                    calculateamount = calculateamount + Convert.ToDecimal(totalamount.Value);
                }


               
                entobj["ig1_totalamountfromquote"] = new Money(calculateamount);
                service.Update(entobj);      
            }
            else
            {
                entobj["ig1_totalamountfromquote"] = new Money(0);
                service.Update(entobj);
            }
        }
    }
}
