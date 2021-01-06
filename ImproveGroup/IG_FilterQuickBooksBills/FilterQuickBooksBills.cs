/* 
 *This plugin is responsible to fetch all the data from the QB Bill(ig1_quickbooksbill) entity and it'll perform below actions...
 * 1. Identityfy the record category(PO Bill, IG Install PO Bill, Overhead, Category)
 * 2. Upsert the record in respective antity on the basis of category.
 *      1. If Category is PO Bill, IG Install PO Bill and travel then record will be save in Actual Cost(ig1_projectrecordcost) entity
 *      2. If the Category is Overhead then the record will be saved in QB Overhead(ig1_overhead) entity
 *      3. If Category is Not yet Categorized then the record will be saved in QB Not Categorized(ig1_notcategorized) entity
 * 3. Records will be deleted from QB Bill entity after upserting respective entities
 * 4. If there is any occured then the record will not be deleted from the QB Bill entity
 * 5. Finally it will delete the record from trigger entity...
 * 
 * DATE:04-03-2020
 * WRITTEN BY: Mohd Nazish
 */
using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text.RegularExpressions;

namespace IG_FilterQuickBooksBills
{
    public class FilterQuickBooksBills : IPlugin
    {
        IPluginExecutionContext context;
        ITracingService tracingService;
        IOrganizationServiceFactory serviceFactory;
        IOrganizationService service;
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(null);

            try
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
                {
                    return;
                }

