"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

function handleFieldChange(context, event) {
    var name = event.target.name;
    var value = event.target.value;

    context.setState(_defineProperty({}, name, value));
}

exports.default = handleFieldChange;