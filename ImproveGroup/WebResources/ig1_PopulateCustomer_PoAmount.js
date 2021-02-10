function setActualRevenue(executionContext) {
  // Test
    var formContext = executionContext.getFormContext();
    var ig1_customerpoamount = 0;
    var ig1_customerpodate = null;
    var entityXRecord = formContext.getAttribute("opportunityid");

    if (entityXRecord != null) {

        var value = entityXRecord.getValue();

        var recordid1 = value[0].id;

        var recordid = recordid1.replace(/{/g, '').replace(/}/g, '');

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(" + recordid + ")?$select=ig1_customerpoamount,ig1_customerpodate", false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    ig1_customerpodate = result["ig1_customerpodate"];

                   

                     if (ig1_customerpodate == null) {
                        var date = new Date();
                    }
                 else {
                        var date = new Date(ig1_customerpodate);
                    }


                    if(result["ig1_customerpoamount"]==null || result["ig1_customerpoamount"] == undefined){

                        var fetchData = {
                            opportunityid: recordid
                        };
                        var fetchXml = [
                    "<fetch>",
                    "  <entity name='opportunity'>",
                    "    <attribute name='estimatedvalue' />",
                    "    <filter type='and'>",
                    "      <condition attribute='opportunityid' operator='eq' value='", fetchData.opportunityid/*b59ae3b0-3fcd-407c-83b6-a69c1189be2e*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                        ].join("");

                        var result =XrmServiceToolkit.Soap.Fetch(fetchXml);

                       var  ig1_customerpoamount1 =result[0].attributes
                      ig1_customerpoamount=ig1_customerpoamount1.estimatedvalue;

                    }
            else{

                        ig1_customerpoamount = parseFloat(result["ig1_customerpoamount"]);
                    }

                    ig1_customerpoamount_formatted = result["ig1_customerpoamount@OData.Community.Display.V1.FormattedValue"];
                   

                   // var date = new Date(ig1_customerpodate);
                    setTimeout(function () {
                        formContext.getAttribute("actualend").setValue(date);
                        formContext.getAttribute("actualrevenue").setValue(ig1_customerpoamount);
                    }, 1000);

                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();

    }

}