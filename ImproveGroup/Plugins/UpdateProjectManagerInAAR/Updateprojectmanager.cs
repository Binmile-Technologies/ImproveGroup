using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateProjectManagerInAAR
{
    public class Updateprojectmanager: IPlugin
    {


        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        Entity objentity = null;
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

                    string entityName = entity.LogicalName;

                    if (entityName == "msdyn_workorder")
                    {

                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;

                        objentity = new Entity("ig1_projectrecord", projid);

                        SetProjectowner(projid);

                    }


                }


            }

            catch(Exception ex)
            {
                throw ex;

            }

        }


        public void SetProjectowner(Guid id)
        {

            var fetchData = new
            {
                ig1_projectrecord = id,
                statecode = "0"
            };
            var fetchXml = $@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='msdyn_workorder'>
                <attribute name='ownerid' />
                <attribute name='createdon' />
                <filter type='and'>
                <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*3503359b-37d2-ea11-a812-000d3a55da4f*/}'/>
                <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                </filter>
               <order attribute='createdon' />
              </entity>
              </fetch>";





            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result.Entities.Count > 0)
            {
                EntityReference entref = (EntityReference)result.Entities[0].Attributes["ownerid"];
                var ownername = entref.LogicalName;
                var ownerid = entref.Id;
                // string ownername =result.Entities[result.Entities.Count - 1].GetAttributeValue<EntityReference>("ownerid").Name;
                objentity["ig1_projectmanager1"] = new EntityReference(ownername, ownerid);
                service.Update(objentity);
            }


            else
            {
                objentity["ig1_projectmanager1"] = null;
                service.Update(objentity);
            }
        }
    }
}
