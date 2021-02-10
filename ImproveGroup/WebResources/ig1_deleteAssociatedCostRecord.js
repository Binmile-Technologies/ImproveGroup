function deleteAssociatedCostRecord()
{
        var bidSheetId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
        var fetchData = {ig1_bidsheet: bidSheetId};
        var fetchXml = [
                        "<fetch mapping='logical' version='1.0'>",
                        "  <entity name='ig1_associatedcost'>",
                        "    <attribute name='ig1_bidsheetcategory' />",
                        "    <attribute name='ig1_associatedcostid' />",
                        "    <filter type='and'>",
                        "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*dd7055fb-943b-ea11-a812-000d3a55dd4e*/, "'/>",
                        "    </filter>",
                        "  </entity>",
                        "</fetch>",
                       ].join("");
        
        var associatedCostCategories = XrmServiceToolkit.Soap.Fetch(fetchXml);
        if(associatedCostCategories!=null && associatedCostCategories!="" && associatedCostCategories.length>0)
        {
           for(i=0; i<associatedCostCategories.length; i++)
           {
               var result=associatedCostCategories[i].attributes;
               if(result.ig1_bidsheetcategory!=undefined && result.ig1_bidsheetcategory!=null && result.ig1_bidsheetcategory!="")
               {
                   var associatedCostCategoryId = result.ig1_bidsheetcategory.id;
                   var isBidSheetLineItemExist = isBidSheetLineItmeExist(bidSheetId, associatedCostCategoryId);
                   if(!isBidSheetLineItemExist)
                   {
                          var associatedCostId = result.ig1_associatedcostid.value.replace("{", "").replace("}", "");
                          //Dis-Associating associatedCost from bidsheet line item if category changed
                          disAssociate_AssociatedCostFromBSLineItems(associatedCostId);
                          
                          Xrm.WebApi.online.deleteRecord("ig1_associatedcost", associatedCostId).then(
                          function success(result) 
                          {
                            //Success - No Return Data - Do Something
                          },
                          function(error)
                          {
                            Xrm.Utility.alertDialog(error.message);
                          }
                       ); 
                   }
               }
           }
          // Xrm.Page.data.refresh();
        }
}

function isBidSheetLineItmeExist(bidSheetId, categoryId)
{
    var isLineItemExist = false;
   	var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_category: categoryId
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheetpricelistitem'>",
                    "    <attribute name='ig1_category' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*dd7055fb-943b-ea11-a812-000d3a55dd4e*/, "'/>",
                    "      <condition attribute='ig1_category' operator='eq' value='", fetchData.ig1_category/*75c6d32a-22cf-e911-a969-000d3a1d578c*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                   ].join(""); 
    
    var bidSheetLineItem = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(bidSheetLineItem!=undefined && bidSheetLineItem!=null && bidSheetLineItem!="" && bidSheetLineItem.length>0)
    {
        isLineItemExist = true;
    }
    return isLineItemExist;    
}

function disAssociate_AssociatedCostFromBSLineItems(associatedCostId)
{
    	var fetchData = {
		ig1_associatedcostid: associatedCostId
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheetpricelistitem'>",
                    "    <attribute name='ig1_bidsheetpricelistitemid' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_associatedcostid' operator='eq' value='", fetchData.ig1_associatedcostid/*93e693fe-413c-ea11-a812-000d3a55d0f0*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                   ].join("");
    
    var bidSheetLineItemData= XrmServiceToolkit.Soap.Fetch(fetchXml)
    if(bidSheetLineItemData!=undefined && bidSheetLineItemData!=null && bidSheetLineItemData!="" && bidSheetLineItemData.length>0)
    {
        for(i=0; i<bidSheetLineItemData.length>0; i++)
        {
            var result = bidSheetLineItemData[i].attributes;
            if(result.ig1_bidsheetpricelistitemid!=undefined && result.ig1_bidsheetpricelistitemid!=null && result.ig1_bidsheetpricelistitemid!="")
            {
                var bidsheetLineemid = result.ig1_bidsheetpricelistitemid.value.replace("{", "").replace("}", "");
                
                var req = new XMLHttpRequest();
                req.open("DELETE", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+associatedCostId+")/ig1_ig1_associatedcost_ig1_bidsheetpricelisti("+bidsheetLineemid+")/$ref", false);
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.onreadystatechange = function() {
                    if (this.readyState === 4) 
                    {
                        req.onreadystatechange = null;
                        if (this.status === 204 || this.status === 1223) 
                        {
                            //Success - No Return Data - Do Something
                        } 
                        else 
                        {
                            Xrm.Utility.alertDialog(this.statusText);
                        }
                    }
                };
                req.send();
            }
        }
    }
}