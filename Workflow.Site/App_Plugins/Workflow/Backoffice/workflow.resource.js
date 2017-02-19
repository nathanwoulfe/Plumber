(function () {
    'use strict';

    // create service
    function WorkflowResource($http, $q) {
        var service = {

            urlBase: '/umbraco/backoffice/api/workflow/',
            urlTasksBase: '/umbraco/backoffice/api/workflowtasks/',

            getStatus: function (id) {
                var deferred = $q.defer();
                $http({ method: 'GET', url: this.urlBase + 'getStatus?nodeId=' + id, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response.data);
                    }, function (err) {
                        return deferred.reject('Something broke');
                    });
                return deferred.promise;
            },

            initiateWorkflow: function (nodeId, comment, publish) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: this.urlBase + 'initiateWorkflow?nodeId=' + nodeId + '&comment=' + comment + '&publish=' + publish, cache: false })
                    .then(function (response) {
                        return deferred.resolve(response);
                    }, function (err) {
                        return deferred.reject('Something broke');
                    });
                return deferred.promise;
            },

            getApprovalsForUser: function (userId) {
                return $http.post(this.urlTasksBase + 'getapprovalsforuser?userId=' + userId);
            },
            getSubmissionsForUser: function (userId) {
                return $http.post(this.urlTasksBase + 'getsubmissionsforuser?userId=' + userId);
            },
            getActiveTasks: function () {
                return $http.get(this.urlTasksBase + 'getactivetasks');
            },
            approveWorkflowTask: function (taskId, comment) {
                return $http.post(this.urlTasksBase + 'approveworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            rejectWorkflowTask: function (taskId, comment) {
                return $http.post(this.urlTasksBase + 'rejectworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            cancelWorkflowTask: function (taskId, comment) {
                return $http.post(this.urlTasksBase + 'cancelworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            showDifferences: function (nodeId, taskId) {
                return $http.post(this.urlTasksBase + 'showdifferences?nodeId=' + nodeId + '&taskId=' + taskId);
            },

            /* get/set workflow settings*/
            getSettings: function () {
                return $http.get(this.urlBase + 'getSettings');
            },
            saveSettings: function (settings) {
                var deferred = $q.defer();
                $http({ method: 'POST', url: this.urlBase + 'saveSettings', data: settings, cache: false })
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
    angular.module('umbraco.services').factory('workflowResource', WorkflowResource);

}());