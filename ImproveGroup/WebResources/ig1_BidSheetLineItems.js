function checkDuplicateProduct(executionContext)
{   
     debugger;
        var bidSheetId='';
	var productId='';
	var formContext = executionContext.getFormContext();
	var product=Xrm.Page.getAttribute("ig1_product").getValue();
	if(product!='' && product!=null && product!=undefined)
	{
		productId=product[0].id.replace('{','').replace('}','');
	}
	var bidSheet=Xrm.Page.getAttribute("ig1_bidsheet").getValue();
	if(bidSheet!='' && bidSheet!=null && bidSheet!=undefined)
	{
		bidSheetId=bidSheet[0].id.replace('{','').replace('}','');
	}
	
        var fetchData = {
		ig1_bidsheet: bidSheetId,
		ig1_product: productId,
		statecode: "0"
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheetpricelistitem'>",
                    "    <attribute name='ig1_product' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"      <condition attribute='ig1_product' operator='eq' value='", fetchData.ig1_product, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
         var bidSheetPriceListData=XrmServiceToolkit.Soap.Fetch(fetchXml);
         if(bidSheetPriceListData.length>0)
          {
            Xrm.Page.getAttribute("ig1_product").setValue(null);
	   alert("This product is already exist please choose different or update existing one");
          }
        else
	{
		var category= new Array();
		category[0]=new Object();
		var dataFetch = {
		statecode: "0",
		productid: productId
	    };
	    var fetchXmlProduct = [
								"<fetch>",
								"  <entity name='product'>",
								"    <attribute name='name' />",
								"    <attribute name='ig1_bidsheetcategory' />",
								"    <attribute name='name' />",
								"    <attribute name='ig1_projectlu' />",
								"    <attribute name='ig1_freight' />",
								"    <filter>",
								"      <condition attribute='statecode' operator='eq' value='", dataFetch.statecode/*0*/, "'/>",
								"      <condition attribute='productid' operator='eq' value='", dataFetch.productid/*F2183FA4-7BA2-E911-A95D-000D3A1D5D58*/, "'/>",
								"    </filter>",
								"    <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_bidsheetcategory'>",
								"      <attribute name='ig1_laborunit' alias='laborUnit'/>",
								"      <attribute name='ig1_defaultmatcostmargin' alias='margin'/>",
								"    </link-entity>",
								"  </entity>",
								"</fetch>",
							   ].join("");
	    var productFetchData = executeFetchXml("products", fetchXmlProduct);
		//var productFetchData=XrmServiceToolkit.Soap.Fetch(fetchXmlProduct);
		if(productFetchData.value!='' && productFetchData.value!=null && productFetchData.value!=undefined && productFetchData.value.length>0)
		{
				var result=productFetchData.value[0];
				if(result._ig1_bidsheetcategory_value != '' && result._ig1_bidsheetcategory_value != null && result._ig1_bidsheetcategory_value != undefined)
				{
				category[0].id=result._ig1_bidsheetcategory_value.toString();
				category[0].name=result["_ig1_bidsheetcategory_value@OData.Community.Display.V1.FormattedValue"];
				category[0].entityType=result["_ig1_bidsheetcategory_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
				Xrm.Page.getAttribute("ig1_category").setValue(category);
				formContext.getAttribute("ig1_category").fireOnChange();
				}
                else
				{
					Xrm.Page.getAttribute("ig1_category").setValue(null);
				}
				Xrm.Page.getAttribute("ig1_name").setValue(result.name);
				if(result.ig1_projectlu!=undefined && result.ig1_projectlu!=null && result.ig1_projectlu!="")
				{
				   Xrm.Page.getAttribute("ig1_projectlu").setValue(result.ig1_projectlu);
				}
				else if(result.laborUnit!="" && result.laborUnit!=null && result.laborUnit!= undefined)
				{
					  Xrm.Page.getAttribute("ig1_projectlu").setValue(result.laborUnit);
				}
			    if(result.ig1_freight!= undefined && result.ig1_freight!=null && result.ig1_freight!="")
                {
					Xrm.Page.getAttribute("ig1_freightamount").setValue(result.ig1_freight);
                }
				else
				{
					Xrm.Page.getAttribute("ig1_freightamount").setValue(0);
				}
			        calculateFreightTotal();
		}
	}
}

function calculateLUExtend()
{
  var maetrialCost=Xrm.Page.getAttribute("ig1_materialcost").getValue();
  if(maetrialCost=='' || maetrialCost==null || maetrialCost==undefined)
    {
      maetrialCost=0;
    }
  var laborUnit=Xrm.Page.getAttribute("ig1_projectlu").getValue();
  if(laborUnit=='' || laborUnit==null || laborUnit==undefined) 
   {
     laborUnit=0;
   }
  var luExtend=parseFloat(maetrialCost)*parseFloat(laborUnit);
  Xrm.Page.getAttribute("ig1_luextend").setValue(luExtend);
}


//Padmaja Calculate Freight Total
function calculateFreightTotal()
{
	
  var freightamount=Xrm.Page.getAttribute("ig1_freightamount").getValue();
  if(freightamount=='' || freightamount==null || freightamount==undefined)
    {
      freightamount=0;
    }
  var margin=Xrm.Page.getAttribute("ig1_markup").getValue();
  if(margin=='' || margin==null || margin==undefined) 
   {
     margin=0;
   }
  if(margin>=0 && margin<100)
   {
     //var freighttotal=parseFloat(freightamount)/parseFloat(margin);
     var freighttotal=(parseFloat(freightamount)/ parseFloat(1- (parseFloat(margin)/100)));
     Xrm.Page.getAttribute("ig1_freighttotal").setValue(freighttotal);
   }
   else
   {
      alert("Margin cannot be more than or equal to 100%. Please enter a valid value");
   }
}



function preFilterLookup() 
{    
	Xrm.Page.getControl("ig1_category").addPreSearch(function ()
	{
          PopulateLookup();
        });
   
}


//Padmaja get categories associated to the BidSheet
function PopulateLookup()
{
	 var bidSheetId='';
	 var bidSheet=Xrm.Page.getAttribute("ig1_bidsheet").getValue();
	if(bidSheet!='' && bidSheet!=null && bidSheet!=undefined)
	{
		bidSheetId=bidSheet[0].id.replace('{','').replace('}','');
	}
	
	var filterfetchXMLbyBidSheet =[ "<fetch>",
							  "<entity name='ig1_bscategoryvendor' >",
								"<attribute name='ig1_category'/>",
								"<attribute name='ig1_bidsheet'/>",
								"<filter>",
								  "<condition attribute='ig1_bidsheet' operator='eq' value='", bidSheetId, "' />",
								"</filter>",
							  "</entity>",
							"</fetch>" ].join("");
							
	 var fetchData = XrmServiceToolkit.Soap.Fetch(filterfetchXMLbyBidSheet);
	 var conditionstring = "";
	 
	  for(var i = 0; i<= fetchData.length - 1;i++)
		{
		conditionstring  += "<value>" + fetchData[i].attributes.ig1_category.id + "</value>" ;
		}
	 						
		var filterfetchXMLbyBidSheet ="<filter type ='and'><condition attribute='ig1_bidsheetcategoryid' operator='in'>"+conditionstring+"</condition></filter>";							  "</entity>",
		Xrm.Page.getControl("ig1_category").addCustomFilter(filterfetchXMLbyBidSheet);					
}


//Prepopulate Labor unit and Margin from category selected
function PopulateCategoryDefaultValues()
{
	var bidsheetCategoryId;
	var bidsheetCategory = Xrm.Page.getAttribute("ig1_category").getValue();
	if(bidsheetCategory!='' && bidsheetCategory!=null && bidsheetCategory!=undefined)
	{
		bidsheetCategoryId=bidsheetCategory[0].id;
		var categoryName = bidsheetCategory[0].name;
		if(categoryName!=undefined && categoryName!=null && categoryName!="" && categoryName=="Labor")
		{
			alert("Labor category can't be selected please choose another category");
			Xrm.Page.getAttribute("ig1_category").setValue(null);
			return;
		}
	}
	if(bidsheetCategoryId!='' && bidsheetCategoryId!=null && bidsheetCategoryId!=undefined)
	{
	//get category defaults
	var filterfetchXMLCategory =[ "<fetch>",
							  "<entity name='ig1_bidsheetcategory' >",
								"<attribute name='ig1_laborunit'/>",
								"<attribute name='ig1_defaultmatcostmargin'/>",
								"<filter>",
								  "<condition attribute='ig1_bidsheetcategoryid' operator='eq' value='", bidsheetCategoryId , "' />",
								"</filter>",
							  "</entity>",
							"</fetch>" ].join("");
							
	 var fetchData = XrmServiceToolkit.Soap.Fetch(filterfetchXMLCategory);
	 
	 
	 if (fetchData[0].attributes.ig1_laborunit!= undefined && fetchData[0].attributes.ig1_laborunit!= null)
	 {
		 var LU = fetchData[0].attributes.ig1_laborunit.value;
		 if(Xrm.Page.getAttribute("ig1_laborunit") == undefined || Xrm.Page.getAttribute("ig1_laborunit") == null)
		 {
			 Xrm.Page.getAttribute("ig1_projectlu").setValue(LU);
			 
			 calculateLUExtend();
		 }
	 }
	  if (fetchData[0].attributes.ig1_defaultmatcostmargin!= undefined && fetchData[0].attributes.ig1_defaultmatcostmargin!= null)
	 {
		 var Margin = fetchData[0].attributes.ig1_defaultmatcostmargin.value;
		 if(Xrm.Page.getAttribute("ig1_defaultmatcostmargin") == undefined || Xrm.Page.getAttribute("ig1_defaultmatcostmargin") == null)
		 {
			 Xrm.Page.getAttribute("ig1_markup").setValue(Margin);
			 
		 }
	 }
	 
	}
	 
							
}



//Nazish - Get Opportunity associated with bidsheet and set the value of opportunity
function setOpportunity()
{
	var bidSheetId='';
	var bidSheet= Xrm.Page.getAttribute("ig1_bidsheet").getValue();
	if(bidSheet!='' && bidSheet!=null && bidSheet!=undefined)
	{
		bidSheetId=bidSheet[0].id.replace('{', '').replace('}', '');
	}
		var fetchData = {
		ig1_bidsheetid: bidSheetId
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_opportunitytitle' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
						].join("");
						
	var opportunityFetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(opportunityFetchData.length>0 && opportunityFetchData!='' && opportunityFetchData!=null && opportunityFetchData!=undefined)
	{
		var result = opportunityFetchData[0].attributes;
		if(result.ig1_opportunitytitle !='' && result.ig1_opportunitytitle != null && result.ig1_opportunitytitle != undefined)
		{
			var opportunity = new Array();
			opportunity[0]= new Object();
			opportunity[0].id=result.ig1_opportunitytitle.id;
			opportunity[0].name=result.ig1_opportunitytitle.name;
			opportunity[0].entityType=result.ig1_opportunitytitle.logicalName;
			Xrm.Page.getAttribute("ig1_opportunity").setValue(opportunity);
		}
	}
}

//Nazish - Filter applied on product lookup view
function preFilterProductLookup() 
{    
	Xrm.Page.getControl("ig1_product").addPreSearch(function ()
	{
     populateProductLookup();

   });
}
function populateProductLookup()
{
	var bidSheetId='';
	var bidSheet=Xrm.Page.getAttribute("ig1_bidsheet").getValue();
	if(bidSheet!='' && bidSheet!=null && bidSheet!=undefined)
	{
		bidSheetId=bidSheet[0].id.replace('{','').replace('}','');
	}
// Nazish - Filtering those product which contain the categories	
	var fetchData = {
		ig1_bidsheet: bidSheetId
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='product'>",
					"    <attribute name='name' />",
					"    <attribute name='productid' />",
					"    <attribute name='ig1_bidsheetcategory' />",
					"    <link-entity name='ig1_bscategoryvendor' from='ig1_category' to='ig1_bidsheetcategory'>",
					"      <filter type='and'>",
					"        <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"      </filter>",
					"    </link-entity>",
					"  </entity>",
					"</fetch>",
				   ].join("");
				   
	
	var productFetchXmldata = XrmServiceToolkit.Soap.Fetch(fetchXml);
	var conditionstring = "";
	
	if(productFetchXmldata!='' && productFetchXmldata!=null && productFetchXmldata!=undefined && productFetchXmldata.length>0)
	{
		for(var i = 0; i<= productFetchXmldata.length - 1;i++)
		{
			conditionstring  += "<value>" + productFetchXmldata[i].attributes.productid.value + "</value>" ;
		}
	}
	
	
	
		
// Nazish - Filtering those product which didn't contain categories
		var fetchXmlNullCategory = [
									"<fetch>",
									"  <entity name='product'>",
									"    <attribute name='name' />",
									"    <attribute name='productid' />",
									"    <filter type='and'>",
									"      <condition attribute='ig1_bidsheetcategory' operator='null' />",
									"    </filter>",
									"  </entity>",
									"</fetch>",
										].join("");
	
		var productFetchXmlNullCategorydata = XrmServiceToolkit.Soap.Fetch(fetchXmlNullCategory);
	    if(productFetchXmlNullCategorydata!='' && productFetchXmlNullCategorydata!=null && productFetchXmlNullCategorydata!=undefined && productFetchXmlNullCategorydata.length>0)
	    {
			for(i=0; i<productFetchXmlNullCategorydata.length-1; i++)
			{
		          conditionstring  += "<value>" + productFetchXmlNullCategorydata[i].attributes.productid.value + "</value>" ;
			}
	    }
	
		var filterfetchProduct ="<filter type ='and'><condition attribute='productid' operator='in'>"+conditionstring+"</condition></filter>";"</entity>",
		Xrm.Page.getControl("ig1_product").addCustomFilter(filterfetchProduct);
	
}


//Calculate Material Cost basis unit price and quantity
function CalculateMaterialCost()
{
	 var quantity=Xrm.Page.getAttribute("ig1_quantity").getValue();
  if(quantity=='' || quantity==null || quantity==undefined)
    {
      quantity=0;
    }
  var unitprice=Xrm.Page.getAttribute("ig1_unitprice").getValue();
  if(unitprice=='' || unitprice==null || unitprice==undefined) 
   {
     unitprice=0;
   }
  //var freighttotal=parseFloat(freightamount)/parseFloat(margin);
  var materialCost=parseFloat(quantity)*parseFloat(unitprice);
  Xrm.Page.getAttribute("ig1_materialcost").setValue(materialCost);
  calculateLUExtend();
}


//Calculate Material Cost basis unit price and quantity
function SyncMaterialCost()
{
	
	 var materialCost=Xrm.Page.getAttribute("ig1_materialcost").getValue();
	if(materialCost=='' || materialCost==null || materialCost==undefined)
        {
		materialCost=0;
	}
	var quantity=Xrm.Page.getAttribute("ig1_quantity").getValue();
	if(quantity=='' || quantity==null || quantity==undefined)
	{
		quantity=1;
	}
	var unitprice=Xrm.Page.getAttribute("ig1_unitprice").getValue();
	if(unitprice=='' || unitprice==null || unitprice==undefined) 
	{
		unitprice=0;
	}
	var materialCost=parseFloat(quantity)*parseFloat(unitprice);
	Xrm.Page.getAttribute("ig1_materialcost").setValue(materialCost);
	Xrm.Page.getAttribute("ig1_quantity").setValue(quantity);
	Xrm.Page.getAttribute("ig1_unitprice").setValue(unitprice);
}

function isCategoryExist()
{
    var bidSheetId="";
    var bidSheet=Xrm.Page.getAttribute("ig1_bidsheet").getValue();
    if(bidSheet!=undefined && bidSheet!=null && bidSheet!="")
    {
      bidSheetId= bidSheet[0].id.replace("{", "").replace("}", ""); 
    }
    
    var productCategoryId=getProductCategory();
        if(productCategoryId==null || productCategoryId=="")
        {
          return;
        }
    	var fetchData = {
		ig1_category: productCategoryId,
		ig1_bidsheet: bidSheetId
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bscategoryvendor'>",
                    "    <attribute name='ig1_category' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_category' operator='eq' value='", fetchData.ig1_category/*795fb5a1-4a71-435a-99b6-13f93ba29ae2*/, "'/>",
                    "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*da371137-65d4-4ff1-9948-8d3dd0441388*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                   ].join("");
                   
    var categoryVendor=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(categoryVendor.length<=0 || categoryVendor==null)
    {
        alert("The Product Category is not associated to this Bidsheet.");
        Xrm.Page.getAttribute("ig1_product").setValue(null);
        Xrm.Page.getAttribute("ig1_category").setValue(null);
    }
}


function getProductCategory()
{
    var categoryId="";
    var product=Xrm.Page.getAttribute("ig1_product").getValue();
    if(product==undefined || product==null || product=="")
    {
        return;
    }
    var productid=product[0].id.replace("{", "").replace("}", "");
    var fetchData = {
	productid: productid
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='product'>",
                    "    <attribute name='ig1_bidsheetcategory' />",
                    "    <filter type='and'>",
                    "      <condition attribute='productid' operator='eq' value='", fetchData.productid/*106d28ff-fca7-46e6-94f8-bd10e2e57a80*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                   ].join("");
                  
    var category = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(category!=undefined && category!=null && category!="")
    {
        categoryId=category[0].attributes.ig1_bidsheetcategory.id;
    }
    return categoryId;
}



function setAsReadOnly(executionContext)
{
  debugger;
  var formContext = executionContext.getFormContext();
  var bsStatus=isBidSheetStatusDraft();
  if(bsStatus!="286150001" && bsStatus!="286150003")
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
}

function isBidSheetStatusDraft()
{
	debugger;
	var bidSheetStatus = "";
    var bidSheetId="";
    var bidSheet=Xrm.Page.getAttribute("ig1_bidsheet").getValue();
    if(bidSheet!=undefined && bidSheet!=null && bidSheet!="")
    {
       bidSheetId= bidSheet[0].id.replace("{", "").replace("}", "");
    }
    	var fetchData = {
		ig1_bidsheetid: bidSheetId
	};
	var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheet'>",
                    "    <attribute name='ig1_status' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*bf1962bf-a8fb-e911-a812-000d3a55d933*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
                   ].join("");
    var BsSatatusData=XrmServiceToolkit.Soap.Fetch(fetchXml);
    if(BsSatatusData!=undefined && BsSatatusData!=null && BsSatatusData.length>0)
	{
		var result = BsSatatusData[0].attributes;
		if(result.ig1_status!=undefined && result.ig1_status!=null && result.ig1_status!="")
		{
			bidSheetStatus = result.ig1_status.value;
		}
	}
	return bidSheetStatus;
}
function bidSheetLineItem_DisableFields_forContingency(executionContext)
{	
	var formContext = executionContext.getFormContext();
	var category=formContext.getAttribute("ig1_category").getValue();
	if(category!=null)
	{
		var category = formContext.getAttribute("ig1_category").getValue()[0].name;
                var formType=formContext.ui.getFormType();
		if(category=="Contingency")
	    {
            if(formType==1)
            {
               formContext.getAttribute("ig1_quantity").setValue(1);
               formContext.getAttribute("ig1_unitprice").setValue(0);
               formContext.getAttribute("ig1_materialcost").setValue(0);
            }
			formContext.ui.controls.get("ig1_quantity").setDisabled(true);	            
            formContext.ui.controls.get("ig1_unitprice").setDisabled(true);
			formContext.ui.controls.get("ig1_materialcost").setDisabled(true);
            formContext.ui.controls.get("ig1_projectlu").setDisabled(true);	
			formContext.ui.controls.get("ig1_freightamount").setDisabled(true);
			formContext.ui.controls.get("ig1_markup").setDisabled(true);
			formContext.ui.controls.get("ig1_freighttotal").setDisabled(true);
		}
        else
        {
			var bsStatus=isBidSheetStatusDraft();
			if(bsStatus=="286150001" && bsStatus=="286150003")
			{
				formContext.ui.controls.get("ig1_quantity").setDisabled(false);	            
				formContext.ui.controls.get("ig1_unitprice").setDisabled(false);
				formContext.ui.controls.get("ig1_projectlu").setDisabled(false);	 
				formContext.ui.controls.get("ig1_freightamount").setDisabled(false);
				formContext.ui.controls.get("ig1_markup").setDisabled(false);
			}
        }
         		
	}

}
function bidSheetLineItem_SetVaue_forContingency(executionContext)
{
	//debugger;
	var formContext = executionContext.getFormContext();
	var category=formContext.getAttribute("ig1_category").getValue();
	if(category!=null)
	{
		var category = formContext.getAttribute("ig1_category").getValue()[0].name;
		if(category=="Contingency")
		{
	      var formType=formContext.ui.getFormType();
			if(formType==1)
			{
				formContext.getAttribute("ig1_unitprice").setValue(0);
				formContext.getAttribute("ig1_materialcost").setValue(0);
				formContext.getAttribute("ig1_freightamount").setValue(0);
				formContext.getAttribute("ig1_markup").setValue(0);
				formContext.getAttribute("ig1_freighttotal").setValue(0);
			}
		}
	}
}

function setAsReadOnlyForLabor(executionContext)
{
       var formContext = executionContext.getFormContext();
	var category=formContext.getAttribute("ig1_category").getValue();
	if(category!=undefined && category!=null)
	{
		var category = formContext.getAttribute("ig1_category").getValue()[0].name;
               if(category == "Labor")
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
         }
}