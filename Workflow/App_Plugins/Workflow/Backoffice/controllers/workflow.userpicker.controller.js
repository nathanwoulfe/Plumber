// this is almost identical to the Umbraco default, only the id property on the user object is changed to userId
(() => {
    'use strict';

    function userPickerController($scope, usersResource, localizationService) {

        this.users = [];
        this.loading = false;
        this.usersOptions = {};

        //////////

        const preSelect = (selection, users) => {
            selection.forEach(selected => {
                users.forEach(user => {
                    if (selected.userId === user.id) {
                        user.selected = true;
                    }
                });
            });
        };

        const getUsers = () => {

            this.loading = true;

            // Get users
            usersResource.getPagedResults(this.usersOptions).then(users => {

                this.users = users.items;

                this.usersOptions.pageNumber = users.pageNumber;
                this.usersOptions.pageSize = users.pageSize;
                this.usersOptions.totalItems = users.totalItems;
                this.usersOptions.totalPages = users.totalPages;

                preSelect($scope.model.selection, this.users);

                this.loading = false;

            });
        };

        const search = _.debounce(() => {
            $scope.$apply(() => {
                getUsers();
            });
        }, 500);

        const onInit = () => {

            this.loading = true;

            // set default title
            if (!$scope.model.title) {
                $scope.model.title = localizationService.localize('defaultdialogs_selectUsers');
            }

            // make sure we can push to something
            if (!$scope.model.selection) {
                $scope.model.selection = [];
            }

            // get users
            getUsers();

        };


        this.searchUsers = () => {
            search();
        };

        this.changePageNumber = pageNumber => {
            this.usersOptions.pageNumber = pageNumber;
            getUsers();
        };

        this.selectUser = user => {

            if (!user.selected) {

                user.selected = true;
                $scope.model.selection.push(user);

            } else {

                $scope.model.selection.forEach((selectedUser, index) => {
                    if (selectedUser.userId === user.id) {
                        user.selected = false;
                        $scope.model.selection.splice(index, 1);
                    }
                });

            }

        };

        onInit();

    }

    angular.module('plumber').controller('Workflow.UserPicker.Controller', ['$scope', 'usersResource', 'localizationService', userPickerController]);

})();