function onLoad()
{
  var status=Xrm.Page.getAttribute("ig1_status").getValue();
  if(status!=286150001)
   {
     var controls = Xrm.Page.ui.controls.get();
       for (var i in controls) 
       {
         var control = controls[i];
         if (control.getDisabled && control.setDisabled && !control.getDisabled()) 
	   {
             control.setDisabled(true);
            }
      }
    }
//totalCost();
}

function reviseBidSheet()
{
  debugger;
 var status=Xrm.Page.getAttribute("ig1_status").getValue();
 if(status==286150001)
 {
   alert("Can't be revise as bid sheet have draft status");
   return;
 }
  var currentRecordIdString = Xrm.Page.data.entity.getId();
  var currentRecordId = currentRecordIdString.replace("{", '').replace("}", '');

  Process.callAction("ig1_ActionReviseBidSheet",[
  {
    key: "Target",
    type: Process.Type.EntityReference,
    value: new Process.EntityReference("ig1_bidsheet", Xrm.Page.data.entity.getId())
 }], function (params) {
 Xrm.Page.data.refresh(true);
 var controls = Xrm.Page.ui.controls.get();
       for (var i in controls) 
       {
         var control = controls[i];
         control.setDisabled(false);
      }
Xrm.Page.getAttribute("ig1_status").setValue(286150001);
 },
 function (e, t) {
 // Error
 alert("error");
 });
}

function updateStatus()
{
 alert("Updated");
}

function activateBidSheet()
{
	Xrm.Page.getAttribute("ig1_status").setValue(286150000);
	var controls = Xrm.Page.ui.controls.get();
       for (var i in controls) 
       {
         var control = controls[i];
         if (control.getDisabled && control.setDisabled && !control.getDisabled()) 
	   {
             control.setDisabled(true);
            }
      }
Xrm.Page.ui.refreshRibbon();
}
function closeBidSheet()
{
	alert("Closed");
       Xrm.Page.ui.refreshRibbon();
}



function checkBPF()
{
  var selectedStage = Xrm.Page.data.process.getSelectedStage();
  var activeStage = Xrm.Page.data.process.getActiveStage();
  var activeStageId = activeStage.getId();
  var activeStagename = activeStage.getName();
  var selectedStagename = selectedStage.getName();

  if( activeStagename == "Bid Sheet Setup" || selectedStagename =="Bid Sheet Setup" )
  {
         Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(true);
         Xrm.Page.ui.tabs.get("Product Management").setVisible(false);
         Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(false);
	 Xrm.Page.ui.tabs.get("Create BOM").setVisible(false);
         Xrm.Page.ui.tabs.get("Associated Cost").setVisible(false);
	Xrm.Page.ui.tabs.get("Summary").setVisible(false);
	 Xrm.Page.ui.tabs.get("Reports").setVisible(false);
  }
   else if ( activeStagename == "Product Management" || selectedStagename== "Product Management")
   {
		 Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(false);
		 Xrm.Page.ui.tabs.get("Product Management").setVisible(true);
         Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(true);
		 Xrm.Page.ui.tabs.get("Create BOM").setVisible(false);
		 Xrm.Page.ui.tabs.get("Associated Cost").setVisible(false);
		 Xrm.Page.ui.tabs.get("Summary").setVisible(false);
		 Xrm.Page.ui.tabs.get("Reports").setVisible(false);
         listAllProductGroups();
   }
   else if ( activeStagename == "Bill Of Materials" || selectedStagename== "Bill Of Materials")
   {
             Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(false);
	     Xrm.Page.ui.tabs.get("Product Management").setVisible(false);
             Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(false);
	     Xrm.Page.ui.tabs.get("Create BOM").setVisible(true);
	     Xrm.Page.ui.tabs.get("Associated Cost").setVisible(false);
	     Xrm.Page.ui.tabs.get("Summary").setVisible(false);
	     Xrm.Page.ui.tabs.get("Reports").setVisible(false);
		 createPriceListItems();
		 summary();
		
   }
   else if ( activeStagename == "Associated Cost" || selectedStagename =="Associated Cost")
   {
		Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(false);
		Xrm.Page.ui.tabs.get("Product Management").setVisible(false);
        Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(false);
		Xrm.Page.ui.tabs.get("Create BOM").setVisible(false);
		Xrm.Page.ui.tabs.get("Associated Cost").setVisible(true);
		Xrm.Page.ui.tabs.get("Summary").setVisible(false);
		Xrm.Page.ui.tabs.get("Reports").setVisible(false);
		associatedCost();
   }
   else if ( activeStagename == "Summary" || selectedStagename=="Summary")
   {
		Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(false);
	    Xrm.Page.ui.tabs.get("Product Management").setVisible(false);
        Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(false);
	    Xrm.Page.ui.tabs.get("Create BOM").setVisible(false);
	    Xrm.Page.ui.tabs.get("Associated Cost").setVisible(false);
		Xrm.Page.ui.tabs.get("Summary").setVisible(true);
	    Xrm.Page.ui.tabs.get("Reports").setVisible(false);
   }
   else if ( activeStagename == "Reports" || selectedStagename=="Reports")
   {
		Xrm.Page.ui.tabs.get("Bid Sheet Setup").setVisible(false);
	    Xrm.Page.ui.tabs.get("Product Management").setVisible(false);
        Xrm.Page.ui.tabs.get("Upload BOMs").setVisible(false);
	    Xrm.Page.ui.tabs.get("Create BOM").setVisible(false);
	    Xrm.Page.ui.tabs.get("Associated Cost").setVisible(false);
		Xrm.Page.ui.tabs.get("Summary").setVisible(false);
	    Xrm.Page.ui.tabs.get("Reports").setVisible(true);
   createOpportunityLine();
   }

}
 
