(function () {
    'use strict';

    function workflowGroupsResource($http, $q, umbRequestHelper) {
        var service = {

            urlBase: Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/backoffice/api/workflow/groups/',

            request: function (method, url, data) {
                return umbRequestHelper.resourcePromise(
                    method === 'DELETE' ? $http.delete(url) :
                    method === 'POST' ? $http.post(url, data) :
                    method === 'PUT' ? $http.put(url, data) :
                        $http.get(url),
                    'Something broke'
                );
            },

            /**
             * @returns {array} user groups
             * @description Get single group by id, or all groups if no id parameter provided
             */
            get: function (id) {
                return this.request('GET', this.urlBase + (id ? 'get/' + id : 'get'));
            },

            /**
             * @returns the new user group
             * @description Add a new group, where the param is the new group name
             */
            add: function (name) {
                return this.request('POST', this.urlBase + 'add', { data: name } );
            },

            /**
             * @returns {string}
             * @description save updates to an existing group object
             */
            save: function (group) {
                return this.request('PUT', this.urlBase + 'save', group);
            },

            /**
             * @returns {string}
             * @description delete group by id
             */
            'delete': function (id) {
                return this.request('DELETE', this.urlBase + 'delete/' + id );
            }
        };

        return service;
    }

    angular.module('umbraco.services').factory('workflowGroupsResource', workflowGroupsResource);

}());