                var entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName == "ig1_qbtrigger")
                {
                    QuickBooksBiils("ig1_quickbooksbill");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in FilterQuickBooksBills " + ex);
            }
        }
        protected void QuickBooksBiils(string entity)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = entity,
                ColumnSet = new ColumnSet
                    (
                        "ig1_txnid",
                        "ig1_txnlineid",
                        "ig1_refno",
                        "ig1_name",
                        "ig1_memo",
                        "ig1_date",
                        "ig1_billdue",
                        "ig1_amount",
                        "ig1_terms",
                        "ig1_transactiontype"
                    ),
            };
            EntityCollection entityCollection = service.RetrieveMultiple(query);
            foreach (var record in entityCollection.Entities)
            {
                if (record.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(record.Attributes["ig1_name"].ToString()))
                {
                    if (record.Attributes.Contains("ig1_memo") && !string.IsNullOrEmpty(record.Attributes["ig1_memo"].ToString()))
                    {
                        CategorizeRecords(record);
                    }
                    else
                    {
                        SaveRecord(record, string.Empty, string.Empty, "ig1_overhead");
                    }
                }
                else
                {
                    SaveRecord(record, string.Empty, string.Empty, "ig1_quickbooksbillsnotcategorized");
                }
            }
        }
        //Method Created to categorized(PO Bill, IG Install to PO Bill, Overhead and Travel etc..)
        protected void CategorizeRecords(Entity record)
        {
            string jobNumber = GetJobNumber(record);
            string expenseType = string.Empty;
            string entityName = string.Empty;
            if (!string.IsNullOrEmpty(jobNumber) && jobNumber !="11234" && jobNumber != "11318" && jobNumber!= "45027")
            {
                if (record.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(record.Attributes["ig1_name"].ToString()))
                {
                    string accountRef = record.Attributes["ig1_name"].ToString().ToLower();
                    if (!string.IsNullOrEmpty(accountRef) && (accountRef.Contains("ced") || accountRef.Contains("prof-serv") || accountRef.Contains("install") || accountRef.Contains("equipment")))
                    {
                        expenseType = "IG Install PO Bill";
                        entityName = "ig1_projectrecordcost";
                    }
                    else if (!string.IsNullOrEmpty(accountRef) && (accountRef.Contains("travel") || accountRef.Contains("meal") || accountRef.Contains("gas")))
                    {
                        expenseType = "Travel Cost";
                        entityName = "ig1_projectrecordcost";

                    }
                    else
                    {
                        expenseType = "PO Bill";
                        entityName = "ig1_projectrecordcost";
                    }
                }
                else
                {
                    expenseType = "Not Yet Categorized";
                    entityName = "ig1_quickbooksbillsnotcategorized";
                }
            }
            else
            {
                expenseType = "Overhead";
                entityName = "ig1_overhead";
            }

            SaveRecord(record, expenseType, jobNumber, entityName);
        }

        //Save records to the restpective entity based on category..
        protected void SaveRecord(Entity record, string expenseType, string jobNumber, string entityName)
        {
            string txnId = string.Empty;
            string txnLineId = string.Empty;
            if (record.Attributes.Contains("ig1_txnid") && !string.IsNullOrEmpty(record.Attributes["ig1_txnid"].ToString()))
            {
                txnId = record.Attributes["ig1_txnid"].ToString();
            }
            if (record.Attributes.Contains("ig1_txnlineid") && !string.IsNullOrEmpty(record.Attributes["ig1_txnlineid"].ToString()))
            {
                txnLineId = record.Attributes["ig1_txnlineid"].ToString();
            }
            var fetchData = new
            {
                ig1_txnid = txnId,
                ig1_txnlineid = txnLineId,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='{entityName}'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='ig1_txnid' operator='eq' value='{fetchData.ig1_txnid/*10AE83-1437585808*/}'/>
                                  <condition attribute='ig1_txnlineid' operator='eq' value='{fetchData.ig1_txnlineid/*10AE83-1437585808*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                Guid projectRecordId = ProjectRecord(jobNumber);
                Entity entity;
                if (entityName == "ig1_projectrecordcost")
                {
                    entity = service.Retrieve(entityName, ec.Entities[0].Id, new ColumnSet("ig1_refno", "ig1_name", "ig1_date", "ig1_billdue", "ig1_amount", "ig1_terms", "ig1_expensetype"));
                }
                else
                {
                    entity = service.Retrieve(entityName, ec.Entities[0].Id, new ColumnSet("ig1_refno", "ig1_name", "ig1_date", "ig1_billdue", "ig1_amount", "ig1_terms"));
                }
                if (!string.IsNullOrEmpty(jobNumber))
                {
                    entity.Attributes["ig1_name"] = jobNumber;
                }
                if (entityName == "ig1_projectrecordcost")
                {
                    Guid expenseTypeId = ExpenseType(expenseType);
                    if (expenseTypeId != Guid.Empty)
                    {
                        entity.Attributes["ig1_expensetype"] = new EntityReference("ig1_expensecategories", expenseTypeId);
                    }
                }
                if (projectRecordId != Guid.Empty)
                {
                    entity.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecordId);
                }
                if (record.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(record.Attributes["ig1_name"].ToString()))
                {
                    entity["ig1_accountref"] = record.Attributes["ig1_name"].ToString();
                }
                if (record.Attributes.Contains("ig1_memo") && !string.IsNullOrEmpty(record.Attributes["ig1_memo"].ToString()))
                {
                    entity["ig1_memo"] = record.Attributes["ig1_memo"].ToString();
                }
                if (record.Attributes.Contains("ig1_refno") && !string.IsNullOrEmpty(record.Attributes["ig1_refno"].ToString()))
                {
                    entity["ig1_refno"] = record.Attributes["ig1_refno"].ToString();
                }
                if (record.Attributes.Contains("ig1_date") && !string.IsNullOrEmpty(record.Attributes["ig1_date"].ToString()))
                {
                    entity["ig1_date"] = (DateTime)record.Attributes["ig1_date"];
                }
                if (record.Attributes.Contains("ig1_billdue") && !string.IsNullOrEmpty(record.Attributes["ig1_billdue"].ToString()))
                {
                    entity["ig1_billdue"] = (DateTime)record.Attributes["ig1_billdue"];
                }
                if (record.Attributes.Contains("ig1_amount") && !string.IsNullOrEmpty(record.Attributes["ig1_amount"].ToString()))
                {
                    entity["ig1_amount"] = (Money)record.Attributes["ig1_amount"];
                }
                if (record.Attributes.Contains("ig1_terms") && !string.IsNullOrEmpty(record.Attributes["ig1_terms"].ToString()))
                {
                    entity["ig1_terms"] = record.Attributes["ig1_terms"].ToString();
                }
                if (record.Attributes.Contains("ig1_transactiontype") && !string.IsNullOrEmpty(record.Attributes["ig1_transactiontype"].ToString()))
                {
                    entity["ig1_transactiontype"] = record.Attributes["ig1_transactiontype"].ToString();
                }
                service.Update(entity);
                if (entityName == "ig1_projectrecordcost")
                {
                    DeleteRecord("ig1_quickbooksbillsnotcategorized", txnId, txnLineId);
                    DeleteRecord("ig1_overhead", txnId, txnLineId);
                }
            }
            else
            {
                Guid projectRecordId = ProjectRecord(jobNumber);
                Entity entity = new Entity(entityName);
                if (!string.IsNullOrEmpty(jobNumber))
                {
                    entity["ig1_name"] = jobNumber;
                }
                if (entityName == "ig1_projectrecordcost")
                {
                    Guid expenseTypeId = ExpenseType(expenseType);

                    if (expenseTypeId != Guid.Empty)
                    {
                        entity.Attributes["ig1_expensetype"] = new EntityReference("ig1_expensecategories", expenseTypeId);
                    }
                }
                if (projectRecordId != Guid.Empty)
                {
                    entity.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecordId);
                }
                if (record.Attributes.Contains("ig1_txnid") && !string.IsNullOrEmpty(record.Attributes["ig1_txnid"].ToString()))
                {
                    entity["ig1_txnid"] = record.Attributes["ig1_txnid"].ToString();
                }
                if (record.Attributes.Contains("ig1_txnlineid") && !string.IsNullOrEmpty(record.Attributes["ig1_txnlineid"].ToString()))
                {
                    entity["ig1_txnlineid"] = record.Attributes["ig1_txnlineid"].ToString();
                }
                if (record.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(record.Attributes["ig1_name"].ToString()))
                {
                    entity["ig1_accountref"] = record.Attributes["ig1_name"].ToString();
                }
                if (record.Attributes.Contains("ig1_memo") && !string.IsNullOrEmpty(record.Attributes["ig1_memo"].ToString()))
                {
                    entity["ig1_memo"] = record.Attributes["ig1_memo"].ToString();
                }
                if (record.Attributes.Contains("ig1_refno") && !string.IsNullOrEmpty(record.Attributes["ig1_refno"].ToString()))
                {
                    entity["ig1_refno"] = record.Attributes["ig1_refno"].ToString();
                }
                if (record.Attributes.Contains("ig1_date") && !string.IsNullOrEmpty(record.Attributes["ig1_date"].ToString()))
                {
                    entity["ig1_date"] = (DateTime)record.Attributes["ig1_date"];
                }
                if (record.Attributes.Contains("ig1_billdue") && !string.IsNullOrEmpty(record.Attributes["ig1_billdue"].ToString()))
                {
                    entity["ig1_billdue"] = (DateTime)record.Attributes["ig1_billdue"];
                }
                if (record.Attributes.Contains("ig1_amount") && !string.IsNullOrEmpty(record.Attributes["ig1_amount"].ToString()))
                {
                    entity["ig1_amount"] = (Money)record.Attributes["ig1_amount"];
                }
                if (record.Attributes.Contains("ig1_terms") && !string.IsNullOrEmpty(record.Attributes["ig1_terms"].ToString()))
                {
                    entity["ig1_terms"] = record.Attributes["ig1_terms"].ToString();
                }
                if (record.Attributes.Contains("ig1_transactiontype") && !string.IsNullOrEmpty(record.Attributes["ig1_transactiontype"].ToString()))
                {
                    entity["ig1_transactiontype"] = record.Attributes["ig1_transactiontype"].ToString();
                }
                Guid id = service.Create(entity);
                if (entityName == "ig1_projectrecordcost")
                {
                    DeleteRecord("ig1_quickbooksbillsnotcategorized", txnId, txnLineId);
                    DeleteRecord("ig1_overhead", txnId, txnLineId);
                }
            }
            service.Delete(record.LogicalName, record.Id);
        }

        //Delete Record from ig1_quickbooksbill entity which has been saved in respective entity...
        protected void DeleteRecord(string entityName, string txnId, string txnLineId)
        {
            var fetchData = new
            {
                ig1_txnid = txnId,
                ig1_txnlineid = txnLineId,
                statuscode = "1"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='{entityName}'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='ig1_txnid' operator='eq' value='{fetchData.ig1_txnid/*10AE83-1437585808*/}'/>
                                  <condition attribute='ig1_txnlineid' operator='eq' value='{fetchData.ig1_txnlineid/*10AE83-1437585808*/}'/>
                                  <condition attribute='statuscode' operator='eq' value='{fetchData.statuscode/*1*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                service.Delete(entityName, ec.Entities[0].Id);
            }
        }

        //Fetching the project number from the from the memo...
        protected string GetJobNumber(Entity record)
        {
            string jobNumber = string.Empty;

            if (record.Attributes.Contains("ig1_memo") && !string.IsNullOrEmpty(record.Attributes["ig1_memo"].ToString()))
            {
                string memo = record.Attributes["ig1_memo"].ToString().ToLower();
                if (memo.Contains("job #"))
                {
                    string submemo = memo.Substring(memo.IndexOf("job #") + 5);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("job #") + 5, 5);
                    }
                }
                else if (memo.Contains("job# "))
                {
                    string submemo = memo.Substring(memo.IndexOf("job# ") + 5);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("job# ") + 5, 5);
                    }
                }
                else if (memo.Contains("job#"))
                {
                    string submemo = memo.Substring(memo.IndexOf("job#") + 4);
                    if (submemo.Length >= 5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("job#") + 4, 5);
                    }
                }
                else if (memo.Contains("project #"))
                {
                    string submemo = memo.Substring(memo.IndexOf("project #") + 9);
                    if (submemo.Length >= 5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("project #") + 9, 5);
                    }
                }
                else if (memo.Contains("project# "))
                {
                    string submemo = memo.Substring(memo.IndexOf("project# ") + 9);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("project# ") + 9, 5);
                    }
                }
                else if (memo.Contains("project#"))
                {
                    string submemo = memo.Substring(memo.IndexOf("project#") + 8);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("project#") + 8, 5);
                    }
                }
                else if (memo.Contains("project "))
                {
                    string submemo = memo.Substring(memo.IndexOf("project ") + 8);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("project ") + 8, 5);
                    }
                }
                else if (memo.Contains("project"))
                {
                    string submemo = memo.Substring(memo.IndexOf("project") + 7);
                    if (submemo.Length>=5)
                    {
                        jobNumber = memo.Substring(memo.IndexOf("project") + 7, 5);
                    }
                }
                else if (memo.Contains("- "))
                {
                    string submemo = memo.Substring(memo.IndexOf("- ") + 2);
                    if (submemo.Length >= 5)
                    {
                        jobNumber = Regex.Match(submemo, "(\\d{5})").Value;
                    }
                }
                else if (memo.Contains("-"))
                {
                    string submemo = memo.Substring(memo.IndexOf("-") + 1);
                    if (submemo.Length >= 5)
                    {
                        jobNumber = Regex.Match(submemo, "(\\d{5})").Value;
                    }
                }
                else if (memo.Length == 5)
                {
                    int num;
                    if (int.TryParse(memo, out num))
                    {
                        jobNumber = num.ToString();
                    }
                }
                else if (memo.Length > 5 && !memo.Contains("job #") && !memo.Contains("job# ") && !memo.Contains("job#") && !memo.Contains("project #") && !memo.Contains("project# ") && !memo.Contains("project#") && !memo.Contains("project ") && !memo.Contains("project") && !memo.Contains("- ") && !memo.Contains("-"))
                {
                    char ch = Convert.ToChar(memo.Substring(5, 1));
                    if (!Char.IsDigit(ch))
                    {
                        int num;
                        string projectNumber = memo.Substring(0, 5).Trim();
                        if (int.TryParse(projectNumber, out num) && projectNumber.Length == 5)
                        {
                            jobNumber = num.ToString();
                        }
                    }
                }
                int numeric = 0;
                if (!int.TryParse(jobNumber, out numeric) || jobNumber.Trim().Length!=5)
                {
                    jobNumber = "";
                }
            }
            return jobNumber;
        }

        //Fetching the expense type Guid from the ig1_expensecategories entity..
        protected Guid ExpenseType(string type)
        {
            Guid expenceType = Guid.Empty;
            var fetchData = new
            {
                statecode = "0",
                ig1_name = type
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_expensecategories'>
                                <attribute name='ig1_expensecategoriesid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_name' operator='eq' value='{fetchData.ig1_name/*IG Install PO Bill*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                expenceType = ec.Entities[0].Id;
            }
            return expenceType;
        }

        //Fetching the project record Guid from the ig1_projectrecord entity...
        protected Guid ProjectRecord(string jobNumber)
        {
            Guid projectRecordId = Guid.Empty;
            var fetchData = new
            {
                statecode = "0",
                ig1_projectnumber = jobNumber
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='ig1_projectrecord'>
                                <attribute name='ig1_projectrecordid' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                  <condition attribute='ig1_projectnumber' operator='eq' value='{fetchData.ig1_projectnumber/*58654*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
                projectRecordId = ec.Entities[0].Id;
            }
            return projectRecordId;
        }
    }
}
