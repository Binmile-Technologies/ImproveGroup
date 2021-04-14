function DeleteOpportunity(selecteditemreference, formContext) {


    debugger;
    var flag = confirm("All associated entity records (Bid Sheet, Quote, Work Order, Project Record, etc.) will be deleted along with this Opportunity. This cannot be undone. Click OK to Proceed");
    if (!flag) {
        return;
    }
    var opportunityid = "";

    if (selecteditemreference != undefined && selecteditemreference != null && selecteditemreference != "") {

        if (selecteditemreference.length > 1) {
            alert("select only one record at a time as there may be lots of related entity");
            return;
        }
        else if (selecteditemreference.length == 1) {
            opportunityid = selecteditemreference[0].Id;
        }

    }
    else if (selecteditemreference.length == 0 && formContext == undefined) {
        alert("Please select the opportunity which you want to Delete");
        return;
    }


    else if (formContext != undefined && formContext != null && formContext != "") {
        opportunityid = formContext.data.entity.getId().replace("{", "").replace("}", "");

    }

    var parameters = {};
    parameters.selectedOpportunity = opportunityid;
    Xrm.Utility.showProgressIndicator("Processing");
    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_DeleteOpportunityAndRelatedRecords", true);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                if (results.Checkstatus && results != undefined && results != null && results != "")
                {
                    alert(results.EntityName);
                     Xrm.Utility.closeProgressIndicator();
                }
                else {

                    alert("Record Deleted successfully.");
                      Xrm.Utility.closeProgressIndicator();
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(parameters));


}