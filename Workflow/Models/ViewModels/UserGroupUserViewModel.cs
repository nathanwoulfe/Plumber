using Newtonsoft.Json;

namespace Workflow.Models
{
    public class UserGroupUserModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}