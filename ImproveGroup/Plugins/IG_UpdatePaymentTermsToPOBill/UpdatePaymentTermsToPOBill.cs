using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG_UpdatePaymentTermsToPOBill
{
    public class UpdatePaymentTermsToPOBill : IPlugin
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
                    if (entity.LogicalName == "msdyn_purchaseorder")
                    {
                        if (entity.Attributes.Contains("msdyn_paymentterm") && entity.Attributes["msdyn_paymentterm"] != null)
                        {
                            paymentTerms =(EntityReference)entity.Attributes["msdyn_paymentterm"];
                        }
                        UpdateBillPaymentTerms(entity.Id, paymentTerms);
                    }
                    else if (entity.LogicalName == "msdyn_purchaseorderbill")
                    {
                        if (entity.Attributes.Contains("msdyn_purchaseorder") && entity.Attributes["msdyn_purchaseorder"] != null)
                        {
                            EntityReference entityReference = (EntityReference)entity.Attributes["msdyn_purchaseorder"];
                            paymentTerms = GetPaymentTerms(entityReference.Id);
                        }
                        entity.Attributes["msdyn_paymentterm"] = paymentTerms;
                        service.Update(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in UpdatePaymentTermsToPOBill Plugin " + ex);
            }
        }
        protected EntityReference GetPaymentTerms(Guid purchaseOrderId)
        {
            EntityReference paymentTerms = null;
            var fetchData = new
            {
                msdyn_purchaseorderid = purchaseOrderId
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorder'>
                                <attribute name='msdyn_paymentterm' />
                                <filter>
                                  <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*msdyn_purchaseorder*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                Entity entity = entityCollection.Entities[0];
                if (entity.Attributes.Contains("msdyn_paymentterm") && entity.Attributes["msdyn_paymentterm"] != null)
                {
                    paymentTerms = (EntityReference)entity.Attributes["msdyn_paymentterm"];
                }
            }
            return paymentTerms;
        }
        protected void UpdateBillPaymentTerms(Guid purchaseOrderId, EntityReference paymentTerms)
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
                                <attribute name='msdyn_paymentterm' />
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
                    service.Update(entity);
                }
            }
        }
    }
}
