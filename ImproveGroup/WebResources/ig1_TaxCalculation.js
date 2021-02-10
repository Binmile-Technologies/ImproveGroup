var hold_request="";
function calculateQuoteTax()
{
	debugger;
	Xrm.Utility.showProgressIndicator("Processing...");
	var recordData=getRecordDetails();
	var recordDetails=recordData.recordDetails;
	var lines=recordData.lines;
	var data=requestBody(lines);
	hold_request=data;
	if(!data)
    {
       Xrm.Utility.closeProgressIndicator();
	   return;
    }
	var config = avalaraConfig();
	
	$.ajax({
        type: "POST",
		url: "https://igimprovegroupwebapi.azurewebsites.net/api/CalculateTax?url="+config.apiUrl+"&auth="+config.authorization,
        contentType: "application/json",
        processData: false,
		data: data,
		success: function (result) 
		{
			
		  var responseData=result;
		  for(i=0; i<recordDetails.length; i++)
		  {
			var line=responseData["lines"][i];
			var entity = {};
				entity.tax = parseFloat(line.tax);
				Xrm.WebApi.online.updateRecord("quotedetail", recordDetails[i], entity).then(
					function success(result) 
					{
						var updatedEntityId = result.id;
					},
					function(error) 
					{
						Xrm.Utility.alertDialog(error.message);
					});
		  }
		  var freightIndex=responseData["lines"].length-1;
		  var freightTax=parseFloat(responseData["lines"][freightIndex].tax);
		  Xrm.Page.getAttribute("ig1_freighttax").setValue(freightTax);
          Xrm.Page.data.save().then(function() {Xrm.Page.data.refresh();});
		  //createTaxInformation(hold_request, this.responseText);
		  Xrm.Utility.closeProgressIndicator();
        },
        error: function (xhr, status, error)
		{
		  Xrm.Utility.closeProgressIndicator();
		  var err =  xhr.response;
		  alert(err);
		} 
    });
	
}

function calculateInvoiceTax()
{
	debugger;
    Xrm.Utility.showProgressIndicator("Processing...");
	var recordData=getRecordDetails();
	var recordDetails=recordData.recordDetails;
	var lines=recordData.lines;
	var data=requestBody(lines);
        hold_request=data;
	if(!data)
    {
      Xrm.Utility.closeProgressIndicator();
	  return;
    }
       var config = avalaraConfig();
	$.ajax({
        type: "POST",
		url: "https://igimprovegroupwebapi.azurewebsites.net/api/CalculateTax?url="+config.apiUrl+"&auth="+config.authorization,
        contentType: "application/json",
        processData: false,
		data: data,
		success: function (result) 
		{
		  var responseData=result;
		  for(i=0; i<recordDetails.length; i++)
		  {
			var line=responseData["lines"][i];
			var entity = {};
				entity.tax = parseFloat(line.tax);
				Xrm.WebApi.online.updateRecord("invoicedetail", recordDetails[i], entity).then(
					function success(result) 
					{
						var updatedEntityId = result.id;
					},
					function(error) 
					{
						Xrm.Utility.alertDialog(error.message);
					});
		  }
		  var freightIndex=responseData["lines"].length-1;
		  var freightTax=parseFloat(responseData["lines"][freightIndex].tax);
		  Xrm.Page.getAttribute("ig1_freighttax").setValue(freightTax);
          Xrm.Page.data.save().then(function() {Xrm.Page.data.refresh();});
          //createTaxInformation(hold_request, this.responseText);
	      Xrm.Utility.closeProgressIndicator();
        },
        error: function (xhr, status, error)
		{
		  Xrm.Utility.closeProgressIndicator();
		  var err =  xhr.response;
		  alert(err);
		} 
    });
}

