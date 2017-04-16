(function () {
    'use strict';

    function dashboardController() {
        var vm = this;

        angular.extend(vm, {
            tabs: [{
                id: 101,
                label: "Settings",
                alias: "tab101",
                active: true
            }, {
                id: 102,
                label: "History",
                alias: "tab102",
                active: false
            }, {
                id: 103,
                label: "Approval groups",
                alias: "tab103",
                active: false
            }, {
                id: 104,
                label: "Context menu",
                alias: "tab104",
                active: false
            }, {
                id: 105,
                label: "User dashboard",
                alias: "tab105",
                active: false
            }, {
                id: 106,
                label: "Editor drawer",
                alias: "tab106",
                active: false
            }]
        });
    }

    angular.module('umbraco').controller('Workflow.DocsDashboard.Controller', dashboardController);

}());