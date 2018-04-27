(function () {
    'use strict';

    function dashboardController($rootScope, workflowGroupsResource) {

        this.name = 'Approval groups';
        this.loading = true;
        this.items = [];

        this.init = () => {
            workflowGroupsResource.get()
                .then(resp => {
                    this.loading = false;
                    this.items = resp;
                });
        };

        this.getEmail = (users) => users.map(v => v.user.email).join(';');

        $rootScope.$on('refreshGroupsDash', () => {
            this.init();
        });

        this.init();
    }

    angular.module('umbraco').controller('Workflow.Groups.Dashboard.Controller', ['$rootScope', 'plmbrGroupsResource', dashboardController]);

}());