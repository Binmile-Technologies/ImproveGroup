function finalizeProjectRecord(formContext) {
    var finalizedDate = formContext.getAttribute("ig1_finalizeddate").getValue();
    if (finalizedDate == undefined || finalizedDate == null || finalizedDate == "") {
        var isConfirmed = confirm("While Finalizing the Project first time, the Finalized date will be captured and can't be changed");
        if (isConfirmed == false)
            return;
    }
    else {
        var isConfirmed = confirm("While Finalizing the Project, the Finalized date will stay same as first time captured during finalization of project");
        if (isConfirmed == false)
            return;

    }

    var projectRecordId = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var fetchData = {
        ig1_projectrecordid: projectRecordId
    };
    var fetchXml = [
                    "<fetch mapping='logical' version='1.0'>",
                    "  <entity name='opportunity'>",
                    "    <attribute name='statecode' />",
                    "    <link-entity name='ig1_projectrecord' from='ig1_opportunity' to='opportunityid'>",
                    "      <filter type='and'>",
                    "        <condition attribute='ig1_projectrecordid' operator='eq' value='", fetchData.ig1_projectrecordid/*53eaf431-2331-ea11-a810-000d3a55dce2*/, "'/>",
                    "      </filter>",
                    "    </link-entity>",
                    "  </entity>",
                    "</fetch>",
    ].join("");
    var oppStatuses = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (oppStatuses != undefined && oppStatuses != null && oppStatuses != "" && oppStatuses.length > 0) {
        var result = oppStatuses[0].attributes;
        if (result.statecode != undefined && result.statecode != null && result.statecode != "") {
            if (result.statecode.value == 0) {
                alert("Please Close the Opportunity and then Finalize the Project");
                return;
            }
            else if (result.statecode.value == 1) {

                var fetchData = {
                    ig1_projectrecordid: projectRecordId
                };
                var fetchXml = [
                "<fetch>",
                "  <entity name='ig1_projectrecord'>",
                "    <attribute name='ig1_projectstatus' />",
                "    <filter>",
                "      <condition attribute='ig1_projectrecordid' operator='eq' value='", fetchData.ig1_projectrecordid/*69bf0dcc-30a4-ea11-a812-000d3a55df98*/, "'/>",
                "    </filter>",
                "  </entity>",
                "</fetch>",
                ].join("");
                debugger;
                var holdpreviousstatus = XrmServiceToolkit.Soap.Fetch(fetchXml);
                var result1 = holdpreviousstatus[0].attributes;
                if (result1.ig1_projectstatus != null && result1.ig1_projectstatus != undefined) {
                    var hvalue = result1.ig1_projectstatus.value;
                    var textvalue = hvalue.toString();
                    setTimeout(function () {
                        formContext.getAttribute("ig1_holdpreviousstatus").setValue(textvalue);

                    }, 1000);
                }

                formContext.getAttribute("ig1_projectstatus").setValue(286150004);
            }
            else if (result.statecode.value == 2) {
                formContext.getAttribute("ig1_projectstatus").setValue(286150006);
            }

            formContext.data.entity.save();
            formContext.ui.refreshRibbon();
        }
    }
}

function disablefinalizeButton(formContext) {
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status != 286150004 && status != 286150006)
            return true;
        else
            return false;
    }
}

function disable_Revert_ProjectStatus_Button(formContext) {
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150004)
            return true;
        else
            return false;
    }
}

