//Nazish -- Creating/Updating Associated Cost from Bidsheet Price List Item
function createUpdateAssociatedCost()
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
						"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet/*E1E951CF-63C2-E911-A960-000D3A1D52E7*/, "'/>",
						"    </filter>",
						"    <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category'>",
						" <attribute name='ig1_name'/> ",
						" <attribute name='ig1_defaultmatcostmargin' alias='defaultMargin' /> ",
						"    </link-entity>",
						"  </entity>",
						"</fetch>",
				    ].join("");
	  var fetchData = executeFetchXml("ig1_bidsheetpricelistitems", fetchXml);
	 //var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	 if(fetchData.value!='' && fetchData.value!=null && fetchData.value!=undefined && fetchData.value.length>0)
	 {
		var categories=new Array();
		for(s=0; s<fetchData.value.length; s++)
		{
			 var materialCost=0;
	         var projectHour=0;
			 var margin=0;
			 var luExtend=0;
			 var freighttotal=0;
			 var freightsell = 0;
			 var flag=false;
			 var categoryId=fetchData.value[s]._ig1_category_value;
			 for(j=0; j<fetchData.value.length; j++)
			 {
				  var secondResult=fetchData.value[j];
				  if(!categories.includes(categoryId) && secondResult._ig1_category_value==categoryId && secondResult["_ig1_category_value@OData.Community.Display.V1.FormattedValue"]!="Labor" && secondResult["_ig1_category_value@OData.Community.Display.V1.FormattedValue"]!="Contingency")
				   {
					 materialCost+=parseFloat(secondResult.ig1_materialcost);
                     luExtend+=parseFloat(secondResult.ig1_luextend);
					 freighttotal+=parseFloat(secondResult.ig1_freightamount);
					 freightsell+= parseFloat(secondResult.ig1_freighttotal);
					 if(secondResult.defaultMargin !="" && secondResult.defaultMargin !=null && secondResult.defaultMargin !=undefined)
					 {
						 margin=parseFloat(secondResult.defaultMargin);
					 }
					 flag=true;
				   }
			 }
			 categories.push(categoryId);
			 if(flag)
			 {
				associatedCostData(categoryId, materialCost,  luExtend, freighttotal, margin,freightsell); 
                UpdateAssociatedRecordId(categoryId);
			 }

    
		}
	 }
//Updating Total Cost of BidSheet
	           calculateAssociatedTotalCostAndHour();

}

//Create New Associated Cost Data 
function associatedCostData(categoryId, materialCost,  luExtend, freight, margin,freightsell)
{
	var bidSheetId=Xrm.Page.data.entity.getId().toUpperCase().replace('{','').replace('}','');
    var categoryid=categoryId.replace('{','').toUpperCase().replace('}','')
  var fetchXmlCostAllowance = [
								"<fetch mapping='logical' version='1.0'>",
								"  <entity name='ig1_projectcostallowances'>",
								"    <attribute name='ig1_salesfactor' />",
								"    <attribute name='ig1_perdiem_base' />",
								"    <attribute name='ig1_salesmargin' />",
								"    <attribute name='ig1_labormargin' />",
								"    <attribute name='ig1_corpgna_base' />",
								"    <attribute name='ig1_corpgna' />",
								"    <attribute name='ig1_designmargin' />",
								"    <attribute name='ig1_perdiem' />",
								"    <attribute name='ig1_designlaborrate' />",
								"    <attribute name='ig1_designfactor' />",
								"    <attribute name='ig1_effectivedate' />",
								"    <attribute name='ig1_saleslaborrate' />",
								"    <attribute name='ig1_designlaborrate_base' />",
								"    <attribute name='ig1_margin' />",
								"    <attribute name='ig1_lodging' />",
								"    <attribute name='ig1_defaultlaborrate' />",
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
						"    <attribute name='ig1_totalsellprice' />",
						"    <attribute name='ig1_margin' />",
						"    <attribute name='ig1_travelcost' />",
						"    <attribute name='ig1_lodgingrate' />",
						"    <attribute name='ig1_numberoftrip' />",
						"    <attribute name='ig1_peoplepertrip' />",
						"    <attribute name='ig1_perdiem' />",
						"    <attribute name='ig1_airfaretrans' />",
						"    <attribute name='ig1_days' />",
						"    <attribute name='ig1_laborrate' />",
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
			if(result.ig1_defaultlaborrate!=undefined && result.ig1_defaultlaborrate!=null && result.ig1_defaultlaborrate!="")
			{
				entity.ig1_laborrate = parseFloat(result.ig1_defaultlaborrate.value);
			}
            entity.ig1_lodgingrate = parseFloat(result.ig1_lodging.value);
			entity.ig1_perdiem= Number((parseFloat(result.ig1_perdiem.value)).toFixed(4));
			entity.ig1_lodgingtotal = 0;
			entity.ig1_margin = margin;
			entity.ig1_freight = Number((parseFloat(freight)).toFixed(4));
			entity.ig1_freightsell = Number((parseFloat(freightsell)).toFixed(4));
			entity.ig1_totalmaterialcost = Number((parseFloat(materialCost)).toFixed(4));
			entity.ig1_pmlaborsme = parseFloat(entity.ig1_laborrate)*parseFloat(luExtend);
			
			if(result.ig1_labormargin!=undefined && result.ig1_labormargin!=null && result.ig1_labormargin!="")
			{
				entity.ig1_baselabor=parseFloat(entity.ig1_pmlaborsme)*parseFloat(result.ig1_labormargin.value);
			}
			
			entity.ig1_designfactor = parseFloat(result.ig1_designfactor.value);
			entity.ig1_designlaborrate = Number(parseFloat(result.ig1_designlaborrate.value).toFixed(4));
			entity.ig1_designcost = Number(((parseFloat(result.ig1_designfactor.value))*(parseFloat(luExtend))*(parseFloat(result.ig1_designlaborrate.value))).toFixed(4));
			
			if(result.ig1_designmargin!=undefined && result.ig1_designmargin!=null && result.ig1_designmargin!="")
			{
			  entity.ig1_basedesign=Number((parseFloat(entity.ig1_designcost))*(parseFloat(result.ig1_designmargin.value)));
			}
			
			entity.ig1_salesfactor = parseFloat(result.ig1_salesfactor.value);
			entity.ig1_saleslaborrate = Number(parseFloat(result.ig1_saleslaborrate.value).toFixed(4));
			entity.ig1_salescost = Number(((parseFloat(result.ig1_salesfactor.value))*(parseFloat(luExtend))*(parseFloat(result.ig1_saleslaborrate.value))).toFixed(4));
			
			if(result.ig1_salesmargin!=undefined && result.ig1_salesmargin!=null && result.ig1_salesmargin!="")
			{
			  entity.ig1_basesales=Number((parseFloat(entity.ig1_salescost))*(parseFloat(result.ig1_salesmargin.value)));
			}
			
			entity.ig1_airfaretrans = Number(parseFloat(0).toFixed(4));
			entity.ig1_days = 0;
			entity.ig1_travelcost = Number(parseFloat(0).toFixed(4));
			entity.ig1_materialcost = Number((parseFloat(materialCost)).toFixed(4));
			entity.ig1_totaldirectcost=parseFloat(entity.ig1_freight) + parseFloat(entity.ig1_materialcost) ;
			
			entity.ig1_totalindirectcost=parseFloat(entity.ig1_designcost) + parseFloat(entity.ig1_pmlaborsme) + parseFloat(entity.ig1_salescost) +parseFloat(entity.ig1_travelcost);
			entity.ig1_baseindirect=Number(parseFloat(entity.ig1_basesales)+parseFloat(entity.ig1_basedesign)+parseFloat(entity.ig1_baselabor)+parseFloat(entity.ig1_travelcost));
			var sellPrice=0;
			var materialCostWithMargin=parseFloat(entity.ig1_materialcost);
			if(margin!= 100)
			{
				materialCostWithMargin = (parseFloat(entity.ig1_materialcost)/parseFloat(1-parseFloat(entity.ig1_margin)/100));
				sellPrice = parseFloat(entity.ig1_designcost)+parseFloat(entity.ig1_freightsell)+parseFloat(materialCostWithMargin)+parseFloat(entity.ig1_pmlaborsme)+parseFloat(entity.ig1_salescost)+parseFloat(entity.ig1_travelcost);
			}
			else
			{
				sellPrice = parseFloat(entity.ig1_designcost) + parseFloat(entity.ig1_freight) + parseFloat(entity.ig1_materialcost) + parseFloat(entity.ig1_pmlaborsme) + parseFloat(entity.ig1_salescost) +parseFloat(entity.ig1_travelcost);
			}
            //entity.ig1_totalcost = Number((entity.ig1_materialcost +entity.ig1_designcost+entity.ig1_salescost+entity.ig1_travelcost+entity.ig1_freight).toFixed(4));
			entity.ig1_totalcost = Number((entity.ig1_materialcost +entity.ig1_basedesign+entity.ig1_basesales+entity.ig1_travelcost+entity.ig1_freight+entity.ig1_baselabor).toFixed(4));
			entity.ig1_totalsellprice=Number(sellPrice.toFixed(4));
			entity.ig1_totaldirectsell=parseFloat(entity.ig1_freightsell)+parseFloat(materialCostWithMargin);
          
			
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
			updateAssociatedCost(recordId,materialCost,freight,freightsell,luExtend,associatedCostData[0], result);
		}
}

