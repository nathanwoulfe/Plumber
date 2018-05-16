(() => {
    'use strict';

    // create service
    function workflowResource($http, $q, umbRequestHelper) {
        const urlBase = '/umbraco/backoffice/api/workflow/';

        // are there common elements between two arrays?
        const common = (arr1, arr2) => arr1.some(el => arr2.indexOf(el) > -1);

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke');

        const urls = {
            settings: urlBase + 'settings/',
            tasks: urlBase + 'tasks/',
            instances: urlBase + 'instances/',
            actions: urlBase + 'actions/',
            logs: urlBase + 'logs/',
        };

        const service = {

            getContentTypes: () => request('GET', urls.settings + 'getcontenttypes'),

            /* tasks and approval endpoints */
            getApprovalsForUser: (userId, count, page) => request('GET', urls.tasks + 'flows/' + userId + '/0/' + count + '/' + page),

            getSubmissionsForUser: (userId, count, page) => request('GET', urls.tasks + 'flows/' + userId + '/1/' + count + '/' + page),

            getPendingTasks: (count, page) => request('GET', urls.tasks + 'pending/' + count + '/' + page),

            getAllTasksForRange: days => request('GET', urls.tasks + 'range/' + days),

            getAllInstances: (count, page) => request('GET', urls.instances + count + '/' + page),

            getAllInstancesForRange: days => request('GET', urls.instances + 'range/' + days),

            getAllTasksForGroup: (groupId, count, page) => request('GET', urls.tasks + 'group/' + groupId + '/' + count + '/' + page),

            getAllTasksByGuid: guid => request('GET', urls.tasks + 'tasksbyguid/' + guid),

            getNodeTasks: (id, count, page) => request('GET', urls.tasks + 'node/' + id + '/' + count + '/' + page),

            getNodePendingTasks: id => request('GET', urls.tasks + 'node/pending/' + id),


            /* workflow actions */
            initiateWorkflow: (nodeId, comment, publish) =>
                request('POST',
                    urls.actions + 'initiate', { nodeId: nodeId, comment: comment, publish: publish }),

            approveWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'approve', { instanceGuid: instanceGuid, comment: comment }),

            rejectWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'reject', { instanceGuid: instanceGuid, comment: comment }),

            resubmitWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'resubmit', { instanceGuid: instanceGuid, comment: comment }),

            cancelWorkflowTask: (instanceGuid, comment) =>
                request('POST',
                    urls.actions + 'cancel', { instanceGuid: instanceGuid, comment: comment }),


            /* get/set workflow settings*/
            getSettings: () => request('GET', urls.settings + 'get'),

            saveSettings: settings => request('POST', urls.settings + 'save', settings),

            getVersion: () => request('GET', urls.settings + 'version'),

            getDocs: () => request('GET', urls.settings + 'docs'),

            getLog: date => request('GET', urls.logs + 'get/' + (date || '')),

            getLogDates: () => request('GET', urls.logs + 'datelist'),


            doImport: model => request('POST', urlBase + 'import', model),

            doExport: () => request('GET', urlBase + 'export'),

            /*** SAVE PERMISSIONS ***/
            saveConfig: p => request('POST', urlBase + 'config/saveconfig', p),

            saveDocTypeConfig: p => request('POST', urlBase + 'config/savedoctypeconfig', p),

            checkExclusion: (excludedNodes, path) => {
                if (!excludedNodes) {
                    return false;
                }

                const excluded = excludedNodes.split(',');
                // if any elements are shared, exclude the node from the workflow mechanism
                // by checking the path not just the id, this becomes recursive, and the excludeNodes cascades down the tree
                return common(path.split(','), excluded);
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

})();