function showHideTabs()
{
 Xrm.Page.data.process.addOnStageSelected(checkBPF);
  Xrm.Page.data.process.addOnStageChange(checkBPF);  
  checkBPF();
}

function listAllProductGroups()
{   
    var selectedCategory = new Array();
    var str='';
    var fetchData = {
		 ig1_bidsheet: Xrm.Page.data.entity.getId()
	};
	var categoriesFetcXml = [
					"<fetch>",
					"  <entity name='ig1_bscategoryvendor'>",
					"    <attribute name='ig1_category' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				].join("");
	 var fetchCategoryData = XrmServiceToolkit.Soap.Fetch(categoriesFetcXml);
     if(fetchCategoryData!='' && fetchCategoryData!=null && fetchCategoryData!=undefined)	
     {
		 for(i=0; i<fetchCategoryData.length; i++)
		 {
			 var categoryId=fetchCategoryData[i].attributes.ig1_category.id;
                          var categoryName=fetchCategoryData[i].attributes.ig1_category.name;
			 for(j=0; j<fetchCategoryData.length; j++)
			 {
				 var result=fetchCategoryData[j].attributes;
                                  var catName=result.ig1_category.name;
				 if(!selectedCategory.includes(result.ig1_category.id) && categoryId==result.ig1_category.id)
				 {
					selectedCategory.push(result.ig1_category.id.replace('{','').replace('}',''));
				 }
			 }
		 }
	 }		 
				
	str+='<option value="">Select Group to Add Selected Products</option>';
	var fetchXML= "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"+
					"  <entity name='product'>"+
					"    <attribute name='name' />"+
					"    <attribute name='productid' />"+
					"    <attribute name='productnumber' />"+
					"    <attribute name='description' />"+
					"    <attribute name='ig1_bidsheetcategory' />"+
					"    <order attribute='productnumber' descending='false' />"+
					"    <filter type='and'>"+
					"      <condition attribute='ig1_isproductgroup' operator='eq' value='1' />"+
					"      <condition attribute='ig1_bidsheetcategory' operator='not-null' />"+
					"    </filter>"+
					"  </entity>"+
					"</fetch>"
	
	var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXML);
	if(fetchData!=null && fetchData!=undefined && fetchData!='')
	{
		for(i=0; i< selectedCategory.length; i++)
		{
			for(j=0; j<fetchData.length; j++)
			{
			   var result=fetchData[j].attributes;
			   if(selectedCategory[i]==result.ig1_bidsheetcategory.id)
			   {
				 str+='<option value='+result.productid.value+'>'+result.name.value+'</option>';
				}
			}
		}
		$(Xrm.Page.ui.controls.get('WebResource_GroupProducts').getObject()).contents().find('#productGroup').html(str);
	}
}
Xrm.Page.listAllProductGroups=listAllProductGroups;
function getSelectedRows()
{
	     var groupId = $(Xrm.Page.ui.controls.get('WebResource_GroupProducts').getObject()).contents().find('#productGroup').val();
		 var parameters = {};
		 if(groupId =='' || groupId == undefined || groupId ==null)
		 {
			alert("Please select Product Group from dropdown");
			return;
		 }
		 parameters.selectedGroupID = groupId;
		 var selectedrecords = '';
		 var selectedRows = Xrm.Page.getControl("UngroupedProducts").getGrid().getSelectedRows();
                selectedRows.forEach(function(selectedRow, i) {
                entityGuid = selectedRow.getData().getEntity().getId();
				if(selectedrecords=='')
				{
					selectedrecords=entityGuid;
				}
				else
				{
					selectedrecords+=','+entityGuid;
				}
             });
		parameters.selectedRecordsId = selectedrecords;
		
		if(selectedrecords =='' || selectedrecords ==null || selectedrecords ==undefined)
		{
			alert("Please select record(s) from Ungrouped Products grid");
			return;
		}

		var req = new XMLHttpRequest();
		req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_GroupProducts", true);
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
					alert("Product(s) has been added to the Selected Group");
                                        Xrm.Page.data.refresh();
				} 
                                else 
				{
					alert(Xrm.Utility.alertDialog(this.statusText));
				}
			}
		};
		req.send(JSON.stringify(parameters));
}
Xrm.Page.getSelectedRows=getSelectedRows;

