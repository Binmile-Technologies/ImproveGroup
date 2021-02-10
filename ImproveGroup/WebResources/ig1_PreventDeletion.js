function PreventCategoryDeletionFromSubGrid(executionContext)
{       
		var flag=true;
		var bidSheetId=Xrm.Page.data.entity.getId().replace("{","").replace("}","");
        var fetchData = {
		ig1_bidsheet: bidSheetId,
		statecode: "0"
	};
	var fetchXmlBidSheetCategory = [
									"<fetch>",
									"  <entity name='ig1_bscategoryvendor'>",
									"    <attribute name='ig1_categoryname' />",
									"    <attribute name='ig1_category' />",
									"    <filter type='and'>",
									"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
									"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
									"    </filter>",
									"    <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category'>",
									"      <attribute name='ig1_bidsheetcategoryid' />",
									"      <attribute name='ig1_name' />",
									"      <link-entity name='product' from='ig1_bidsheetcategory' to='ig1_bidsheetcategoryid'>",
									"        <attribute name='name' />",
									"        <link-entity name='ig1_bidsheetproduct' from='ig1_productgroup' to='productid'>",
									"          <attribute name='ig1_name' />",
									"        </link-entity>",
									"      </link-entity>",
									"    </link-entity>",
									"  </entity>",
									"</fetch>",
									].join("");
	var bidSheetCategory=XrmServiceToolkit.Soap.Fetch(fetchXmlBidSheetCategory);
	if(bidSheetCategory!="" && bidSheetCategory!=null && bidSheetCategory!=undefined && bidSheetCategory.length>0)
	{
		flag=false;
	}
	
		var fetchData = {
		ig1_bidsheet: bidSheetId,
		statecode: "0",
	};
	var fetchXmlBOMItems = [
					"<fetch>",
					"  <entity name='ig1_bscategoryvendor'>",
					"    <attribute name='ig1_categoryname' />",
					"    <attribute name='ig1_category' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					"    </filter>",
					"    <link-entity name='ig1_bidsheetcategory' from='ig1_bidsheetcategoryid' to='ig1_category'>",
					"      <attribute name='ig1_bidsheetcategoryid' />",
					"      <attribute name='ig1_name' />",
					"      <link-entity name='ig1_bidsheetpricelistitem' from='ig1_category' to='ig1_bidsheetcategoryid'>",
					"        <attribute name='ig1_product' />",
					"        <filter type='and'>",
					"          <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"          <condition attribute='statecode' operator='eq' value='", fetchData.statecode, "'/>",
					"        </filter>",
					"      </link-entity>",
					"    </link-entity>",
					"  </entity>",
					"</fetch>",
				   ].join("");
	
	var BOMItems=XrmServiceToolkit.Soap.Fetch(fetchXmlBOMItems);
	if(BOMItems!="" && BOMItems!=null && BOMItems!=undefined && BOMItems.length>0)
	{
		flag=false;
	}
	if(flag==false)
        {
           Xrm.Page.ui.setFormNotification("Selected Category can't be deleted because of association", "INFO", "2001");
        }
         else
          {
            Xrm.Page.ui.clearFormNotification("2001"); 
           }
	return flag;
}