(function () {
    'use strict';

    // create service
    function userGroupsResource($http, $q, umbRequestHelper) {
        var service = {

            urlBase: '/umbraco/backoffice/api/workflow/groups/',

            request: function (method, url, data) {
                return umbRequestHelper.resourcePromise(
                    method === 'GET' ?
                        $http.get(url, { params: data }) :
                        $http.post(url, data),
                    'Something broke'
                );
            },

            /**
             * @returns {array} user groups
             * @description Get single group by id, or all groups if no id parameter provided
             */
            get: function (id) {
                return this.request('GET', this.urlBase + 'get', { id: id });
            },

            /*** ADD NEW GROUP ***/
            add: function (name) {
                return this.request('POST', this.urlBase + 'add?name=' + name);
            },

            /*** SAVE GROUP ***/
            save: function (group) {
                return this.request('POST', this.urlBase + 'save', group);
            },

            /*** DELETE GROUP ***/
            'delete': function (id) {
                return this.request('POST', this.urlBase + 'delete?id=' + id);
            }
        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('userGroupsResource', userGroupsResource);

}());