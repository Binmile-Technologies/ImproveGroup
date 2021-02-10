function SetDocumentFrame() {

    //You can see what the url should be by navigating to the 'Documents' area under related records, viewing the page soure
    //and looking for 'areaSPDocuments'. The formid appears to be nothing more than a random guid value and not tied to anything 
    //specific in your org. 

    //Use: Make sure Document Management is enabled for the entity (helps to turn on automatic folder creation)
    //     Add a web resource with this code to the form 
    //     Execute this function during the form's OnLoad event

    var url = Xrm.Page.context.getClientUrl() +
        "/userdefined/areas.aspx?formid=8b18c580-1d52-4cd8-b30f-f490809a61c2&inlineEdit=1&navItemName=Documents&oId=%7b" +
        Xrm.Page.data.entity.getId().replace("{", "").replace("}", "") + "%7d&oType=" +
        Xrm.Page.context.getQueryStringParameters().etc + 
        "&pagemode=iframe&rof=true&security=852023&tabSet=areaSPDocuments&theme=Outlook15White";

    Xrm.Page.getControl("IFRAME_BidsheetBOM").setSrc(url); //Replace IFRAME_??? with actual IFRAME name
}