(function () {
    'use strict';

    // create controller 
    function Controller($scope, $routeParams, notificationsService, navigationService) {
        var vm = this,
            nodeId = $routeParams.id;

        workflowResource.getStatus(nodeId)
            .then(function (resp) {
                if (resp.status === 0) {
                    vm.active = true;
                    vm.statusMsg = resp.msg;
                }
            });

        function ok() {
            workflowResource.initiateWorkflow(nodeId, vm.comment, true)
                .then(function (resp) {
                    navigationService.hideDialog();
                    if (resp.status === 200) {
                        notificationsService.success("SUCCESS", resp.data);
                    }
                    else {
                        notificationsService.error("ERROR", resp.data);
                    }
                });
        }

        angular.extend(vm, {
            ok: ok,
            comment: '',
            active: false
        });
    };

    // register controller 
    angular.module('umbraco').controller('Workflow.SendToUnpublish.Controller', Controller);
}());

