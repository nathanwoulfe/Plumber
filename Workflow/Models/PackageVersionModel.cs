namespace Workflow.Models
{
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
