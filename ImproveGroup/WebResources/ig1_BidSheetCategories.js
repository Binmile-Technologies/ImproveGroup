function onLoad()
{
	var name=Xrm.Page.getAttribute("ig1_name").getValue();
	if(name!="" && name!=null && name!=undefined)
	{
		Xrm.Page.getControl("ig1_name").setDisabled(true);
	}
}
function checkForDuplicateRecord()
{
	var name=Xrm.Page.getAttribute("ig1_name").getValue();

        var regex = /^[A-Za-z0-9_ ]+$/;
        var isValid = regex.test(name);
        if (!isValid) 
        {
            Xrm.Page.getAttribute("ig1_name").setValue(null);
            alert("Special character is not allowed");
           // Xrm.Page.getControl("ig1_name").setNotification("Special character is not allowed");
            return;
        } 

  		var fetchData = {
		ig1_name: name
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheetcategory'>",
					"    <attribute name='ig1_name' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_name' operator='eq' value='", fetchData.ig1_name, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				   ].join("");
						
	var categoryFetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(categoryFetchData.length>0 && categoryFetchData !='' && categoryFetchData !=null && categoryFetchData !=undefined)
	{
		alert("The Category already exist with the same name");
		Xrm.Page.getAttribute("ig1_name").setValue(null);
	}
}