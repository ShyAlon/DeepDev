
angular.module('DeepDev.services')
.service('local', ['$http', '$q', '$rootScope', '$location', '$window', 'server', '$translate', '$timeout', '$document',
    function ($http, $q, $rootScope, $location, $window, server, $translate, $timeout, $document) {

        var localPersist = function () {
            return $window.localStorage;
        }

        var setUser = function () {
            // console.log("setting user");
            $rootScope.tokenId = localPersist().getItem('token:Id');
            if ($rootScope.tokenId) {
                $rootScope.tokenText = localPersist().getItem('token:Text');
                $rootScope.tokenOrganization = localPersist().getItem('token:Organization');
                $rootScope.tokenCreation = localPersist().getItem('token:Creation');
                $rootScope.tokenExpiration = localPersist().getItem('token:Expiration');
                $rootScope.userId = localPersist().getItem('token:UserId');
                $rootScope.userName = localPersist().getItem('token:UserName');
                $rootScope.userEmail = localPersist().getItem('token:UserEmail');
            } else {
                console.log("failed setting user - no token id");
            }
            $rootScope.loginTime = new Date();
        }

        var fixUser = function (user) {
            for (var key in user) {
                var value = user[key];
                localPersist().setItem("token:" + key, value);
                // console.log("Got key:" + "token:" + key + " value:" + value);
            }
        }

        var clearUser = function () {
            console.log("clearing user");
            $rootScope.tokenText = null; localPersist().removeItem('token:Text');
            $rootScope.tokenCreation = null; localPersist().removeItem('token:Creation');
            $rootScope.tokenOrganization = null; localPersist().removeItem('token:Organization');
            $rootScope.tokenExpiration = null; localPersist().removeItem('token:Expiration');
            $rootScope.userId = null; localPersist().removeItem('token:UserId');
            $rootScope.userName = null; localPersist().removeItem('token:UserName');
            $rootScope.userEmail = null; localPersist().removeItem('token:UserEmail');
            $rootScope.tree = false;
            $rootScope.wizardItem = false;
            $rootScope.data = false;
            $rootScope.loginTime = false;
        }

        var tokenValid = function () {
            if (!$rootScope.tokenId) {
                setUser();
            }
            if ($rootScope.tokenId) {
                var now = new Date();
                //console.log("token expiration " + $rootScope.tokenExpiration);
                var expiration = new Date($rootScope.tokenExpiration);
                var expired = expiration < now;
                //console.log("token expiration " + expiration);
                //console.log("now " + now);
                //console.log("token expired " + expired);
                return !expired;
            } else {
                console.log("failed validating token - no token id");
            }
            return false;
        }

        var getDummy = function (type) {
            return {
                "Name": "Dummy " + type,
                "Description": "Dummey " + type + " description",
                "Id": -1
            }
        }

        var gotCollection = function (data, state) {
            $rootScope[state] = data;
        }

        var gotCollections = function (data, state) {
            $rootScope.collections = data;
        }

        var getCollectionFailed = function (data) {
            console.log("Failed to get collection ");
            console.log(data);
        }



        var editChildLocalImpl = function (item) {
            // console.log(item);
            var location = '';
            if (item.Entity.ParentId > -1) {
                if (item.Entity.Id < 0) {
                    location = "/Edit/:" + item.Type + "/:" + item.Entity.ParentId;
                } else {
                    location = "/Edit/:" + item.Type + "/:" + item.Entity.ParentId + "/:" + item.Entity.Id;
                }
            } else { // Root item like a project
                location = "/Edit/:" + item.Type + "/:" + item.Entity.ParentId + "/:" + item.Entity.Id;
            }
            //console.log(location);
            $location.path(location);
        }

        var createSubItemImpl = function (childType, skipChild) {
            // First save to be sure you don't lose data
            wizardDoneImpl();
            server.getXForY(childType.substr(0, childType.length - 1), $rootScope.data.Type, { id: $rootScope.wizardItem.Id }, function (item, state) {
                if ($rootScope.gotChild) {
                    // console.log("Got child");
                    $rootScope.gotChild(item);
                } else {
                    // console.log("Didn't get child");
                    refreshTreeImpl(function () { editChildLocalImpl(item); });
                }
            }, function (date) {
                console.log('Failed to get ' + childType.substr(0, childType.length - 1));
            });
        }

        var updateTree = function () {
            if ($rootScope.data) {
                var node = getEntityNode($rootScope.data);
                if (node) {
                    //console.log(node.name);
                    if ($rootScope.wizardItem) {
                        node.name = $rootScope.wizardItem.Name;
                        node.index = indexToString($rootScope.wizardItem.Index);
                    } else {
                        node.name = $rootScope.data.Entity.Name;
                    }
                    //console.log(node.name);
                    expandNode(node);
                }
                else {
                    //console.log("Shit");
                }
            }
        }

        var getTaskStatusImpl = function (task) {
            var result;
            if (task.Status == "Complete" || task.RiskStatus == "Resolved") {
                result = { text: 'Complete', shy: 'label-success', color: 'green' };
            } else if (task.RiskType == "None") {
                result = { text: task.Status, shy: 'label-success', color: 'green' };
            } else if (task.RiskType == "High") {
                // console.log("high risk");
                result = { text: task.Status, shy: 'label-danger', color: 'red' };
            } else {
                result = { text: task.Status, shy: 'label-warning', color: 'orange' };
            }

            if (task.Effort > task.EffortEstimation) {
                result.effortColor = 'red';
            } else if (task.Effort > task.EffortEstimation * 0.7) {
                result.effortColor = 'orange';
            } else {
                result.effortColor = 'green';
            }

            // console.log(result);
            return result
        }

        var wizardDoneImpl = function (success) {
            // console.log($rootScope.wizardItem);
            if ($rootScope.wizardItem.Id > 0) { // Update
                server.updateEntity($rootScope.wizardItem, function (data, state) {
                    $rootScope.wizardItem = data;
                    $rootScope.wizardItem.IndexString = indexToString($rootScope.wizardItem.Index);
                    if ($rootScope.unit) {
                        $rootScope.wizardItem.ContainerID = $rootScope.unit.id;
                        $rootScope.wizardItem.ContainerEntityType = $rootScope.unit.isMilestone ? 'Milestone' : 'Sprint';
                    }
                    updateTree();
                    //refreshTreeImpl(function () {
                    //    if (success) {
                    //        success(data, state);
                    //    }
                    //    updateTree();
                    //});
                }, null, $rootScope.data.Type);
            } else { // Create
                server.commitEntity($rootScope.wizardItem, function (data, state) {
                    $rootScope.canEdit = data.CanEdit;
                    $rootScope.wizardItem = data.Entity;
                    $rootScope.parent = data.Parent;
                    $rootScope.data = data;
                    refreshTreeImpl(function () {
                        if (success) {
                            success(data, state);
                        }
                        updateTree();
                    });
                }, null, $rootScope.data.Type);
            }
        }

        var duplicateImpl = function (success) {
            console.log($rootScope.wizardItem);
            if (!success) {
                success = function (data) {
                    $rootScope.tree = null;
                    $location.path("/Edit/:" + data.Type.toLowerCase() + "/:" + data.Entity.ParentId + "/:" + data.Entity.Id);
                }
            }
            var entity = { id: -1, ParentId: $rootScope.wizardItem.Id };
            //if ($rootScope.data.Type == "Note") {
            //    entity.parentType = $rootScope.wizardItem.ParentType
            //}
            server.getEntity(entity, success, null, $rootScope.data.Type);
        }


        var finalStatus = function (status) {
            // "New"1: "Accepted"2: "Rejected"3: "InProgress"4: "WontFix"5: "PendingApproval"6: "Complete"
            var final = status == "Complete";
            //console.log(status);
            //console.log(final);
            return final;
        }

        var terminalStatus = function (status) {
            // "New"1: "Accepted"2: "Rejected"3: "InProgress"4: "WontFix"5: "PendingApproval"6: "Complete"
            var terminal = status == "Rejected" || status == "WontFix";
            //console.log(terminal);
            return terminal;
        }

        var getChildStatusImpl = function (item, collectionName, childName, requiredItems) {
            var child = item.item;
            var result = { text: 'Error', shy: 'label-important' };
            var missingItem = null;
            if (requiredItems) {
                for (var i = 0; i < requiredItems.length; i++) {
                    if (!child[requiredItems[i]]) {
                        missingItem = requiredItems[i];
                        break;
                    }
                }
            }
            // console.log(child);
            if (child.Entity.Id < 0) {
                result = { text: 'Not saved', shy: 'label-important' };
            } else if (missingItem) {
                result = { text: 'Missing ' + missingItem, shy: 'label-important' };
            } else if (collectionName && (!child[collectionName] || child[collectionName].length == 0)) {
                result = { text: 'No ' + childName + 's', shy: 'label-important' };
            } else if (child.Entity.Status && finalStatus(child.Entity.Status.toString())) {
                result = { text: child.Entity.Status, shy: 'label-success', color: 'green' };
            } else if (child.Entity.Status && terminalStatus(child.Entity.Status.toString())) {
                result = { text: child.Entity.Status, shy: 'label-important' };
            } else if (child.Entity.Status) {
                result = { text: child.Entity.Status, shy: 'label-warning' };
            } else {
                result = { text: 'In process', shy: 'label-warning' };
            }
            return result;
        }

        var deleteChildImpl = function (item) {
            var entity = item.item;
            //console.log(entity);
            //console.log(item);
            server.deleteEntity(entity.Entity, function (data) {
                if (entity.Entity.ParentId > 0) {
                    server.getEntity({ id: entity.Entity.ParentId }, deleteChildCallback, null, entity.parentType);
                } else { //project
                    $location.path("/");
                }
            }, function (data) {
                //console.log("failed to delete " + entity.Type);
            }, entity.Type);
        }

        var deleteChildCallback = function (data, state) {
            $rootScope.refreshItem(data, state);
            refreshTreeImpl();
        }

        var getCollectionsImpl = function () {
            server.getEntity({}, gotStaticCollections, null, 'collections');
        }

        var gotStaticCollections = function (item) {
            for (var property in item) {
                if (item.hasOwnProperty(property)) {
                    $rootScope[property] = item[property];
                }
            }
        }

        var expandItemNode = function () {
            var node = getEntityNode($rootScope.data);
            if (node) {
                expandNode(node);
            }
        }


        var refreshTreeImpl = function (callback) {
            server.getEntity({}, function (item) {
                gotTopLevel(item);
                if (callback) {
                    callback();
                }
                expandItemNode();

            }, failedGetTopLevel, 'topLevel');
        }

        var gotTopLevel = function (item) {
            //console.log('gotTopLevel');
            var tree = createProjectNodes(item);
            $rootScope.tree = tree;
            //console.log($rootScope.tree);
        }

        var createProjectNodes = function (source) {
            var tree = [];
            for (var i = 0; i < source.length; i++) {
                var node = { name: source[i].Entity.Item.Name, collapsed: false, id: source[i].Entity.Item.Id, type: 'Project', isProject: true };
                node.nodes = createTypeNodes(source[i].Entity.Children, node)
                node.target = '/Edit/:project/:' + source[i].Entity.Item.ParentId + '/:' + source[i].Entity.Item.Id;
                node.hasChildren = node.nodes && node.nodes.length > 0;
                tree.push(node);
            }
            return tree;
        }

        var createTypeNodes = function (source, parent) {
            var typeNodes = [];
            for (var property in source) {
                if (source.hasOwnProperty(property)) {
                    if (property.toLowerCase().indexOf('method') == -1) {
                        var node = { name: property, collapsed: true, parent: parent, id: -1 };
                        node.nodes = createChildrenNodes(source[property], property, node);
                        node.hasChildren = node.nodes && node.nodes.length > 0;
                        typeNodes.push(node);
                    }
                }
            }
            return typeNodes;
        }

        var expandNode = function (node) {
            // console.log("Expanding " + node.name);
            node.collapsed = false;
            if (node.parent) {
                expandNode(node.parent);
            } else { // top level
                if ($rootScope.tree) {
                    //console.log(node);
                    for (var i = 0; i < $rootScope.tree.length; i++) {
                        if (node.id != $rootScope.tree[i].id) {
                            //console.log($rootScope.tree[i]);
                            $rootScope.tree[i].collapsed = true;
                        }
                    }
                }
            }
        }

        var getEntityNode = function (data, root) {
            if (!root) {
                //console.log(data);
                for (var i = 0; i < $rootScope.tree.length; i++) {
                    var node = getEntityNode(data, $rootScope.tree[i]);
                    // console.log($rootScope.tree[i]);
                    if (node) {
                        return node;
                    }
                }
                return;
            }
            if (data.Entity.Id == root.id && root.type == data.Type) {
                //console.log(root);
                return root;
            }
            if (root.nodes) {
                for (var i = 0; i < root.nodes.length; i++) {
                    var node = getEntityNode(data, root.nodes[i]);
                    if (node) {
                        return node;
                    }
                }
            }

            //  { name: source[i].Entity.Item.Name, nodes: createTypeNodes(source[i].Entity.Children), collapsed: false, expand: expandNode, id: source[i].Entity.Item.Id, type: source[i].Type };
        }

        var indexToString = function (index) {
            //console.log(index);
            if (!index || index.length == 0) {
                return '';
            }
            var result = index[0];
            for (var i = 1; i < index.length; i++) {
                result = result + '.';
                result = result + index[i];
            }
            return result;
        }


        var createChildrenNodes = function (source, type, parent) {
            var tree = [];
            var expanded;
            type = type.substring(0, type.length - 1);
            if (type.indexOf("Sub") == 0) {
                type = type.substring(4, type.length);
            }
            if (!parent) {
                console.log(source);
            }
            for (var i = 0; i < source.length; i++) {
                var node = {
                    name: source[i].Item.Name, collapsed: true, parent: parent, id: source[i].Item.Id, type: type.substring(0, type.length),
                    target: '/Edit/:' + type + '/:' + source[i].Item.ParentId + '/:' + source[i].Item.Id, index: indexToString(source[i].Item.Index)
                };
                //console.log(node);
                node.nodes = createTypeNodes(source[i].Children, node);
                node.hasChildren = node.nodes && node.nodes.length > 0;
                //console.log(node);
                if ($rootScope.wizardItem) {
                    if (source[i].Item.Id == $rootScope.wizardItem.Id &&
                                                source[i].Item.Name == $rootScope.wizardItem.Name &&
                                                source[i].Item.ParentId == $rootScope.wizardItem.ParentId) {
                        // console.log("Found " + source[i].Item.Name);
                        expanded = node;
                    } else {
                        //console.log(node.name + " is not " + $rootScope.wizardItem.Name);
                    }
                } else {
                    //console.log("No wizard item");
                }
                tree.push(node);
            }
            if (expanded) {
                expandNode(expanded);
            }

            return tree;
        }


        var failedGetTopLevel = function (failure) {
            console.log("Failed to get tree in  " + $location.path());
            console.log(failure);
        }


        var editItemImpl = function (container) {
            if ($rootScope.wizardItem.ParentId > -1) {
                // console.log("/" +$rootScope.data.Type.toLowerCase() + "Wizard/:" + $rootScope.wizardItem.ParentId + "/:" +$rootScope.wizardItem.Id);
                $location.path("/" + $rootScope.data.Type.toLowerCase() + "Wizard/:" + $rootScope.wizardItem.ParentId + "/:" + $rootScope.wizardItem.Id);
            } else {
                // console.log("/" + $rootScope.data.Type.toLowerCase() + "Wizard/:" +$rootScope.wizardItem.Id);
                $location.path("/" + $rootScope.data.Type.toLowerCase() + "Wizard/:" + $rootScope.wizardItem.Id);
            }
        }

        var getCommonItem = function (type) {
            wizardDoneImpl();
            var parent = $rootScope.wizardItem;
            server.getXForY(type, 'Item', { id: -1, parentId: $rootScope.wizardItem.Id, parentType: $rootScope.data.Type }, function (data, state) {
                $location.path("/Edit/:" + type + "/:" + data.Parent.Id + "/:" + data.Entity.Id);
            }, function (data) {
                console.log('Failed to get new ' + type);
                console.log(data);
            });
        }

        var addCollectionItemImpl = function (system, systemCollection, newItem, availableCollection) {
            if ($rootScope[newItem] == null) {
                console.log("no item to add");
                return;
            } else {
                console.log($rootScope[newItem]);
            }
            system[systemCollection].push($rootScope[newItem].Entity.Id);
            var index = -1;// $rootScope[availableCollection].indexOf($rootScope[newItem]);
            for (var i = 0; i < $rootScope[availableCollection].length; i++) {
                if ($rootScope[availableCollection][i].Entity.Id == $rootScope[newItem].Entity.Id) {
                    index = i;
                    break;
                }
            }
            // console.log("Item exists at " + index);
            if (index > -1) {
                $rootScope[availableCollection].splice(index, 1);
                if ($rootScope[availableCollection].length > 0) {
                    $rootScope[newItem] = $rootScope[availableCollection][0];
                } else {
                    $rootScope[newItem] = null;
                }
            } else {
                console.log("failed to remove method");
            }
            //console.log(system);
        }

        var connectTargetItem = function (project, rootCollection, projectItem, isArray) {
            if ($rootScope.collections[rootCollection]) {
                //console.log($rootScope.collections[rootCollection]);
                //console.log(project[projectItem]);
                if (project[projectItem]) {
                    if (true) {//isArray) {
                        for (var i = 0; i < $rootScope.collections[rootCollection].length; i++) {
                            for (var j = 0; j < project[projectItem].length; j++) {
                                console.log(project[projectItem][j]);
                                console.log($rootScope.collections[rootCollection][i]);
                                if ($rootScope.collections[rootCollection][i].Id == project[projectItem][j].Id) {
                                    console.log('Assign ' + $rootScope.collections[rootCollection][i].Name + ' to ' + project[projectItem][j].Name);
                                    project[projectItem][j] = $rootScope.collections[rootCollection][i];
                                    break;
                                }
                            }
                        }
                    } else {
                        console.log(project[projectCollection][projectItem]);
                        for (var i = 0; i < $rootScope.collections[rootCollection].length; i++) {
                            if ($rootScope.collections[rootCollection][i].Id == project[projectItem].Id) {
                                console.log('Assign ' + $rootScope.collections[rootCollection][i].Name + ' to ' + project[projectCollection][projectItem].Name);
                                project[projectItem] = $rootScope.collections[rootCollection][i];
                                break;
                            }
                        }
                    }
                } else {
                    // console.log(projectItem + " is undefined!");
                }
            } else {
                console.log("Failed to find " + rootCollection);
            }
        }

        var gotFrameworks = function (collection) {
            $rootScope.availableFrameworks = [];
            for (var i = 0; i < collection.length; i++) {
                var included = false;
                for (var j = 0; j < $rootScope.project.DevelopmentInfrastructureIds.length; j++) {
                    included |= $rootScope.project.DevelopmentInfrastructureIds[j] == collection[i].Id;
                }
                if (included) {
                    console.log('Not adding ' + collection[i].Name);
                } else {
                    //console.log("Adding " + collection[i].Name + " method to available colleciton");
                    $rootScope.availableFrameworks.push(collection[i]);
                }
            }
        }

        var saveEntityImpl = function (success, failure, entity) {
            // console.log($rootScope.wizardItem);
            if ($rootScope.wizardItem.Id > 0) { // Update
                server.updateEntity($rootScope.wizardItem, success, failure, entity);
            } else { // Create
                server.commitEntity($rootScope.wizardItem, success, failure, entity);
            }
        }

        var getMethodText = function (method) {
            str = method.Entity.Name.replace(/ /g, '');
            str += "(" + (method.Entity.Parameters || "") + ")";
            if (method.Entity.CallMode != "InProcess") {
                str += ";;" + method.Entity.CallMode;
            }
            str = (method.Entity.Return || "void") + " " + str;

            return str;
        }

        var getUserString = function (method) {
            // console.log(method);
            return method.Module + ";;" + method.Parent.Name;
        }

        var initializeTour = function (scope) {
            // Well, this needs to be generated dynamically

            var steps = [];
            if (document.querySelector('#leftbar-wrapper')) {
                steps.push(
                {
                    element: '#leftbar-wrapper',
                    intro: $translate.instant('navigationBlock'),
                    position: 'auto'
                });
            };
            if (document.querySelector('#rightbar-audit')) {
                steps.push(
                {
                    element: '#rightbar-audit',
                    intro: $translate.instant('auditTrail'),
                    position: 'auto'
                });
            }
            if (document.querySelector('#rightbar-session')) {
                steps.push(
                {
                    element: '#rightbar-session',
                    intro: $translate.instant('sessionTrail'),
                    position: 'auto'
                });
            }
            if (document.querySelector('#userEMail')) {
                steps.push(
                {
                    element: '#userEMail',
                    intro: $translate.instant('userEMail'),
                    position: 'auto'
                });
            };
            if (document.querySelector('#userToken')) {
                steps.push(
                {
                    element: '#userToken',
                    intro: $translate.instant('userToken'),
                    position: 'auto'
                });
            }
            if (document.querySelector('#userPassword')) {
                steps.push(
                {
                    element: '#userPassword',
                    intro: $translate.instant('userPassword'),
                    position: 'auto'
                });
            } if (document.querySelector('#userOrganization')) {
                steps.push(
                {
                    element: '#userOrganization',
                    intro: $translate.instant('userOrganization'),
                    position: 'auto'
                });
            }
            if (document.querySelector('#userName')) {
                steps.push(
                {
                    element: '#userName',
                    intro: $translate.instant('userName'),
                    position: 'auto'
                });
            }


            addCreateChildrenSteps(steps);
            if ($rootScope.data) {
                if ($rootScope.data.Type != "Task") {
                    steps.push(
                    {
                        element: '#taskButton',
                        intro: $translate.instant('taskButton', { data: $rootScope.data }),
                        position: 'auto'
                    });
                }
                if ($rootScope.data.Type != "Note") {
                    steps.push(
                    {
                        element: '#noteButton',
                        intro: $translate.instant('noteButton', { data: $rootScope.data }),
                        position: 'auto'
                    });
                }
                steps.push(
                    {
                        element: '#saveButton',
                        intro: $translate.instant('saveButton', { data: $rootScope.data }),
                        position: 'auto'
                    }, {
                        element: '#duplicateButton',
                        intro: $translate.instant('duplicateButton', { data: $rootScope.data }),
                        position: 'auto'
                    }, {
                        element: '#deleteButton',
                        intro: $translate.instant('deleteButton', { data: $rootScope.data }),
                        position: 'auto'
                    }
                );
            }


            addEntitySteps(steps);
            addAuxSteps(steps);
            addMethodMarkers(steps);
            // console.log(steps);
            scope.ngIntroOptions = {
                steps: steps,
                showStepNumbers: false,
                exitOnOverlayClick: true,
                exitOnEsc: true,
                nextLabel: '<strong>NEXT!</strong>',
                prevLabel: '<span style="color:green">Previous</span>',
                skipLabel: 'Exit',
                doneLabel: 'Thanks'
            };
        }

        var relateUnreleate = function (relate, collection, ids) {
            result = [];
            for (var i = 0; i < collection.length; i++) {
                var include = !relate;
                for (var j = 0; j < ids.length; j++) { // support data object
                    var id = collection[i].Entity ? collection[i].Entity.Id : collection[i].Id;
                    if (id == ids[j]) {
                        include = relate;
                        break;
                    }
                }
                if (include) {
                    result.push(collection[i]);
                }
            }
            return result;
        }

        var addAuxSteps = function (steps) {
            if ($rootScope.data) {
                addAuxStep('Module', steps, 'method');
                addAuxStep('Component', steps, 'method');
                addAuxStep('Method', steps, 'method');
                addAuxStep('ProjectUser', steps, 'item');
            }
        }

        var addMethodMarkers = function (steps) {
            if ($rootScope.data) {
                addAuxStep('AddMethod', steps, 'sequence');
                addAuxStep('AddReturn', steps, 'sequence');
                addAuxStep('AddNoReturn', steps, 'sequence');
            }
        }

        /*
         * 
         * 
         * 
         * 
         sequenceAddMethod: "<h1>Add Method</h1>Add the selected method at the end of the sequence.",
	sequenceAddReturn: "<h1>Add Return</h1>Add a return marker to the end of the sequence returning from the last method.",
	sequenceAddNoReturn: "<h1>Add No Return</h1>Add a no return marker to the end of the sequence returning to the method before last.",
         * 
         * 
         * 
         * 
         */

        var addAuxStep = function (name, steps, prefix) {
            if (document.querySelector('#item' + name)) {
                steps.push({
                    element: '#item' + name,
                    intro: $translate.instant(prefix + name, { data: $rootScope.data }),
                })
            }
        }

        var addCreateChildrenSteps = function (steps) {
            if ($rootScope.data) {
                steps.push(
                {
                    element: '#entityHeader',
                    intro: $translate.instant($rootScope.data.Type + 'Description'),
                    position: 'auto'
                });
                for (var property in $rootScope.data.Children) {
                    if ($rootScope.data.Children.hasOwnProperty(property)) {
                        if (document.querySelector('#add' + property)) {
                            var child = property.charAt(0).toUpperCase() + property.substr(1).replace(/([a-z])([A-Z])/g, '$1 $2').slice(0, -1).toLowerCase();
                            steps.push({
                                element: '#add' + property,
                                intro: $translate.instant('childButton', { data: $rootScope.data, property: child }),
                                // position: 'bottom'
                            })
                        } else {
                            // console.log("No control for #add" + property);
                        }
                    }
                }
            }
        }

        var addEntitySteps = function (steps) {
            if ($rootScope.wizardItem) {
                // console.log($rootScope.wizardItem);
                for (var property in $rootScope.wizardItem) {
                    if ($rootScope.wizardItem.hasOwnProperty(property)) {
                        if (document.querySelector('#item' + property)) {
                            steps.push({
                                element: '#item' + property,
                                intro: $translate.instant('item' + property, { data: $rootScope.data, property: property }),
                                // position: 'bottom'
                            });
                            //console.log("Added help for " + property);
                        } else {
                            // console.log("No control for " + property);
                        }
                    }
                }
            }
        }

        $rootScope.keys = function (obj) {
            var keys = [],
                k;
            for (k in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, k)) {
                    keys.push(k);
                }
            }
            return keys;
        };

        $rootScope.keySum = function (obj) {
            var sum = 0;
            for (k in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, k)) {
                    sum += obj[k];
                }
            }
            return sum;
        };


        return {

            /* User management */
            getCurrentUser: function (success, failure) {
                //console.log("Get current user");
                if (tokenValid()) {
                    success();
                } else {
                    /* Login automatically */
                    var user = {
                        Email: localPersist().getItem("uibuilditUsername"),
                        Password: localPersist().getItem("uibuilditPassword")
                    }
                    //console.log("Get current user from local storage");
                    //console.log(user);
                    if (user.Email && user.Password) {
                        server.login(user, function (data) {
                            // console.log(data);
                            fixUser(data);
                            setUser();
                            success(data);
                        }, failure);
                    } else {
                        failure();
                    }
                }
            },

            setCurrentUser: function (user) {
                fixUser(user);
                setUser();
            },

            logout: function (user) {
                clearUser();
            },

            getToken: function () {
                return {
                    Id: $rootScope.tokenId,
                    Text: $rootScope.tokenText,
                    UserId: $rootScope.userId,
                    UserName: $rootScope.userName,
                    UserEmail: $rootScope.userEmail
                };
            },

            clearLocalUser: function () {
                localPersist().removeItem("uibuilditUsername");
                localPersist().removeItem("uibuilditPassword");
            },

            /* Get collections */

            getTargetOSCollection: function () {
                server.getCollection("TargetOS", gotCollection, getCollectionFailed, 'targetOSCollection');
            },

            getHostingServerCollection: function () {
                server.getCollection("HostingServer", gotCollection, getCollectionFailed, 'hostingServerCollection');
            },

            getPersistenceLayerCollection: function () {
                server.getCollection("PersistenceLayer", gotCollection, getCollectionFailed, 'persistenceLayerCollection');
            },

            loadCollections: function (callback) {
                // If it's here don't do jack
                if ($rootScope.collections) {
                    callback();
                    return;
                }
                server.getCollections(function (data, state) {
                    gotCollections(data, state);
                    callback(data, state);
                }, function (data, state) {
                    console.log('failed to get collection. Logging out.');
                    clearUser();
                }
                );
            },

            getFrameworksForOS: function (os, callback) {
                if ($rootScope.project) {
                    server.getCollection("Framework", function (data, state) {
                        if ($rootScope.project) {
                            gotFrameworks(data);
                            if (callback) {
                                callback(data, state);
                            }
                        } else {
                            console.log("Got collections when no project exists");
                        }
                    }, getCollectionFailed, 'frameworkCollection', { type: 'targetOs', id: os.Id });
                }
            },

            /* Tie the project to the existing collections */
            gotProject: function (project) {
                //console.log(project);
                connectTargetItem(project, 'TargetOSs', 'TargetOS');
                connectTargetItem(project, 'HostingServers', 'HostingServer');
                connectTargetItem(project, 'PersistenceLayers', 'PersistenceLayer');
                //connectDeploymentTargetItem(project, 'frameworkCollection', 'TargetOS');
                connectTargetItem(project, 'Methods', 'DevelopmentMethodIds', true);
                connectTargetItem(project, 'Languages', 'DevelopmentLanguage');
            },

            addCollectionItem: function (system, systemCollection, newItem, availableCollection) {
                addCollectionItemImpl(system, systemCollection, newItem, availableCollection);
            },

            complexities: [
                    { name: 'Simple', id: 0 },
                     { name: 'Integrated', id: 1 },
                      { name: 'Enterprise', id: 2 }
            ],

            projectStatus: [
                        { text: 'No Milstones', id: 0 },
                        { text: 'Ready', id: 1 },
            ],

            currentSomething: function (somethingIds, collection) {
                //console.log(somethingIds);
                //console.log(collection);

                var result = [];
                somethingIds = somethingIds || [];
                collection = collection || [];
                for (var j = 0; j < somethingIds.length; j++) {
                    for (var i = 0; i < collection.length; i++) {

                        if (collection[i].Entity && collection[i].Entity.Id == somethingIds[j]) {
                            if (collection[i]) {
                                result.push(collection[i]);
                            }
                        } else if (collection[i].Id == somethingIds[j]) {
                            if (collection[i]) {
                                result.push(collection[i]);
                            }
                        }
                    }
                }
                //console.log(result);
                return result;
            },

            currentNotSomething: function (somethingIds, collection) {
                ///console.log('currentNotSomething');
                var result = [];
                somethingIds = somethingIds || [];
                for (var i = 0; i < collection.length; i++) {
                    var found = false;
                    for (var j = 0; j < somethingIds.length; j++) {
                        if (collection[i].Id == somethingIds[j]) {
                            //console.log('Found item to remove');
                            //console.log(collection[i]);
                            found = true;
                            break;;
                        }
                    }
                    if (!found) {
                        if (collection[i]) {
                            result.push(collection[i]);
                        }
                    }
                }
                return result;
            },

            editChildImpl: function (item) {
                editChildLocalImpl(item);
            },

            goToParent: function () {
                $location.path($rootScope.backPage.substring(1));
            },

            saveEntity: function (success, failure, entity) {
                saveEntityImpl(success, failure, entity);
            },

            saveEntityAndCreate: function (X, Y, targetPage, callback) {
                saveEntityImpl(function (data) {
                    server.getXForY(X, Y, { id: $rootScope.wizardItem.Id }, function (item, state) {
                        console.log(item);
                        if (targetPage) {
                            editChildLocalImpl(item);
                        } else {
                            callback(item);
                        }
                    }, function (date) {
                        console.log('Failed to get dummy usecase');
                    });
                }, null, Y);
            },

            getSequenceStatus: function (sequence) {
                for (var i = 0; i < sequence.Methods.length; i++) {
                    if (sequence.Methods[i].Entity.RiskStatus == "Terminal") {
                        return { text: 'Terminal Risk', shy: 'label-important', color: 'red' };
                    }
                }

                // console.log(sequence);
                if (sequence.Entity.Status == "PendingApproval") {
                    result = { text: 'Pending Approval', shy: 'label-warning', color: 'orange' };
                } else if (sequence.Entity.Status == "Done") {
                    result = { text: 'Done', shy: 'label-success', color: 'green' };
                } else {
                    result = { text: 'In process', shy: 'label-warning', color: 'orange' };
                }
                return result;
            },

            getTaskStatus: function (task) {
                return getTaskStatusImpl(task);
            },

            getIssueStatus: function (item) {
                var issue = item;
                if (issue.Status == "New" || issue.Status == "Rejected" || issue.Status == "WontFix") {
                    result = { text: 'Pending Approval', shy: 'label-warning', color: 'red' };
                } else if (issue.Status == "Complete") {
                    result = { text: 'Complete', shy: 'label-success', color: 'green' };
                } else {
                    result = { text: 'In process', shy: 'label-important', color: 'orange' };
                }
                if (issue.Effort > issue.EffortEstimation) {
                    result.effortColor = 'red';
                } else if (issue.Effort > issue.EffortEstimation * 0.7) {
                    result.effortColor = 'orange';
                } else {
                    result.effortColor = 'green';
                }
                return result;
            },


            updateSequence: function (sequence) {
                var returnQueue = [];

                sequence.Entity.Initiator = sequence.Entity.Initiator || "";
                if (sequence.Entity.Initiator && sequence.Methods && sequence.Methods.length > 0) {
                    sequence.Entity.definition = sequence.Entity.Initiator;
                    for (var i = 0; i < sequence.Methods.length; i++) {
                        var methodObj = sequence.Methods[i];
                        if (methodObj.Entity.Id < 0) {
                            if (methodObj.Entity.Id % 2 == -1) { // it's a return flag. Return to the previous component
                                if (returnQueue.length > 0) {
                                    var returnMethod = returnQueue.pop();
                                    var returnComponentName = sequence.Entity.Initiator;
                                    if (returnQueue.length > 0) {
                                        returnComponentName = getUserString(returnQueue[returnQueue.length - 1]);
                                    }
                                    // console.log(returnComponentName);
                                    sequence.Entity.definition += "-->" + returnComponentName + ": "
                                                + returnMethod.Entity.Return + "\n";
                                    if (methodObj.comment) {
                                        sequence.Entity.definition += "note over " + getUserString(returnMethod) + ": " + methodObj.comment + "\n";
                                        /*note over C: processing...\n\*/
                                    }
                                    if (i + 1 < sequence.Methods.length) {
                                        sequence.Entity.definition += returnComponentName;
                                    }
                                }
                            } else if (methodObj.Entity.Id % 2 == 0) { // it's a no return flag
                                if (returnQueue.length > 0) {
                                    var returnMethod = returnQueue.pop();
                                    var returnComponentName = sequence.Entity.Initiator;
                                    if (returnQueue.length > 0) {
                                        returnComponentName = getUserString(returnQueue[returnQueue.length - 1]);
                                    }
                                    if (methodObj.comment) {
                                        //console.log(methodObj.comment);
                                        sequence.Entity.definition += "\nnote over " + getUserString(returnMethod) + ": " + methodObj.comment + "\n";
                                        /*note over C: processing...\n\*/
                                    }
                                    var index = sequence.Entity.definition.lastIndexOf("\n");
                                    if (index > -1) {
                                        sequence.Entity.definition = sequence.Entity.definition.substr(0, index + 1);
                                        sequence.Entity.definition += returnComponentName;
                                    }
                                    //console.log(sequence.Entity.definition);
                                }

                            }
                        } else { // it's a method
                            var arrow = "->";
                            if (methodObj.Entity.ReturnMode == "ASync") {
                                var arrow = "-->";
                            }
                            sequence.Entity.definition += arrow + getUserString(methodObj) + ": " + getMethodText(methodObj) + "\n";
                            returnQueue.push(methodObj);
                            if (methodObj.comment) {
                                sequence.Entity.definition += "note over " + getUserString(methodObj) + ": " + methodObj.comment + "\n";
                                /*note over C: processing...\n\*/
                            } else if (methodObj.Entity.Description && methodObj.Entity.Description.substr(0, 14) != "new method for") {
                                sequence.Entity.definition += "note over " + getUserString(methodObj) + ": " + methodObj.Entity.Description + "\n";
                                /*note over C: processing...\n\*/
                            }

                            if (i + 1 < sequence.Methods.length) {
                                sequence.Entity.definition += getUserString(sequence.Methods[i]);
                            }
                        }
                    }
                }
                else {
                    sequence.Entity.definition = '';
                }
                // console.log(sequence.Entity.definition);
                /*  
                 *                     if (methodObj.ReturnMode == 'ASync') {
                                $scope.definition += $rootScope.sequence.methods[i].Component.Name + "->";
                            } else if (methodObj.ReturnMode == 'ASync') {
                 */
            },

            getMethodStatus: function (method) {
                if (method.RiskType == "No" || method.RiskStatus == "Resolved") {
                    return { text: 'No Risk', shy: 'label-success', color: 'green' };
                }
                if (method.RiskStatus == "Terminal") {
                    return { text: 'Terminal Risk', shy: 'label-important', color: 'red' };
                }

                return { text: 'Pending', shy: 'label-warning', color: 'orange' };
            },

            clearData: function () {
                $rootScope.moduleName = null;
                $rootScope.wizardItem = null;
                $rootScope.data = null;
                $rootScope.componentName = null;
                $rootScope.methodName = null;
                $rootScope.parent = null;
                $rootScope.canEdit = false;
            },

            methodText: function (method) {
                return getMethodText(method);
            },

            refreshTree: function (callback) {
                refreshTreeImpl(callback);
            },


            initialize: function (item, scope, refresh, gotChild) {
                $rootScope.backButton = null;
                $rootScope.backPage = null;
                $rootScope.searchResults = null;
                if (item) {
                    $rootScope.canEdit = item.CanEdit;
                    $rootScope.wizardItem = item.Entity;
                    $rootScope.parent = item.Parent;
                    $rootScope.data = item;

                    if (item.Parent) {
                        $rootScope.backButton = item.Parent.Name;
                        $rootScope.backPage = "#/Edit/:" + item.ParentType.toLowerCase() + "/:" + item.Parent.ParentId + "/:" + item.Parent.Id;
                    }
                    if (item.Children) {
                        for (var property in item.Children) {
                            if (item.Children.hasOwnProperty(property)) {
                                for (var i = 0; i < item.Children[property].length; i++) {
                                    // item.Children[property][i].type = property.substr(0, property.length - 1);
                                    item.Children[property][i].parentType = item.Type;
                                    item.Children[property][i].parent = item.Entity;
                                }
                            }
                        }
                    }

                    $rootScope.addTag = function (tag) {
                        for (var i = 0; i < $rootScope.data.Metadata.Tags.length; i++) {
                            var existing = $rootScope.data.Metadata.Tags[i];
                            if (tag.Name == existing.Name && existing.Deleted) {
                                existing.Deleted = false;
                                return;
                            }
                        }
                        $rootScope.data.Metadata.Tags.push({
                            Name: tag,
                            Id: -1,
                            ProjectId: $rootScope.wizardItem.ProjectId,
                            EntityId: $rootScope.wizardItem.Id,
                            EntityType: $rootScope.data.Type
                        });
                    }

                    $rootScope.removeTag = function (tag) {
                        for (var i = 0; i < $rootScope.data.Metadata.Tags.length; i++) {
                            var existing = $rootScope.data.Metadata.Tags[i];
                            if (tag == existing) {
                                tag.Deleted = true;
                            }
                        }
                    }

                    $rootScope.disableNewTag = function () {
                        if ($rootScope.newTag) {
                            for (var i = 0; i < $rootScope.data.Metadata.Tags.length; i++) {
                                var existing = $rootScope.data.Metadata.Tags[i];
                                if ($rootScope.newTag == existing.Name && !existing.Deleted) {
                                    return true;
                                }
                            }
                            return false;
                        }
                        return true;
                    }

                    $rootScope.saveTags = function () {
                        for (var i = 0; i < $rootScope.data.Metadata.Tags.length; i++) {
                            var tag = $rootScope.data.Metadata.Tags[i];
                            if (tag.Id == -1) { // it's a new one - save it
                                server.commitEntity(tag, function (data, state) {
                                    console.log("Saved tag " + tag);
                                }, function (data, state) {
                                    console.log("Failed to save tag " + tag);
                                    console.log(data);
                                }, "TagEntity");
                            } else if (tag.Deleted) {
                                server.deleteEntity(tag, function (data) {
                                    console.log("deleted " + tag.Name);
                                }, function (data) {
                                    console.log("failed to delete " + tag.Name);
                                }, "TagEntity");
                            }
                        }
                        return false;
                    }

                    $rootScope.wizardItem.IndexString = indexToString($rootScope.wizardItem.Index);
                }

                $rootScope.refreshItem = refresh;
                $rootScope.gotChild = gotChild;
                $rootScope.toggleNode = function (family, event) {
                    family.collapsed = !family.collapsed;
                }
                $rootScope.createProject = function () {
                    server.getEntity({ id: -1 }, function (project, state) {
                        var location = "/Edit/:" + project.Type + "/:" + project.Entity.ParentId + "/:" + project.Entity.Id;
                        refreshTreeImpl(function () {
                            $location.path(location);
                        });

                    }, function (date) {
                        console.log('Failed to create project');
                    }, 'Project');
                };
                scope.editChild = editChildLocalImpl;
                scope.getChildStatus = getChildStatusImpl;
                scope.getTaskStatus = getTaskStatusImpl;
                scope.deleteChild = deleteChildImpl;
                scope.createSubItem = createSubItemImpl;
                scope.wizardDone = wizardDoneImpl;
                scope.editItem = editItemImpl;

                scope.tooltip = function (name, property, data) {
                    // console.log(name);
                    data = data ? data : $rootScope.data
                    return {
                        "title": $translate.instant(name, { data: data, property: property, main: $rootScope.data }),
                        "checked": false
                    };
                }

                $rootScope.tooltip = scope.tooltip;

                scope.buttonTooltip = function (name, property) {
                    return {
                        "title": $translate.instant(name + 'Button', { data: $rootScope.data, property: property }),
                        "checked": false
                    };
                }

                scope.itemTooltip = function (name) {
                    // console.log(name);
                    return {
                        "title": $translate.instant('item' + name, { data: $rootScope.data, property: property }),
                        "checked": false
                    };
                }

                scope.addTask = function () {
                    getCommonItem('Task');
                }

                scope.addNote = function () {
                    getCommonItem('Note');
                }
                scope.delete = function () {
                    var item = { item: $rootScope.data };
                    var target = "/home";
                    var node = getEntityNode($rootScope.data);
                    //console.log(node);
                    if (node.parent) {// not a project
                        if (node) {
                            var parent = node.parent;
                            for (var i = 0; i < parent.nodes.length; i++) {
                                if (node.id == parent.nodes[i].id) {
                                    parent.nodes.splice(i, 1);
                                    break;
                                }
                            }
                        }
                    } else { // a project
                        for (var i = 0; i < $rootScope.tree.length; i++) {
                            if (node.id == $rootScope.tree[i].id) {
                                $rootScope.tree.splice(i, 1);
                                break;
                            }
                        }
                    }

                    if ($rootScope.data.Type != "Project") {
                        target = $location.path("/Edit/:" + $rootScope.data.ParentType.toLowerCase() + "/:" + $rootScope.data.Parent.ParentId + "/:" + $rootScope.data.Parent.Id);
                    }
                    $rootScope.refreshItem = function (data, state) {
                        $location.path(target);
                    }
                    deleteChildImpl(item);
                }
                scope.duplicate = duplicateImpl;

                // local.setBackButton('Project:' + item.Parent.Name, 'projectEdit', item.Parent.Id);
                scope.deleteCommonItem = function (item) {
                    item.item.ParentType = $rootScope.data.Type;
                    item.item.Entity.ParentId = $rootScope.wizardItem.Id;
                    deleteChildImpl(item);
                };

                scope.getRepeat = function (num) {
                    return new Array(num - 1);
                }

                // The item is a part of the collection and might be entity or data
                var removeCollectionItem = function (item, ids) { //(pred, $root.data.PotentialPredecessors, $root.data.Entity.PredecessorIds)
                    for (var i = 0; i < ids.length; i++) {
                        if (item.Id == ids[i]) {
                            ids.splice(i, 1);
                            return;
                        }
                    }
                    console.log("Failed to remove " + item.Id);
                }

                if ($rootScope.data && $rootScope.data.PotentialPredecessors) {
                    scope.unrelatedPredecessors = function () {
                        var result = relateUnreleate(false, $rootScope.data.PotentialPredecessors, $rootScope.wizardItem.PredecessorIds);
                        return result;
                    }

                    scope.relatedPredecessors = function () {
                        var result = relateUnreleate(true, $rootScope.data.PotentialPredecessors, $rootScope.wizardItem.PredecessorIds);
                        return result;
                    }

                    selectFirstPredecessor = function () {
                        var trimmed = scope.unrelatedPredecessors();
                        if (trimmed.length > 0) {
                            $rootScope.newPredecessor = trimmed[0];
                        }
                    }

                    if ($rootScope.data.PotentialPredecessors.length > 0) {
                        scope.newPredecessor = $rootScope.data.PotentialPredecessors[0];

                        for (var i = 0; i < $rootScope.data.PotentialPredecessors.length; i++) {
                            $rootScope.data.PotentialPredecessors[i].Entity.link = "#/Edit/:" + $rootScope.data.Type + "/:" +
                                $rootScope.data.PotentialPredecessors[i].Entity.ParentId + "/:" + item.PotentialPredecessors[i].Entity.Id;
                        }
                    }

                    selectFirstPredecessor();

                    scope.removePredecessor = function (item) {
                        removeCollectionItem(item, $rootScope.data.Entity.PredecessorIds);
                        selectFirstPredecessor();
                    }

                    scope.addPredecessor = function () {
                        //console.log($rootScope.newPredecessor);
                        $rootScope.wizardItem.PredecessorIds.push($rootScope.newPredecessor.Entity.Id);
                        //console.log($rootScope.wizardItem.PredecessorIds);
                        selectFirstPredecessor();
                    };
                }

                if ($rootScope.data && $rootScope.data.AvailableRequirements) {

                    scope.unrelatedRequirements = function () {
                        var result = relateUnreleate(false, $rootScope.data.AvailableRequirements, $rootScope.wizardItem.RequirementIds);
                        return result;
                    }

                    scope.relatedRequirements = function () {
                        var result = relateUnreleate(true, $rootScope.data.AvailableRequirements, $rootScope.wizardItem.RequirementIds);
                        return result;
                    }

                    var selectFirstRequirement = function () {
                        var trimmed = scope.unrelatedRequirements();
                        if (trimmed.length > 0) {
                            $rootScope.newRequirement = trimmed[0];
                        }
                    }

                    var unrelated = scope.unrelatedRequirements();
                    if (unrelated.length > 0) {
                        $rootScope.newRequirement = unrelated[0];

                        for (var i = 0; i < $rootScope.data.AvailableRequirements.length; i++) {
                            $rootScope.data.AvailableRequirements[i].link = "#/Edit/:requirement/:" + $rootScope.wizardItem.ParentId + "/:" + $rootScope.data.AvailableRequirements[i].Id;
                        }
                    }

                    scope.addRequirement = function () {
                        $rootScope.wizardItem.RequirementIds.push($rootScope.newRequirement.Id);
                        selectFirstRequirement();
                    }

                    scope.removeRequirement = function (item) {
                        removeCollectionItem(item, $rootScope.data.Entity.RequirementIds);
                        selectFirstRequirement();
                    }
                }

                $rootScope.treeClick = function (target) {
                    expandNode(target);
                    $location.path(target.target);
                }

                var onKeyDownImpl = function (e) {
                    if (e.keyCode == 83 && (navigator.platform.match("Mac") ? e.metaKey : e.ctrlKey)) { // Save
                        e.preventDefault();
                        if (scope.save) {
                            scope.save();
                        } else {
                            wizardDoneImpl();
                        }
                    } else if (e.keyCode == 76 && (navigator.platform.match("Mac") ? e.metaKey : e.ctrlKey) || e.keyCode == 113) { // Show Options on F2 or CTRL L
                        e.preventDefault();
                        angular.element('#docuementAttributesModal').modal('show');
                    } else if (e.keyCode == 13) {
                        // On enter
                        //e.preventDefault();

                    } else if (e.keyCode == 112) { // F1
                        // On enter
                        e.preventDefault();
                        initializeTour(scope);
                        scope['startTour']();
                        console.log("starting tour");
                    }
                }

                $document.off("keydown");
                $document.on("keydown", onKeyDownImpl);

                //document.removeEventListener("keydown", onKeyDownImpl, false);
                //document.addEventListener("keydown", onKeyDownImpl, false);

                scope.ngIntroAutostart = function () { return false; };
                scope.ngIntroAutorefresh = true;

                if ($rootScope.tokenId) {
                    if (!$rootScope.tree) {
                        refreshTreeImpl();
                    } else {
                        updateTree();
                    }

                    if (!$rootScope.CallTypes) {
                        getCollectionsImpl();
                    }
                }


                scope.tooltipDelay = { show: 1000, hide: 200 };

                // TODO: Fix this!!! The ID is created only after the angular is linked.
                if ($rootScope.data) {
                    $timeout(function () {
                        initializeTour(scope);
                    }, 1000);
                }
            },

            refreshTree: function (callback) {
                refreshTreeImpl(callback);
            }
        };
    }]).service('Sample', ['$http', '$q', '$rootScope', '$location', '$window', 'server', '$translate', '$timeout', '$document',
    function ($http, $q, $rootScope, $location, $window, server, $translate, $timeout, $document) {
        return {
            getSampleData: function () {
                return [
                        // Order is optional. If not specified it will be assigned automatically
                        {
                            name: 'Milestones', height: '3em', sortable: false, classes: 'gantt-row-milestone', color: '#45607D', tasks: [
                               // Dates can be specified as string, timestamp or javascript date object. The data attribute can be used to attach a custom object
                               { name: 'Kickoff', color: '#93C47D', from: '2013-10-07T09:00:00', to: '2013-10-07T10:00:00', data: 'Can contain any custom data or object' },
                               { name: 'Concept approval', color: '#93C47D', from: new Date(2013, 9, 18, 18, 0, 0), to: new Date(2013, 9, 18, 18, 0, 0), est: new Date(2013, 9, 16, 7, 0, 0), lct: new Date(2013, 9, 19, 0, 0, 0) },
                               { name: 'Development finished', color: '#93C47D', from: new Date(2013, 10, 15, 18, 0, 0), to: new Date(2013, 10, 15, 18, 0, 0) },
                               { name: 'Shop is running', color: '#93C47D', from: new Date(2013, 10, 22, 12, 0, 0), to: new Date(2013, 10, 22, 12, 0, 0) },
                               { name: 'Go-live', color: '#93C47D', from: new Date(2013, 10, 29, 16, 0, 0), to: new Date(2013, 10, 29, 16, 0, 0) }
                            ], data: 'Can contain any custom data or object'
                        },
                        {
                            name: 'Status meetings', tasks: [
                               { name: 'Demo #1', color: '#9FC5F8', from: new Date(2013, 9, 25, 15, 0, 0), to: new Date(2013, 9, 25, 18, 30, 0) },
                               { name: 'Demo #2', color: '#9FC5F8', from: new Date(2013, 10, 1, 15, 0, 0), to: new Date(2013, 10, 1, 18, 0, 0) },
                               { name: 'Demo #3', color: '#9FC5F8', from: new Date(2013, 10, 8, 15, 0, 0), to: new Date(2013, 10, 8, 18, 0, 0) },
                               { name: 'Demo #4', color: '#9FC5F8', from: new Date(2013, 10, 15, 15, 0, 0), to: new Date(2013, 10, 15, 18, 0, 0) },
                               { name: 'Demo #5', color: '#9FC5F8', from: new Date(2013, 10, 24, 9, 0, 0), to: new Date(2013, 10, 24, 10, 0, 0) }
                            ]
                        },
                        {
                            name: 'Kickoff', movable: { allowResizing: false }, tasks: [
                               {
                                   name: 'Day 1', color: '#9FC5F8', from: new Date(2013, 9, 7, 9, 0, 0), to: new Date(2013, 9, 7, 17, 0, 0),
                                   progress: { percent: 100, color: '#3C8CF8' }, movable: false
                               },
                               {
                                   name: 'Day 2', color: '#9FC5F8', from: new Date(2013, 9, 8, 9, 0, 0), to: new Date(2013, 9, 8, 17, 0, 0),
                                   progress: { percent: 100, color: '#3C8CF8' }
                               },
                               {
                                   name: 'Day 3', color: '#9FC5F8', from: new Date(2013, 9, 9, 8, 30, 0), to: new Date(2013, 9, 9, 12, 0, 0),
                                   progress: { percent: 100, color: '#3C8CF8' }
                               }
                            ]
                        },
                        {
                            name: 'Create concept', tasks: [
                               {
                                   name: 'Create concept', content: '<i class="fa fa-cog" ng-click="scope.handleTaskIconClick(task.model)"></i> {{task.model.name}}', color: '#F1C232', from: new Date(2013, 9, 10, 8, 0, 0), to: new Date(2013, 9, 16, 18, 0, 0), est: new Date(2013, 9, 8, 8, 0, 0), lct: new Date(2013, 9, 18, 20, 0, 0),
                                   progress: 100
                               }
                            ]
                        },
                        {
                            name: 'Finalize concept', tasks: [
                               {
                                   name: 'Finalize concept', color: '#F1C232', from: new Date(2013, 9, 17, 8, 0, 0), to: new Date(2013, 9, 18, 18, 0, 0),
                                   progress: 100
                               }
                            ]
                        },
                        { name: 'Development', children: ['Sprint 1', 'Sprint 2', 'Sprint 3', 'Sprint 4'], content: '<i class="fa fa-file-code-o" ng-click="scope.handleRowIconClick(row.model)"></i> {{row.model.name}}' },
                        {
                            name: 'Sprint 1', tooltips: false, tasks: [
                               {
                                   name: 'Product list view', color: '#F1C232', from: new Date(2013, 9, 21, 8, 0, 0), to: new Date(2013, 9, 25, 15, 0, 0),
                                   progress: 25
                               }
                            ]
                        },
                        {
                            name: 'Sprint 2', tasks: [
                               { name: 'Order basket', color: '#F1C232', from: new Date(2013, 9, 28, 8, 0, 0), to: new Date(2013, 10, 1, 15, 0, 0) }
                            ]
                        },
                        {
                            name: 'Sprint 3', tasks: [
                               { name: 'Checkout', color: '#F1C232', from: new Date(2013, 10, 4, 8, 0, 0), to: new Date(2013, 10, 8, 15, 0, 0) }
                            ]
                        },
                        {
                            name: 'Sprint 4', tasks: [
                               { name: 'Login & Signup & Admin Views', color: '#F1C232', from: new Date(2013, 10, 11, 8, 0, 0), to: new Date(2013, 10, 15, 15, 0, 0) }
                            ]
                        },
                        { name: 'Hosting' },
                        {
                            name: 'Setup', tasks: [
                               { name: 'HW', color: '#F1C232', from: new Date(2013, 10, 18, 8, 0, 0), to: new Date(2013, 10, 18, 12, 0, 0) }
                            ]
                        },
                        {
                            name: 'Config', tasks: [
                               { name: 'SW / DNS/ Backups', color: '#F1C232', from: new Date(2013, 10, 18, 12, 0, 0), to: new Date(2013, 10, 21, 18, 0, 0) }
                            ]
                        },
                        { name: 'Server', parent: 'Hosting', children: ['Setup', 'Config'] },
                        {
                            name: 'Deployment', parent: 'Hosting', tasks: [
                               { name: 'Depl. & Final testing', color: '#F1C232', from: new Date(2013, 10, 21, 8, 0, 0), to: new Date(2013, 10, 22, 12, 0, 0), 'classes': 'gantt-task-deployment' }
                            ]
                        },
                        {
                            name: 'Workshop', tasks: [
                               { name: 'On-side education', color: '#F1C232', from: new Date(2013, 10, 24, 9, 0, 0), to: new Date(2013, 10, 25, 15, 0, 0) }
                            ]
                        },
                        {
                            name: 'Content', tasks: [
                               { name: 'Supervise content creation', color: '#F1C232', from: new Date(2013, 10, 26, 9, 0, 0), to: new Date(2013, 10, 29, 16, 0, 0) }
                            ]
                        },
                        {
                            name: 'Documentation', tasks: [
                               { name: 'Technical/User documentation', color: '#F1C232', from: new Date(2013, 10, 26, 8, 0, 0), to: new Date(2013, 10, 28, 18, 0, 0) }
                            ]
                        }
                ];
            },
            getSampleTimespans: function () {
                return [
                        {
                            from: new Date(2013, 9, 21, 8, 0, 0),
                            to: new Date(2013, 9, 25, 15, 0, 0),
                            name: 'Sprint 1 Timespan'
                            //priority: undefined,
                            //classes: [],
                            //data: undefined
                        }
                ];
            }
        };
    }]);
