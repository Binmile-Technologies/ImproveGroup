using Avalara.AvaTax.RestClient;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace IG_CalculateTax.Controllers
{
    public class HomeController : Controller
    {
        [System.Web.Http.HttpGet]
        [EnableCors(origins: "https://igcalculatetax.azurewebsites.net/", headers: "*", methods: "*")]
        public void Index()
        {
            var client = new AvaTaxClient("Dynamics", "9.1", Environment.MachineName, AvaTaxEnvironment.Production).WithSecurity("gordonklein@improvegroup.net", "Impr@ve.3550");

            // Setup transaction model
            var createModel = new CreateTransactionModel()
            {
                type = DocumentType.SalesOrder,
                companyCode = "DEMOPAGE",
                date = DateTime.Today,
                customerCode = "ABC",
                lines = new List<LineItemModel>()
    {
        new LineItemModel()
        {
            number = "1",
            quantity = 1,
            amount = 40.21m,
            taxCode = "PC040400"
        }
    },
                addresses = new AddressesModel()
                {
                    singleLocation = new AddressLocationInfo()
                    {
                        line1 = "2000 Main Street",
                        city = "Irvine",
                        region = "CA",
                        country = "US",
                        postalCode = "92614"
                    }
                }
            };
            // Create transaction
            var transaction = client.CreateTransaction(null, createModel);
        }
        public void GetData()
        {
            var client = new AvaTaxClient("Dynamics", "9.1", Environment.MachineName, AvaTaxEnvironment.Sandbox)
    .WithSecurity("gordonklein@improvegroup.net", "3550.Impr@ve");

            // Setup transaction model
            var createModel = new CreateTransactionModel()
            {
                type = DocumentType.SalesOrder,
                companyCode = "IMPROVEGROUPINC",
                date = DateTime.Today,
                customerCode = "65e8b975-15cf-e911-a96a-000d3a1d55a5",
                lines = new List<LineItemModel>()
    {
        new LineItemModel()
        {
            number = "1",
            quantity = 1,
            amount = 40.21m,
            taxCode = "PC040400"
        }
    },
                addresses = new AddressesModel()
                {
                    singleLocation = new AddressLocationInfo()
                    {
                        line1 = "2000 Main Street",
                        city = "Irvine",
                        region = "CA",
                        country = "USA",
                        postalCode = "92614"
                    }
                }
            };
        }
    }
}
