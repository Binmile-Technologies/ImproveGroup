using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IG_UpdateQuickbookIdWOPOInvoice
{
    public class UpdateQuickbookId:IPlugin
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
                    if (entity.LogicalName == "opportunity")
                    {

                       
                        String projenumber = string.Empty;
                        String qbid = string.Empty;
                        
                           Entity projectentity =GetProject(entity.Id);
                         
                            if (projectentity!=null && projectentity.Attributes.Contains("ig1_projectnumber") && projectentity.Attributes["ig1_projectnumber"] != null)
                               {

                                 projenumber= projectentity.Attributes["ig1_projectnumber"].ToString();
                            
                                }

                      
                          if (projectentity!= null && projectentity.Attributes.Contains("ig1_quickbooksid") && entity.Attributes["ig1_quickbooksid"] != null)
                           {
                            qbid = projectentity.GetAttributeValue<string>("ig1_quickbooksid");
                                }
                        if (!string.IsNullOrEmpty(qbid))
                        {
                            UpdateWorkorderquickbookId(entity.Id, qbid);
                            UpdatePoQuickbookId(entity.Id, qbid);
                            UpdatePOBillQuickbookID(entity.Id, qbid);
                            

                        }


                    }

                    else if(entity.LogicalName == "msdyn_workorder")
                    {
                        Guid oppid = Guid.Empty;
                        Entity quicbookid = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_opportunityid"));
                        if (quicbookid.Attributes.Contains("msdyn_opportunityid") && quicbookid.Attributes["msdyn_opportunityid"] != null)
                        {
                            oppid = quicbookid.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                            UpdateOpportunityQuickbookId(oppid,entity.Id);

                        }
                            
                       
                    }

                    else if(entity.LogicalName == "msdyn_purchaseorder")
                  
                    {
                        Guid Woid = Guid.Empty;
                        Entity quicbookid = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_workorder"));
                        if (quicbookid.Attributes.Contains("msdyn_workorder") && quicbookid.Attributes["msdyn_workorder"] != null)
                        {
                            Woid = quicbookid.GetAttributeValue<EntityReference>("msdyn_workorder").Id;
                            UpdatePOfromWO(Woid, entity.Id);

                        }
                        
                        
                    }


                    else if(entity.LogicalName == "msdyn_purchaseorderbill")
                    {
                        Guid poid = Guid.Empty;
                        Entity quicbookid = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("msdyn_purchaseorder"));
                        if (quicbookid.Attributes.Contains("msdyn_purchaseorder") && quicbookid.Attributes["msdyn_purchaseorder"] != null)
                        {
                            poid = quicbookid.GetAttributeValue<EntityReference>("msdyn_purchaseorder").Id;
                                                      
                            UpdatePOBillfronPO(poid, entity.Id);
                        }

                                                
                    }
                }




            }

            catch (Exception ex)
            {
                throw ex;


            }
        }

        public void UpdateWorkorderquickbookId(Guid oppid,string qbid)
        {

            var fetchData = new
            {
                msdyn_opportunityid = oppid,
                statecode = "0"
            };
            var fetchXml = $@"
                                <fetch>
                                <entity name='msdyn_workorder'>
                                <attribute name='msdyn_workorderid' />
                                <filter type='and'>
                                <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*b7187ce2-daf4-4978-b6cf-2a0f91a7c903*/}'/>
                                <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                                </entity>
                                </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if( result.Entities.Count > 0 && result.Entities != null )
            {                
                    foreach(var wo in result.Entities)
                {
                    wo.Attributes["ig1_quickbooksid"] = qbid;
                    service.Update(wo);

                }                   
                                  
                
            }

        }

        public void UpdatePoQuickbookId(Guid Oppid,string qbid)
        {
            	var fetchData = new {
		             opportunityid = Oppid
                };
	var fetchXml = $@"
                    <fetch>
                      <entity name='msdyn_purchaseorder'>
                        <attribute name='ig1_opportunityidname' />
                        <attribute name='ig1_opportunityid' />
                        <attribute name='msdyn_purchaseorderid' />
                        <attribute name='msdyn_name' />
                        <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                          <link-entity name='opportunity' from='opportunityid' to='msdyn_opportunityid'>
                            <filter>
                              <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*f334ecd1-ce7c-4148-8671-c9d593d9235a*/}'/>
                            </filter>
                          </link-entity>
                        </link-entity>
                      </entity>
                    </fetch>";



            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if ( result.Entities.Count > 0 && result.Entities != null )
            {
                foreach(var po in result.Entities)
                {   
                    po.Attributes["ig1_quickbooksid"] = qbid;
                    service.Update(po);
                }

                   

                

               

            }

        }

        public void UpdatePOBillQuickbookID(Guid oppid, string qbid)
        {
            var fetchData = new
            {
                opportunityid = oppid
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='msdyn_purchaseorderbill'>
                            <attribute name='msdyn_name' />
                            <link-entity name='msdyn_purchaseorder' from='msdyn_purchaseorderid' to='msdyn_purchaseorder'>
                              <link-entity name='msdyn_workorder' from='msdyn_workorderid' to='msdyn_workorder'>
                                <link-entity name='opportunity' from='opportunityid' to='msdyn_opportunityid'>
                                  <filter>
                                    <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*54123084-ac83-49b5-9921-54fdf0d53fa0*/}'/>
                                  </filter>
                                </link-entity>
                              </link-entity>
                            </link-entity>
                          </entity>
                        </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if ( result.Entities.Count > 0 && result.Entities != null )
            {
                
                   foreach(var pobil in result.Entities)
                 {
                    pobil.Attributes["ig1_quickbooksid"] = qbid;
                    service.Update(pobil);
                }
                   
                                                
            }


        }

        public void UpdateOpportunityQuickbookId(Guid oppid,Guid woid)
        {
            var fetchData = new
            {
                opportunityid = oppid
            };
            var fetchXml = $@"
                                <fetch>
                                <entity name='opportunity'>
                                <attribute name='ig1_quickbooksid' />
                                <filter>
                                <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b7187ce2-daf4-4978-b6cf-2a0f91a7c903*/}'/>
                                </filter>
                                </entity>
                                </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities[0].Contains("ig1_quickbooksid") && result.Entities[0].Attributes["ig1_quickbooksid"] != null)
            {
                String qbid = result.Entities[0].Attributes["ig1_quickbooksid"].ToString();

                Entity objwo = new Entity("msdyn_workorder", woid);
                objwo["ig1_quickbooksid"] = qbid;
                service.Update(objwo);
            }

               

        }
      
        public void UpdatePOfromWO(Guid Woid,Guid poid)
        {
            var fetchData = new
            {
                msdyn_workorderid = Woid,
                statecode = "0"
            };
            var fetchXml = $@"
                        <fetch>
                          <entity name='msdyn_workorder'>
                            <attribute name='ig1_quickbooksid' />
                            <filter>
                              <condition attribute='msdyn_workorderid' operator='eq' value='{fetchData.msdyn_workorderid/*d1f98e8d-32da-e911-a960-000d3a1d5d58*/}'/>
                              <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                            </filter>
                          </entity>
                        </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities[0].Contains("ig1_quickbooksid") && result.Entities[0].Attributes["ig1_quickbooksid"] != null)
            {
                String qbid = result.Entities[0].Attributes["ig1_quickbooksid"].ToString();

                Entity objpo = new Entity("msdyn_purchaseorder", poid);
                objpo["ig1_quickbooksid"] = qbid;
                service.Update(objpo);
            }


        }

        public void UpdatePOBillfronPO(Guid poid,Guid pobilid)
        {

            var fetchData = new
            {
                msdyn_purchaseorderid = poid,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch>
                              <entity name='msdyn_purchaseorder'>
                                <attribute name='ig1_quickbooksid' />
                                <filter>
                                  <condition attribute='msdyn_purchaseorderid' operator='eq' value='{fetchData.msdyn_purchaseorderid/*725881fa-7bf2-4821-b2b4-5316839543be*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities[0].Contains("ig1_quickbooksid") && result.Entities[0].Attributes["ig1_quickbooksid"] != null)
            {
                String qbid = result.Entities[0].Attributes["ig1_quickbooksid"].ToString();
                Entity objpob = new Entity("msdyn_purchaseorderbill", pobilid);
                objpob["ig1_quickbooksid"] = qbid;
                service.Update(objpob);
            }
            }

        public Entity GetProject(Guid oppid)
        {
            Entity result1 = null;

            var fetchData = new
            {
                opportunityid = oppid
            };
            var fetchXml = $@"
                                <fetch>
                                <entity name='opportunity'>
    <attribute name='ig1_projectnumber' />
    <attribute name='ig1_projectrecord' />
    <attribute name='ig1_quickbooksid' />
    <filter>
      <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*4932ab61-39cf-e911-a968-000d3a1d5d97*/}'/>
    </filter>
  </entity>
</fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if(result.Entities.Count>0)
            {
                result1= result.Entities[0];

            }
            return result1;
        }
          
    }
}
