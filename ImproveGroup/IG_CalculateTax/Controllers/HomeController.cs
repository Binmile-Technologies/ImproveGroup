using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace IG_CalculateTax.Controllers
{
    public class HomeController : Controller
    {
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public string Index()
        {
            return "Success";
        }

        /*public string Index()
        {
            string totalTax= string.Empty;
            try
            {
                //Ship from Address
                string shipFromLine1 = "3550 Pan American Freeway";
                string shipFromLine2 = null;
                string shipFromLine3 = null;
                string shipFromCity = "Albuquerque";
                string shipFromRegion = "NM";
                string shipFromPostalCode = "87107";
                string shipFromCountry = "USA";

                //Ship To Address:
                string shipToLine1 = "2137 Birchwood Dr";
                string shipToLine2 = null;
                string shipToLine3 = null;
                string shipToCity = "San Antonio";
                string shipToRegion = "TX";
                string shipToPostalCode = "78236";
                string shipToCountry = "USA";

                decimal frAmount = 10000;
                decimal line1Amount = 4104;
                decimal Line2Amount = 10000;

                var client = new AvaTaxClient("Dynamics", "9.1", Environment.MachineName, AvaTaxEnvironment.Production)
                .WithSecurity("gordonklein@improvegroup.net", "Impr@ve.3550"); 
                .WithSecurity("gordonklein@improvegroup.net", "3550.Impr@ve");

                var transaction = new TransactionBuilder(client, "IMPROVEGROUPINC", DocumentType.SalesOrder, "824859ec-efce-e911-a95e-000d3a110bbd")
                .WithAddress(TransactionAddressType.ShipFrom, "3550 Pan American Freeway", null, null, "Albuquerque", "NM", "87107", "USA")
                .WithAddress(TransactionAddressType.ShipTo, "100 Ravine Lane NE", null, null, "Bainbridge Island", "WA", "98110", "US")
                .WithLine(100.0m)
                .WithLine(1234.56m) // Each line is added as a separate item on the invoice!
                .WithExemptLine(50.0m, "NT") // An exempt item!
                .WithLine(2000.0m) // The two addresses below apply to this $2000 line item
                .WithLineAddress(TransactionAddressType.ShipFrom, shipFromLine1, shipFromLine2, shipFromLine3, shipFromCity, shipFromRegion, shipFromPostalCode, shipFromCountry)
                .WithLineAddress(TransactionAddressType.ShipTo, shipToLine1, shipToLine2, shipToLine3, shipToCity, shipToRegion, shipToPostalCode, shipToCountry)
                .WithLine(line1Amount, 1, "SP156226", "PM Labor", "PML001")
                .WithLine(Line2Amount, 1, "P0000000", "Ban Air Cargo Rack", "0065")
                .WithLine(frAmount, taxCode: "FR010000") // shipping costs!
                .Create();
                totalTax =Convert.ToString(transaction.totalTax);
            }
            catch (Exception ex)
            {
                totalTax = ex.Message.ToString();
            }

            return totalTax;
        }*/
    }
}
