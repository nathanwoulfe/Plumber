(function () {
    'use strict';

    // create service
    function userGroupsResource($http, $q) {
        var service = {

            urlBase: '/umbraco/backoffice/api/usergroups/',

            request: function (method, url, data) {
                var deferred = $q.defer();
                $http({ method: method, url: url, data: data, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response.data);
                    }, function (err) {
                        return deferred.reject('Something broke');
                    });
                return deferred.promise;
            },

            /*** GET ALL GROUPS ***/
            getAllGroups: function () {
                return this.request('GET', this.urlBase + 'getAllGroups');
            },

            /*** GET GROUP BY ID ***/
            getGroup: function (id) {
                return this.request('GET', this.urlBase + 'getGroup?id=' + id);             
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