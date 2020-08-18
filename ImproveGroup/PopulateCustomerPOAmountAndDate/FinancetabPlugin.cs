using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopulateCustomerPOAmountAndDate
{
   public class FinancetabPlugin: IPlugin
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

                if(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters["Target"];
                    if (entity.LogicalName != "ig1_projectrecord")
                    {
                        return;
                    }

                   UpdateFinancetabfields(entity.Id, entity);
                }

            }

            catch(Exception ex)
            {

                throw ex;
            }


            
        }

        protected void UpdateFinancetabfields(Guid id,Entity entity)
        {
            entity[""]=
            entity[""] =
            entity[""] =
            entity[""] =
            entity[""] =
            entity[""] =
            entity[""] =
            entity[""] =
            entity[""] =

        }
    }
}
