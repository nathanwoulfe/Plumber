using Newtonsoft.Json;
using Umbraco.Core.Models;
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
        private readonly IContentType _contentType;

        public UserGroupPermissionsPoco()
        {
            _contentType = ContentTypeId > 0 ? _utility.GetContentType(ContentTypeId) : null; 
        }

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
        public string ContentTypeName => _contentType != null ? _contentType.Name : MagicStrings.NoContentType;

        [ResultColumn]
        [JsonProperty("contentTypeAlias")]
        public string ContentTypeAlias => _contentType != null ? _contentType.Alias : MagicStrings.NoContentType;

        [ResultColumn]
        [JsonProperty("userGroup")]
        public UserGroupPoco UserGroup { get; set; }
        
    }  
}
