"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _axios = require("axios");

var _axios2 = _interopRequireDefault(_axios);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var BaseService = function () {
    function BaseService(config) {
        _classCallCheck(this, BaseService);

        this.config = config;
    }

    _createClass(BaseService, [{
        key: "GetToken",
        value: function GetToken() {
            return new Promise(function (resolve, reject) {
                try {
                    try {
                        var ReactNative = require("react-native");
                        ReactNative.AsyncStorage.getItem("token", function (err, result) {
                            return err ? reject(err) : resolve(result);
                        });
                    } catch (e) {
                        var token = localStorage.getItem("token");
                        resolve(token);
                    }
                } catch (e) {
                    reject(e);
                }
            });
        }
    }, {
        key: "CriarRequisicao",
        value: function CriarRequisicao(tipo, url) {
            var _this = this;

            var data = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : null;

            return new Promise(function (resolve, reject) {
                _this.GetToken().then(function (token) {

                    (0, _axios2.default)({
                        method: tipo,
                        url: _this.config.apiUrl + url,
                        data: data,
                        headers: {
                            "Authorization": "Bearer " + token
                        }
                    }).then(resolve).catch(reject);
                });
            });
        }
    }, {
        key: "CriarRequisicaoBlob",
        value: function CriarRequisicaoBlob(tipo, url) {
            var _this2 = this;

            var data = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : null;

            return new Promise(function (resolve, reject) {
                _this2.GetToken().then(function (token) {

                    (0, _axios2.default)({
                        method: tipo,
                        url: _this2.config.apiUrl + url,
                        data: data,
                        headers: {
                            "Authorization": "Bearer " + token
                        },
                        responseType: 'blob'
                    }).then(resolve).catch(reject);
                });
            });
        }
    }]);

    return BaseService;
}();

exports.default = BaseService;