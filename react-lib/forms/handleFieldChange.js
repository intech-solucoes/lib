function handleFieldChange(context, event) {
    var parent = arguments.length > 2 && arguments[2] !== undefined ? arguments[2] : null;

    var name = event.target.name;
    var value = event.target.value;

    if(parent) {
        var parentObj = context.state[parent];
        parentObj[name] = value;

        context.setState({
            [parent]: parentObj
        });
    } else {
        context.setState({
            [name]: value
        });
    }
}

export default handleFieldChange;