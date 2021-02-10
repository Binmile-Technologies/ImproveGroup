function  test (executionContext){


     debugger;
  
    var formContext = executionContext.getFormContext();

   
    
     var recordid1= formContext.data.entity.getId();

        var recordid = recordid1.replace(/{/g, '').replace(/}/g, '');

   var req = new XMLHttpRequest();
req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders(" + recordid + ")?$select=ig1_anticipatedcompletion", false);
req.setRequestHeader("OData-MaxVersion", "4.0");
req.setRequestHeader("OData-Version", "4.0");
req.setRequestHeader("Accept", "application/json");
req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
req.onreadystatechange = function() {
    if (this.readyState === 4) {
        req.onreadystatechange = null;
        if (this.status === 200) {
            var result = JSON.parse(this.response);
            var ig1_anticipatedcompletion = result["ig1_anticipatedcompletion"];
            if (ig1_anticipatedcompletion == null) {
                        var date = new Date();
                    }
                 else {
                        var date = new Date(ig1_anticipatedcompletion);
                    }
            formContext.getAttribute("ig1_oldanticipatedcompletiondate").setValue(date);
           formContext.data.entity.save();
            formContext.ui.refreshRibbon();
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }
};
req.send();
  
  
}