function getUngroupSelectedRows()
{
		 var parameters = {};
		 var selectedrecords = '';
		 var selectedRows = Xrm.Page.getControl("GroupedProduct").getGrid().getSelectedRows();
                selectedRows.forEach(function(selectedRow, i) {
                entityGuid = selectedRow.getData().getEntity().getId();
				if(selectedrecords=='')
				{
					selectedrecords=entityGuid;
				}
				else
				{
					selectedrecords+=','+entityGuid;
				}
             });
		parameters.SelectedRecords = selectedrecords;
		
		if(selectedrecords =='' || selectedrecords ==null || selectedrecords ==undefined)
		{
			alert("Please select record(s) from Grouped Products grid");
			return;
		}

		var req = new XMLHttpRequest();
		req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_UngroupProducts", true);
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
					alert("Product(s) has been Ungrouped");
                                        Xrm.Page.data.refresh();
				} else 
				{
					alert(Xrm.Utility.alertDialog(this.statusText));
				}
			}
		};
		req.send(JSON.stringify(parameters));
}
Xrm.Page.getUngroupSelectedRows=getUngroupSelectedRows;

function createPriceListItems()
{
     var bidSheetId = Xrm.Page.data.entity.getId();
     var opportunityId=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue()[0].id;
	var parameters = {};
    parameters.bidSheetId = bidSheetId;
    parameters.opportunityId=opportunityId;
    var req = new XMLHttpRequest();
    Xrm.Utility.showProgressIndicator("Please Wait Updating Data...");
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_CreatePriceListItems", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function() {
    if (this.readyState === 4) {
        req.onreadystatechange = null;
        if (this.status === 204) {
            //Success - No Return Data - Do Something
			 summary();
             Xrm.Utility.closeProgressIndicator();
             Xrm.Page.data.refresh(true);
        } else {
            Xrm.Utility.alertDialog(this.statusText);
        }
    }
};
req.send(JSON.stringify(parameters));
}
//Nazish - Creating Opportunity Line Items
function createOpportunityLine()
{
	var opportunityId='';
	var bidSheetId=Xrm.Page.data.entity.getId().replace('{','').replace('}','');
	var opportunityTitle=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
	if(opportunityTitle!='' && opportunityTitle!= null && opportunityTitle!= undefined)
	{
		opportunityId=opportunityTitle[0].id.replace('{','').replace('}','');
	}
	var parameters = {};
	parameters.bidSheetId = bidSheetId;
	parameters.opportunityId = opportunityId;

	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_CreateOpportunityLine", true);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) {
				//Success - No Return Data - Do Something
                                alert("Opportunity Line Created");
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
}
//Nazish - Below function is added to calculate summary
function summary()
{
	var materialCost=0;
	var projectHour=0;
	var rate=0;
	var ProjectCost=0;
	var freight=0
	var totalCost=0;
        var luextendTotal =0;
	var str= '';
	str+='<tbody><th>Material Cost</th><!--th>Rate</th--><th>Freight Total</th><th>Total Cost</th>';
	
	var bidSheetId = {
		ig1_bidsheetId: Xrm.Page.data.entity.getId()
	};
	var fetchXml = [
						"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
						"  <entity name='ig1_bidsheetpricelistitem'>",
						
						"    <attribute name='ig1_materialcost' />",
						
						"    <attribute name='ig1_projectlu' />",
						
						"    <attribute name='ig1_freighttotal' />",
                                                "    <attribute name='ig1_luextend' />",

						"    <filter type='and'>",
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", bidSheetId.ig1_bidsheetId, "'/>",
						"    </filter>",
						"    <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category' link-type='inner'>",
						"      <attribute name='ig1_rate' alias='rate' />",
						"    </link-entity>",
						"  </entity>",
						"</fetch>",
					].join("");
	
					var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
					if(fetchData!=null && fetchData!=undefined && fetchData!='')
					{
					 for(i=0; i< fetchData.length; i++)
					 {
					   result=fetchData[i].attributes;
					   if(result.ig1_materialcost != null)
					   materialCost+=parseFloat(result.ig1_materialcost.value);
						if(result.ig1_freighttotal != null)
					   freight+=parseFloat(result.ig1_freighttotal.value);
				       if(result.ig1_luextend != null)
                       luextendTotal+=parseFloat(result.ig1_luextend.value);

					 }
					 totalCost+=materialCost+freight;
                                          
					 //str+='<tr><td>$'+materialCost.toFixed(2)+'</td><td>'+luextendTotal.toFixed(4)+'</td><td>$'+freight.toFixed(2)+'</td><td>$'+totalCost.toFixed(2)+'</td></tr>';
					 str+='<tr><td>$'+materialCost.toFixed(2)+'</td><td>$'+freight.toFixed(2)+'</td><td>$'+totalCost.toFixed(2)+'</td></tr>';
					 
					}
					str+='</tbody></table>';
					$(Xrm.Page.ui.controls.get('WebResource_Summary').getObject()).contents().find('#summary').html(str);
}
Xrm.Page.summary=summary;

