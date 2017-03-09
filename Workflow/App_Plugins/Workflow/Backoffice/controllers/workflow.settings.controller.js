(function () {
    'use strict';

    function settingsController($q, workflowResource, notificationsService, contentTypeResource, userGroupsResource) {
        var vm = this,
            promises = [workflowResource.getSettings(), contentTypeResource.getAll(), userGroupsResource.get()];

        $q.all(promises)
            .then(function (resp) {

                vm.settings = resp[0];
                vm.docTypes = resp[1];
                vm.groups = resp[2];

                if (vm.settings.defaultApprover) {
                    vm.defaultApprover = vm.groups.filter(function (v) {
                        return v.groupId == vm.settings.defaultApprover;
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
                            })
                        }
                    });
                });
            });


        function save() {
            var permissions = [];
            vm.settings.defaultApprover = vm.defaultApprover.groupId;
            angular.forEach(vm.docTypes, function (dt, i) {
                if (dt.approvalPath && dt.approvalPath.length) {
                    angular.forEach(dt.approvalPath, function (path, ii) {
                        permissions.push({
                            contentTypeId: dt.id,
                            permission: ii,
                            groupId: path.groupId,
                        });
                    });
                }
            });

            var p = [workflowResource.saveConfig(permissions), workflowResource.saveSettings(vm.settings)];
            $q.all(p)
                .then(function (resp) {
                    notificationsService.success("SUCCESS!", "Settings updated");
                }, function (err) {
                    notificationsService.error("OH SNAP!", err);
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
            dt.approvalPath.splice(index, 1);
        };

        angular.extend(vm, {
            save: save,
            add: add,
            remove: remove,
            name: 'Workflow settings',

            email: '',
            defaultApprover: '',
            settings: {
                email: '',
                defaultApprover: ''
            }
        });
    }

    angular.module('umbraco').controller('Workflow.Settings.Controller', settingsController);

}());