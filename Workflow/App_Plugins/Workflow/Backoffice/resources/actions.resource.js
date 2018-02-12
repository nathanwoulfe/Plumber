(function () {
    'use strict';

    function workflowActionsService($rootScope, workflowResource, notificationsService) {

        var service = {

            action: function (item, approve, fromDash) {
                var workflowOverlay = {
                    view: '../app_plugins/workflow/backoffice/dialogs/workflow.action.dialog.html',
                    show: true,
                    typeForText: approve ? 'an approval' : 'a rejection',
                    title: (approve ? 'Approve' : 'Reject') + ' workflow process',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: item.comments,
                    approvalComment: '',
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    submit: function (model) {

                        buttonState('busy');
                        if (approve) {
                            workflowResource.approveWorkflowTask(item.instanceGuid, model.approvalComment)
                                .then(function (resp) {
                                    notify(resp, fromDash);
                                });
                        }
                        else {
                            workflowResource.rejectWorkflowTask(item.instanceGuid, model.approvalComment)
                                .then(function (resp) {
                                    notify(resp, fromDash);
                                });
                        }
                        workflowOverlay.close();
                    },
                    close: function () {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            initiate: function (name, id, dirty, publish) {
                var workflowOverlay = {
                    view: '../app_plugins/workflow/backoffice/dialogs/workflow.submit.dialog.html',
                    show: true,
                    title: 'Send for ' + (publish ? 'publish' : 'unpublish') + ' approval',
                    subtitle: 'Document: ' + name,
                    isPublish: publish,
                    isDirty: dirty,
                    nodeId: id,
                    submit: function (model) {

                        buttonState('busy');

                        workflowResource.initiateWorkflow(id, model.comment, publish)
                            .then(function (resp) {
                                notify(resp);
                            });

                        workflowOverlay.close();
                    },
                    close: function () {
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

                        buttonState('busy');

                        workflowResource.cancelWorkflowTask(item.instanceGuid, model.comment)
                            .then(function (resp) {
                                notify(resp, fromDash);
                            });

                        workflowOverlay.close();
                    },
                    close: function () {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            }
        };

        // UI feedback for button directive
        function buttonState(state) {
            $rootScope.$emit('buttonStateChanged', state);
        }

        // display notification after actioning workflow task
        function notify(d, fromDash) {
            if (d.status === 200) {

                notificationsService.success('SUCCESS!', d.message);

                if (fromDash) {
                    $rootScope.$emit('refreshWorkflowDash');
                }
                $rootScope.$emit('workflowActioned');
                $rootScope.$emit('buttonStateChanged', 'success');
            }
            else {
                notificationsService.error('OH SNAP!', d.message);
                $rootScope.$emit('buttonStateChanged', 'error');
            }
        }

        return service;
    }

    angular.module('umbraco.services').factory('workflowActionsService', workflowActionsService);

}());