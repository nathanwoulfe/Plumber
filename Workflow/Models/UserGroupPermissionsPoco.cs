using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Workflow.Helpers;

namespace Workflow.Models
{
    [TableName("WorkflowUserGroupPermissions")]
    [ExplicitColumns]
    [PrimaryKey("Id", autoIncrement = true)]
    public class UserGroupPermissionsPoco
    {
        private readonly Utility _utility = new Utility();

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Column("GroupId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [Column("NodeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("nodeId")]
        public int NodeId { get; set; }

        [Column("ContentTypeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("Permission")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [JsonProperty("permission")]
        public int Permission { get; set; }

        [ResultColumn]
        [JsonProperty("nodeName")]
        public string NodeName => NodeId > 0 ? _utility.GetNodeName(NodeId) : string.Empty;

        [ResultColumn]
        [JsonProperty("contentTypeName")]
        public string ContentTypeName => ContentTypeId > 0 ? _utility.GetContentType(ContentTypeId).Name : string.Empty;

        [ResultColumn]
        [JsonProperty("contentTypeAlias")]
        public string ContentTypeAlias => ContentTypeId > 0 ? _utility.GetContentType(ContentTypeId).Alias : string.Empty;

        [ResultColumn]
        [JsonProperty("userGroup")]
        public UserGroupPoco UserGroup { get; set; }
        
    }  
}