//Nazish Update Associated Cost Associated Cost Stage is selected
function updateAssociatedCost(recordId ,materialCost, freightTotal ,freightsell, luExtend,associatedCostData, result)
{
	var entity = {};
	var number_of_trip=0;
	var peaple_per_trip=0;
	var days_per_trip=0;
	var airFare=0
	var perDiem = 0;
	var lodging=0;
	
	entity.ig1_luextend = parseFloat(luExtend);
	entity.ig1_freight = Number(parseFloat(freightTotal).toFixed(4));
	entity.ig1_freightsell = Number(parseFloat(freightsell).toFixed(4));
	entity.ig1_designcost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend)*parseFloat(associatedCostData.attributes.ig1_designlaborrate.value)).toFixed(4));
	if(result.ig1_designmargin!=undefined && result.ig1_designmargin!=null && result.ig1_designmargin!="")
	{
	  entity.ig1_basedesign=Number((parseFloat(entity.ig1_designcost))*(parseFloat(result.ig1_designmargin.value)));
	}
	entity.ig1_salescost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_salesfactor.value)*parseFloat(luExtend)*parseFloat(associatedCostData.attributes.ig1_saleslaborrate.value)).toFixed(4));
	if(result.ig1_salesmargin!=undefined && result.ig1_salesmargin!=null && result.ig1_salesmargin!="")
	{
	  entity.ig1_basesales=Number((parseFloat(entity.ig1_salescost))*(parseFloat(result.ig1_salesmargin.value)));
	}
	entity.ig1_designhours = parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend);
	entity.ig1_saleshours = parseFloat(associatedCostData.attributes.ig1_designfactor.value)*parseFloat(luExtend);
	entity.ig1_pmlaborsme = parseFloat(associatedCostData.attributes.ig1_laborrate.value)*parseFloat(luExtend);
	if(result.ig1_labormargin!=undefined && result.ig1_labormargin!=null && result.ig1_labormargin!="")
	{
		entity.ig1_baselabor=parseFloat(entity.ig1_pmlaborsme)*parseFloat(result.ig1_labormargin.value);
	}
	if(associatedCostData.attributes.ig1_numberoftrip!= undefined )
	{
		if(associatedCostData.attributes.ig1_numberoftrip.value!= null)
		number_of_trip=associatedCostData.attributes.ig1_numberoftrip.value;
	}
	if(associatedCostData.attributes.ig1_peoplepertrip!= undefined  )
	{
		if(associatedCostData.attributes.ig1_peoplepertrip.value!= null)
		peaple_per_trip=associatedCostData.attributes.ig1_peoplepertrip.value;
	}
		if(associatedCostData.attributes.ig1_days!= undefined )
	{
		if(associatedCostData.attributes.ig1_days.value!= null)
		days_per_trip=associatedCostData.attributes.ig1_days.value;
	}
		if(associatedCostData.attributes.ig1_perdiem!= undefined)
	{
		if(associatedCostData.attributes.ig1_perdiem.value!= null)
		perDiem=associatedCostData.attributes.ig1_perdiem.value;
	}
		if(associatedCostData.attributes.ig1_lodgingrate!= undefined )
	{
		if(associatedCostData.attributes.ig1_lodgingrate.value!= null)
		lodging=associatedCostData.attributes.ig1_lodgingrate.value;
	}
		if(associatedCostData.attributes.ig1_airfaretrans!= undefined )
	{
		if(associatedCostData.attributes.ig1_airfaretrans.value!= null)
		airFare=associatedCostData.attributes.ig1_airfaretrans.value;
	}
	

	
	
	
	entity.ig1_travelcost = parseFloat(parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(airFare)) + ((parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(days_per_trip))*(parseFloat(perDiem)+parseFloat(lodging)));
	entity.ig1_lodgingtotal = parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(days_per_trip)*parseFloat(lodging);
	entity.ig1_perdiemtotal =  parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(days_per_trip)*parseFloat(perDiem);
	entity.ig1_transporttotal =parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(airFare);
	
	entity.ig1_materialcost = Number(parseFloat(materialCost).toFixed(4));
	entity.ig1_totaldirectcost=Number(parseFloat(parseFloat(entity.ig1_materialcost)+parseFloat(entity.ig1_freight)));
	entity.ig1_totalindirectcost=parseFloat(entity.ig1_designcost) + parseFloat(entity.ig1_pmlaborsme) + parseFloat(entity.ig1_salescost) +parseFloat(entity.ig1_travelcost);
	entity.ig1_baseindirect=Number(parseFloat(entity.ig1_basesales)+parseFloat(entity.ig1_basedesign)+parseFloat(entity.ig1_baselabor)+parseFloat(entity.ig1_travelcost));
	if(associatedCostData.attributes.ig1_margin.value != undefined && associatedCostData.attributes.ig1_margin.value != 100)
	{
		entity.ig1_totalmaterialcost = Number(parseFloat(materialCost).toFixed(4)/parseFloat(1-parseFloat(associatedCostData.attributes.ig1_margin.value)/100).toFixed(4));
		entity.ig1_totalsellprice = parseFloat(entity.ig1_designcost)+parseFloat(entity.ig1_freightsell)+parseFloat(entity.ig1_totalmaterialcost )+parseFloat(entity.ig1_pmlaborsme)+parseFloat(entity.ig1_salescost)+parseFloat(entity.ig1_travelcost);
	}
	else
	{
		entity.ig1_totalmaterialcost  = entity.ig1_materialcost; //If no margin total material cost is same as material cost
		entity.ig1_totalsellprice = parseFloat(entity.ig1_designcost)+parseFloat(entity.ig1_freightsell)+parseFloat(entity.ig1_totalmaterialcost )+parseFloat(entity.ig1_pmlaborsme)+parseFloat(entity.ig1_salescost)+parseFloat(entity.ig1_travelcost);
	}
	//entity.ig1_totalcost = Number(parseFloat(parseFloat(associatedCostData.attributes.ig1_materialcost.value)+ parseFloat(entity.ig1_freight)+parseFloat(entity.ig1_designcost)+parseFloat(entity.ig1_salescost)+parseFloat(associatedCostData.attributes.ig1_travelcost.value)+parseFloat(associatedCostData.attributes.ig1_pmlaborsme.value)).toFixed(4));
	entity.ig1_totalcost = Number((entity.ig1_materialcost +entity.ig1_designcost+entity.ig1_salescost+entity.ig1_travelcost+entity.ig1_freight+entity.ig1_pmlaborsme).toFixed(4));
	entity.ig1_totalprojecthours = parseFloat(entity.ig1_saleshours) + parseFloat(entity.ig1_designhours) + parseFloat(entity.ig1_luextend);
	entity.ig1_totaldirectsell=parseFloat(entity.ig1_freightsell)+parseFloat(entity.ig1_totalmaterialcost);
	
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
				calculateAssociatedTotalCostAndHour();
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}

