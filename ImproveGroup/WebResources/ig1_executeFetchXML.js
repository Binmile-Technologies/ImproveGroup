function executeFetchXml(entity, fethXmlQuery)
{
       debugger;
	var results = "";
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/"+entity+"?fetchXml="+encodeURIComponent(fethXmlQuery), false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 200) {
				results = JSON.parse(this.response);
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();

	return results ;
}