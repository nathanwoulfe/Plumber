(function () {
	'use strict';

	// create controller 
	function Controller($scope, $routeParams, userGroupsResource, workflowResource, notificationsService, contentResource) {
		var vm = this,
			nodeId = $routeParams.id;

		function init() {
			userGroupsResource.getAllGroups()
				.then(function (resp) {
					vm.groups = resp.data;

					var hasExplicit = false;
					angular.forEach(vm.groups, function (v, i) {
					    angular.forEach(v.Permissions, function (p) {
						    if (p.NodeId == nodeId) {
						        vm.approvalPath[p.Permission] = v;
								hasExplicit = true;
							}
						});						
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
								Permission: p.Permission
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
						response.push(p);
					}
				});
			});

			workflowResource.saveConfig(response)
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

		function add() {
		    vm.approvalPath.push(vm.selectedApprovalGroup);
		    vm.selectedApprovalGroup.Permissions.push({
		    	NodeId: nodeId,
		    	Permission: vm.approvalPath.indexOf(vm.selectedApprovalGroup),
		    	GroupId: vm.selectedApprovalGroup.GroupId
		    });		
		}

		function remove($event, index) {
		    $event.stopPropagation();
			$event.target.classList.add('disabled');
			vm.approvalPath.splice(index, 1);
		}

		angular.extend(vm, {
		    inherited: [],
            approvalPath: [],

			save: save,
			add: add,
			remove: remove
		});
	};

	// register controller 
	angular.module('umbraco').controller('Workflow.Config.Controller', Controller);
}());

