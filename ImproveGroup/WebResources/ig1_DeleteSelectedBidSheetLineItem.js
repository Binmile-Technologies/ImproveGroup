function deleteSelectedBidSheetLineItems(selectedControl, selectedItemRefrence, selectedEntityTypeName)
{
  debugger;
  var confirmButton = confirm("This action will remove all the associated records from Indirect Cost stage. Are you sure, you want to delete the selected Bid Sheet Line item(s).");
  if (confirmButton == false) 
  {
    return;
  }
  var selectedItem=selectedItemRefrence;				
  for(i=0; i<selectedItem.length; i++)
  {
    var req = new XMLHttpRequest();
	req.open("DELETE", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+selectedItem[i].Id+")", false);
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204 || this.status === 1223) {
				//Success - No Return Data - Do Something
				setTimeout(function(){Xrm.Page.data.refresh();}, 2000);
				flag=true;
			} else {
				
			}
		}
	};
	req.send();	  
  }
}


function getGrouppedProduct(productId)
{
	var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetproducts?$select=ig1_bidsheetproductid&$filter=_ig1_bidsheet_value eq "+bidSheetId+" and  _ig1_productgroup_value eq "+productId+"", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 200) {
				var results = JSON.parse(this.response);
				for (var i = 0; i < results.value.length; i++) {
					var ig1_bidsheetproductid = results.value[i]["ig1_bidsheetproductid"].replace("{", "").replace("}", "");
					ungroupProduct(ig1_bidsheetproductid);
				}
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();
}

function ungroupProduct(bidSheetProductId)
{
	var parameters = {};
	parameters.SelectedRecords = bidSheetProductId;

	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_UngroupProducts", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) {
				//Success - No Return Data - Do Something
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
}