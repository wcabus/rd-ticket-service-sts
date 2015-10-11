'use strict';

/**
 * @ngdoc function
 * @name uiApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the uiApp
 */
angular.module('uiApp')
  .controller('mainCtrl',
    ['OidcManager', MainCtrl]);

function MainCtrl(OidcManager) {
    var vm = this;

    vm.logOut = function() {
        vm.mgr.removeToken();
        window.location = 'index.html';
    }

    vm.logOutOfIdSrv = function() {
        vm.mgr.redirectForLogout();
    }

    vm.mgr = OidcManager.OidcTokenManager();

    if (vm.mgr.expired) {
        vm.mgr.redirectForToken();
    }
}
