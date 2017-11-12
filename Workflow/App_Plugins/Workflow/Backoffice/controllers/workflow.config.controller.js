(function () {
    'use strict';

    // create controller 
    function configController($scope, workflowGroupsResource, workflowResource, notificationsService, contentResource, navigationService) {
        var vm = this,
            nodeId = $scope.dialogOptions.currentNode ? $scope.dialogOptions.currentNode.id : undefined,
            nodeIdInt = nodeId ? parseInt(nodeId, 10) : undefined;

        function init() {
            workflowGroupsResource.get()
				.then(function (resp) {
				    vm.groups = resp;

				    contentResource.getById(nodeId)
                        .then(function (resp) {
                            vm.contentTypeName = resp.contentTypeName;
                            checkNodePermissions();
                            checkAncestorPermissions(resp.path.split(','));
                        });
				});
        }

        if (!nodeId) {
            navigationService.hideDialog();
            notificationsService.error('ERROR', 'No active content node');
        }
        else {
            init();
        }

        function checkNodePermissions() {
            angular.forEach(vm.groups, function (v) {
                angular.forEach(v.permissions, function (p) {
                    if (p.nodeId === nodeIdInt) {
                        vm.approvalPath[p.permission] = v;
                    }

                    if (p.contentTypeName === vm.contentTypeName) {
                        vm.contentTypeApprovalPath[p.permission] = v;
                    }
                });
            });
        }

        function checkAncestorPermissions(path) {
            // first is -1, last is the current node
            path.shift();
            path.pop();

            angular.forEach(path, function (id) {
                angular.forEach(vm.groups, function (v) {
                    angular.forEach(v.permissions, function (p) {
                        if (p.nodeId === parseInt(id, 10)) {
                            vm.inherited[p.permission] = {
                                name: v.name,
                                groupId: p.groupId,
                                nodeName: p.nodeName,
                                permission: p.permission
                            };
                        }
                    });
                });
            });
        }

        function save() {
            debugger;

            var response = {};
            response[nodeIdInt] = [];

            angular.forEach(vm.approvalPath, function (v, i) {
                response[nodeIdInt].push(v.permissions.filter(function (p) {
                    return +p.nodeId === nodeIdInt && p.permission === i;
                })[0]);
            });

            workflowResource.saveConfig(response)
                .then(function () {
                    notificationsService.success('SUCCESS', 'Workflow configuration updated');
                    init();
                }, function (err) {
                    notificationsService.error('ERROR', err);
                });               
            
        }

        function add() {
            vm.selectedApprovalGroup.permissions.push({
                nodeId: nodeId,
                permission: vm.approvalPath.length,
                groupId: vm.selectedApprovalGroup.groupId
            });

            vm.approvalPath.push(vm.selectedApprovalGroup);
        }

        function remove($event, index) {
            $event.stopPropagation();
            $event.target.classList.add('disabled');
            vm.approvalPath.splice(index, 1);

            vm.approvalPath.forEach(function (v, i) {
                v.permissions.forEach(function (p) {
                    if (p.nodeId === nodeIdInt) {
                        p.permission = i;
                    }
                });
            });
        }

        angular.extend(vm, {
            inherited: [],
            approvalPath: [],
            contentTypeApprovalPath: [],

            save: save,
            add: add,
            remove: remove
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Config.Controller', configController);
}());

