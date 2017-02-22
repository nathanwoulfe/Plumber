(function () {
    'use strict';

    // create controller 
    function AddController($scope, userGroupsResource, navigationService, notificationsService) {

        $scope.add = function (name) {
            userGroupsResource.addGroup(name)
                .then(function (resp) {
                    console.log(resp);
                    if (resp.status === 200) {
                        notificationsService.success('SUCCESS', 'Successfully created new user group \'' + name + '\'.');
                        window.location = '/umbraco/#/users/usergroups/edit/' + resp.data;
                    } else {
                        notificationsService.error('ERROR', resp.data);
                    }

                    navigationService.hideNavigation();
                });
        };

        $scope.cancelAdd = function () {
            navigationService.hideNavigation();
        };


    };

    // register controller 
    angular.module('umbraco').controller('Workflow.UserGroups.Add.Controller', AddController);
}());

