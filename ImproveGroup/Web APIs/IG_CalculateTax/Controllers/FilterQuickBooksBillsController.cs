﻿using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Web.Http;
using IG_ImproveGroup_Web_API.Models;
using Microsoft.Crm.Sdk.Messages;
using System.Net.Http;
using System.Net;

namespace IG_ImproveGroup_Web_API.Controllers
{
    public class FilterQuickBooksBillsController : ApiController
    {
        IOrganizationService service;
        [HttpPost]
        public HttpResponseMessage FilterBills()
        {
            service = Authentication.OrgService();
            if (service != null)
            {
                Guid userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
                if (userid != Guid.Empty)
                {
                    QuickBooksBiils("ig1_quickbooksbill");
                }
            }
            return Request.CreateResponse(HttpStatusCode.Created,"Bills Filtered");
        }
        protected void QuickBooksBiils(string entity)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = entity,
                ColumnSet = new ColumnSet
                    (
                        "ig1_qb_unique_id",
                        "ig1_refno",
                        "ig1_name",
                        "ig1_memo",
                        "ig1_date",
                        "ig1_billdue",
                        "ig1_amount",
                        "ig1_terms"
                    ),
            };
            EntityCollection entityCollection = service.RetrieveMultiple(query);
            foreach (var record in entityCollection.Entities)
            {
                bool isRecordExist = IsRecordExist(record.Id, record.LogicalName);
                if (isRecordExist)
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
        }
        protected bool IsRecordExist(Guid recId, string entity)
        {
            var fetchData = new
            {
                id = recId
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='{entity}'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='{entity}id' operator='eq' value='{fetchData.id/*a118d3e4-d21e-4eb2-91d0-5eec311ea7b4*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection entityCollection = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (entityCollection.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Method Created to categorized(PO Bill, IG Install to PO Bill, Overhead and Travel etc..)
        protected void CategorizeRecords(Entity record)
        {
            string expenseType = string.Empty;
            if (record.Attributes.Contains("ig1_name") && !string.IsNullOrEmpty(record.Attributes["ig1_name"].ToString()))
            {
                string[] type = record.Attributes["ig1_name"].ToString().Split(':');
                if (type.Length > 0)
                {
                    if (type.Length >= 2)
                    {
                        if (type[0].Trim() == "Europe Direct Sales Expense"
                        && !(type[1].Trim() == "Europe Meals & Ent (80% Deduct)" || type[1].Trim() == "Europe Sales Training" || type[1].Trim() == "Europe Sales Travel" || type[1].Trim() == "Europe Start Up Costs" || type[1].Trim() == "Europe Tradeshows")
                        || (type[0].Trim() == "Sales Dept Direct Expenses" && type[1].Trim() == "Project Management Salaries")
                        || (type[0].Trim() == "Sales Dept Direct Expenses" && type[1].Trim() == "Sales Outside Services")
                        || (type[0].Trim() == "Sales Dept Direct Expenses" && type[1].Trim() == "Sales Professional Fees")
                        || (type[0].Trim() == "Sales Dept Direct Expenses" && type[1].Trim() == "Sales-Equip Rental & Storage")
                        )
                        {
                            expenseType = "PO Bill";
                        }
                        else if ((type[0].Trim() == "Europe Direct Sales Expense" && (type[1].Trim() == "Europe Meals & Ent (80% Deduct)" || type[1].Trim() == "Europe Sales Travel"))
                        || (type[0].Trim() == "Sales Dept Direct Expenses"
                        && (type[1].Trim() == "Sales Travel Expenses"
                        || type[1].Trim() == "Asia - Sales Vehicle Gas & Oil"
                        || type[1].Trim() == "Asia-Sales Meal & Enter(50% ded"
                        || type[1].Trim() == "Asia-Sales Meal & Enter(50% ded)"
                        || type[1].Trim() == "Asia - Sales Meal & Enter(50% ded"
                        || type[1].Trim() == "Asia - Sales Meal & Enter(50% ded)"
                        || type[1].Trim() == "Sales Meal & Enter (50% ded)")
                        || type[1].Trim() == "Sales Travel Expense")
                        || type[1].Trim() == "Sales Vehicle Gas & Oil"
                        )
                        {
                            expenseType = "Travel Cost";
                        }
                        else if ((type[0].Trim() == "Europe Direct Sales Expense" && (type[1].Trim() == "Europe Sales Training" || type[1].Trim() == "Europe Start Up Costs" || type[1].Trim() == "Europe Tradeshows"))
                        || (type[0].Trim() == "Sales Dept Direct Expenses"
                        && (type[1].Trim() == "BD Association Fees"
                        || type[1].Trim() == "Direct Mail-Marketing"
                        || type[1].Trim() == "Engagement"
                        || type[1].Trim() == "IGDNA"
                        || type[1].Trim() == "Sales Software"
                        || type[1].Trim() == "Sales Training"
                        || type[1].Trim() == "rade Shows"
                        || type[1].Trim() == "Vehicle Repairs & Maintenance"
                        || type[1].Trim() == "Websites"))
                        )
                        {
                            expenseType = "Overhead";
                        }
                    }
                    if (expenseType == string.Empty)
                    {
                        if (type[0].Trim() == "CED Costs of Goods Sold")
                        {
                            expenseType = "IG Install PO Bill";

                        }
                        else if (type[0].Trim() == "Europe Costs of Goods Sold"
                        || type[0].Trim() == "Filing Systems Cost of Sales"
                        || type[0].Trim() == "GSA Cost of Sales"
                        || type[0].Trim() == "Product Costs of Goods Sold")
                        {
                            expenseType = "PO Bill";
                        }
                        else if (type[0].Trim() == "General and Admin Expenses"
                        || type[0].Trim() == "GSA Revenue/Commissions"
                        || type[0].Trim() == "Interest Expense"
                        || type[0].Trim() == "Misc. Expense"
                        || type[0].Trim() == "Payroll Expenses"
                        || type[0].Trim() == "Uncategorized Expenses"
                        || type[0].Trim() == "Vehicles"
                        || type[0].Trim() == "Vendor Deposits")
                        {
                            expenseType = "Overhead";
                        }
                        else
                        {
                            expenseType = "Not Yet Categorized";
                        }
                    }
                }
                else if (type.Length <= 0)
                {
                    expenseType = "Not Yet Categorized";
                }
            }
            GetJobNumber(record, expenseType);
        }

        //Save records to the restpective entity based on category..
        protected void SaveRecord(Entity record, string expenseType, string jobNumber, string entityName)
        {
            string qb_unique_id = string.Empty;
            if (record.Attributes.Contains("ig1_qb_unique_id") && !string.IsNullOrEmpty(record.Attributes["ig1_qb_unique_id"].ToString()))
            {
                qb_unique_id = record.Attributes["ig1_qb_unique_id"].ToString();
            }
            var fetchData = new
            {
                ig1_qb_unique_id = qb_unique_id,
                statecode = "0"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='{entityName}'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='ig1_qb_unique_id' operator='eq' value='{fetchData.ig1_qb_unique_id/*10AE83-1437585808*/}'/>
                                  <condition attribute='statecode' operator='eq' value='{fetchData.statecode/*0*/}'/>
                                </filter>
                              </entity>
                            </fetch>";
            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (ec.Entities.Count > 0)
            {
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
                    entity["ig1_name"] = jobNumber;
                }
                if (entityName == "ig1_projectrecordcost")
                {
                    Guid expenseTypeId = ExpenseType(expenseType);
                    Guid projectRecordId = ProjectRecord(jobNumber);
                    if (expenseTypeId != Guid.Empty)
                    {
                        entity.Attributes["ig1_expensetype"] = new EntityReference("ig1_expensecategories", expenseTypeId);
                    }
                    if (projectRecordId != Guid.Empty)
                    {
                        entity.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecordId);
                    }
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
                service.Update(entity);
                if (entityName == "ig1_projectrecordcost")
                {
                    DeleteRecord("ig1_quickbooksbillsnotcategorized", qb_unique_id);
                    DeleteRecord("ig1_overhead", qb_unique_id);
                }
            }
            else
            {
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

                    Guid projectRecordId = ProjectRecord(jobNumber);
                    if (projectRecordId != Guid.Empty)
                    {
                        entity.Attributes["ig1_projectrecord"] = new EntityReference("ig1_projectrecord", projectRecordId);
                    }
                }
                if (record.Attributes.Contains("ig1_qb_unique_id") && !string.IsNullOrEmpty(record.Attributes["ig1_qb_unique_id"].ToString()))
                {
                    entity["ig1_qb_unique_id"] = record.Attributes["ig1_qb_unique_id"].ToString();
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
                Guid id = service.Create(entity);
                if (entityName == "ig1_projectrecordcost")
                {
                    DeleteRecord("ig1_quickbooksbillsnotcategorized", qb_unique_id);
                    DeleteRecord("ig1_overhead", qb_unique_id);
                }
            }
            service.Delete(record.LogicalName, record.Id);
        }

        //Delete Record from ig1_quickbooksbill entity which has been saved in respective entity...
        protected void DeleteRecord(string entityName, string qb_unique_id)
        {
            var fetchData = new
            {
                ig1_qb_unique_id = qb_unique_id,
                statuscode = "1"
            };
            var fetchXml = $@"
                            <fetch mapping='logical' version='1.0'>
                              <entity name='{entityName}'>
                                <attribute name='ig1_name' />
                                <filter type='and'>
                                  <condition attribute='ig1_qb_unique_id' operator='eq' value='{fetchData.ig1_qb_unique_id/*10AE83-1437585808*/}'/>
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
        protected void GetJobNumber(Entity record, string expenseType)
        {
            string jobNumber = string.Empty;

            if (record.Attributes.Contains("ig1_memo") && !string.IsNullOrEmpty(record.Attributes["ig1_memo"].ToString()))
            {
                string memo = record.Attributes["ig1_memo"].ToString();
                if (memo.Contains("Job #"))
                {
                    jobNumber = memo.Substring(memo.IndexOf("Job #") + 5, 5);
                }
                else if (memo.Contains("Job#"))
                {
                    jobNumber = memo.Substring(memo.IndexOf("Job#") + 4, 5);
                }
                else if (memo.Length == 5)
                {
                    int jobId;
                    if (int.TryParse(memo, out jobId))
                    {
                        jobNumber = jobId.ToString();
                    }
                }
                int numeric = 0;
                if (!int.TryParse(jobNumber, out numeric))
                {
                    jobNumber = "";
                }
            }
            if (expenseType == "Overhead" || string.IsNullOrEmpty(jobNumber))
            {
                SaveRecord(record, expenseType, jobNumber, "ig1_overhead");
            }
            else if (expenseType == "Not Yet Categorized")
            {
                SaveRecord(record, expenseType, jobNumber, "ig1_quickbooksbillsnotcategorized");
            }
            else
            {
                SaveRecord(record, expenseType, jobNumber, "ig1_projectrecordcost");
            }
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
