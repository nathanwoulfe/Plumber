(function () {
    'use strict';

    function historyController($scope, workflowResource, contentResource, dialogService) {

        var vm = this;

        $scope.numPerPage = 10;

        (function () {
            vm.loading = true;
            vm.isNode = false;
            workflowResource.getActiveTasks()
                .then(function (resp) {
                    $scope.items = resp.data;
                    vm.loading = false;
                });
        }());

        $scope.$on('LastRepeaterElement', function () {
            angular.forEach($scope.pagedItems, function (v, i) {
                if (v.NodeName === undefined) {
                    contentResource.getById(v.NodeId)
                        .then(function (resp) {
                            v.NodeName = resp.name;
                            v.ContentType = resp.contentTypeName;
                        });
                }
            });
        });

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