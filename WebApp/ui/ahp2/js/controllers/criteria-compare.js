angular.module('ahp').controller('criteriaCompareCtrl', [
'$scope', '$routeParams', 'ahp', '$rootScope',
function($scope, $routeParams, ahp, $scope){

  var criteria = $scope.criteria = ahp.getCriteria(),
      criteriaComparison = ahp.getCriteriaComparison();
  
  $scope.comparison = [];
  for (var i = 0, len = criteria.length; i < len; i++) {
    for (var j = i + 1; j < len; j++) {
      $scope.comparison.push({
        row : i,
        col : j,
        value : criteriaComparison[i][j]
      });
    }
  };
  
  $scope.isComparisonChecked = function(i, j, val) {
    return criteriaComparison[i][j] == val;
  };
  
  $scope.changeComparison = function(i, j, val) {
    criteriaComparison[i][j] = val;
    criteriaComparison[j][i] = 1 / val;
  };
  
}
]);