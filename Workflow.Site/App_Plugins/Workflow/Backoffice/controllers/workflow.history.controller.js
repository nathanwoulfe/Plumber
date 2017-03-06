(function () {
    'use strict';

    function historyController($scope, workflowResource, contentResource, dialogService, $timeout, editorState) {

        var vm = this;

        $scope.numPerPage = 10;

        (function () {

            var state = editorState.getCurrent();

            if (!state) {
                vm.loading = true;
                vm.isNode = false;
                workflowResource.getAllInstances()
                    .then(function (resp) {
                        $scope.items = resp.data;
                        vm.loading = false;
                    });
            } else {
                auditNode(state);
            }
        }());

        function selectNode() {
            var dialog = dialogService.contentPicker({
                multipicker: false,
                callback: auditNode
            });
        }

        function auditNode(data) {
            vm.loading = true;
            vm.isNode = true;
            vm.node = data;
            workflowResource.getNodeTasks(data.id)
                .then(function (resp) {
                    $scope.currentPage = 1;
                    $scope.items = resp.data.sort(function (a, b) { return new Date(b.RequestedOn) - new Date(a.RequestedOn) });
                    $scope.loading = false;
                });
        }

        angular.extend(vm, {
            auditNode: auditNode,
            selectNode: selectNode,

            name: 'Workflow history'
        });
    }

    angular.module('umbraco').controller('Workflow.History.Controller', historyController);

}());