function Revert_previous_satate(formContext) {
    debugger;
    var isConfirmed = confirm("Reverting the status of a Project, the Finalized date will stay Intact");
    if (isConfirmed == false)
        return;

    var opportunity = formContext.getAttribute("ig1_holdpreviousstatus").getValue();
    if (opportunity == undefined || opportunity == null || opportunity == "") {
        return;
    }
    else if (opportunity == "286150003") {
        formContext.getAttribute("ig1_projectstatus").setValue(286150003);

    }
    else if (opportunity == "286150007") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150007);
    }

    else if (opportunity == "286150001") {
        formContext.getAttribute("ig1_projectstatus").setValue(286150001);

    }
    else if (opportunity == "286150008") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150008);
    }
    else if (opportunity == "286150000") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150000);
    }
    else if (opportunity == "286150004") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150004);
    }
    else if (opportunity == "286150002") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150002);
    }
    else if (opportunity == "286150006") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150006);
    }
    else if (opportunity == "286150005") {

        formContext.getAttribute("ig1_projectstatus").setValue(286150005);
    }
    else if (opportunity == "286150009") {
        formContext.getAttribute("ig1_projectstatus").setValue(286150009);
    }
    formContext.data.entity.save();
    formContext.ui.refreshRibbon();
}
function wo_Status(opportunityId) {
    var woStatus = "";
    var fetchData = {
        statecode: "0",
        msdyn_opportunityid: opportunityId
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='msdyn_workorder'>",
					"    <attribute name='msdyn_systemstatus' />",
					"    <filter type='and'>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"      <condition attribute='msdyn_opportunityid' operator='eq' value='", fetchData.msdyn_opportunityid/*{d7f19844-f7e9-4ffc-9661-76cde6348aee*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");
    var wo = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (wo != undefined && wo != null && wo != "" && wo.length > 0) {
        for (i = 0; i < wo.length; i++) {
            var result = wo[i].attributes;
            if (result.msdyn_systemstatus != undefined && result.msdyn_systemstatus != null && result.msdyn_systemstatus != "") {
                if (result.msdyn_systemstatus.value == 690970000 || result.msdyn_systemstatus.value == 690970001 || result.msdyn_systemstatus.value == 690970002 || result.msdyn_systemstatus.value == 690970003) {
                    woStatus = "Installing";
                    break;
                }
                else {
                    woStatus = "Completed";
                }
            }
        }
    }
    return woStatus;
}

function opp_Status(opportunityId) {
    var oppStatus = "Lost";
    var fetchData = {
        opportunityid: opportunityId
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='opportunity'>",
					"    <attribute name='statecode' />",
					"    <filter type='and'>",
					"      <condition attribute='opportunityid' operator='eq' value='", fetchData.opportunityid/*61a1b52c-ff33-4345-bdad-b72f80a9cb9b*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");

    var opp = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (opp != undefined && opp != null && opp != "" && opp.length > 0) {
        for (i = 0; i < opp.length; i++) {
            var result = opp[i].attributes;
            if (result.statecode != undefined && result.statecode != null && result.statecode != "") {
                if (result.statecode.value == 1) {
                    oppStatus = "Won";
                }
                else if (result.statecode.value == 0) {
                    oppStatus = "Open";
                }
            }
        }
    }
    return oppStatus;
}
function BS_Status(opportunityId) {
    var isAssociated = "No";
    var fetchData = {
        ig1_associated: "1",
        ig1_opportunitytitle: opportunityId,
        statecode: "0"
    };
    var fetchXml = [
					"<fetch>",
					"  <entity name='ig1_bidsheet'>",
					"    <attribute name='ig1_status' />",
					"    <attribute name='ig1_associated' />",
					"    <filter type='and'>",
					"      <condition attribute='ig1_associated' operator='eq' value='", fetchData.ig1_associated/*1*/, "'/>",
					"      <condition attribute='ig1_opportunitytitle' operator='eq' value='", fetchData.ig1_opportunitytitle/*9a9e790e-59c2-4463-b5c1-1e94a3131cd4*/, "'/>",
					"      <condition attribute='statecode' operator='eq' value='", fetchData.statecode/*0*/, "'/>",
					"    </filter>",
					"  </entity>",
					"</fetch>",
    ].join("");

    var bs = XrmServiceToolkit.Soap.Fetch(fetchXml);
    if (bs != undefined && bs != null && bs != "") {
        if (bs.length > 0) {
            isAssociated = "Yes"
        }
    }
    return isAssociated;
}

function SetAArstatus(formContext) {

    debugger;
    var Isconfirm = confirm('Are you sure you want to set AR Status?');
    if (Isconfirm == false)
        return false;

    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150003 || status == 286150008 || status == 286150009) {
            formContext.getAttribute("ig1_projectstatus").setValue(286150007);
            formContext.getAttribute("ig1_holdpreviousstatus").setValue("286150007");
            formContext.data.entity.save();
            formContext.ui.refreshRibbon();
        }

    }
}

function disablesetprojectstatusBtn(formContext) {
    debugger;
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150006)
            return false;
        else
            return true;
    }
}



function disableAArStatus(formContext) {
    debugger;
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150003 || status == 286150008 || status == 286150009)
            return true;
        else
            return false;
    }
}

function disablecommissioningststus(formContext) {
    debugger;
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150003 || status == 286150007 || status == 286150009)
            return true;
        else
            return false;
    }
}

function SetCommissioningstatus(formContext) {
    debugger;
    var Isconfirm = confirm('Are you sure you want to set Commissioning status?');
    if (Isconfirm == false)
        return false;

    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status == 286150003 || status == 286150007 || status == 286150009) {
        formContext.getAttribute("ig1_projectstatus").setValue(286150008);
        formContext.getAttribute("ig1_holdpreviousstatus").setValue("286150008");
        formContext.data.entity.save();
        formContext.ui.refreshRibbon();
    }
}
function holdingForFinancials(formContext) {
    debugger;
    var Isconfirm = confirm('Are you sure you want to set Holding for Financials status?');
    if (Isconfirm == false)
        return false;
    formContext.getAttribute("ig1_projectstatus").setValue(286150009);
    formContext.getAttribute("ig1_holdpreviousstatus").setValue("286150009");
    formContext.data.entity.save();
    formContext.ui.refreshRibbon();
}
function enableHoldingForFinancials(formContext) {
    debugger;
    var status = formContext.getAttribute("ig1_projectstatus").getValue();
    if (status != undefined && status != null && status != "") {
        if (status == 286150003 || status == 286150007 || status == 286150008)
            return true;
        else
            return false;
    }
}

