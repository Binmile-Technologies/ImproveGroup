using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG_FetchAvalaraConfiguration
{
    public class FetchAvalaraConfiguration : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracingService;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);

            try
            {
                FetchAvalaraConfig();
            }
            catch (Exception ex)
            {
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in IG_FetchAvalaraConfiguration Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                service.Create(errorLog);
            }
        }
        protected void FetchAvalaraConfig()
        {
            var fetchData = new
            {
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_avalaraconfiguration'>
                                <attribute name='ig1_authorization' />
                                <attribute name='ig1_apiurl' />
                                <attribute name='ig1_currencycode' />
                                <attribute name='ig1_shipfromcountry' />
                                <attribute name='ig1_shipfromline1' />
                                <attribute name='ig1_shipfromcity' />
                                <attribute name='ig1_shipfromregion' />
                                <attribute name='ig1_companycode' />
                                <attribute name='ig1_shipfrompostalcode' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                                <order attribute='createdon'/>
                              </entity>
                            </fetch>";

            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                var result = entityCollection.Entities[0].Attributes;
                if (result.Contains("ig1_apiurl") && !string.IsNullOrEmpty(result["ig1_apiurl"].ToString()))
                {
                    context.OutputParameters["ApiUrl"] = result["ig1_apiurl"].ToString();
                }
                if (result.Contains("ig1_companycode") && !string.IsNullOrEmpty(result["ig1_companycode"].ToString()))
                {
                    context.OutputParameters["CompanyCode"] = result["ig1_companycode"].ToString();
                }
                if (result.Contains("ig1_currencycode") && !string.IsNullOrEmpty(result["ig1_currencycode"].ToString()))
                {
                    context.OutputParameters["CurrencyCode"] = result["ig1_currencycode"].ToString();
                }
                if (result.Contains("ig1_authorization") && !string.IsNullOrEmpty(result["ig1_authorization"].ToString()))
                {
                    context.OutputParameters["Authorization"] = result["ig1_authorization"].ToString();
                }
                if (result.Contains("ig1_shipfromline1") && !string.IsNullOrEmpty(result["ig1_shipfromline1"].ToString()))
                {
                    context.OutputParameters["ShipFromLine1"] = result["ig1_shipfromline1"].ToString();
                }
                if (result.Contains("ig1_shipfromcity") && !string.IsNullOrEmpty(result["ig1_shipfromcity"].ToString()))
                {
                    context.OutputParameters["ShipFromCity"] = result["ig1_shipfromcity"].ToString();
                }
                if (result.Contains("ig1_shipfromregion") && !string.IsNullOrEmpty(result["ig1_shipfromregion"].ToString()))
                {
                    context.OutputParameters["ShipFromRegion"] = result["ig1_shipfromregion"].ToString();
                }
                if (result.Contains("ig1_shipfromcountry") && !string.IsNullOrEmpty(result["ig1_shipfromcountry"].ToString()))
                {
                    context.OutputParameters["ShipFromCountry"] = result["ig1_shipfromcountry"].ToString();
                }
                if (result.Contains("ig1_shipfrompostalcode") && !string.IsNullOrEmpty(result["ig1_shipfrompostalcode"].ToString()))
                {
                    context.OutputParameters["ShipFromPostalCode"] = result["ig1_shipfrompostalcode"].ToString();
                }

            }
        }
    }
}
