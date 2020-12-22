using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace IG_CloneOpportunity
{
    public class CloneOpportunity : IPlugin
    {
        IOrganizationService service;
        IOrganizationServiceFactory serviceFactory;
        IPluginExecutionContext context;
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

                if (context.MessageName == "ig1_CloneOpportunity" && context.InputParameters != null && context.InputParameters.Contains("opportunityid"))
                {
                    Guid opportunityid = new Guid(context.InputParameters["opportunityid"].ToString());
                    if (opportunityid != Guid.Empty)
                    {
                        Clone_Opportunity(opportunityid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in CloneOpportunity plugin " + ex);
            }

        }
        protected void Clone_Opportunity(Guid opportunityid)
        {
            string name = string.Empty;
            var fetchData = new
            {
                opportunityid = opportunityid
            };
            var fetchXml = $@"
                            <fetch>
                                <entity name='opportunity'>
                                <attribute name='ig1_notesonterms' />
                                <attribute name='parentaccountid' />
                                <attribute name='ig1_termswithvendors' />
                                <attribute name='new_estprojectcompletion' />
                                <attribute name='ig1_termscustomerprime' />
                                <attribute name='ig1_buyfromig' />
                                <attribute name='ig1_leadtimematerials' />
                                <attribute name='ig1_qualfunding' />
                                <attribute name='new_defenseprocurementmethod' />
                                <attribute name='ig1_billingaccount' />
                                <attribute name='estimatedclosedate' />
                                <attribute name='estimatedvalue' />
                                <attribute name='name' />
                                <attribute name='contactid' />
                                <attribute name='ig1_leadtimeinstallation' />
                                <attribute name='accountid' />
                                <attribute name='ig1_howtheyuse' />
                                <attribute name='ig1_visiondetails' />
                                <attribute name='ig1_pricinglevel' />
                                <attribute name='ig1_aargoals' />
                                <attribute name='customerneed' />
                                <attribute name='ig1_whoiscustomer' />
                                <attribute name='ig1_projecttype' />
                                <attribute name='ig1_installcountry' />
                                <attribute name='ig1_installstreet1' />
                                <attribute name='ig1_entityusecode' />
                                <attribute name='ig1_installstreet2' />
                                <attribute name='ig1_installsiteshipcontact' />
                                <attribute name='ig1_installstreet3' />
                                <attribute name='ig1_installstateprovince' />
                                <attribute name='ig1_installcity' />
                                <attribute name='ig1_resaleorprimegc' />
                                <attribute name='ig1_contractcustomertype' />
                                <attribute name='ig1_installzippostalcode' />
                                <attribute name='ig1_projectnumber' />
                                <filter>
                                    <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b3ed5104-7606-41c4-aec3-fd2ee5817adb*/}'/>
                                </filter>
                                </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                string projectNumber = string.Empty;
                var result = entityCollection.Entities[0].Attributes;
                Entity entity = new Entity("opportunity");
                if (result.Contains("ig1_projectnumber") && result["ig1_projectnumber"] != null)
                {
                    projectNumber = result["ig1_projectnumber"].ToString();
                }
                if (result.Contains("ig1_notesonterms") && result["ig1_notesonterms"] != null)
                {
                    entity.Attributes["ig1_notesonterms"] = result["ig1_notesonterms"];
                }
                if (result.Contains("parentaccountid") && result["parentaccountid"] != null)
                {
                    entity.Attributes["parentaccountid"] = result["parentaccountid"];
                }
                if (result.Contains("ig1_termswithvendors") && result["ig1_termswithvendors"] != null)
                {
                    entity.Attributes["ig1_termswithvendors"] = result["ig1_termswithvendors"];
                }
                if (result.Contains("new_estprojectcompletion") && result["new_estprojectcompletion"] != null)
                {
                    entity.Attributes["new_estprojectcompletion"] = result["new_estprojectcompletion"];
                }
                if (result.Contains("ig1_termscustomerprime") && result["ig1_termscustomerprime"] != null)
                {
                    entity.Attributes["ig1_termscustomerprime"] = result["ig1_termscustomerprime"];
                }
                if (result.Contains("ig1_buyfromig") && result["ig1_buyfromig"] != null)
                {
                    entity.Attributes["ig1_buyfromig"] = result["ig1_buyfromig"];
                }
                if (result.Contains("ig1_leadtimematerials") && result["ig1_leadtimematerials"] != null)
                {
                    entity.Attributes["ig1_leadtimematerials"] = result["ig1_leadtimematerials"];
                }
                if (result.Contains("ig1_qualfunding") && result["ig1_qualfunding"] != null)
                {
                    entity.Attributes["ig1_qualfunding"] = result["ig1_qualfunding"];
                }
                if (result.Contains("new_defenseprocurementmethod") && result["new_defenseprocurementmethod"] != null)
                {
                    entity.Attributes["new_defenseprocurementmethod"] = result["new_defenseprocurementmethod"];
                }
                if (result.Contains("ig1_billingaccount") && result["ig1_billingaccount"] != null)
                {
                    entity.Attributes["ig1_billingaccount"] = result["ig1_billingaccount"];
                }
                if (result.Contains("estimatedclosedate") && result["estimatedclosedate"] != null)
                {
                    entity.Attributes["estimatedclosedate"] = result["estimatedclosedate"];
                }
                if (result.Contains("estimatedvalue") && result["estimatedvalue"] != null)
                {
                    entity.Attributes["estimatedvalue"] = result["estimatedvalue"];
                }
                if (result.Contains("name") && result["name"] != null)
                {
                    entity.Attributes["name"] = "Cloned_" + projectNumber;
                }
                if (result.Contains("contactid") && result["contactid"] != null)
                {
                    entity.Attributes["contactid"] = result["contactid"];
                }
                if (result.Contains("ig1_leadtimeinstallation") && result["ig1_leadtimeinstallation"] != null)
                {
                    entity.Attributes["ig1_leadtimeinstallation"] = result["ig1_leadtimeinstallation"];
                }
                if (result.Contains("accountid") && result["accountid"] != null)
                {
                    entity.Attributes["accountid"] = result["accountid"];
                }
                if (result.Contains("ig1_howtheyuse") && result["ig1_howtheyuse"] != null)
                {
                    entity.Attributes["ig1_howtheyuse"] = result["ig1_howtheyuse"];
                }
                if (result.Contains("ig1_visiondetails") && result["ig1_visiondetails"] != null)
                {
                    entity.Attributes["ig1_visiondetails"] = result["ig1_visiondetails"];
                }
                if (result.Contains("ig1_pricinglevel") && result["ig1_pricinglevel"] != null)
                {
                    entity.Attributes["ig1_pricinglevel"] = result["ig1_pricinglevel"];
                }
                if (result.Contains("ig1_aargoals") && result["ig1_aargoals"] != null)
                {
                    entity.Attributes["ig1_aargoals"] = result["ig1_aargoals"];
                }
                if (result.Contains("customerneed") && result["customerneed"] != null)
                {
                    entity.Attributes["customerneed"] = result["customerneed"];
                }
                if (result.Contains("ig1_whoiscustomer") && result["ig1_whoiscustomer"] != null)
                {
                    entity.Attributes["ig1_whoiscustomer"] = result["ig1_whoiscustomer"];
                }
                if (result.Contains("ig1_projecttype") && result["ig1_projecttype"] != null)
                {
                    entity.Attributes["ig1_projecttype"] = result["ig1_projecttype"];
                }
                if (result.Contains("ig1_installcountry") && result["ig1_installcountry"] != null)
                {
                    entity.Attributes["ig1_installcountry"] = result["ig1_installcountry"];
                }
                if (result.Contains("ig1_installstreet1") && result["ig1_installstreet1"] != null)
                {
                    entity.Attributes["ig1_installstreet1"] = result["ig1_installstreet1"];
                }
                if (result.Contains("ig1_entityusecode") && result["ig1_entityusecode"] != null)
                {
                    entity.Attributes["ig1_entityusecode"] = result["ig1_entityusecode"];
                }
                if (result.Contains("ig1_installstreet2") && result["ig1_installstreet2"] != null)
                {
                    entity.Attributes["ig1_installstreet2"] = result["ig1_installstreet2"];
                }
                if (result.Contains("ig1_installsiteshipcontact") && result["ig1_installsiteshipcontact"] != null)
                {
                    entity.Attributes["ig1_installsiteshipcontact"] = result["ig1_installsiteshipcontact"];
                }
                if (result.Contains("ig1_installstreet3") && result["ig1_installstreet3"] != null)
                {
                    entity.Attributes["ig1_installstreet3"] = result["ig1_installstreet3"];
                }
                if (result.Contains("ig1_installstateprovince") && result["ig1_installstateprovince"] != null)
                {
                    entity.Attributes["ig1_installstateprovince"] = result["ig1_installstateprovince"];
                }
                if (result.Contains("ig1_installcity") && result["ig1_installcity"] != null)
                {
                    entity.Attributes["ig1_installcity"] = result["ig1_installcity"];
                }
                if (result.Contains("ig1_resaleorprimegc") && result["ig1_resaleorprimegc"] != null)
                {
                    entity.Attributes["ig1_resaleorprimegc"] = result["ig1_resaleorprimegc"];
                }
                if (result.Contains("ig1_contractcustomertype") && result["ig1_contractcustomertype"] != null)
                {
                    entity.Attributes["ig1_contractcustomertype"] = result["ig1_contractcustomertype"];
                }
                if (result.Contains("ig1_installzippostalcode") && result["ig1_installzippostalcode"] != null)
                {
                    entity.Attributes["ig1_installzippostalcode"] = result["ig1_installzippostalcode"];
                }

                Guid clonedOppoertunityid = service.Create(entity);
                if (clonedOppoertunityid != Guid.Empty)
                {
                    context.OutputParameters["clonedopportunityid"] = Convert.ToString(clonedOppoertunityid);
                }
            }
        }
    }
}
