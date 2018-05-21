(function () {
    "use strict";

    function DeleteFileController($scope, $location, treeService, notificationsService, navigationService, tourResource) {
        var vm = this;

        vm.busy = false;

        function cancel() {
            navigationService.hideDialog();
        }

        vm.cancel = cancel;

        function performDelete() {
            // stop from firing again on double-click
            if (vm.busy === false) {
                //mark it for deletion (used in the UI)
                $scope.currentNode.loading = true;
                vm.busy = true;

                tourResource.deleteTourFile($scope.currentNode.id).then(function (data) {
                    $scope.currentNode.loading = false;
                    vm.busy = false;
                    notificationsService.showNotification(data.notifications[0]);

                    treeService.removeNode($scope.currentNode);

                    navigationService.hideMenu();


                    $location.path("/settings/");
                },
                    function (err) {
                        notificationsService.showNotification(err.data.notifications[0]);
                        navigationService.hideMenu();
                    });

            }
        };

        vm.performDelete = performDelete;
    }

    angular.module("umbraco").controller("Our.Umbraco.TourEditor.DeleteFileController",
        [
            '$scope',
            '$location',
            'treeService',
            'notificationsService',
            'navigationService',
            'Our.Umbraco.TourEditor.TourResource',
            DeleteFileController
        ]);

})();