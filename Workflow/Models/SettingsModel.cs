using System;

namespace Workflow.Models
{
    public class SettingsModel
    {
        public string DefaultApprover { get; set; }
        public string Email { get; set; }
        public string EditUrl { get; set; }
        public string SiteUrl { get; set; }
        public int FlowType { get; set; }
        public bool SendNotifications { get; set; }
    }

    /// <summary>
    /// The permitted flow types
    /// Other -> all groups the author is not a member of
    /// All -> all groups 
    /// Exclude -> all groups, but don't notify the orignal author for approval requests
    /// </summary>
    [Obsolete("This is being revised and will be removed before v1")]
    public enum FlowType
    {
        Other = 0,
        All = 1,
        Exclude = 2
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
