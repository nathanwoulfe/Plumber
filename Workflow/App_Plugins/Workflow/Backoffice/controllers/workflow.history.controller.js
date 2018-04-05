(function () {
    'use strict';

    function historyController($scope, workflowResource) {

      var vm = this,
            width = $scope.dialogOptions ? $scope.dialogOptions.currentAction.metaData.width : undefined,
            node = $scope.dialogOptions ? $scope.dialogOptions.currentNode : undefined;

        if (width) {
            angular.element('#dialog').css('width', width);
        }

        function perPage() {
            return [2, 5, 10, 20, 50];
        }

        function selectNode() {
            vm.overlay = {
                view: 'contentpicker',
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
                close: function () {
                    vm.overlay.show = false;
                    vm.overlay = null;
                }
            };
        }

        function getAllInstances() {
            vm.loading = true;

            // when switching, set state, reset paging and clear node data
            if (!vm.instanceView) {
                vm.instanceView = true;
                vm.pagination.pageNumber = 1;
                vm.node = undefined;
            }

            workflowResource.getAllInstances(vm.pagination.perPage, vm.pagination.pageNumber)
                .then(function (resp) {
                    setPaging(resp);
                });
        }

        function auditNode(data) {
            vm.loading = true;

            // when switching from instance to node, reset paging, toggle state and store node
            if (vm.instanceView) {
                vm.pagination.pageNumber = 1;
                vm.instanceView = false;
            }

            vm.node = data || vm.node;

            workflowResource.getNodeTasks(vm.node.id, vm.pagination.perPage, vm.pagination.pageNumber)
                .then(function (resp) {
                    setPaging(resp);
                });
        }

        function goToPage(i) {
            vm.pagination.pageNumber = i;
            if (vm.node !== undefined) {
                auditNode();
            } else {
                getAllInstances();
            }
        }

        function setPaging(resp) {
            vm.items = resp.items;
            vm.pagination.pageNumber = resp.page;
            vm.pagination.totalPages = Math.ceil(resp.total / resp.count);
            vm.loading = false;
        }

        angular.extend(vm, {
            auditNode: auditNode,
            getAllInstances: getAllInstances,
            selectNode: selectNode,
            perPage: perPage,

            name: 'Workflow history',
            pagination: {
                pageNumber: 1,
                totalPages: 0,
                perPage: 10,
                goToPage: goToPage
            }
        });

        // go get the data
        (function () {
            if (node) {
                auditNode(node);
            } else {
                getAllInstances();
            }
        }());
    }

    angular.module('umbraco').controller('Workflow.History.Controller', historyController);

}());
