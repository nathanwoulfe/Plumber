(function () {
    'use strict';

    // create controller 
    function publishController($scope, $routeParams, notificationsService, workflowResource, navigationService) {
        var vm = this,
			nodeId = $routeParams.id;

        workflowResource.getStatus(nodeId)
            .then(function (resp) {
                vm.active = resp;
            }, function (err) {
                notificationsService.error("ERROR", err);
            });

        var formScope = angular.element($('form[name="contentForm"]')).scope();
        vm.dirty = formScope.contentForm.$dirty;        
        
        function ok() {            
            workflowResource.initiateWorkflow(nodeId, vm.comment, true)
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
            active: false
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.SendToPublish.Controller', publishController);
}());

