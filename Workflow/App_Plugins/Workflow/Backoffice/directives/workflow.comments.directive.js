(function () {
    'use strict';    

    function CommentsDirective() {

        var directive = {
            restrict: 'AEC',
            scope: {
                intro: '=',
                labelText: '=',
                comment: '=',
                limit: '=',
                isFinalApproval: '=',
                disabled: '='
            },
            template: '<p ng-bind="intro"></p><label for="comments">{{labelText}} <span ng-bind="info"></span><textarea name="comments" ng-model="comment" ng-change="limitChars()" no-dirty-check></textarea>',
            link: function (scope) {

                scope.limitChars = function () {

                    var limit = scope.limit;

                    if (scope.comment.length > limit) {
                        scope.info = '(Comment max length exceeded - limit is ' + limit + ' characters.)';
                        scope.comment = scope.comment.substr(0, limit);
                    } else {
                        scope.info = '(' + (limit - scope.comment.length) + ' characters remaining.)';
                    }

                    if (!scope.isFinalApproval) {
                        scope.disabled = scope.comment.length === 0;
                    }
                };
            }
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('wfComments', CommentsDirective);

}());