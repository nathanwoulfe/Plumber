(function () {
    'use strict';

    function workflowActionsService($rootScope, workflowResource, notificationsService) {
        var service = {

            action: function (item, approve, fromDash) {
                var workflowOverlay = {
                    view: '../app_plugins/workflow/backoffice/dialogs/workflow.action.dialog.html',
                    show: true,
                    title: (approve ? 'Approve' : 'Reject') + ' workflow process',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: item.comments,
                    approvalComment: '',
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    submit: function (model) {
                        if (approve) {
                            workflowResource.approveWorkflowTask(item.taskId, model.comment)
                                .then(function (resp) {
                                    notify(resp, fromDash);
                                });
                        }
                        else {
                            workflowResource.rejectWorkflowTask(item.taskId, model.comment)
                                .then(function (resp) {
                                    notify(resp, fromDash);
                                });
                        }
                        workflowOverlay.close();
                    },
                    close: function (model) {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            initiate: function (name, id, publish) {
                var workflowOverlay = {
                    view: '../app_plugins/workflow/backoffice/dialogs/workflow.submit.dialog.html',
                    show: true,
                    title: 'Send for ' + (publish ? 'publish' : 'unpublish') + ' approval',
                    subtitle: 'Document: ' + name,
                    isPublish: publish,
                    nodeId: id,
                    submit: function (model) {
                        workflowResource.initiateWorkflow(id, model.comment, publish)
                            .then(function (resp) {
                                notify(resp);
                            });

                        workflowOverlay.close();
                    },
                    close: function (model) {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            cancel: function (item, fromDash) {
                var workflowOverlay = {
                    view: '../app_plugins/workflow/backoffice/dialogs/workflow.cancel.dialog.html',
                    show: true,
                    title: 'Cancel workflow process',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: '',
                    isFinalApproval: item.activeTask === 'Pending Final Approval',
                    submit: function (model) {
                        workflowResource.cancelWorkflowTask(item.taskId, model.comment)
                            .then(function (resp) {
                                notify(resp, fromDash);
                                workflowOverlay.close();
                            });
                    },
                    close: function (model) {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            }
        };

        // display notification after actioning workflow task
        function notify(d, fromDash) {
            if (d.status === 200) {
                var contentForm = document.querySelector('[name="contentForm"]');
                if (contentForm) {
                    var scope = angular.element(contentForm).scope();
                    scope.contentForm.$setPristine();
                }
                notificationsService.success("SUCCESS!", d.message);

                if (fromDash) {
                    $rootScope.$emit('refreshWorkflowDash');
                }
            }
            else {
                notificationsService.error("OH SNAP!", d.message);
            }
        }

        return service;
    }

    angular.module('umbraco.services').factory('workflowActionsService', workflowActionsService);

}());