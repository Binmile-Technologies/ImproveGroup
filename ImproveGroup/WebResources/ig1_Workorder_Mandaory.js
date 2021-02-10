function SetRequiredLevel(executionContext)    
{
  debugger;
 var formContext = executionContext.getFormContext();

var level ="required";

 setTimeout(function(){
        formContext.getAttribute("msdyn_workorder").setRequiredLevel(level);
    },500);
}