(function () {
	'use strict';

	// create controller 
	function configController($scope, $routeParams, userGroupsResource, workflowResource, notificationsService, contentResource) {
	    var vm = this,
			nodeId = $routeParams.id;

		function init() {
			userGroupsResource.getAllGroups()
				.then(function (resp) {
					vm.groups = resp;

					contentResource.getById(nodeId)
						.then(function (resp) {
						    vm.contentTypeName = resp.contentTypeName;
						    checkNodePermissions();
						    checkAncestorPermissions(resp.path.split(','));
						});					
				});
		};
		init();

		function checkNodePermissions() {
		    angular.forEach(vm.groups, function (v, i) {
		        angular.forEach(v.permissions, function (p) {
		            if (p.nodeId == nodeId) {
		                vm.approvalPath[p.permission] = v;
		            }

		            if (p.contentTypeName === vm.contentTypeName) {
		                vm.contentTypeApprovalPath[p.permission] = v;
		            }
		        });
		    });
		}

		function checkAncestorPermissions(path) {
			// first is -1, last is the current node
			path.shift();
			path.pop();
			
			angular.forEach(path, function (id, i) {
				angular.forEach(vm.groups, function (v, i) {
					angular.forEach(v.permissions, function (p) {
						if (p.nodeId == id) {
							vm.inherited.push({
								name: v.name,
								groupId: p.groupId,
								nodeName: p.nodeName,
								permission: p.permission
							});
						}
					});
				});
			});			
		}

		function save() {

			var response = [];
			angular.forEach(vm.groups, function (v, i) {
			    angular.forEach(v.permissions, function (p) {
				    if (p.nodeId == nodeId) {
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
		    vm.selectedApprovalGroup.permissions.push({
		    	nodeId: nodeId,
		    	permission: vm.approvalPath.indexOf(vm.selectedApprovalGroup),
		    	groupId: vm.selectedApprovalGroup.groupId
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
		    contentTypeApprovalPath: [],

			save: save,
			add: add,
			remove: remove
		});
	};

	// register controller 
	angular.module('umbraco').controller('Workflow.Config.Controller', configController);
}());

