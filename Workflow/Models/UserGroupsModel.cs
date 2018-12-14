using Newtonsoft.Json;

namespace Workflow.Models
{ 
    //public class UserGroupModel
    //{
    //    public string Name { get; set; }
    //    public string Alias { get; set; }
    //    public string Description { get; set; }
    //    public string GroupEmail { get; set; }
    //    public string UsersSummary { get; set; }
    //    public int GroupId { get; set; }
    //    public List<UserGroupUserModel> Users { get; set; }
    //    public List<UserGroupPermissionsModel> Permissions { get; set; }

    //    public UserGroupModel()
    //    {
    //        Users = new List<UserGroupUserModel>();
    //        Permissions = new List<UserGroupPermissionsModel>();
    //    }
    //}

    public class UserGroupPermissionsModel
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

    public class UserGroupUserModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}