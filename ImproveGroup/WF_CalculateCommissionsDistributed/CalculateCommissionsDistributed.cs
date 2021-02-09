using System;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk;

namespace WF_CalculateCommissionsDistributed
{
    public class CalculateCommissionsDistributed : CodeActivity
    {
        [RequiredArgument]
        [Input("Sales Rep Commission Amount")]
        public InArgument<Money> SalesRepCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Sales Rep2 Commission Amount")]
        public InArgument<Money> SalesRep2CommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Primary Designer Commission Amount")]
        public InArgument<Money> PrimaryDesignerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Project Manager Commission Amount")]
        public InArgument<Money> ProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("DIRTT Project Manager Commission Amount")]
        public InArgument<Money> DIRTTProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("SR Project Manager Commission Amount")]
        public InArgument<Money> SRProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Equipment Project Manager Commission Amount")]
        public InArgument<Money> EquipmentProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Tech Project Manager Commission Amount")]
        public InArgument<Money> TechProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("EU Site Project Manager Commission Amount")]
        public InArgument<Money> EUSiteProjectManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Sales Engineer Commission Amount")]
        public InArgument<Money> SalesEngineerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("US Sales Manager Commission Amount")]
        public InArgument<Money> USSalesManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("EU Sales Manager Commission Amount")]
        public InArgument<Money> EUSalesManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("PM Manager Commission Amount")]
        public InArgument<Money> PMManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Design Manager Commission Amount")]
        public InArgument<Money> DesignManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("SR Designer Commission Amount")]
        public InArgument<Money> SRDesignerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("DIRTT Designer Commission Amount")]
        public InArgument<Money> DIRTTDesignerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Tech Designer Commission Amount")]
        public InArgument<Money> TechDesignerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Equipment Designer Commission Amount")]
        public InArgument<Money> EquipmentDesignerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Business Development Commission Amount")]
        public InArgument<Money> BusinessDevelopmentCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Operations Manager Commission Amount")]
        public InArgument<Money> OperationsManagerCommissionAmount { get; set; }
        [RequiredArgument]
        [Input("Commissionable Amount")]
        public InArgument<Money> CommissionableAmount { get; set; }
        
        [Output("Commissions Distributed")]
        public OutArgument<Money> CommissionsDistributed { get; set; }

        [Output("Commissions Remaining")]
        public OutArgument<Money> CommissionsRemaining { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            try
            {
                decimal commissionsDistributed = Convert.ToDecimal(0);
                decimal commissionsRemaining = Convert.ToDecimal(0);

                Money salesRepCommissionAmount = new Money(0);
                Money salesRep2CommissionAmount = new Money(0);
                Money primaryDesignerCommissionAmount = new Money(0);
                Money projectManagerCommissionAmount = new Money(0);
                Money dIRTTProjectManagerCommissionAmount = new Money(0);
                Money sRProjectManagerCommissionAmount = new Money(0);
                Money equipmentProjectManagerCommissionAmount = new Money(0);
                Money techProjectManagerCommissionAmount = new Money(0);
                Money eUSiteProjectManagerCommissionAmount = new Money(0);
                Money salesEngineerCommissionAmount = new Money(0);
                Money uSSalesManagerCommissionAmount = new Money(0);
                Money eUSalesManagerCommissionAmount = new Money(0);
                Money pMManagerCommissionAmount = new Money(0);
                Money designManagerCommissionAmount = new Money(0);
                Money sRDesignerCommissionAmount = new Money(0);
                Money dIRTTDesignerCommissionAmount = new Money(0);
                Money techDesignerCommissionAmount = new Money(0);
                Money equipmentDesignerCommissionAmount = new Money(0);
                Money businessDevelopmentCommissionAmount = new Money(0);
                Money operationsManagerCommissionAmount = new Money(0);
                Money commissionableAmount = new Money(0);

                if (SalesRepCommissionAmount.Get(executionContext) != null)
                {
                   salesRepCommissionAmount = SalesRepCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(salesRepCommissionAmount.Value);
                }
                if (SalesRep2CommissionAmount.Get(executionContext) != null)
                {
                    salesRep2CommissionAmount = SalesRep2CommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(salesRep2CommissionAmount.Value);
                }
                if (PrimaryDesignerCommissionAmount.Get(executionContext)!=null)
                { 
                    primaryDesignerCommissionAmount = PrimaryDesignerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(primaryDesignerCommissionAmount.Value);
                }
                if (ProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    projectManagerCommissionAmount = ProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(projectManagerCommissionAmount.Value);
                }
                if (DIRTTProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    dIRTTProjectManagerCommissionAmount = DIRTTProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(dIRTTProjectManagerCommissionAmount.Value);
                }
                if (SRProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    sRProjectManagerCommissionAmount = SRProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(sRProjectManagerCommissionAmount.Value);
                }
                if (EquipmentProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    equipmentProjectManagerCommissionAmount = EquipmentProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(equipmentProjectManagerCommissionAmount.Value);
                }
                if (TechProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    techProjectManagerCommissionAmount = TechProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(techProjectManagerCommissionAmount.Value);
                }
                if (EUSiteProjectManagerCommissionAmount.Get(executionContext) != null)
                {
                    eUSiteProjectManagerCommissionAmount = EUSiteProjectManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(eUSiteProjectManagerCommissionAmount.Value);
                }
                if (SalesEngineerCommissionAmount.Get(executionContext) != null)
                {
                    salesEngineerCommissionAmount = SalesEngineerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(salesEngineerCommissionAmount.Value);
                }
                if (USSalesManagerCommissionAmount.Get(executionContext) != null)
                {
                    uSSalesManagerCommissionAmount = USSalesManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(uSSalesManagerCommissionAmount.Value);
                }
                if (EUSalesManagerCommissionAmount.Get(executionContext) != null)
                {
                    eUSalesManagerCommissionAmount = EUSalesManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(eUSalesManagerCommissionAmount.Value);
                }
                if (PMManagerCommissionAmount.Get(executionContext) != null)
                {
                    pMManagerCommissionAmount = PMManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(pMManagerCommissionAmount.Value);
                }
                if (DesignManagerCommissionAmount.Get(executionContext) != null)
                {
                    designManagerCommissionAmount = DesignManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(designManagerCommissionAmount.Value);
                }
                if (SRDesignerCommissionAmount.Get(executionContext) != null)
                {
                    sRDesignerCommissionAmount = SRDesignerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(sRDesignerCommissionAmount.Value);
                }
                if (DIRTTDesignerCommissionAmount.Get(executionContext) != null)
                {
                    dIRTTDesignerCommissionAmount = DIRTTDesignerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(dIRTTDesignerCommissionAmount.Value);
                }
                if (TechDesignerCommissionAmount.Get(executionContext) != null)
                {
                    techDesignerCommissionAmount = TechDesignerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(techDesignerCommissionAmount.Value);
                }
                if (EquipmentDesignerCommissionAmount.Get(executionContext) != null)
                {
                    equipmentDesignerCommissionAmount = EquipmentDesignerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(equipmentDesignerCommissionAmount.Value);
                }
                if (BusinessDevelopmentCommissionAmount.Get(executionContext) != null)
                {
                    businessDevelopmentCommissionAmount = BusinessDevelopmentCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(businessDevelopmentCommissionAmount.Value);
                }
                if (OperationsManagerCommissionAmount.Get(executionContext) != null)
                {
                    operationsManagerCommissionAmount = OperationsManagerCommissionAmount.Get(executionContext);
                    commissionsDistributed += Convert.ToDecimal(operationsManagerCommissionAmount.Value);
                }
                if (CommissionableAmount.Get(executionContext) != null)
                {
                    commissionableAmount = CommissionableAmount.Get(executionContext);
                }

                commissionsRemaining = Convert.ToDecimal(commissionableAmount.Value) - commissionsDistributed;

                CommissionsDistributed.Set(executionContext, new Money(commissionsDistributed));
                CommissionsRemaining.Set(executionContext, new Money(commissionsRemaining));
                
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in Custom Workflow CalculateCommissionsDistributed " + ex);
            }
        }
    }
}
