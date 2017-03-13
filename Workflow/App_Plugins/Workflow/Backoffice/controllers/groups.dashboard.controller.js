(function () {
    'use strict';

    function dashboardController(userGroupsResource) {

        var vm = this;

        userGroupsResource.get()
            .then(function (resp) {
                vm.loading = false;
                vm.items = resp;
            });

        function getEmail(users) {
            return users.map(function (v) {
                return v.user.email;
            }).join(';');
        };

        angular.extend(vm, {
            name: 'User groups',
            loading: true,
            items: [],

            getEmail: getEmail
        });
    };

    angular.module('umbraco').controller('Workflow.Groups.Dashboard.Controller', dashboardController);

}());