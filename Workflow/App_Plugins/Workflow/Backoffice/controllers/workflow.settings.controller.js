(() => {
    'use strict';

    function settingsController($scope, $q, workflowResource, notificationsService, workflowGroupsResource, contentResource) {

        const promises = [workflowResource.getSettings(), workflowResource.getContentTypes(), workflowGroupsResource.get()];
        const overlayBase = '../app_plugins/workflow/backoffice/dialogs/';

        this.excludeNodesModel = {
            view: 'contentpicker',
            alias: 'excludeNodesPicker',
            config: {
                multiPicker: '1'
            }
        };

        this.name = 'Workflow settings';
        this.email = '';
        this.defaultApprover = '';
        this.settings = {
            email: '',
            defaultApprover: ''
        };

        $q.all(promises)
            .then(resp => {

                [this.settings, this.docTypes, this.groups] = resp;

                if (this.settings.excludeNodes) {
                    this.excludeNodesModel.value = this.settings.excludeNodes;

                    // this feels super hacky - fetch nodes and push into the content picker
                    // there's a watch in the picker controller, but it's not seeing changes to the value
                    const picker = document.querySelector('#exclude-nodes-picker ng-form');
                    if (picker) {
                        var s = angular.element(picker).scope();

                        this.settings.excludeNodes.split(',').forEach(id => {
                            contentResource.getById(id).then(entity => {
                                s.add(entity);
                            });
                        });
                    }
                }

                this.flowTypes = [
                    { i: 0, v: 'Explicit' },
                    { i: 1, v: 'Implicit' },
                ];

                this.flowType = this.flowTypes[this.settings.flowType];

                if (this.settings.defaultApprover) {
                    this.defaultApprover = this.groups.filter(g => parseInt(g.groupId, 10) === parseInt(this.settings.defaultApprover, 10))[0];
                }

                this.groups.forEach(g => {
                    g.permissions.forEach(p => {
                        if (p.contentTypeId > 0) {
                            this.docTypes.forEach(dt => {
                                if (dt.id === p.contentTypeId) {
                                    if (!dt.approvalPath) {
                                        dt.approvalPath = [];
                                    }

                                    dt.approvalPath[p.permission] = g;
                                }
                            });
                        }
                    });
                });
            });


        this.save = () => {
            const permissions = {};

            this.settings.defaultApprover = this.defaultApprover.groupId;
            this.settings.flowType = this.flowType.i;

            if (this.excludeNodesModel.value) {
                this.settings.excludeNodes = this.excludeNodesModel.value;
            }

            // convert the approval path group collection into a set of permissions objects for saving
            // means we're holding extra data, but makes it easier to manipulate as it's less abstract
            this.docTypes.forEach((dt, i) => {
                if (dt.approvalPath && dt.approvalPath.length) {
                    permissions[i] = [];
                    dt.approvalPath.forEach((path, ii) => {
                        permissions[i].push({
                            contentTypeId: dt.id,
                            permission: ii,
                            groupId: path.groupId
                        });
                    });
                }
            });

            var p = [workflowResource.saveDocTypeConfig(permissions), workflowResource.saveSettings(this.settings)];

            $q.all(p)
                .then(
                () => { notificationsService.success('SUCCESS!', 'Settings updated'); },
                err => { notificationsService.error('OH SNAP!', err); });
        };


        /**
         * Removes the approval path for the group, which will remove it from config on save
         * @param {any} type
         */
        this.removeDocTypeFlow = type => {
            delete type.approvalPath;
        };

        this.editDocTypeFlow = type => {
            this.overlay = {
                view: `${overlayBase}workflow.contenttypeflow.overlay.html`,
                show: true,
                type: type,
                groups: this.groups,
                title: 'Edit content type approval flow',
                submit: model => {

                    // map the updated approval path back onto the doctypes collection 
                    if (model.type.approvalPath.length) {
                        this.docTypes.forEach(v => {
                            if (v.id === model.type.id) {
                                v.approvalPath = model.type.approvalPath;
                            }
                        });
                    }

                    this.overlay.close();
                },
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };

        this.openContentTypePicker = () => {
            this.overlay = {
                view: `${overlayBase}workflow.contenttypepicker.overlay.html`,
                show: true,
                types: this.docTypes.filter(v => !v.approvalPath),
                title: 'Select content type',
                submit: model => {

                    // if a selection is returned, add an approval group array to the doctype
                    // so that it appears in the list view
                    if (model.selection) {
                        this.docTypes.forEach(v => {
                            if (v.id === model.selection.id) {
                                v.approvalPath = [];
                            }
                        });
                    }

                    this.overlay.close();
                },
                close: () => {
                    this.overlay.show = false;
                    this.overlay = null;
                }
            };
        };

        this.hasApprovalPath = d => d.approvalPath !== undefined;
    }

    angular.module('umbraco').controller('Workflow.Settings.Controller',
        ['$scope', '$q', 'plmbrWorkflowResource', 'notificationsService', 'plmbrGroupsResource', 'contentResource', settingsController]);

})();