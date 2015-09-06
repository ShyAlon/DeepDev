angular.module('ahp').controller('criteriaConfigureCtrl', [
'$scope', '$routeParams', 'ahp', '$rootScope',
function($scope, $routeParams, ahp, $scope){

  var criteria = $scope.criteria = ahp.getCriteria()[$routeParams.index];
  var alternatives = $scope.alternatives = ahp.getAlternatives();
  
  $scope.comparison = [];
  for (var i = 0, len = alternatives.length; i < len; i++) {
    for (var j = i + 1; j < len; j++) {
      $scope.comparison.push({
        row : i,
        col : j,
        value : criteria.map[i][j]
      });
    }
  };
  
  $scope.isComparisonChecked = function(i, j, val) {
    return criteria.map[i][j] == val;
  };
  
  $scope.changeComparison = function(i, j, val) {
    criteria.map[i][j] = val;
    criteria.map[j][i] = 1 / val;
  };
  
}
]);