function updateIsGrouped()
{
    var productGroup= Xrm.Page.getAttribute('ig1_productgroup').getValue();
   if(productGroup != null && productGroup != undefined && productGroup != "")
   { 
     Xrm.Page.getAttribute("ig1_isgrouped").setValue(1);
   }
   else
   {
    Xrm.Page.getAttribute("ig1_isgrouped").setValue(0);
   }
}