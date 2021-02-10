function getQuoteDetails()
{
    var quoteId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    
  	var fetchData = {
		salesorderid: quoteId
	};
	var fetchXml = [
    "<fetch mapping='logical' version='1.0'>",
    "  <entity name='quote'>",
    "    <attribute name='ig1_freighttax' />",
    "    <attribute name='ig1_freightamount' />",
    "    <link-entity name='salesorder' from='quoteid' to='quoteid'>",
    "      <filter type='and'>",
    "        <condition attribute='salesorderid' operator='eq' value='", fetchData.salesorderid/*4061ca1b-4cf6-e911-a812-000d3a55d2c3*/, "'/>",
    "      </filter>",
    "    </link-entity>",
    "  </entity>",
    "</fetch>",
        ].join("");
        
    var quoteData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(quoteData!=undefined && quoteData!=null && quoteData!="")
    {
        var result=quoteData[0].attributes;
        if(Xrm.Page.getAttribute("ig1_freighttax").getValue()==null || Xrm.Page.getAttribute("ig1_freighttax").getValue()=="")
        {
            if(result.ig1_freighttax!=undefined && result.ig1_freighttax!=null && result.ig1_freighttax!="")
            {
                Xrm.Page.getAttribute("ig1_freighttax").setValue(parseFloat(result.ig1_freighttax.value));
            }
            else
            {
              Xrm.Page.getAttribute("ig1_freighttax").setValue(parseFloat(0));
            }
        }
        if(Xrm.Page.getAttribute("ig1_freightamount").getValue()==null || Xrm.Page.getAttribute("ig1_freightamount").getValue()=="")
        {
            if(result.ig1_freightamount!=undefined && result.ig1_freightamount!=null && result.ig1_freightamount!="")
            {
                Xrm.Page.getAttribute("ig1_freightamount").setValue(parseFloat(result.ig1_freightamount.value));
            }
            else
            {
              Xrm.Page.getAttribute("ig1_freightamount").setValue(parseFloat(0));
            }
        }
    }
}


function getOrderDetails()
{
    var invoiceid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var IsFreightSet = Xrm.Page.getAttribute("ig1_isfreightset").getValue();
       if(IsFreightSet)
          return;

  	var fetchData = {
		invoiceid: invoiceid
	};
	var fetchXml = [
                                  "<fetch mapping='logical' version='1.0'>",
                                  "  <entity name='salesorder'>",
                                  "    <attribute name='ig1_freighttax' />",
                                  "    <attribute name='ig1_freightamount' />",
                                  "    <link-entity name='invoice' from='salesorderid' to='salesorderid'>",
                                  "      <filter type='and'>",
                                  "        <condition attribute='invoiceid' operator='eq' value='", fetchData.invoiceid/*B8A6D251-60F6-E911-A812-000D3A55D2C3*/, "'/>",
                                  "      </filter>",
                                  "    </link-entity>",
                                  "  </entity>",
                                  "</fetch>",
	                           ].join("");
        
    var orderData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(orderData!=undefined && orderData!=null && orderData!="")
    {
        var result=orderData[0].attributes;
        if(Xrm.Page.getAttribute("ig1_freighttax").getValue()==null || Xrm.Page.getAttribute("ig1_freighttax").getValue()=="")
        {
            if(result.ig1_freighttax!=undefined && result.ig1_freighttax!=null && result.ig1_freighttax!="")
            {
                Xrm.Page.getAttribute("ig1_freighttax").setValue(parseFloat(result.ig1_freighttax.value));
            }
            else
            {
              Xrm.Page.getAttribute("ig1_freighttax").setValue(parseFloat(0));
            }
        }
        if(Xrm.Page.getAttribute("ig1_freightamount").getValue()==null || Xrm.Page.getAttribute("ig1_freightamount").getValue()=="")
        {
            if(result.ig1_freightamount!=undefined && result.ig1_freightamount!=null && result.ig1_freightamount!="")
            {
                Xrm.Page.getAttribute("ig1_freightamount").setValue(parseFloat(result.ig1_freightamount.value));
            }
            else
            {
              Xrm.Page.getAttribute("ig1_freightamount").setValue(parseFloat(0));
            }
        }
		Xrm.Page.getAttribute("ig1_isfreightset").setValue(true);
    }
}


