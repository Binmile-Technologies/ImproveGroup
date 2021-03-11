function runDirectVsLaborReport(formContext)
{
        var hight=800;
        var width=900;
        var left = (window.screen.width / 2) - ((width/ 2) + 10);
        var top = (window.screen.height / 2) - ((hight/ 2) + 50);

        var bidSheetId=formContext.data.entity.getId().replace("{", "").replace("}", "");
		var rdlName = "DirectVsLabor.rdl";
		//var reportGuid = "929203fc-fea6-e911-a95d-000d3a1d58e9";
		var reportGuid = getReportId("DirectVsLabor");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidSheetId);
        var url = formContext.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" + reportGuid + "}&p:bidSheetId=" + parameter;
    window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

function runSummaryReport(formContext)
{
        var hight=800;
      var width=900;
      var left = (window.screen.width / 2) - ((width/ 2) + 10);
      var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var bidSheetId=formContext.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "BidSheetSummary.rdl";
	   var reportGuid = getReportId("BidSheetSummary");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidSheetId);
       var url = formContext.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" + reportGuid + "}&p:bidsheetid=" + parameter + "&p:bidSheet="+ parameter;
       window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
}

function runProjectBudgetReport(formContext)
{
  var hight=800;
      var width=900;
      var left = (window.screen.width / 2) - ((width/ 2) + 10);
      var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var bidsheetid=formContext.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "ProjectBudgetReport.rdl";
       //var reportGuid = "f772fd8d-548f-ea11-a811-000d3a98d1ad";
	   var reportGuid = getReportId("ProjectBudgetReport");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
        var parameter = encodeURIComponent(bidsheetid);
       var url = formContext.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" +  reportGuid + "}&p:bidsheetid=" + parameter;
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

function runBidSheetSummaryDetailsReport(formContext)
{
       try
     {
       debugger;
       var hight=800;
       var width=1160;
       var left = (window.screen.width/2) - ((width/2) + 10);
       var top = (window.screen.height / 2) - ((hight/ 2) + 50);
       var bidsheetid=formContext.data.entity.getId().replace("{", "").replace("}", "");
       var rdlName = "BidSheetSummaryDetailsReport.rdl";
	   var reportGuid = getReportId("BidSheetSummaryDetailsReport");
		if(reportGuid=="")
		{
			alert("Sorry, report does not exists");
			return;
		}
       var parameter = encodeURIComponent(bidsheetid);
      var url = formContext.context.getClientUrl() + "/crmreports/viewer/viewer.aspx?action=filter&helpID=" + rdlName + "&id={" + reportGuid + "}&p:bidsheetid=" + parameter;
       window.open(url, null, "width="+width+ ", height="+hight+", top ="+top+", left="+left);
   }
   catch(err)
  {
   alert(err.message);
  }
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