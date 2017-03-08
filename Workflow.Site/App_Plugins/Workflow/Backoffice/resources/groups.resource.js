(function () {
    'use strict';

    // create service
    function userGroupsResource($http, $q, umbRequestHelper) {
        var service = {

            urlBase: '/umbraco/backoffice/api/usergroups/',

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
             * @description Get all user groups
             */
            getAllGroups: function () {
                return this.request('GET', this.urlBase + 'getAllGroups');
            },

            /*** GET GROUP BY ID ***/
            getGroup: function (id) {
                return this.request('GET', this.urlBase + 'getGroup', { id: id });
            },

            /*** ADD NEW GROUP ***/
            addGroup: function (name) {
                return this.request('POST', this.urlBase + 'addGroup?name=' + name);
            },

            /*** SAVE GROUP ***/
            saveGroup: function (group) {
                return this.request('POST', this.urlBase + 'saveGroup', group);
            },

            /*** DELETE GROUP ***/
            deleteGroup: function (id) {
                return this.request('POST', this.urlBase + 'deleteGroup?id=' + id);
            }
        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('userGroupsResource', userGroupsResource);

}());