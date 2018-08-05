(() => {

    const urlBase = '/umbraco/backoffice/api/workflow/';

    const urls = {
        tasks: urlBase + 'tasks/',
        actions: urlBase + 'actions/'
    };

    function workflowPreviewService($http, $q, umbRequestHelper, notificationsService) {

        const dialogPath = '/app_plugins/workflow/backoffice/views/dialogs/';

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke');

        /* workflow actions */
        const approve = (instanceGuid, comment) =>
            request('POST',
                urls.actions + 'approve',
                { instanceGuid: instanceGuid, comment: comment });

        const reject = (instanceGuid, comment) =>
            request('POST',
                urls.actions + 'reject',
                { instanceGuid: instanceGuid, comment: comment });

        // display notification after actioning workflow task
        const notify = d => {
            if (d.status === 200) {
                notificationsService.success('SUCCESS', d.message);
            } else {
                notificationsService.error('OH SNAP', d.message);
            }
        };

        const service = {

            action: (item, type) => {
                let workflowOverlay = {
                    view: dialogPath + 'workflow.action.dialog.html',
                    show: true,
                    title: type + ' workflow process',
                    subtitle: `Document: ${item.nodeName}`,
                    comment: item.comment,
                    approvalComment: '',
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    submit: model => {
                        // build the function name and access it via index rather than property - saves duplication
                        if (type === 'Approve') {
                            approve(item.instanceGuid, model.approvalComment)
                                .then(resp => {
                                    notify(resp);
                                });
                        }
                        else if (type === 'Reject') {
                            reject(item.instanceGuid, model.approvalComment)
                                .then(resp => {
                                    notify(resp);
                                });
                        }

                        workflowOverlay.close();
                    },
                    close: () => {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },
        };

        return service;
    }

    angular.module('plumber.services').factory('workflowPreviewService', ['$http', '$q', 'umbRequestHelper', 'notificationsService', workflowPreviewService]);


    // clone the workflow resource and remove a heap of unused stuff, keeping only what's needed for front-end approval
    function workflowResource($http, $q, umbRequestHelper) {

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke');

        return {
            getTask: id => request('GET', urls.tasks + 'get/' + id),
            getAllTasksByGuid: guid => request('GET', urls.tasks + 'tasksbyguid/' + guid)
        };
    }

    // register service
    angular.module('plumber.services').factory('plmbrWorkflowResource', ['$http', '$q', 'umbRequestHelper', workflowResource]);

})();