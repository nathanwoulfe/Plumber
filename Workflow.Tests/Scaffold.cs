using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Workflow.Models;
using Workflow.Services;

namespace Workflow.Tests
{
    public static class Scaffold
    {
        public static void AddTables()
        {
            // ensure required tables exist
            Persistence.Helper().CreateTable<UserGroupPoco>();
            Persistence.Helper().CreateTable<User2UserGroupPoco>();
            Persistence.Helper().CreateTable<UserGroupPermissionsPoco>();
            Persistence.Helper().CreateTable<WorkflowSettingsPoco>();
        }

        public static void AddContent()
        {
            var service = new ImportExportService();
            var model = ReadFromJsonFile<ImportExportModel>(@"Config.json");

            service.Import(model);
        }

        public static User2UserGroupPoco GetUser2UserGroupPoco(int groupId)
        {
            int id = Utility.RandomInt();

            return new User2UserGroupPoco
            {
                GroupId = groupId,
                Id = id,
                UserId = id
            };
        }

        private static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                string fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                reader?.Close();
            }
        }
    }
}
