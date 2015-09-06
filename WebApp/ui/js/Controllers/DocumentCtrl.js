angular.module('DeepDev.controllers')
.controller('DocumentCtrl', ['$scope', '$rootScope', '$http', '$window', '$location', '$animate', '$timeout', '$routeParams', 'server', 'local', 'authentication', 'ui',
function ($scope, $rootScope, $http, $window, $location, $animate, $timeout, $routeParams, server, local, authentication, ui) {

    /* Initialization */
    $scope.$on('$viewContentLoaded', function () {
        authentication.initialize(getData);

        $rootScope.helpTitle = 'Documents Help';
        $rootScope.hide = true;
        $rootScope.loading = true;
    });

    var getData = function () {
        local.clearData();
        $rootScope.projectId = parseInt($routeParams.projectId.replace(':', '') || '-1');
        $rootScope.documentType = $routeParams.documentId.replace(':', '');
        if ($rootScope.projectId >= 0) {
            server.getEntity({ id: $rootScope.projectId, ParentId: $rootScope.projectId, parentType: $rootScope.documentType }, gotProject, failedGetProject, 'projectForDocument');
        } else {
            console.log('Failed to get projectId from route parameters');
            console.log($routeParams);
            $location.path("/dashboard");
        }
    }

    var gotProject = function (item) {
        local.initialize(item, $scope, gotProject);
        setSize($rootScope.data, 1);

        if ($rootScope.data.Properties.Gantt) {
            $scope.tasks = { data: $rootScope.data.Properties.Gantt }; /*{
                    data: [
                      {
                          id: 1, text: "Project #2", start_date: "01-04-2013", duration: 18, order: 10,
                          progress: 0.4, open: true
                      },
                      {
                        text: "Concept", id: 11, start_date: "2015-05-19T23:00:00Z", progress: 0, duration: 936, parent: 0},
                      {
                          id: 2, text: "Task #1", start_date: "02-04-2013", duration: 8, order: 10,
                          progress: 0.6, parent: 1
                      },
                      {
                          id: 3, text: "Task #2", start_date: "11-04-2013", duration: 8, order: 20,
                          progress: 0.6, parent: 1
                      }
                    ],
                    links: [
                      { id: 1, source: 1, target: 2, type: "1" },
                      { id: 2, source: 2, target: 3, type: "0" },
                      { id: 3, source: 3, target: 4, type: "0" },
                      { id: 4, source: 2, target: 5, type: "2" },
                    ]
                };*/
                //console.log($scope.tasks);
        }

        $rootScope.documentVersion = "1.0";
        $rootScope.documentAuthor = $rootScope.userName;
        $rootScope.documentTitle = $rootScope.data.ParentType;
        $rootScope.documentTime = (new Date()).toDateString();
        for (var i = 0; i < $rootScope.data.DocumentTypes.length; i++) {
            if ($rootScope.data.DocumentTypes[i].Id == $rootScope.documentId) {
                $rootScope.documentType = $rootScope.data.DocumentTypes[i];
                break;
            }
        }

        if ($rootScope.data.Children.UseCases) {
            for (var i = 0; i < $rootScope.data.Children.UseCases.length; i++) {
                var sequence = $rootScope.data.Children.UseCases[i];
                initializeSequence(sequence);
            }
        }

        if ($rootScope.data.Children.Tasks) {
            for (var i = 0; i < $rootScope.data.Children.Tasks.length; i++) {
                var task = $rootScope.data.Children.Tasks[i];
                initializeTask(task.Entity);
                task.openIssues = false;
                for (var property in task.Children) {
                    if (task.Children.hasOwnProperty(property)) {
                        for (var j = 0; j < task.Children[property].length; j++) {
                            //setSize(task.Children[property][i], 4);
                            var issue = task.Children[property][j];
                            initializeIssue(issue.Entity);
                            setSize(issue, 5);
                            if (issue.Entity.Status == "New" ||
                                issue.Entity.Status == "Accepted" ||
                                issue.Entity.Status == "InProgress" ||
                                issue.Entity.Status == "PendingApproval") {
                                task.openIssues = true;
                            } 
                        }
                    }
                }
                var container = task.openIssues ? task.Entity.Status + "Tasks (Open Issues)" : task.Entity.Status + "Tasks";
                if (!$rootScope.data.Children[container]) {
                    $rootScope.data.Children[container] = [];
                }
                $rootScope.data.Children[container].push(task);
            }
            $rootScope.data.Children.Tasks = [];
        }

        
        $rootScope.data.hideBlock = true; // hide the project's description

        if ($rootScope.data.Children.Requirements) {
            for (var j = 0; j < $rootScope.data.Children.Requirements.length; j++) {
                var requirement = $rootScope.data.Children.Requirements[j];
                for (var k = 0; k < requirement.Children.UseCases.length; k++) {
                    var useCase = requirement.Children.UseCases[k];
                    initializeSequence(useCase);
                }
            }
        }

        var showhideTOC = function () {
            var newText = 'Content'
            document.getElementById('contentheader').innerHTML = newText;
        }

        var getElementsByTagNames = function (list, obj) {
            if (!obj) var obj = document;
            var tagNames = list.split(',');
            var resultArray = new Array();
            for (var i=0;i<tagNames.length;i++) {
                var tags = obj.getElementsByTagName(tagNames[i]);
                for (var j=0;j<tags.length;j++) {
                    resultArray.push(tags[j]);
                }
            }
            var testNode = resultArray[0];
            if (!testNode) return [];
            if (testNode.sourceIndex) {
                resultArray.sort(function (a,b) {
                    return a.sourceIndex - b.sourceIndex;
                });
            }
            else if (testNode.compareDocumentPosition) {
                resultArray.sort(function (a,b) {
                    return 3 - (a.compareDocumentPosition(b) & 6);
                });
            }
            return resultArray;
        }

        var createTOC = function () {
            var y = document.createElement('div');
            y.id = 'innertoc';
            var a = y.appendChild(document.createElement('span'));
            // a.onclick = showhideTOC;
            a.id = 'contentheader';
            a.innerHTML = '<h2 class="document">Content</h2>';
            var z = y.appendChild(document.createElement('div'));
            // z.onclick = showhideTOC;
            var toBeTOCced = getElementsByTagNames('h1,h2,h3,h4,h5');
            if (toBeTOCced.length < 2) return false;

            $scope.headers = [];

            for (var i = 6; i < toBeTOCced.length; i++) {
                if (toBeTOCced[i].offsetParent === null || toBeTOCced[i].textContent == 'Table of Content') {
                    //console.log("I hidden");
                    //console.log(toBeTOCced[i]);
                    continue;
                }
                var depth = parseInt(toBeTOCced[i].nodeName.substring(1, 2));
                var element = toBeTOCced[i];
                var line = { id: toBeTOCced[i].id, text: element.textContent, depth: depth };
                //console.log(toBeTOCced[i].childNodes[0]);
                $scope.headers.push(line);
            }
            //console.log(y);
            //document.getElementById('TOCAnchor').appendChild(y);
            return y;
        }

        var TOCstate = 'block';
        $rootScope.loading = false;
        $timeout(function () {
            createTOC();
        }, 1000);

        $timeout(function () {
            createTOC();
        }, 3000);

        $timeout(function () {
            createTOC();
        }, 9000);
        
    }

    var initializeSequence = function (sequence) {
        // console.log(sequence);
        var res = local.getSequenceStatus(sequence);
        sequence.Entity.Status = res.text;
        sequence.Entity.color = res.color;
        sequence.Entity.hideInProj = true;

        for (var m = 0; m < sequence.Methods.length; m++) {
            if (sequence.Entity.MethodComments[sequence.Methods[m].Entity.Id]) {
                sequence.Methods[m].comment = sequence.Entity.MethodComments[sequence.Methods[m].Entity.Id];
                //console.log(sequence.Methods[i]);
            }
        }

        // console.log(sequence);
        ui.initSequenceDiagram(sequence.Entity, $scope);
        // console.log("before updateSequence");
        updateSequence(sequence);
        // console.log("After updateSequence");
    }

    var initializeTask = function (task) {
        // console.log(sequence);
        var res = local.getTaskStatus(task);
        // task.Status = res.text;
        task.color = res.color;
        task.effortColor = res.effortColor;
        task.hideInProj = true;
    }

    var initializeIssue = function (issue) {
        // console.log(sequence);
        var res = local.getIssueStatus(issue);
        // task.Status = res.text;
        issue.color = res.color;
        issue.effortColor = res.effortColor;
        issue.hideInProj = true;
    }

    var setSize = function (parent, size) {
        parent.size = size;
        if (parent.Type == "Method") {
            parent.string = local.methodText(parent);
        }
        for (var property in parent.Children) {
            if (parent.Children.hasOwnProperty(property)) {
                for (var i = 0; i < parent.Children[property].length; i++) {
                    setSize(parent.Children[property][i], size + 1);
                }
            }
        }
        if (parent.Trees.Notes) {
            for (var j = 0; j < parent.Trees.Notes.length; j++) {
                setTreeSize(parent.Trees.Notes[j], size + 1);
            }
        }
        if (parent.Trees.Tasks) {
            for (var k = 0; k < parent.Trees.Tasks.length; k++) {
                setTreeSize(parent.Trees.Tasks[k], size + 1);
            }
        }
    }

    var setTreeSize = function (parent, size) {
        parent.size = size;
        parent.Entity.size = size;
        if (parent.Type == "Task") {
            initializeTask(parent.Entity);
        }
        // console.log(parent);
        for (var i = 0; i < parent.Children.length; i++) {
            setTreeSize(parent.Children[i], size + 1);
        }
    }

    var updateSequence = function (sequence) {
        local.updateSequence(sequence);
    }

    var failedGetProject = function (failure) {
        $rootScope.loading = true;
        console.log("Failed to get action for user in  " + $location.path());
        console.log(failure);
    }

    var commitUseCaseSuccess = function (data) {
        $location.path("/useCaseEdit/:" + $rootScope.milestone.Id + "/:" + $rootScope.action.UseCaseId);
    }

    var commitParameterSuccess = function (data) {
        console.log('commitParameterSuccess');
    }

    var isWhatever = function (name) {
        var result = false;
        if (name) {
            var lower = name.toLowerCase();
            if (lower.length > 8) {
                lower = lower.substring(0, 8);
            }
            //console.log(lower);
            result = lower == 'whatever';
        }
        return result;
    }


    /* API */
    $scope.showItem = function (item) {
        var result = $rootScope.project &&
            $rootScope.project[item] &&
            !isWhatever($rootScope.project[item].Name);
        return result;
    };

    

    $rootScope.getScale = function (users) {
        //console.log(users);
        var excess = users.length - 8;
        //console.log(excess);
        var result = 1;
        for (var i = 0; i < excess; i++) {
            result = result * 0.9;
        }
        // console.log(result);
        return result;
    };


    $scope.showDeployment = function () {
        var result = $scope.showItem('TargetOS') ||
            $scope.showItem('DevelopmentLanguage')
        //console.log(result);
        return result;
    };

    $scope.showGlossary = function () {
        var result = $scope.showItem('Glossaries');
        //console.log(result);
        return result;
    };

    $scope.tasksForMilestone = function (component, milestone) {
        var milestoneId = milestone.Milestone.Id;
        var result = [];
        if (milestone.Tasks) {
            for (var i = 0; i < milestone.Tasks.length; i++) {
                if (milestone.Tasks[i].Task.ComponentId == component.Component.Id) {
                    result.push(milestone.Tasks[i]);
                }
            }
        }
        
        return result;
    };

    $scope.finishItem = function () {
        console.log($rootScope.action);
        for (var i = 0; i < $rootScope.action.Parameters.length; i++) {
            server.updateEntity($rootScope.action.Parameters[i], commitParameterSuccess, null, 'parameter');
        }
        if ($rootScope.action.Id > 0) { // Update
            server.updateEntity($rootScope.action, commitActionSuccess, null, 'action');
        } else { // Create
            server.commitEntity($rootScope.action, commitActionSuccess, null, 'action');
        }
    }

    var commitActionSuccess = function (data) {
        console.log('commitActionSuccess');
    }

    /* Private methods */


    /* Event handlers */
    var gotParameter = function (item) {
        console.log(item);
        var parameter = item.Parameter;
        $rootScope.action.Parameters = $rootScope.action.Parameters || [];
        $rootScope.action.Parameters.push(parameter);

        var found = false;
        for (var i = 0; i < $rootScope.parameterNames.length && !found; i++) {
            if (parameter.Name == $rootScope.parameterNames[i]) {
                found = true;
            }
        }
        if (!found) {
            $rootScope.parameterNames.push(parameter.Name);
        }

        found = false;
        for (var i = 0; i < $rootScope.parameterTypes.length && !found; i++) {
            if (parameter.Type == $rootScope.parameterTypes[i]) {
                found = true;
            }
        }
        if (!found) {
            $rootScope.parameterTypes.push(parameter.Type);
        }
            

        console.log('Got parameter in ' + $location.path());
        console.log($rootScope.action.Parameters);
    };
}]);
