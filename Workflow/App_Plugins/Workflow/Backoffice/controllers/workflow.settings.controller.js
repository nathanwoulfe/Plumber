(() => {
    'use strict';

    function settingsController($scope, $q, workflowResource, notificationsService, workflowGroupsResource, contentResource) {

        const promises = [workflowResource.getSettings(), workflowResource.getContentTypes(), workflowGroupsResource.get()];
        const overlayBase = Umbraco.Sys.ServerVariables.workflow.overlayPath;

        this.excludeNodesModel = {
            view: 'contentpicker',
            editor: 'Umbraco.MultiNodeTreePicker2',
            alias: 'excludeNodesPicker',
            config: {
                multiPicker: '1',
                maxNumber: null,
                minNumber: null,
                idType: 'id',
                showEditButton: '0',
                showOpenButton: '0',
                showPathOnHover: '0',
                startNode: {
                    type: 'content'
                }
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

                            if (p.condition) {
                                p.condition = p.condition.split(',');
                            }

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

            this.settings.defaultApprover = this.defaultApprover ? this.defaultApprover.groupId : '';
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
                        const permissionObject = {
                            contentTypeId: dt.id,
                            permission: ii,
                            groupId: path.groupId
                        };

                        // filter the permissions for cases where the contenttype id and permission index match the current loop iteration
                        const permissionsForStep =
                            path.permissions.filter(perm => perm.contentTypeId === dt.id && perm.permission === ii)[0];

                        // if a permission exists, push it onto the current permission object
                        if (permissionsForStep && permissionsForStep.condition) {
                            permissionObject.condition = permissionsForStep.condition.join(',');
                        }

                        permissions[i].push(permissionObject);
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
                view: `${overlayBase}/workflow.contenttypeflow.overlay.html`,
                show: true,
                type: type,
                groups: this.groups,
                types: this.docTypes.filter(v => !v.approvalPath),
                title: `${type ? 'Edit' : 'Add'} content type approval flow`,
                submit: model => {

                    // map the updated approval path back onto the doctypes collection 
                    if (model.type.approvalPath.length) {

                        // multi has a value when adding - can add more than one
                        const ids = model.typesToAdd ? model.typesToAdd.map(t => t.id) : [model.type.id];

                        this.docTypes.forEach(v => {
                            if (ids.indexOf(v.id) !== -1) {
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

        this.hasApprovalPath = d => d.approvalPath !== undefined;
    }

    angular.module('plumber').controller('Workflow.Settings.Controller',
        ['$scope', '$q', 'plmbrWorkflowResource', 'notificationsService', 'plmbrGroupsResource', 'contentResource', settingsController]);

})();