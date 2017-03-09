(function () {
    'use strict';

    function workflowRouter($q) {
        return {
            request: function (request) {
                if (request.url.toLowerCase().indexOf('workflow/tree') !== -1) {
                    //request.url = '/App_Plugins/USCStartup/partials/umbLinkPicker.html';
                }
                return request || $q.when(request);
            }
        }
    }

    angular.module('umbraco').factory('workflowInterceptor', ['$q', workflowRouter]);

    angular.module('umbraco')
        .config(function ($httpProvider) {
            $httpProvider.interceptors.push('workflowInterceptor');
        });
}());