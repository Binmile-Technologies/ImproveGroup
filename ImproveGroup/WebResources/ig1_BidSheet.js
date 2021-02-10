function onLoad(executionContext)
{
    debugger;
    var formContext = executionContext.getFormContext();
    var isBSCalculated = formContext.getAttribute("ig1_isbidsheetcalculated").getValue();
    if(!isBSCalculated)
    {
        formContext.ui.setFormNotification('Please click on "Calculate Indirect Cost" to see updated costs', "WARNING",);
    }
    var status=formContext.getAttribute("ig1_status").getValue();
    var associate= formContext.getAttribute("ig1_associated").getValue();
    if(status!=286150001 && status!=286150003)
    {
        var controls = formContext.ui.controls.get();
        for (var i in controls) 
        {
            var control = controls[i];
            if (control.getDisabled && control.setDisabled && !control.getDisabled()) 
            {
                control.setDisabled(true);
            }
        }
    }

    if(status==286150002 && associate==true){

        var  associatecontrol= formContext.ui.controls.get("header_ig1_associated");
         
        associatecontrol.setDisabled(false);
    }
}

function reviseBidSheet(formContext)
{
    debugger;
    Xrm.Utility.showProgressIndicator("Processing...");
    var opportunityTitle = formContext.getAttribute("ig1_opportunitytitle").getValue();
    var oppStatusOpen = isOpportunityOpen(opportunityTitle);
    if(!oppStatusOpen)
    {
        alert("Can't Revise BidSheet as Opportunity is Closed");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    var maxRevId=reviseBidSheet_maxUpperRevision();
    var UppRevId=formContext.getAttribute("ig1_upperrevisionid").getValue();
    if(maxRevId>UppRevId)
    {
        alert("Bid Sheet of revision id "+ maxRevId +".0 or above can only be revised");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
 
    var status=formContext.getAttribute("ig1_status").getValue();
    if(status==286150001)
    {
        alert("Can't be revise as bid sheet have draft status");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    var currentRecordIdString = formContext.data.entity.getId();
    var bidSheetId = currentRecordIdString.replace("{", '').replace("}", '');

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheets("+bidSheetId+")/Microsoft.Dynamics.CRM.ig1_ActionReviseBidSheet", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function() {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                deleteOpportunityProduct(formContext);
                delete_PriceListItems(formContext);
                Xrm.Page.data.refresh();
                setTimeout(function()
                {
                    Xrm.Page.ui.refreshRibbon();
                    Xrm.Utility.closeProgressIndicator();
                },2000);
            } else {
                Xrm.Utility.alertDialog(this.statusText);
                Xrm.Utility.closeProgressIndicator();
            }
        }
    };
    req.send();
}