function getProjectCostAllowances()
{
		var fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='ig1_projectcostallowances'>",
						"    <attribute name='ig1_salesfactor' />",
						"    <attribute name='ig1_perdiem_base' />",
						"    <attribute name='ig1_salesmargin' />",
						"    <attribute name='ig1_labormargin' />",
						"    <attribute name='ig1_corpgna_base' />",
						"    <attribute name='ig1_corpgna' />",
						"    <attribute name='ig1_designmargin' />",
						"    <attribute name='ig1_perdiem' />",
						"    <attribute name='ig1_designlaborrate' />",
						"    <attribute name='ig1_designfactor' />",
						"    <attribute name='ig1_effectivedate' />",
						"    <attribute name='ig1_saleslaborrate' />",
						"    <attribute name='ig1_designlaborrate_base' />",
						"    <attribute name='ig1_margin' />",
						"    <attribute name='ig1_lodging' />",
						"  </entity>",
						"</fetch>",
					   ].join("");
					   
     var costAllowancesData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	 if(costAllowancesData!=undefined && costAllowancesData!=null && costAllowancesData!="")
	 {
		 var result=costAllowancesData[0].attributes;
		 return result;
	 }
	 return null;
}

//Nazish - Function added to calculate PM Labor/SME
function CalculatePMCost(context)
{
	
	var result=getProjectCostAllowances();
	var control = context.getEventSource();
	var row= control.getParent();
	var laborRate=row.attributes.get("ig1_laborrate").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var pmLaborSme=parseFloat(laborRate)*parseFloat(luExtend);
	
	//var baseLabor="";
	// if(result.ig1_labormargin!=undefined && result.ig1_labormargin!=null && result.ig1_labormargin!="")
	// {
		// baseLabor=parseFloat(pmLaborSme)*parseFloat(result.ig1_labormargin.value);
		// row.attributes.get("ig1_baselabor").setValue(baseLabor);
	// }

	row.attributes.get("ig1_pmlaborsme").setValue(pmLaborSme);
	//Updating Associated Cost Entity basis the change
	UpdatePMCostRecordOnCellChange(context);
       createUpdatePMLabor();
	
	
}
//Nazish : Update Associated record entity when cell is changed
function UpdatePMCostRecordOnCellChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var entity = {};
	
	entity.ig1_laborrate = parseFloat(row.attributes.get("ig1_laborrate").getValue());
	
	entity.ig1_pmlaborsme = parseFloat(row.attributes.get("ig1_pmlaborsme").getValue());
	
	//entity.ig1_baselabor = parseFloat(row.attributes.get("ig1_baselabor").getValue());
	
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
					calculateCategoryTotalAssociatedCost(context);
				
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}