//Nazish - Function is added to make the read only cells into the editable subgrid.
function disableEditableGridCells(context)
{
     var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if ((field._attributeName !=="ig1_unitprice" && field._attributeName !=="ig1_quantity" && field._attributeName!==undefined) || (field.getName()!=="ig1_unitprice" && field.getName()!=="ig1_quantity" && field.getName()!==undefined)) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}





//Nazish - Function added to calculate amount based on unit price and quantity.
function calculateTotalAmount(context)
{   
	var control = context.getEventSource();
	var row= control.getParent();
	var unitPrice=row.attributes.get("ig1_unitprice").getValue();
	var quantity = row.attributes.get("ig1_quantity").getValue();
	var totalAmount=parseFloat(unitPrice)*parseFloat(quantity);
	row.attributes.get("ig1_totalamount").setValue(totalAmount);
}

//Nazih - Function added to calculate LU Extend
function luExtendCalculation(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var laborUnit=row.attributes.get("ig1_projectlu").getValue();
	var materialCost = row.attributes.get("ig1_materialcost").getValue();
	var luExtend=(parseFloat(laborUnit)*parseFloat(materialCost));
	row.attributes.get("ig1_luextend").setValue(luExtend);
	//Updating BOM 
	UpdateBillOfMaterials(context);
}

//Nazish - Function added to calculate freight total
function freightTotalCalculation(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var freightamount=row.attributes.get("ig1_freightamount").getValue();
	var margin = row.attributes.get("ig1_markup").getValue();
	var freightTotal=(parseFloat(freightamount)/parseFloat(margin));
	row.attributes.get("ig1_freighttotal").setValue(freightTotal);
	//Updating BOM 
	UpdateBillOfMaterials(context);
}


function UpdateBillOfMaterials(context)
{
	
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace('{','').replace('}','');
	
	var entity = {};
	entity.ig1_luextend = parseFloat(row.attributes.get("ig1_luextend").getValue());
	entity.ig1_projectlu = parseFloat(row.attributes.get("ig1_projectlu").getValue());
	entity.ig1_freightamount = Number(parseFloat(row.attributes.get("ig1_freightamount").getValue()).toFixed(4));
	entity.ig1_freighttotal = Number(parseFloat(row.attributes.get("ig1_freighttotal").getValue()).toFixed(4));
	entity.ig1_markup = parseFloat(row.attributes.get("ig1_markup").getValue());


	var req = new XMLHttpRequest();
	req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+recordId+")", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) {
				summary();
				//Success - No Return Data - Do Something
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}

//Nazih - Function added to make all non-numeric fields, materialCost, LuExtend and freight total field as read only
function disbaleEditableGridCellsOnCreateBOM(context)
{
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_category" 
			|| field._attributeName ==="ig1_product" 
			|| field._attributeName==="ig1_materialcost" 
			|| field._attributeName==="ig1_luextend" 
			|| field._attributeName === "ig1_freighttotal" 
                        || field._attributeName === "ig1_projecthours" 
			|| field._attributeName==="ig1_totalamount") 
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_category" 
			|| field.getName()==="ig1_product" 
			|| field.getName()==="ig1_materialcost"
			|| field.getName()==="ig1_luextend"
			|| field.getName()=== "ig1_freighttotal" 
                        || field.getName()=== "ig1_projecthours" 
			|| field.getName()==="ig1_totalamount") 
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Nazish -- Creating/Updating Associated Cost from Bidsheet Price List Item
function associatedCost()
{
		var fetchData = {
		ig1_bidsheet: Xrm.Page.data.entity.getId()
	};
	var fetchXml = [
					 "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
					 "  <entity name='ig1_bidsheetpricelistitem'>",
					 "    <attribute name='ig1_freightamount' />",
                                         "    <attribute name='ig1_freighttotal' />",
					 "    <attribute name='ig1_markup' />",
					 "    <attribute name='ig1_materialcost' />",
					 "    <attribute name='ig1_category' />",
					 "    <attribute name='ig1_projectcost' />",
					 "    <attribute name='ig1_projectlu' />",
					 "    <attribute name='ig1_projecthours' />",
					 "    <attribute name='ig1_luextend' />",
					 "    <filter type='and'>",
					 "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					 "    </filter>",
					 "  </entity>",
					 "</fetch>",
				    ].join("");
					
	 var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	 if(fetchData!='' && fetchData!=null && fetchData!=undefined)
	 {
		 var categories=new Array();
		 for(i=0; i<fetchData.length; i++)
		 {
			 var materialCost=0;
	         var projectHour=0;
			 var margin=0;
			 var luExtend=0;
			 var freighttotal=0;
			 var flag=false;
			 var categoryId=fetchData[i].attributes.ig1_category.id;
			 for(j=0; j<fetchData.length; j++)
			 {
				  var secondResult=fetchData[j].attributes;
				  if(!categories.includes(categoryId) && secondResult.ig1_category.id==categoryId)
				   {
					 materialCost+=parseFloat(secondResult.ig1_materialcost.value);
                                          luExtend+=parseFloat(secondResult.ig1_luextend.value);
					 freighttotal+=parseFloat(secondResult.ig1_freighttotal.value);
					 flag=true;
				   }
			 }
			 categories.push(categoryId);
			 if(flag)
			 {
				associatedCostData(categoryId, materialCost,  luExtend, freighttotal); 
			 }

    
		 }
	 }

}


