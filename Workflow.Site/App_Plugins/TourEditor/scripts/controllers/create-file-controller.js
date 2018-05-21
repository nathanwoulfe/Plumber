(function () {
    "use strict";

    function CreateFileController($scope, $location, formHelper, notificationsService, navigationService, tourResource) {
        var vm = this;

        vm.filename = '';

        function createFile() {
            
            tourResource.createTourFile(vm.filename).then(function (data) {
                    navigationService.hideMenu();

                    navigationService.syncTree({ tree: "toureditor", path: "-1," + vm.filename, forceReload: true, activate: true });

                    notificationsService.showNotification(data.notifications[0]);

                    // reset the form, otherwise we can see unsaved changes warnings
                    formHelper.resetForm({ scope: $scope });

                    // navigate to edit view
                    $location.path("/settings/toureditor/edit/" + vm.filename);
                    
                },
                function (err) {
                    notificationsService.showNotification(err.data.notifications[0]);
                    navigationService.hideMenu();
                });

        };

        vm.createFile = createFile;
    }

    angular.module("umbraco").controller("Our.Umbraco.TourEditor.CreateFileController",
        [
            '$scope',
            '$location',
            'formHelper',
            'notificationsService',
            'navigationService',
            'Our.Umbraco.TourEditor.TourResource',
            CreateFileController
        ]);

})();