function getRecordDetails()
{
	var lines=[];
	var recordDetails=[];
	var number=0;
	var entityName = Xrm.Page.data.entity.getEntityName();
	var currentRecordid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
	var fetchData = {msdyn_taxable: "1", currentRecordid: currentRecordid};
	var fetchXml="";
	 if(entityName=="quote")
	 {
		fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='product'>",
						"    <attribute name='ig1_taxcode' alias='taxCode' />",
						"    <attribute name='name' alias='description' />",
						"    <attribute name='productnumber' alias='itemCode' />",
						"    <filter type='and'>",
						"      <condition attribute='msdyn_taxable' operator='eq' value='", fetchData.msdyn_taxable/*1*/, "'/>",
						"    </filter>",
						"    <link-entity name='quotedetail' from='productid' to='productid'>",
						"      <attribute name='quotedetailid' alias='recordId'/>",
						"      <attribute name='quantity' alias='quantity'/>",
						"      <attribute name='baseamount' alias='amount' />",
						"      <filter type='and'>",
						"        <condition attribute='quoteid' operator='eq' value='", fetchData.currentRecordid/*DDF180EC-5ADA-E911-A960-000D3A1D5D58*/, "'/>",
						"      </filter>",
						"    </link-entity>",
						"  </entity>",
						"</fetch>",
					   ].join("");
	 }
	 
	 else if(entityName=="invoice")
	 {
		var fetchXml = [
						"<fetch mapping='logical' version='1.0'>",
						"  <entity name='product'>",
						"    <attribute name='ig1_taxcode' alias='taxCode' />",
						"    <attribute name='name' alias='description' />",
						"    <attribute name='productnumber' alias='itemCode' />",
						"    <filter type='and'>",
						"      <condition attribute='msdyn_taxable' operator='eq' value='", fetchData.msdyn_taxable/*1*/, "'/>",
						"    </filter>",
						"    <link-entity name='invoicedetail' from='productid' to='productid'>",
						"      <attribute name='invoicedetailid' alias='recordId' />",
						"      <attribute name='quantity' alias='quantity' />",
						"      <attribute name='baseamount' alias='amount' />",
						"      <filter type='and'>",
						"        <condition attribute='invoiceid' operator='eq' value='", fetchData.currentRecordid/*fdaf47ec-a267-4280-841f-3b657a74e19f*/, "'/>",
						"      </filter>",
						"    </link-entity>",
						"  </entity>",
						"</fetch>",
					   ].join("");
	 }
	 
    var getData=executeFetchXml("products", fetchXml);
	
	if(getData.value!=undefined && getData.value!=null && getData.value!="" && getData.value.length>0)
	{
	  for(i=0; i<getData.value.length; i++)
	  {
		  var result=getData.value[i];
		  number=i+1;
          var qty = 1;
		  var lineItem={};
		  lineItem["number"]=number;
		  if(result.quantity!=undefined && result.quantity!=null && result.quantity!="")
			lineItem["quantity"]=parseInt(result.quantity);
		
		  if(result.amount!=undefined && result.amount!=null && result.amount!="")
			  lineItem["amount"]=parseFloat(result.amount);
		  
		  if(result.taxCode!=undefined && result.taxCode!=null && result.taxCode!="")
			  lineItem["taxCode"]=result.taxCode;
		  
		  if(result.itemCode!=undefined && result.itemCode!=null && result.itemCode!="")
			  lineItem["itemCode"]=result.itemCode;
		  
		  if(result.description!=undefined && result.description!=null && result.description!="")
			  lineItem["description"]=result.description;
		  
		  if(result.recordId!=undefined && result.recordId!=null && result.recordId!="")
			  recordDetails.push(result.recordId.replace("{", "").replace("}", ""));
		  
		  lines.push(lineItem);
	  }
	  if(Xrm.Page.getAttribute("ig1_freightamount")!=undefined && Xrm.Page.getAttribute("ig1_freightamount")!=null && Xrm.Page.getAttribute("ig1_freightamount")!="")
	  {
		  var freightAmount = 0;
		  if(parseFloat(Xrm.Page.getAttribute("ig1_freightamount").getValue())>0)
		  {
			freightAmount = Xrm.Page.getAttribute("ig1_freightamount").getValue();
		  }
          var freightLineItem={};
		  freightLineItem["number"]=number+1;
		  freightLineItem["quantity"]=parseInt(1);
		  freightLineItem["amount"]=parseFloat(freightAmount);
		  freightLineItem["taxCode"]="FR020100";
		  freightLineItem["itemCode"]="Shipping";
		  lines.push(freightLineItem);
	   }
	}
  return {"lines":lines,"recordDetails":recordDetails}
}