function associatedCostData(categoryId, materialCost,  luExtend, freight)
{
	var bidSheetId=Xrm.Page.data.entity.getId().toUpperCase().replace('{','').replace('}','');
    var categoryid=categoryId.replace('{','').toUpperCase().replace('}','')
  var fetchXmlCostAllowance = [
							"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
							"  <entity name='ig1_projectcostallowances'>",
							"    <attribute name='ig1_salesfactor' />",
							"    <attribute name='ig1_designlaborrate' />",
							"    <attribute name='ig1_designfactor' />",
							"    <attribute name='ig1_saleslaborrate' />",
							"  </entity>",
							"</fetch>",
					    ].join("");
		var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXmlCostAllowance);
		var result=fetchData[0].attributes;

	    var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_bidsheetcategory: categoryid
	};
		var fetchXmlAssociatedCost = [
						"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
						"  <entity name='ig1_associatedcost'>",
						"    <attribute name='ig1_associatedcostid' />",
						"    <attribute name='ig1_bidsheetcategory' />",
						"    <attribute name='ig1_bidsheet' />",
						"    <attribute name='ig1_designfactor' />",
						"    <attribute name='ig1_designlaborrate' />",
						"    <attribute name='ig1_salesfactor' />",
						"    <attribute name='ig1_saleslaborrate' />",
						"    <attribute name='ig1_materialcost' />",
						"    <attribute name='ig1_totalmaterialcost' />",
						"    <attribute name='ig1_margin' />",
						"    <attribute name='ig1_travelcost' />",
						"    <attribute name='ig1_pmlaborsme' />",
						"    <filter type='and'>",
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
						"      <condition attribute='ig1_bidsheetcategory' operator='eq' value='", fetchData.ig1_bidsheetcategory, "'/>",
						"    </filter>",
						"  </entity>",
						"</fetch>",
							].join("");
		var associatedCostData=XrmServiceToolkit.Soap.Fetch(fetchXmlAssociatedCost);
		if(associatedCostData.length<=0 || associatedCostData=='' || associatedCostData==null || associatedCostData==undefined)
		{
			var entity = {};
			entity["ig1_BidSheet@odata.bind"] = "/ig1_bidsheets("+bidSheetId+")";
			entity["ig1_BidSheetCategory@odata.bind"] = "/ig1_bidsheetcategories("+categoryid+")";
			
			entity.ig1_luextend = luExtend;
			entity.ig1_laborrate = 0;
			entity.ig1_margin = 1;
			entity.ig1_freight = Number((parseFloat(freight)).toFixed(4));
			entity.ig1_totalmaterialcost = Number((parseFloat(materialCost)).toFixed(4));
			entity.ig1_pmlaborsme = parseFloat(entity.ig1_laborrate)*parseFloat(luExtend);
			entity.ig1_designfactor = parseFloat(result.ig1_designfactor.value);
			//entity.ig1_totalprojecthours = parseFloat(projectHour);
			entity.ig1_designlaborrate = Number(parseFloat(result.ig1_designlaborrate.value).toFixed(4));
			entity.ig1_designcost = Number(((parseFloat(result.ig1_designfactor.value))*(parseFloat(luExtend))*(parseFloat(result.ig1_designlaborrate.value))).toFixed(4));
			entity.ig1_salesfactor = parseFloat(result.ig1_salesfactor.value);
			entity.ig1_saleslaborrate = Number(parseFloat(result.ig1_saleslaborrate.value).toFixed(4));
			entity.ig1_salescost = Number(((parseFloat(result.ig1_salesfactor.value))*(parseFloat(luExtend))*(parseFloat(result.ig1_saleslaborrate.value))).toFixed(4));
			entity.ig1_airfaretrans = Number(parseFloat(0).toFixed(4));
			entity.ig1_perdiem = Number(parseFloat(0).toFixed(4));
			entity.ig1_days = 0;
			entity.ig1_travelcost = Number(parseFloat(0).toFixed(4));
			entity.ig1_materialcost = Number((parseFloat(materialCost)).toFixed(4));
            entity.ig1_totalcost = Number((entity.ig1_totalmaterialcost+entity.ig1_designcost+entity.ig1_salescost+entity.ig1_travelcost+entity.ig1_freight).toFixed(4));

			var req = new XMLHttpRequest();
			req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts", true);
			req.setRequestHeader("OData-MaxVersion", "4.0");
			req.setRequestHeader("OData-Version", "4.0");
			req.setRequestHeader("Accept", "application/json");
			req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
			req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
			req.onreadystatechange = function() {
				if (this.readyState === 4) {
					req.onreadystatechange = null;
					if (this.status === 204) {
						var uri = this.getResponseHeader("OData-EntityId");
						var regExp = /\(([^)]+)\)/;
						var matches = regExp.exec(uri);
						var newEntityId = matches[1];
					} else {
						Xrm.Utility.alertDialog(this.statusText);
					}
				}
			};
			req.send(JSON.stringify(entity));
		}
		else
		{
			var recordId =  associatedCostData[0].attributes.ig1_associatedcostid.value;
			updateAssociatedCost(recordId,materialCost,freight,luExtend,associatedCostData[0]);
		}
}

