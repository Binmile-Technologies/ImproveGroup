using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImproveGroup
{
    public class FilterContingencyGrid : IPlugin
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
            service = serviceFactory.CreateOrganizationService(context.UserId);
            #endregion
            try
            {
                if (context.MessageName != "RetrieveMultiple" || context.Stage != 20 || context.Mode != 0 || !context.InputParameters.Contains("Query") || !(context.InputParameters["Query"] is Microsoft.Xrm.Sdk.Query.FetchExpression))
                {
                    return;
                }
                if (context.InputParameters.Contains("Query"))
                {
                    string bidsheetId = string.Empty;
                    var query = ((Microsoft.Xrm.Sdk.Query.FetchExpression)context.InputParameters["Query"]).Query;
                    int IsContainsFlag = query.IndexOf("ZZZAAA");
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
                        var categoryId = GetcategoryId(bidsheetId);
                        FetchXmlToQueryExpressionRequest req = new FetchXmlToQueryExpressionRequest();
                        req.FetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                        "  <entity name='ig1_bidsheetpricelistitem'>" +
                                        "    <attribute name='ig1_bidsheetpricelistitemid' />" +
                                        "    <attribute name='ig1_product' />" +
                                        "    <attribute name='ig1_materialcost' />" +
                                        "    <attribute name='ig1_category' />" +
                                        "    <order attribute='ig1_category' descending='false' />" +
                                        "    <filter type='and'>" +
                                        "      <condition attribute='ig1_category' operator='eq' value='"+ categoryId + "' />" +
                                        "      <condition attribute='ig1_bidsheet' operator='eq' value='"+ bidsheetId + "' />" +
                                        "    </filter>" +
                                        "  </entity>" +
                                        "</fetch>";
                        FetchXmlToQueryExpressionResponse resp = (FetchXmlToQueryExpressionResponse)service.Execute(req);
                        context.InputParameters["Query"] = resp.Query;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in FilterContingencyGrid Plug-in.", ex);
            }

        }       
        public string GetcategoryId(string bidSheetId)
        {
            var categoryId = string.Empty;
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
                    if (category.Attributes.Contains("ig1_name") && category.Attributes["ig1_name"].ToString() == "Contingency")
                        categoryId = category.Attributes["ig1_bidsheetcategoryid"].ToString();
                }
            return categoryId;
        }
    }
}
