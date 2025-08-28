var LoadingSpinner = (function () {

    var _timeout;
    var spinner = "<div id='loading-spinner' class='loading-spinner-container'><div id='loading-spinner-background' class='loading-spinner-bg'></div><div class='spinner-border loading-spinner' role='status'><span class='visually-hidden'>Loading...</span></div></div>";

    function show(delay) {
        if (_timeout != undefined || $("#loading-spinner").length) {
            return;
        }

        if (!Number.isInteger(delay)) {
            delay = 1000;
        }

        _timeout = setTimeout(function () {
            $("body").append($(spinner));
            setTimeout(function () {
                $("#loading-spinner").addClass("opacity-100")
            }, 200);
        }, delay);
    }

    function clear() {
        clearTimeout(_timeout);
        _timeout = undefined;

        $("#loading-spinner").removeClass("opacity-100");
        setTimeout(function () {
            $("#loading-spinner").remove();
        }, 200);
    }

    return {
        Show: show,
        Clear: clear
    };

})();