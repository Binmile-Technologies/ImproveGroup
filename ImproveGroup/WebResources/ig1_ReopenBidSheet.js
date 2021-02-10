function reopenBidSheet(formContext)
{
 debugger;
 var isConfirmed = confirm("Bidsheet will be reopned");
 if (!isConfirmed) 
 {
  return;
 } 
  var opportunityId="";
  var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
  var opportunityTitle=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
  
  var upperRev=Xrm.Page.getAttribute("ig1_upperrevisionid").getValue();
  
  if(opportunityTitle==undefined || opportunityTitle==null || opportunityTitle=="")
  {
	alert("Please Select Opportunity Title");
        return;
  }
  opportunityId=opportunityTitle[0].id.replace("{", "").replace("}", "");
  var oppStatusOpen = isOpportunityOpen(opportunityTitle);
    if(!oppStatusOpen)
    {
        alert("Can't Reopen BidSheet as Opportunity is Closed");
        return;
    }
  var maxUpperRevision=reopenBidSheet_maxUpperRevision(opportunityId);
  if(maxUpperRevision>upperRev)
  {
      alert("Bid Sheet of revision id "+ maxUpperRevision +".0 or above can only be opened");
      return;
  }
  action_ReopenBidSheet(formContext);
}

function action_ReopenBidSheet(formContext)
{
	Xrm.Utility.showProgressIndicator("Processing...");
	var parameters = {};
  	var opportunity = formContext.getAttribute("ig1_opportunitytitle").getValue();
	if(opportunity!=undefined && opportunity!=null && opportunity!="")
	{
		parameters.opportunityid = opportunity[0].id.replace("{", "").replace("}", "");
	}
	var pricelist = formContext.getAttribute("ig1_pricelist").getValue();
	if(pricelist!=undefined && pricelist!=null && pricelist!="")
	{
		parameters.pricelistid = pricelist[0].id.replace("{", "").replace("}", "");
	}
	if(formContext.data.entity.getId()!=undefined && formContext.data.entity.getId()!=null && formContext.data.entity.getId()!="")
	{
		parameters.bidsheetid = formContext.data.entity.getId().replace("{", "").replace("}", "");
	}
	if(formContext.getAttribute("ig1_upperrevisionid").getValue()!=undefined && formContext.getAttribute("ig1_upperrevisionid").getValue()!=null && formContext.getAttribute("ig1_upperrevisionid").getValue()!="")
	{	
		parameters.upperRevision = formContext.getAttribute("ig1_upperrevisionid").getValue().toString();
	}
	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_ReopenBidsheet", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) 
		{
			req.onreadystatechange = null;
			if (this.status === 204) 
			{
				//Success - No Return Data - Do Something
				formContext.data.refresh();
				Xrm.Utility.closeProgressIndicator();
			} 
			else 
			{
				Xrm.Utility.alertDialog(this.statusText);
				Xrm.Utility.closeProgressIndicator();
			}
		}
	};
	req.send(JSON.stringify(parameters));
	
}
//Nazish - 12-24-2019 - Below function is added to get max revision id.
function reopenBidSheet_maxUpperRevision(opportunityId)
{
        var maxUpperRevision=0;
        var upperRevisionId=Xrm.Page.getAttribute("ig1_upperrevisionid").getValue();
    	var fetchData = {
		ig1_opportunitytitle: opportunityId,
		ig1_upperrevisionid: upperRevisionId
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheet'>",
                    "    <attribute name='ig1_upperrevisionid' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*8b4029e1-1a06-ea11-a811-000d3a55d0f0*/, "'/>",
                    "      <condition attribute='ig1_upperrevisionid' operator='gt' value='", fetchData.ig1_upperrevisionid/*3*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                    ].join("");
    
    var fetchRecord=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(fetchRecord!=undefined && fetchRecord!=null && fetchRecord!="")
    {
        for(i=0; i<fetchRecord.length; i++)
        {
            if(fetchRecord[i].attributes.ig1_upperrevisionid.value>maxUpperRevision)
            {
              maxUpperRevision=fetchRecord[i].attributes.ig1_upperrevisionid.value;
            }
        }
    }
  return maxUpperRevision;
}