(function () {
    'use strict';

    function workflowActionsService($rootScope, workflowResource, notificationsService) {

        var service = {

            dialogPath: '../app_plugins/workflow/backoffice/dialogs/',

            action: function (item, type, fromDash) {

                var workflowOverlay = {
                    view: this.dialogPath + 'workflow.action.dialog.html',
                    show: true,
                    title: type + ' workflow process',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: item.comments,
                    approvalComment: '',
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    submit: function (model) {

                        buttonState('busy', item.nodeId);

                        // build the function name and access it via index rather than property - saves duplication
                        var functionName = type.toLowerCase() + 'WorkflowTask';
                        workflowResource[functionName](item.instanceGuid, model.approvalComment)
                            .then(function (resp) {
                                notify(resp, fromDash, item.nodeId);
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

            initiate: function (name, id, dirty, publish) {
                var workflowOverlay = {
                    view: this.dialogPath + 'workflow.submit.dialog.html',
                    show: true,
                    title: 'Send for ' + (publish ? 'publish' : 'unpublish') + ' approval',
                    subtitle: 'Document: ' + name,
                    isPublish: publish,
                    isDirty: dirty,
                    nodeId: id,
                    submit: function (model) {

                        buttonState('busy', id);

                        workflowResource.initiateWorkflow(id, model.comment, publish)
                            .then(function(resp) {
                                notify(resp, false, id);
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
                    view: this.dialogPath + 'workflow.cancel.dialog.html',
                    show: true,
                    title: 'Cancel workflow process',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: '',
                    isFinalApproval: item.activeTask === 'Pending Final Approval',
                    submit: function (model) {

                        buttonState('busy', item.nodeId);

                        workflowResource.cancelWorkflowTask(item.instanceGuid, model.comment)
                            .then(function (resp) {
                                notify(resp, fromDash, item.nodeId);
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

            detail: function (item) {

                var workflowOverlay = {
                    view: this.dialogPath + 'workflow.action.dialog.html',
                    show: true,
                    title: 'Workflow detail',
                    subtitle: 'Document: ' + item.nodeName,
                    comment: item.comments,
                    guid: item.instanceGuid,
                    requestedBy: item.requestedBy,
                    requestedOn: item.requestedOn,
                    detail: true,
                    
                    close: function () {
                        workflowOverlay.show = false;
                        workflowOverlay = null;
                    }
                };

                return workflowOverlay;
            },

            buttonState: function(state, id) {
                buttonState(state, id);
            }
        };

        // UI feedback for button directive
        function buttonState(state, id) {
            $rootScope.$emit('buttonStateChanged', { state: state, id: id });
        }

        // display notification after actioning workflow task
        function notify(d, fromDash, id) {
            if (d.status === 200) {

                notificationsService.success('SUCCESS', d.message);

                if (fromDash) {
                    $rootScope.$emit('refreshWorkflowDash');
                }
                $rootScope.$emit('workflowActioned');
                buttonState('success', id);
            }
            else {
                notificationsService.error('OH SNAP', d.message);
                buttonState('error', id);
            }
        }

        return service;
    }

    angular.module('umbraco.services').factory('workflowActionsService', workflowActionsService);

}());