function requestBody(lines)
{
	var type="";
	var install_ship="";
	var entityName=Xrm.Page.data.entity.getEntityName();
	if(entityName=="quote")
	{
		type="SalesOrder";
		install_ship="Install";
	}
	else if(entityName=="invoice")
	{
		type="SalesInvoice";
		install_ship="Ship";
	}
	
	//Fetching the shipFrom Address...
	var config = avalaraConfig();
	var shipFrom={
		  "line1": config.shipFromLine1,
		  "city": config.shipFromCity,
		  "region": config.shipFromRegion,
		  "country": config.shipFromCountry,
		  "postalCode": config.shipFromPostalCode
		}
	//Fetching the shipTo Address...
	var shipToLine1="";
	var shipToCity="";
	var shipToRegion="";
	var shipToCountry="";
	var shipToPostalCode="";
	
	if(Xrm.Page.getAttribute("shipto_line1").getValue()!=undefined && Xrm.Page.getAttribute("shipto_line1").getValue()!=null && Xrm.Page.getAttribute("shipto_line1").getValue()!="")
	{
		shipToLine1=Xrm.Page.getAttribute("shipto_line1").getValue();
	}
	if(Xrm.Page.getAttribute("shipto_city").getValue()!=undefined && Xrm.Page.getAttribute("shipto_city").getValue()!=null && Xrm.Page.getAttribute("shipto_city").getValue()!="")
	{
		shipToCity=Xrm.Page.getAttribute("shipto_city").getValue();
	}
	else
	{
		alert("Please Enter "+install_ship+" City");
		return false;
	}
	if(Xrm.Page.getAttribute("shipto_stateorprovince").getValue()!=undefined && Xrm.Page.getAttribute("shipto_stateorprovince").getValue()!=null && Xrm.Page.getAttribute("shipto_stateorprovince").getValue()!="")
	{
		shipToRegion=Xrm.Page.getAttribute("shipto_stateorprovince").getValue();
	}
	else
	{
		alert("Please Enter "+install_ship+" State/Province");
		return false;
	}
	if(Xrm.Page.getAttribute("shipto_country").getValue()!=undefined && Xrm.Page.getAttribute("shipto_country").getValue()!=null && Xrm.Page.getAttribute("shipto_country").getValue()!="")
	{
		shipToCountry=Xrm.Page.getAttribute("shipto_country").getValue();
	}
	else
	{
		alert("Please Enter "+install_ship+" Country");
		return false;
	}
	if(Xrm.Page.getAttribute("shipto_postalcode").getValue()!=undefined && Xrm.Page.getAttribute("shipto_postalcode").getValue()!=null && Xrm.Page.getAttribute("shipto_postalcode").getValue()!="")
	{
		shipToPostalCode=Xrm.Page.getAttribute("shipto_postalcode").getValue();
	}
	else
	{
		alert("Please Enter "+install_ship+" Postal Code");
		return false;
	}
	
	var shipTo={
				"line1": shipToLine1,
				"city": shipToCity,
				"region": shipToRegion,
				"country": shipToCountry,
				"postalCode": shipToPostalCode
			   }
			   
	var opportunitydetails=getOpportunityDetails();
	var date=new Date();
	var data = JSON.stringify({
	  "lines": lines,
	  "type": type,
	  "companyCode": config.companyCode,
	  "date": date,
	  "customerCode": opportunitydetails.customerCode,
	  "entityUseCode" : opportunitydetails.entityUseCode,
	  "commit": true,
	  "currencyCode": config.currencyCode,
	  "description": opportunitydetails.description,
	  "addresses": {
		// "line1": "123 Jones St",
		// "city": "San Antonio",
		// "region": "TX",
		// "country": "USA",
		// "postalCode": "78236",
		"shipFrom": shipFrom,
		"shipTo": shipTo,
		"pointOfOrderOrigin": shipTo,
		"pointOfOrderAcceptance": shipTo
	  }
	});
	
	return data;
}

function getOpportunityDetails()
{
	var fetchXml="";
	var account="";
	var useCode="";
	var description="";
	var entityName=Xrm.Page.data.entity.getEntityName();
	var currentRecordid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", ""); 
		var fetchData = {
		currentRecordid: currentRecordid
	};
	
	 if(entityName=="quote")
	 {
		fetchXml= [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='quote'>",
					"    <filter type='and'>",
					"      <condition attribute='quoteid' operator='eq' value='", fetchData.currentRecordid/*2c4d860f-19ef-e911-a964-000d3a1d5d22*/, "'/>",
					"    </filter>",
					"    <link-entity name='opportunity' from='opportunityid' to='opportunityid'>",
					"      <attribute name='parentaccountid' alias='account' />",
					"      <attribute name='ig1_entityusecode' alias='entityUseCode' />",
					"      <attribute name='proposedsolution' alias='description' />",
					"    </link-entity>",
					"  </entity>",
					"</fetch>",
				   ].join("");
	 }
	 else if(entityName=="invoice")
	 {
		 fetchXml = [
					"<fetch mapping='logical' version='1.0'>",
					"  <entity name='invoice'>",
					"    <filter type='and'>",
					"      <condition attribute='invoiceid' operator='eq' value='", fetchData.currentRecordid/*407cdaf7-64e4-e911-a960-000d3a110bbd*/, "'/>",
					"    </filter>",
					"    <link-entity name='opportunity' from='opportunityid' to='opportunityid'>",
					"	   <attribute name='parentaccountid' alias='account' />",
					"      <attribute name='ig1_entityusecode' alias='entityUseCode' />",
					"      <attribute name='proposedsolution' alias='description' />",
					"    </link-entity>",
					"  </entity>",
					"</fetch>",
				   ].join("");
	 }
	 
     //var opportunityData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	 var opportunityData=executeFetchXml(entityName+"s", fetchXml);
	 if(opportunityData.value!=undefined && opportunityData.value!=null && opportunityData.value!="" && opportunityData.value.length>0)
	 {
			 result=opportunityData.value[0];
			 if(result.account!=undefined && result.account!=null && result.account!="")
			 {
				account=result.account.replace("{", "").replace("}", ""); 
			 }
			 if(result.entityUseCode!=undefined && result.entityUseCode!=null && result.entityUseCode!="")
			 {
				 useCode=result.entityUseCode;
			 }
			 if(result.description!=undefined && result.description!=null && result.description!="")
			 {
				 description=result.description;
			 }
	 }
	
	return{customerCode:account, entityUseCode:useCode, description:description};
}

