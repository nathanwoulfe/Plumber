using Chauffeur.TestingTools;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Repositories.Interfaces;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class SettingsServiceTests : UmbracoHostTestBase
    {
        private readonly ISettingsService _service;
        private readonly IPocoRepository _repo;

        public SettingsServiceTests()
        {
            Host.Run(new[] {"install y"}).Wait();

            Scaffold.Run();

            _service = new SettingsService();
            _repo = new PocoRepository();
        }

        [Fact]
        public void Can_Get_Settings()
        {
            WorkflowSettingsPoco settings = _service.GetSettings();

            Assert.NotNull(settings);
            Assert.False(settings.LockIfActive);
            Assert.Equal(1, settings.Id);
            Assert.Equal(0, settings.FlowType);
        }

        [Fact]
        public void Can_Update_Settings()
        {
            WorkflowSettingsPoco settings = _service.GetSettings();
            const string editUrl = "not.a.real.domain.com";

            settings.FlowType = 1;
            settings.SendNotifications = !settings.SendNotifications;
            settings.EditUrl = editUrl;

            _service.UpdateSettings(settings);

            settings = _service.GetSettings();

            Assert.Equal(editUrl, settings.EditUrl);
            Assert.Equal(1, settings.FlowType);
        }
    }
}