function commissioning(executionContext) {
    try {
        debugger;
        var formContext = executionContext.getFormContext();
        var tab = formContext.ui.tabs.get("Commissioning_Tab");
        if (tab != undefined && tab != null) {
            var projectStataus = formContext.getAttribute("ig1_projectstatus").getValue();
            if (projectStataus != undefined && projectStataus != null && projectStataus != "" && projectStataus == 286150008) {
                var currentUserRoles = formContext.context.getUserRoles();
                var isFinanceRoleExist = financeRoleExist(currentUserRoles);
                if (isFinanceRoleExist) {
                    tab.setVisible(true);
                }
                else {
                    tab.setVisible(false);
                }
            }
        }
        else {
            setTimeout(function () { commissioning(executionContext); }, 1000);
        }
        manageCommissionsOnload(executionContext)
    }
    catch (err) {
        alert(err.message);
    }
}
function financeRoleExist(currentUserRoles) {
    flag = false;
    for (i = 0; i < currentUserRoles.length; i++) {
        var fetchData = {
            roleid: currentUserRoles[i]
        };
        var fetchXml = [
						"<fetch>",
						"  <entity name='role'>",
						"    <attribute name='name' />",
						"    <filter>",
						"      <condition attribute='roleid' operator='eq' value='", fetchData.roleid/*role*/, "'/>",
						"    </filter>",
						"  </entity>",
						"</fetch>",
        ].join("");
        var ec = executeFetchXml("roles", fetchXml);
        if (ec.value.length > 0) {
            flag = true;
            break;
        }
    }
    return flag;
}

