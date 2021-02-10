function setOpportunity(executionContext)
{
	try
	{
	    debugger;
	    var formContext = executionContext.getFormContext();
	    var regarding = formContext.getAttribute("regardingobjectid").getValue();
		if(regarding!=undefined && regarding!=null && regarding!="")
		{
		  var regardingId = regarding[0].id.replace("{", "").replace("}", "");
		  var regardingType = regarding[0].entityType;
		  if(regardingType=="msdyn_workorder")
		  {
			var opportunity = setRegardingOpportunity(regardingId);
			formContext.getAttribute("ig1_opportunity").setValue(opportunity);
			formContext.getControl("ig1_opportunity").setVisible(true);
		  }
		  else
		  {
			formContext.getAttribute("ig1_opportunity").setValue(null);
			formContext.getControl("ig1_opportunity").setVisible(false);
		  }
		}
		else
		{
			formContext.getAttribute("ig1_opportunity").setValue(null);
			formContext.getControl("ig1_opportunity").setVisible(false);
		}
	}
	catch(err)
	{
	  alert(err.message);
	}
  
}

function setRegardingOpportunity(regardingid)
{	
       var opportunity =new Array();
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/msdyn_workorders("+regardingid+")?$select=_msdyn_opportunityid_value", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
	req.onreadystatechange = function() 
	{
		if (this.readyState === 4) 
		{
			req.onreadystatechange = null;
			if (this.status === 200) 
			{
				var result = JSON.parse(this.response);
				opportunity[0] = new Object();
				opportunity[0].id = result["_msdyn_opportunityid_value"].replace("{", "").replace("{", "");
				opportunity[0].name = result["_msdyn_opportunityid_value@OData.Community.Display.V1.FormattedValue"];
				opportunity[0].entityType = result["_msdyn_opportunityid_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
			} 
			else 
			{
				opportunity = null;
			}
		}
	};
	req.send();
    return opportunity;
}