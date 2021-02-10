using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdatePoTotalAmountInBill
{
    public class UpdatePoamountinPoBill:IPlugin
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
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {

                    Entity entity = (Entity)context.InputParameters["Target"];

                    if (entity.LogicalName == "msdyn_purchaseorderbill")
                    {
                        Entity poentity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_purchaseorder"));
                        if(poentity.Attributes.Contains("msdyn_purchaseorder") && entity.Attributes["msdyn_purchaseorder"] != null)
                        {
                            Guid poid = poentity.GetAttributeValue<EntityReference>("msdyn_purchaseorder").Id;
                            Money amount = GetPOtotalamount(poid);
                            Money amountbilled = GetPObilledamount(poid);
                            entity["ig1_pototalamount"] = amount.Value;
                            entity["ig1_pototalamountbilled"] = amountbilled.Value;
                            service.Update(entity);
                        }
                       
                       

                    }

                    else if(entity.LogicalName == "msdyn_purchaseorder")
                    {
                        Money money = new Money(0);
                        Money billamount = new Money(0);
                        Entity pobil=service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_totalamount", "msdyn_amountbilled"));
                        if(pobil.Attributes.Contains("msdyn_totalamount") && pobil.Attributes["msdyn_totalamount"] != null)
                        {
                            money = pobil.GetAttributeValue<Money>("msdyn_totalamount");
                            
                        }

                        if(pobil.Attributes.Contains("msdyn_amountbilled") && pobil.Attributes["msdyn_amountbilled"] != null)
                        {
                            billamount = pobil.GetAttributeValue<Money>("msdyn_amountbilled");

                        }
                       
                        UpdatePOBillAmount(entity.Id, money, billamount);


                    }


                }


            }



            catch(Exception ex)
            {

                throw ex;
            }

            
        }

        public void UpdatePOBillAmount(Guid poid, Money amount,Money billamounts)
        {
            var fetchData = new
            {
                msdyn_purchaseorder = poid,
                statecode = "0"
            };
            var fetchXml = $@"
                    <fetch>
                      <entity name='msdyn_purchaseorderbill'>
                        <attribute name='msdyn_purchaseorderbillid' />
                        <attribute name='ig1_pototalamount' />
                        <filter>
                          <condition attribute='msdyn_purchaseorder' operator='eq' value='{fetchData.msdyn_purchaseorder/*af5f28e4-918d-4889-9d98-e9024b9e37f1*/}'/>
                          <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                        </filter>
                      </entity>
                    </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));


             if ( result.Entities.Count > 0 && result.Entities != null )
            {

                for (int i = 0; i < result.Entities.Count; i++)
                {
                    if(result.Entities[i].Attributes.Contains("msdyn_purchaseorderbillid") && result.Entities[i].Attributes["msdyn_purchaseorderbillid"] != null)
                    {

                        Guid id = result.Entities[i].GetAttributeValue<Guid>("msdyn_purchaseorderbillid");
                        Entity pobil = new Entity("msdyn_purchaseorderbill", id);
                        pobil.Attributes["ig1_pototalamount"] = amount.Value;
                       // pobil.Attributes["ig1_pototalamountbilled"] = billamounts.Value;
                        service.Update(pobil);

                    }
                    

                }
                                                
            }

        }

        public Money GetPOtotalamount(Guid Id)
        {
            Money pototal = new Money(0);

            var fetchData = new
            {
                msdyn_purchaseorderid = Id
            };
            var fetchXml = $@"
                <fetch>
                  <entity name='msdyn_purchaseorder'>
                    <attribute name='msdyn_totalamount' />
                    <filter>
                      <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*f48d50db-1f6e-4397-8694-18fac4664b4b*/}'/>
                    </filter>
                  </entity>
                </fetch>";
              EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (result.Entities[0].Attributes.Contains("msdyn_totalamount") && result.Entities[0].Attributes["msdyn_totalamount"] !=null)
            {

                pototal = result.Entities[0].GetAttributeValue<Money>("msdyn_totalamount");

            }


            return pototal;
        }

        public Money GetPObilledamount(Guid id)
        {
            Money poamountbill = new Money(0);

            var fetchData = new
            {
                msdyn_purchaseorderid = id
            };
            var fetchXml = $@"
                <fetch>
                  <entity name='msdyn_purchaseorder'>
                    <attribute name='msdyn_amountbilled' />
                    <filter>
                      <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*f48d50db-1f6e-4397-8694-18fac4664b4b*/}'/>
                    </filter>
                  </entity>
                </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (result.Entities[0].Attributes.Contains("msdyn_amountbilled") && result.Entities[0].Attributes["msdyn_amountbilled"] != null)
            {

                poamountbill = result.Entities[0].GetAttributeValue<Money>("msdyn_amountbilled");

            }



            return poamountbill;
        }


    }
}
