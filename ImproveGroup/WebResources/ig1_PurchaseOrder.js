function SetPoTotalAmount(executionContext) {

    debugger;
    var formContext = executionContext.getFormContext();
    var id = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var poamount = formContext.data.entity.attributes.get("msdyn_totalamount").getValue();

    var fetchData = {
        msdyn_purchaseorder: id,
        statecode: "0"
    };
    var fetchXml = [
"<fetch>",
"  <entity name='msdyn_purchaseorderbill'>",
"    <filter>",
"      <condition attribute='msdyn_purchaseorder' operator='eq' value='", fetchData.msdyn_purchaseorder/*98067b2c-604f-eb11-a813-000d3a98d873*/, "'/>",
"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
"    </filter>",
"  </entity>",
"</fetch>",
    ].join("");

    var pobillresult = XrmServiceToolkit.Soap.Fetch(fetchXml);

    if (pobillresult != '' && pobillresult != null && pobillresult != undefined) {


        for (i = 0; i < pobillresult.length; i++) {

                
            var entity = {};
            entity.ig1_pototalamount = poamount;

            var req = new XMLHttpRequest();
            req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_purchaseorderbills(" + pobillresult[i].id + ")", true);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 204) {
                        //Success - No Return Data - Do Something
                    } else {
                        Xrm.Utility.alertDialog(this.statusText);
                    }
                }
            };
            req.send(JSON.stringify(entity));

        }
    }
}