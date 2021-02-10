function newBOM()
{
   var grid = Xrm.Page.ui.controls.getByName("CreateBOM");
        if (grid) 
		{
            grid.addOnLoad(onLoadFunction);
            var query = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"+
				"  <entity name='ig1_bidsheetproduct'>"+
				"    <attribute name='ig1_bidsheetproductid' />"+
				"    <attribute name='ig1_name' />"+
				"    <attribute name='createdon' />"+
				"    <order attribute='ig1_name' descending='false' />"+
				"    <filter type='and'>"+
				"      <condition attribute='ig1_isgrouped' operator='eq' value='1' />"+
				"      <condition attribute='ig1_productgroup' operator='not-null' />"+
				"    </filter>"+
				"  </entity>"+
				"</fetch>";
            UpdateSetFetchXmlFunc(query);
            grid.refresh();
        }
}


 UpdateSetFetchXmlFunc: function (fetchXml) {
        var setFetchXmlStr = Microsoft.Crm.Client.Core.Storage.DataApi.ListQuery.prototype.set_FetchXml.toString();
        var newFunc = setFetchXmlStr.replace("function(e){", "function(e){if (e.indexOf('not-null') >= 0) {e = fetchXml;}");
        eval("Microsoft.Crm.Client.Core.Storage.DataApi.ListQuery.prototype.set_FetchXml=" + newFunc);
    }