(function () {
  'use strict';

  function settingsController($q, workflowResource, notificationsService, workflowGroupsResource) {
    var vm = this,
        promises = [workflowResource.getSettings(), workflowResource.getContentTypes(), workflowGroupsResource.get()];

    $q.all(promises)
        .then(function (resp) {

          vm.settings = resp[0];
          vm.docTypes = resp[1];
          vm.groups = resp[2];

          vm.flowTypes = [
              { i: 0, v: 'Other groups must approve' },
              { i: 1, v: 'All groups must approve' },
              { i: 2, v: 'All groups must approve, ignore author' }
          ];

          vm.flowType = vm.flowTypes[vm.settings.flowType];

          if (vm.settings.defaultApprover) {
            vm.defaultApprover = vm.groups.filter(function (v) {
              return parseInt(v.groupId, 10) === parseInt(vm.settings.defaultApprover, 10);
            })[0];
          }

          vm.groups.forEach(function (g) {
            g.permissions.forEach(function (p) {
              if (p.contentTypeId > 0) {
                vm.docTypes.forEach(function (dt) {
                  if (dt.id === p.contentTypeId) {
                    if (!dt.approvalPath) {
                      dt.approvalPath = [];
                    }

                    dt.approvalPath[p.permission] = g;
                  }
                });
              }
            });
          });
        });


    function save() {
      var permissions = {};
      vm.settings.defaultApprover = vm.defaultApprover.groupId;
      vm.settings.flowType = vm.flowType.i;

      angular.forEach(vm.docTypes, function (dt, i) {
        if (dt.approvalPath && dt.approvalPath.length) {
          permissions[i] = [];
          angular.forEach(dt.approvalPath,
            function(path, ii) {
              permissions[i].push({
                contentTypeId: dt.id,
                permission: ii,
                groupId: path.groupId
              });
            });
        } 
      });

      var p = [workflowResource.saveDocTypeConfig(permissions), workflowResource.saveSettings(vm.settings)];
      $q.all(p)
          .then(function () {
            notificationsService.success('SUCCESS!', 'Settings updated');
          }, function (err) {
            notificationsService.error('OH SNAP!', err);
          });
    }

    function add(dt) {
      if (dt.approvalPath) {
        dt.approvalPath.push(dt.selectedApprovalGroup);
      } else {
        dt.approvalPath = [dt.selectedApprovalGroup];
      }
    }

    function remove(dt, index) {
      dt.approvalPath.splice(index, 1);
    }

    angular.extend(vm, {
      save: save,
      add: add,
      remove: remove,
      name: 'Workflow settings',

      email: '',
      defaultApprover: '',
      settings: {
        email: '',
        defaultApprover: ''
      }
    });
  }

  angular.module('umbraco').controller('Workflow.Settings.Controller', settingsController);

}());