(() => {
    function historyController($scope, workflowResource) {

        this.activityFilter = workflowResource.getActivityFilter();
        this.perPage = () => [2, 5, 10, 20, 50];
        this.name = 'Workflow history';
        this.view = '';

        this.pagination = {
            pageNumber: 1,
            totalPages: 0,
            perPage: 10,
            goToPage: i => {
                this.pagination.pageNumber = i;
                if (this.activityFilter) {
                    this.getActivity();
                }
                else if (this.node !== undefined) {
                    this.auditNode();
                } else {
                    this.getAllInstances();
                }
            }
        };

        const width = $scope.dialogOptions ? $scope.dialogOptions.currentAction.metaData.width : undefined;
        const node = $scope.dialogOptions ? $scope.dialogOptions.currentNode : undefined;

        const setPaging = resp => {
            this.items = resp.items;
            this.pagination.pageNumber = resp.page;
            this.pagination.totalPages = resp.totalPages;
            this.loading = false;
        };

        if (width) {
            angular.element('#dialog').css('width', width);
        }

        this.selectNode = () => {
            this.overlay = {
                view: 'contentpicker',
                show: true,
                submit: model => {
                    if (model.selection) {
                        this.auditNode(model.selection[0]);
                    } else {
                        $scope.items = [];
                    }
                    this.overlay.close();
                },
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };

        /**
         * 
         */
        this.getAllInstances = () => {
            this.loading = true;

            // when switching, set state, reset paging and clear node data
            if (this.view !== 'instance') {
                this.view = 'instance';
                this.pagination.pageNumber = 1;
                this.node = undefined;
            }

            workflowResource.getAllInstances(this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.instancesLoaded = true;
                });
        };

        /**
         * 
         * @param {any} data
         */
        this.auditNode = data => {
            this.loading = true;

            // when switching from instance to node, reset paging, toggle state and store node
            if (this.view !== 'node') {
                this.pagination.pageNumber = 1;
                this.view = 'node';
            }

            this.node = data || this.node;

            workflowResource.getAllInstancesForNode(this.node.id, this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.nodeInstancesLoaded = true;
                });
        };

        /**
         * 
         */
        const getActivity = () => {
            if (this.view.indexOf('activity') === -1) {
                this.pagination.pageNumber = 1;
                this.node = undefined;
                this.view = `activity-${this.activityFilter.type.toLowerCase()}`;
            }

            workflowResource[this.activityFilter.type === 'Task' ? 'getFilteredTasksForRange' : 'getFilteredInstancesForRange'](
                    this.activityFilter.range,
                    this.activityFilter.filter,
                    this.pagination.perPage,
                    this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                    this.activityLoaded = true;
                });
        };

        // go get the data
        if (this.activityFilter) {
            getActivity();
        }
        else if (node) {
            this.auditNode(node);
        } else {
            this.getAllInstances();
        }
    }

    angular.module('umbraco').controller('Workflow.History.Controller', ['$scope', 'plmbrWorkflowResource', historyController]);

})();
