function bidSheetProcess() {
    var status = Xrm.Page.getAttribute("ig1_bidsheetprocess").getValue();
    var opportunityId = Xrm.Page.data.entity.getId().toLowerCase().replace('{', '').replace('}', '');

    if (status) {

        Xrm.Page.ui.tabs.get("Product_Line_Items").setVisible(false);
        Xrm.Page.ui.tabs.get("BidSheet").setVisible(true);
    }
    else {
        Xrm.Page.ui.tabs.get("Product_Line_Items").setVisible(true);
        Xrm.Page.ui.tabs.get("BidSheet").setVisible(false);
    }
}

function openBidSheet(formContext, selectedControl) {
    debugger;
    var parameters = {};
    var opportunityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
    var opportunityname = formContext.getAttribute("name").getValue();
    var entityName = formContext.data.entity.getEntityName();
    revisionfromopportunity(opportunityId);
    var bidsheetid = Isbidsheetexist(opportunityId);
    var getupperversion = UpperVersion(opportunityId);
    if (bidsheetid == null || bidsheetid == "" || bidsheetid == undefined) {
        parameters["ig1_opportunitytitle"] = opportunityId;
        parameters["ig1_opportunitytitlename"] = opportunityname;
        parameters["ig1_opportunitytitletype"] = entityName;
        parameters["ig1_iscreateorder"] = true;
        Xrm.Utility.openEntityForm("ig1_bidsheet", null, parameters);
    }
    if (bidsheetid != null && bidsheetid != undefined && bidsheetid != "") {
        var Isconfirm = confirm('If you want to create a Change Order, click OK. If you want to create an Empty Revision of the current bid sheet, click Cancel.?');
        if (Isconfirm == true) {
            var Isconfirmagain = confirm("Are you sure you want to create a Change Order? (Cancel will create nothing)");
            if (Isconfirmagain == true) {

                parameters["ig1_opportunitytitle"] = opportunityId;
                parameters["ig1_opportunitytitlename"] = opportunityname;
                parameters["ig1_opportunitytitletype"] = entityName;
                parameters["ig1_iscreateorder"] = true;
                Xrm.Utility.openEntityForm("ig1_bidsheet", null, parameters);
            }
            else {
                return;
            }

        }
        else {
            var confirmrevise = confirm("Are you sure you want to create a New Empty Revision of the " + "[" + getupperversion + "]" + " bidsheet? (Cancel will create nothing).")
            if (confirmrevise == true) {
                parameters["ig1_opportunitytitle"] = opportunityId;
                parameters["ig1_opportunitytitlename"] = opportunityname;
                parameters["ig1_opportunitytitletype"] = entityName;
                parameters["ig1_iscreateorder"] = false;
                Xrm.Utility.openEntityForm("ig1_bidsheet", null, parameters);

            }

            else {

                return;
            }


        }
    }




}
function Isbidsheetexist(id) {
    var bidsheetid = "";
    var fetchData = {
        ig1_opportunitytitle: id
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <attribute name='ig1_bidsheetid' />",
        "    <attribute name='ig1_name' />",
        "    <attribute name='ig1_status' />",
        "    <filter type='and'>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*809dda40-911b-457c-9068-975c3607c3f4*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");

    var bidsheetstatus = XrmServiceToolkit.Soap.Fetch(fetchXml);

    if (bidsheetstatus != null && bidsheetstatus != undefined && bidsheetstatus != "") {

        if (bidsheetstatus.length > 0) {

            bidsheetid = bidsheetstatus[0].attributes.ig1_bidsheetid.value;
        }

        return bidsheetid;
    }
}

function UpperVersion(id) {
    debugger;
    var upperversion = 0;
    var fetchData = {
        ig1_opportunitytitle: id
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <attribute name='ig1_version' />",
        "    <filter>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*5c25fccc-1c31-4c8a-a999-a0dfe5433efb*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");

    var version = XrmServiceToolkit.Soap.Fetch(fetchXml);

    for (i = 0; i < version.length; i++) {

        var fetchversion = version[i].attributes.ig1_version.value;
        var result = parseFloat(fetchversion);
        if (result > upperversion) {
            upperversion = result;

        }

    }
    return upperversion;
}



