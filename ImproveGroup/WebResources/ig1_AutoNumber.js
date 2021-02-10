function validateProjectNumber()
{
      var projectNumber=Xrm.Page.getAttribute("ig1_projectnumber").getValue();
      if(projectNumber=="" || projectNumber==null || projectNumber==undefined)
        {
          return;
        }

       var fetchXml =  "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >"+
		                 "    <entity name='opportunity' >"+
		                 "        <attribute name='new_opportunitynumber' />"+
                                 "        <order attribute='new_opportunitynumber' descending='true'/>"+
	                       	"        <filter type='and' >"+
	                  	"            <condition attribute='new_opportunitynumber' operator='ge' value='"+projectNumber+"' />"+
	                	"        </filter>"+
	                	"    </entity>"+
		                "</fetch>";
        var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!="" && fetchData!=null && fetchData!=undefined && fetchData.length>0)
	{
            var result=fetchData[0].attributes;
             var maxNumber=result.new_opportunitynumber.value;
	    alert("Please Provide five digit number and must be greater "+maxNumber);
	}
}