function createTaxInformation(request, response)
{

	var entityName=Xrm.Page.data.entity.getEntityName();
	var currentRecordid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
	var entity = {};
	if(entityName=="invoice")
	{
		var entity = {};
		entity.ig1_name = entityName;
		entity["ig1_Invoice@odata.bind"] = "/invoices("+currentRecordid+")";
		entity.ig1_request = request;
		entity.ig1_response = response;

		Xrm.WebApi.online.createRecord("ig1_taxinformation", entity).then(
			function success(result) {
				var newEntityId = result.id;
			},
			function(error) {
				Xrm.Utility.alertDialog(error.message);
			}
		);
	}
	else if(entityName=="quote")
	{
		var entity = {};
		entity.ig1_name = entityName;
		entity["ig1_quote@odata.bind"] = "/quotes("+currentRecordid+")";
		entity.ig1_request = request;
		entity.ig1_response = response;

		Xrm.WebApi.online.createRecord("ig1_taxinformation", entity).then(
			function success(result) {
				var newEntityId = result.id;
			},
			function(error) {
				Xrm.Utility.alertDialog(error.message);
			}
		);
	}
}


function avalaraConfig()
{
	var apiUrl="";
	var authorization="";
	var currencyCode="";
	var companyCode="";
	var shipFromLine1="";
	var shipFromCity="";
	var shipFromRegion="";
	var shipFromCountry="";
	var shipFromPostalCode="";
	
	var req = new XMLHttpRequest();
		req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_AvalaraConfiguration", false);
		req.setRequestHeader("OData-MaxVersion", "4.0");
		req.setRequestHeader("OData-Version", "4.0");
		req.setRequestHeader("Accept", "application/json");
		req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
		req.onreadystatechange = function() {
			if (this.readyState === 4) {
				req.onreadystatechange = null;
				if (this.status === 200) 
				{
					var result = JSON.parse(this.response);
					if(result!=undefined && result!=null)
					{
					   if(result.ApiUrl!=undefined && result.ApiUrl!=null && result.ApiUrl!="")
					   {
						   apiUrl=result.ApiUrl;
					   }
					   if(result.Authorization!=undefined && result.Authorization!=null && result.Authorization!="")
					   {
						   authorization=result.Authorization;
					   }
					   if(result.CurrencyCode!=undefined && result.CurrencyCode!=null && result.CurrencyCode!="")
					   {
						   currencyCode=result.CurrencyCode;
					   }
					   if(result.CompanyCode!=undefined && result.CompanyCode!=null && result.CompanyCode!="")
					   {
						   companyCode=result.CompanyCode;
					   }
					   if(result.ShipFromLine1!=undefined && result.ShipFromLine1!=null && result.ShipFromLine1!="")
					   {
						   shipFromLine1=result.ShipFromLine1;
					   }
					   if(result.ShipFromCity!=undefined && result.ShipFromCity!=null && result.ShipFromCity!="")
					   {
						   shipFromCity=result.ShipFromCity;
					   }
					   if(result.ShipFromRegion!=undefined && result.ShipFromRegion!=null && result.ShipFromRegion!="")
					   {
						   shipFromRegion=result.ShipFromRegion;
					   }
					   if(result.ShipFromCountry!=undefined && result.ShipFromCountry!=null && result.ShipFromCountry!="")
					   {
						   shipFromCountry=result.ShipFromCountry;
					   }
					   if(result.ShipFromPostalCode!=undefined && result.ShipFromPostalCode!=null && result.ShipFromPostalCode!="")
					   {
						   shipFromPostalCode=result.ShipFromPostalCode;
					   }
					}
				} 
				else 
				{
					alert(this.statusText);
				}
			}
		};
		req.send();
		
		return{
			"apiUrl": apiUrl, 
			"authorization": authorization, 
			"currencyCode":currencyCode, 
			"companyCode":companyCode, 
			"shipFromLine1":shipFromLine1, 
			"shipFromCity":shipFromCity,
			"shipFromRegion": shipFromRegion,
			"shipFromCountry":shipFromCountry,
			"shipFromPostalCode":shipFromPostalCode
		}
}