function manageCommissionsOnload(executionContext) {
    try {
        debugger;
        var formContext = executionContext.getFormContext();
        var salesRep = formContext.getAttribute("ig1_opportunityowner").getValue();
        var salesRep2 = formContext.getAttribute("ig1_salesrep2").getValue();
        var primaryDesigner = formContext.getAttribute("ig1_primarydesigner").getValue();
        var projectManager = formContext.getAttribute("ig1_woowner").getValue();
        var equipmentProjectManager = formContext.getAttribute("ig1_equipmentprojectmanager").getValue();
        var techProjectManager = formContext.getAttribute("ig1_techprojectmanager").getValue();
        var euSiteProjectManager = formContext.getAttribute("ig1_eusiteprojectmanager").getValue();
        var salesEngineer = formContext.getAttribute("ig1_salesengineer").getValue();
        var usSalesManager = formContext.getAttribute("ig1_ussalesmanager").getValue();
        var srProjectManager = formContext.getAttribute("ig1_srprojectmanager").getValue();
        var dirttProjectManager = formContext.getAttribute("ig1_dirttprojectmanager").getValue();
        var euSalesManager = formContext.getAttribute("ig1_eusalesmanager").getValue();
        var pmManager = formContext.getAttribute("ig1_pmmanager").getValue();
        var designManager = formContext.getAttribute("ig1_designmanager").getValue();
        var srDesigner = formContext.getAttribute("ig1_srdesigner").getValue();
        var dirttDesigner = formContext.getAttribute("ig1_dirttdesigner").getValue();
        var techDesigner = formContext.getAttribute("ig1_techdesigner").getValue();
        var equipmentDesigner = formContext.getAttribute("ig1_equipmentdesigner").getValue();
        var businessDevelopment = formContext.getAttribute("ig1_businessdevelopment").getValue();
        var operationsManager = formContext.getAttribute("ig1_operationsmanager").getValue();

        var salesRepPercent = formContext.getAttribute("ig1_salesrepcommissionpercent").getValue();
        var salesRep2Percent = formContext.getAttribute("ig1_salesrep2commissionpercent").getValue();
        var primaryDesignerPercent = formContext.getAttribute("ig1_primarydesignercommissionpercent").getValue();
        var projectManagerPercent = formContext.getAttribute("ig1_projectmanagercommissionpercent").getValue();

        if (salesRep == undefined || salesRep == null || salesRep == "") {
            formContext.getControl("ig1_salesrepcommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesrepcommissionpercent").setValue(null);
        }
        if (salesRep2 == undefined || salesRep2 == null || salesRep2 == "") {
            formContext.getControl("ig1_salesrep2commissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesrep2commissionpercent").setValue(null);
        }
        if (primaryDesigner == undefined || primaryDesigner == null || primaryDesigner == "") {
            formContext.getControl("ig1_primarydesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_primarydesignercommissionpercent").setValue(null);
        }
        if (projectManager == undefined || projectManager == null || projectManager == "") {
            formContext.getControl("ig1_projectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_projectmanagercommissionpercent").setValue(null);
        }
        if (equipmentProjectManager == undefined || equipmentProjectManager == null || equipmentProjectManager == "") {
            formContext.getControl("ig1_equipmentprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_equipmentprojectmanagercommissionpercent").setValue(null);
        }
        if (techProjectManager == undefined || techProjectManager == null || techProjectManager == "") {
            formContext.getControl("ig1_techprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_techprojectmanagercommissionpercent").setValue(null);
        }
        if (euSiteProjectManager == undefined || euSiteProjectManager == null || euSiteProjectManager == "") {
            formContext.getControl("ig1_eusiteprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_eusiteprojectmanagercommissionpercent").setValue(null);
        }
        if (salesEngineer == undefined || salesEngineer == null || salesEngineer == "") {
            formContext.getControl("ig1_salesengineercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesengineercommissionpercent").setValue(null);
        }
        if (usSalesManager == undefined || usSalesManager == null || usSalesManager == "") {
            formContext.getControl("ig1_ussalesmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_ussalesmanagercommissionpercent").setValue(null);
        }
        if (srProjectManager == undefined || srProjectManager == null || srProjectManager == "") {
            formContext.getControl("ig1_srprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_srprojectmanagercommissionpercent").setValue(null);
        }
        if (dirttProjectManager == undefined || dirttProjectManager == null || dirttProjectManager == "") {
            formContext.getControl("ig1_dirttprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_dirttprojectmanagercommissionpercent").setValue(null);
        }
        if (euSalesManager == undefined || euSalesManager == null || euSalesManager == "") {
            formContext.getControl("ig1_eusalesmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_eusalesmanagercommissionpercent").setValue(null);
        }
        if (pmManager == undefined || pmManager == null || pmManager == "") {
            formContext.getControl("ig1_pmmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_pmmanagercommissionpercent").setValue(null);
        }
        if (designManager == undefined || designManager == null || designManager == "") {
            formContext.getControl("ig1_designmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_designmanagercommissionpercent").setValue(null);
        }
        if (srDesigner == undefined || srDesigner == null || srDesigner == "") {
            formContext.getControl("ig1_srdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_srdesignercommissionpercent").setValue(null);
        }
        if (dirttDesigner == undefined || dirttDesigner == null || dirttDesigner == "") {
            formContext.getControl("ig1_dirttdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_dirttdesignercommissionpercent").setValue(null);
        }
        if (techDesigner == undefined || techDesigner == null || techDesigner == "") {
            formContext.getControl("ig1_techdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_techdesignercommissionpercent").setValue(null);
        }
        if (equipmentDesigner == undefined || equipmentDesigner == null || equipmentDesigner == "") {
            formContext.getControl("ig1_equipmentdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_equipmentdesignercommissionpercent").setValue(null);
        }
        if (businessDevelopment == undefined || businessDevelopment == null || businessDevelopment == "") {
            formContext.getControl("ig1_businessdevelopmentcommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_businessdevelopmentcommissionpercent").setValue(null);
        }
        if (operationsManager == undefined || operationsManager == null || operationsManager == "") {
            formContext.getControl("ig1_operationsmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_operationsmanagercommissionpercent").setValue(null);
        }

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_financeandadminroles?$select=ig1_primarydesignercommissionpercent,ig1_projectmanagercommissionpercent,ig1_salesrep2commissionpercent,ig1_salesrepcommissionpercent&$orderby=createdon desc", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    var salesrepcommissionpercent = results.value[0]["ig1_salesrepcommissionpercent"];
                    var salesrep2commissionpercent = results.value[0]["ig1_salesrep2commissionpercent"];
                    var primarydesignercommissionpercent = results.value[0]["ig1_primarydesignercommissionpercent"];
                    var projectmanagercommissionpercent = results.value[0]["ig1_projectmanagercommissionpercent"];

                    if (salesRep != undefined && salesRep != null && salesRep != "") {
                        if (salesRepPercent == undefined || salesRepPercent == null || salesRep == "") {
                            formContext.getAttribute("ig1_salesrepcommissionpercent").setValue(parseFloat(salesrepcommissionpercent));
                        }
                    }
                    if (salesRep2 != undefined && salesRep2 != null && salesRep2 != "") {
                        if (salesRep2Percent == undefined || salesRep2Percent == null || salesRep2Percent == "") {
                            formContext.getAttribute("ig1_salesrep2commissionpercent").setValue(parseFloat(salesrep2commissionpercent));
                        }
                    }
                    if (primaryDesigner != undefined && primaryDesigner != null && primaryDesigner != "") {
                        if (primaryDesignerPercent == undefined || primaryDesignerPercent == null || primaryDesignerPercent == "") {
                            formContext.getAttribute("ig1_primarydesignercommissionpercent").setValue(parseFloat(primarydesignercommissionpercent));
                        }
                    }
                    if (projectManager != undefined && projectManager != null && projectManager != "") {
                        if (projectManagerPercent == undefined || projectManagerPercent == null || projectManagerPercent == "") {
                            formContext.getAttribute("ig1_projectmanagercommissionpercent").setValue(parseFloat(projectmanagercommissionpercent));
                        }
                    }
                }
                else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();

    }
    catch (err) {
        alert(err.message);
    }
}

