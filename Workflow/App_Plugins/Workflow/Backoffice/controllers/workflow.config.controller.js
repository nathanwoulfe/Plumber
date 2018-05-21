(() => {
    'use strict';

    // create controller 
    function configController($scope, $rootScope, $q, workflowGroupsResource, workflowResource, notificationsService, contentResource, navigationService) {
        const nodeId = $scope.dialogOptions.currentNode ? $scope.dialogOptions.currentNode.id : undefined;
        const nodeIdInt = nodeId ? parseInt(nodeId, 10) : undefined;

        this.inherited = [];
        this.approvalPath = [];
        this.contentTypeApprovalPath = [];

        this.sortOptions = {
            axis: 'y',
            cursor: 'move',
            handle: '.sort-handle',
            stop: () => { }
        };

        let node;
        let settings;

        /**
         * Fetch the groups and content type data
         */
        const init = () => {
            this.contentTypeAlias = node.contentTypeAlias;
            this.contentTypeName = node.contentTypeName;

            const nodePerms = workflowResource.checkNodePermissions(this.groups, nodeIdInt, this.contentTypeAlias);
            this.approvalPath = nodePerms.approvalPath;
            this.contentTypeApprovalPath = nodePerms.contentTypeApprovalPath;

            this.inherited = workflowResource.checkAncestorPermissions(node.path, this.groups);

            if (!this.excludeNode) {
                this.activeType =
                    this.approvalPath.length ? 'content' :
                    this.contentTypeApprovalPath.length ? 'type' :
                    this.inherited.length ? 'inherited' : null;
            }
        };

        if (!nodeId) {
            navigationService.hideDialog();
            notificationsService.error('ERROR', 'No active content node');
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

        // it all starts here
        const promises = [contentResource.getById(nodeId), workflowResource.getSettings(), workflowGroupsResource.get()];

        $q.all(promises)
            .then(resp => {
                [node, settings, this.groups] = resp;

                this.excludeNode = workflowResource.checkExclusion(settings.excludeNodes, nodeId);
                init();
            });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.Config.Controller',
        ['$scope', '$rootScope', '$q', 'plmbrGroupsResource', 'plmbrWorkflowResource', 'notificationsService', 'contentResource', 'navigationService', configController]);
})();

