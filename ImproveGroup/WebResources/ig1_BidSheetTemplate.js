function createBidSheetTemplate(formContext, entityRefrence, selectedRowCount)
{
 debugger;
 if(selectedRowCount>1)
 {
	 alert("Only one row can be selected");
	 return;
 }
 if(selectedRowCount<=0)
 {
	alert("Please select BidSheet to create Template");
	 return; 
 }

  var isConfirmed = confirm("Template will be created for selected Bid Sheet");
  if (!isConfirmed) 
  {
    return
  }
 
   var gridContext = formContext.getControl("BidSheetTemplate");
    var parameters = {};
    var isConfirmed = confirm("Keep Pricing? Ok = keep product pricing, Cancel = Set prices to 0");
    if (isConfirmed) 
    {
		parameters.keepPricing = true;
    }
	else
	{
		parameters.keepPricing = false;
	}
	parameters.bidSheetId = entityRefrence[0].Id.replace("{", "").replace("}", "");
	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_createTemplateFromBidSheet", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() 
	{
		if (this.readyState === 4) 
		{
			req.onreadystatechange = null;
			if (this.status === 204) 
			{
				alert("Template created");
				gridContext.refresh();
			}
			else 
			{
				alert(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
}

function createBidSheetFromTemplatet(formContext, entityRefrence, selectedRowCount)
{
  debugger;
  if(selectedRowCount>1)
  {
	alert("Only one row can be selected");
	return;
  }
  if(selectedRowCount<=0)
 {
	alert("Please select Template to create BidSheet");
	 return; 
 }
  var isConfirmed = confirm("BidSheet will be created for selected Template");
  if (!isConfirmed) 
  {
    return
  }
       var opportunityid = formContext.data.entity.getId().replace("{", "").replace("}", "");
	var parameters = {};
	var gridContext = formContext.getControl("BidSheet");
	var templateGridContext = formContext.getControl("BidSheetTemplate"); 
	parameters.topic = formContext.getAttribute("name").getValue();
	parameters.opportunityId = opportunityid;
	parameters.templateId = entityRefrence[0].Id.replace("{", "").replace("}", "");
        var bidsheetExist = isBidsheetExist(opportunityid);
	if(bidsheetExist)
	{
	   var isConfirmed = confirm("Change Order ? Ok = Create Change Order, Cancel = Create Revision");
	   if (isConfirmed) 
	   {
		 parameters.isChangeOrder = true;
	   }
	   else
	   {
		  parameters.isChangeOrder = false;
	   }
	}
	else
	{
	  parameters.isChangeOrder = true;	
	}
     var isConfirmed = confirm("Keep Pricing? Ok = keep product pricing, Cancel = Set prices to 0");
    if (isConfirmed) 
    {
		parameters.keepPricing = true;
    }
	else
	{
		parameters.keepPricing = false;
	}
	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_createBidsheetFromTemplate", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) 
			{
				//Success - No Return Data - Do Something
				alert("Bid Sheet has been created");
				gridContext.refresh();
				templateGridContext.refresh();
				gridContext.refreshRibbon();
				templateGridContext.refreshRibbon();
			} 
			else 
			{
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
  
}

function createBidSheetTemplate_openedBidsheet(formContext)
{
  debugger;
  var isConfirmed = confirm("Template will be created for this Bid Sheet");
  if (!isConfirmed) 
  {
    return
  }
 if(!isConfirmed)
 {
	return; 
 }
	var parameters = {};
	var isConfirmed = confirm("Keep Pricing? Ok = keep product pricing, Cancel = Set prices to 0");
    if (isConfirmed) 
    {
		parameters.keepPricing = true;
    }
	else
	{
		parameters.keepPricing = false;
	}
	parameters.bidSheetId = formContext.data.entity.getId().replace("{", "").replace("}", "");
	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_createTemplateFromBidSheet", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() 
	{
		if (this.readyState === 4) 
		{
			req.onreadystatechange = null;
			if (this.status === 204) 
			{
				alert("Template created");
			}
			else 
			{
				alert(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
}

function deleteBidSheetOrTemplate(formContext, entityRefrence)
{     
	debugger;
	var isConfirmed = confirm("The selected record[s] will be deleted permanently");
	if (!isConfirmed) 
	{
		return
	}
	var bidsheetsid = "";
	for(var i=0; i<entityRefrence.length; i++)
	{
		if(i==0)
		{
			bidsheetsid = entityRefrence[i].Id.replace("{", "").replace("}", "");
		}
		else
		{
			bidsheetsid =bidsheetsid+ ","+entityRefrence[i].Id.replace("{", "").replace("}", "");
		}
	}
	if(bidsheetsid==undefined || bidsheetsid == null || bidsheetsid=="")
	{
		alert("Pleae select bidsheet");
		return;
	}
	var isRecordsDeleted = false;
	var parameters = {};
	parameters.recordId = bidsheetsid;

	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_DeleteBidSheetOrTemplate", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() 
	{
		if (this.readyState === 4) 
		{
			req.onreadystatechange = null;
			if (this.status === 204) 
			{
				//Success - No Return Data - Do Something
				isRecordsDeleted = true;
			} else 
			{
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
		
	if(isRecordsDeleted)
	{
		alert("Selected Record[s] have been deleted");
		var entityName = formContext._entityName;
		if(entityName=="opportunity")
		{
			var bidSheetTemplateContext = formContext.getControl("BidSheetTemplate"); 
			var bidSheetContext = formContext.getControl("BidSheet");
			bidSheetTemplateContext.refresh();
			bidSheetContext.refresh();
		}
		else
	    {
		  formContext.refresh();
	    }
	}
}

function enableCreateBidSheetFromTemplate(formContext, entityRefrence)
{
	debugger;
	var opportunityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
	var isDraftBidSheetExist = isDraftBSExist(opportunityId);
	var fetchData = {
		ig1_status: "286150003",
		ig1_bidsheetid: entityRefrence[0].Id.replace("{", "").replace("}", "")
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_status' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150003*/, "'/>",
					"      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*38f07b5f-8d74-48d5-92ac-e858506968e7*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
	var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!=undefined && fetchData!=null && fetchData!="" && fetchData.length>0 )
	{
		return true;
	}
	else
	{
		return false;
	}
}
function enableCreateBidSheetTemplate(formContext, entityRefrence)
{
	debugger;
	
	var fetchData = {
		ig1_status: "286150003",
		ig1_bidsheetid: entityRefrence[0].Id.replace("{", "").replace("}", "")
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_status' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_status' operator='neq' value='", fetchData.ig1_status/*286150003*/, "'/>",
					"      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*38f07b5f-8d74-48d5-92ac-e858506968e7*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
	var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!=undefined && fetchData!=null && fetchData!="" && fetchData.length>0)
	{
		return true;
	}
	else
	{
		return false;
	}
}
function enable_bidSheetSaveAsTemplate(formContext)
{
	debugger;
	
	var formType = formContext.ui.getFormType();
	var bsStatus = formContext.getAttribute("ig1_status").getValue();
	if(bsStatus!=286150003 && formType!=1)
	{
		return true;
	}
	else
	{
		return false;
	}
}

function enable_deleteSelectedRecord(entityRefrence)
{
	debugger;
    var flag = true;
	for(i=0; i<entityRefrence.length; i++)
	{
		var fetchData = 
		{
			ig1_associated: "1",
			ig1_bidsheetid: entityRefrence[i].Id.replace("{", "").replace("}", "")
		};
		var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_name' />",
					"    <attribute name='ig1_associated' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_associated' operator='eq' value='", fetchData.ig1_associated/*1*/, "'/>",
					"      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*57b5bddf-6552-ea11-a812-000d3a55d0f0*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
					].join("");
		var recordData = XrmServiceToolkit.Soap.Fetch(fetchXml);
		if(recordData!=undefined && recordData!=null && recordData!="" && recordData.length>0)
		{
			flag = false;
			break;
		}
	}
	return flag;
}

function isDraftBSExist(opportunityId)
{
		var fetchData = {
		ig1_status: "286150001",
		ig1_opportunitytitle: opportunityId
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_status' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150001*/, "'/>",
					"      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*8ce041d1-fad2-e911-a967-000d3a1d57cf*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				 ].join("");
				 
	var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!=undefined && fetchData!=null && fetchData!="" && fetchData.length>0)
	{
		return true;
	}
	else
	{
		return false;
	}
}

function isBidsheetExist(opportunityId)
{
  var fetchData = {
		statecode: "0",
		ig1_opportunitytitle: opportunityId
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_status' />",
					"    <filter type='and'>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*8ce041d1-fad2-e911-a967-000d3a1d57cf*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				 ].join("");
				 
	var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!=undefined && fetchData!=null && fetchData!="" && fetchData.length>0)
	{
		return true;
	}
	else
	{
		return false;
	}	
}