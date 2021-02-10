function projectFinanceProjection()
{
	var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
	var fetchData = {
		ig1_bidsheetid: bidSheetId
	};

	
		
			var fetchXml = [
		"<fetch mapping='logical' version='1.0'>",
		"  <entity name='ig1_bidsheet'>",
		"    <attribute name='ig1_sellprice' />",
		"    <attribute name='ig1_indirectcost' />",
		"    <attribute name='ig1_directcost' />",
		"    <attribute name='ig1_baseindirect' />",
		"    <attribute name='ig1_pmcost' />",
		"    <attribute name='ig1_totaltravel' />",
		"    <attribute name='ig1_salescost' />",
		"    <attribute name='ig1_basedesign' />",
		"    <attribute name='ig1_basesales' />",
		"    <attribute name='ig1_designcost' />",
		"    <filter type='and'>",
		"      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*fdba28d2-6ada-4c3a-ae31-68105a1c07fd*/, "'/>",
		"    </filter>",
		"  </entity>",
		"</fetch>",
			].join("");
	
	// var fetchXml = [
					// "<fetch>",
					// "  <entity name='ig1_associatedcost'>",
					// "    <attribute name='ig1_totalsellprice' />",
					// "    <attribute name='ig1_totalindirectcost' />",
					// "    <attribute name='ig1_totaldirectcost' />",
					// "    <attribute name='ig1_pmcost' />",
					// "    <attribute name='ig1_designcost' />",
					// "    <attribute name='ig1_salescost' />",
					// "    <attribute name='ig1_totaltravel' />",
					// "    <filter type='and'>",
					// "      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					// "      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					// "    </filter>",
					// "  </entity>",
					// "</fetch>",
				   // ].join("");
				   
				   
				   
				   
	var costData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	
	var totalSellPrice=0;
	var totalDirectCost=0;
	var totalIndirectCost=0;
	var totalDesignCost=0;
	var totalSalesCost=0;
	var totalPmCost=0;
	var totalTravelCost=0;
	var baseIndirectCost=0;
	var baseDesin=0;
	var baseSales=0;
	var baseLabor=0;
	var anticipatedCommissionableValueInPercent=0;
	
	//for(i=0; i<costData.length; i++)
	//{
	  result=costData[0].attributes;
	  if(result.ig1_sellprice!="" && result.ig1_sellprice!=null && result.ig1_sellprice!=undefined)
	  {
		totalSellPrice =(parseFloat(result.ig1_sellprice.value));
	  }
	  if(result.ig1_directcost!="" && result.ig1_directcost!=null && result.ig1_directcost!=undefined)
	  {
		totalDirectCost =(parseFloat(result.ig1_directcost.value));
	  }
	  if(result.ig1_indirectcost!="" && result.ig1_indirectcost!=null && result.ig1_indirectcost!=undefined)
	  {
		totalIndirectCost =(parseFloat(result.ig1_indirectcost.value));
	  }
	  if(result.ig1_baseindirect!="" && result.ig1_baseindirect!=null && result.ig1_baseindirect!=undefined)
	  {
		baseIndirectCost =(parseFloat(result.ig1_baseindirect.value));
	  }
	   if(result.ig1_designcost!="" && result.ig1_designcost!=null && result.ig1_designcost!=undefined)
	   {
		totalDesignCost =(parseFloat(result.ig1_designcost.value));
	   }
	   if(result.ig1_basedesign!="" && result.ig1_basedesign!=null && result.ig1_basedesign!=undefined)
	   {
		baseDesin =(parseFloat(result.ig1_basedesign.value));
	   }
	   if(result.ig1_salescost!="" && result.ig1_salescost!=null && result.ig1_salescost!=undefined)
	  {
		totalSalesCost =(parseFloat(result.ig1_salescost.value));
	  }
	  if(result.ig1_basesales!="" && result.ig1_basesales!=null && result.ig1_basesales!=undefined)
	  {
		baseSales =(parseFloat(result.ig1_basesales.value));
	  }
	   if(result.ig1_pmcost!="" && result.ig1_pmcost!=null && result.ig1_pmcost!=undefined)
	  {
		totalPmCost =(parseFloat(result.ig1_pmcost.value));
	  }
	   if(result.ig1_totaltravel!="" && result.ig1_totaltravel!=null && result.ig1_totaltravel!=undefined)
	  {
		totalTravelCost =(parseFloat(result.ig1_totaltravel.value));
	  }
	//}
	var anticipatedGPInPercent=0;
	var anticipatedNetInPercent=0;
	var anticipatedGP=totalSellPrice-totalDirectCost-0.7 * (totalDesignCost+totalSalesCost+totalPmCost)- totalTravelCost;
	//var anticipatedGP=totalSellPrice-totalDirectCost-(baseDesin+baseSales+baseLabor)- totalTravelCost;
	
	if(totalSellPrice>0)
	{
	  anticipatedGPInPercent=(anticipatedGP * 100)/totalSellPrice;	
	}
	
	var anticipatedNetPreCommissionper = anticipatedGPInPercent - 14;
	
	//var anticipatedNet=anticipatedGP-totalIndirectCost;
	
	var anticipatedNet= (anticipatedNetPreCommissionper/100) * totalSellPrice ;
	
	var anticipatedCommissionableValue=anticipatedNet*0.3;
	
	var net_netamt = anticipatedNet - anticipatedCommissionableValue;
	
	if(totalSellPrice != 0)
	var net_netper = (net_netamt *100) / totalSellPrice;
	else 
		var net_netper = 0;
	
	//if(totalSellPrice>0)
	//{
		//anticipatedNetInPercent=anticipatedNet/totalSellPrice;
	//}
	
	Xrm.Page.getAttribute("ig1_anticipatedgp").setValue(anticipatedGP);
	Xrm.Page.getAttribute("ig1_anticipatednet").setValue(anticipatedNet);
	Xrm.Page.getAttribute("ig1_anticipatedcommissionablevalue").setValue(anticipatedCommissionableValue);
	Xrm.Page.getAttribute("ig1_anticipatedgpinpercent").setValue(anticipatedGPInPercent);
	Xrm.Page.getAttribute("ig1_anticipatednetinpercent").setValue(anticipatedNetPreCommissionper);
	Xrm.Page.getAttribute("ig1_netnet").setValue(net_netamt);
	Xrm.Page.getAttribute("ig1_anticipatedcommissionablevalueinpercent").setValue(net_netper);

}