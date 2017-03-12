namespace Workflow.Models
{
    public class SettingsModel
    {
        public string DefaultApprover { get; set; }
        public string Email { get; set; }
        public string EditUrl { get; set; }
        public string SiteUrl { get; set; }
        public int FlowType { get; set; }
    }

    /// <summary>
    /// The permitted flow types
    /// Other -> all groups the author is not a member of
    /// All -> all groups 
    /// Exclude -> all groups, but don't notify the orignal author for approval requests
    /// </summary>
    public enum FlowType
    {
        Other = 0,
        All = 1,
        Exclude = 2
    }
}
