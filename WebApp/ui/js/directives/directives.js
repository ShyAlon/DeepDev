'use strict';

/* Directives */


angular.module('DeepDev.directives').
  directive('appVersion', ['version', function (version) {
      return function (scope, elm, attrs) {
          elm.text(version);
      };
  }]).directive('vbox', function () {
      return {
          link: function (scope, element, attrs) {
              attrs.$observe('vbox', function (value) {
                  element.attr('viewBox', value);
              })
          }
      };
  }).directive('saveButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {

          },
          template: '<span><button class="btn btn-lg btn-success" href="#" ng-click="save()" ng-disabled="generalForm.$invalid || !$root.canEdit" onclick="return false;" role="button" id="saveButton" bs-tooltip="buttonTooltip(\'save\')" placement="auto" >Save</button></span>'
      }
  }]).directive('duplicateButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {

          },
          template: '<span><button class="btn btn-lg btn-success" href="#" ng-click="duplicate()" ng-disabled="generalForm.$invalid || !$root.canEdit" onclick="return false;" role="button" id="duplicateButton" bs-tooltip="buttonTooltip(\'duplicate\')" placement="auto">Duplicate</button></span>'
      }
  }]).directive('deleteButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {

          },
          template: '<span><button class="btn btn-lg btn-danger" href="#" ng-click="delete()" ng-disabled="!$root.wizardItem" onclick="return false;" role="button" id="deleteButton" bs-tooltip="buttonTooltip(\'delete\')" placement="auto">Delete</button></span>'
      }
  }]).directive('addTaskButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {

          },
          template: '<span><button class="btn btn-lg btn-success" href="#" ng-click="addTask()" ng-disabled="generalForm.$invalid || !$root.canEdit" onclick="return false;" role="button" id="taskButton" bs-tooltip="buttonTooltip(\'task\')" placement="auto">Add Task</button></span>'
      }
  }]).directive('addNoteButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {

          },
          template: '<span><button class="btn btn-lg btn-success" href="#" ng-click="addNote()" ng-disabled="generalForm.$invalid || !$root.canEdit" onclick="return false;" role="button" id="noteButton" bs-tooltip="buttonTooltip(\'note\')" placement="auto">Add Note</button></span>'
      }
  }]).directive('addButton', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          link: function ($scope, element, attrs) {

          },
          templateUrl: './partials/blocks/addButton.html',
      }
  }]).directive('project', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          link: function ($scope, element, attrs) {

          },
          templateUrl: './partials/document/project.html',
      }
  }]).directive('sequence', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          link: function ($scope, element, attrs) {

          },
          templateUrl: './partials/blocks/sequence.html',
      }
  }]).directive('pairs', ['$http', function ($http) {
      return {
          scope: {
              item: '=',
              hideRemove: '='
          },
          restrict: 'EA',
          link: function ($scope, element, attrs) {
              // console.log($scope.item);
          },
          templateUrl: './partials/document/pairs.html',
      }
  }]).directive('wizardPage', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          //templateUrl: function (elem, attrs) { return ('./partials/wizard/' + attrs.wizardPage + '.html') },
          link: function ($scope, element, attrs) {
              $scope.contentUrl = './partials/wizard/' + attrs.wizardPage + '.html';
              attrs.$observe("wizardPage", function (w) {
                  //console.log(w);
                  $scope.contentUrl = './partials/wizard/' + w + '.html';
              });
          },
          template: '<div ng-include="contentUrl"></div>'
      }
  }]).directive('children', ['$http', function ($http) {
      return {
          scope: false,
          transclude: true,
          restrict: 'EA',
          templateUrl: function (elem, attrs) { return ('./partials/blocks/children.html') },
          link: function ($scope, element, attrs) {

          }
      };
  }]).directive('block', ['$http', function ($http) {
      return {
          scope: {
              item: '=',
              entityType: '=',
              data: '=',
              onButton: '&',
              onEdit: '&',
              onAdd: '&',
              onRemove: '&',
              onReplace: '&',
              getStatus: '&',
              hideRemove: '='
          },
          transclude: true,
          restrict: 'EA',
          templateUrl: function (elem, attrs) {
              return ('./partials/blocks/' + attrs.type + '.html')
          },
          link: function ($scope, element, attrs) {

          }
      };
  }]).directive('document', ['$http', function ($http) {
      return {
          scope: false,
          restrict: 'EA',
          link: function ($scope, element, attrs) {
              $scope.contentUrl = './partials/documents/' + attrs.document + '.html';
              attrs.$observe("document", function (d) {
                  //console.log(w);
                  $scope.contentUrl = './partials/documents/' + d + '.html';
              });
          },
          template: '<div ng-include="contentUrl"></div>'
      };
  }]).directive('numbersOnly', function () {
      return {
          require: 'ngModel',
          link: function (scope, element, attrs, modelCtrl) {
              modelCtrl.$parsers.push(function (inputValue) {
                  // this next if is necessary for when using ng-required on your input. 
                  // In such cases, when a letter is typed first, this parser will be called
                  // again, and the 2nd time, the value will be undefined
                  if (inputValue == undefined) return ''
                  var transformedInput = inputValue.replace(/[^0-9]/g, '');
                  if (transformedInput != inputValue) {
                      modelCtrl.$setViewValue(transformedInput);
                      modelCtrl.$render();
                  }

                  return transformedInput;
              });
          }
      };
  }).directive('contentItem', ['$compile', '$rootScope', function ($compile, $rootScope) { // document for entity
      var headerTemplate =
          '<u><hsetHeaderSize class="document" id="setId" ng-click="onButton()">' +
          '<span>*setLink*</span>' +
          '</hsetHeaderSize></u>&nbsp<i ng-hide="$root.hideIcons" ng-class="{\'fa fa-expand \': onEdit(), \'fa fa-compress \': !onEdit()}"></i>';

      var indexString = function (index) {
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


      var linker = function (scope, element, attrs) {
          scope.$watch(attrs.dynamic, function (html) {
              if (!scope.item) {
                  return;
              }
              var guid = function () {
                  function _p8(s) {
                      var p = (Math.random().toString(16) + "000000000").substr(2, 8);
                      return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
                  }
                  return _p8() + _p8(true) + _p8(true) + _p8();
              }

              var template = headerTemplate;
              if (scope.item.Entity) {
                  //console.log(scope.Entity);
                  scope.IndexString = indexString(scope.item.Entity.Index);

              } else {
                  //console.log("no entity");
              }
              console.log(scope.item);
              if (scope.item.ParentType && scope.item.Type) {
                  //console.log(scope.item);
                  var url = "<a a-disabled='$root.passiveHeaders' ng-href='#/Edit/:" +
                            scope.item.Type.toLowerCase() + '/:' + scope.item.Entity.ParentId + '/:' + scope.item.Entity.Id +
                            "'>{{IndexString}} {{item.Entity.Name | ordinal}}</a>";
                  // console.log(url);
                  template = template.replace("*setLink*", url);
              } else if (scope.item.Entity) {
                  template = template.replace("*setLink*", "{{item.Entity.Name | ordinal}}");
              } else {
                  template = template.replace("*setLink*", "{{item.Type | ordinal}}");
              }
              scope.item = scope.item || 1;
              template = template.replace("setHeaderSize", scope.item.size);
              template = template.replace("setTitle", scope.item.staticTitle);
              template = template.replace("setCount", scope.item.count);
              template = template.replace("setId", guid());
              if (scope.item.Type == "UseCaseAction" || scope.item.Type == "Method" && !scope.content) {
                  element.html('<div></div>');
              } else {
                  element.html(template);
              }
              $compile(element.contents())(scope);
          });
      }

      return {
          restrict: 'EA',
          rep1ace: true,
          link: linker,
          transclude: true,
          scope: {
              content: '=',
              item: '=',
              onButton: '&',
              onEdit: '&',
              getStatus: '&',
              hideIcons: '=',
              header: '=',
          }
      };
  }]).factory('RecursionHelper', ['$compile', function ($compile) {
      return {
          /**
           * Manually compiles the element, fixing the recursion loop.
           * @param element
           * @param [link] A post-link function, or an object with function(s) registered via pre and post properties.
           * @returns An object containing the linking functions.
           */
          compile: function (element, link) {
              // Normalize the link parameter
              if (angular.isFunction(link)) {
                  link = { post: link };
              }

              // Break the recursion loop by removing the contents
              var contents = element.contents().remove();
              var compiledContents;
              return {
                  pre: (link && link.pre) ? link.pre : null,
                  /**
                   * Compiles and re-adds the contents
                   */
                  post: function (scope, element) {
                      // Compile the contents
                      if (!compiledContents) {
                          compiledContents = $compile(contents);
                      }
                      // Re-add the compiled contents to the element
                      compiledContents(scope, function (clone) {
                          element.append(clone);
                      });

                      // Call the post-linking function, if any
                      if (link && link.post) {
                          link.post.apply(null, arguments);
                      }
                  }
              };
          }
      };
  }]).directive('documentChildren', ['$http', 'RecursionHelper', function ($http, RecursionHelper) {
      return {
          scope: {
              data: '=',
              header: '=',
              type: '='

          },
          restrict: 'EA',
          compile: function (element) {
              return RecursionHelper.compile(element, function (scope, iElement, iAttrs, controller, transcludeFn) {
                  // Define your normal link function here.
                  // Alternative: instead of passing a function,
                  // you can also pass an object with 
                  // a 'pre'- and 'post'-link function.
                  scope.contentUrl = './partials/document/documentChildren.html';
                  //console.log(scope.type);
                  iAttrs.$observe("type", function (w) {
                      if (!scope.data) {
                          return;
                      }
                      if (!w) {
                          return;
                      }
                      //console.log(scope.data);
                      //console.log(scope.header);
                      //console.log(w);
                      if (scope.data.Type == "UseCase" || scope.data.Type == "Parameter" || scope.data.Type == "UseCaseAction" || scope.data.Type == "Note"
                          || scope.data.Type == "Method" || scope.data.Type == "Task" || scope.data.Type == "Issue" || scope.data.Type == "Requirement") {
                          var computed = './partials/document/' + scope.data.Type.toLowerCase() + '.html';
                          //console.log(computed);
                          scope.contentUrl = computed;
                      }
                  });
              });
          },
          template: '<div ng-include="contentUrl"></div>'
      }
  }]).directive('scrollTo', ['ScrollTo', function (ScrollTo) {
      return {
          restrict: "AC",
          compile: function () {
              return function (scope, element, attr) {
                  element.bind("click", function (event) {
                      // console.log(attr.scrollTo);
                      ScrollTo.idOrName(attr.scrollTo, attr.offset);
                  });
              };
          }
      };
  }]).directive('aDisabled', function () {
      return {
          compile: function (tElement, tAttrs, transclude) {
              //Disable ngClick
              tAttrs["ngClick"] = ("ng-click", "!(" + tAttrs["aDisabled"] + ") && (" + tAttrs["ngClick"] + ")");

              //Toggle "disabled" to class when aDisabled becomes true
              return function (scope, iElement, iAttrs) {
                  scope.$watch(iAttrs["aDisabled"], function (newValue) {
                      if (newValue !== undefined) {
                          iElement.toggleClass("disabled", newValue);
                      }
                  });

                  //Disable href on click
                  iElement.on("click", function (e) {
                      if (scope.$eval(iAttrs["aDisabled"])) {
                          e.preventDefault();
                      }
                  });
              };
          }
      };
  }).directive("tree", ['$compile', function ($compile) {
      //Here is the Directive Definition Object being returned 
      //which is one of the two options for creating a custom directive
      //http://docs.angularjs.org/guide/directive
      return {
          restrict: "E",
          //We are stating here the HTML in the element the directive is applied to is going to be given to
          //the template with a ng-transclude directive to be compiled when processing the directive
          transclude: true,
          scope: { family: '=' },
          template:
              '<ul class="nopadding" style="margin-left:5px;list-style: none;">' +
                  //Here we have one of the ng-transclude directives that will be give the HTML in the 
                  //element the directive is applied to
                  '<li ng-transclude></li>' +
                  '<li ng-repeat="child in family.nodes">' +
                      //Here is another ng-transclude directive which will be given the same transclude HTML as
                      //above instance
                      //Notice that there is also another directive, 'tree', which is same type of directive this 
                      //template belongs to.  So the directive in the template will handle the ng-transclude 
                      //applied to the div as the transclude for the recursive compile call to the tree 
                      //directive.  The recursion will end when the ng-repeat above has no children to 
                      //walkthrough.  In other words, when we hit a leaf.
                      '<tree family="child" ng-show="!family.collapsed"><div ng-transclude></div></tree>' +
                  '</li>' +
              '</ul>',
          compile: function (tElement, tAttr, transclude) {
              //We are removing the contents/innerHTML from the element we are going to be applying the 
              //directive to and saving it to adding it below to the $compile call as the template
              var contents = tElement.contents().remove();
              var compiledContents;
              return function (scope, iElement, iAttr) {

                  if (!compiledContents) {
                      //Get the link function with the contents frome top level template with 
                      //the transclude
                      compiledContents = $compile(contents, transclude);
                  }
                  //Call the link function to link the given scope and
                  //a Clone Attach Function, http://docs.angularjs.org/api/ng.$compile :
                  // "Calling the linking function returns the element of the template. 
                  //    It is either the original element passed in, 
                  //    or the clone of the element if the cloneAttachFn is provided."
                  compiledContents(scope, function (clone, scope) {
                      //Appending the cloned template to the instance element, "iElement", 
                      //on which the directive is to used.
                      iElement.append(clone);
                  });
              };
          }
      };
  }]).directive('dhxGantt', function () {
      return {
          restrict: 'A',
          scope: false,
          transclude: true,
          template: '<div ng-transclude></div>',

          link: function ($scope, $element, $attrs, $controller) {
              //watch data collection, reload on changes
              $scope.$watch($attrs.data, function (collection) {
                  gantt.clearAll();
                  gantt.parse(collection, "json");
              }, true);

              //size of gantt
              $scope.$watch(function () {
                  return $element[0].offsetWidth + "." + $element[0].offsetHeight;
              }, function () {
                  gantt.setSizes();
              });

              gantt.config.work_time = true;


              gantt.config.scale_unit = "day";
              gantt.config.date_scale = "%D, %d";
              gantt.config.min_column_width = 60;
              gantt.config.duration_unit = "day";
              gantt.config.scale_height = 20 * 3;
              gantt.config.row_height = 30;



              var weekScaleTemplate = function (date) {
                  var dateToStr = gantt.date.date_to_str("%d %M");
                  var weekNum = gantt.date.date_to_str("(week %W)");
                  var endDate = gantt.date.add(gantt.date.add(date, 1, "week"), -1, "day");
                  return dateToStr(date) + " - " + dateToStr(endDate) + " " + weekNum(date);
              };

              gantt.config.subscales = [
                  { unit: "month", step: 1, date: "%F, %Y" },
                  { unit: "week", step: 1, template: weekScaleTemplate }

              ];

              gantt.templates.task_cell_class = function (task, date) {
                  if (!gantt.isWorkTime(date))
                      return "week_end";
                  return "";
              };              setScaleConfig('1');


              var func = function (e) {
                  e = e || window.event;
                  var el = e.target || e.srcElement;
                  var value = el.value;
                  setScaleConfig(value);
                  gantt.render();
              };

              var els = document.getElementsByName("scale");
              for (var i = 0; i < els.length; i++) {
                  els[i].onclick = func;
              }
              gantt.config.readonly = true;
              //init gantt
              gantt.init($element[0]);
          }
      }
  }).directive('ganttTemplate', ['$filter', function ($filter) {
      gantt.aFilter = $filter;
      return {
          restrict: 'AE',
          terminal: true,

          link: function ($scope, $element, $attrs, $controller) {
              var template = Function('sd', 'ed', 'task', 'return "' + templateHelper($element) + '"');
              gantt.templates[$attrs.ganttTemplate] = template;
          }
      };
  }]).directive('ganttColumn', ['$filter', function ($filter) {
      gantt.aFilter = $filter;

      return {
          restrict: 'AE',
          terminal: true,

          link: function ($scope, $element, $attrs, $controller) {
              var label = $attrs.label || " ";
              var width = $attrs.width || "*";
              var align = $attrs.align || "left";

              var template = Function('task', 'return "' + templateHelper($element) + '"');
              var config = { template: template, label: label, width: width, align: align };

              if (!gantt.config.columnsSet)
                  gantt.config.columnsSet = gantt.config.columns = [];

              if (!gantt.config.columns.length)
                  config.tree = true;
              gantt.config.columns.push(config);

          }
      };
  }]).directive('ganttColumnAdd', ['$filter', function ($filter) {
      return {
          restrict: 'AE',
          terminal: true,
          link: function () {
              gantt.config.columns.push({ width: 45, name: "add" });
          }
      }
  }]);

function templateHelper($element) {
    var template = $element[0].innerHTML;
    return template.replace(/[\r\n]/g, "").replace(/"/g, "\\\"").replace(/\{\{task\.([^\}]+)\}\}/g, function (match, prop) {
        if (prop.indexOf("|") != -1) {
            var parts = prop.split("|");
            return "\"+gantt.aFilter('" + (parts[1]).trim() + "')(task." + (parts[0]).trim() + ")+\"";
        }
        return '"+task.' + prop + '+"';
    });
}

