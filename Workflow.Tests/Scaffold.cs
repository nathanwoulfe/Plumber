using Workflow.Models;

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
    }
}
