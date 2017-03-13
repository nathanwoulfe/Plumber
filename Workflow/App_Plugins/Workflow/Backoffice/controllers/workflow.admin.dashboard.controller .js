(function () {
    'use strict';

    function dashboardController(workflowResource) {

        var vm = this;

        angular.extend(vm, {
            something: 'else'
        });
    }

    // register controller 
    angular.module('umbraco').controller('Workflow.AdminDashboard.Controller', dashboardController);
}());