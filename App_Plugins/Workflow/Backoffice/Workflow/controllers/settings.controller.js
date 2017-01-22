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

                if (vm.settings.FastTrack.length) {
                    vm.fastTrack = vm.docTypes.filter(function (v) {
                        return v.alias == vm.settings.FastTrack[0];
                    })[0];
                }
                if (vm.settings.FinalApprover) {
                    vm.finalApprover = vm.groups.filter(function (v) {
                        return v.GroupId == vm.settings.FinalApprover;
                    })[0];
                }
            });
       
        function save() {

            vm.settings.FastTrack = [vm.fastTrack.alias];
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

        angular.extend(vm, {
            save: save,

            email: '',
            fastTrack: [],
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