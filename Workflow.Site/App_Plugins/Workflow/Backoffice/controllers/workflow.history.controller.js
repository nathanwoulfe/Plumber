(function () {
    'use strict';

    function historyController($scope, workflowResource, contentResource, dialogService, $timeout, editorState) {

        var vm = this,
            node = $scope.dialogOptions ? $scope.dialogOptions.currentNode : undefined;

        $scope.numPerPage = 10;        

        (function () {
            if (node) {
                auditNode(node);
            } else {
                getAllInstances();
            }
        }());

        function selectNode() {
            vm.overlay = {
                view: "contentpicker",
                show: true,
                submit: function (model) {
                    vm.overlay.show = false;
                    vm.overlay = null;
                    if (model.selection) {
                        auditNode(model.selection[0]);
                    } else {
                        $scope.items = [];
                    }
                },
                close: function (oldModel) {
                    vm.overlay.show = false;
                    vm.overlay = null;
                }
            };
        }

        function getAllInstances() {
            vm.loading = true;
            vm.instanceView = true;
            workflowResource.getAllInstances()
                .then(function (resp) {
                    $scope.items = resp;
                    vm.loading = false;
                });
        }

        function auditNode(data) {
            vm.loading = true;
            vm.instanceView = false;
            vm.node = data;
            workflowResource.getNodeTasks(data.id)
                .then(function (resp) {
                    $scope.currentPage = 1;
                    $scope.items = resp.sort(function (a, b) { return new Date(b.RequestedOn) - new Date(a.RequestedOn); });
                    $scope.loading = false;
                });
        }

        angular.extend(vm, {
            auditNode: auditNode,
            getAllInstances: getAllInstances,
            selectNode: selectNode,

            name: 'Workflow history'
        });
    }

    angular.module('umbraco').controller('Workflow.History.Controller', historyController);

}());