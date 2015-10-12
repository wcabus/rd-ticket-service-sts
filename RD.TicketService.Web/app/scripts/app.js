'use strict';

/**
 * @ngdoc overview
 * @name uiApp
 * @description
 * # uiApp
 *
 * Main module of the application.
 */
angular
  .module('uiApp', [
    'ngAnimate',
    'ngCookies',
    'ngResource',
    'ngRoute',
    'ngSanitize',
    'ngTouch'
  ])
  .constant("appSettings", {
    RdTicketServiceApi: "https://localhost:44300" 
  })
  .config(function ($routeProvider, $locationProvider, $httpProvider) {
    $locationProvider.html5Mode(true);

    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'mainCtrl'
      })
      .when('/webapi', {
        templateUrl: 'views/webapi.html',
        controller: 'WebAPiCtrl'
      })
      .otherwise({
        redirectTo: '/'
      });

    $httpProvider.interceptors.push(function (appSettings, OidcManager) {
        return {
            'request': function (config) {

                // if it's a request to the API, we need to provide the
                // access token as bearer token.             
                if (config.url.indexOf(appSettings.RdTicketServiceApi) === 0) {
                    config.headers.Authorization = 'Bearer ' + OidcManager.OidcTokenManager().access_token;
                }

                return config;
            }

        };
    });
  })
    .factory('OidcManager', function() {
        var config = {
            client_id: 'RD.TicketService.Web',
            redirect_uri: window.location.protocol + '//' + window.location.host + '/callback.html',
            post_logout_redirect_uri: window.location.protocol + '//' + window.location.host + '/',
            response_type: 'id_token token',
            scope: 'openid profile RD.TicketService.API',
            authority: 'https://localhost:44301/identity'
        };

        var mgr = new OidcTokenManager(config);
        return {
            OidcTokenManager: function() {
                return mgr;
            }
        };
    });
