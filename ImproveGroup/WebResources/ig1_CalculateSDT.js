//Nazish Calculate Category SDT
function calculateSDT()
{
    debugger;
	var bidSheetId=Xrm.Page.data.entity.getId().replace("{","").replace("}","");
    var parameters = {};
	parameters.bidsheetId = bidSheetId;
	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_CalculateSDT", false);
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

function createUpdatePMLabor()
{
	    
		createLaborCategoryAndProduct();
	    var opportunityId="";
		var opportunity=Xrm.Page.getAttribute("ig1_opportunitytitle").getValue();
		if(opportunity!="" && opportunity!=null && opportunity!=undefined)
		{
			opportunityId=opportunity[0].id;
		}
	    var bidSheetId=Xrm.Page.data.entity.getId();
		var categoryId=laborCategoryId();
		var productId=pmLaborProductId();
		var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_category: categoryId,
		ig1_product: productId
	};
		var fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='ig1_bidsheetpricelistitem'>",
						"    <attribute name='ig1_bidsheetpricelistitemid' />",
						"    <filter type='and'>",
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*f6926b69-772f-453c-9f1c-189048a3bed6*/, "'/>",
						"      <condition attribute='ig1_category' operator='eq' value='", fetchData.ig1_category/*628b8a02-512e-4009-92a3-235e4c6acc59*/, "'/>",
						"      <condition attribute='ig1_product' operator='eq' value='", fetchData.ig1_product/*4e97d23e-60e6-4777-a616-d4e6ec05f349*/, "'/>",
						"    </filter>",
						"  </entity>",
						"</fetch>",
					   ].join("");
		var pmLaborData=XrmServiceToolkit.Soap.Fetch(fetchXml);
		if(pmLaborData!="" && pmLaborData!=null && pmLaborData!=undefined)
		{
			var result=pmLaborData[0].attributes;
			var bidSheetLineItemId=result.ig1_bidsheetpricelistitemid.value.replace("{", "").replace("}", "");
			updatePMLabor(bidSheetLineItemId);
		}
		else
		{
			createPMLabor(bidSheetId, categoryId, productId, opportunityId);
		}
}