function deleteOpportunityProduct(formContext)
{
	debugger;
    var opportunityId="";
    var opportunity=formContext.getAttribute("ig1_opportunitytitle").getValue();
    if(opportunity!=undefined && opportunity !=null && opportunity!="")
    {
        opportunityId = opportunity[0].id.replace("{", "").replace("}", "");
    }
    else 
    {
        return;
    }
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunityproducts?$select=opportunityproductid&$filter=_opportunityid_value eq "+opportunityId, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function() {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    var opportunityproductid = results.value[i]["opportunityproductid"];
                    deleteOppLineItem(opportunityproductid)
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

function deleteOppLineItem(opportunityproductid)
{
    var req = new XMLHttpRequest();
    req.open("DELETE", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunityproducts("+opportunityproductid+")", false);
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.onreadystatechange = function() {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204 || this.status === 1223) {
                //Success - No Return Data - Do Something
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}
//Nazish - 25-12-2019 - Function added to get upper max revision id
function reviseBidSheet_maxUpperRevision()
{
    var opportunityId="";
    var opportunity = Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
    if(opportunity!=undefined && opportunity!=null && opportunity!="")
    {
        opportunityId = opportunity[0].id.replace("{", "").replace("}", "");
    }
    else
    {
        alert("Can not revise Bid Sheet as opportunity does not exist");
        return;
    }
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
        for(upmr=0; upmr<fetchRecord.length; upmr++)
        {
            if(fetchRecord[upmr].attributes.ig1_upperrevisionid.value>maxUpperRevision)
            {
                maxUpperRevision=fetchRecord[upmr].attributes.ig1_upperrevisionid.value;
            }
        }
    }
    return maxUpperRevision;
}

function updateStatus()
{
    alert("Updated");
}

function delete_PriceListItems(formContext)
{
	debugger;
    var priceListid = "";
	var pricelist = formContext.getAttribute("ig1_pricelist").getValue();
    if(pricelist==undefined || pricelist==null || pricelist=="")
    {
        return;
    }
	priceListid = pricelist[0].id.replace("{", "").replace("}", "");
	
    var fetchData = {
        pricelevelid: priceListid
    };
    var fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='productpricelevel'>",
					"    <attribute name='productpricelevelid' />",
					"    <filter type='and'>",
					"      <condition attribute='pricelevelid' operator='eq' value='", fetchData.pricelevelid/*30a31f6f-7a31-417b-9d15-dcd1b26f0344*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
					
    var productPriceLevel = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(productPriceLevel!=undefined && productPriceLevel!=null && productPriceLevel!="" && productPriceLevel.length>0)
    {
        for(ppl=0; ppl < productPriceLevel.length; ppl++)
        {
            var result = productPriceLevel[ppl].attributes;
            if(result.productpricelevelid!=undefined && result.productpricelevelid!=null && result.productpricelevelid!="")
            {
                var id = result.productpricelevelid.value.replace("{", "").replace("}", "");
                var req = new XMLHttpRequest();
                req.open("DELETE", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/productpricelevels("+id+")", false);
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.onreadystatechange = function() {
                    if (this.readyState === 4) {
                        req.onreadystatechange = null;
                        if (this.status === 204 || this.status === 1223) {
                            //Success - No Return Data - Do Something
                        } else {
                            Xrm.Utility.alertDialog(this.statusText);
                        }
                    }
                };
                req.send();
            }
        }
    }
}

function activateBidSheet(formContext)
{     debugger;
    Xrm.Utility.showProgressIndicator("Processing...");     
    var opportunityTitle=formContext.getAttribute("ig1_opportunitytitle").getValue();
    if(opportunityTitle=="" || opportunityTitle==null || opportunityTitle==undefined)
    {
        alert("Please select Opportunity Title before activating the Bid Sheet");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    var oppStatusOpen = isOpportunityOpen(opportunityTitle);
    if(!oppStatusOpen)
    {
        alert("Can't activate BidSheet as Opportunity is Closed");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    var isExist=isLineItemsExist();
    if(!isExist)
    {
        alert("Please Add Bid Sheet Line Item(s) before Activating the Bid Sheet!");
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    var bidsheetid = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var retiredProducts = checkRetiredProducts(bidsheetid)
    if(retiredProducts!="")
    {
        alert("One or more products are retired and can not be activated. The retired product(s) are:\n"+retiredProducts);
        Xrm.Utility.closeProgressIndicator();
        return;   
    }
	   
    var isBidsheetCalculated = formContext.getAttribute("ig1_isbidsheetcalculated").getValue();
    if(!isBidsheetCalculated)
    {
        alert('Please click on "Calculate Indirect Cost" button to update the costs before activation of Bid Sheet');
        Xrm.Utility.closeProgressIndicator();
        return;
    }
    deleteOpportunityProduct(formContext);
    delete_PriceListItems(formContext);
    createOpportunityLine();
    Xrm.Page.ui.refreshRibbon();
    setTimeout(function(){Xrm.Utility.closeProgressIndicator();}, 5000);
}

function checkRetiredProducts(bidsheetid)
{
	
    var fetchData = {
        ig1_bidsheet: bidsheetid,
        statecode: "1",
        statecode2: "2",
        statecode3: "3"
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheetpricelistitem'>",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*d02e6d25-d104-ea11-a811-000d3a55dce2*/, "'/>",
					"    </filter>",
					"    <link-entity name='product' from='productid' to='ig1_product'>",
					"      <attribute name='name' alias='productName'/>",
					"      <filter type='or'>",
					"        <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*1*/, "'/>",
					"        <condition attribute='statecode' operator='eq' value='", fetchData.statecode2/*2*/, "'/>",
					"        <condition attribute='statecode' operator='eq' value='", fetchData.statecode3/*3*/, "'/>",
					"      </filter>",
					"    </link-entity>",
					"  </entity>",
					"</fetch>",
    ].join("");
    var result = executeFetchXml("ig1_bidsheetpricelistitems", fetchXml);
    var retiredProducts = "";
    if(result.value!=undefined && result.value!=null && result.value!="" && result.value.length>0)
    {
        for(i=0; i<result.value.length; i++)
        {
            if(result.value[i].productName!=undefined && result.value[i].productName!=null && result.value[i].productName!="")
            {
                retiredProducts+="\n"+result.value[i].productName;
            }
        }
    }
    return retiredProducts;
}

function activatedBSExist(opportunityId)
{
    debugger;
    var fetchData = {
        ig1_status: "286150000",
        ig1_opportunitytitle: opportunityId
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_bidsheetid' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150000*/, "'/>",
					"      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*00000000-0000-0000-0000-000000000000*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
    var result = executeFetchXml("ig1_bidsheets", fetchXml);
    if(result.value!=undefined && result.value!=null && result.value!="" && result.value.length>0)
    {
        return true;
    }
    else
    {
        return false;
    }
}

function isOpportunityOpen(opportunityTitle)
{
    var oppStatus = false;
    if(opportunityTitle==undefined || opportunityTitle==null || opportunityTitle=="")
    {
        return;
    }
    var opporunityId = opportunityTitle[0].id.replace("{", "").replace("}", "");
    var fetchData = {
        statecode: "0",
        opportunityid: opporunityId
    };
    var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='opportunity'>",
                    "    <attribute name='name' />",
                    "    <filter type='and'>",
                    "      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
                    "      <condition attribute='opportunityid' operator='eq' value='", fetchData.opportunityid/*b5af3c1f-4b43-ea11-a812-000d3a55d933*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
    ].join("");
     
    var oppStatusData = XrmServiceToolkit.Soap.Fetch(fetchXml);     
    if(oppStatusData!=undefined && oppStatusData!=null && oppStatusData!="")
    {
        if(oppStatusData.length>0)
        {
            oppStatus = true; 
        }
    }
    return oppStatus;
}

function closeBidSheet()
{
    alert("Closed");
    Xrm.Page.ui.refreshRibbon();
}



function checkBPF(executionContext)
{
   var formContext = executionContext.getFormContext();
    var selectedStage = formContext.data.process.getSelectedStage();
    var activeStage = formContext.data.process.getActiveStage();
    //var activeStageId = activeStage.getId();
    var activeStagename = activeStage.getName();
    var selectedStagename = selectedStage.getName();

    if( activeStagename == "Bid Sheet Setup" || selectedStagename =="Bid Sheet Setup" )
    {
        formContext.ui.tabs.get("Bid Sheet Setup").setVisible(true);
        formContext.ui.tabs.get("Product Management").setVisible(false);
        formContext.ui.tabs.get("Upload BOMs").setVisible(false);
        formContext.ui.tabs.get("Create BOM").setVisible(false);
        formContext.ui.tabs.get("Associated Cost").setVisible(false);
        formContext.ui.tabs.get("Summary").setVisible(false);
        // formContext.ui.tabs.get("Reports").setVisible(false);
        formContext.ui.tabs.get("Contingency").setVisible(false);
        formContext.ui.tabs.get("Display Message").setVisible(false);
        if(activeStagename == "Bid Sheet Setup")
        {
            var hasReadOnlyPermission=getCurrentUserRoles();
            var flag=uploadPartsStage();
            if(flag)
            {
                formContext.getControl("ig1_uploadpartslist").setDisabled(true);
            }
            else
            {
                if(!hasReadOnlyPermission)
                    setTimeout(function(){Xrm.Page.getControl("ig1_uploadpartslist").setDisabled(false)}, 2000); 
            }
        }
        else
        {
            formContext.getControl("ig1_uploadpartslist").setDisabled(true);
        }
    }
    else if ( activeStagename == "Upload Parts" || selectedStagename== "Upload Parts")
    {
        formContext.ui.tabs.get("Bid Sheet Setup").setVisible(false);
        formContext.ui.tabs.get("Create BOM").setVisible(false);
        formContext.ui.tabs.get("Associated Cost").setVisible(false);
        formContext.ui.tabs.get("Summary").setVisible(false);
        //formContext.ui.tabs.get("Reports").setVisible(false);
        formContext.ui.tabs.get("Contingency").setVisible(false);
        var isReadPermission=getCurrentUserRoles();
        if(isReadPermission)
        {
            formContext.ui.tabs.get("Product Management").setVisible(false);
            formContext.ui.tabs.get("Upload BOMs").setVisible(false);
            formContext.ui.tabs.get("Display Message").setVisible(true);
        }
        else
        {
            formContext.ui.tabs.get("Product Management").setVisible(true);
            formContext.ui.tabs.get("Upload BOMs").setVisible(true);
            formContext.ui.tabs.get("Display Message").setVisible(false);
        }
        listAllProductGroups();
    }
    else if ( activeStagename == "Products & Services" || selectedStagename== "Products & Services")
    {
        formContext.ui.tabs.get("Bid Sheet Setup").setVisible(false);
        formContext.ui.tabs.get("Product Management").setVisible(false);
        formContext.ui.tabs.get("Upload BOMs").setVisible(false);
        formContext.ui.tabs.get("Create BOM").setVisible(true);
        formContext.ui.tabs.get("Associated Cost").setVisible(false);
        formContext.ui.tabs.get("Summary").setVisible(false);
        formContext.ui.tabs.get("Display Message").setVisible(false);
        formContext.getControl("ig1_uploadpartslist").setDisabled(true);
        formContext.ui.tabs.get("Contingency").setVisible(false);
        //deleteAssociatedCostRecord();
        //createPriceListItems();
        //createUpdatePMLabor();
        //calculateBOMBidSheetTotals();
        //createUpdateAssociatedCost();
        // setTimeout(function()
        // {
        // calculateSDT();
        // },3000);
    }
    else if ( activeStagename == "Indirect Cost" || selectedStagename =="Indirect Cost")
    {
        formContext.ui.tabs.get("Bid Sheet Setup").setVisible(false);
        formContext.ui.tabs.get("Product Management").setVisible(false);
        formContext.ui.tabs.get("Upload BOMs").setVisible(false);
        formContext.ui.tabs.get("Create BOM").setVisible(false);
        formContext.ui.tabs.get("Summary").setVisible(false);
        //formContext.ui.tabs.get("Reports").setVisible(false);
        formContext.ui.tabs.get("Contingency").setVisible(false);
        var isReadPermission=getCurrentUserRoles();
        if(isReadPermission)
        {
            formContext.ui.tabs.get("Associated Cost").setVisible(false);
            formContext.ui.tabs.get("Display Message").setVisible(true);
        }
        else
        {
            formContext.ui.tabs.get("Associated Cost").setVisible(true);
            formContext.ui.tabs.get("Display Message").setVisible(false);
        }
        formContext.data.refresh();
        //deleteAssociatedCostRecord();
        //createUpdateAssociatedCost();
        // calculateSDT();
    }
    else if ( activeStagename == "Summary" || selectedStagename=="Summary")
    {
        formContext.ui.tabs.get("Bid Sheet Setup").setVisible(false);
        formContext.ui.tabs.get("Product Management").setVisible(false);
        formContext.ui.tabs.get("Upload BOMs").setVisible(false);
        formContext.ui.tabs.get("Create BOM").setVisible(false);
        formContext.ui.tabs.get("Associated Cost").setVisible(false);
        formContext.ui.tabs.get("Summary").setVisible(true);
        //formContext.ui.tabs.get("Reports").setVisible(false);
        formContext.getControl("ig1_uploadpartslist").setDisabled(true);
        formContext.ui.tabs.get("Display Message").setVisible(false);
        deleteAssociatedCostRecord();
        contingencyExist=isContingencyExist();
        if(contingencyExist)
        {
            formContext.ui.tabs.get("Contingency").setVisible(true);
        }
        else
        {
            formContext.ui.tabs.get("Contingency").setVisible(false);
        }
        //createUpdateAssociatedCost();
        //calculateSDT();
        //projectFinanceProjection();
    }
 
    formContext.ui.refreshRibbon();
}
 
function showHideTabs(executionContext)
{
    var formContext = executionContext.getFormContext();
    formContext.data.process.addOnStageSelected(checkBPF);
    formContext.data.process.addOnStageChange(checkBPF);	
    checkBPF(executionContext);
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
                                        "      <condition attribute='statecode' operator='eq' value='0'/>",
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
                                        "      <condition attribute='statecode' operator='eq' value='0'/>"+
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






//Nazish - Creating Opportunity Line Items
function createOpportunityLine()
{
    var opportunityId='';
    var opportunityTitle=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
    if(opportunityTitle!='' && opportunityTitle!= null && opportunityTitle!= undefined)
    {
        opportunityId=opportunityTitle[0].id.replace('{','').replace('}','');
    }
    var bidSheetId=Xrm.Page.data.entity.getId().replace('{','').replace('}','');
    var parameters = {};
    parameters.bidSheetId = bidSheetId;

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_CreateOpportunityLine", false);
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
                activate_updateBidSheetStatus(opportunityId, bidSheetId);
                Xrm.Page.data.save();
                setTimeout(function()
                {
                    Xrm.Page.data.refresh(false);
                },2000);
            } 
            else 
            {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(parameters));
}

//Nazish - 12-23-2019 - Function added to update the BidSheetStatus and associated field
function activate_updateBidSheetStatus(opportunityId, bidSheetId)
{
    debugger;
    if(opportunityId==undefined || opportunityId==null || opportunityId=="" ||bidSheetId==undefined || bidSheetId==null || bidSheetId=="")
        return;
    var ig1_iscreateorder = Xrm.Page.getAttribute("ig1_iscreateorder").getValue();
    Xrm.Page.getAttribute("ig1_status").setValue(286150000);
    Xrm.Page.getAttribute("ig1_associated").setValue(true);
    Xrm.WebApi.online.retrieveMultipleRecords("ig1_bidsheet", "?$select=ig1_associated,ig1_bidsheetid,ig1_status&$filter=_ig1_opportunitytitle_value eq "+opportunityId+" and  ig1_bidsheetid ne "+bidSheetId).then(
    function success(results) {
        for (var i = 0; i < results.entities.length; i++) 
        {
            var ig1_bidsheetid = results.entities[i]["ig1_bidsheetid"];
            var ig1_status = results.entities[i]["ig1_status"];
            var ig1_associated = results.entities[i]["ig1_associated"];
            
            if(ig1_status==286150000  && ig1_iscreateorder==false){

                var entity = {};
                entity.ig1_status = 286150002;
                entity.ig1_associated=false;
                Xrm.WebApi.online.updateRecord("ig1_bidsheet", ig1_bidsheetid, entity).then(
                    function success(result) {
                        var updatedEntityId = result.id;
                    },
                    function(error) {
                        Xrm.Utility.alertDialog(error.message);
                    }
                );
            }

            else if(ig1_status==286150000 && ig1_iscreateorder==true){
                var entity = {};
                entity.ig1_status = 286150002;
                entity.ig1_associated=true;
                Xrm.WebApi.online.updateRecord("ig1_bidsheet", ig1_bidsheetid, entity).then(
                    function success(result) {
                        var updatedEntityId = result.id;
                    },
                    function(error) {
                        Xrm.Utility.alertDialog(error.message);
                    }
                );

            }
            else if(ig1_status==286150002 && ig1_associated==true){
                var entity = {};
                entity.ig1_status = 286150002;
                entity.ig1_associated=true;
                Xrm.WebApi.online.updateRecord("ig1_bidsheet", ig1_bidsheetid, entity).then(
                    function success(result) {
                        var updatedEntityId = result.id;
                    },
                    function(error) {
                        Xrm.Utility.alertDialog(error.message);
                    }
                );

            }
            else{

                var entity = {};
                entity.ig1_status = 286150002;
                Xrm.WebApi.online.updateRecord("ig1_bidsheet", ig1_bidsheetid, entity).then(
                   function success(result) {
                       var updatedEntityId = result.id;
                   },
                   function(error) {
                       Xrm.Utility.alertDialog(error.message);
                   }
               );
            }
			
        }
    },
    function(error) {
        Xrm.Utility.alertDialog(error.message);
    }
  );
}

//Nazish - Function is added to make the read only cells into the editable subgrid.
function disableEditableGridCells(context)
{
    var formContext = context.getFormContext().data.entity;
    formContext.attributes.forEach(function(field, i) 
    {
        if ((field._attributeName !=="ig1_unitprice" && field._attributeName !=="ig1_quantity" && field._attributeName!==undefined) || (field.getName()!=="ig1_unitprice" && field.getName()!=="ig1_quantity" && field.getName()!==undefined) || bidSheetStatus!="286150001") 
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







//Calculate Material Cost basis unit price and quantity
function SyncMaterialCostLineItem()
{
    var control = context.getEventSource();
    var row= control.getParent();
    var materialCost=row.attributes.get("ig1_materialcost").getValue();
	 
    if(materialCost=='' || materialCost==null || materialCost==undefined)
    {
        materialCost=0;
        var quantity=row.attributes.get("ig1_quantity").getValue();
        if(quantity=='' || quantity==null || quantity==undefined)
        {
            quantity=0;
        }
	 
        var unitprice=row.attributes.get("ig1_unitprice").getValue();
        if(unitprice=='' || unitprice==null || unitprice==undefined) 
        {
            unitprice=0;
        }
        var materialCost=parseFloat(quantity)*parseFloat(unitprice);
        row.attributes.get("ig1_materialcost").setValue(materialCost);;
    }
    else
    {
        row.attributes.get("ig1_quantity").setValue(1);;
        row.attributes.get("ig1_unitprice").setValue(materialCost);;
		
		
    }
	 
}


//


//Nazih - Function added to make all non-numeric fields, materialCost, LuExtend and freight total field as read only
function disbaleEditableGridCellsOnCreateBOM(context)
{
    var formContext = context.getFormContext().data.entity;
    var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();

    formContext.attributes.forEach(function(field, i) 
    {
        if((field._attributeName ==="ig1_category" 
        || field._attributeName ==="ig1_product" 
        || field._attributeName==="ig1_materialcost" 
        || field._attributeName==="ig1_luextend" 
        || field._attributeName === "ig1_freighttotal" 
        || field._attributeName === "ig1_projecthours"
        || field._attributeName === "ig1_sdt"			
        || field._attributeName==="ig1_totalamount")
        || (bidSheetStatus!="286150001" && bidSheetStatus != "286150003")
        &&field._attributeName!==undefined 
        ||(field.getName()==="ig1_category" 
        || field.getName()==="ig1_product" 
        || field.getName()==="ig1_materialcost"
        || field.getName()==="ig1_luextend"
        || field.getName()=== "ig1_freighttotal" 
        || field.getName()=== "ig1_projecthours" 
        || field.getName()=== "ig1_sdt" 
        || field.getName()==="ig1_totalamount")
        || (bidSheetStatus!="286150001" && bidSheetStatus != "286150003")
        && field.getName()!==undefined) 
        {
            field.controls.get(0).setDisabled(true);
        }
    });
}




//Padmaja Update BidsheetLineItem with associatedcostid 
function UpdateAssociatedRecordId(categoryId)
{
    try
    {
        bidsheetId = Xrm.Page.data.entity.getId().replace("{","").replace("}","");
        var fetchXml = [
                         "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
                         "  <entity name='ig1_bidsheetpricelistitem'>",
					 
                         "    <filter type='and'>",
                         "      <condition attribute='ig1_bidsheet' operator='eq' value='", bidsheetId, "'/>",
                         "      <condition attribute='ig1_category' operator='eq' value='", categoryId, "'/>",
                         "    </filter>",
                         "  </entity>",
                         "</fetch>",
        ].join("");
					
					
        var fetchXmlAssociatedCost = [
                         "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
                         "  <entity name='ig1_associatedcost'>",
                         " <attribute name='ig1_associatedcostid' />",
                         "    <filter type='and'>",
                         "      <condition attribute='ig1_bidsheet' operator='eq' value='", bidsheetId, "'/>",
                         "      <condition attribute='ig1_bidsheetcategory' operator='eq' value='", categoryId, "'/>",
                         "    </filter>",
                         "  </entity>",
                         "</fetch>",
        ].join("");
					
        var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
        var fetchDataAssociatedCost =XrmServiceToolkit.Soap.Fetch(fetchXmlAssociatedCost);
        var associatedCostId = fetchDataAssociatedCost[0].id;
	
        if(fetchData!='' && fetchData!=null && fetchData!=undefined)
        {
            for(i=0; i<fetchData.length; i++)
            {
	
                var entity = {};
                // entity["ig1_BidSheet@odata.bind"] = "/ig1_bidsheets("+bidSheetId+")";
                entity["ig1_AssociatedCostId@odata.bind"] = "/ig1_associatedcosts("+associatedCostId+")";
				

                var req = new XMLHttpRequest();
                req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+fetchData[i].id+")", false);
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
                req.send(JSON.stringify(entity));
            }
        }
    }
    catch(err)
    {}
	
}

function AddFilterInContigencyTab(context)
{
    var formContext = context.getFormContext();
    var customerAccountFilter = "<filter type='and'><condition attribute='ig1_category' operator='eq' value=''/></filter>";
    formContext.getControl("parentaccountid").addCustomFilter(customerAccountFilter, "account");
}






//Nazish - Function added to make disable field in the associatedCost and summary grid
function disbaleAssociatedCostAndSummaryGridCells(context)
{
    var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
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
                   || bidSheetStatus!="286150001"		
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
                    || bidSheetStatus!="286150001"
        && field.getName()!==undefined) 
        {
            field.controls.get(0).setDisabled(true);
        }
    });
}

function disbaleAssociatedCategoryGridCells(context)
{
    var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
    var formContext = context.getFormContext().data.entity;
    formContext.attributes.forEach(function(field, i) 
    {
        if((field._attributeName ==="ig1_category" 
        || field._attributeName==="ig1_vendor")
                    || bidSheetStatus!="286150001"			
        &&field._attributeName!==undefined 
        ||(field.getName()==="ig1_category" 
        || field.getName()==="ig1_vendor") 
                    || bidSheetStatus!="286150001"
        && field.getName()!==undefined) 
        {
            field.controls.get(0).setDisabled(true);
        }
    });
}

function switchBPF()
{
    Xrm.Page.getControl("ig1_uploadpartslist").setDisabled(true);
    setTimeout(function(){Xrm.Page.getControl("ig1_uploadpartslist").setDisabled(false);}, 2000);
}
//Nazish - Function Added to check if category subgrid contains data or not
function addEventToGridRefresh() 
{
    var grid = Xrm.Page.getControl('IG_BidSheetCategory');
    if (!grid) {
        setTimeout(function () { addEventToGridRefresh(); }, 2000);
        return;
    }
    ValidateSelectedStage();
    setTimeout(function(){Xrm.Page.getControl('IG_BidSheetCategory').addOnLoad(ValidateSelectedStage);}, 2000);
    setTimeout(function () 
    { 
        Xrm.Page.getControl("header_process_ig1_selectedcategory").setVisible(false);
        Xrm.Page.getControl("header_process_ig1_uploadpartslist").setVisible(false);
    }, 3000);
}


function ValidateSelectedStage() 
{
    var stageName = Xrm.Page.data.process.getSelectedStage().getName().toString().toLowerCase();
    if (stageName == "bid sheet setup") 
    { 
        var bidSheetId=Xrm.Page.data.entity.getId();
		  
        var fetchData = {
            ig1_bidsheet: bidSheetId,
            statecode: "0"
        };
        var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bscategoryvendor'>",
					"    <attribute name='ig1_name' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
                                       "      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
        ].join("");
        var categoryData=XrmServiceToolkit.Soap.Fetch(fetchXml);
        if(categoryData!="" && categoryData!=null && categoryData!=undefined && categoryData.length>0)
        {
            Xrm.Page.getControl("header_process_ig1_selectedcategory").getAttribute().setValue(286150000);
            Xrm.Page.ui.clearFormNotification("2001"); 
            //Xrm.Page.data.save();
        }
        else 
        {
            Xrm.Page.getControl("header_process_ig1_selectedcategory").getAttribute().setValue(null);
            Xrm.Page.ui.setFormNotification("Please add at least one category", "INFO", "2001");
            //Xrm.Page.data.save();
        }
    }
}
//Nazish - Function added to disable 
function uploadPartsStage()
{
    var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var fetchData = {
        ig1_bidsheet: bidSheetId,
        statecode: "0"
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheetproduct'>",
					"    <attribute name='ig1_bidsheetcategory' />",
					"    <attribute name='ig1_name' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
						
    var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(fetchData!="" && fetchXml!=null && fetchXml!=undefined && fetchData.length>0)
    {
        return true;
    }
    else
    {
        return false;
    }
}
//BOM Stage Calculations //
//This function created the Price List Items
function createPriceListItems()
{
    var bidSheetId = Xrm.Page.data.entity.getId();
    var opportunityId="";
    var opportunity=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
    if(opportunity!="" && opportunity!=null && opportunity!=undefined)
    {
        opportunityId = opportunity[0].id
    }
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
                createUpdatePMLabor();
                calculateBOMBidSheetTotals();
                Xrm.Utility.closeProgressIndicator();
                Xrm.Page.data.refresh(true);
            } else {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(parameters));
}


//This function calculates the total Material Cost / Freight Cost / Total Cost at BOM stage and updates the bidsheet fields
function calculateBOMBidSheetTotals()
{
    var bidSheetId=Xrm.Page.data.entity.getId();
    var material_Cost=0;
	
    var freight=0
    var totalBOMCost=0;
    var freightsell=0;
    var fetchXml = [
                      "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>",
                      "  <entity name='ig1_bidsheetpricelistitem'>",
						
                      "    <attribute name='ig1_materialcost' />",
						
                      "    <attribute name='ig1_projectlu' />",
						
                      "    <attribute name='ig1_freightamount' />",
                      "    <attribute name='ig1_freighttotal' />",
                      "    <attribute name='ig1_luextend' />",
                        
                      "    <attribute name='ig1_category' />",

                      "    <filter type='and'>",
                      "      <condition attribute='ig1_bidsheet' operator='eq' value='", bidSheetId , "'/>",
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
            if(result.ig1_category.formattedValue!="Contingency")
            {
                if(result.ig1_materialcost != null)
                    material_Cost+=parseFloat(result.ig1_materialcost.value);
                if(result.ig1_freightamount != null)
                    freight+=parseFloat(result.ig1_freightamount.value);
                if(result.ig1_freighttotal != null)
                    freightsell+=parseFloat(result.ig1_freighttotal.value);
            }
        }
    }
    totalBOMCost = material_Cost+freight;  
    Xrm.Page.getAttribute("ig1_materialcost").setValue(material_Cost);
    Xrm.Page.getAttribute("ig1_freightcost").setValue(freight);
    Xrm.Page.getAttribute("ig1_totalbomcost").setValue(totalBOMCost);
    Xrm.Page.getAttribute("ig1_freightsell").setValue(freightsell);
    //setTimeout(function(){Xrm.Page.data.save();}, 2000);
    // calculateSDT();
}



//Nazish - Function added to calculate freight total
function freightTotalCalculation(context)
{
    var control = context.getEventSource();
    var row= control.getParent();
    var freightamount=row.attributes.get("ig1_freightamount").getValue();
    var margin = row.attributes.get("ig1_markup").getValue();
    if(margin !=0 && (margin ==undefined || margin ==null || margin =="" || margin >= 100))
    {
        alert("Margin should not be empty and must be less than 100%");
        row.attributes.get("ig1_markup").setValue(parseFloat(30));
        margin = parseFloat(30);
    }
    //var freightTotal=(parseFloat(freightamount)/parseFloat(margin));
    var freightTotal=(parseFloat(freightamount)/ parseFloat(1- (parseFloat(margin)/100)));
    row.attributes.get("ig1_freighttotal").setValue(freightTotal);
    //Updating BOM 
    //UpdateBillOfMaterials(context);
}


//Nazish - Function added to calculate LU Extend
function luExtendCalculation(context)
{
    var control = context.getEventSource();
    var row= control.getParent();
    var laborUnit=row.attributes.get("ig1_projectlu").getValue();
    var materialCost = row.attributes.get("ig1_materialcost").getValue();
    var luExtend=(parseFloat(laborUnit)*parseFloat(materialCost));
    row.attributes.get("ig1_luextend").setValue(luExtend);
    //Updating BOM 
    //UpdateBillOfMaterials(context);
}

//Calculate Material Cost basis unit price and quantity
function CalculateMaterialCostLineItem(context)
{
	
    var control = context.getEventSource();
    var row= control.getParent();
    var quantity=row.attributes.get("ig1_quantity").getValue();
	 
	 
    if(quantity=='' || quantity==null || quantity==undefined)
    {
        quantity=0;
    }
  
    var unitprice=row.attributes.get("ig1_unitprice").getValue();
    if(unitprice=='' || unitprice==null || unitprice==undefined) 
    {
        unitprice=0;
    }
    var materialCost=parseFloat(quantity)*parseFloat(unitprice);
    row.attributes.get("ig1_materialcost").setValue(materialCost);;
    //UpdateBillOfMaterials(context);
  
}

function CalculateMaterialCostLineItemContingency(context)
{
	
    var control = context.getEventSource();
    var row= control.getParent();
    var quantity=row.attributes.get("ig1_quantity").getValue();
	 
	 
    if(quantity=='' || quantity==null || quantity==undefined)
    {
        quantity=1;
    }
  
    var unitprice=row.attributes.get("ig1_unitprice").getValue();
    if(unitprice=='' || unitprice==null || unitprice==undefined) 
    {
        unitprice=0;
    }
    var materialCost=parseFloat(quantity)*parseFloat(unitprice);
    row.attributes.get("ig1_materialcost").setValue(materialCost);
}

function UpdateBillOfMaterials(context)
{
    var formContext = context.getFormContext();
    var control = context.getEventSource();
    var row= control.getParent();
	
    var recordId = row.getEntityReference().id.replace('{','').replace('}','');
	
    var entity = {};
    //entity.ig1_luextend = parseFloat(row.attributes.get("ig1_luextend").getValue());
    entity.ig1_projectlu = parseFloat(row.attributes.get("ig1_projectlu").getValue());
    entity.ig1_freightamount = parseFloat(row.attributes.get("ig1_freightamount").getValue());
    entity.ig1_materialcost = parseFloat(row.attributes.get("ig1_materialcost").getValue());
    entity.ig1_quantity = parseFloat(row.attributes.get("ig1_quantity").getValue());
    entity.ig1_unitprice = parseFloat(row.attributes.get("ig1_unitprice").getValue());
    entity.ig1_freighttotal = parseFloat(row.attributes.get("ig1_freighttotal").getValue());
    entity.ig1_markup = parseFloat(row.attributes.get("ig1_markup").getValue());
    entity.ig1_luextend = parseFloat(entity.ig1_materialcost * entity.ig1_projectlu);


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
                //Success - No Return Data - Do Something
                Xrm.Page.getAttribute("ig1_isbidsheetcalculated").setValue(false);
                formContext.ui.clearFormNotification();
                formContext.ui.setFormNotification('Please click on "Calculate Indirect Cost" to see updated costs', "WARNING",);
                //createUpdatePMLabor();
                //createUpdateAssociatedCost();
                calculateBOMBidSheetTotals();
                RefreshBOMGrid(context);
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));
}

function bidsheetLineItemGridOnSave()
{
    // createUpdatePMLabor();
    // createUpdateAssociatedCost();
    // calculateBOMBidSheetTotals();
    // RefreshBOMGrid(context);
}

function RefreshBOMGrid(executionContext) 
{
	
    var formContext = executionContext.getFormContext();

    formContext.data.save();
    setTimeout(function(){ValidateSelectedStage();}, 2000);
    var grid = Xrm.Page.getControl('BidSheetPriceListItem');
    if (grid == null) 
    {
        setTimeout(function () { RefreshTotalsGrid(); }, 2000);
    }
    // else
    // {
    //  setTimeout(function () { Xrm.Page.data.refresh(); }, 2000);
    //}
}
function filterSubGrid(subgridName, fetchXml,formContext, attempts) { 
    // Try 10 times, then stop
    debugger;
    if (attempts < 0) { return; }		
    var isUnified = isUCI();
    var Contingency = formContext;
    //--------------------------------------Unified Interface----------------------------------------//
    if (isUnified) { 
        if (Contingency == null) {
            setTimeout(function () { filterSubGrid(subgridName, fetchXml,formContext, (attempts || 10) - 1); }, 1000);
            return;
        }

        else { 
            formContext.setFilterXml(fetchXml);
            formContext.refresh();	
            //var setFetchXmlStr = Microsoft.Crm.Client.Core.Storage.DataApi.ListQuery.prototype.set_FetchXml.toString();
            //var newFunc = setFetchXmlStr.replace("function(e){", "function(e){if (e.indexOf('ZZZAAA') >= 0) {e = fetchXml;}");
            //eval("Microsoft.Crm.Client.Core.Storage.DataApi.ListQuery.prototype.set_FetchXml=" + newFunc);
            //Contingency.refresh();
            //Refresh grid to show filtered records only.
            //formContext.ui.controls.get("Contingency").refresh();
            //SetVisibility(formContext,true); 
        }	  
    }
    else
    {
        // Get the subgrid element to filter
        var subgrid = window.parent.document.getElementById(subgridName);
        if (subgrid == null || subgrid.control == null) {
            // If the subgrid hasn't loaded yet, keep trying
            setTimeout(function () { filterSubGrid(subgridName, fetchXml,formContext, (attempts || 10) - 1); }, 500);
            return;
        }
        // Update the fetchXml on the subgrid and refresh the grid     	
        subgrid.control.SetParameter("fetchXml", fetchXml);
        subgrid.control.refresh();
        SetVisibility(formContext,true); 
    }
}

// Filters an accounts subgrid by the primary contact ID - this function is unique for your requirements
function filterContingencyGrid(executionContext) {    
    var category;
    var formContext = executionContext.getFormContext();	
    //var stageName=getStageName(formContext);
    //if(stageName=="Summary")
    //{
    //SetVisibility(formContext,false);
    //Get the current Record Guid
    var bidSheetId = formContext._formContext._data._entity._entityId.guid;
    //var bidSheetId=Xrm.Page.data.entity.getId();
    var fetchXml = 	"<fetch mapping=\"logical\" version=\"1.0\">"+
                   "  <entity name=\"ig1_bidsheetcategory\">"+
                   "    <attribute name=\"ig1_name\" />"+
                   "    <attribute name=\"ig1_bidsheetcategoryid\" />"+
                   "    <link-entity name=\"ig1_bscategoryvendor\" from=\"ig1_category\" to=\"ig1_bidsheetcategoryid\" link-type=\"inner\" alias=\"ak\">"+					
                   "    <filter type=\"and\">"+
                   "      <condition attribute=\"ig1_bidsheet\" operator=\"eq\" value='"+ bidSheetId +"'/>"+
                   "      <condition attribute=\"statecode\" operator=\"eq\" value=\"0\"/>"+
                   "    </filter>"+					
                   "  </link-entity>"+
                   "  </entity>"+
                   "</fetch>";
    var encodedFetchXml = encodeURI(fetchXml);
    var queryPath = "/api/data/v9.1/ig1_bidsheetcategories";

    var requestPath = Xrm.Page.context.getClientUrl() + queryPath;
    var req = new XMLHttpRequest();
    req.open("GET", requestPath + "?fetchXml=" + encodedFetchXml, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");

    req.onreadystatechange = function ()

    {
        if (this.readyState === 4)

        {
            this.onreadystatechange = null;

            if (this.status === 200)

            {
                var returned = JSON.parse(this.responseText);
                var results = returned.value;
                for (var i = 0; i < results.length; i++)
                {
                    if(results[i]["ig1_name"]=="Contingency")
                    {
                        category=results[i]["ig1_bidsheetcategoryid"];
                        break;
                    }
                    else
                        category="";
									
                    //TODO: Implement logic for handling results as desired
                }
                Subgrid(category,formContext);
            }

            else

            {

                alert(this.statusText);

            }

        }

    };

    req.send();
    //}
    //else
    //{
    //SetVisibility(formContext,false);
    //}
}
function Subgrid(category,formContext)
{		
    //var conSubGrid = formContext.getControl("Contingency");
    //Create FetchXML for sub grid to filter records
    if(category!="")
    {
        // Fetch xml code which will retrieve all the accounts related to the contact 
        var fetchXml="<filter type='and'><condition attribute='ig1_category' operator='eq' uitype='ig1_bidsheetcategory' value='" + category + "' /></filter>";
           
        filterSubGrid("Contingency", fetchXml,formContext);	 
   
    }
    else
        SetVisibility(formContext,false); 
}
function isUCI() {
    return Xrm.Internal.isUci()
}
function SetVisibility(formContext,flag)
{
    var tabObj = formContext.ui.tabs.get("Contingency");
    var secObj = tabObj.sections.get("Contingency");
    if(flag)
    {
        secObj.setVisible(true);
        tabObj.setVisible(true);
    }
    else
    {
        secObj.setVisible(false);
        tabObj.setVisible(false);
    }
	
}
function getStageName(formContext)
{
    var stageName=formContext.data.process.getSelectedStage().getName();
    return stageName; 	
}
function OnStageChange(executionContext)
{	
    var formContext = executionContext.getFormContext();
    formContext.data.process.addOnStageChange(filterContingencyGrid);
    formContext.data.process.addOnStageSelected(filterContingencyGrid);
    filterContingencyGrid(executionContext);
}
function restrictContigencyRecordNonEditable(executionContext)
{ 
    var entityObject = executionContext.getFormContext().data.entity;    
    entityObject.attributes.forEach(function(field, i) 
    {
        var probAttr = entityObject.attributes.getByName("ig1_category");
        var prob = probAttr.getValue();
        if(prob!=null)
        {
            if (prob[0].name == "Contingency" || prob[0].name=="Labor") {
                field.controls.get(0).setDisabled(true);
            }
        }
    });	
	
}
function HideNewButtonOnContingencyTab(executionContext)
{
    debugger;
    //var context = Xrm.Utility.getGlobalContext();
    var formContext=executionContext;
    var stageName=getStageName(formContext);
    if(stageName=="Summary")
        return false;
    else
        return true;
}
function AddContingencyPriceToTotalSellPrice(context)
{
    debugger;
    //var control = executionContext.getEventSource();	
    //var recordId= control.getId();
    var control = context.getEventSource();
    var row= control.getParent();	
    var recordId = row.getEntityReference().id.replace("{","").replace("}","");
    var bidSheetId,category;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+recordId+")?$select=_ig1_bidsheet_value,_ig1_category_value", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function() {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(req.responseText);
				
                bidSheetId=result._ig1_bidsheet_value;
                category= result._ig1_category_value;					
					
            }
            else
            {
                alert(this.statusText);
            }

        }
    };
    req.send();
	
    var materialcost=0;
    var fetchXml = 	"<fetch mapping=\"logical\" version=\"1.0\">"+
                   "  <entity name=\"ig1_bidsheetpricelistitem\">"+
                   "    <attribute name=\"ig1_materialcost\" />"+
                   "    <attribute name=\"ig1_bidsheetpricelistitemid\" />"+									
                   "    <filter type=\"and\">"+
                   "      <condition attribute=\"ig1_bidsheet\" operator=\"eq\" value='"+ bidSheetId +"'/>"+
                   "      <condition attribute=\"ig1_category\" operator=\"eq\" value='"+ category +"'/>"+
                   "      <condition attribute=\"statecode\" operator=\"eq\" value=\"0\"/>"+
                   "    </filter>"+
                   "  </entity>"+
                   "</fetch>";
    var encodedFetchXml = encodeURI(fetchXml);	
    var req = new XMLHttpRequest();
    var queryPath = "/api/data/v9.1/ig1_bidsheetpricelistitems";
    var requestPath = Xrm.Page.context.getClientUrl() + queryPath;
    var req = new XMLHttpRequest();
    req.open("GET", requestPath + "?fetchXml=" + encodedFetchXml, false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");	
    req.onreadystatechange = function() {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                var result=results.value;
                for (var i = 0; i < result.length; i++)
                {
                    materialcost= parseFloat(materialcost)+parseFloat(result[i]["ig1_materialcost"]);							
                }			
										
                var sellPriceAssociated=GetSellPriceAssociatedCost();
                sellPrice= parseFloat(materialcost)+parseFloat(sellPriceAssociated);
                Xrm.Page.getAttribute("ig1_sellprice").setValue(sellPrice);
                Xrm.Page.data.save();
            }
            else
            {
                alert(this.statusText);
            }

        }
    };
    req.send();
}
function GetSellPriceAssociatedCost()
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
					"    <attribute name='ig1_totalmaterialcost' />",
					"    <attribute name='ig1_materialcost' />",
					"    <attribute name='ig1_freight' />",
					"    <attribute name='ig1_totaldirectcost' />",
					"    <attribute name='ig1_totalsellprice' />",
					
					"    <attribute name='ig1_pmlaborsme' />",
					"    <attribute name='ig1_labourhours' />",
					"    <attribute name='ig1_designcost' />",
					"    <attribute name='ig1_designhours' />",
					"    <attribute name='ig1_salescost' />",
					"    <attribute name='ig1_saleshours' />",
					"    <attribute name='ig1_travelcost' />",
					"    <attribute name='ig1_totalindirectcost' />",
					"    <attribute name='ig1_totalindirectcost' />",
					"    <attribute name='ig1_lodgingtotal' />",
                    "    <attribute name='ig1_lodgingrate' />",
					"    <attribute name='ig1_perdiem' />",
					"    <attribute name='ig1_perdiemtotal' />",
					"    <attribute name='ig1_lodgingtotal' />",
					"    <attribute name='ig1_transporttotal' />",
					"    <attribute name='ig1_airfaretrans' />",
					"    <attribute name='ig1_basedesign' />",
					"    <attribute name='ig1_basesales' />",
					"    <attribute name='ig1_baselabor' />",
					"    <attribute name='ig1_baseindirect' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
		
    var fetchXmlData=XrmServiceToolkit.Soap.Fetch(fetchXml);
			
    if(fetchXmlData.length>0)
    {
        var sellprice = 0;
        for(i=0; i<fetchXmlData.length; i++)
        {
            result=fetchXmlData[i].attributes;
            if(result.ig1_totalsellprice != undefined)
                sellprice+=parseFloat(result.ig1_totalsellprice.value);
        }
        return sellprice;
			
    }
    else
        return 0;
}
function UpdateContingencyBillOfMaterials(context)
{
	
    var control = context.getEventSource();
    var row= control.getParent();
	
    var recordId = row.getEntityReference().id.replace('{','').replace('}','');
	
    var entity = {};	
    entity.ig1_materialcost = parseFloat(row.attributes.get("ig1_materialcost").getValue());
    entity.ig1_unitprice = parseFloat(row.attributes.get("ig1_materialcost").getValue());
    entity.ig1_quantity = parseInt(1);


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
                //Success - No Return Data - Do Something
                //calculateAssociatedTotalCostAndHour(context);
                                  
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));
}

function getContingencyMaterialCost()
{
    var totlaContingencyMaterialCost=0;
    var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
  
    var fetchData = {
        ig1_bidsheet: bidSheetId,
        ig1_categoryname: "Contingency"
    };
    var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheetpricelistitem'>",
                    "    <attribute name='ig1_materialcost' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*bf1962bf-a8fb-e911-a812-000d3a55d933*/, "'/>",
                    "      <condition attribute='ig1_categoryname' operator='eq' value='", fetchData.ig1_categoryname/*Contingency*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
    ].join("");  
                   
    var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(fetchData!=undefined && fetchData!=null && fetchData!="")
    { 
        for(ac=0; ac<fetchData.length; ac++)
        {
            var result=fetchData[ac].attributes;
            if(result.ig1_materialcost!=undefined && result.ig1_materialcost!=null && result.ig1_materialcost!="")
            {
                totlaContingencyMaterialCost+=parseFloat(result.ig1_materialcost.value);  
            }
        }
    }
    return totlaContingencyMaterialCost;
}

function isContingencyExist()
{
    var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var fetchData = {
        ig1_categoryname: "Contingency",
        ig1_bidsheet: bidSheetId
    };
    var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bscategoryvendor'>",
                    "    <attribute name='ig1_category' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_categoryname' operator='eq' value='", fetchData.ig1_categoryname/*Contingency*/, "'/>",
                    "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*201c7e6a-d105-ea11-a811-000d3a55d0f0*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
    ].join("");
    var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(fetchData.length>0)
    {
        return true;
    }
    else
    {
        return false;
    }
}
function GridOnLoad(executionContext)
{
    var formContext = executionContext.getFormContext();
    var gridContext = formContext.getControl("Contingency");// get the grid context    
    gridContext.addOnLoad(filterContingencyGrid);	
}