function Openbidsheetpage() {

    var windowOptions = { openInNewWindow: "False" };
    //parameters["ig1_opportunitytitle"]=opportunityId;
    //parameters["ig1_opportunitytitlename"]=opportunityname;
    //parameters["formid"] = "8B18C580-1D52-4CD8-B30F-F490809A61C2";
    Xrm.Utility.openEntityForm("ig1_bidsheet", null, windowOptions);
}

function revisionfromopportunity(opportunityId) {
    var oppStatusOpen = isOpportunityOpenforrevision(opportunityId);
    if (!oppStatusOpen) {
        alert("Can't Revise BidSheet as Opportunity is Closed");
        Xrm.Utility.closeProgressIndicator();
        return;
    }

}

function isOpportunityOpenforrevision(id) {

    if (id != undefined && id != null && id != "") {
        var fetchData = {
            statecode: "0",
            opportunityid: id
        };
        var fetchXml = [
            "<fetch mapping='logical' version='1.0'>",
            "  <entity name='opportunity'>",
            "    <attribute name='name' />",
            "    <filter type='and'>",
            "      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
            "      <condition attribute='opportunityid' operator='eq' value='", fetchData.opportunityid/*b5af3c1f-4b43-ea11-a812-000d3a55d933*/, "'/>",
            "    </filter>",
            "  </entity>",
            "</fetch>",
        ].join("");

        var oppStatusData = XrmServiceToolkit.Soap.Fetch(fetchXml);
        if (oppStatusData != undefined && oppStatusData != null && oppStatusData != "") {
            if (oppStatusData.length > 0) {
                oppStatus = true;
            }
        }
        return oppStatus;
    }
}


function HideNewButton() {
    var condition = true;
    var opprtunityId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
    var fetchData =
    {
        ig1_opportunitytitle: opprtunityId
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <attribute name='ig1_name' />",
        "    <filter type='and'>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");
    var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    var IsActiveBSExist = IsAnyActivatedBSExists(opprtunityId);
    var IsDraftBidSheetExist = IsAnyDraftBSExists(opprtunityId);
    var OpportunityStatus = OppStatus(opprtunityId);
    if (fetchData != undefined && fetchData != null && fetchData != "" && fetchData.length > 0) {
        if (IsActiveBSExist && OpportunityStatus && !IsDraftBidSheetExist) {
            condition = true;
        }
        else {
            condition = false;
        }
    }
    if (condition) {
        return true;
    }
    else {
        return false;
    }
}

function permanentHideSubGridRibbon() {
    return false;
}

function IsAnyActivatedBSExists(opprtunityId) {
    var Flag = false;
    var fetchData =
    {
        ig1_opportunitytitle: opprtunityId,
        ig1_status: '286150000'
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <attribute name='ig1_name' />",
        "    <filter type='and'>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle, "'/>",
        "      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150000*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");
    var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (fetchData != "" && fetchData != null && fetchData != undefined && fetchData.length > 0) {
        Flag = true;
    }
    return Flag;
}

function IsAnyDraftBSExists(opprtunityId) {
    var Flag = false;
    var fetchData =
    {
        ig1_opportunitytitle: opprtunityId,
        ig1_status: '286150001'
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <attribute name='ig1_name' />",
        "    <filter type='and'>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle, "'/>",
        "      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150001*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");
    var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (fetchData != "" && fetchData != null && fetchData != undefined && fetchData.length > 0) {
        Flag = true;
    }
    return Flag;
}

