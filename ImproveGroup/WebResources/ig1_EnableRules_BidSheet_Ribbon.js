function enableRule_Activate()
{
        var flag=true;
     
	    var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	if(bidSheetStatus!="286150001")
	{
		flag=false;
	}
	return flag;
}

function enableRule_Revise()
{
	var flag=true;
	var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	if(bidSheetStatus=="286150001" || bidSheetStatus=="286150002")
	{
		flag=false;
	}
	return flag;
}

function enableRule_Reopen()
{
	var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
	if(bidSheetStatus=="286150002")
		return true;
	else
		return false;
	
	
}


//This will disable new, delete and edit button from the subGrids if bidSheet satus not draft
function enableRule_SubGrids()
{
	var bidSheetStatus=Xrm.Page.getAttribute("ig1_status").getValue();
    if(bidSheetStatus!="286150001" && bidSheetStatus!="286150003")	
		return false;
	else
		return true;
}

function enableRule_ProjectRecord_SubGrids(executionContext)
{
    var entityName = executionContext._entityName;
   
    if(entityName=="ig1_projectrecord")
    {
        return false;
    }
}