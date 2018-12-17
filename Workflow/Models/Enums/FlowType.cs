namespace Workflow.Models
{
    /// <summary>
    /// The permitted flow types
    /// Explicit -> all groups, regardles of original author membership
    /// Implicit -> approval is implied when original author is in the approving group. Default behaviour 
    /// </summary>
    public enum FlowType
    {
        Explicit = 0,
        Implicit = 1
    }
}
