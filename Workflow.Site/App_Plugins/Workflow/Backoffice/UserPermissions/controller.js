(function () {
	'use strict';

	// create controller 
	function Controller($scope, $routeParams, UserGroupsResource, notificationsService, contentResource) {
		var vm = this,
			nodeId = $routeParams.id;

		function init() {
			UserGroupsResource.getAllGroups()
				.then(function (resp) {
					vm.groups = resp;

					var hasExplicit = false;
					angular.forEach(vm.groups, function (v, i) {
						if (!hasExplicit) {
							angular.forEach(v.Permissions, function (p) {
								if (p.NodeId == nodeId) {
								    hasExplicit = true;

								    if (p.Permission === 3) {
								        vm.elevated = true;
								    }
								}
							});
						}
					});

					if (!hasExplicit) {
						contentResource.getById(nodeId)
							.then(function (resp) {
								checkAncestorPermissions(resp.path.split(','));
							});						
					}
				});
		};
		init();

		function checkAncestorPermissions(path) {
			// first is -1, last is the current node
			path.shift();
			path.pop();
			
			angular.forEach(path, function (id, i) {
				angular.forEach(vm.groups, function (v, i) {
					angular.forEach(v.Permissions, function (p) {
						if (p.NodeId == id) {
							vm.inherited.push({
								Name: v.Name,
								GroupId: p.GroupId,
								NodeName: p.NodeName,
								Permission: p.Permission === 1 ? 'Workflow author' : 'Workflow approver'
							});
						}
					});
				});
			});			
		}

		function save() {

			var response = [];
			angular.forEach(vm.groups, function (v, i) {
			    angular.forEach(v.Permissions, function (p) {
				    if (p.NodeId == nodeId) {

				        if (vm.elevated && p.Permission === 2) {
				            p.Permission = 3;
				        } else if (!vm.elevated && p.Permission === 3) {
				            p.Permission = 2;
				        }

						response.push(p);
					}
				});
			});

			UserGroupsResource.savePermissions(response)
				.then(function (resp) {
					if (resp.status === 200) {
						notificationsService.success("SUCCESS", resp.data);
					}
					else {
						notificationsService.error("ERROR", resp.data);
					}
					init();
				});
		}

		function add(type) {
			var toAdd = type === 1 ? vm.selectedAuthorGroup : vm.selectedApproverGroup,
				group = vm.groups.filter(function (v) {
					return v.GroupId == toAdd;
				})[0];

			group.Permissions.push({
				NodeId: nodeId,
				Permission: type,
				GroupId: toAdd
			});
		}

		function remove($event, id, type) {
			$event.target.classList.add('disabled');

			angular.forEach(vm.groups, function (v, i) {
				angular.forEach(v.Permissions, function (p) {
					if (p.Permission == type && p.GroupId == id && p.NodeId == nodeId) {
						p.Permission = 0;
					}
				});
			});
		}

		function hasPermission(type, p) {
			return function (item) {
				var x = item.Permissions.find(function (v) {
					return v.Permission == type && v.NodeId == nodeId;
				});
				return x !== undefined;
			}
		}

		angular.extend(vm, {
		    inherited: [],
            elevated: false,

			hasPermission: hasPermission,
			save: save,
			add: add,
			remove: remove
		});
	};

	// register controller 
	angular.module('umbraco').controller('Workflow.UserPermissions.Controller', Controller);
}());

