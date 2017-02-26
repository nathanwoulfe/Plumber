(function () {
    'use strict';

    function settingsController($q, workflowResource, notificationsService, contentTypeResource, userGroupsResource) {
        var vm = this,
            promises = [workflowResource.getSettings(), contentTypeResource.getAll(), userGroupsResource.getAllGroups()];

        $q.all(promises)
            .then(function (resp) {

                vm.settings = resp[0].data;
                vm.docTypes = resp[1];
                vm.groups = resp[2].data;

                if (vm.settings.DefaultApprover) {
                    vm.defaultApprover = vm.groups.filter(function (v) {
                        return v.GroupId == vm.settings.DefaultApprover;
                    })[0];
                }

                vm.groups.forEach(function (g) {
                    g.Permissions.forEach(function (p) {
                        if (p.ContentTypeId > 0) {
                            vm.docTypes.forEach(function (dt) {
                                if (dt.id === p.ContentTypeId) {
                                    if (!dt.approvalPath) {
                                        dt.approvalPath = [];
                                    }

                                    dt.approvalPath[p.Permission] = g;
                                }
                            })
                        }
                    });
                });
            });


        function save() {

            vm.settings.DefaultApprover = vm.defaultApprover.GroupId;
            var permissions = [];
            angular.forEach(vm.docTypes, function (dt, i) {
                if (dt.approvalPath && dt.approvalPath.length) {
                    angular.forEach(dt.approvalPath, function (path, ii) {
                        permissions.push({
                            ContentTypeId: dt.id,
                            Permission: ii,
                            GroupId: path.GroupId,
                        });
                    });
                }
            });

            var p = [workflowResource.saveConfig(permissions), workflowResource.saveSettings(vm.settings)];
            $q.all(p)
                .then(function (resp) {
                    if (resp[0].status === 200 && resp[1].status === 200) {
                        notificationsService.success("SUCCESS!", resp.data);
                    }
                    else {
                        notificationsService.error("OH SNAP!", resp[resp[0].status !== 200 ? 0 : 1].data);
                    }
                });
        }

        function add(dt) {
            if (dt.approvalPath) {
                dt.approvalPath.push(dt.selectedApprovalGroup);
            } else {
                dt.approvalPath = [dt.selectedApprovalGroup];
            }       
        };


        function remove(dt, index) {
            console.log(dt, index);
        };

        angular.extend(vm, {
            save: save,
            add: add,
            remove: remove,
            name: 'Workflow settings',

            email: '',
            fastTrack: [],
            notFastTrack: [],
            defaultApprover: '',
            settings: {
                Email: '',
                FastTrack: [],
                DefaultApprover: ''
            }
        });
    }

    angular.module('umbraco').controller('Workflow.Settings.Controller', settingsController);

}());