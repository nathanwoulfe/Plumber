(function () {
    'use strict';

    var umbraco = angular.module('umbraco');

    function interceptor($q) {
        return {
            request: function (request) {
                if (request.url.toLowerCase().indexOf('workflow') !== -1) {
                    console.log(request);
                    request.url = '/app_plugins/workflow/backoffice/tree/settings.html';
                }
                return request || $q.when(request);
            }
        }
    }
    angular.module('umbraco').factory('usersDashboardInterceptor', ['$q', interceptor]);

    // add the route for the dashboard view
    umbraco.config(function ($httpProvider) {
        $httpProvider.interceptors.push('usersDashboardInterceptor');
    });

}());