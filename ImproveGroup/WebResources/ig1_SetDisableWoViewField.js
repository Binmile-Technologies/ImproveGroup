function gridRowSelected(context) {
    debugger;
    context.getFormContext().getData().getEntity().attributes.forEach(function (attr) {
        if (attr.getName() === "ig1_projectnumber" || attr.getName() === "msdyn_name" ||   attr.getName()==="msdyn_opportunityid"  || 
attr.getName() === "ig1_requiredcompletiondate") {
            attr.controls.forEach(function (c) {
                c.setDisabled(true);
            })
        }
                       
    });
}

function gridRowSelectedforopportunity(context) {
    debugger;
    context.getFormContext().getData().getEntity().attributes.forEach(function (attr) {
        if (attr.getName() === "ig1_forecaststatus" || attr.getName() === "ig1_projectnumber"  || attr.getName() === "parentaccountid"  || attr.getName() === "new_designready" || attr.getName()=== "createdon" || attr.getName()==="modifiedon") {
            attr.controls.forEach(function (c) {
                c.setDisabled(true);
            })
        }
                       
    });
}

