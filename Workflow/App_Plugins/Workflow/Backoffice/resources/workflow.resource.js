(function () {
    'use strict';

    // create service
    function workflowResource($http, $q, umbRequestHelper) {
        var urlBase = '/umbraco/backoffice/api/workflow/';

        var service = {
            settingsUrl: urlBase + 'settings/',
            tasksUrl: urlBase + 'tasks/',
            instancesUrl: urlBase + 'instances/',
            actionsUrl: urlBase + 'actions/',
            logsUrl: urlBase + 'logs/',

            request: function (method, url, data) {
                return umbRequestHelper.resourcePromise(
                    method === 'GET' ?
                        $http.get(url) :
                        $http.post(url, data),
                    'Something broke'
                );
            },

            getContentTypes: function () {
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
            getAllTasksByGuid: function (guid) {
                return this.request('GET', this.tasksUrl + 'tasksbyguid/' + guid);
            },
            getNodeTasks: function (id, count, page) {
                return this.request('GET', this.tasksUrl + 'node/' + id + '/' + count + '/' + page);
            },
            getNodePendingTasks: function (id) {
                return this.request('GET', this.tasksUrl + 'node/pending/' + id);
            },

            /* workflow actions */
            initiateWorkflow: function (nodeId, comment, publish) {
                return this.request('POST', this.actionsUrl + 'initiate', { nodeId: nodeId, comment: comment, publish: publish });
            },
            approveWorkflowTask: function (instanceGuid, comment) {
                return this.request('POST', this.actionsUrl + 'approve', { instanceGuid: instanceGuid, comment: comment });
            },
            rejectWorkflowTask: function (instanceGuid, comment) {
                return this.request('POST', this.actionsUrl + 'reject', { instanceGuid: instanceGuid, comment: comment });
            },
            resubmitWorkflowTask: function (instanceGuid, comment) {
                return this.request('POST', this.actionsUrl + 'resubmit', { instanceGuid: instanceGuid, comment: comment });
            },
            cancelWorkflowTask: function (instanceGuid, comment) {
                return this.request('POST', this.actionsUrl + 'cancel', { instanceGuid: instanceGuid, comment: comment });
            },

            /* get/set workflow settings*/
            getSettings: function () {
                return this.request('GET', this.settingsUrl + 'get');
            },
            saveSettings: function (settings) {
                return this.request('POST', this.settingsUrl + 'save', settings);
            },

            getVersion: function () {
                return this.request('GET', this.settingsUrl + 'version');
            },
            getDocs: function () {
                return this.request('GET', this.settingsUrl + 'docs');
            },
            getLog: function (date) {
                return this.request('GET', this.logsUrl + 'get/' + (date || ''));
            },
            getLogDates: function () {
                return this.request('GET', this.logsUrl + 'datelist');
            },

            doImport: function (model) {
                return this.request('POST', urlBase + 'import', model);
            },

            doExport: function () {
                return this.request('GET', urlBase + 'export');
            },

            /*** SAVE PERMISSIONS ***/
            saveConfig: function (p) {
                return this.request('POST', urlBase + 'config/saveconfig', p);
            },

            saveDocTypeConfig: function (p) {
                return this.request('POST', urlBase + 'config/savedoctypeconfig', p);
            },

            checkNodePermissions: (groups, id, contentTypeAlias) => {
                const resp = {
                    approvalPath: [],
                    contentTypeApprovalPath: []
                };

                groups.forEach(v => {
                    v.permissions.forEach(p => {
                        if (p.nodeId === id) {
                            resp.approvalPath[p.permission] = v;
                        }

                        if (p.contentTypeAlias === contentTypeAlias) {
                            resp.contentTypeApprovalPath[p.permission] = v;
                        }
                    });
                });
                return resp;
            },

            checkAncestorPermissions: (path, groups) => {
                // first is -1, last is the current node
                path = path.split(',');
                path.shift();
                path.pop();

                const resp = [];

                path.forEach(id => {
                    groups.forEach(group => {
                        group.permissions.forEach(p => {
                            if (p.nodeId === parseInt(id, 10)) {
                                resp[p.permission] = {
                                    name: group.name,
                                    groupId: p.groupId,
                                    nodeName: p.nodeName,
                                    permission: p.permission
                                };
                            }
                        });
                    });
                });

                return resp;
            }

        };

        return service;
    }

    // register service
    angular.module('umbraco.services').factory('plmbrWorkflowResource', workflowResource);

}());