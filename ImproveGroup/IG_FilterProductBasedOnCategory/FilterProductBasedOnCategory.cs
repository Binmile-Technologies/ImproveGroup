using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG_FilterProductBasedOnCategory
{
    public class FilterProductBasedOnCategory: IPlugin
    {
        ITracingService tracingService;
        IPluginExecutionContext context;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Setup
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);
            #endregion
            try
            {
                if (context.MessageName != "RetrieveMultiple" || context.Stage != 40 || context.Mode != 0 || !context.InputParameters.Contains("Query") || !(context.InputParameters["Query"] is Microsoft.Xrm.Sdk.Query.FetchExpression))
                {
                    return;
                }
                if (context.InputParameters.Contains("Query"))
                {
                        string bidsheetId = string.Empty;
                        var query = ((Microsoft.Xrm.Sdk.Query.FetchExpression)context.InputParameters["Query"]).Query;
                    int IsContainsFlag = query.IndexOf("FilterProd");
                    if (IsContainsFlag != -1)
                    {
                        if (context.InputParameters["Query"] is FetchExpression)
                        {
                            FetchExpression fetchExpression = context.InputParameters["Query"] as FetchExpression;
                            FetchXmlToQueryExpressionRequest fetchXmlToQueryExpressionRequest = new FetchXmlToQueryExpressionRequest()
                            {
                                FetchXml = fetchExpression.Query
                            };
                            FetchXmlToQueryExpressionResponse fetchXmlToQueryExpressionResponse = (service.Execute(fetchXmlToQueryExpressionRequest) as FetchXmlToQueryExpressionResponse);

                            foreach (var filter in fetchXmlToQueryExpressionResponse.Query.LinkEntities)
                            {
                                foreach (var condition in filter.LinkCriteria.Conditions)
                                {
                                    if (condition.AttributeName == "ig1_bidsheetid")
                                        bidsheetId = condition.Values[0].ToString();
                                }
                            }
                        }
                        string conditions = GetConditions("b88439fd-1efc-49aa-9e0b-365742715e33");
                        FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest();
                        req.FetchXml = $@"<fetch>
                                            <entity name='product'>
                                            <attribute name='name' />
                                            <attribute name='ig1_bidsheetcategory' />
                                            <filter type='or'>
                                            {conditions}
                                            </filter>
                                            <order attribute='ig1_bidsheetcategory' />
                                            </entity>
                                        </fetch>";

                        FetchXmlToQueryExpressionResponse resp = (FetchXmlToQueryExpressionResponse)service.Execute(req);
                        context.InputParameters["Query"] = resp.Query;
                    }
                }
            }
            catch (Exception ex)
            {
                IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);
                Entity errorLog = new Entity("ig1_pluginserrorlogs");
                errorLog["ig1_name"] = "An error occurred in FilterProductBasedOnCategory Plug-in";
                errorLog["ig1_errormessage"] = ex.Message;
                errorLog["ig1_errordescription"] = ex.ToString();
                serviceAdmin.Create(errorLog);
            }

        }
        public string GetConditions(string bidSheetId)
        {
            string conditions = string.Empty;
            var fetchXml = $@"<fetch mapping='logical' version='1.0'>   
                        <entity name='ig1_bidsheetcategory'>   
                          <attribute name='ig1_name' />   
                          <attribute name='ig1_bidsheetcategoryid' />   
                          <link-entity name='ig1_bscategoryvendor' from='ig1_category' to='ig1_bidsheetcategoryid' link-type='inner' alias='ak'>   
                          <filter type='and'>   
                            <condition attribute='ig1_bidsheet' operator='eq' value='{bidSheetId}'/>   
                            <condition attribute='statecode' operator='eq' value='0'/>   
                          </filter>   
                        </link-entity>   
                        </entity>   
                      </fetch>";
            EntityCollection categoryData = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (categoryData.Entities.Count > 0)
                foreach (Entity category in categoryData.Entities)
                {
                    if (category.Attributes.Contains("ig1_name") && category.Attributes["ig1_name"].ToString() != "Contingency" && category.Attributes["ig1_name"].ToString() != "Labor")
                    {
                        
                        if (category.Id!=Guid.Empty)
                        {
                            conditions += $@"<condition attribute = 'ig1_bidsheet' operator= 'eq' value = '{category.Id}' />";
                        }
                    }
                }
            return conditions;
        }

    }
}
