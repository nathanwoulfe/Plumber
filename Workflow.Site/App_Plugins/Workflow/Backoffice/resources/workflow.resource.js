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
                        $http.get(url, { params: data }) :
                        $http.post(url, data),
                    'Something broke'
                );
            },

            getStatus: function (id) {
                return this.request('GET', this.urlTaskBase + 'getStatus', { nodeId: id });
            },

            /* tasks and approval endpoints */
            getApprovalsForUser: function (userId) {
                return this.request('GET', this.urlTasksBase + 'getflowsforuser', { type: 0, userId: userId });
            },
            getSubmissionsForUser: function (userId) {
                return this.request('GET', this.urlTasksBase + 'getflowsforuser', { type: 1, userId: userId });
            },
            getPendingTasks: function () {
                return this.request('GET', this.urlTasksBase + 'getpendingtasks');
            },
            getAllTasks: function () {
                return this.request('GET', this.urlTasksBase + 'getalltasks');
            },
            getAllInstances: function () {
                return this.request('GET', this.urlTasksBase + 'getallinstances');
            },
            getNodeTasks: function(id) {
                return this.request('GET', this.urlTasksBase + 'getnodetasks', { id: id });
            },

            /* workflow actions */
            initiateWorkflow: function (nodeId, comment, publish) {
                return this.request('POST', this.urlTasksBase + 'initiateWorkflow', { 'nodeId': nodeId, 'comment': comment, 'publish': publish });
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