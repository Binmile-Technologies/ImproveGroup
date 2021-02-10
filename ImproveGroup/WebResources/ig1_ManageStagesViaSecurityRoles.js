function readOnlyOnLoad()
{
   
   var hasReadOnlyPermissions=getCurrentUserRoles();
   if(hasReadOnlyPermissions)
   {
	   Xrm.Page.getControl("ig1_name").setDisabled(true);
	   Xrm.Page.getControl("ig1_includetaxes").setDisabled(true);
	   Xrm.Page.getControl("ig1_includebonding").setDisabled(true);
	   Xrm.Page.getControl("ig1_uploadpartslist").setDisabled(true);
   }
}

function getCurrentUserRoles()
{
    var flag=false;
	var securityRolesName=new Array();
  	var currentUserRoles = Xrm.Page.context.getUserRoles();
	for(var i = 0; i < currentUserRoles.length; i++) 
	{
        var userRoleId = currentUserRoles[i].replace("{", "").replace("}", "");
		
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			datatype: "json",
			url: Xrm.Page.context.getClientUrl() + "/api/data/v9.1/roles("+userRoleId+")?$select=name",
			beforeSend: function(XMLHttpRequest) {
				XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
				XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
				XMLHttpRequest.setRequestHeader("Accept", "application/json");
				XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
			},
			async: false,
			success: function(data, textStatus, xhr) {
				var result = data;
				securityRolesName.push(result["name"]);
			},
			error: function(xhr, textStatus, errorThrown) {
				Xrm.Utility.alertDialog(textStatus + " " + errorThrown);
			}
		});    
	}
	
	if(!securityRolesName.includes("PM Internal")
	  && !securityRolesName.includes("Designers")
          && !securityRolesName.includes("Salesperson")
          && !securityRolesName.includes("Finance")
	  && !securityRolesName.includes("Order Entry")
	  && !securityRolesName.includes("Procurement")
	  && !securityRolesName.includes("System Administrator")
	  && !securityRolesName.includes("Sales, Enterprise app access")
	  && !securityRolesName.includes("Field Service - App Access"))
	  {
		flag=true;
	  }
          return flag;
}
