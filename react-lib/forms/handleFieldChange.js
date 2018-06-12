function handleFieldChange(context, event) {
    var name = event.target.name;
    var value = event.target.value;

    context.setState({
        [name]: value
    });
}

export default handleFieldChange;