function OppStatus(opprtunityId) {
    var Flag = false;
    var fetchData = {
        statuscode: "1",
        opportunityid: opprtunityId
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='opportunity'>",
        "    <attribute name='name' />",
        "    <filter type='and'>",
        "      <condition attribute='statuscode' operator='eq' value='", fetchData.statuscode/*1*/, "'/>",
        "      <condition attribute='opportunityid' operator='eq' value='", fetchData.opportunityid/*827bdfbc-84f6-495d-bb28-82c88606b6f7*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");
    var fetchData = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (fetchData != "" && fetchData != null && fetchData != undefined && fetchData.length > 0) {
        Flag = true;
    }
    return Flag;
}


function disableProjectedGP(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    var id = formContext.data.entity.getId().replace("{", "").replace("}", "");

    var fetchData = {
        ig1_opportunitytitle: id,
        statecode: "0",
        ig1_status: "286150000"
    };
    var fetchXml = [
        "<fetch>",
        "  <entity name='ig1_bidsheet'>",
        "    <filter type='and'>",
        "      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
        "      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*opportunity*/, "'/>",
        "      <condition attribute='ig1_status' operator='eq' value='", fetchData.ig1_status/*286150000*/, "'/>",
        "    </filter>",
        "  </entity>",
        "</fetch>",
    ].join("");

    var bs = executeFetchXml("ig1_bidsheets", fetchXml);
    if (bs.value != undefined && bs.value != null && bs.value != "" && bs.value.length > 0) {
        formContext.getControl("ig1_totalgrossprofit").setDisabled(true);
        formContext.getControl("pricelevelid").setDisabled(true);
    }
    else {
        formContext.getControl("ig1_totalgrossprofit").setDisabled(false);
        formContext.getControl("pricelevelid").setDisabled(false);
    }
}

function cloneOpportunity(selectedControl, formContext) {
    try {
        debugger;
        var isConfirmed = false;
        var confirmStrings = { text: "Are you sure want to clone this opportunity?", title: "Confirmation Dialog" };
        var confirmOptions = { height: 150, width: 450 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
            function (success) {
                if (success.confirmed) {
                    Xrm.Utility.showProgressIndicator("Cloning Opportunity Please Wait...");
                    var opportunityid = "";
                    var isSelectedControlArray = Array.isArray(selectedControl);
                    var insideOpportunity = false;
                    if (isSelectedControlArray && selectedControl != null && selectedControl != "" && selectedControl.length > 0) {
                        if (selectedControl.length > 1) {
                            alert("Only one record can be selected to clone");
                            Xrm.Utility.closeProgressIndicator();
                            return;
                        }
                        else {
                            opportunityid = selectedControl[0].Id.replace("{", "").replace("}", "");
                        }
                    }
                    else {
                        opportunityid = formContext.data.entity.getId().replace("{", "").replace("}", "");
                        insideOpportunity = true;
                    }
                    if (opportunityid == undefined || opportunityid == null || opportunityid == "") {
                        alert("Atleast one record should be seleted");
                        Xrm.Utility.closeProgressIndicator();
                        return;
                    }
                    var parameters = {};
                    parameters.opportunityid = opportunityid;

                    var req = new XMLHttpRequest();
                    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_CloneOpportunity", true);
                    req.setRequestHeader("OData-MaxVersion", "4.0");
                    req.setRequestHeader("OData-Version", "4.0");
                    req.setRequestHeader("Accept", "application/json");
                    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                    req.onreadystatechange = function () {
                        if (this.readyState === 4) {
                            req.onreadystatechange = null;
                            if (this.status === 200) {
                                var results = JSON.parse(this.response);
                                alert("Opportunity Cloned Successfully");
                                if (selectedControl.length == 1) {
                                    var lookupOptions = {};
                                    lookupOptions.entityType = "opportunity";
                                    Xrm.Utility.refreshParentGrid(lookupOptions);
                                }
                                else if (insideOpportunity && results.clonedopportunityid != undefined && results.clonedopportunityid != null && results.clonedopportunityid != "") {
                                    Xrm.Utility.openEntityForm("opportunity", results.clonedopportunityid);

                                }
                                Xrm.Utility.closeProgressIndicator();
                            }
                            else {
                                Xrm.Utility.alertDialog(this.statusText);
                            }
                        }
                    };
                    req.send(JSON.stringify(parameters));
                }
                else {
                    Xrm.Utility.closeProgressIndicator();
                }
            });
    }
    catch (err) {
        alert(err.message);
        Xrm.Utility.closeProgressIndicator();
    }
}

function unlockFieldsForFinance(executionContext) {
    try {
        var formContext = executionContext.getFormContext();
        var status = formContext.getAttribute("statecode").getValue();
        if (status == 0) {
            return
        }

        var currentUserRoles = formContext.context.getUserRoles();
        var isFinance = financeRole(currentUserRoles);
        if (isFinance)//currentUserRoles.includes("2690bbc6-65c9-e911-a960-000d3a1d52e7")
        {
            formContext.ui.controls.get("ig1_synctoqb").setDisabled(false);
            formContext.ui.controls.get("ig1_quickbooksid").setDisabled(false);
            formContext.ui.controls.get("ig1_splitcommissionwith").setDisabled(false);
            formContext.ui.controls.get("ig1_commissionpercentsplit").setDisabled(false);
            formContext.ui.controls.get("ig1_commissionlastedited").setDisabled(false);
        }
    }
    catch (err) {
        alert(err.message);
    }
}