//Nazish Update Associated Cost when BidSheet Price List Item changes 
function updateAssociatedCost(recordId ,materialCost, freightTotal , luExtend,associatedCostData)
{
	var entity = {};
	
	
	entity.ig1_luextend = parseFloat(luExtend);
	entity.ig1_freight = Number(parseFloat(freightTotal).toFixed(4));
	entity.ig1_designcost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend)*parseFloat(associatedCostData.attributes.ig1_designlaborrate.value)).toFixed(4));
	
	entity.ig1_salescost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_salesfactor.value)*parseFloat(luExtend)*parseFloat(associatedCostData.attributes.ig1_saleslaborrate.value)).toFixed(4));
	entity.ig1_designhours = parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend);
	entity.ig1_saleshours = parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend);
	
	
	entity.ig1_materialcost = Number(parseFloat(materialCost).toFixed(4));
	if(associatedCostData.attributes.ig1_margin.value != undefined && associatedCostData.attributes.ig1_margin.value != 0)
	entity.ig1_totalmaterialcost = Number(parseFloat(materialCost).toFixed(4)/parseFloat(associatedCostData.attributes.ig1_margin.value).toFixed(4));
	else
	entity.ig1_totalmaterialcost  = entity.ig1_materialcost;
	entity.ig1_totalcost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_totalmaterialcost.value)+parseFloat(entity.ig1_designcost)+parseFloat(entity.ig1_salescost)+parseFloat(associatedCostData.attributes.ig1_travelcost.value)+parseFloat(entity.ig1_freight)+parseFloat(associatedCostData.attributes.ig1_pmlaborsme.value)).toFixed(4));
	entity.ig1_totalprojecthours = parseFloat(entity.ig1_saleshours) + parseFloat(entity.ig1_designhours) + parseFloat(entity.ig1_luextend);
	
    //Update corresponding fields related to luextend and freighttotal
	var req = new XMLHttpRequest();
	req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+recordId+")", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) {
				calculateTotalCostAndHour();
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}


//Nazish - Function added to calculate PM Labor/SME
function pmLaborSme(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var laborRate=row.attributes.get("ig1_laborrate").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var pmLaborSme=parseFloat(laborRate)*parseFloat(luExtend);
	row.attributes.get("ig1_pmlaborsme").setValue(pmLaborSme);
	calculateCategoryTotalCost(context);
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
}
//Nazish : Change Design related on design factor change
function calculateDesignCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var designFactor=row.attributes.get("ig1_designfactor").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var designLaborRate=row.attributes.get("ig1_designlaborrate").getValue();
	
	var designCost=parseFloat(designFactor)*parseFloat(designLaborRate)*parseFloat(luExtend);
	var designhours = parseFloat(designFactor)*parseFloat(luExtend);
	
	
	var saleshours=row.attributes.get("ig1_saleshours").getValue();
	var labourhours=row.attributes.get("ig1_labourhours").getValue();
	
	var totalProjectHours = parseFloat(saleshours) + parseFloat(designhours) + parseFloat(labourhours) ;
	
	row.attributes.get("ig1_designcost").setValue(designCost);
	row.attributes.get("ig1_designhours").setValue(designhours);
	row.attributes.get("ig1_totalprojecthours").setValue(totalProjectHours);
	
	//Calculate Total Cost basis the change
	calculateCategoryTotalCost(context);
	
	
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
	
}

