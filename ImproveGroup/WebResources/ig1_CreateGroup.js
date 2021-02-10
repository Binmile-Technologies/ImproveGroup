function checkDuplicate()
{
  var name=Xrm.Page.getAttribute("name").getValue();
  	var fetchData = {
		name: name
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='product'>",
					"    <attribute name='name' />",
					"    <attribute name='productnumber' />",
					"    <filter type='and'>",
					"      <condition attribute='name' operator='eq' value='", fetchData.name, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
	var fetchXmlData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchXmlData!="" && fetchXmlData!=null && fetchXmlData!=undefined && fetchXmlData.length>0)
	{
		alert("Group already exist with given name")
		Xrm.Page.getAttribute("name").setValue(null);
                Xrm.Page.getAttribute("productnumber").setValue(null);
	}
        else
        {
            Xrm.Page.getAttribute("productnumber").setValue(name);
         }
}