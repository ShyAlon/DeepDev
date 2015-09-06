
angular.module('DeepDev.controllers')
.controller('entityCtrl', ['$scope', '$rootScope', '$http', '$window', '$location', '$animate', '$timeout', '$routeParams', 'server', 'local', 'authentication', 'ui',
function ($scope, $rootScope, $http, $window, $location, $animate, $timeout, $routeParams, server, local, authentication, ui) { //, $timeout, $routeParams, $location, $http, $rootScope, ngDialog, Geo, Flickr, data, Platform, apiService, localAuth, storageService) {
    /* Initialization */
    $scope.$on('$viewContentLoaded', function () {
        authentication.initialize(getData);
        $scope.subitem = 'Requirement';
        $rootScope.hide = false;
        $rootScope.loading = true;
    });

    var getData = function () {
        $rootScope.wizardItem = null;
        // $routeProvider.when('/Edit/:entity/:parentId/:entityId', { templateUrl: 'partials/editEntity.html', controller: 'entityCtrl' });
        server.getEntity({ id: $routeParams.entityId.replace(':', '') }, gotMilestone, failedGetMilestone, $routeParams.entity.replace(':', ''));
    }

    var gotMilestone = function (item) {
        $rootScope.loading = false;
        if ($rootScope.clearWatch) {
            for (var i = 0; i < $rootScope.clearWatch.length; i++) {
                // console.log("Clearing method watch " + i);
                $rootScope.clearWatch[i]();
            }
        }
        $rootScope.clearWatch = [];
        local.initialize(item, $scope, gotMilestone);
        $scope.showEffort = ($rootScope.wizardItem.hasOwnProperty('Effort'));
        $scope.showEffortEstimation = ($rootScope.wizardItem.hasOwnProperty('EffortEstimation'));
        $scope.showInitiator= ($rootScope.wizardItem.hasOwnProperty('Initiator'));
        $rootScope.wizardItem.requirements = [];
        if (item.Effort) {
            $rootScope.wizardItem.Effort = item.Effort;
        }
        
        configureUI();
        $scope.save = function () {
            $scope.wizardDone();
        };
        if ($rootScope.data.Type == "UseCase") {
            $rootScope.data.size = 0;
            $rootScope.data.hide = false;
            initializeSequence($rootScope.data);
        } else if ($rootScope.data.Type == "Task") {
            initializeTask();
        } else if ($rootScope.data.Type == "Project") {
            initializeProject();
        } else if ($rootScope.data.Type == "Component") {
            initializeComponent();
        }
    }

    //var gotComponentChild = function (item) {
    //    console.log(item);
    //    if (item.Type == "Method") {
    //        console.log("Component got method");
    //        $rootScope.data.Children.Methods.push(item);
    //    } else {
    //        console.log("Component got item");
    //        local.refreshTree(function () { $scope.editChild(item); });
    //    }
    //}
    
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
        // console.log($rootScope.wizardItem);
        // local.goToParent();
        //$location.path("/milestoneEdit/:" + $rootScope.wizardItem.ProjectId + "/:" + $rootScope.wizardItem.Id);
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

        $scope.exportProject = function () {
            // $window.open("/Server/api/Export?id=" + $rootScope.wizardItem.Id);



            function toBinaryString(data) {
                var ret = [];
                var len = data.length;
                var byte;
                for (var i = 0; i < len; i++) {
                    byte = (data.charCodeAt(i) & 0xFF) >>> 0;
                    ret.push(String.fromCharCode(byte));
                }

                return ret.join('');
            }

            var gotLocation = function (item) {
                console.log(item);
                $window.open("/Server/api/FileGet?fileId=" + item);
            }

            var failedGetLocation = function (failure) {
                console.log("Failed to get location for user in  " + $location.path());
                console.log(failure);
            }

            server.getEntity({ id: $rootScope.wizardItem.Id }, gotLocation, failedGetLocation, 'Export');



            //return $http({
            //    method: 'get',
            //    url: "/Server/api/Export",
            //    data: $rootScope.wizardItem,
            //    params: { id: $rootScope.wizardItem.Id },
            //    headers: { 'Content-Type': 'application/octet-stream' }
            //}).success(function (data, status, headers, config) {
            //    console.log(data);
            //    var octetStreamMime = 'application/octet-stream';
            //    var contentType = octetStreamMime;
            //    success = false;
            //    filename = 'download.gz';
            //    try {
            //        // Try using msSaveBlob if supported
            //        console.log("Trying saveBlob method ...");
            //        var blob = new Blob([data], { type: contentType });
            //        if (navigator.msSaveBlob)
            //            navigator.msSaveBlob(blob, filename);
            //        else {
            //            // Try using other saveBlob implementations, if available
            //            var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
            //            if (saveBlob === undefined) throw "Not supported";
            //            saveBlob(blob, filename);
            //        }
            //        console.log("saveBlob succeeded");
            //        success = true;
            //    } catch (ex) {
            //        console.log("saveBlob method failed with the following exception:");
            //        console.log(ex);
            //    }
            //    if (!success) {
            //        // Get the blob url creator
            //        var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
            //        if (urlCreator) {
            //            // Try to use a download link
            //            var link = document.createElement('a');
            //            if ('download' in link) {
            //                // Try to simulate a click
            //                try {
            //                    // Prepare a blob URL
            //                    console.log("Trying download link method with simulated click ...");
            //                    var blob = new Blob([data], { type: contentType });
            //                    var url = urlCreator.createObjectURL(blob);
            //                    link.setAttribute('href', url);

            //                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
            //                    link.setAttribute("download", filename);

            //                    // Simulate clicking the download link
            //                    var event = document.createEvent('MouseEvents');
            //                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
            //                    link.dispatchEvent(event);
            //                    console.log("Download link method with simulated click succeeded");
            //                    success = true;

            //                } catch (ex) {
            //                    console.log("Download link method with simulated click failed with the following exception:");
            //                    console.log(ex);
            //                }
            //            }

            //            if (!success) {
            //                // Fallback to window.location method
            //                try {
            //                    // Prepare a blob URL
            //                    // Use application/octet-stream when using window.location to force download
            //                    console.log("Trying download link method with window.location ...");
            //                    var blob = new Blob([data], { type: octetStreamMime });
            //                    var url = urlCreator.createObjectURL(blob);
            //                    window.location = url;
            //                    console.log("Download link method with window.location succeeded");
            //                    success = true;
            //                } catch (ex) {
            //                    console.log("Download link method with window.location failed with the following exception:");
            //                    console.log(ex);
            //                }
            //            }

            //        }
            //    }

            //    if (!success) {
            //        // Fallback to window.open method
            //        console.log("No methods worked for saving the arraybuffer, using last resort window.open");
            //        window.open(httpPath, '_blank', '');
            //    }
            //});
        }

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

    /* Sequences */

    var initializeSequence = function (sequence) {
        var updateComments = function () {
            if ($rootScope.wizardItem) {
                updateSequence();
                $rootScope.wizardItem.MethodComments = {};
                for (var i = 0; i < $rootScope.data.Methods.length; i++) {
                    if ($rootScope.data.Methods[i].comment) {
                        $rootScope.wizardItem.MethodComments[$rootScope.data.Methods[i].Entity.Id] = $rootScope.data.Methods[i].comment;
                    }
                }
            }        
            //console.log($rootScope.wizardItem.MethodComments);
        }
        var res = local.getSequenceStatus(sequence);
        sequence.Entity.Status = res.text;
        sequence.Entity.color = res.color;
        sequence.Entity.hideInProj = true;
        $scope.minId = -2;
        $rootScope.clearWatch.push($rootScope.$watch('wizardItem.Initiator', function () {
            updateComments();
        }));
        for (var m = 0; m < sequence.Methods.length; m++) {
            if (sequence.Entity.MethodComments[sequence.Methods[m].Entity.Id]) {
                sequence.Methods[m].comment = sequence.Entity.MethodComments[sequence.Methods[m].Entity.Id];
                //console.log(sequence.Methods[i]);
            }
            if ($rootScope.data.Methods[m].Entity.Id < $scope.minId) {
                $scope.minId = $rootScope.data.Methods[m].Entity.Id;
            }
            $rootScope.clearWatch.push($rootScope.$watch('data.Methods[' + m + '].comment', function () {
                updateComments();
            }));
        }

        $scope.getScale = function (users) {
            //console.log(users);
            var excess = (users.length)/2 - 4;
            //console.log(excess);
            var result = 1;
            for (var i = 0; i < excess; i++) {
                result = result * 0.9;
            }
            // console.log(result);
            return result;
        };

        // console.log(sequence);
        // ui.initSequenceDiagram(sequence.Entity, $scope);
        // console.log("before updateSequence");
        updateSequence(sequence);
        // console.log("After updateSequence");

        $scope.module = null;
        $scope.component = null;
        $scope.method = null;

        $scope.components = function () {
            var result = [];
            if ($rootScope.moduleName && $rootScope.data && $rootScope.data.Modules) {
                for (var i = 0; i < $rootScope.data.Modules.length; i++) {
                    if ($rootScope.moduleName == $rootScope.data.Modules[i].Entity.Name) {
                        $scope.module = $rootScope.data.Modules[i];
                        if ($rootScope.data.Modules[i].Children.Components) {
                            for (var c = 0; c < $rootScope.data.Modules[i].Children.Components.length; c++) {
                                result.push($rootScope.data.Modules[i].Children.Components[c]);
                            }
                        } else {
                            console.log("No components");
                        }

                        break;
                    }
                }
            }
            return result;
        }

        $scope.methods = function () {
            var result = [];
            if ($scope.module && $scope.module.Children.Components) {
                for (var c = 0; c < $scope.module.Children.Components.length; c++) {
                    if ($rootScope.componentName == $scope.module.Children.Components[c].Entity.Name) {
                        $scope.component = $scope.module.Children.Components[c];
                        if ($scope.module.Children.Components[c].Children.Methods) {
                            for (var m = 0; m < $scope.module.Children.Components[c].Children.Methods.length; m++) {
                                result.push($scope.module.Children.Components[c].Children.Methods[m]);
                            }
                        }
                    }
                }
            }
            //console.log(result);
            return result;
        }

        $scope.deleteChild = function (item) {
            var method = item.item;
            var index = -1;
            for (var i = 0; i < $rootScope.data.Methods.length; i++) {
                if (method.Entity.Id == $rootScope.data.Methods[i].Entity.Id) {
                    // console.log("Deleting method " + $rootScope.data.Methods[i].Entity.Id);
                    $rootScope.data.Methods.splice(i, 1);
                    $rootScope.wizardItem.MethodIds.splice(i, 1);
                    break;
                } else {
                    // console.log("Not deleting method " + $rootScope.data.Methods[i].Entity.Id);
                }
            }
            if (index > -1) {
                $rootScope.wizardItem.MethodIds.splice(index, 1);
            }
            updateSequence();
        };

        $scope.onAdd = function (item) {
            var method = item.item;
            console.log(method);
            var index = -1;
            for (var i = 0; i < $rootScope.data.Methods.length; i++) {
                if (method.Entity.Id == $rootScope.data.Methods[i].Entity.Id) {
                    console.log("Inserting return after " + $rootScope.data.Methods[i].Entity.Id);
                    $scope.addReturn(i + 1);
                    break;
                } else {
                    console.log("Not deleting method " + $rootScope.data.Methods[i].Entity.Id);
                }
            }
        };

        $scope.onRemove = function (item) {
            var method = item.item;
            // console.log(method);
            var index = -1;
            for (var i = 0; i < $rootScope.data.Methods.length; i++) {
                if (method.Entity.Id == $rootScope.data.Methods[i].Entity.Id) {
                    console.log("Inserting no return after " + $rootScope.data.Methods[i].Entity.Id);
                    $scope.addNoReturn(i + 1);
                    break;
                } else {
                    console.log("Not deleting method " + $rootScope.data.Methods[i].Entity.Id);
                }
            }
        };

        $scope.onReplace = function (item) {
            var method = item.item;
            var index = -1;
            for (var i = 0; i < $rootScope.data.Methods.length; i++) {
                if (method.Entity.Id == $rootScope.data.Methods[i].Entity.Id) {
                    // console.log("Inserting method after " + $rootScope.data.Methods[i].Entity.Id);
                    index = i;
                    break;
                } else {
                    // console.log("Not inserting method " + $rootScope.data.Methods[i].Entity.Id);
                }
            }
            $scope.insertIndex = index;
            //console.log("Method index " + $scope.insertIndex);
            //console.log($scope.insertIndex);
            $scope.addMethod();
            //console.log(method);
        };

        $scope.addMethod = function () {
            // console.log("Add Method");
            // 1. Add the module if it doesn't exist
            $scope.module = null;
            for (var i = 0; i < $rootScope.data.Modules.length; i++) {
                if ($rootScope.moduleName == $rootScope.data.Modules[i].Entity.Name) {
                    // Module exists
                    $scope.module = $rootScope.data.Modules[i];
                    break;
                }
            }
            if (!$scope.module) {
                var desciption = "new module for " + $rootScope.data.Parent.Name;
                $rootScope.module = { ParentId: $rootScope.data.ProjectId, Name: $rootScope.moduleName, Description: desciption, Id: -1 }
                server.commitEntity($rootScope.module, function (data) {
                    local.refreshTree(commitModuleSuccess(data));
                }, null, 'Module');
            }
            else {
                commitModuleSuccess($scope.module)
            }
        };

        var commitModuleSuccess = function (data) {
            //console.log("commitModuleSuccess");
            //console.log(data);
            $scope.module = data;
            $scope.component = null;
            for (var i = 0; i < $scope.module.Children.Components.length; i++) {
                if ($rootScope.componentName == $scope.module.Children.Components[i].Entity.Name) {
                    // Module exists
                    $scope.component = $scope.module.Children.Components[i];
                    break;
                }
            }
            if (!$scope.component) {
                var desciption = "new component for " + $scope.module.Entity.Name;
                $rootScope.component = { ParentId: $scope.module.Entity.Id, Name: $rootScope.componentName, Description: desciption, Id: -1 }
                server.commitEntity($rootScope.component, function (data) {
                    local.refreshTree(commitComponentSuccess(data));
                }, null, 'component');
            }
            else {
                commitComponentSuccess($scope.component)
            }
        }

        var commitComponentSuccess = function (data) {
            // console.log("commitComponentSuccess");
            // console.log(data);
            $scope.component = data;
            $scope.method = null;
            for (var i = 0; i < $scope.component.Children.Methods.length; i++) {
                if ($rootScope.methodName == $scope.component.Children.Methods[i].Entity.Name) {
                    // Module exists
                    $scope.method = $scope.component.Children.Methods[i];
                    //console.log($scope.method);
                    break;
                }
            }
            if (!$scope.method) {
                var desciption = "new method for " + $rootScope.data.Parent.Name;
                $rootScope.method = { ParentId: $scope.component.Entity.Id, Name: $rootScope.methodName, Description: desciption, Id: -1 }
                server.commitEntity($rootScope.method, commitMethodSuccess, null, 'method');
            }
            else {
                commitMethodSuccess($scope.method)
            }
        }

        var commitMethodSuccess = function (data) {
            if ($scope.insertIndex || $scope.insertIndex == 0) {
                // console.log("Push method at index " + $scope.insertIndex);
                $rootScope.data.Methods.splice($scope.insertIndex, 0, data);
                $rootScope.wizardItem.MethodIds.splice($scope.insertIndex, 0, data.Entity.Id);
            } else {
                $rootScope.data.Methods.push(data);
                $rootScope.wizardItem.MethodIds.push(data.Entity.Id);
            }
            $scope.insertIndex = null;
            $rootScope.$watch('data.Methods[' + $rootScope.data.Methods.length + '].comment', function () {
                updateComments();
            });
            updateSequence();
        }

        $scope.addReturn = function (index) {
            $scope.minId = $scope.minId % 2 == -1 ? $scope.minId - 2 : $scope.minId - 1;
            if (!index) {
                $rootScope.wizardItem.MethodIds.push($scope.minId);
                $rootScope.data.Methods.push({ Entity: { Id: $scope.minId, Name: 'Return' } });
            } else {
                $rootScope.wizardItem.MethodIds.splice(index, 0, $scope.minId);
                $rootScope.data.Methods.splice(index, 0, { Entity: { Id: $scope.minId, Name: 'Return' } });
            }
            updateSequence();
            //console.log($rootScope.wizardItem.MethodIds);
        };

        $scope.addNoReturn = function (index) {
            if (!index) {
                index = $rootScope.data.Methods.length;
            }
            $scope.minId = $scope.minId % 2 == 0 ? $scope.minId - 2 : $scope.minId - 1;
            $rootScope.wizardItem.MethodIds.push($scope.minId);
            $rootScope.data.Methods.push({ Entity: { Id: $scope.minId, Name: 'No Return' } });
            updateSequence();
        };
    }

    var updateSequence = function () {
        ui.initSequenceDiagram($rootScope.wizardItem, $scope);
        // console.log($scope.data);
        local.updateSequence($rootScope.data);
    }

    var initializeComponent = function () {
        //console.log("Component");
        $scope.save = function () {
            //console.log("Component save");
            if ($rootScope.data.Children.Methods) {
                for (var i = 0; i < $rootScope.data.Children.Methods.length; i++) {
                    var method = $rootScope.data.Children.Methods[i].Entity;
                    var success = function (item) { console.log("saved method"); };
                    var failure = function (item) { console.log("failed to save method"); };
                    if ($rootScope.wizardItem.Id > 0) { // Update
                        server.updateEntity(method, success, failure, "Method");
                    } else { // Create
                        server.commitEntity(method, success, failure, "Method");
                    }
                }
            }
            $scope.wizardDone();
        };
        $rootScope.gotChild = function (item) {
            if (item.Type == "Method") {
                $rootScope.data.Children.Methods.push(item);
            } else {
                local.refreshTree(function () { $scope.editChild(item); });
            }
        }
    }

    var initializeTask = function () {
        // console.log("IsTask");
        var counter = 0;
        $scope.executionUnits = function () {
            if (!$scope.calculatedUnits) {
                var result = [{ id: -1, name: 'Not Assigned', isMilestone: true, counter: counter++ }];
                $rootScope.unit = result[0];
                if ($rootScope.data.Milestones) {
                    for (var m = 0; m < $rootScope.data.Milestones.length; m++) {
                        var milestone = $rootScope.data.Milestones[m];

                        var item = { id: milestone.Id, name: milestone.Name, isMilestone: true, counter: counter++ };
                        if ($rootScope.wizardItem.ContainerID == milestone.Id && $rootScope.wizardItem.ContainerEntityType == "Milestone") {
                            $rootScope.unit = item;
                        }
                        result.push(item);
                        if (milestone.Children) {
                            for (var c = 0; c < milestone.Children.length; c++) {
                                var sprint = milestone.Children[c];
                                item = { id: sprint.Id, name: sprint.Name, counter: counter++ };
                                if ($rootScope.wizardItem.ContainerID == sprint.Id && $rootScope.wizardItem.ContainerEntityType == "Sprint") {
                                    $rootScope.unit = item;
                                }
                                result.push(item);
                            }
                        }
                    }
                    if (result.length > 0 && $scope.unit == null) {
                        $rootScope.unit = result[0];
                    }
                }
                $scope.calculatedUnits = result;
                // console.log(result);
            }
            // console.log($scope.calculatedUnits);
            return $scope.calculatedUnits;
        }
    }
}]);