//Nazish : Change Design related on design factor change
function CalculateAssociatedDesignCost(context)
{
	var result=getProjectCostAllowances();
	var control = context.getEventSource();
	var row= control.getParent();

	var designFactor=row.attributes.get("ig1_designfactor").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var designLaborRate=row.attributes.get("ig1_designlaborrate").getValue();
	
	var designCost=parseFloat(designFactor)*parseFloat(designLaborRate)*parseFloat(luExtend);
	
	// if(result.ig1_designmargin!=undefined && result.ig1_designmargin!=null && result.ig1_designmargin!="")
	// {
	  // var baseDesign = Number((parseFloat(designCost))*(parseFloat(result.ig1_designmargin.value)));
	  // row.attributes.get("ig1_basedesign").setValue(baseDesign);
	// }
	
	var designhours = parseFloat(designFactor)*parseFloat(luExtend);
	
	
	row.attributes.get("ig1_designcost").setValue(designCost);
	row.attributes.get("ig1_designhours").setValue(designhours);
	
	
	//Updating Associated Cost Entity basis the change
	UpdateDesignCostRecordOnCellChange(context);
        //Nazish - 09-02-2019 - Update BOM SDT on design cost change
        calculateSDT();
	
}


//Nazish : Update Associated record entity when cell is changed
function UpdateDesignCostRecordOnCellChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var entity = {};
	
	entity.ig1_designfactor = parseFloat(row.attributes.get("ig1_designfactor").getValue());	
	entity.ig1_designlaborrate = parseFloat(row.attributes.get("ig1_designlaborrate").getValue());
	entity.ig1_designcost = parseFloat(row.attributes.get("ig1_designcost").getValue());
    //entity.ig1_basedesign = parseFloat(row.attributes.get("ig1_basedesign").getValue());	
	


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
					calculateCategoryTotalAssociatedCost(context);
				
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}



//Nazish: Change Sales related on sales factor change
function CalculateAssociatedSalesCost(context)
{
	var result=getProjectCostAllowances();
	var control = context.getEventSource();
	var row= control.getParent();
	var salesFactor=row.attributes.get("ig1_salesfactor").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var salesLaborRate=row.attributes.get("ig1_saleslaborrate").getValue();
	var salesCost=parseFloat(salesFactor)*parseFloat(salesLaborRate)*parseFloat(luExtend);
	var saleshours = parseFloat(salesFactor)*parseFloat(luExtend);
	
	// if(result.ig1_salesmargin!=undefined && result.ig1_salesmargin!=null && result.ig1_salesmargin!="")
	// {
	  // var bseSales = Number((parseFloat(salesCost))*(parseFloat(result.ig1_salesmargin.value)));
	  // row.attributes.get("ig1_basesales").setValue(bseSales);
	// }
	
	row.attributes.get("ig1_salescost").setValue(salesCost);
	row.attributes.get("ig1_saleshours").setValue(saleshours);
	
		
	//Updating Associated Cost Entity basis the change
	UpdateSalesCostRecordOnCellChange(context);
	//Nazish - 02-09-2019 - Updating BOM SDT on sales cost change
         calculateSDT();
}

//Nazish : Update Associated record entity when cell is changed
function UpdateSalesCostRecordOnCellChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var entity = {};
	
	entity.ig1_salesfactor = parseFloat(row.attributes.get("ig1_salesfactor").getValue());
	entity.ig1_saleslaborrate = parseFloat(row.attributes.get("ig1_saleslaborrate").getValue());
	entity.ig1_salescost = parseFloat(row.attributes.get("ig1_salescost").getValue());
    //entity.ig1_basesales = parseFloat(row.attributes.get("ig1_basesales").getValue());	
	


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
					calculateCategoryTotalAssociatedCost(context);
				
			   
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}

//Calculate Travel Cost
function CalculateAssociatedTravelCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	var number_of_trip=0;
	var peaple_per_trip=0;
	var days_per_trip=0;
	var airFare=0
	var perDiem = 0;
	var lodging=0;
	
	if(row.attributes.get("ig1_numberoftrip")!= undefined && row.attributes.get("ig1_numberoftrip").getValue()!= null)
	{
		number_of_trip=row.attributes.get("ig1_numberoftrip").getValue();
	}
	if(row.attributes.get("ig1_peoplepertrip")!= undefined && row.attributes.get("ig1_peoplepertrip").getValue()!= null)
	{
		peaple_per_trip=row.attributes.get("ig1_peoplepertrip").getValue();
	}
	if(row.attributes.get("ig1_days")!= undefined && row.attributes.get("ig1_days").getValue()!= null)
	{
		days_per_trip=row.attributes.get("ig1_days").getValue();
	}
	if(row.attributes.get("ig1_airfaretrans")!= undefined && row.attributes.get("ig1_airfaretrans").getValue()!= null)
	{
		airFare=row.attributes.get("ig1_airfaretrans").getValue();
	}
	if(row.attributes.get("ig1_perdiem")!= undefined && row.attributes.get("ig1_perdiem").getValue()!= null)
	{
		perDiem=row.attributes.get("ig1_perdiem").getValue();
	}
	if(row.attributes.get("ig1_lodgingrate")!= undefined && row.attributes.get("ig1_lodgingrate").getValue()!= null)
	{
	 lodging=row.attributes.get("ig1_lodgingrate").getValue();
	 
	}
	
	var travelCost=((parseFloat(number_of_trip))*(parseFloat(peaple_per_trip))*(parseFloat(airFare)))
	+((parseFloat(number_of_trip))*(parseFloat(peaple_per_trip))*(parseFloat(days_per_trip)))
	*((parseFloat(perDiem))+(parseFloat(lodging)));
	
	var lodgingtotal = parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(days_per_trip)*parseFloat(lodging);
	var perdiemtotal =  parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(days_per_trip)*parseFloat(perDiem);
	var transporttotal =parseFloat(number_of_trip)*parseFloat(peaple_per_trip)*parseFloat(airFare);
	
	
	 row.attributes.get("ig1_travelcost").setValue(travelCost);
	row.attributes.get("ig1_lodgingtotal").setValue(lodgingtotal);
	row.attributes.get("ig1_perdiemtotal").setValue(perdiemtotal);
	row.attributes.get("ig1_transporttotal").setValue(transporttotal);
	
	
	//Updating Associated Cost Entity basis the change
	 UpdateTravelCostRecordOnCellChange(context);
         //Nazish - 02-09-2019 - Update BOM SDT on travel cost change
          calculateSDT();
}


