(function () {
    'use strict';

    // create controller 
    function unpublishController($scope, $routeParams, notificationsService, navigationService) {
        var vm = this,
            nodeId = $routeParams.id;

        workflowResource.getStatus(nodeId)
            .then(function (resp) {
                vm.active = resp;
            }, function (err) {
                notificationsService.error("ERROR", err);
            });

        function ok() {
            workflowResource.initiateWorkflow(nodeId, vm.comment, false)
                .then(function (resp) {
                    navigationService.hideDialog();
                    notificationsService.success("SUCCESS", resp);           
                }, function (err) {
                    notificationsService.error("ERROR", resp.data);
                });
        }

        angular.extend(vm, {
            ok: ok,
            comment: '',
            active: false
        });
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.SendToUnpublish.Controller', unpublishController);
}());

