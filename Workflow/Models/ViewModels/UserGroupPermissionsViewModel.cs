using Newtonsoft.Json;

namespace Workflow.Models.ViewModels
{
    public class UserGroupPermissionsViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nodeId")]
        public string NodeId { get; set; }

        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [JsonProperty("permission")]
        public int Permission { get; set; }

        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