function EnableTestProjectField(executionContext) {
    try {
        debugger;
        var formContext = executionContext.getFormContext();
        var currentUserRoles = formContext.context.getUserRoles();
        var oppstatus = formContext.getAttribute("statecode").getValue();
        var isAdminExist = SystemAdminRole(currentUserRoles);
        if (isAdminExist && (oppstatus == 1 || oppstatus == 2)) {
            formContext.ui.controls.get("ig1_istestproject").setDisabled(false);
            formContext.data.save();
        }
    }

    catch (err) {
        alert(err.message);

    }

}

function SystemAdminRole(currentUserRoles) {
    try {
        var isAdmin = false;
        for (i = 0; i < currentUserRoles.length; i++) {
            var req = new XMLHttpRequest();
            req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/roles(" + currentUserRoles[i] + ")?$select=name", false);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 200) {
                        var result = JSON.parse(this.response);
                        var name = result["name"];
                        if (name == "System Administrator") {
                            isAdmin = true;
                        }
                    }
                    else {
                        Xrm.Utility.alertDialog(this.statusText);
                    }
                }
            };
            req.send();
        }
        return isAdmin;
    }
    catch (err) {
        aert(err.message);
    }
}

function financeRole(currentUserRoles) {

    var isFinanceRoleExist = false;
    try {

        for (i = 0; i < currentUserRoles.length; i++) {
            var req = new XMLHttpRequest();
            req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/roles(" + currentUserRoles[i] + ")?$select=name", false);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 200) {
                        var result = JSON.parse(this.response);
                        var name = result["name"];
                        if (name == "Finance") {
                            isFinanceRoleExist = true;
                        }
                    }
                    else {
                        Xrm.Utility.alertDialog(this.statusText);
                    }
                }
            };
            req.send();
        }
    }
    catch (err) {
        aert(err.message);
    }

    return isFinanceRoleExist;
}

function saveOnQBDetailsChange(executionContext) {
    try {
        var formContext = executionContext.getFormContext();
        var currentUserRoles = formContext.context.getUserRoles();
        var isFinance = financeRole(currentUserRoles);
        if (isFinance) {
            formContext.data.save();
        }
    }
    catch (err) {
        alert(err.message);
    }
}

function captureLastModifiedDateByOwner(executionContext)
{
    try
    {
        debugger;
        var formContext = executionContext.getFormContext();
        formContext.data.entity.addOnPostSave(onPostSave);
    }
    catch (err)
    {
        alert(err.message);
    }
}

function onPostSave(executionContext)
{
    try
    {
        debugger;
        var formContext = executionContext.getFormContext();
        var saveEventArgs = executionContext.getEventArgs();
        if (!saveEventArgs.getIsSaveSuccess()) {
            var saveError = saveEventArgs.getSaveErrorInfo();
            alert(saveError);
            return;
        }
        else
        {
            var ownerid = "";
            var currentUser = formContext.context.getUserId().replace("{", "").replace("}", "");
            var owner = formContext.getAttribute("ownerid").getValue();
            if (owner == undefined && owner == null && owner == "")
            {
                return;
            }
            ownerid = owner[0].id.replace("{", "").replace("}", "");

            if (ownerid != currentUser)
            {
                return;
            }
            var opportunityid = formContext.data.entity.getId().replace("{", "").replace("}", "");
            var entity = {};

            entity.ig1_lastmodifiedbyowneron = new Date().toISOString();
            var req = new XMLHttpRequest();
            req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/opportunities(" + opportunityid+")", true);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Accept", "application/json");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 204) {
                        //Success - No Return Data - Do Something
                        formContext.data.refresh();
                    } else {
                        Xrm.Utility.alertDialog(this.statusText);
                    }
                }
            };
            req.send(JSON.stringify(entity));
        }
    }
    catch (err) {
        alert(err.message);
    }
}