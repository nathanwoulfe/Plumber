(function () {
    "use strict";

    function StepDetailsController($scope, $window, eventsService, formHelper, umbRequestHelper) {
        var vm = this;
        vm.step = null;
        vm.stepIndex = -1;
        vm.tourIndex = -1;
        vm.form = null;
        vm.isIntro = false;
        vm.sections = [];

        vm.eventList = [
            {
                "label": "Click",
                "value": "click"
            },
            {
                "label": "Double click",
                "value": "dblclick"
            },
            {
                "label": "Mouse over",
                "value": "mouseover"
            },
            {
                "label": "Mouse out",
                "value": "mouseout"
            },
            {
                "label": "Blur",
                "value": "blur"
            },
            {
                "label": "Change",
                "value": "change"
            },
            {
                "label": "Focus",
                "value": "focus"
            }
        ];

        // config for rte
        vm.rte = {           
            view: 'rte',
            config: {
                editor: {
                    "toolbar": [
                        "code",
                        "undo",
                        "redo",
                        "cut",
                        "copy",
                        "paste",
                        "bold",
                        "italic",
                        "alignleft",
                        "aligncenter",
                        "alignright",
                        "bullist",
                        "numlist",
                        "outdent",
                        "indent",
                        "charmap"
                    ],
                    stylesheets: [],
                    dimensions: { height: 250, width: 500 }
                }
            },
            value : ''
        };        

        function resetSliderConfig() {
            vm.slider = {
                view: 'slider',
                value: '',
                config: {
                    "orientation": "horizontal",
                    "initVal1": 0.5,
                    "minVal": 0,
                    "maxVal": 1,
                    "step": 0.1,
                    "handle": "round",
                    "tooltip": "show",
                    "enableRange": false,
                    "initVal2": 0,
                    "precision": null,
                    "tooltipSplit": false,
                    "tooltipFormat": null,
                    "tooltipPosition": null,
                    "reversed": false,
                    "ticks": "0,1",
                    "ticksPositions": "0,100",
                    "ticksLabels": "Light, Dark",
                    "ticksSnapBounds": 0
                }
            };
        };

        var evts = [];

        vm.properties = {
            'Title': { 'label': 'Title', 'description': 'Enter the title for this step', 'propertyErrorMessage': 'The title is a required field' },
            'Content': { 'label': 'Text', 'description': 'Enter the text for this step' },
            'Type': { 'label': 'Is intro step', 'description': 'Check if it is a intro step' },
            'Element': { 'label': 'Element', 'description': 'Enter the selector for the element you wish to highlight', 'propertyErrorMessage' : 'The element field is required' },
            'Event': { 'label': 'Event', 'description': 'Enter the event needed to trigger the next step eg. click' },
            'EventElement': { 'label': 'Event element', 'description': 'Enter the selector for the element you wish the event to happen on' },
            'PreventClick': { 'label': 'Prevent clicking', 'description': 'Check this if you want to prevent the user clicking the highlighted element' },
            'BackDropOpacity': { 'label': 'Backdrop opacity', 'description': 'The backdrop opacity', 'propertyErrorMessage': 'The backdrop opacity is a required field' },
            'View': { 'label': 'Custom view', 'description': 'Enter the path to a custom view for this step' },
            'CustomProperties': { 'label': 'Custom properties', 'description': 'If you use a custom view, you can pass in custom properties as JSON object', 'propertyErrorMessage': 'Custom properties is not valid JSON' }
        };

        function openStepPicker(isElement) {
            
            vm.elementPicker = {
                title: 'Element picker',
                subtitle : 'You can select a element from a predefined list. Only the sections, and the trees and dashboards from those sections, configured in this tour are available',
                view: umbRequestHelper.convertVirtualToAbsolutePath("~/App_Plugins/TourEditor/backoffice/toureditor/overlays/element-picker.html"),
                closeButtonLabel: 'Cancel',
                hideSubmitButton : true,
                show: true,
                sections : vm.sections,
                submit: function (model) {
                    if (isElement) {
                        vm.step.element = model;
                    } else {
                        vm.step.eventElement = model;
                    }
                   
                    vm.elementPicker.show = false;
                    vm.elementPicker = null;
                },
                close: function (oldModel) {

                    vm.elementPicker.show = false;
                    vm.elementPicker = null;
                }
            };
        }

        vm.openStepPicker = openStepPicker;

        evts.push(eventsService.on("toureditor.editstep", function (name, arg) {

            resetSliderConfig();

            vm.stepIndex = arg.stepIndex;
            vm.tourIndex = arg.tourIndex;
            vm.step = arg.step;
            vm.sections = arg.sections;

            // convert custom properties json object to string for editing
            if (vm.step.customProperties) {
                vm.step.customPropertiesText = JSON.stringify(vm.step.customProperties);
            }

            // set content of rte
            vm.rte.value = vm.step.content;

            // set value for the slider
            vm.slider.value = vm.step.backdropOpacity;

            // set event value so we have one selected
            if (vm.step.event === null || vm.step.event === '') {
                vm.step.event = 'click';
            }

            vm.isIntro = vm.step.type === 'intro';

            // scroll the step details to the top when starting editing..otherwise our tour won't work
            var containerElement = angular.element('[data-element="editor-container"]');

            if (containerElement && containerElement[0]) {                
                containerElement[0].scrollTop = 0;
            }
        }));

        evts.push(eventsService.on("toureditor.discardstepchanges", function (name, arg) {
            vm.stepIndex = -1;
            vm.tourIndex = -1;
            vm.step = null;
            vm.sections = [];

            vm.isIntro = false;

            eventsService.emit('toureditor.stepchangesdiscarded');
        }));

        evts.push(eventsService.on("toureditor.updatestepchanges", function (name, arg) {
            if (formHelper.submitForm({ scope: $scope, formCtrl: vm.form })) {

                if (vm.step.customPropertiesText && vm.step.customPropertiesText != '') {
                    // convert step to json object, otherwise it will not be saved
                    vm.step.customProperties = JSON.parse(vm.step.customPropertiesText);
                }

                // store the value from the rte with the step
                vm.step.content = vm.rte.value;

                // store the value from the slider with the step
                vm.step.backdropOpacity = vm.slider.value;

                // set the even value to empty if it's a intro or prevent click is checked
                if (vm.step.type === 'intro' || vm.step.elementPreventClick) {
                    vm.step.event = null;
                }

                eventsService.emit('toureditor.stepchangesupdate',
                    {
                        "stepIndex": vm.stepIndex,
                        "tourIndex" : vm.tourIndex,
                        "step": vm.step
                    });
                vm.stepIndex = -1;
                vm.tourIndex = -1;
                vm.step = null;
                vm.isIntro = false;
                vm.form = null;
                vm.sections = [];
            }
        }));

        
        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

        $scope.$watch('vm.isIntro', function () {

            if (vm.step) {
                if (vm.isIntro) {
                    vm.step.type = 'intro';
                } else {
                    vm.step.type = null;
                }
            }
            
        });

    }

    angular.module("umbraco").controller("Our.Umbraco.TourEditor.StepDetailsController",
        [
            '$scope',
            '$window',
            'eventsService',
            'formHelper',
            'umbRequestHelper',
            StepDetailsController
        ]);

})();