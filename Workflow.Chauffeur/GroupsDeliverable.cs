using Chauffeur;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Workflow.Services;

namespace Workflow.Chauffeur
{
    [DeliverableName("workflow-group")]
    [DeliverableAlias("wg")]
    public class GroupsDeliverable : Deliverable
    {
        private readonly IGroupService groupService;

        public GroupsDeliverable(
            TextReader reader,
            TextWriter writer,
            IGroupService groupService) : base(reader, writer)
        {
            this.groupService = groupService;
        }

        public async override Task<DeliverableResponse> Run(string command, string[] args)
        {
            if (!args.Any())
            {
                await Out.WriteLineAsync("You didn't provide an operation, please provide an operation to perform against the groups");
                return DeliverableResponse.Continue;
            }

            var operation = args[0];

            switch (operation.ToLower())
            {
                case "get-all":
                    await GetAll();
                    return DeliverableResponse.Continue;

                case "add":
                    return await Add(args[1]);

                default:
                    await Out.WriteLineAsync($"The operation {operation} is not supported");
                    return DeliverableResponse.Continue;
            }
        }

        private async Task<DeliverableResponse> Add(string name)
        {
            var group = await groupService.CreateUserGroupAsync(name);

            if (group == null)
                await Out.WriteLineAsync($"Unable to create a group with the name '{name}', it already exists.");
            else
                await Out.WriteLineAsync($"A group named {name} was created");

            return DeliverableResponse.Continue;
        }

        private async Task GetAll()
        {
            var groups = await groupService.GetUserGroupsAsync();

            if (!groups.Any())
            {
                await Out.WriteLineAsync("There are no groups defined at the moment");
            }
            else
            {
                await Out.WriteLineAsync("The following groups were found:");
                foreach (var group in groups)
                    await Out.WriteLineAsync($"\tName: {group.Name}, Email: {group.GroupEmail}");
            }
        }
    }
}
