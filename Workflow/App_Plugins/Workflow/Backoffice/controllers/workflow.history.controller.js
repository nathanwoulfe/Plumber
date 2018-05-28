(() => {
    function historyController($scope, workflowResource) {

        this.activityFilter = workflowResource.getActivityFilter();

        this.name = 'Workflow history';

        this.pagination = {
            pageNumber: 1,
            totalPages: 0,
            perPage: 10,
            goToPage: i => {
                this.pagination.pageNumber = i;
                if (this.node !== undefined) {
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

        this.perPage = () => [2, 5, 10, 20, 50];

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

        this.getAllInstances = () => {
            this.loading = true;

            // when switching, set state, reset paging and clear node data
            if (!this.instanceView) {
                this.instanceView = true;
                this.pagination.pageNumber = 1;
                this.node = undefined;
            }

            workflowResource.getAllInstances(this.pagination.perPage, this.pagination.pageNumber, this.activityFilter)
                .then(resp => {
                    setPaging(resp);
                });
        };

        this.auditNode = data => {
            this.loading = true;

            // when switching from instance to node, reset paging, toggle state and store node
            if (this.instanceView) {
                this.pagination.pageNumber = 1;
                this.instanceView = false;
            }

            this.node = data || this.node;

            workflowResource.getNodeTasks(this.node.id, this.pagination.perPage, this.pagination.pageNumber)
                .then(resp => {
                    setPaging(resp);
                });
        };

        // go get the data
        if (node) {
            this.auditNode(node);
        } else {
            this.getAllInstances();
        } 
    }

    angular.module('umbraco').controller('Workflow.History.Controller', ['$scope', 'plmbrWorkflowResource', historyController]);

})();
