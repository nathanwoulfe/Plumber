(function () {
    'use strict';

    function submitController($scope, notificationsService, workflowResource, navigationService) {
        var vm = this,
			nodeId = $scope.dialogOptions.currentNode.id;

        workflowResource.getStatus(nodeId)
            .then(function (resp) {
                if (resp === 'true') {
                    navigationService.hideDialog();
                    notificationsService.error('ERROR', 'Page is already in a workflow process.');
                }
            }, function (err) {
                notificationsService.error("ERROR", err);
            });

        var formScope = angular.element($('form[name="contentForm"]')).scope();
        vm.dirty = formScope ? formScope.contentForm.$dirty : false;  

        function ok() {
            workflowResource.initiateWorkflow(nodeId, vm.comment, vm.isPublish)
                .then(function (resp) {
                    navigationService.hideDialog();
                    notificationsService.success("SUCCESS", resp);
                }, function (err) {
                    notificationsService.error("ERROR", err);
                });
        }

        angular.extend(vm, {
            ok: ok,
            comment: '',
            isPublish: $scope.dialogOptions.currentAction.metaData.isPublish
        });
    }

    angular.module('umbraco').controller('Workflow.Submit.Controller', submitController);
}());

