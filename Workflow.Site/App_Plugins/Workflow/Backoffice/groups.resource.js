(function () {
    'use strict';

    // create service
    function UserGroupsResource($http, $q) {
        var service = {

            urlBase: '/umbraco/backoffice/api/usergroups/',

            /*** GET ALL GROUPS ***/
            getAllGroups: function () {
                var deferred = $q.defer();
                $http({ method: 'GET', url: this.urlBase + 'getAllGroups', cache: false })
                    .then(function (response) {
                        return deferred.resolve(response.data);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            },

            /*** GET GROUP BY ID ***/
            getGroup: function (id) {
                var deferred = $q.defer();
                $http({ method: 'GET', url: this.urlBase + 'getGroup?id=' + id, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response.data);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            },

            /*** ADD NEW GROUP ***/
            addGroup: function (name) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: this.urlBase + 'addGroup?name=' + name, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            },


            /*** SAVE GROUP ***/
            saveGroup: function (group) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: this.urlBase + 'saveGroup', data: group, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            },


            /*** DELETE GROUP ***/
            deleteGroup: function (id) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: this.urlBase + 'deleteGroup?id=' + id, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            },

            /*** SAVE GROUP ***/
            savePermissions: function (p) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: '/umbraco/backoffice/api/grouppermissions/savePermissions', data: p, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response);
                    }, function (err) {
                        return deferred.reject('Something broke: ' + err);
                    });
                return deferred.promise;
            }


        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('UserGroupsResource', UserGroupsResource);

}());