//Update Associated record entity when cell is changed
function UpdateTravelCostRecordOnCellChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var entity = {};
	
	entity.ig1_numberoftrip = parseFloat(row.attributes.get("ig1_numberoftrip").getValue());
	entity.ig1_peoplepertrip = parseFloat(row.attributes.get("ig1_peoplepertrip").getValue());
	entity.ig1_days = parseFloat(row.attributes.get("ig1_days").getValue());
	entity.ig1_airfaretrans = parseFloat(row.attributes.get("ig1_airfaretrans").getValue());
	entity.ig1_perdiem = parseFloat(row.attributes.get("ig1_perdiem").getValue());
	entity.ig1_lodgingrate= parseFloat(row.attributes.get("ig1_lodgingrate").getValue());
	
	
	entity.ig1_travelcost = parseFloat(row.attributes.get("ig1_travelcost").getValue());	
	entity.ig1_lodgingtotal = parseFloat(row.attributes.get("ig1_lodgingtotal").getValue());
	entity.ig1_perdiemtotal = parseFloat(row.attributes.get("ig1_perdiemtotal").getValue());
	entity.ig1_transporttotal= parseFloat(row.attributes.get("ig1_transporttotal").getValue());
	


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
					calculateCategoryTotalAssociatedCost(context);
				
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}




//Calculate Total cost for category
function calculateCategoryTotalAssociatedCost(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	var ig1_designcost ;
	var ig1_freight ;
	var ig1_totalmaterialcost ;
	var ig1_pmlaborsme;
	var ig1_salescost;
	var ig1_travelcost;
	var ig1_margin;
	var totalSellPrice;
	
	var ig1_basedesign;
	var ig1_basesales;
	var ig1_baselabor;
	var baseindirect;
	
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+recordId+")?$select=ig1_designcost,ig1_freight,ig1_freightsell,ig1_materialcost,ig1_pmlaborsme,ig1_salescost,ig1_travelcost,ig1_margin,ig1_lodgingtotal,ig1_perdiemtotal,ig1_transporttotal, ig1_basedesign, ig1_basesales, ig1_baselabor", false);
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
				 
				 ig1_freightsell = result["ig1_freightsell"];
				
				 ig1_totalmaterialcost = result["ig1_materialcost"];
				
				 ig1_pmlaborsme = result["ig1_pmlaborsme"];
				
				 ig1_salescost = result["ig1_salescost"];
				

				 ig1_travelcost = result["ig1_travelcost"];

				 ig1_perdiemtotal = result["ig1_perdiemtotal"];

				ig1_lodgingtotal = result["ig1_lodgingtotal"];

				ig1_transporttotal = result["ig1_transporttotal"];

				 ig1_margin=result["ig1_margin"];
				 
				 ig1_basedesign=result["ig1_basedesign"];
				 
				 ig1_basesales=result["ig1_basesales"];
				 
				 ig1_baselabor=result["ig1_baselabor"];
				 
				
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send();
	
	
	
	
	var totalCost = parseFloat(ig1_designcost) + parseFloat(ig1_freight) + parseFloat(ig1_totalmaterialcost) + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost);
    
    //var totalCost = parseFloat(ig1_basedesign) + parseFloat(ig1_freight) + parseFloat(ig1_totalmaterialcost) + parseFloat(ig1_baselabor) + parseFloat(ig1_basesales) +parseFloat(ig1_travelcost);

	var totalDirectCost = parseFloat(ig1_freight) + parseFloat(ig1_totalmaterialcost) ;

	var totalIndirectCost = parseFloat(ig1_designcost) + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost);
	
	var baseIndirect=parseFloat(ig1_basedesign) + parseFloat(ig1_baselabor) + parseFloat(ig1_basesales) +parseFloat(ig1_travelcost);
	
	if(ig1_margin !== 100)
	{
		var materialCostWithMargin = (parseFloat(ig1_totalmaterialcost)/parseFloat(1-parseFloat(ig1_margin)/100));
		    totalSellPrice=parseFloat(ig1_designcost)+parseFloat(ig1_freightsell)+parseFloat(materialCostWithMargin)+parseFloat(ig1_pmlaborsme)+parseFloat(ig1_salescost)+parseFloat(ig1_travelcost);
	}
	else 
	{
		totalSellPrice=parseFloat(ig1_designcost) + parseFloat(ig1_freightsell) + parseFloat(ig1_totalmaterialcost) + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost);
	}
	

	
	
	var entity = {};
	
	entity.ig1_totalcost = parseFloat(totalCost);
	entity.ig1_totaldirectcost = parseFloat(totalDirectCost);
	entity.ig1_totalindirectcost = parseFloat(totalIndirectCost);
	entity.ig1_totalsellprice=parseFloat(totalSellPrice);
	entity.ig1_baseindirect=parseFloat(baseIndirect);
	
	
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
	           calculateAssociatedTotalCostAndHour(context);
			  
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}

