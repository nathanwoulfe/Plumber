(function () {
    'use strict';

    // create service
    function WorkflowResource($http, $q, umbRequestHelper) {
        var urlBase = '/umbraco/backoffice/api/workflow/';

        var service = {

            settingsUrl: urlBase + 'settings/',
            tasksUrl: urlBase + 'tasks/',
            instancesUrl: urlBase + 'instances/',
            actionsUrl: urlBase + 'actions/',

            request: function (method, url, data) {
                return umbRequestHelper.resourcePromise(
                    method === 'GET' ?
                        $http.get(url) :
                        $http.post(url, data),
                    'Something broke'
                );
            },

            getStatus: function (id) {
                return this.request('GET', this.tasksUrl + 'status/' + id);
            },

            getContentTypes: function() {
                return this.request('GET', this.settingsUrl + 'getcontenttypes');
            },

            /* tasks and approval endpoints */
            getApprovalsForUser: function (userId, count, page) {
                return this.request('GET', this.tasksUrl + 'flows/' + userId + '/0/' + count + '/' + page);
            },
            getSubmissionsForUser: function (userId, count, page) {
                return this.request('GET', this.tasksUrl + 'flows/' + userId + '/1/' + count + '/' + page);
            },
            getPendingTasks: function (count, page) {
                return this.request('GET', this.tasksUrl + 'pending/' + count + '/' + page);
            },
            getAllTasksForRange: function (days) {
                return this.request('GET', this.tasksUrl + 'range/' + days);
            },
            getAllInstances: function (count, page) {
                return this.request('GET', this.instancesUrl + count + '/' + page);
            },
            getAllInstancesForRange: function (days) {
                return this.request('GET', this.instancesUrl + 'range/' + days);
            },
            getAllTasksForGroup: function (groupId, count, page) {
                return this.request('GET', this.tasksUrl + 'group/' + groupId + '/' + count + '/' + page);
            },
            getNodeTasks: function(id, count, page) {
                return this.request('GET', this.tasksUrl + 'node/' + id  + '/' + count + '/' + page);
            },
            getNodePendingTasks: function(id) {
                return this.request('GET', this.tasksUrl + 'node/pending/' + id);
            },

            /* workflow actions */
            initiateWorkflow: function (nodeId, comment, publish) {
                return this.request('POST', this.actionsUrl + 'initiate', { nodeId: nodeId, comment: comment, publish: publish });
            },
            approveWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.actionsUrl + 'approve', { taskId: taskId, comment: comment });
            },
            rejectWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.actionsUrl + 'reject', { taskId: taskId, comment: comment });
            },
            cancelWorkflowTask: function (taskId, comment) {
                return this.request('POST', this.actionsUrl + 'cancel', { taskId: taskId, comment: comment });
            },

            /* get/set workflow settings*/
            getSettings: function () {
                return this.request('GET', this.settingsUrl + 'get');
            },
            saveSettings: function (settings) {
                return this.request('POST', this.settingsUrl + 'save', settings);
            },

            /*** SAVE PERMISSIONS ***/
            saveConfig: function (p) {
                return this.request('POST', urlBase + 'config/saveconfig', p);              
            },

            saveDocTypeConfig: function (p) {
              return this.request('POST', urlBase + 'config/savedoctypeconfig', p);
            },

            /**
             *  Helper for generating node path for setting active state in tree
             * @param {} node 
             * @param {} path 
             * @returns {} 
             */
            buildPath: function(node, path) {
              path.push(node.id);

              if (node.id === '-1') {
                return path.reverse();
              }

              var parent = node.parent();

              if (parent === undefined) {
                return path;
              }

              return service.buildPath(parent, path);
            }

        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('workflowResource', WorkflowResource);

}());