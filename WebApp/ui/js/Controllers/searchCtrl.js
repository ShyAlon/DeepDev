
angular.module('DeepDev.controllers')
.controller('searchCtrl', ['$scope', '$rootScope', '$http', '$window', '$location', '$animate', '$timeout', '$routeParams', 'server', 'local', 'authentication', 'ui',
function ($scope, $rootScope, $http, $window, $location, $animate, $timeout, $routeParams, server, local, authentication, ui) { //, $timeout, $routeParams, $location, $http, $rootScope, ngDialog, Geo, Flickr, data, Platform, apiService, localAuth, storageService) {
    /* Initialization */
    $scope.$on('$viewContentLoaded', function () {
        console.log("In Search");
        authentication.initialize(getData);
        $scope.subitem = 'Requirement';
        $rootScope.hide = false;
        $rootScope.loading = true;
    });

    var getData = function () {
        $rootScope.wizardItem = null;
        $scope.availableSearchParams = [
          { key: "name", name: "Name", placeholder: "Name..." },
          { key: "tag", name: "Tag", placeholder: "Tag..." },
          { key: "status", name: "Status", placeholder: "Status..." },
          { key: "risk", name: "Risk", placeholder: "Risk..." },
          { key: "owner", name: "Owner", placeholder: "Owner..." },
          { key: "deadline", name: "Deadline", placeholder: "Deadline after..." },
        ];
        local.initialize(null, $scope, gotData);
        // $routeProvider.when('/Edit/:entity/:parentId/:entityId', { templateUrl: 'partials/editEntity.html', controller: 'entityCtrl' });
        // server.getEntity({ id: $routeParams.entityId.replace(':', '') }, gotMilestone, failedGetMilestone, $routeParams.entity.replace(':', ''));
    }

    $scope.searchRequest = function () {
        if ($scope.hasNoKeys($scope.searchParams)) {
            console.log("No query");
            return;
        }
        var search ={ id: -1, params: $scope.searchParams };

        server.commitEntity(search, function (data, state) {
            gotData(data);
            console.log("Saved tag " + search);
        }, function (search, state) {
            console.log("Failed to set search " + search);
            console.log(search);
        }, "SearchRequest");
    }

    var gotData = function (item) {
        $rootScope.searchResults = item.Results;
    }

    var gotMethod = function (item) {
        $rootScope.data.Children.Methods.push(item);
    }
    
    var failedGetMilestone = function (failure) {
        $rootScope.loading = false;
        console.log("Failed to get projects for user in  " + $location.path());
        console.log(failure);
    }

    var configureUI = function () {
        ui.configureDatetimepicker($scope);
        ui.configureWizard($scope, ['General', 'Predecessors', 'Summary']);
    }

    var commitMilestoneSuccess = function (data) {
        console.log($rootScope.wizardItem);
        // local.goToParent();
        //$location.path("/milestoneEdit/:" + $rootScope.wizardItem.ProjectId + "/:" + $rootScope.wizardItem.Id);
    }

    $scope.hasNoKeys = function (searchParams) {
        for (k in searchParams) {
            if (Object.prototype.hasOwnProperty.call(searchParams, k)) {
                return false;
            }
        }
        return true;
    }


    /* Tasks */

    $scope.createTask = function () {
        local.saveEntityAndCreate('Task', 'Milestone', 'taskEdit');
    };

    var initializeProject = function () {
        $scope.usersTypes = [
              { id: 1, name: "ProductManager", users: [] }
            , { id: 2, name: "ProjectManager", users: [] }
            , { id: 4, name: "SystemEngineer", users: [] }
        ];

        $rootScope.projectUserEmail = $rootScope.userEmail;

        for (var i = 0; i < $rootScope.data.Users.length; i++) {
            var user = $rootScope.data.Users[i];
            if (user.UserType & 1) {
                $scope.usersTypes[0].users.push(user);
            }
            if (user.UserType & 2) {
                $scope.usersTypes[1].users.push(user);
            }
            if (user.UserType & 4) {
                $scope.usersTypes[2].users.push(user);
            }
        }

        $rootScope.userType = $scope.usersTypes[0];

        $scope.addUser = function () {
            for (var i = 0; i < $rootScope.userType.users.length; i++) {
                if ($rootScope.userType.users[i].UserMail == $rootScope.projectUserEmail) {
                    console.log($rootScope.userType.users[i].UserMail + " is already a " + $rootScope.userType.name);
                    return;
                }
            }
            $rootScope.userType.users.push({ Id: -1, UserMail: $rootScope.projectUserEmail, ProjectId: $rootScope.wizardItem.Id, UserType: $scope.userType.id, Name: "A", Description: "A" });
        }

        $scope.removeUser = function (item, userGroup) {
            //console.log(item);
            index = -1;
            for (var i = 0; i < userGroup.users.length; i++) {
                if (userGroup.users[i].UserMail === item.UserMail) {
                    index = i;
                    break;
                }
            }
            if (index > -1) {
                userGroup.users.splice(index, 1);
                //console.log("removed user from group");
            }
        };

        var deleteProjectUser = function (data) {
            console.log("deleted Project User");
            //console.log(data);
        }

        var commitProjectUser = function (data) {
            console.log("Created Project User");
            //console.log(data);
        }

        var updateProjectUser = function (data) {
            console.log("Updated Project User");
            //console.log(data);
        }

        var failUserFunction = function (data, state) {
            console.log("failed to something project user");
            //console.log(data);
            //console.log(state);
        }

        $scope.saveUsers = function () {
            var created = {};
            var modified = {};
            var deleted = {};
            var original = {};

            for (var i = 0; i < $rootScope.data.Users.length; i++) {
                original[$rootScope.data.Users[i].UserMail] = $rootScope.data.Users[i];
            }

            for (var j = 0; j < $scope.usersTypes.length; j++) {
                for (var k = 0; k < $scope.usersTypes[j].users.length; k++) {
                    if (original[$scope.usersTypes[j].users[k].UserMail] != undefined) {
                        // Modified
                        if (modified[$scope.usersTypes[j].users[k].UserMail] != undefined) {
                            modified[$scope.usersTypes[j].users[k].UserMail].UserType |= $scope.usersTypes[j].users[k].UserType;
                        } else {
                            modified[$scope.usersTypes[j].users[k].UserMail] = $scope.usersTypes[j].users[k];
                        }
                    } else {
                        // Created
                        if (created[$scope.usersTypes[j].users[k].UserMail] != undefined) {
                            created[$scope.usersTypes[j].users[k].UserMail].UserType |= $scope.usersTypes[j].users[k].UserType;
                        } else {
                            created[$scope.usersTypes[j].users[k].UserMail] = $scope.usersTypes[j].users[k];
                        }
                    }
                }
            }

            for (var i = 0; i < $rootScope.data.Users.length; i++) {
                if (!modified[$rootScope.data.Users[i].UserMail]) {
                    deleted[$rootScope.data.Users[i].UserMail] = $rootScope.data.Users[i];
                }
            }
            for (var user in created) {
                server.commitEntity(created[user], commitProjectUser, failUserFunction, 'ProjectUser');
            }
            for (var user in modified) {
                server.updateEntity(modified[user], updateProjectUser, failUserFunction, 'ProjectUser');
            }
            for (var user in deleted) {
                server.deleteEntity(deleted[user], deleteProjectUser, failUserFunction, 'ProjectUser');
            }
        }
    }

}]);