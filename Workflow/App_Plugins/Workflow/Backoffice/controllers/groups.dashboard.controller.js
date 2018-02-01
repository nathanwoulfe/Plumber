(function () {
    'use strict';

    function dashboardController($rootScope, workflowGroupsResource) {

        var vm = this;

        $rootScope.$on('refreshGroupsDash', function () {
            init();
        });

        function init() {
            workflowGroupsResource.get()
                .then(function(resp) {
                    vm.loading = false;
                    vm.items = resp;
                });
        }

        function getEmail(users) {
            return users.map(function (v) {
                return v.user.email;
            }).join(';');
        }

        angular.extend(vm, {
            name: 'Approval groups',
            loading: true,
            items: [],

            getEmail: getEmail
        });

        init();
    }

    angular.module('umbraco').controller('Workflow.Groups.Dashboard.Controller', dashboardController);

}());