using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Workflow.Models
{
    /// <summary>
    /// A UI-ready sub-set of properties from the WorkflowTaskPoco
    /// </summary>
    public class WorkflowTask
    {
        [JsonProperty("nodeId")]
        public int NodeId { get; set; }

        [JsonProperty("taskId")]
        public int TaskId { get; set; }

        [JsonProperty("approvalGroupId")]
        public int? ApprovalGroupId { get; set; }

        [JsonProperty("currentStep")]
        public int CurrentStep { get; set; }

        [JsonProperty("requestedById")]
        public int RequestedById { get; set; }



        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("typeId")]
        public int TypeId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("instanceStatus")]
        public string InstanceStatus { get; set; }

        [JsonProperty("cssStatus")]
        public string CssStatus { get; set; }

        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

        [JsonProperty("requestedBy")]
        public string RequestedBy { get; set; }

        [JsonProperty("requestedOn")]
        public string RequestedOn { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("approvalGroup")]
        public string ApprovalGroup { get; set; }

        [JsonProperty("completedBy")]
        public string CompletedBy { get; set; }

        [JsonProperty("completedOn")]
        public string CompletedOn { get; set; }


        [JsonProperty("permissions")]
        public List<UserGroupPermissionsPoco> Permissions { get; set; }


        [JsonProperty("instanceGuid")]
        public Guid InstanceGuid { get; set; }
    }
}

