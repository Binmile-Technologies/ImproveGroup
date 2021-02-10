using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace WF_SetOpportunityReopenDateAndUser
{
    public class SetOpportunityReopenDateAndUser : CodeActivity
    {
        [Output("Current User")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> CurrentUser { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                CurrentUser.Set(context, new EntityReference("systemuser", workflowContext.InitiatingUserId));
            }
            catch (Exception ex)
            {
                throw new InvalidWorkflowException("Error in Custom Workflow SetOpportunityReopenDateAndUser "+ex);
            }
        }
    }
}
