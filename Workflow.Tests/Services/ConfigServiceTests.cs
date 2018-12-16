using System.Collections.Generic;
using Chauffeur.TestingTools;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Workflow.Extensions;
using Workflow.Models;
using Workflow.Repositories;
using Workflow.Services;
using Workflow.Services.Interfaces;
using Xunit;

namespace Workflow.Tests.Services
{
    public class ConfigServiceTests : UmbracoHostTestBase
    {
        private readonly IConfigService _configService;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;

        public ConfigServiceTests()
        {
            Host.Run(new[] { "install y" }).Wait();

            _configService = new ConfigService(new PocoRepository());

            _contentService = ApplicationContext.Current.Services.ContentService;
            _contentTypeService = ApplicationContext.Current.Services.ContentTypeService;

            Scaffold.Run();
            Scaffold.Config();
            Scaffold.ContentType(_contentTypeService);
        }

        [Fact]
        public void Can_Get_Service()
        {
            Assert.NotNull(_configService);
        }

        [Fact]
        public void Can_Get_All_Permissions()
        {
            List<UserGroupPermissionsPoco> allPermissions = _configService.GetAll();

            Assert.NotEmpty(allPermissions);
        }

        [Fact]
        public void Can_Get_Permissions_For_Node()
        {
            // will return an empty collection as no permissions exist
            Assert.Empty(_configService.GetPermissionsForNode(9999));

            // this one has a permission, so should return something
            List<UserGroupPermissionsPoco> permissions = _configService.GetPermissionsForNode(1089);
            Assert.NotEmpty(permissions);
        }

        [Fact]
        public void Recursive_Permissions_Returns_Null_When_Node_Is_Null()
        {
            // no node, returns immediately
            List<UserGroupPermissionsPoco> permissions = _configService.GetRecursivePermissionsForNode(null);
            Assert.Null(permissions);
        }

        [Fact]
        public void Can_Get_Recursive_Permissions_For_Node_When_Node_Has_Permissions()
        {
            IContent root = Scaffold.Node(_contentService);
            IContent child = Scaffold.Node(_contentService, root.Id);
            IContent grandChild = Scaffold.Node(_contentService, child.Id);

            List<UserGroupPermissionsPoco> permissions = _configService.GetRecursivePermissionsForNode(grandChild.ToPublishedContent());

            // node has no permissions, yet
            Assert.Empty(permissions);

            // add a permission so something is returned...
            var poco = new UserGroupPermissionsPoco
            {
                GroupId = 3,
                Permission = 0,
                NodeId = grandChild.Id
            };

            _configService.UpdateNodeConfig(new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                { 0, new List<UserGroupPermissionsPoco> { poco } }
            });

            permissions = _configService.GetRecursivePermissionsForNode(grandChild.ToPublishedContent());
            Assert.Single(permissions);
        }

        [Fact]
        public void Can_Get_Recursive_Permissions_For_Node_When_Ancestor_Has_Permissions()
        {
            IContent root = Scaffold.Node(_contentService);
            IContent child = Scaffold.Node(_contentService, root.Id);
            IContent grandChild = Scaffold.Node(_contentService, child.Id);

            List<UserGroupPermissionsPoco> permissions = _configService.GetRecursivePermissionsForNode(grandChild.ToPublishedContent());

            // node has no permissions, yet
            Assert.Empty(permissions);

            // add a permission so something is returned...
            var poco = new UserGroupPermissionsPoco
            {
                GroupId = 3,
                Permission = 0,
                NodeId = child.Id
            };

            _configService.UpdateNodeConfig(new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                { 0, new List<UserGroupPermissionsPoco> { poco } }
            });

            permissions = _configService.GetRecursivePermissionsForNode(grandChild.ToPublishedContent());
            Assert.Single(permissions);
        }

        [Fact]
        public void Can_Get_Recursive_Permissions_When_Node_Has_No_Permissions()
        {
            IContentType contentType = _contentTypeService.GetContentType("textpage");

            IContent root = Scaffold.Node(_contentService);
            IContent child = Scaffold.Node(_contentService, root.Id);

            List<UserGroupPermissionsPoco> permissions = _configService.GetRecursivePermissionsForNode(root.ToPublishedContent());
            Assert.Empty(permissions);

            permissions = _configService.GetRecursivePermissionsForNode(child.ToPublishedContent());
            Assert.Empty(permissions);

            // add a permission so something is returned...
            var poco = new UserGroupPermissionsPoco
            {
                ContentTypeId = contentType.Id,
                GroupId = 3,
                Permission = 0
            };

            _configService.UpdateContentTypeConfig(new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                { 0, new List<UserGroupPermissionsPoco> { poco } }
            });

            permissions = _configService.GetRecursivePermissionsForNode(child.ToPublishedContent());
            Assert.Single(permissions);
        }

        [Fact]
        public void Return_Null_If_Node_Is_Null()
        {
            List<UserGroupPermissionsPoco> permissions = _configService.GetRecursivePermissionsForNode(null);

            Assert.Null(permissions);
        }

        [Fact]
        public void Can_Get_Content_Type_Permission_For_Node()
        {
            IContentType type = _contentTypeService.GetContentType("textpage");
            IContent node = Scaffold.Node(_contentService);

            Dictionary<int, List<UserGroupPermissionsPoco>> perms = Scaffold.Permissions(0, 2, 0, type.Id);

            _configService.UpdateContentTypeConfig(perms);

            Assert.NotNull(_configService.GetPermissionsForNode(node.Id));
        }

        [Fact]
        public void Can_Update_Node_Config_And_Event_Is_Raised()
        {
            ConfigService.Updated += (sender, args) =>
            {
                Assert.NotNull(args);
            };

            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config = new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                [0] = new List<UserGroupPermissionsPoco>
                {
                    new UserGroupPermissionsPoco
                    {
                        NodeId = 1089,
                        GroupId = 3,
                        Permission = 2
                    }
                },
                [1] = new List<UserGroupPermissionsPoco>()
            };

            bool updated = _configService.UpdateNodeConfig(config);

            Assert.True(updated);
        }

        [Fact]
        public void Cannot_Update_Node_Config_If_Permissions_Are_Null()
        {
            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config =
                new Dictionary<int, List<UserGroupPermissionsPoco>>
                {
                    [0] = new List<UserGroupPermissionsPoco>()
                };

            Assert.False(_configService.UpdateNodeConfig(config));
        }

        [Fact]
        public void Can_Update_ContentType_Config_And_Event_Is_Raised()
        {
            ConfigService.Updated += (sender, args) =>
            {
                Assert.NotNull(args);
            };

            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config = new Dictionary<int, List<UserGroupPermissionsPoco>>
            {
                [0] = new List<UserGroupPermissionsPoco>
                {
                    new UserGroupPermissionsPoco
                    {
                        ContentTypeId = 1069,
                        GroupId = 4,
                        Permission = 0
                    }
                },
                [1] = new List<UserGroupPermissionsPoco>()
            };

            Assert.True(_configService.UpdateContentTypeConfig(config));
        }

        // update when model is null        
        [Fact]
        public void ContentType_Returns_False_When_Config_Model_Is_Null()
        {
            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config = new Dictionary<int, List<UserGroupPermissionsPoco>>();

            Assert.False(_configService.UpdateContentTypeConfig(config));
        }

        [Fact]
        public void Node_Returns_False_When_Config_Model_Is_Null()
        {
            // mock some data
            Dictionary<int, List<UserGroupPermissionsPoco>> config = new Dictionary<int, List<UserGroupPermissionsPoco>>();

            Assert.False(_configService.UpdateNodeConfig(config));
        }
    }
}