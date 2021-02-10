
function AssociateDocumentLocation()
{
	var control = context.getEventSource();
	var row= control.getParent();
	var laborRate=row.attributes.get("ig1_laborrate").getValue();
	var luExtend = row.attributes.get("ig1_luextend").getValue();
	var pmLaborSme=parseFloat(laborRate)*parseFloat(luExtend);
	row.attributes.get("ig1_pmlaborsme").setValue(pmLaborSme);
	calculateCategoryTotalCost(context);
	//Updating Associated Cost Entity basis the change
	UpdateAssociatedCostRecordOnCellChange(context);
}
