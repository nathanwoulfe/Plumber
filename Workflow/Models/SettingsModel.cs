namespace Workflow.Models
{
    //public class SettingsModel
    //{
    //    public string DefaultApprover { get; set; }
    //    public string Email { get; set; }
    //    public string EditUrl { get; set; }
    //    public string SiteUrl { get; set; }
    //    public int FlowType { get; set; }
    //    public bool SendNotifications { get; set; }
    //    public bool LockIfActive { get; set; }
    //}

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

    public class PackageVersion
    {
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public string ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
        public string PackageUrl { get; set; }
        public string PackageName { get; set; }

        public bool OutOfDate { get; set; }
    }
}
