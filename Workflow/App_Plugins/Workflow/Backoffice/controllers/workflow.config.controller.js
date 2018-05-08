(function () {
    'use strict';

    // create controller 
    function configController($scope, $rootScope, workflowGroupsResource, workflowResource, notificationsService, contentResource, navigationService) {
        var nodeId = $scope.dialogOptions.currentNode ? $scope.dialogOptions.currentNode.id : undefined,
            nodeIdInt = nodeId ? parseInt(nodeId, 10) : undefined;

        this.inherited = [];
        this.approvalPath = [];
        this.contentTypeApprovalPath = [];

        this.sortOptions = {
            axis: 'y',
            cursor: 'move',
            handle: '.sort-handle',
            stop: () => { }
        };

        /**
         * Fetch the groups and content type data
         */
        const init = () => {
            workflowGroupsResource.get()
                .then(groups => {
                    this.groups = groups;

                    contentResource.getById(nodeId)
                        .then(resp => {
                            this.contentTypeName = resp.contentTypeName;

                            const nodePerms = workflowResource.checkNodePermissions(this.groups, nodeIdInt, this.contentTypeName);
                            this.approvalPath = nodePerms.approvalPath;
                            this.contentTypeApprovalPath = nodePerms.contentTypeApprovalPath;

                            this.inherited = workflowResource.checkAncestorPermissions(resp.path, this.groups);
                        });
                });
        };

        if (!nodeId) {
            navigationService.hideDialog();
            notificationsService.error('ERROR', 'No active content node');
        }
        else {
            init();
        }

        /**
         * Process the approvalPath object, then save it
         */
        this.save = () => {
            var response = {};
            response[nodeIdInt] = [];

            // convert the approvalPath array into something resembling the expected model
            // Dictionary<int, List<UserGroupPermissionsPoco>
            this.approvalPath.forEach((v, i) => {
                response[nodeIdInt].push({
                    nodeId: nodeId,
                    permission: i,
                    groupId: v.groupId
                });
            });

            workflowResource.saveConfig(response)
                .then(() => {
                    navigationService.hideNavigation();
                    notificationsService.success('SUCCESS', 'Workflow configuration updated');
                    $rootScope.$broadcast('configUpdated');
                    init();
                },
                err => {
                    navigationService.hideNavigation();
                    notificationsService.error('ERROR', err);
                });

        };

        /**
         * Adds a stage to the approval flow
         */
        this.add = () => {
            this.selectedApprovalGroup.permissions.push({
                nodeId: nodeId,
                permission: this.approvalPath.length,
                groupId: this.selectedApprovalGroup.groupId
            });

            this.approvalPath.push(this.selectedApprovalGroup);
        };

        /**
         * Removes a stage from the approval flow
         * @param {any} $event
         * @param {any} index
         */
        this.remove = ($event, index) => {
            $event.stopPropagation();
            $event.target.classList.add('disabled');
            this.approvalPath.splice(index, 1);

            this.approvalPath.forEach((v, i) => {
                v.permissions.forEach(p => {
                    if (p.nodeId === nodeIdInt) {
                        p.permission = i;
                    }
                });
            });
        };
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Config.Controller',
        ['$scope', '$rootScope', 'plmbrGroupsResource', 'plmbrWorkflowResource', 'notificationsService', 'contentResource', 'navigationService', configController]);
}());

