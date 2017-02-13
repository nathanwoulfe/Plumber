(function () {
    'use strict';

    var umbraco = angular.module('umbraco');

    // grab requests for the user groups dash, and reroute to the correct view
    // needs to happen here as Umbraco defines /:section/:tree/:method, which takes prioerity over a similarly formatted route defined later
    function interceptor($q) {
        return {
            request: function (request) {
                console.log(request);
                if (request.url.toLowerCase().indexOf('views/usergroups/dashboard') !== -1) {
                    request.url = '/App_Plugins/Workflow/Backoffice/UserGroups/dashboard.html';
                }
                return request || $q.when(request);
            }
        }
    }
    umbraco.factory('userGroupsDashboardInterceptor', ['$q', interceptor]);

    // add the route for the dashboard view
    umbraco.config(function ($httpProvider) {
        $httpProvider.interceptors.push('userGroupsDashboardInterceptor');
    });
}());