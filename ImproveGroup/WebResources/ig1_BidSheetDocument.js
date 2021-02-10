/* This function is used to display the grid of sharepoint documents grid in the form. */
function documentSubgrid()
{    
var recordId = Xrm.Page.data.entity.getId().replace(/[{}]/g, “”);
var iFrame = Xrm.Page.getControl(“IFRAME_Documents”);
var currentFormId = Xrm.Page.ui.formSelector.getCurrentItem().getId();
var oTypeCode = Xrm.Page.context.getQueryStringParameters().etc;
if (iFrame != null)
    {
//Build the url for document grid using the record id.
var url = Xrm.Page.context.getClientUrl() + “/userdefined/areas.aspx?formid=8b18c580-1d52-4cd8-b30f-f490809a61c2&inlineEdit=1&navItemName=Documents&oId=%7bAD412C3C-FF63-E911-A959-000D3A1D52E7%7d&oType=10162&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White”;
//Sets the source url for IFrame
        Xrm.Page.getControl(“IFRAME_Documents”).setSrc(url);
    }
}