function manageCommissions(executionContext, roleType) {
    try {
        debugger;
        var formContext = executionContext.getFormContext();
        var salesRep = formContext.getAttribute("ig1_opportunityowner").getValue();
        var salesRep2 = formContext.getAttribute("ig1_salesrep2").getValue();
        var primaryDesigner = formContext.getAttribute("ig1_primarydesigner").getValue();
        var projectManager = formContext.getAttribute("ig1_woowner").getValue();
        var equipmentProjectManager = formContext.getAttribute("ig1_equipmentprojectmanager").getValue();
        var techProjectManager = formContext.getAttribute("ig1_techprojectmanager").getValue();
        var euSiteProjectManager = formContext.getAttribute("ig1_eusiteprojectmanager").getValue();
        var salesEngineer = formContext.getAttribute("ig1_salesengineer").getValue();
        var usSalesManager = formContext.getAttribute("ig1_ussalesmanager").getValue();
        var srProjectManager = formContext.getAttribute("ig1_srprojectmanager").getValue();
        var dirttProjectManager = formContext.getAttribute("ig1_dirttprojectmanager").getValue();
        var euSalesManager = formContext.getAttribute("ig1_eusalesmanager").getValue();
        var pmManager = formContext.getAttribute("ig1_pmmanager").getValue();
        var designManager = formContext.getAttribute("ig1_designmanager").getValue();
        var srDesigner = formContext.getAttribute("ig1_srdesigner").getValue();
        var dirttDesigner = formContext.getAttribute("ig1_dirttdesigner").getValue();
        var techDesigner = formContext.getAttribute("ig1_techdesigner").getValue();
        var equipmentDesigner = formContext.getAttribute("ig1_equipmentdesigner").getValue();
        var businessDevelopment = formContext.getAttribute("ig1_businessdevelopment").getValue();
        var operationsManager = formContext.getAttribute("ig1_operationsmanager").getValue();

        if (roleType == "ig1_opportunityowner" && (salesRep == undefined || salesRep == null || salesRep == "")) {
            formContext.getControl("ig1_salesrepcommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesrepcommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_opportunityowner") {
            formContext.getControl("ig1_salesrepcommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_salesrepcommissionpercent"), "ig1_salesrepcommissionpercent", executionContext);

        }
        if (roleType == "ig1_salesrep2" && (salesRep2 == undefined || salesRep2 == null || salesRep2 == "")) {
            formContext.getControl("ig1_salesrep2commissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesrep2commissionpercent").setValue(null);
        }
        else if (roleType == "ig1_salesrep2") {
            formContext.getControl("ig1_salesrep2commissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_salesrep2commissionpercent"), "ig1_salesrep2commissionpercent", executionContext);
        }
        if (roleType == "ig1_primarydesigner" && (primaryDesigner == undefined || primaryDesigner == null || primaryDesigner == "")) {
            formContext.getControl("ig1_primarydesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_primarydesignercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_primarydesigner") {
            formContext.getControl("ig1_primarydesignercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_primarydesignercommissionpercent"), "ig1_primarydesignercommissionpercent", executionContext);
        }
        if (roleType == "ig1_woowner" && (projectManager == undefined || projectManager == null || projectManager == "")) {
            formContext.getControl("ig1_projectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_projectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_woowner") {
            formContext.getControl("ig1_projectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_projectmanagercommissionpercent"), "ig1_projectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_equipmentprojectmanager" && (equipmentProjectManager == undefined || equipmentProjectManager == null || equipmentProjectManager == "")) {
            formContext.getControl("ig1_equipmentprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_equipmentprojectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_equipmentprojectmanager") {
            formContext.getControl("ig1_equipmentprojectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_equipmentprojectmanagercommissionpercent"), "ig1_equipmentprojectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_techprojectmanager" && (techProjectManager == undefined || techProjectManager == null || techProjectManager == "")) {
            formContext.getControl("ig1_techprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_techprojectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_techprojectmanager") {
            formContext.getControl("ig1_techprojectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_techprojectmanagercommissionpercent"), "ig1_techprojectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_eusiteprojectmanager" && (euSiteProjectManager == undefined || euSiteProjectManager == null || euSiteProjectManager == "")) {
            formContext.getControl("ig1_eusiteprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_eusiteprojectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_eusiteprojectmanager") {
            formContext.getControl("ig1_eusiteprojectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_eusiteprojectmanagercommissionpercent"), "ig1_eusiteprojectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_salesengineer" && (salesEngineer == undefined || salesEngineer == null || salesEngineer == "")) {
            formContext.getControl("ig1_salesengineercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_salesengineercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_salesengineer") {
            formContext.getControl("ig1_salesengineercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_salesengineercommissionpercent"), "ig1_salesengineercommissionpercent", executionContext);
        }
        if (roleType == "ig1_ussalesmanager" && (usSalesManager == undefined || usSalesManager == null || usSalesManager == "")) {
            formContext.getControl("ig1_ussalesmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_ussalesmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_ussalesmanager") {
            formContext.getControl("ig1_ussalesmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_ussalesmanagercommissionpercent"), "ig1_ussalesmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_srprojectmanager" && (srProjectManager == undefined || srProjectManager == null || srProjectManager == "")) {
            formContext.getControl("ig1_srprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_srprojectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_srprojectmanager") {
            formContext.getControl("ig1_srprojectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_srprojectmanagercommissionpercent"), "ig1_srprojectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_dirttprojectmanager" && (dirttProjectManager == undefined || dirttProjectManager == null || dirttProjectManager == "")) {
            formContext.getControl("ig1_dirttprojectmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_dirttprojectmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_dirttprojectmanager") {
            formContext.getControl("ig1_dirttprojectmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_dirttprojectmanagercommissionpercent"), "ig1_dirttprojectmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_eusalesmanager" && (euSalesManager == undefined || euSalesManager == null || euSalesManager == "")) {
            formContext.getControl("ig1_eusalesmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_eusalesmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_eusalesmanager") {
            formContext.getControl("ig1_eusalesmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_eusalesmanagercommissionpercent"), "ig1_eusalesmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_pmmanager" && (pmManager == undefined || pmManager == null || pmManager == "")) {
            formContext.getControl("ig1_pmmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_pmmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_pmmanager") {
            formContext.getControl("ig1_pmmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_pmmanagercommissionpercent"), "ig1_pmmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_designmanager" && (designManager == undefined || designManager == null || designManager == "")) {
            formContext.getControl("ig1_designmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_designmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_designmanager") {
            formContext.getControl("ig1_designmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_designmanagercommissionpercent"), "ig1_designmanagercommissionpercent", executionContext);
        }
        if (roleType == "ig1_srdesigner" && (srDesigner == undefined || srDesigner == null || srDesigner == "")) {
            formContext.getControl("ig1_srdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_srdesignercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_srdesigner") {
            formContext.getControl("ig1_srdesignercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_srdesignercommissionpercent"), "ig1_srdesignercommissionpercent", executionContext);
        }
        if (roleType == "ig1_dirttdesigner" && (dirttDesigner == undefined || dirttDesigner == null || dirttDesigner == "")) {
            formContext.getControl("ig1_dirttdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_dirttdesignercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_dirttdesigner") {
            formContext.getControl("ig1_dirttdesignercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_dirttdesignercommissionpercent"), "ig1_dirttdesignercommissionpercent", executionContext);
        }
        if (roleType == "ig1_techdesigner" && (techDesigner == undefined || techDesigner == null || techDesigner == "")) {
            formContext.getControl("ig1_techdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_techdesignercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_techdesigner") {
            formContext.getControl("ig1_techdesignercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_techdesignercommissionpercent"), "ig1_techdesignercommissionpercent", executionContext);
        }
        if (roleType == "ig1_equipmentdesigner" && (equipmentDesigner == undefined || equipmentDesigner == null || equipmentDesigner == "")) {
            formContext.getControl("ig1_equipmentdesignercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_equipmentdesignercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_equipmentdesigner") {
            formContext.getControl("ig1_equipmentdesignercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_equipmentdesignercommissionpercent"), "ig1_equipmentdesignercommissionpercent", executionContext);
        }
        if (roleType == "ig1_businessdevelopment" && (businessDevelopment == undefined || businessDevelopment == null || businessDevelopment == "")) {
            formContext.getControl("ig1_businessdevelopmentcommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_businessdevelopmentcommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_businessdevelopment") {
            formContext.getControl("ig1_businessdevelopmentcommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_businessdevelopmentcommissionpercent"), "ig1_businessdevelopmentcommissionpercent", executionContext);
        }
        if (roleType == "ig1_operationsmanager" && (operationsManager == undefined || operationsManager == null || operationsManager == "")) {
            formContext.getControl("ig1_operationsmanagercommissionpercent").setDisabled(true);
            formContext.getAttribute("ig1_operationsmanagercommissionpercent").setValue(null);
        }
        else if (roleType == "ig1_operationsmanager") {
            formContext.getControl("ig1_operationsmanagercommissionpercent").setDisabled(false);
            setDefaultCommission(formContext.getAttribute("ig1_operationsmanagercommissionpercent"), "ig1_operationsmanagercommissionpercent", executionContext);
        }
    }
    catch (err) {
        alert(err.message);
    }
}

function managePercentRange(executionContext, field) {
    try {
        debugger;
        var flag = false;
        var formContext = executionContext.getFormContext();
        var salesRep = formContext.getAttribute("ig1_salesrepcommissionpercent").getValue();
        var salesRep2 = formContext.getAttribute("ig1_salesrep2commissionpercent").getValue();
        var primaryDesigner = formContext.getAttribute("ig1_primarydesignercommissionpercent").getValue();
        var projectManager = formContext.getAttribute("ig1_projectmanagercommissionpercent").getValue();
        var equipmentProjectManager = formContext.getAttribute("ig1_equipmentprojectmanagercommissionpercent").getValue();
        var techProjectManager = formContext.getAttribute("ig1_techprojectmanagercommissionpercent").getValue();
        var euSiteProjectManager = formContext.getAttribute("ig1_eusiteprojectmanagercommissionpercent").getValue();
        var salesEngineer = formContext.getAttribute("ig1_salesengineercommissionpercent").getValue();
        var usSalesManager = formContext.getAttribute("ig1_ussalesmanagercommissionpercent").getValue();
        var srProjectManager = formContext.getAttribute("ig1_srprojectmanagercommissionpercent").getValue();
        var dirttProjectManager = formContext.getAttribute("ig1_dirttprojectmanagercommissionpercent").getValue();
        var euSalesManager = formContext.getAttribute("ig1_eusalesmanagercommissionpercent").getValue();
        var pmManager = formContext.getAttribute("ig1_pmmanagercommissionpercent").getValue();
        var designManager = formContext.getAttribute("ig1_designmanagercommissionpercent").getValue();
        var srDesigner = formContext.getAttribute("ig1_srdesignercommissionpercent").getValue();
        var dirttDesigner = formContext.getAttribute("ig1_dirttdesignercommissionpercent").getValue();
        var techDesigner = formContext.getAttribute("ig1_techdesignercommissionpercent").getValue();
        var equipmentDesigner = formContext.getAttribute("ig1_equipmentdesignercommissionpercent").getValue();
        var businessDevelopment = formContext.getAttribute("ig1_businessdevelopmentcommissionpercent").getValue();
        var operationsManager = formContext.getAttribute("ig1_operationsmanagercommissionpercent").getValue();

        var sumCommissionsPercent = parseFloat(0);

        if (salesRep != undefined && salesRep != null && salesRep != "") {
            sumCommissionsPercent += parseFloat(salesRep);
        }
        if (salesRep2 != undefined && salesRep2 != null && salesRep2 != "") {
            sumCommissionsPercent += parseFloat(salesRep2);
        }
        if (primaryDesigner != undefined && primaryDesigner != null && primaryDesigner != "") {
            sumCommissionsPercent += parseFloat(primaryDesigner);
        }
        if (projectManager != undefined && projectManager != null && projectManager != "") {
            sumCommissionsPercent += parseFloat(projectManager);
        }
        if (equipmentProjectManager != undefined && equipmentProjectManager != null && equipmentProjectManager != "") {
            sumCommissionsPercent += parseFloat(equipmentProjectManager);
        }
        if (techProjectManager != undefined && techProjectManager != null && techProjectManager != "") {
            sumCommissionsPercent += parseFloat(techProjectManager);
        }
        if (euSiteProjectManager != undefined && euSiteProjectManager != null && euSiteProjectManager != "") {
            sumCommissionsPercent += parseFloat(euSiteProjectManager);
        }
        if (salesEngineer != undefined && salesEngineer != null && salesEngineer != "") {
            sumCommissionsPercent += parseFloat(salesEngineer);
        }
        if (usSalesManager != undefined && usSalesManager != null && usSalesManager != "") {
            sumCommissionsPercent += parseFloat(usSalesManager);
        }
        if (srProjectManager != undefined && srProjectManager != null && srProjectManager != "") {
            sumCommissionsPercent += parseFloat(srProjectManager);
        }
        if (dirttProjectManager != undefined && dirttProjectManager != null && dirttProjectManager != "") {
            sumCommissionsPercent += parseFloat(dirttProjectManager);
        }
        if (euSalesManager != undefined && euSalesManager != null && euSalesManager != "") {
            sumCommissionsPercent += parseFloat(euSalesManager);
        }
        if (pmManager != undefined && pmManager != null && pmManager != "") {
            sumCommissionsPercent += parseFloat(pmManager);
        }
        if (designManager != undefined && designManager != null && designManager != "") {
            sumCommissionsPercent += parseFloat(designManager);
        }
        if (srDesigner != undefined && srDesigner != null && srDesigner != "") {
            sumCommissionsPercent += parseFloat(srDesigner);
        }
        if (dirttDesigner != undefined && dirttDesigner != null && dirttDesigner != "") {
            sumCommissionsPercent += parseFloat(dirttDesigner);
        }
        if (techDesigner != undefined && techDesigner != null && techDesigner != "") {
            sumCommissionsPercent += parseFloat(techDesigner);
        }
        if (equipmentDesigner != undefined && equipmentDesigner != null && equipmentDesigner != "") {
            sumCommissionsPercent += parseFloat(equipmentDesigner);
        }
        if (businessDevelopment != undefined && businessDevelopment != null && businessDevelopment != "") {
            sumCommissionsPercent += parseFloat(businessDevelopment);
        }
        if (operationsManager != undefined && operationsManager != null && operationsManager != "") {
            sumCommissionsPercent += parseFloat(operationsManager);
        }
        if (field == "ig1_salesrepcommissionpercent" && salesRep != undefined && salesRep != null && salesRep != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - salesRep);
            if (salesRep > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + salesRep);
                formContext.getAttribute("ig1_salesrepcommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_salesrep2commissionpercent" && salesRep2 != undefined && salesRep2 != null && salesRep2 != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - salesRep2);
            if (salesRep2 > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + salesRep2);
                formContext.getAttribute("ig1_salesrep2commissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_primarydesignercommissionpercent" && primaryDesigner != undefined && primaryDesigner != null && primaryDesigner != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - primaryDesigner);
            if (primaryDesigner > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + primaryDesigner);
                formContext.getAttribute("ig1_primarydesignercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_projectmanagercommissionpercent" && projectManager != undefined && projectManager != null && projectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - projectManager);
            if (projectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + projectManager);
                formContext.getAttribute("ig1_projectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_equipmentprojectmanagercommissionpercent" && equipmentProjectManager != undefined && equipmentProjectManager != null && equipmentProjectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - equipmentProjectManager);
            if (equipmentProjectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + equipmentProjectManager);
                formContext.getAttribute("ig1_equipmentprojectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_techprojectmanagercommissionpercent" && techProjectManager != undefined && techProjectManager != null && techProjectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - techProjectManager);
            if (techProjectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + techProjectManager);
                formContext.getAttribute("ig1_techprojectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_eusiteprojectmanagercommissionpercent" && euSiteProjectManager != undefined && euSiteProjectManager != null && euSiteProjectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - euSiteProjectManager);
            if (euSiteProjectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + euSalesManager);
                formContext.getAttribute("ig1_eusiteprojectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_salesengineercommissionpercent" && salesEngineer != undefined && salesEngineer != null && salesEngineer != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - salesEngineer);
            if (salesEngineer > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + salesEngineer);
                formContext.getAttribute("ig1_salesengineercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_ussalesmanagercommissionpercent" && usSalesManager != undefined && usSalesManager != null && usSalesManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - usSalesManager);
            if (usSalesManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + usSalesManager);
                formContext.getAttribute("ig1_ussalesmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_srprojectmanagercommissionpercent" && srProjectManager != undefined && srProjectManager != null && srProjectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - srProjectManager);
            if (srProjectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + srProjectManager);
                formContext.getAttribute("ig1_srprojectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_dirttprojectmanagercommissionpercent" && dirttProjectManager != undefined && dirttProjectManager != null && dirttProjectManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - dirttProjectManager);
            if (dirttProjectManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + dirttProjectManager);
                formContext.getAttribute("ig1_dirttprojectmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_eusalesmanagercommissionpercent" && euSalesManager != undefined && euSalesManager != null && euSalesManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - euSalesManager);
            if (euSalesManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + euSalesManager);
                formContext.getAttribute("ig1_eusalesmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_pmmanagercommissionpercent" && pmManager != undefined && pmManager != null && pmManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - pmManager);
            if (pmManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + pmManager);
                formContext.getAttribute("ig1_pmmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_designmanagercommissionpercent" && designManager != undefined && designManager != null && designManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - designManager);
            if (designManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + designManager);
                formContext.getAttribute("ig1_designmanagercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_srdesignercommissionpercent" && srDesigner != undefined && srDesigner != null && srDesigner != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - srDesigner);
            if (srDesigner > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + srDesigner);
                formContext.getAttribute("ig1_srdesignercommissionpercent").setValue(null);

            }
        }
        else if (field == "ig1_dirttdesignercommissionpercent" && dirttDesigner != undefined && dirttDesigner != null && dirttDesigner != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - dirttDesigner);
            if (dirttDesigner > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + dirttDesigner);
                formContext.getAttribute("ig1_dirttdesignercommissionpercent").setValue(null);
            }
        }
        else if (field == "ig1_techdesignercommissionpercent" && techDesigner != undefined && techDesigner != null && techDesigner != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - techDesigner);
            if (techDesigner > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + techDesigner);
                formContext.getAttribute("ig1_techdesignercommissionpercent").setValue(null);
            }
        }
        else if (field == "ig1_equipmentdesignercommissionpercent" && equipmentDesigner != undefined && equipmentDesigner != null && equipmentDesigner != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - equipmentDesigner);
            if (equipmentDesigner > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + equipmentDesigner);
                formContext.getAttribute("ig1_equipmentdesignercommissionpercent").setValue(null);
            }
        }
        else if (field == "ig1_businessdevelopmentcommissionpercent" && businessDevelopment != undefined && businessDevelopment != null && businessDevelopment != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - businessDevelopment);
            if (businessDevelopment > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + businessDevelopment);
                formContext.getAttribute("ig1_businessdevelopmentcommissionpercent").setValue(null);
            }
        }
        else if (field == "ig1_operationsmanagercommissionpercent" && operationsManager != undefined && operationsManager != null && operationsManager != "") {
            debugger;
            var availablePercentage = parseFloat(100) - (sumCommissionsPercent - operationsManager);
            if (operationsManager > availablePercentage) {
                alert("Percentage Remaining: " + availablePercentage + "% and Percentage Entered: " + operationsManager);
                formContext.getAttribute("ig1_operationsmanagercommissionpercent").setValue(null);
            }
        }
    }
    catch (err) {
        alert(err.message);
    }
}

function setDefaultCommission(roleCommissionPercent, fieldName, executionContext) {
    try {
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/ig1_financeandadminroles?$select=" + fieldName + "&$filter=" + fieldName + " ne null", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var results = JSON.parse(this.response);
                    var commissionpercent = parseFloat(results.value[0][fieldName]);
                    roleCommissionPercent.setValue(commissionpercent);
                    managePercentRange(executionContext, fieldName);
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }
    catch (err) {
        alert(err.message);
    }
}