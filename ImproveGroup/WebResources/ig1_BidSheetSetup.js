function onLoad() {
    var category = Xrm.Page.getAttribute("ig1_category").getValue();
    var vendor = Xrm.Page.getAttribute("ig1_vendor").getValue();
    var paymentTerms = Xrm.Page.getAttribute("ig1_paymentterms").getValue();
    if (category != "" && category != null && category != undefined) {
        Xrm.Page.getControl("ig1_category").setDisabled(true);
    }
    if (vendor != "" && vendor != null && vendor != undefined) {
        Xrm.Page.getControl("ig1_vendor").setDisabled(true);
    }
}

//function setVendor() {
//    var bidSheetId = "";
//    var categoryId = '';
//    var vendorId = '';
//    var bidSheet = Xrm.Page.getAttribute("ig1_bidsheet").getValue();
//    var category = Xrm.Page.getAttribute("ig1_category").getValue();
//    var vendor = Xrm.Page.getAttribute("ig1_vendor").getValue();
//    if (category != null && category != undefined) {
//        categoryId = category[0].id;
//    }
//    if (vendor != null && vendor != undefined) {
//        vendorId = vendor[0].id;
//    }
//    if (bidSheet != null && bidSheet != undefined) {
//        bidSheetId = bidSheet[0].id;
//    }

//    var fetchData = {
//        ig1_bidsheet: bidSheetId,
//        ig1_category: categoryId,
//        ig1_vendor: vendorId
//    };
//    var fetchXml = [
//					"<fetch top='50'>",
//					"  <entity name='ig1_bscategoryvendor'>",
//					"    <attribute name='ig1_vendor' />",
//					"    <attribute name='ig1_category' />",
//					"    <attribute name='ig1_bidsheet' />",
//					"    <filter>",
//                                        "      <condition attribute='statecode' operator='eq' value='0' />",
//					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
//					"      <condition attribute='ig1_category' operator='eq' value='", fetchData.ig1_category, "'/>",
//					"      <condition attribute='ig1_vendor' operator='eq' value='", fetchData.ig1_vendor, "'/>",
//					"    </filter>",
//					"  </entity>",
//					"</fetch>",
//    ].join("");
//    var fetchXmlData = XrmServiceToolkit.Soap.Fetch(fetchXml);
//    if (fetchXmlData.length > 0) {
//        Xrm.Page.getAttribute("ig1_vendor").setValue(null);
//        alert("This vendor is already selected within the same category please choose another");
//    }
//}

function setCategory() {
    debugger;
    var bidSheetId = "";
    var categoryName = "";
    var categoryId = '';
    var vendorId = '';
    var bidSheet = Xrm.Page.getAttribute("ig1_bidsheet").getValue();
    var category = Xrm.Page.getAttribute("ig1_category").getValue();
    var vendor = Xrm.Page.getAttribute("ig1_vendor").getValue();
    if (category != null && category != undefined && category != "") {
        categoryId = category[0].id;
        categoryName = category[0].name;
    }
    if (categoryName != undefined && categoryName != null && categoryName == "Labor") {
        alert("Labor category can not be selected please choose another category");
        Xrm.Page.getAttribute("ig1_category").setValue(null);
        return;
    }
    if (vendor != null && vendor != undefined) {
        vendorId = vendor[0].id;
    }
    if (bidSheet != null && bidSheet != undefined) {
        bidSheetId = bidSheet[0].id;
    }

    var fetchData = {
        ig1_bidsheet: bidSheetId,
        ig1_category: categoryId,
       // ig1_vendor: vendorId
    };
    var fetchXml = [
					"<fetch top='50'>",
					"  <entity name='ig1_bscategoryvendor'>",					
					"    <attribute name='ig1_category' />",
					"    <attribute name='ig1_bidsheet' />",
					"    <filter>",
                                        "      <condition attribute='statecode' operator='eq' value='0' />",
					"      <condition attribute='ig1_bidsheet' operator='eq' value='", fetchData.ig1_bidsheet, "'/>",
					"      <condition attribute='ig1_category' operator='eq' value='", fetchData.ig1_category, "'/>",					
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
    var fetchXmlData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (fetchXmlData.length > 0) {
        Xrm.Page.getAttribute("ig1_category").setValue(null);
        alert("This categoey is already selected within the Bidsheet please choose another");
    }
}

function setAsReadOnly() {
    var bsStatus = isBidSheetStatusDraft();
    if (bsStatus == null) {
        Xrm.Page.getControl("ig1_bidsheet").setDisabled(false);
        return;
    }
    if (!bsStatus) {
        var controls = Xrm.Page.ui.controls.get();
        for (var i in controls) {
            var control = controls[i];
            if (control.getDisabled && control.setDisabled && !control.getDisabled()) {
                control.setDisabled(true);
            }
        }
    }
}

function isBidSheetStatusDraft() {
    var bidSheetId = "";
    var bidSheet = Xrm.Page.getAttribute("ig1_bidsheet").getValue();
    if (bidSheet == undefined || bidSheet == null || bidSheet == "") {
        return null;
    }
    bidSheetId = bidSheet[0].id.replace("{", "").replace("}", "");
    var fetchData = {
        ig1_bidsheetid: bidSheetId,
        ig1_status: "286150001"
    };
    var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='ig1_bidsheet'>",
                    "    <attribute name='ig1_status' />",
                    "    <filter type='and'>",
                    "      <condition attribute='ig1_bidsheetid' operator='eq' value='", fetchData.ig1_bidsheetid/*bf1962bf-a8fb-e911-a812-000d3a55d933*/, "'/>",
                    "      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150001*/, "'/>",
                    "    </filter>",
                    "  </entity>",
                    "</fetch>",
    ].join("");
    var BsSatatusData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (BsSatatusData != null && BsSatatusData.length > 0)
        return true;
    else
        return false;
}