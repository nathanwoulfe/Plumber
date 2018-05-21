
function valIsJson() {

    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, elm, attrs, ctrl) {

            if (!ctrl) return;

            function isJson(fieldValue) {

                var isValid = true;


                if (fieldValue === null || fieldValue === '') {
                    // don't need to validate if no custom view is set
                    isValid = true;
                } else {
                    // check if it's valid json
                    try {
                        var o = JSON.parse(fieldValue);

                        // Handle non-exception-throwing cases:
                        // Neither JSON.parse(false) or JSON.parse(1234) throw errors, hence the type-checking,
                        // but... JSON.parse(null) returns null, and typeof null === "object", 
                        // so we must check for that, too. Thankfully, null is falsey, so this suffices:
                        if (o && typeof o === "object") {
                            isValid = true;
                        } else {
                            isValid = false;
                        }
                    } catch (e) {
                        isValid = false;
                    }
                }


                return isValid;
            }

            var jsonValidator = function (viewValue) {

                //NOTE: we don't validate on empty values, use required validator for that
                if (!viewValue || isJson(viewValue)) {
                    // it is valid
                    ctrl.$setValidity('valIsJson', true);
                    //assign a message to the validator
                    ctrl.errorMsg = "";
                    return viewValue;
                }
                else {
                    // it is invalid, return undefined (no model update)
                    ctrl.$setValidity('valIsJson', false);
                    //assign a message to the validator
                    ctrl.errorMsg = "Custom properties is not valid JSON";
                    return undefined;
                }

            };

            ctrl.$formatters.push(jsonValidator);
            ctrl.$parsers.unshift(jsonValidator);

            attrs.$observe('valIsJson', function () {
                jsonValidator(ctrl.$viewValue);
            });          
        }
    };
}
angular.module('umbraco.directives.validation').directive("valIsJson", valIsJson);