function opendialog(executionContext) {

    debugger;
    var params = {};

    Xrm.Utility.openQuickCreate("task", null, params).then(function () { console.log("Success"); }, function (error) {
        console.log(error.message);
    });
}