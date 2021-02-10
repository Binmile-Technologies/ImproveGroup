function runReport()
{
        var hight=800;
        var width=900;
        var left = (window.screen.width / 2) - ((width/ 2) + 10);
        var top = (window.screen.height / 2) - ((hight/ 2) + 50);

        var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
		var rdlName = "MaterialVsLabor.rdl";
		//var reportGuid = "929203fc-fea6-e911-a95d-000d3a1d58e9";
		var reportGuid = getReportId("Direct Vs Labor");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidSheetId);
        var url = Xrm.Page.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" + reportGuid + "}&p:bidSheetId=" + parameter;
    window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

function runSummaryReport()
{
        var hight=800;
      var width=900;
      var left = (window.screen.width / 2) - ((width/ 2) + 10);
      var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var bidSheetId=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "BidSheetSummary.rdl";
       //var reportGuid = "d5640ad4-6012-ea11-a811-000d3a55d2c3";
	   var reportGuid = getReportId("BidSheetSummary");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidSheetId);
       var url = Xrm.Page.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" + reportGuid + "}&p:bidsheetid=" + parameter + "&p:bidSheet="+ parameter;
       window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

function runProjectBudgetReport()
{
  var hight=800;
      var width=900;
      var left = (window.screen.width / 2) - ((width/ 2) + 10);
      var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var bidsheetid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "ProjectBudgetReport.rdl";
       //var reportGuid = "f772fd8d-548f-ea11-a811-000d3a98d1ad";
	   var reportGuid = getReportId("ProjectBudgetReport");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidsheetid);
       var url = Xrm.Page.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" +  reportGuid + "}&p:bidsheetid=" + parameter;
       window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

// AAR Report for project report

function  runAARReport()
{

       var hight=800;
       var width=900;
       var left = (window.screen.width / 2) - ((width/ 2) + 10);
       var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var projectrecordid=Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "AARReport.rdl";
       //var reportGuid = "1d67a028-c899-ea11-a811-000d3a98d1ad";
	   var reportGuid = getReportId("AARReport");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
       var parameter = encodeURIComponent(projectrecordid);
       var url = Xrm.Page.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" +  reportGuid + "}&p:projectrecordid=" + parameter;
       window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

function getReportId(reportName)
{
	var reportId = "";
			var fetchData = {
		name: reportName
	};
	var fetchXml = [
					"<fetch>",
					"  <entity name='report'>",
					"    <attribute name='reportid' />",
					"    <attribute name='name' />",
					"    <filter type='and'>",
					"      <condition attribute='name' operator='eq' value='", fetchData.name/*ProjectBudgetReport*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				  ].join("");
				  
	var fetchData=XrmServiceToolkit.Soap.Fetch(fetchXml);
	if(fetchData!=undefined && fetchData!=null && fetchData!="" && fetchData.length>0)
	{
		if(fetchData[0].attributes.reportid!=undefined && fetchData[0].attributes.reportid!=null && fetchData[0].attributes.reportid!="")
		{
			reportId = fetchData[0].attributes.reportid.value.replace("{", "").replace("}", "");
		}
	}
	return reportId;
}