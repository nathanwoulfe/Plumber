using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.preview;
using Umbraco.Core.IO;
using Workflow.Services.Interfaces;

namespace Workflow.Services
{
    public class PreviewService : IPreviewService
    {
        private const string TargetPath = "/app_plugins/workflow/preview";

        public void Generate(int nodeId, Guid workflowInstanceGuid)
        {
            // yes, these are obsolete but this is how preview works...
            var d = new Document(nodeId);
            var user = new User(0);
            var pc = new PreviewContent(user, workflowInstanceGuid, false);

            pc.PrepareDocument(user, d, true);
            pc.SavePreviewSet();

            Copy(workflowInstanceGuid);
        }

        /// <summary>
        /// Delete from /app_plugins/workflow/preview
        /// </summary>
        /// <param name="guid"></param>
        public void Delete(Guid guid)
        {

        }

        /// <summary>
        /// Get the file contents from /app_plugins/workflow/preview
        /// </summary>
        /// <param name="guid"></param>
        public XmlDocument Fetch(Guid guid)
        {
            // get from preview folder
            var previewDir = new DirectoryInfo(IOHelper.MapPath(TargetPath));
            IEnumerable<FileInfo> previewFiles = previewDir.EnumerateFiles("*").ToArray();

            FileInfo previewFile = previewFiles.FirstOrDefault(f => f.Name.Contains(guid.ToString()));

            // if the set doesn't exist, bail
            if (previewFile == null)
            {
                return null;
            }

            var doc = new XmlDocument();
            doc.Load(IOHelper.MapPath(previewFile.FullName));

            return doc;
        }

        /// <summary>
        /// Copy the preview set into /app_plugins/workflow/preview
        /// </summary>
        /// <param name="guid"></param>
        private static void Copy(Guid guid)
        {
            string previewFileName = $"{guid}.config";

            // get from preview folder
            var previewDir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Preview));
            IEnumerable<FileInfo> previewFiles = previewDir.EnumerateFiles("*").ToArray();

            FileInfo previewFile = previewFiles.FirstOrDefault(f => f.Name.Contains(guid.ToString()));

            // if the set doesn't exist, bail
            if (previewFile == null)
            {
                return;
            }

            var dir = new DirectoryInfo(IOHelper.MapPath(TargetPath));
            if (!dir.Exists)
            {
                dir.Create();
            }

            string previewFilePath = IOHelper.MapPath($"{TargetPath}/{previewFileName}");

            previewFile.CopyTo(previewFilePath, true);
            previewFile.Delete();
        }
    }
}
