"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;

var _react = _interopRequireWildcard(require("react"));

var _propTypes = _interopRequireDefault(require("prop-types"));

var _reactLib = require("@intechprev/react-lib");

var _ = require(".");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } else { var newObj = {}; if (obj != null) { for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) { var desc = Object.defineProperty && Object.getOwnPropertyDescriptor ? Object.getOwnPropertyDescriptor(obj, key) : {}; if (desc.get || desc.set) { Object.defineProperty(newObj, key, desc); } else { newObj[key] = obj[key]; } } } } newObj.default = obj; return newObj; } }

function _typeof(obj) { if (typeof Symbol === "function" && typeof Symbol.iterator === "symbol") { _typeof = function _typeof(obj) { return typeof obj; }; } else { _typeof = function _typeof(obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; }; } return _typeof(obj); }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _possibleConstructorReturn(self, call) { if (call && (_typeof(call) === "object" || typeof call === "function")) { return call; } return _assertThisInitialized(self); }

function _assertThisInitialized(self) { if (self === void 0) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return self; }

function _getPrototypeOf(o) { _getPrototypeOf = Object.setPrototypeOf ? Object.getPrototypeOf : function _getPrototypeOf(o) { return o.__proto__ || Object.getPrototypeOf(o); }; return _getPrototypeOf(o); }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function"); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, writable: true, configurable: true } }); if (superClass) _setPrototypeOf(subClass, superClass); }

function _setPrototypeOf(o, p) { _setPrototypeOf = Object.setPrototypeOf || function _setPrototypeOf(o, p) { o.__proto__ = p; return o; }; return _setPrototypeOf(o, p); }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var InputMask = require('react-input-mask');

var CampoTexto =
/*#__PURE__*/
function (_Component) {
  _inherits(CampoTexto, _Component);

  function CampoTexto() {
    _classCallCheck(this, CampoTexto);

    return _possibleConstructorReturn(this, _getPrototypeOf(CampoTexto).apply(this, arguments));
  }

  _createClass(CampoTexto, [{
    key: "render",
    value: function render() {
      var _this = this;

      var col = "col-lg-2";
      if (this.props.col) col = this.props.col;
      return _react.default.createElement(_.Row, {
        formGroup: true
      }, this.props.label && _react.default.createElement("div", {
        className: col + " col-md-12 text-lg-right col-form-label"
      }, _react.default.createElement("b", null, _react.default.createElement("label", {
        htmlFor: this.props.nome
      }, this.props.label, this.props.obrigatorio && " *"))), _react.default.createElement(_.Col, null, _react.default.createElement(InputMask, {
        mask: this.props.mascara,
        name: this.props.nome,
        value: this.props.valor,
        maxLength: this.props.max,
        className: "form-control",
        type: this.props.tipo,
        placeholder: this.props.placeholder,
        id: this.props.nome,
        disabled: this.props.desabilitado,
        onChange: function onChange(e) {
          return (0, _reactLib.handleFieldChange)(_this.props.contexto, e, _this.props.parent);
        }
      })));
    }
  }]);

  return CampoTexto;
}(_react.Component);

exports.default = CampoTexto;

_defineProperty(CampoTexto, "propTypes", {
  col: _propTypes.default.string,
  obrigatorio: _propTypes.default.bool,
  label: _propTypes.default.string,
  nome: _propTypes.default.string,
  contexto: _propTypes.default.object,
  parent: _propTypes.default.object,
  desabilitado: _propTypes.default.bool,
  max: _propTypes.default.number,
  min: _propTypes.default.number,
  valor: _propTypes.default.string,
  placeholder: _propTypes.default.string,
  mascara: _propTypes.default.string,
  tipo: _propTypes.default.string
});