//Margin Change in Summary
function SummaryMarginChange(context)
{
	var control = context.getEventSource();
	var row= control.getParent();
	
	var recordId = row.getEntityReference().id.replace("{","").replace("}","");
	
	var margin=row.attributes.get("ig1_margin").getValue();
        if(margin !=0 && (margin==undefined || margin == null || margin == "" || margin>=100))
        {
          alert("Margin should not be empty and must be less than 100");
          row.attributes.get("ig1_margin").setValue(parseFloat(30));
          margin = parseFloat(30);
        }
	var ig1_designcost ;
	var ig1_freight ;
	var ig1_materialcost ;
	var ig1_pmlaborsme;
	var ig1_salescost;
	var ig1_travelcost;
	var ig1_directsell;
	
	var req = new XMLHttpRequest();
	req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_associatedcosts("+recordId+")?$select=ig1_designcost,ig1_freight,ig1_freightsell,ig1_materialcost,ig1_pmlaborsme,ig1_salescost,ig1_travelcost", false);
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
				
				 ig1_freightsell = result["ig1_freightsell"];
				 
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
	
	
	if(margin !== 100)
	{
	//var totalMaterialCost = Number(parseFloat(row.attributes.get("ig1_materialcost").getValue())/parseFloat(1-parseFloat(margin)/100));
	var totalMaterialCost = Number(parseFloat(ig1_materialcost)/parseFloat(1-parseFloat(margin)/100));
	}
	
	var totalSellPrice= parseFloat(ig1_designcost)  + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost) + parseFloat(ig1_freightsell) + parseFloat(totalMaterialCost);
	var totalDirectCost = parseFloat(ig1_freight) + parseFloat(ig1_materialcost) ;
	var totalIndirectCost = parseFloat(ig1_designcost) + parseFloat(ig1_pmlaborsme) + parseFloat(ig1_salescost) +parseFloat(ig1_travelcost);
	var totalDirectSell=parseFloat(ig1_freightsell)+parseFloat(totalMaterialCost);
	row.attributes.get("ig1_totalsellprice").setValue(totalSellPrice);
	row.attributes.get("ig1_totaldirectsell").setValue(totalDirectCost);
	row.attributes.get("ig1_totalindirectcost").setValue(totalIndirectCost);
	
	
	
	var entity = {};
	
	
	entity.ig1_totalmaterialcost = parseFloat(totalMaterialCost);
	
	entity.ig1_totalsellprice= parseFloat(totalSellPrice);
	entity.ig1_totaldirectcost = parseFloat(totalDirectCost);
	entity.ig1_totalindirectcost = parseFloat(totalIndirectCost);
	entity.ig1_totaldirectsell=parseFloat(totalDirectSell);
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
	                calculateAssociatedTotalCostAndHour(context);
                    projectFinanceProjection();
			        RefreshTotalsGrid(executionContext);
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
	
	
}



// Calculate Total Cost and Hour of BidSheet 
function calculateAssociatedTotalCostAndHour(executionContext)
{
 debugger;
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
			
			var	totalCost=0;
			var projectHour=0;
			
			var	materialCost=0;
			var	feightCost=0;
			
			
			var	directCost=0;
			
			var pmCost=0;
			var	pmHours=0
			
			var designCost=0;
			var	designHours=0;
			
			var salesCost=0;
			var	salesHours=0;
			
			var transCost = 0;
			var perdiem= 0;
			var lodging = 0;
			var travelCost=0;
			
			var	indirectCost=0;
			var sellprice = 0;
			
			var baseDesign=0;
			var baseSales=0;
			var baseLabor=0;
			var baseIndirect=0;
			
			for(i=0; i<fetchXmlData.length; i++)
			{
				result=fetchXmlData[i].attributes;
				
								
				projectHour+=parseFloat(result.ig1_totalprojecthours.value);
				
				materialCost+=parseFloat(result.ig1_materialcost.value);
				feightCost+=parseFloat(result.ig1_freight.value);
				directCost+=parseFloat(result.ig1_totaldirectcost.value);
				
				if(result.ig1_totalsellprice != undefined)
				sellprice+=parseFloat(result.ig1_totalsellprice.value);
				
				pmCost+=parseFloat(result.ig1_pmlaborsme.value);
				pmHours+=parseFloat(result.ig1_labourhours.value);

				designCost+=parseFloat(result.ig1_designcost.value);
				designHours+=parseFloat(result.ig1_designhours.value);				
				
				salesCost+=parseFloat(result.ig1_salescost.value);
				salesHours+=parseFloat(result.ig1_saleshours.value);	
				if(result.ig1_transporttotal != undefined)
				transCost+=parseFloat(result.ig1_transporttotal.value);
			
			    if(result.ig1_perdiemtotal != undefined)
				perdiem+=parseFloat(result.ig1_perdiemtotal.value);
			
				if(result.ig1_lodgingtotal != undefined)
				{
				lodging+=parseFloat(result.ig1_lodgingtotal.value);
				}
				travelCost+=parseFloat(result.ig1_travelcost.value);
				
				indirectCost+=parseFloat(result.ig1_totalindirectcost.value);
			     
				if(result.ig1_basedesign!=undefined && result.ig1_basedesign!=null && result.ig1_basedesign!="")
				{
					baseDesign+=parseFloat(result.ig1_basedesign.value);
				}
				if(result.ig1_basesales!=undefined && result.ig1_basesales!=null && result.ig1_basesales!="")
				{
					baseSales+=parseFloat(result.ig1_basesales.value);
				}
				if(result.ig1_baselabor!=undefined && result.ig1_baselabor!=null && result.ig1_baselabor!="")
				{
					baseLabor+=parseFloat(result.ig1_baselabor.value);
				}
				if(result.ig1_baseindirect!=undefined && result.ig1_baseindirect!=null && result.ig1_baseindirect!="")
				{
					baseIndirect+=parseFloat(result.ig1_baseindirect.value);
				}
			}
			totalCost = directCost + indirectCost;
			//totalCost = directCost + baseIndirect;
                        Xrm.Page.getAttribute("ig1_totalhours").setValue(projectHour);
			Xrm.Page.getAttribute("ig1_totalcost").setValue(totalCost);
			
			Xrm.Page.getAttribute("ig1_materialcost").setValue(materialCost);
			Xrm.Page.getAttribute("ig1_freightamount").setValue(feightCost);
			Xrm.Page.getAttribute("ig1_directcost").setValue(directCost);
			Xrm.Page.getAttribute("ig1_indirectcost").setValue(indirectCost);
			
			Xrm.Page.getAttribute("ig1_pmcost").setValue(pmCost);
			Xrm.Page.getAttribute("ig1_pmhours").setValue(pmHours);
			
			Xrm.Page.getAttribute("ig1_designcost").setValue(designCost);			
			Xrm.Page.getAttribute("ig1_designhours").setValue(designHours);
			
			Xrm.Page.getAttribute("ig1_salescost").setValue(salesCost);
			Xrm.Page.getAttribute("ig1_saleshours").setValue(salesHours);
			
			Xrm.Page.getAttribute("ig1_perdiem").setValue(perdiem);
			Xrm.Page.getAttribute("ig1_lodgingtotal").setValue(lodging);
			Xrm.Page.getAttribute("ig1_transtotal").setValue(transCost);
			Xrm.Page.getAttribute("ig1_totaltravel").setValue(travelCost);
			 
                        var contingencyCostMaterialCost=getContingencyMaterialCost();
                        var sellPriceWithContingency=parseFloat(contingencyCostMaterialCost)+parseFloat(sellprice);
                        Xrm.Page.getAttribute("ig1_sellprice").setValue(sellPriceWithContingency);
		}
                else
		{
			Xrm.Page.getAttribute("ig1_totalhours").setValue(0);
			Xrm.Page.getAttribute("ig1_totalcost").setValue(0);
			
			Xrm.Page.getAttribute("ig1_materialcost").setValue(0);
			Xrm.Page.getAttribute("ig1_freightamount").setValue(0);
			Xrm.Page.getAttribute("ig1_directcost").setValue(0);
			
			Xrm.Page.getAttribute("ig1_pmcost").setValue(0);
			Xrm.Page.getAttribute("ig1_pmhours").setValue(0);
			
			Xrm.Page.getAttribute("ig1_designcost").setValue(0);			
			Xrm.Page.getAttribute("ig1_designhours").setValue(0);
			
			Xrm.Page.getAttribute("ig1_salescost").setValue(0);
			Xrm.Page.getAttribute("ig1_saleshours").setValue(0);
			
			Xrm.Page.getAttribute("ig1_perdiem").setValue(0);
			Xrm.Page.getAttribute("ig1_lodgingtotal").setValue(0);
			Xrm.Page.getAttribute("ig1_transtotal").setValue(0);
			Xrm.Page.getAttribute("ig1_totaltravel").setValue(0);
			Xrm.Page.getAttribute("ig1_sellprice").setValue(0);
		}
		setTimeout(function(){Xrm.Page.data.save();}, 2000);
		if(executionContext != undefined || executionContext != null)
		RefreshTotalsGrid(executionContext) ;
}

