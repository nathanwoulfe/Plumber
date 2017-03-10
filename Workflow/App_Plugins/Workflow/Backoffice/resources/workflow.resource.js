(function () {
    'use strict';

    // create service
    function WorkflowResource($http, $q, umbRequestHelper) {
        var service = {

            urlSettingsBase: '/umbraco/backoffice/api/workflow/settings/',
            urlTasksBase: '/umbraco/backoffice/api/workflow/tasks/',

            request: function (method, url, data) {
                return umbRequestHelper.resourcePromise(
                    method === 'GET' ?
                        $http.get(url) :
                        $http.post(url, data),
                    'Something broke'
                );
            },

            getStatus: function (id) {
                return this.request('GET', this.urlTasksBase + 'status/' + id);
            },

            /* tasks and approval endpoints */
            getApprovalsForUser: function (userId) {
                return this.request('GET', this.urlTasksBase + 'flows/' + userId);
            },
            getSubmissionsForUser: function (userId) {
                return this.request('GET', this.urlTasksBase + 'flows/' + userId + '/1');
            },
            getPendingTasks: function () {
                return this.request('GET', this.urlTasksBase + 'pending');
            },
            getAllTasks: function () {
                return this.request('GET', this.urlTasksBase + 'all');
            },
            getAllInstances: function () {
                return this.request('GET', this.urlTasksBase + 'instances');
            },
            getNodeTasks: function(id) {
                return this.request('GET', this.urlTasksBase + 'node/' + id);
            },

            /* workflow actions */
            initiateWorkflow: function (nodeId, comment, publish) {
                return this.request('POST', this.urlTasksBase + 'initiate', { nodeId: nodeId, comment: comment, publish: publish });
            },
            approveWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'approve', { taskId: taskId, comment: comment });
            },
            rejectWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'reject', { taskId: taskId, comment: comment });
            },
            cancelWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.urlTasksBase + 'cancel', { taskId: taskId, comment: comment });
            },
            //showDifferences: function (nodeId, taskId) {
            //    return this.request('POST', this.urlTasksBase + 'showdifferences?nodeId=' + nodeId + '&taskId=' + taskId);
            //},

            /* get/set workflow settings*/
            getSettings: function () {
                return this.request('GET', this.urlSettingsBase + 'get');
            },
            saveSettings: function (settings) {
                return this.request('POST', this.urlSettingsBase + 'save', settings);
            },

            /*** SAVE PERMISSIONS ***/
            saveConfig: function (p) {
                return this.request('POST', '/umbraco/backoffice/api/workflow/config/save', p);              
            }

        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('workflowResource', WorkflowResource);

}());