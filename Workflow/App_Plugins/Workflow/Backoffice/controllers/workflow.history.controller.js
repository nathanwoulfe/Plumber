(function () {
    'use strict';

    function historyController($scope, workflowResource, contentResource, dialogService, $timeout, editorState) {

        var vm = this;

        $scope.numPerPage = 10;

        (function () {

            var state = editorState.getCurrent();

            if (!state) {
                getAllInstances();
            } else {
                auditNode(state);
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
            }
        }

        function getAllInstances() {
            vm.loading = true;
            vm.instanceView = true;
            workflowResource.getAllInstances()
                .then(function (resp) {
                    $scope.items = resp.data;
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
                    $scope.items = resp.data.sort(function (a, b) { return new Date(b.RequestedOn) - new Date(a.RequestedOn) });
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