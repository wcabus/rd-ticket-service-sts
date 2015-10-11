'use strict';

/**
 * @ngdoc function
 * @name uiApp.controller:AboutCtrl
 * @description
 * # AboutCtrl
 * Controller of the uiApp
 */
angular.module('uiApp')
  .controller('WebAPiCtrl', function ($scope, $http) {
      $scope.customers = null;
    
    $scope.httpCall = function() {
    	$http.get('https://localhost:44300/api/customers').success(function(data, status, headers, config) {
    		$scope.customers = data;
    	}).error(function(data, status, headers, config) {

    	});
    };
  });
