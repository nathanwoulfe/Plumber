(function () {
    'use strict';

    function settingsController($q, workflowResource, notificationsService, contentTypeResource, UserGroupsResource) {
        var vm = this,
            promises = [workflowResource.getSettings(), contentTypeResource.getAll(), UserGroupsResource.getAllGroups()];

        $q.all(promises)
            .then(function (values) {
                vm.settings = values[0].data;
                vm.docTypes = values[1];
                vm.groups = values[2];

                if (vm.settings.FastTrack) {
                    vm.fastTrack = vm.settings.FastTrack.split(',');
                }
                if (vm.settings.FinalApprover) {
                    vm.finalApprover = vm.groups.filter(function (v) {
                        return v.GroupId == vm.settings.FinalApprover;
                    })[0];
                }

                vm.notFastTrack = [];
                angular.forEach(vm.docTypes, function (dt) {
                    if (vm.fastTrack.indexOf(dt.alias) === -1) {
                        vm.notFastTrack.push(dt.alias);
                    }
                });
            });


        function save() {

            vm.settings.FastTrack = vm.fastTrack.join(',');
            vm.settings.FinalApprover = vm.finalApprover.GroupId;

            workflowResource.saveSettings(vm.settings)
                .then(function (resp) {
                    if (resp.status === 200) {
                        notificationsService.success("SUCCESS!", resp.data);
                    }
                    else {
                        notificationsService.error("OH SNAP!", resp.data);
                    }
                });
        }


        // add to fasttrack, remove from notFastTrack
        function add(alias) {
            var index,
                dt = $.grep(vm.notFastTrack, function (dt, i) {
                    if (dt === alias) {
                        index = i;
                        vm.fastTrack.push(dt);
                        return true;
                    }
                    return false;
                })[0];

            vm.notFastTrack.splice(index, 1);
        };

        //
        function remove(alias) {
            var index,
                dt = $.grep(vm.fastTrack, function (dt, i) {
                    if (dt === alias) {
                        index = i;
                        vm.notFastTrack.push(alias);
                        return true;
                    }
                    return false;
                });

            vm.fastTrack.splice(index, 1);
        };

        angular.extend(vm, {
            save: save,
            add: add,
            remove: remove,

            email: '',
            fastTrack: [],
            notFastTrack: [],
            finalApprover: '',
            settings: {
                Email: '',
                FastTrack: [],
                FinalApprover: ''
            }
        });
    }

    angular.module('umbraco').controller('Workflow.Settings.Controller', settingsController);

}());