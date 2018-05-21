(function () {
    "use strict";

    function valUniqueAlias() {

        return {
            require: 'ngModel',
            restrict: "A",
            link: function (scope, elm, attrs, ctrl) {

                var existingAliases = [];

                if (!ctrl) return;

                var uniqueAliasValidator = function (viewValue) {


                    if (viewValue) {
                        if (_.contains(existingAliases, viewValue) === false) {
                            ctrl.$setValidity('valUniqueAlias', true);
                            ctrl.errorMsg = "";
                            return viewValue;
                        }
                    }

                    ctrl.$setValidity('valUniqueAlias', false);
                    ctrl.errorMsg = "Alias must be unique";
                    return undefined;                                       
                };

                ctrl.$formatters.push(uniqueAliasValidator);
                ctrl.$parsers.unshift(uniqueAliasValidator);

                attrs.$observe('valUniqueAlias', function (newVal) {
                    if (newVal) {
                        existingAliases = eval(newVal);
                    }

                    uniqueAliasValidator(ctrl.$viewValue);
                });
            }
        };
    }
    angular.module('umbraco.directives.validation').directive("valUniqueAlias", valUniqueAlias);

})();