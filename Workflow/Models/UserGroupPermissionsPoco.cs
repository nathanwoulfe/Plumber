﻿using System.Runtime.InteropServices.WindowsRuntime;
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
        public int Id { get; set; }

        [Column("GroupId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int GroupId { get; set; }

        [Column("NodeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int NodeId { get; set; }

        [Column("ContentTypeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int ContentTypeId { get; set; }

        [Column("Permission")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int Permission { get; set; }

        [ResultColumn]
        public string NodeName => NodeId > 0 ? _utility.GetNodeName(NodeId) : string.Empty;

        [ResultColumn]
        public string ContentTypeName => ContentTypeId > 0 ? _utility.GetContentType(ContentTypeId).Name : string.Empty;

        [ResultColumn]
        public string ContentTypeAlias => ContentTypeId > 0 ? _utility.GetContentType(ContentTypeId).Alias : string.Empty;

        [ResultColumn]
        public UserGroupPoco UserGroup { get; set; }
        
    }  
}
