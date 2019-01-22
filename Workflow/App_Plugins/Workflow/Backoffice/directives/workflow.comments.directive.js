(() => {

    const template = `
                <div>
                    <p ng-bind="intro"></p>
                    <label for="comments">
                        {{ labelText }} <span ng-bind="info"></span>
                    </label>
                    <textarea no-dirty-check id="comments" ng-model="comment" ng-change="limitChars()" umb-auto-focus></textarea>
                </div>`;

    function comments() {

        const directive = {
            restrict: 'AEC',
            scope: {
                intro: '=',
                labelText: '=',
                comment: '=',
                limit: '=',
                isFinalApproval: '=',
                disabled: '='
            },
            template: template,
            link: scope => {

                scope.limitChars = () => {

                    var limit = scope.limit;

                    if (scope.comment.length > limit) {
                        scope.info = `(Comment max length exceeded - limit is ${limit} characters.)`;
                        scope.comment = scope.comment.substr(0, limit);
                    } else {
                        scope.info = `(${limit - scope.comment.length} characters remaining.)`;
                    }

                    if (!scope.isFinalApproval) {
                        scope.disabled = scope.comment.length === 0;
                    }
                };
            }
        };

        return directive;
    }

    angular.module('plumber.directives').directive('wfComments', comments);

})();