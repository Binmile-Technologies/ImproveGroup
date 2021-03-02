using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_PODetailsToPOBill
{
    public class PODetailsToPOBill : IPlugin
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
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    EntityReference paymentTerms = null;
                    Money totalAmount = new Money(0);
                    Money amountBilled = new Money(0);

                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        Entity PO = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_paymentterm", "msdyn_totalamount", "msdyn_amountbilled"));

                        if (PO.Attributes.Contains("msdyn_paymentterm") && PO.Attributes["msdyn_paymentterm"] != null)
                        {
                            paymentTerms = (EntityReference)PO.Attributes["msdyn_paymentterm"];
                        }
                        if (PO.Attributes.Contains("msdyn_totalamount") && PO.Attributes["msdyn_totalamount"] != null)
                        {
                            totalAmount = (Money)PO.Attributes["msdyn_totalamount"];
                        }
                        if (PO.Attributes.Contains("msdyn_amountbilled") && PO.Attributes["msdyn_amountbilled"] != null)
                        {
                            amountBilled = (Money)PO.Attributes["msdyn_amountbilled"];
                        }
                        UpdatePOBill(entity.Id, paymentTerms, totalAmount, amountBilled);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in UpdatePaymentTermsToPOBill Plugin " + ex);
            }
        }
        protected void UpdatePOBill(Guid purchaseOrderId, EntityReference paymentTerms,Money totalAmount, Money amountBilled)
        {
            var fetchData = new
            {
                msdyn_purchaseorder = purchaseOrderId,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorderbill'>
                                <attribute name='msdyn_purchaseorder' />
                                <filter type='and'>
                                  <condition attribute='msdyn_purchaseorder' operator='eq' value='{fetchData.msdyn_purchaseorder/*00000000-0000-0000-0000-000000000000*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                foreach (Entity entity in entityCollection.Entities)
                {
                    entity.Attributes["msdyn_paymentterm"] = paymentTerms;
                    entity.Attributes["ig1_pototalamount"] = totalAmount;
                    entity.Attributes["ig1_pototalamountbilled"] = amountBilled;
                    service.Update(entity);
                }
            }
        }
    }
}
