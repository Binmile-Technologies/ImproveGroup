function onLoad(executionContext) {
    try
    {

        debugger;
        var formContext = executionContext.getFormContext();
        //var formType = formContext.ui.getFormType();
        //if (formType != 1)
        //{
        //    return;
        //}
        var purchaseOrder = formContext.getAttribute("msdyn_purchaseorder").getValue();
        if (purchaseOrder != undefined && purchaseOrder != null && purchaseOrder != "")
        {
            var puchaseOrderId = purchaseOrder[0].id.replace("{", "").replace("}", "");

            var req = new XMLHttpRequest();
            req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_purchaseorders(" + puchaseOrderId + ")?$select=msdyn_amountbilled,_msdyn_paymentterm_value,msdyn_totalamount", false);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
            req.onreadystatechange = function ()
            {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 200)
                    {
                        var result = JSON.parse(this.response)
                        if ((result["msdyn_amountbilled"] != undefined && result["msdyn_amountbilled"] != null && result["msdyn_amountbilled"] != "") || result["msdyn_amountbilled"]=="0")
                        {
                            formContext.getAttribute("ig1_pototalamountbilled").setValue(parseFloat(result["msdyn_amountbilled"]));
                        }
                        else
                        {
                            formContext.getAttribute("ig1_pototalamountbilled").setValue(null);
                        }
                        if ((result["msdyn_totalamount"] != undefined && result["msdyn_totalamount"] != null && result["msdyn_totalamount"] != "") || result["msdyn_totalamount"]=="0")
                        {
                            formContext.getAttribute("ig1_pototalamount").setValue(parseFloat(result["msdyn_totalamount"]));
                        }
                        else
                        {
                            formContext.getAttribute("ig1_pototalamount").setValue(null);
                        }

                        var paymentTerm = new Array();
                        paymentTerm[0] = new Object();
                        paymentTerm[0].id = result["_msdyn_paymentterm_value"].replace("{", "").replace("}", "");
                        paymentTerm[0].name = result["_msdyn_paymentterm_value@OData.Community.Display.V1.FormattedValue"];
                        paymentTerm[0].entityType = result["_msdyn_paymentterm_value@Microsoft.Dynamics.CRM.lookuplogicalname"];

                        if (paymentTerm != undefined && paymentTerm != null && paymentTerm != "")
                        {
                            formContext.getAttribute("msdyn_paymentterm").setValue(paymentTerm);
                        }
                        else
                        {
                            formContext.getAttribute("msdyn_paymentterm").setValue(null);
                        }
                    }
                    else
                    {
                        Xrm.Utility.alertDialog(this.statusText);
                    }
                }
            };
            req.send();
        }
    }
    catch (err)
    {
        alert(err.message);
    }
}