angular.module('ahp').controller('resultCtrl', [
'$scope', 'ahp', 'fileDialog',
function($scope, ahp, fileDialog){

  $scope.solution = ahp.getSolution();
  
  $scope.save = function () {
    fileDialog.saveAs(function(filename){
      var data = ahp.serialize();
      var json = angular.toJson(data);
      var fs = require('fs');
      fs.writeFile(filename, json, 'utf8', function(err) {
        if (err) {
          alert('Під час збереження файлу виникла помилка\n' + err);
        } else {
          alert('Файл успішно збережено');
        }
      });
    }, '', '.JSON');
  };
  
  $scope.load = function() {
    fileDialog.openFile(function(filename){
      var fs = require('fs');
      fs.readFile(filename, 'utf8', function(err, json){
        if (err) {
          alert('Під час зчитування файлу виникла помилка\n' + err);
        } else {
          var data = angular.fromJson(json);
          ahp.deserialize(data);
          alert('Файл успішно завантажено');
          $scope.solution = ahp.getSolution();        
          $scope.$digest();
        }
      });
    }, '.JSON');
  };
  
}]);