function setScaleConfig(value) {
    switch (value) {
        case "1":
            gantt.config.scale_unit = "day";
            gantt.config.step = 1;
            gantt.config.date_scale = "%d %M";
            gantt.config.subscales = [];
            gantt.config.scale_height = 27;
            gantt.templates.date_scale = null;
            break;
        case "2":
            var weekScaleTemplate = function (date) {
                var dateToStr = gantt.date.date_to_str("%d %M");
                var endDate = gantt.date.add(gantt.date.add(date, 1, "week"), -1, "day");
                return dateToStr(date) + " - " + dateToStr(endDate);
            };

            gantt.config.scale_unit = "week";
            gantt.config.step = 1;
            gantt.templates.date_scale = weekScaleTemplate;
            gantt.config.subscales = [
                { unit: "day", step: 1, date: "%D" }
            ];
            gantt.config.scale_height = 50;
            break;
        case "3":
            gantt.config.scale_unit = "month";
            gantt.config.date_scale = "%F, %Y";
            gantt.config.subscales = [
                { unit: "day", step: 1, date: "%j, %D" }
            ];
            gantt.config.scale_height = 50;
            gantt.templates.date_scale = null;
            break;
        case "4":
            gantt.config.scale_unit = "year";
            gantt.config.step = 1;
            gantt.config.date_scale = "%Y";
            gantt.config.min_column_width = 50;

            gantt.config.scale_height = 90;
            gantt.templates.date_scale = null;


            gantt.config.subscales = [
                { unit: "month", step: 1, date: "%M" }
            ];
            break;
    }
}

