function setProjectNumber(executionContext) {
	try {
		debugger;
		var formContext = executionContext.getFormContext();
		var projectNumber = "";
		var projectRecord = formContext.getAttribute("ig1_projectrecord").getValue();
		if (projectRecord != undefined && projectRecord != null && projectRecord != "") {
			var projectid = projectRecord[0].id.replace("{", "").replace("}", "");
			if (projectid != undefined && projectid != null && projectid != "") {
				projectNumber = getProjectNumber(projectid, formContext);
			}
		}
		formContext.getAttribute("ig1_name").setValue(projectNumber);
	}
	catch (err) {
		alert(err.message);
	}
}

function getProjectNumber(projectid, formContext) {
	try {
		var projectNumber = "";
		var req = new XMLHttpRequest();
		req.open("GET", formContext.context.getClientUrl() + "/api/data/v9.1/ig1_projectrecords(" + projectid + ")?$select=ig1_projectnumber", false);
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
					projectNumber = result["ig1_projectnumber"];
				}
				else {
					alert(this.statusText);
				}
			}
		};
		req.send();
		return projectNumber;
	}
	catch (err) {
		alert(err.message);
	}
}