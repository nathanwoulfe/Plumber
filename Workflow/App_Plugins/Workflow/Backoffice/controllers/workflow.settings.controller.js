(function () {
    'use strict';

    function settingsController($scope, $q, workflowResource, notificationsService, workflowGroupsResource, contentResource) {
        var vm = this,
            promises = [workflowResource.getSettings(), workflowResource.getContentTypes(), workflowGroupsResource.get()];

        vm.excludeNodesModel = {
            view: 'contentpicker',
            alias: 'excludeNodesPicker',
            config: {
                multiPicker: '1'
            }
        };

        $q.all(promises)
            .then(function (resp) {

                vm.settings = resp[0];
                vm.docTypes = resp[1];
                vm.groups = resp[2];

                if (vm.settings.excludeNodes) {
                    vm.excludeNodesModel.value = vm.settings.excludeNodes;

                    // this feels super hacky - fetch nodes and push into the content picker
                    // there's a watch in the picker controller, but it's not seeing changes to the value
                    var picker = document.querySelector('#exclude-nodes-picker ng-form');
                    if (picker) {
                        var s = angular.element(picker).scope();

                        vm.settings.excludeNodes.split(',').forEach(function(id) {
                            contentResource.getById(id).then(function (entity) {
                                s.add(entity);
                            });
                        });
                    }
                }

                vm.flowTypes = [
                    { i: 0, v: 'Explicit' },
                    { i: 1, v: 'Implicit' },
                ];

                vm.flowType = vm.flowTypes[vm.settings.flowType];

                if (vm.settings.defaultApprover) {
                    vm.defaultApprover = vm.groups.filter(function (v) {
                        return parseInt(v.groupId, 10) === parseInt(vm.settings.defaultApprover, 10);
                    })[0];
                }
                
                vm.groups.forEach(function (g) {
                    g.permissions.forEach(function (p) {
                        if (p.contentTypeId > 0) {
                            vm.docTypes.forEach(function (dt) {
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


        function save() {
            var permissions = {};
            vm.settings.defaultApprover = vm.defaultApprover.groupId;
            vm.settings.flowType = vm.flowType.i;

            if (vm.excludeNodesModel.value) {
                vm.settings.excludeNodes = vm.excludeNodesModel.value;
            }

            // convert the approval path group collection into a set of permissions objects for saving
            // means we're holding extra data, but makes it easier to manipulate as it's less abstract
            angular.forEach(vm.docTypes, function (dt, i) {
                if (dt.approvalPath && dt.approvalPath.length) {
                    permissions[i] = [];
                    angular.forEach(dt.approvalPath,
                        function (path, ii) {
                            permissions[i].push({
                                contentTypeId: dt.id,
                                permission: ii,
                                groupId: path.groupId
                            });
                        });
                }
            });

            var p = [workflowResource.saveDocTypeConfig(permissions), workflowResource.saveSettings(vm.settings)];

            $q.all(p)
                .then(function () {
                    notificationsService.success('SUCCESS!', 'Settings updated');
                }, function (err) {
                    notificationsService.error('OH SNAP!', err);
                });
        }

        

        /**
         * Removes the approval path for the group, which will remove it from config on save
         * @param {any} type
         */
        function removeDocTypeFlow(type) {
            delete type.approvalPath;
        }

        function editDocTypeFlow(type) {
            vm.overlay = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.contenttypeflow.overlay.html',
                show: true,
                type: type,
                groups: vm.groups,
                title: 'Edit content type approval flow',
                submit: function (model) {

                    // map the updated approval path back onto the doctypes collection 
                    if (model.type.approvalPath.length) {
                        vm.docTypes.forEach(function(v, i) {
                            if (v.id === model.type.id) {
                                v.approvalPath = model.type.approvalPath;
                            }
                        });
                    }

                    vm.overlay.close();
                },
                close: function () {
                    vm.overlay.show = false;
                    vm.overlay = null;
                }
            };
        }

        function openContentTypePicker() {
            vm.overlay = {
                view: '../app_plugins/workflow/backoffice/dialogs/workflow.contenttypepicker.overlay.html',
                show: true,
                types: vm.docTypes.filter(function(v) {
                    return !v.approvalPath;
                }),
                title: 'Select content type',
                submit: function (model) {

                    // if a selection is returned, add an approval group array to the doctype
                    // so that it appears in the list view
                    if (model.selection) {
                        vm.docTypes.forEach(function(v) {
                            if (v.id === model.selection.id) {
                                    v.approvalPath = [];
                            }
                        });
                    }

                    vm.overlay.close();
                },
                close: function () {
                    vm.overlay.show = false;
                    vm.overlay = null;
                }
            };
        }

        angular.extend(vm, {
            save: save,
            editDocTypeFlow: editDocTypeFlow,
            removeDocTypeFlow: removeDocTypeFlow,
            openContentTypePicker: openContentTypePicker,
            hasApprovalPath: function(d) {
                return d.approvalPath !== undefined;
            },

            name: 'Workflow settings',

            email: '',
            defaultApprover: '',
            settings: {
                email: '',
                defaultApprover: ''
            }
        });
    }

    angular.module('umbraco').controller('Workflow.Settings.Controller',
        ['$scope', '$q', 'plmbrWorkflowResource', 'notificationsService', 'plmbrGroupsResource', 'contentResource', settingsController]);

}());