//Nazish : Update Associated record entity when cell is changed
function UpdateAssociatedCostRecordOnCellChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var entity = {};
	
	entity.ig1_laborrate = parseFloat(row.attributes.get("ig1_laborrate").getValue());;
	
	entity.ig1_pmlaborsme = parseFloat(row.attributes.get("ig1_pmlaborsme").getValue());
	
	entity.ig1_designfactor = parseFloat(row.attributes.get("ig1_designfactor").getValue());	
	entity.ig1_designlaborrate = parseFloat(row.attributes.get("ig1_designlaborrate").getValue());
	
	entity.ig1_salesfactor = parseFloat(row.attributes.get("ig1_salesfactor").getValue());
	entity.ig1_saleslaborrate = parseFloat(row.attributes.get("ig1_saleslaborrate").getValue());
	
	entity.ig1_airfaretrans = parseFloat(row.attributes.get("ig1_airfaretrans").getValue());
	entity.ig1_perdiem = parseFloat(row.attributes.get("ig1_perdiem").getValue());
	entity.ig1_days = parseFloat(row.attributes.get("ig1_days").getValue());
	
	entity.ig1_totalmaterialcost = parseFloat(row.attributes.get("ig1_totalmaterialcost").getValue());
	entity.ig1_travelcost = parseFloat(row.attributes.get("ig1_travelcost").getValue());
	entity.ig1_designcost = parseFloat(row.attributes.get("ig1_designcost").getValue());	
	entity.ig1_salescost = parseFloat(row.attributes.get("ig1_salescost").getValue());
	entity.ig1_totalcost = parseFloat(row.attributes.get("ig1_totalcost").getValue());
	
	entity.ig1_saleshours =parseFloat(row.attributes.get("ig1_saleshours").getValue());
	entity.ig1_designhours =parseFloat(row.attributes.get("ig1_designhours").getValue());
	entity.ig1_totalprojecthours = parseFloat(row.attributes.get("ig1_totalprojecthours").getValue());
	


	var req = new XMLHttpRequest();
	req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+ recordId +")", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204)
				{
				//Updating Total Cost of BidSheet
	           calculateTotalCostAndHour();
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}

//Nazish : Change Sales related on sales factor change
function calculateSalesCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var salesFactor=row.attributes.get("ig1_salesfactor").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var salesLaborRate=row.attributes.get("ig1_saleslaborrate").getValue();
	var salesCost=parseFloat(salesFactor)*parseFloat(salesLaborRate)*parseFloat(luExtend);
	var saleshours = parseFloat(salesFactor)*parseFloat(luExtend);
	row.attributes.get("ig1_salescost").setValue(salesCost);
	row.attributes.get("ig1_saleshours").setValue(saleshours);
	
	
	var designhours=row.attributes.get("ig1_designhours").getValue();
	var labourhours=row.attributes.get("ig1_labourhours").getValue();
	
	var totalProjectHours = parseFloat(saleshours) + parseFloat(designhours) + parseFloat(labourhours) ;
	row.attributes.get("ig1_totalprojecthours").setValue(totalProjectHours);
	
	//Calculate Total Cost basis the change
	calculateCategoryTotalCost(context);
	
	
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
	
}

//Nazish - Function added to calculate total materialCost for associatedCost.
function calculateTotalMaterialCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var materialCost=row.attributes.get("ig1_materialcost").getValue();
	var margin = row.attributes.get("ig1_margin").getValue();
	var designCost=row.attributes.get("ig1_designcost").getValue();
	var salesCost=row.attributes.get("ig1_salescost").getValue();
	var travelCost=row.attributes.get("ig1_travelcost").getValue();
	var totalMaterialCost=parseFloat(materialCost)/parseFloat(margin);
	var totalCost=totalMaterialCost+parseFloat(designCost)+parseFloat(salesCost)+parseFloat(travelCost);

	row.attributes.get("ig1_totalmaterialcost").setValue(totalMaterialCost);
	//Calculate Total Cost basis the change
	calculateCategoryTotalCost(context);
	
	
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
}

function calculateTravelCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var airFare=row.attributes.get("ig1_airfaretrans").getValue();
	var perDiem = row.attributes.get("ig1_perdiem").getValue();
	var days=row.attributes.get("ig1_days").getValue();
	var travelCost=parseFloat(airFare)+(parseFloat(perDiem)*parseFloat(days));
	row.attributes.get("ig1_travelcost").setValue(travelCost);
	
	//Calculate Total Cost basis the change
	calculateCategoryTotalCost(context);
	
	
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
}
function calculateCategoryTotalCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var designCost=row.attributes.get("ig1_designcost").getValue();
	var salesCost=row.attributes.get("ig1_salescost").getValue();
	var travelCost=row.attributes.get("ig1_travelcost").getValue();
	var totalMaterialCost=row.attributes.get("ig1_totalmaterialcost").getValue();
	var labourCost=row.attributes.get("ig1_pmlaborsme").getValue();
	var freightCost=row.attributes.get("ig1_freight").getValue();
	var CategoryTotalCost=parseFloat(designCost)+parseFloat(salesCost)+parseFloat(travelCost)+parseFloat(totalMaterialCost)+parseFloat(labourCost)+parseFloat(freightCost);
	row.attributes.get("ig1_totalcost").setValue(CategoryTotalCost);
}

function SummaryCategoryTotalCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var margin=row.attributes.get("ig1_margin").getValue();
	var ig1_designcost ;
	var ig1_freight ;
	var ig1_materialcost ;
	var ig1_pmlaborsme;
	var ig1_salescost;
	var ig1_travelcost;
	
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+recordId+")?$select=ig1_designcost,ig1_freight,ig1_materialcost,ig1_pmlaborsme,ig1_salescost,ig1_travelcost", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 200) {
				var result = JSON.parse(this.response);
				 ig1_designcost = result["ig1_designcost"];
				
				 ig1_freight = result["ig1_freight"];
				
				 ig1_materialcost = result["ig1_materialcost"];
				
				 ig1_pmlaborsme = result["ig1_pmlaborsme"];
				
				 ig1_salescost = result["ig1_salescost"];
				
				 ig1_travelcost = result["ig1_travelcost"];
				
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();
	
	
	if(margin !== 0)
	{
	var totalMaterialCost=parseFloat(row.attributes.get("ig1_materialcost").getValue()) / parseFloat(margin);
	}
	
	var totalCost = parseFloat(ig1_designcost) + parseFloat(ig1_freight) + parseFloat(totalMaterialCost) + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost);
	
	row.attributes.get("ig1_totalcost").setValue(totalCost);
	
	
	
	var entity = {};
	
	
	entity.ig1_totalmaterialcost = parseFloat(totalMaterialCost);
	
	entity.ig1_totalcost = parseFloat(totalCost);
	
	entity.ig1_margin =parseFloat(margin);
	
	var req = new XMLHttpRequest();
	req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+ recordId +")", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204)
				{
				//Updating Total Cost of BidSheet
	           calculateTotalCostAndHour();
			  
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
	
	
}

function calculateTotalCostAndHour()
{
  var bidSheetId=Xrm.Page.data.entity.getId()
  var fetchData = {
	   ig1_bidsheet: bidSheetId
	};
	var fetchXml = [
					"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
					"  <entity name='ig1_associatedcost'>",					
					"    <attribute name='ig1_totalcost' />",
					"    <attribute name='ig1_totalprojecthours' />",
					"    <attribute name='ig1_bidsheetcategory' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
		
			var fetchXmlData=XrmServiceToolkit.Soap.Fetch(fetchXml);
                       if(fetchXmlData.length>0)
		{
			
			var	totalCost=parseFloat(fetchXmlData[0].attributes.ig1_totalcost.value);
			var projectHour=parseFloat(fetchXmlData[0].attributes.ig1_totalprojecthours.value);
			
			for(i=1; i<fetchXmlData.length; i++)
			{
				result=fetchXmlData[i].attributes;
				totalCost+=parseFloat(result.ig1_totalcost.value);				
				projectHour+=parseFloat(result.ig1_totalprojecthours.value);
			}
			
            Xrm.Page.getAttribute("ig1_totalhours").setValue(projectHour);
			Xrm.Page.getAttribute("ig1_totalcost").setValue(totalCost);
		}
                else
		{
			Xrm.Page.getAttribute("ig1_totalcost").setValue(0);
			Xrm.Page.getAttribute("ig1_totalhours").setValue(0);
		}
}
//Nazish - Function added to make disable field in the associatedCost and summary grid
function disbaleAssociatedCostAndSummaryGridCells(context)
{
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			|| field._attributeName ==="ig1_designcost" 
			|| field._attributeName ==="ig1_freight" 
			|| field._attributeName==="ig1_materialcost" 
			|| field._attributeName === "ig1_salescost" 
			|| field._attributeName==="ig1_travelcost" 
			|| field._attributeName==="ig1_luextend" 
			|| field._attributeName==="ig1_pmlaborsme" 
			|| field._attributeName==="ig1_totalmaterialcost" 
            || field._attributeName==="ig1_totalcost")			
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_bidsheetcategory" 
			|| field.getName()==="ig1_designcost" 
			|| field.getName()==="ig1_freight" 
			|| field.getName()==="ig1_materialcost"
			|| field.getName()=== "ig1_salescost" 
			|| field.getName()==="ig1_travelcost" 
			|| field.getName()==="ig1_luextend" 
			|| field.getName()==="ig1_pmlaborsme" 
			|| field.getName()==="ig1_totalmaterialcost" 
			|| field.getName()==="ig1_totalcost") 
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

function disbaleAssociatedCategoryGridCells(context)
{
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_category" 
			|| field._attributeName==="ig1_vendor")			
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_category" 
			|| field.getName()==="ig1_vendor") 
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}
