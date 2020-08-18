using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IG_UpdateFinanceTabValues
{
    public class Updatefinancetab : IPlugin
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

                if (context.MessageName.ToLower() == "delete")
                {

                    if (context.PreEntityImages.Contains("Image"))
                    {
                        Entity entity = (Entity)context.PreEntityImages["Image"];

                        if (entity.LogicalName == "invoice")
                        {
                            EntityReference er = (EntityReference)entity.Attributes["opportunityid"];
                            Guid oppid = er.Id;
                            Entity result = service.Retrieve("opportunity", oppid, new ColumnSet("ig1_projectrecord"));
                            Guid projid = result.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                            objentity = new Entity("ig1_projectrecord", projid);
                            GetInvoiceEntityfields(oppid);
                        }

                        else if(entity.LogicalName == "ig1_projectrecordcost")
                        {

                            EntityReference er = (EntityReference)entity.Attributes["ig1_projectrecord"];
                            Guid projid = er.Id;

                            //Entity fetchentity = service.Retrieve(entity.LogicalName, entity.Id, new ColumnSet("ig1_projectrecord"));
                            //Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                            objentity = new Entity("ig1_projectrecord", projid);
                            GetActualcostentityfields(projid);

                        }
                    }


                }


              else  if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {


                    Entity entity = (Entity)context.InputParameters["Target"];

                    string entityName = entity.LogicalName;


                    if (entityName == "opportunity")
                    {
                        Guid projid = Guid.Empty;
                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        if (fetchentity.Attributes.Contains("ig1_projectrecord") && fetchentity.Attributes["ig1_projectrecord"] != null)
                        {
                            projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                            objentity = new Entity("ig1_projectrecord", projid);
                            GetOpportunityEntityfields(entity.Id);
                        }
                    }

                    else if (entityName == "invoice")
                    {
                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord", "opportunityid"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                        Guid oppid = fetchentity.GetAttributeValue<EntityReference>("opportunityid").Id;
                        objentity = new Entity("ig1_projectrecord", projid);

                        GetInvoiceEntityfields(oppid);
                    }

                    else if (entityName == "quote")
                    {

                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord", "opportunityid"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                        Guid oppid = fetchentity.GetAttributeValue<EntityReference>("opportunityid").Id;
                        objentity = new Entity("ig1_projectrecord", projid);
                        GetQuoteEntityfieldcost(oppid);

                    }

                    else if (entityName == "ig1_bidsheet")
                    {
                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord", "ig1_opportunitytitle"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                        Guid id = fetchentity.GetAttributeValue<EntityReference>("ig1_opportunitytitle").Id;
                        objentity = new Entity("ig1_projectrecord", projid);
                        GetBidsheetEntityfieldcost(id);
                        
                    }

                    else if (entityName == "ig1_projectrecordcost")
                    {
                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                        objentity = new Entity("ig1_projectrecord", projid);
                        GetActualcostentityfields(projid);

                    }

                    else if(entityName== "msdyn_workorder")
                    {
                        Entity fetchentity = service.Retrieve(entityName, entity.Id, new ColumnSet("ig1_projectrecord", "msdyn_opportunityid"));
                        Guid projid = fetchentity.GetAttributeValue<EntityReference>("ig1_projectrecord").Id;
                        Guid id = fetchentity.GetAttributeValue<EntityReference>("msdyn_opportunityid").Id;
                        objentity = new Entity("ig1_projectrecord", projid);
                        Getworkorderfield(id);

                    }

                }

            }


            catch (Exception ex)
            {


                throw ex;
            }
        }

        public void GetActualcostentityfields(Guid id)
        {
            var fetchData = new
            {
                ig1_projectrecord = id
            };
            var fetchXml = $@"
                  <fetch>
                  <entity name='ig1_projectrecordcost'>
                  <attribute name='ig1_expensetype' />
                  <attribute name='ig1_amount' />
                  <filter type='and'>
                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*a5c627ce-a3bf-ea11-a812-000d3a55d7ac*/}'/>
                 </filter>
                 </entity>
                 </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            decimal hardcost = 0;
            decimal softcost = 0;
            decimal actualtotal = 0;
            decimal actualtotalpobills = 0;


            if (result.Entities != null && result.Entities.Count > 0)
            {
                for (int i = 0; i < result.Entities.Count; i++)
                {

                    if (result.Entities[i].Attributes["ig1_expensetype"] != null && result.Entities[i].Attributes.Contains("ig1_expensetype"))
                    {

                        string expensetype = Convert.ToString(result.Entities[i].GetAttributeValue<EntityReference>("ig1_expensetype").Name);

                        if (expensetype == "IG Install PO Bill" || expensetype == "Travel Cost" || expensetype == "PO Bill")
                        {

                            if (expensetype == "PO Bill")
                            {
                                Money poamount = new Money(0);
                                poamount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");
                                actualtotalpobills = actualtotalpobills + Convert.ToDecimal(poamount.Value);

                            }
                            
                            
                                Money hamount = new Money(0);
                                hamount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");
                                hardcost = hardcost + Convert.ToDecimal(hamount.Value);
                            
                        }

                        else if (expensetype == "Design Cost" || expensetype == "Miscellaneous" || expensetype == "PM Cost" || expensetype == "Sales Cost")
                        {
                            Money samount = new Money(0);
                            samount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");
                            softcost = softcost + Convert.ToDecimal(samount.Value);

                        }
                      

                        Money actamount = new Money(0);
                        actamount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");

                        actualtotal = actualtotal + Convert.ToDecimal(actamount.Value);
                    }


                }

            }

            Entity resultoppid= service.Retrieve("ig1_projectrecord", id, new ColumnSet("ig1_opportunity"));
            Guid oppid  =resultoppid.GetAttributeValue<EntityReference>("ig1_opportunity").Id;
            decimal totalofinvoice = TotalAllinvoices(oppid);
            IList<Decimal> cost = Hardcostandsoftcost(id);
            decimal actualgp = 0;
            decimal actualgpper = 0;
            decimal actualnetper = 0;
            decimal actualnet = 0;
            decimal commissionableamount = 0;

               

              
            if (cost.Count > 0 && cost != null)
            {

                actualgp = totalofinvoice - (cost[0] + cost[1]);
            }
            if (actualgp > 0)
            {


                actualgpper = (actualgp / totalofinvoice) * 100;
            }
            if (actualgpper > 0)
            {
                decimal gna = 0;
                gna =CorpGna();

                actualnetper = actualgpper - gna;
            }
            if (actualnetper > 0)
            {
             decimal roundvalue=  decimal.Round(actualnetper, 2);
                decimal roundvalueinvoice = decimal.Round(totalofinvoice,2);

                actualnet = (roundvalue * roundvalueinvoice) /100;
            }
            if (actualnet > 0)
            {
                commissionableamount = actualnet * Convert.ToDecimal(0.3);
            }
            objentity["ig1_actualgp"] = actualgp;
            objentity["ig1_commisionableamount"] = commissionableamount;
            objentity["ig1_actualnetpercentage"] = actualnetper;
            objentity["ig1_actualgppercent"] = actualgpper;
            objentity["ig1_actualnet"] = actualnet;
            objentity["ig1_totalallinvoices"] = totalofinvoice;
           // service.Update(objentity);




            objentity["ig1_actualtotal"] = actualtotal;
            objentity["ig1_anticipatedsoftcosts"] = softcost;
            objentity["ig1_anticipatedmaterialcosts"] = hardcost;
            objentity["ig1_actualtotalpobills"] = actualtotalpobills;
            service.Update(objentity);

        }

        public void GetInvoiceEntityfields(Guid id)
        {
            var fetchData = new
            {
                opportunityid = id
            };
            var fetchXml = $@"
                           <fetch>
          <entity name='invoice'>
    <attribute name='new_dbms_qbpaid_amount' />
    <attribute name='totalamount' />
     <attribute name='totaltax' />
    <attribute name='statecode' />
    <filter>
      <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
    </filter>
  </entity>
</fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Decimal totalpaidinvoice = 0;
            Decimal totalopeninvoice = 0;
            Decimal allinvoice = 0;
            decimal totaltax = 0;
            decimal resnotyetinvoiced = 0; 
            if (result.Entities != null && result.Entities.Count > 0)
            {

                for (int i = 0; i < result.Entities.Count; i++)
                {
                    if (result.Entities[i].Attributes.Contains("statecode") && result.Entities[i].Attributes["statecode"] != null)
                    {
                        OptionSetValue status = null;
                        status = (OptionSetValue)result.Entities[i].Attributes["statecode"];
                        if (status.Value == 2)
                        {
                            Money resultpaidinvoice = new Money(0);
                            if (result.Entities[i].Attributes.Contains("new_dbms_qbpaid_amount") && result.Entities[i].Attributes["new_dbms_qbpaid_amount"] != null)
                            {
                                resultpaidinvoice = result.Entities[i].GetAttributeValue<Money>("new_dbms_qbpaid_amount");
                                totalpaidinvoice = totalpaidinvoice + Convert.ToDecimal(resultpaidinvoice.Value);
                            }
                        }

                        else if (status.Value == 0)
                        {
                            Money resultactive = new Money(0);
                            if (result.Entities[i].Attributes.Contains("new_dbms_qbpaid_amount") && result.Entities[i].Attributes["new_dbms_qbpaid_amount"] != null && status.Value !=3)
                            {

                                resultactive = result.Entities[i].GetAttributeValue<Money>("new_dbms_qbpaid_amount");
                                totalopeninvoice = totalopeninvoice + Convert.ToDecimal(resultactive.Value);
                            }
                        }


                        Money resulttotalamount = new Money(0);
                        if (result.Entities[i].Attributes.Contains("totalamount") && result.Entities[i].Attributes["totalamount"] != null && status.Value !=3)
                        {
                            resulttotalamount = result.Entities[i].GetAttributeValue<Money>("totalamount");
                            allinvoice = allinvoice + Convert.ToDecimal(resulttotalamount.Value);
                        }

                        if(result.Entities[i].Attributes.Contains("totaltax") && result.Entities[i].Attributes["totaltax"] != null && status.Value != 3)
                        {
                            Money resulttotaltax = new Money(0);
                            resulttotaltax = result.Entities[i].GetAttributeValue<Money>("totaltax");
                            totaltax = totaltax + Convert.ToDecimal(resulttotaltax.Value);
                        }
                    }

                }
                Entity fetchresult = service.Retrieve("opportunity", id, new ColumnSet("ig1_customerpoamount"));
                Money notyetinvoiced = new Money(0);

                if (fetchresult.Attributes.Contains("ig1_customerpoamount") && fetchresult.Attributes["ig1_customerpoamount"] != null)
                {
                    notyetinvoiced = fetchresult.GetAttributeValue<Money>("ig1_customerpoamount");

                    resnotyetinvoiced = Convert.ToDecimal(notyetinvoiced.Value) - allinvoice;
                }
                objentity["ig1_totalpaid"] = totalpaidinvoice;
                objentity["ig1_outstandingreceivable"] = totalopeninvoice;
                objentity["ig1_totalallinvoices"] = allinvoice;
                objentity["ig1_totaltaxes"] = totaltax;
                objentity["ig1_notyetinvoiced"] = resnotyetinvoiced;
                service.Update(objentity);
            }



        }

        public void GetQuoteEntityfieldcost(Guid id)
        {
            var fetchData = new
            {
                opportunityid = id,
                statecode = "2"
            };
            var fetchXml = $@"
      <fetch>
         <entity name='quote'>
    <attribute name='totalamount' />
    <filter type='and'>
      <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
      <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*2*/}'/>
    </filter>
       </entity>
         </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Decimal totalofquote = 0;
            if (result != null && result.Entities.Count > 0)
            {

                for (int i = 0; i < result.Entities.Count; i++)
                {
                    if (result.Entities[i].Attributes.Contains("totalamount") && result.Entities[i].Attributes["totalamount"] != null)
                    {

                        Money fetchedamount = new Money(0);
                        fetchedamount = (Money)result.Entities[i].GetAttributeValue<Money>("totalamount");
                        totalofquote = totalofquote + Convert.ToDecimal(fetchedamount.Value);
                    }
                }
            }

            objentity["ig1_totalofwonquotes"] = totalofquote;
            service.Update(objentity);
        }

        public void GetBidsheetEntityfieldcost(Guid id)
        {
            var fetchData = new
            {
                ig1_associated = "1",
                ig1_opportunitytitle = id
            };
            var fetchXml = $@"
                                <fetch>
                                <entity name='ig1_bidsheet'>
                                <attribute name='ig1_anticipatedgp' />
                                <attribute name='ig1_anticipatedgpinpercent' />
                                <attribute name='ig1_anticipatednet' />
                                <attribute name='ig1_anticipatednetinpercent' />
                                <attribute name='ig1_anticipatedcommissionablevalue' />
                                <attribute name='ig1_sellprice' />
                                 <filter type='and'>
                                <condition attribute='ig1_associated' operator='eq' value='{fetchData.ig1_associated/*1*/}'/>
                              <condition attribute='ig1_opportunitytitle' operator='eq' value='{fetchData.ig1_opportunitytitle/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
                                </filter>
                               </entity>
                               </fetch>";
            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            Decimal sumtotalofactivatedbidsheet = 0;
            Decimal sumanticipatedgp = 0;
            Decimal anticipatednet = 0;
           // Decimal commisionableamount = 0;
            Decimal anticipatednetpercent = 0;
            Decimal anticipatedgpper = 0;
            if (result.Entities != null && result.Entities.Count > 0)
            {
                for (int i = 0; i < result.Entities.Count; i++)
                {
                    if (result.Entities[i].Attributes.Contains("ig1_sellprice") && result.Entities[i].Attributes["ig1_sellprice"] != null)
                    {
                        Money totalofactivatedbidsheet = new Money(0);
                        totalofactivatedbidsheet = (Money)result.Entities[i].Attributes["ig1_sellprice"];
                        sumtotalofactivatedbidsheet = sumtotalofactivatedbidsheet + Convert.ToDecimal(totalofactivatedbidsheet.Value);
                    }
                    if (result.Entities[i].Attributes.Contains("ig1_anticipatedgp") && result.Entities[i].Attributes["ig1_anticipatedgp"] != null)
                    {
                        Money resultanticipated = result.Entities[i].GetAttributeValue<Money>("ig1_anticipatedgp");
                        sumanticipatedgp = sumanticipatedgp + Convert.ToDecimal(resultanticipated.Value);
                    }

                    if (result.Entities[i].Attributes.Contains("ig1_anticipatednet") && result.Entities[i].Attributes["ig1_anticipatednet"] != null)
                    {
                        Money resultanticipatednet = new Money(0);
                        resultanticipatednet = result.Entities[i].GetAttributeValue<Money>("ig1_anticipatednet");
                        anticipatednet = anticipatednet + Convert.ToDecimal(resultanticipatednet.Value);
                    }
                    //if (result.Entities[i].Attributes.Contains("ig1_anticipatedgpinpercent") && result.Entities[i].Attributes["ig1_anticipatedgpinpercent"] != null)
                    //{
                    //    Decimal resultcommissionable = new decimal(0);

                    //    resultcommissionable = result.Entities[i].GetAttributeValue<Decimal>("ig1_anticipatedgpinpercent");
                    //    commisionableamount = commisionableamount + Convert.ToDecimal(resultcommissionable);


                    //}
                    //if(result.Entities[i].Attributes.Contains("ig1_anticipatednetinpercent") && result.Entities[i].Attributes["ig1_anticipatednetinpercent"] != null)
                    //{
                    //    decimal resultanticippercent = 0;

                    //    resultanticippercent= result.Entities[i].GetAttributeValue<Decimal>("ig1_anticipatednetinpercent");
                    //    anticipatednetpercent = anticipatednetpercent + Convert.ToDecimal(resultanticippercent);
                    //}
                }


                anticipatedgpper = (sumanticipatedgp * 100) / sumtotalofactivatedbidsheet;
                anticipatednetpercent = anticipatedgpper - 14;

                objentity["ig1_anticipatedgp"] = sumanticipatedgp;
                objentity["ig1_anticipatednet"] = anticipatednet;
                objentity["ig1_anticipatednetpercent"] = anticipatednetpercent;
                objentity["ig1_anticipatedgppercent"] = anticipatedgpper;
                objentity["ig1_totalofactivatedbidsheets"] = sumtotalofactivatedbidsheet;
                service.Update(objentity);
            }

        }

        public void GetOpportunityEntityfields(Guid id)
        {

            var fetchData = new
            {
                opportunityid = id
            };
            var fetchXml = $@"
                                               <fetch>
                                               <entity name='opportunity'>
                                               <attribute name='ig1_projectrecord' />
                                               <attribute name='ig1_customerpodate' />
                                               <attribute name='ig1_customerpoamount' />
                                               <attribute name='ig1_requestedprojectcompletiondate' />
                                               <filter type='and'>
                                               <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
                                               </filter>
                                               </entity>
                                               </fetch>";

            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (result != null)
            {
                if (result.Entities[0].Attributes.Contains("ig1_customerpodate") && result.Entities[0].Attributes["ig1_customerpodate"] != null)
                {
                    objentity["ig1_customerpodate"] = result.Entities[0].GetAttributeValue<DateTime>("ig1_customerpodate");
                }
                else
                {
                    objentity["ig1_customerpodate"] = null;
                }
                if (result.Entities[0].Attributes.Contains("ig1_customerpoamount") && result.Entities[0].Attributes["ig1_customerpoamount"] != null)
                {
                    objentity["ig1_totalrecievable"] = result.Entities[0].GetAttributeValue<Money>("ig1_customerpoamount");
                }
                if (result.Entities[0].Attributes.Contains("ig1_requestedprojectcompletiondate") && result.Entities[0].Attributes["ig1_requestedprojectcompletiondate"] != null)
                {
                    objentity["ig1_datebooked"] = result.Entities[0].GetAttributeValue<DateTime>("ig1_requestedprojectcompletiondate");
                }
                else
                {
                    objentity["ig1_datebooked"] = null;
                }
                service.Update(objentity);
            }
        }

        public decimal TotalAllinvoices(Guid id)
        {

            var fetchData = new
            {
                opportunityid = id
            };
            var fetchXml = $@"
                           <fetch>
          <entity name='invoice'>    
    <attribute name='totalamount' />
    <attribute name='statecode' />
    <filter>
      <condition attribute='opportunityid' operator='eq' value='{fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
    </filter>
  </entity>
</fetch>";


            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

            Decimal allinvoice = 0;
            if (result.Entities != null && result.Entities.Count > 0)
            {

                for (int i = 0; i < result.Entities.Count; i++)
                {
                    if (result.Entities[i].Attributes.Contains("statecode") && result.Entities[i].Attributes["statecode"] != null)
                    {
                        OptionSetValue status = null;
                        status = (OptionSetValue)result.Entities[i].Attributes["statecode"];

                        Money resulttotalamount = new Money(0);
                        if (result.Entities[i].Attributes.Contains("totalamount") && result.Entities[i].Attributes["totalamount"] != null && status.Value != 3)
                        {
                            resulttotalamount = result.Entities[i].GetAttributeValue<Money>("totalamount");
                            allinvoice = allinvoice + Convert.ToDecimal(resulttotalamount.Value);
                        }
                    }

                }

            }
            return allinvoice;
        }

        public void Getworkorderfield(Guid id)
        {
            var fetchData = new
            {
                msdyn_opportunityid = id,
                statecode = "0"
            };
            var fetchXml = $@"
                    <fetch>
                    <entity name='msdyn_workorder'>
                    <attribute name='ig1_requiredcompletiondate' />
                    <filter type='and'>
                    <condition attribute='msdyn_opportunityid' operator='eq' value='{fetchData.msdyn_opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/}'/>
                    <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                    </filter>
                    <order attribute='createdon' descending='true' />
                    </entity>
                    </fetch>";



            EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

              if(result.Entities !=null && result.Entities.Count > 0)
            {

                if (result.Entities[0].Attributes.Contains("ig1_requiredcompletiondate") && result.Entities[0].Attributes["ig1_requiredcompletiondate"] !=null)
                {
                  objentity["ig1_anticipatedprojectcompletiondate"] = result.Entities[0].GetAttributeValue<DateTime>("ig1_requiredcompletiondate");

                }
                else
                {
                    objentity["ig1_anticipatedprojectcompletiondate"] = null;
                }


            }
            service.Update(objentity);

        }

        public IList<Decimal> Hardcostandsoftcost(Guid id)
        {
            decimal hardcost = 0;
            decimal softcost = 0;

            IList<Decimal> list = new List<Decimal>(0);
            if (id != null)
            {
                var fetchData = new
                {
                    ig1_projectrecord = id
                };

                var fetchXml = $@"
                  <fetch>
                  <entity name='ig1_projectrecordcost'>
                  <attribute name='ig1_expensetype' />
                  <attribute name='ig1_amount' />
                  <filter type='and'>
                  <condition attribute='ig1_projectrecord' operator='eq' value='{fetchData.ig1_projectrecord/*a5c627ce-a3bf-ea11-a812-000d3a55d7ac*/}'/>
                 </filter>
                 </entity>
                 </fetch>";

                EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

                
               
                if (result.Entities != null && result.Entities.Count > 0)
                {
                    for (int i = 0; i < result.Entities.Count; i++)
                    {

                        if (result.Entities[i].Attributes["ig1_expensetype"] != null && result.Entities[i].Attributes.Contains("ig1_expensetype"))
                        {

                            string expensetype = Convert.ToString(result.Entities[i].GetAttributeValue<EntityReference>("ig1_expensetype").Name);

                            if (expensetype == "IG Install PO Bill" || expensetype == "Travel Cost" || expensetype == "PO Bill")
                            {
                                Money hamount = new Money(0);
                                hamount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");
                                hardcost = hardcost + Convert.ToDecimal(hamount.Value);
                               // list.Add(hardcost);
                            }

                            else if (expensetype == "Design Cost" || expensetype == "Miscellaneous" || expensetype == "PM Cost" || expensetype == "Sales Cost")
                            {
                                Money samount = new Money(0);
                                samount = result.Entities[i].GetAttributeValue<Money>("ig1_amount");
                                softcost = softcost + Convert.ToDecimal(samount.Value);
                               // list.Add(softcost);
                            }


                        }

                    }
                }
            }

              list.Add(hardcost);
              list.Add(softcost);
                return list;           
        }

        public decimal CorpGna()
        {
            Money corpgna = new Money(0);
            var fetchXml = $@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='ig1_projectcostallowances'>
                 <attribute name='ig1_corpgna' />
              </entity>
             </fetch>";
          EntityCollection result = service.RetrieveMultiple(new FetchExpression(fetchXml));

           if(result.Entities[0].Attributes.Contains("ig1_corpgna") && result.Entities[0].Attributes["ig1_corpgna"] != null)
            {

                corpgna = result.Entities[0].GetAttributeValue<Money>("ig1_corpgna");

            }

            return Convert.ToDecimal(corpgna.Value);
        }




    }
}

