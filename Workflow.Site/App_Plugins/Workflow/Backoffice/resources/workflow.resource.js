(function () {
    'use strict';

    // create service
    function WorkflowResource($http, $q) {
        var service = {

            urlBase: '/umbraco/backoffice/api/workflow/',
            urlTasksBase: '/umbraco/backoffice/api/workflowtasks/',

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
            getStatus: function (id) {
                return this.request('GET', this.urlBase + 'getStatus?nodeId=' + id);
            },
            getApprovalsForUser: function (userId) {
                return this.request('POST', this.urlTasksBase + 'getapprovalsforuser?userId=' + userId);
            },
            getSubmissionsForUser: function (userId) {
                return this.request('POST', this.urlTasksBase + 'getsubmissionsforuser?userId=' + userId);
            },
            getActiveTasks: function () {
                return this.request('GET', this.urlTasksBase + 'getactivetasks');
            },
            getNodeTasks: function(id) {
                return this.request('GET', this.urlTasksBase + 'getnodetasks?id=' + id);
            },
            initiateWorkflow: function (nodeId, comment, publish) {
                return this.request('POST', this.urlTasksBase + 'initiateWorkflow', {'nodeId': nodeId, 'comment': comment, 'publish': publish });
            },
            approveWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'approveworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            rejectWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'rejectworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            cancelWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'cancelworkflowtask?taskId=' + taskId + (comment !== null ? '&comment=' + comment : ''));
            },
            showDifferences: function (nodeId, taskId) {
                return this.request('POST', this.urlTasksBase + 'showdifferences?nodeId=' + nodeId + '&taskId=' + taskId);
            },

            /* get/set workflow settings*/
            getSettings: function () {
                return this.request('GET', this.urlBase + 'getSettings');
            },
            saveSettings: function (settings) {
                return this.request('POST', this.urlBase + 'saveSettings', settings);
            },

            /*** SAVE PERMISSIONS ***/
            saveConfig: function (p) {
                return this.request('POST', '/umbraco/backoffice/api/workflowconfig/saveconfig', p);              
            }

        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('workflowResource', WorkflowResource);

}());