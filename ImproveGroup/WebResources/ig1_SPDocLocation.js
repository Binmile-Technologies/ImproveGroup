function AssociateDocumentLocation()
{
	var locId = "";
var fetchXML = [
					"<fetch>",
					"  <entity name='sharepointdocumentlocation'>",
					"    <all-attributes/>",
					"    <filter type='and'>",
					"      <condition attribute='servicetype' operator='eq' value ='3'/>",
                                        "<condition attribute='relativeurl' operator='eq' value='Dirtt Graphics'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
				].join("");
				
		var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXML);		
				
				 if(fetchData!='' && fetchData!=null && fetchData!=undefined)	
     {
		 for(j=0; j<fetchData.length; j++)
			{
		  locId=fetchData[j].id;
			}
	 }
				
				var association = {
    "@odata.id": Xrm.Page.context.getClientUrl() + "/api/data/v9.1/sharepointdocumentlocations("+locId+")"
};
var req = new XMLHttpRequest();
req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(cfb303e5-881a-e911-a954-000d3a1d58e9)/Opportunity_SharepointDocumentLocation/$ref", false);
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
req.send(JSON.stringify(association));
}


function SetOpportunityDocumentFrame(executionContext) {

    //You can see what the url should be by navigating to the 'Documents' area under related records, viewing the page soure
    //and looking for 'areaSPDocuments'. The formid appears to be nothing more than a random guid value and not tied to anything 
    //specific in your org. 

    //Use: Make sure Document Management is enabled for the entity (helps to turn on automatic folder creation)
    //     Add a web resource with this code to the form 
    //     Execute this function during the form's OnLoad event
var url = Xrm.Page.context.getClientUrl() +
        "/userdefined/areas.aspx?formid=a837e4a7-01b8-4f82-a475-be9abd67e667&inlineEdit=1&navItemName=Documents&oId=%7b" +
        Xrm.Page.data.entity.getId().replace("{", "").replace("}", "") + "%7d&oType=" +/*Xrm.Page.context.getQueryStringParameters().etc*/ 3+ 
        "&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White";
 
    Xrm.Page.getControl("IFRAME_sketch_images").setSrc(url); //Replace IFRAME_??? with actual IFRAME name*/

// var formContext = executionContext.getFormContext();
// var url = formContext.context.getClientUrl() +
                            // "/userdefined/areas.aspx?formid=a837e4a7-01b8-4f82-a475-be9abd67e667&inlineEdit=1&navItemName=Documents&oId=%7b" +
                            // formContext.data.entity.getId().replace("{", "").replace("}", "") +
                            // "%7d&oType=" +
                            // formContext.context.getQueryStringParameters().etc +
                            // "&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White";
                        // formContext.getControl("IFRAME_sketch_images").setSrc(url);
}



/// Show Documents subgrid on the main form
function setDocumentsIFrame(executionContext) {
    var formContext = executionContext.getFormContext();

   

    // Only want to run the code if the record is just being created, otherwise
    // there will no documents to show anyway
	var iframeName = "IFRAME_sketch_images";
    if (formContext.ui.getFormType() !== 1) {
        var iframe = formContext.getControl(iframeName);
        if (iframe === null) {
            console.log("Could not find iframe: " + iframeName);
            return;
        }

        var id = formContext.data.entity.getId().replace("{", "").replace("}", "");
     var CurrentFormId = Xrm.Page.ui.formSelector.getCurrentItem().getId().replace("{", "").replace("}", "");
        // Query SharePoint Documents Locations to find any records which are related to the 
        // record we are currently viewing
        var req = new XMLHttpRequest();
        req.open("GET",
            formContext.context.getClientUrl() +
            "/api/data/v9.1/sharepointdocumentlocations?$select=_regardingobjectid_value&$filter=_regardingobjectid_value eq " +
            id, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    if (results.value.length > 0) {
                        // If we have any related items then show the tab and set the IFrame URL accordingly
                       // tab.setVisible(true);
                        var url = formContext.context.getClientUrl() +
                            "/userdefined/areas.aspx?formid=" +CurrentFormId+ "&inlineEdit=1&navItemName=Documents&oId=%7b" +
                            formContext.data.entity.getId().replace("{", "").replace("}", "") +                                        "%7d&oType=3&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White";
                           // formContext.context.getQueryStringParameters().etc +
                          //  "&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White";
                        iframe.setSrc(url);
                    } else {
                        tab.setVisible(false);
                    }

                } else {
                    alert(this.statusText);
                }
            }
        };
        req.send();
    }
}

