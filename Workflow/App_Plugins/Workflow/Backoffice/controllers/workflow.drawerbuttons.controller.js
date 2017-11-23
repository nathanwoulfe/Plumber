(function () {
  'use strict';

  // create controller 
  // since this controller is loaded in response to an injector match, we can use it to check for active workflow groups 
  // and display a message if none are configured, while also displaying the default button set
  function controller($scope, $rootScope, userService, workflowResource, workflowActionsService, contentEditingHelper, contentResource, editorState, $routeParams, notificationsService) {
    var vm = this,
        user;

    var defaultButtons = contentEditingHelper.configureContentEditorButtons({
      create: $routeParams.create,
      content: $scope.content,
      methods: {
        saveAndPublish: $scope.saveAndPublish,
        sendToPublish: $scope.sendToPublish,
        save: $scope.save,
        unPublish: angular.noop
      }
    });

    var saveAndPublish = defaultButtons.defaultButton && defaultButtons.defaultButton.labelKey === 'buttons_saveAndPublish';

    function getNodeTasks() {
      workflowResource.getNodePendingTasks(editorState.current.id)
          .then(function (resp) {
            if (resp.groups || resp.settings) {
              var msg = resp.groups
                ? 'No workflow groups have been configured - refer to the documentation tab in the Workflow section, then set at minimum an approval flow on the homepage node or document type.'
                : 'Workflow settings are configured incorrectly - refer to the documentation tab in the Workflow section.';
              notificationsService.warning('WORKFLOW INSTALLED BUT NOT CONFIGURED', msg);
            } else if (resp.items && resp.items.length) {
              vm.active = true;
              checkUserAccess(resp.items[0]);
            } else {
              vm.active = false;
              setButtons();
            }
          }, function () {

          });
    }

    // must be a better way of doing this - need to watch the editor state to dynamically change buttons
    $scope.$watch('$parent.$parent.$parent.contentForm.$dirty', function (newVal) {
      $scope.dirty = newVal === true;
      setButtons();
    });

    $rootScope.$on('workflowActioned', function () {
      getNodeTasks();
    });

    var buttons = {
      approveButton: {
        labelKey: 'workflow_approveButtonLong',
        handler: function (item) {
          vm.workflowOverlay = workflowActionsService.action(item, true);
        }
      },
      cancelButton: {
        labelKey: 'workflow_cancelButtonLong',
        cssClass: 'danger',
        handler: function (item) {
          vm.workflowOverlay = workflowActionsService.cancel(item);
        }
      },
      rejectButton: {
        labelKey: 'workflow_rejectButton',
        cssClass: 'warning',
        handler: function (item) {
          vm.workflowOverlay = workflowActionsService.action(item, false);
        }
      },
      saveButton: {
        labelKey: 'workflow_saveButton',
        cssClass: 'success',
        handler: function () {
          contentEditingHelper.contentEditorPerformSave({
            statusMessage: 'Saving...',
            saveMethod: contentResource.save,
            scope: $scope,
            content: editorState.current
          });
          $scope.$parent.$parent.$parent.contentForm.$setPristine();
        }
      },
      publishButton: {
        labelKey: 'workflow_publishButton',
        cssClass: 'success',
        handler: function () {
          vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, true);
        }
      },
      unpublishButton: {
        labelKey: 'workflow_unpublishButton',
        cssClass: 'warning',
        handler: function () {
          vm.workflowOverlay = workflowActionsService.initiate(editorState.current.name, editorState.current.id, false);
        }
      }
    };

    // any user with access to the workflow section will be able to action workflows ie cancel outside their group membership
    function checkUserAccess(task) {
      vm.task = task;
      vm.adminUser = user.allowedSections.indexOf('workflow') !== -1;
      var currentTaskUsers = task.permissions[task.currentStep].userGroup.usersSummary;

      if (currentTaskUsers.indexOf('|' + user.id + '|') !== -1) {
        vm.canAction = true;
      }
      if (vm.active) {
        vm.buttonGroup = {
          defaultButton: vm.adminUser || vm.canAction ? buttons.cancelButton : buttons.approveButton,
          subButtons: vm.adminUser || vm.canAction ? [] : [buttons.rejectButton, buttons.cancelButton]
        };
      }
    }

    function setButtons() {
      // default button will be null when the current user has browse-only permission
      if (defaultButtons.defaultButton !== null) {
        var subButtons = saveAndPublish ? [buttons.unpublishButton, defaultButtons.defaultButton, buttons.saveButton] : [buttons.unpublishButton, buttons.saveButton];

        vm.buttonGroup = {
          defaultButton: $scope.dirty ? buttons.saveButton : buttons.publishButton,
          subButtons: $scope.dirty ? (saveAndPublish ? [defaultButtons.defaultButton] : []) : subButtons
        };
      }
    }

    userService.getCurrentUser()
        .then(function (userResp) {
          user = userResp;
          getNodeTasks();
        });

    angular.extend(vm, {
      active: false
    });
  }

  // register controller 
  angular.module('umbraco').controller('Workflow.DrawerButtons.Controller', controller);
}());