function createPMLabor(bidSheetId, categoryId, productId, opportunityId)
{
	var isExist=isLineItemsExist();
		if(!isExist)
			return;
		
    var bidSheetId=bidSheetId.replace("{", "").replace("}", "");
    var opportunityId=opportunityId.replace("{", "").replace("}", "");
	var laborCost=fetchPMLaborCost();
	if(laborCost==0)
	   return;
	
	var entity = {};
	entity["ig1_BidSheet@odata.bind"] = "/ig1_bidsheets("+bidSheetId+")";
	entity["ig1_Category@odata.bind"] = "/ig1_bidsheetcategories("+categoryId+")";
	entity["ig1_Product@odata.bind"] = "/products("+productId+")";
	entity["ig1_Opportunity@odata.bind"] = "/opportunities("+opportunityId+")";
	entity.ig1_quantity = 1;
	entity.ig1_unitprice = parseFloat(laborCost);
	entity.ig1_materialcost = Number(parseFloat(laborCost).toFixed(4));

	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
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

function updatePMLabor(bidSheetLineItemId)
{
	var laborCost=fetchPMLaborCost();
	var isExist=isLineItemsExist();
	if(laborCost==0 || !isExist)
	{
		var req = new XMLHttpRequest();
		req.open("DELETE", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+bidSheetLineItemId+")", false);
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
	else
	{
		var entity = {};
		entity.ig1_materialcost = Number(parseFloat(laborCost).toFixed(4));
		entity.ig1_unitprice = parseFloat(laborCost);
		entity.ig1_quantity = 1;

		var req = new XMLHttpRequest();
		req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems("+bidSheetLineItemId+")", false);
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
//Nazish - Fetching labor cost from associated cost to add PM labor into the BOM
function fetchPMLaborCost()
{       
        var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
	    var laborCost=0;
		var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_productname: "PM Labor",
		ig1_categoryname: "Labor"
		};
		var fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='ig1_bidsheetpricelistitem'>",
						"    <attribute name='ig1_projectlu' />",
						"    <attribute name='ig1_materialcost' />",
						"    <attribute name='ig1_category' />",
						"    <filter type='and'>",
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*b48bcb61-63e0-e911-a964-000d3a1d58e9*/, "'/>",
						"      <condition attribute='ig1_productname' operator='neq' value='", fetchData.ig1_productname/*PM Labor*/, "'/>",
						"      <condition attribute='ig1_categoryname' operator='neq' value='", fetchData.ig1_categoryname/*Labor*/, "'/>",
						"    </filter>",
						"  </entity>",
						"</fetch>",
					   ].join("");
					   
		var laborCostData=XrmServiceToolkit.Soap.Fetch(fetchXml);
		if(laborCostData!="" && laborCostData!=null && laborCostData!=undefined)
		{
			for(l=0; l<laborCostData.length; l++)
			{
				var materialCost=0;
				var laborUnit=0;
				var LaborRate=0;
				var result=laborCostData[l].attributes;
                if(result.ig1_category.formattedValue!=undefined && result.ig1_category.formattedValue!=null && result.ig1_category.formattedValue!="Contingency")
                {
                    if(result.ig1_materialcost!="" && result.ig1_materialcost!=null && result.ig1_materialcost!= undefined)
                    {
                        materialCost=parseFloat(result.ig1_materialcost.value);
                    }
                    if(result.ig1_projectlu!="" && result.ig1_projectlu!=null && result.ig1_projectlu!= undefined)
                    {
                        laborUnit=parseFloat(result.ig1_projectlu.value);
                    }
                    if(result.ig1_category!="" && result.ig1_category!=null && result.ig1_category!= undefined)
                    {
                        var categoryId=result.ig1_category.id;
                        LaborRate=parseFloat(fetchPMLaborRate(categoryId)).toFixed(2);;
                    }
                    laborCost+=((materialCost*laborUnit)*LaborRate);
                }
			}
            
		}
	return laborCost;
}

function fetchPMLaborRate(categoryId)
{       
        var bidSheetId=Xrm.Page.data.entity.getId();
	    var laborRate=0;
		var fetchData = {
		ig1_bidsheet: bidSheetId.replace("{", "").replace("}", ""),
		ig1_bidsheetcategory:categoryId,
		statecode: "0"
		};
		var fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='ig1_associatedcost'>",
						"    <attribute name='ig1_pmlaborsme' />",
						"    <attribute name='ig1_laborrate' />",
						"    <filter type='and'>",
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*0CA45815-F3CE-E911-A95E-000D3A110BBD*/, "'/>",
						"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
						"      <condition attribute='ig1_bidsheetcategory' operator='eq' value='", fetchData.ig1_bidsheetcategory, "'/>",
						"    </filter>",
						"  </entity>",
						"</fetch>",
					   ].join("");
					   
		var laborRateData=XrmServiceToolkit.Soap.Fetch(fetchXml);
		if(laborRateData!="" && laborRateData!=null && laborRateData!=undefined)
		{
			if(laborRateData[0].attributes.ig1_laborrate!=undefined && laborRateData[0].attributes.ig1_laborrate!=null && laborRateData[0].attributes.ig1_laborrate!="")
			{
				laborRate=parseFloat(laborRateData[0].attributes.ig1_laborrate.value);
			}
		}
		else
		{
			var fetchXml = [
							"<fetch mapping='logical' version='1.0'>",
							"  <entity name='ig1_projectcostallowances'>",
							"    <attribute name='ig1_defaultlaborrate' />",
							"  </entity>",
							"</fetch>",
						  ].join("");
			var defaultLaborRateData=XrmServiceToolkit.Soap.Fetch(fetchXml);
			if(defaultLaborRateData!=undefined && defaultLaborRateData!=null && defaultLaborRateData!="")
			{
				if(defaultLaborRateData[0].attributes.ig1_defaultlaborrate!=undefined && defaultLaborRateData[0].attributes.ig1_defaultlaborrate!=null && defaultLaborRateData[0].attributes.ig1_defaultlaborrate!="")
				{
					laborRate=parseFloat(defaultLaborRateData[0].attributes.ig1_defaultlaborrate.value);
				}
			}
		}
	return laborRate;
}

//Check and create PM labor category and PM labor product if does not exist...
function createLaborCategoryAndProduct()
{
	//Check and Create Labor Category if does not exist...
	var fetchData = {
	ig1_name: "Labor",
	statecode: "0"
	};
	var fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='ig1_bidsheetcategory'>",
					"    <attribute name='ig1_bidsheetcategoryid' />",
					"    <attribute name='ig1_name' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_name' operator='eq' value='", fetchData.ig1_name/*Labor*/, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				   ].join("");
	var categoryData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(categoryData.length<=0 || categoryData=="" || categoryData==null || categoryData==undefined)
	{
		var entity = {};
		entity.ig1_name = "Labor";
		entity.ig1_laborunit = 0;
		entity.ig1_defaultmatcostmargin = 0;

		var req = new XMLHttpRequest();
		req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetcategories", false);
		req.setRequestHeader("OData-MaxVersion", "4.0");
		req.setRequestHeader("OData-Version", "4.0");
		req.setRequestHeader("Accept", "application/json");
		req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
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
	// Check and Create PM Labor Product if does not exist....
	
		var fetchData = {
		ig1_bidsheetcategoryname: "Labor",
		productnumber: "PML001",
		statecode: "0"
	};
	var LaborProductfetchXml = [
								"<fetch mapping='logical' version='1.0'>",
								"  <entity name='product'>",
								"    <attribute name='productid' />",
								"    <attribute name='productnumber' />",
								"    <filter type='and'>",
								//"      <condition attribute='ig1_bidsheetcategoryname' operator='eq' value='", fetchData.ig1_bidsheetcategoryname/*Labor*/, "'/>",
								"      <condition attribute='productnumber' operator='eq' value='", fetchData.productnumber/*PML001*/, "'/>",
								"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
								"    </filter>",
								"  </entity>",
								"</fetch>",
							  ].join("");
	var productData=XrmServiceToolkit.Soap.Fetch(LaborProductfetchXml);
	if(productData.length<=0 || productData=="" || productData==null || productData==undefined)
	{
		var categoryId=laborCategoryId();
		var entity = {};
		entity["defaultuomid@odata.bind"] = "/uoms(D8E7BC9C-47CB-49CE-B226-4FA95E52DA93)";
		entity["defaultuomscheduleid@odata.bind"] = "/uomschedules(EF1C2BDC-945B-41E7-84B3-FB8ED610C5BD)";
		entity["ig1_BidSheetCategory@odata.bind"] = "/ig1_bidsheetcategories("+categoryId+")";
		entity.name = "PM Labor";
		entity.productnumber = "PML001";
		entity.ig1_projectlu = 0;
		entity.ig1_freight = Number(parseFloat(0).toFixed(4));
		entity.ig1_taxcode = "SP156226";

		var req = new XMLHttpRequest();
		req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/products", false);
		req.setRequestHeader("OData-MaxVersion", "4.0");
		req.setRequestHeader("OData-Version", "4.0");
		req.setRequestHeader("Accept", "application/json");
		req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
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
							  
}

//Fetch labor Category id...
function laborCategoryId()
{
	var laborCategoryId="";
	var fetchData = {
	ig1_name: "Labor",
	statecode: "0"
	};
	var fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='ig1_bidsheetcategory'>",
					"    <attribute name='ig1_bidsheetcategoryid' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_name' operator='eq' value='", fetchData.ig1_name/*Labor*/, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				   ].join("");
	var laborCategoryData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(laborCategoryData!="" && laborCategoryData!=null && laborCategoryData!=undefined)
	{
		laborCategoryId=laborCategoryData[0].attributes.ig1_bidsheetcategoryid.value.replace("{", "").replace("}", "");
	}
  return laborCategoryId;
}
//Get PM Labor Product id..
function pmLaborProductId()
{
	var pmLaborProductId="";
		var fetchData = {
		productnumber: "PML001",
		statecode: "0"
	};
	var fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='product'>",
					"    <attribute name='productid' />",
					"    <filter type='and'>",
					"      <condition attribute='productnumber' operator='eq' value='", fetchData.productnumber/*PML001*/, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				  ].join("");
	var pmLaborProductData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(pmLaborProductData!="" && pmLaborProductData!=null && pmLaborProductData!=undefined)
	{
		pmLaborProductId=pmLaborProductData[0].attributes.productid.value.replace("{", "").replace("}", "");
	}
  return pmLaborProductId;
}

