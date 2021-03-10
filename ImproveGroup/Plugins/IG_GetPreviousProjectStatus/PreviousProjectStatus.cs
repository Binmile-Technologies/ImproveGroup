using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IG_GetPreviousProjectStatus
{
    public class PreviousProjectStatus : IPlugin
    {

        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(null);

                if (context.MessageName == "Update" )
                {
                    if (context.PreEntityImages.Contains("Image"))
                    {
                        Entity entity = (Entity)context.PreEntityImages["Image"];

                        if (entity.LogicalName == "ig1_projectrecord")
                        {

                            if (entity.Attributes.Contains("ig1_projectstatus") && entity.Attributes["ig1_projectstatus"] != null)
                            {
                                int previousstate = entity.GetAttributeValue<OptionSetValue>("ig1_projectstatus").Value;

                                Entity prjobj = new Entity(entity.LogicalName, entity.Id);
                                prjobj["ig1_holdpreviousstatus1"] = new OptionSetValue(previousstate);
                                service.Update(prjobj);
                            }

                        }
                    }

                }                  

            }
            catch(Exception ex)
            {

                throw new InvalidPluginExecutionException ("Error in PreviousProjectStatus Plugin " +ex);
            }

           
        }
    }
}
