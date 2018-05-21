(function () {
    "use strict";

    function TourListController($scope, eventsService) {
        var vm = this;
        vm.tours = $scope.model.data.tours;
        vm.filename = $scope.model.data.fileName;
        vm.aliases = $scope.model.aliases;
        vm.groups = $scope.model.groups;

        vm.sortableOptions = {
            distance: 10,
            tolerance: 'move',
            scroll: true,
            zIndex: 6000
        }

        function editTour(index) {

            // create a deep clone of the tour object
            var tour = JSON.parse(JSON.stringify(vm.tours[index]));

            eventsService.emit('toureditor.edittour',
                {
                    "index": index,
                    "tour": tour,
                    "isNew": false,
                    "aliases": vm.aliases,
                    "groups" : vm.groups
                });
        }

        function removeTour(index) {
            // remove the alias
            var alias = vm.tours[index].alias;

            var aliasIndex = vm.aliases.indexOf(alias);

            if (aliasIndex > -1) {
                vm.aliases.splice(aliasIndex,1);
            }

            // remove the tour
            vm.tours.splice(index,1);
        }

        function addTour() {
            var newTour = {
                "name": "",
                "alias": "",
                "group": "",
                "groupOrder": 100,
                "allowDisable": false,
                "requiredSections": [],
                "steps": []
            };

            //vm.tours.push(newTour);

            eventsService.emit('toureditor.edittour',
                { "index": vm.tours.length, "tour": newTour, "isNew": true, "aliases": vm.aliases, "groups" : vm.groups });
        }

        vm.addTour = addTour;
        vm.removeTour = removeTour;
        vm.editTour = editTour;
    }

    angular.module("umbraco").controller("Our.Umbraco.TourEditor.TourListController",
        [
            '$scope',
            'eventsService',
            TourListController
        ]);

})();