//Disable cells in PM Cost Grid
function disbalePMCostsGridCells(context)
{
        var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			
			|| field._attributeName==="ig1_luextend" 
			|| field._attributeName==="ig1_pmlaborsme" 
			|| field._attributeName==="ig1_labourhours"
			)
                        || (bidSheetStatus!="286150001"	&& bidSheetStatus!="286150003")		
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_bidsheetcategory" 
			
			|| field.getName()==="ig1_luextend" 
			|| field.getName()==="ig1_pmlaborsme" 
			|| field._attributeName==="ig1_labourhours"
			) 
                        || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Disable cells in PM Cost Grid
function disbaleDirectCostsGridCells(context)
{
        var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			
			|| field._attributeName==="ig1_materialcost" 
			|| field._attributeName==="ig1_totalmaterialcost" 
			|| field._attributeName==="ig1_freight" 
			|| field._attributeName==="ig1_totaldirectcost"
			)
                         || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")			
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_bidsheetcategory" 
			
			|| field.getName()==="ig1_materialcost" 
			|| field.getName()==="ig1_totalmaterialcost" 
			|| field._attributeName==="ig1_freight" 
			|| field._attributeName==="ig1_totaldirectcost"
			) 
                       || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Disable cells in Design Cost Grid
function disableDesignCostsGridCells(context)
{
        var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			|| field._attributeName ==="ig1_designcost" 
			
			|| field._attributeName==="ig1_luextend" 
			|| field._attributeName==="ig1_name" 
			
            )		
                        || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&&field._attributeName!==undefined 
			||(field.getName()==="ig1_bidsheetcategory" 
			|| field.getName()==="ig1_designcost" 
			
			|| field.getName()==="ig1_luextend" 
			|| field._attributeName==="ig1_name" 
			) 
                        || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Disable cells in Sales Cost Grid
function disableSalesCostsGridCells(context)
{
        var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			|| field._attributeName==="ig1_luextend"
			|| field._attributeName === "ig1_saleshours"
			|| field._attributeName === "ig1_salescost"
			)
                        || (bidSheetStatus!="286150001"	&& bidSheetStatus!="286150003")		
			&&field._attributeName!==undefined 
			||(field.getName ==="ig1_bidsheetcategory" 
			|| field.getName==="ig1_luextend"
			|| field.getName === "ig1_saleshours"
			|| field.getName === "ig1_salescost"
			) 
                        || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Disable cells in Travel Cost Grid
function disableTravelCostsGridCells(context)
{
        var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
            if((field._attributeName ==="ig1_bidsheetcategory" 
			|| field._attributeName==="ig1_travelcost"
			|| field._attributeName==="ig1_perdiemtotal" 
						|| field._attributeName==="ig1_lodgingtotal" 
						|| field._attributeName==="ig1_transporttotal" 
			)
                       || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")		
			&&field._attributeName!==undefined 
			||(field.getName ==="ig1_bidsheetcategory" 
			|| field.getName==="ig1_travelcost"
			|| field._attributeName==="ig1_perdiemtotal" 
						|| field._attributeName==="ig1_lodgingtotal" 
						|| field._attributeName==="ig1_transporttotal" 
			) 
                         || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

//Refresh the totals grid and Save bidsheet totals
function RefreshTotalsGrid(executionContext) 
{
	
	  var formContext = executionContext.getFormContext();

   formContext.data.save();
       setTimeout(function(){ValidateSelectedStage();}, 2000);
    

    var grid = Xrm.Page.getControl('TotalCost');
    if (grid == null) {
        setTimeout(function () { RefreshTotalsGrid(); }, 2000);
        //return;
    }
    else
    {
      setTimeout(function () { Xrm.Page.data.refresh(); }, 2000);
     
    }
}

//Disable cells in Design Cost Grid
function disableBidSheetLineItemSubGridCells(context)
{
	var formContext = context.getFormContext().data.entity;
        formContext.attributes.forEach(function(field, i) 
		{
           
				field.controls.get(0).setDisabled(true);
			
        });
}


function disableAssociatedCostAndSummaryGridCells(context)
{
        debugger;
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
            || field._attributeName==="ig1_totalsellprice"
			|| field._attributeName==="ig1_totaldirectsell"
			|| field._attributeName==="ig1_totalindirectcost"
            || field._attributeName==="ig1_totalcost")
            || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
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
			|| field.getName()==="ig1_totalcost"
			|| field.getName()==="ig1_totaldirectsell"
			|| field.getName()==="ig1_totalindirectcost"
            || field.getName()==="ig1_totalsellprice") 
            || (bidSheetStatus!="286150001" && bidSheetStatus!="286150003")
			&& field.getName()!==undefined) 
			{
				field.controls.get(0).setDisabled(true);
			}
        });
}