function isLineItemsExist()
{
	var bidSheetId=Xrm.Page.data.entity.getId().replace("{","").replace("}","");
		var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_productname: "PM Labor",
		ig1_categoryname: "Labor"
	};
	var fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='ig1_bidsheetpricelistitem'>",
					"    <attribute name='ig1_bidsheetpricelistitemid' />",
					"    <attribute name='ig1_product' />",
					"    <attribute name='ig1_category' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*b48bcb61-63e0-e911-a964-000d3a1d58e9*/, "'/>",
					"      <condition attribute='ig1_productname' operator='neq' value='", fetchData.ig1_productname/*PM Labor*/, "'/>",
					"      <condition attribute='ig1_categoryname' operator='neq' value='", fetchData.ig1_categoryname/*Labor*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				    ].join("");
					
	var lineItemsData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(lineItemsData!=undefined && lineItemsData!=null && lineItemsData!="" && lineItemsData.length>0)
		return true;
     else
		return false;
}


function setPMLabor(executionContext)
{
    var formContext = executionContext.getFormContext();
	var bidsheetid = formContext.data.entity.getId().replace("{", "").replace("}", "");
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_bidsheetpricelistitems?$select=ig1_materialcost&$filter=_ig1_bidsheet_value eq "+bidsheetid+" and  _ig1_product_value eq 34b628fa-d5cf-e911-a960-000d3a1d52e7", true);
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
					var ig1_materialcost = results.value[i]["ig1_materialcost"];
					var ig1_materialcost_formatted = results.value[i] 
                                      ["ig1_materialcost@OData.Community.Display.V1.FormattedValue"];
                                     formContext.getAttribute("ig1_laborcost").setValue(parseFloat(ig1_materialcost));
				}
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();
}