function displayIconTooltip(rowData, userLCID) 
{
   var imgName = "/WebResources/ig1_Editable_Fields_Icon";
   var tooltip = "Edit"; 
   var resultarray = [imgName, tooltip]; 
   return resultarray;  
 }  

function recalculateIndirectCost(formContext)
{
        Xrm.Utility.showProgressIndicator("Calculating Indirect Cost. Please Wait...");
	debugger;
	var bidsheetid = formContext.data.entity.getId().replace("{", "").replace("}", "");
	var parameters = {};
	parameters.bidsheetid = bidsheetid;

	var req = new XMLHttpRequest();
	req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_ReCalculateIndirectCost", false);
	req.setRequestHeader("OData-MaxVersion", "4.0");
	req.setRequestHeader("OData-Version", "4.0");
	req.setRequestHeader("Accept", "application/json");
	req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	req.onreadystatechange = function() {
		if (this.readyState === 4) {
			req.onreadystatechange = null;
			if (this.status === 204) {
				//Success - No Return Data - Do Something
                                 formContext.ui.clearFormNotification();
                                 formContext.data.refresh();
                                 setTimeout(function(){Xrm.Utility.closeProgressIndicator();}, 2000);
			} else {
                                Xrm.Utility.closeProgressIndicator();
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(parameters));
}

function saveIndirectCostGrids(context)
{
        debugger;
	var formContext = context.getFormContext();
	var control = context.getEventSource();
	var row= control.getParent();
	var recordId = row.getEntityReference().id.replace('{','').replace('}','');
	var entity = {};
	if(row.attributes.get("ig1_designfactor")!=undefined && row.attributes.get("ig1_designfactor")!=null && row.attributes.get("ig1_designfactor")!="")
	{
		entity.ig1_designfactor = parseFloat(row.attributes.get("ig1_designfactor").getValue());
	}
	if(row.attributes.get("ig1_designlaborrate")!=undefined && row.attributes.get("ig1_designlaborrate")!=null && row.attributes.get("ig1_designlaborrate")!="")
	{
		entity.ig1_designlaborrate = parseFloat(row.attributes.get("ig1_designlaborrate").getValue());
	}
	if(row.attributes.get("ig1_salesfactor")!=undefined && row.attributes.get("ig1_salesfactor")!=null && row.attributes.get("ig1_salesfactor")!="")
	{
		entity.ig1_salesfactor = parseFloat(row.attributes.get("ig1_salesfactor").getValue());
	}
	if(row.attributes.get("ig1_saleslaborrate")!=undefined && row.attributes.get("ig1_saleslaborrate")!=null && row.attributes.get("ig1_saleslaborrate")!="")
	{
		entity.ig1_saleslaborrate = parseFloat(row.attributes.get("ig1_saleslaborrate").getValue());
	}
	if(row.attributes.get("ig1_laborrate")!=undefined && row.attributes.get("ig1_laborrate")!=null && row.attributes.get("ig1_laborrate")!="")
	{
		entity.ig1_laborrate = parseFloat(row.attributes.get("ig1_laborrate").getValue());;
	}
	if(row.attributes.get("ig1_numberoftrip")!=undefined && row.attributes.get("ig1_numberoftrip")!=null && row.attributes.get("ig1_numberoftrip")!="")
	{
		entity.ig1_numberoftrip = parseFloat(row.attributes.get("ig1_numberoftrip").getValue());
	}
	if(row.attributes.get("ig1_peoplepertrip")!=undefined && row.attributes.get("ig1_peoplepertrip")!=null && row.attributes.get("ig1_peoplepertrip")!="")
	{
		entity.ig1_peoplepertrip = parseFloat(row.attributes.get("ig1_peoplepertrip").getValue());
	}
	if(row.attributes.get("ig1_days")!=undefined && row.attributes.get("ig1_days")!=null && row.attributes.get("ig1_days")!="")
	{
		entity.ig1_days = parseFloat(row.attributes.get("ig1_days").getValue());;
	}
	if(row.attributes.get("ig1_airfaretrans")!=undefined && row.attributes.get("ig1_airfaretrans")!=null && row.attributes.get("ig1_airfaretrans")!="")
	{
		entity.ig1_airfaretrans = parseFloat(row.attributes.get("ig1_airfaretrans").getValue());;
	}
	if(row.attributes.get("ig1_perdiem")!=undefined && row.attributes.get("ig1_perdiem")!=null && row.attributes.get("ig1_perdiem")!="")
	{
		entity.ig1_perdiem = parseFloat(row.attributes.get("ig1_perdiem").getValue());
	}
	if(row.attributes.get("ig1_lodgingrate")!=undefined && row.attributes.get("ig1_lodgingrate")!=null && row.attributes.get("ig1_lodgingrate")!="")
	{
		entity.ig1_lodgingrate = parseFloat(row.attributes.get("ig1_lodgingrate").getValue());
	}
       if(row.attributes.get("ig1_margin")!=undefined && row.attributes.get("ig1_margin") !=null && row.attributes.get("ig1_margin")!="")
	{
		entity.ig1_margin = parseFloat(row.attributes.get("ig1_margin").getValue());
	}
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
				//Success - No Return Data - Do Something
				  RefreshBOMGrid(context);
                                  formContext.ui.clearFormNotification();
			} else {
				Xrm.Utility.alertDialog(this.statusText);
			}
		}
	};
	req.send(JSON.stringify(entity));
}