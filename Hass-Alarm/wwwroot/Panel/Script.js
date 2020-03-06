$(document).ready(function (e) {
    $(function () {
        var pin = "";
        var numbers = $('#numberInputs .numberinput');
        $('.btn-lock').click(Lock);
        $('.cmdButton').click(function () {
            var actionName = $(this).data('state');
            action(actionName);

        });
        $(".content").click(function () {
            var $this = $(this);
            var $number = $this.find(".number");
            var value = $number.text();

            if ($number.hasClass("accept")) {
                action(actions.unlock);
                ClearPin();
            }
            else if ($number.hasClass("delete")) {
                if (pin.length > 0)
                    pin = pin.substr(0, pin.length - 1);
                $($(".numberinput").get().reverse()).each(function () {
                    var a = $(this).text();
                    if (a) {
                        $(this).text("");
                        $(this).removeClass("nocircle");
                        return false;
                    }
                });
            }
            else if (value !== "<" && pin.length < numbers.length) {
                pin += value;
                $(".numberinput").each(function () {
                    var a = $(this).text();
                    if (!a) {
                        $(this).text("*");
                        $(this).addClass("nocircle");
                        return false;
                    }
                });
            }

            $('#pin-code').val(pin);
        });
    });
    function displayState(name) {
        var StateIcon = $('#StateIcon');
        var StateText = $('#StateText');
        var StateColor = $('#StateColor');

        if (name != actions.unlock) {
            switch (name) {
                case actions.arm:
                    StateIcon.attr('class', 'mdi mdi-bell-alert');
                    StateText.text('Armé');
                    StateColor.attr('class', "text-danger");
                    break;

                case actions.arm_home:
                    StateIcon.attr('class', 'mdi mdi-bell-circle');
                    StateText.text('Armé à la maison');
                    StateColor.attr('class', "text-warning");
                    break;

                case actions.disarm:
                    StateIcon.attr('class', 'mdi mdi-bell-off');
                    StateText.text('Désarmé');
                    StateColor.attr('class', "text-success");
                    break;
                default:
                    break;
            }
        }

    }
    function action(name) {
        $('#action-field').val(name);
        $('#send').click();
    }
    function ClearPin() {
        numbers.removeClass("nocircle");
        pin = "";
    }
    var actions = {
        arm: "arm",
        disarm: "disarm",
        arm_home: "arm_home",
        unlock: "unlock"
    }
    var lockTimerHandler = null;
    function refreshLock() {
        if (lockTimerHandler)
            clearTimeout(lockTimerHandler);
        lockTimerHandler = setTimeout(Lock, 30000);
    }

    function Unlock() {
        $('.UnLocked').removeClass('hidden');
        $('.Locked').addClass('hidden');
        refreshLock();
    }
    function Lock() {
        if (lockTimerHandler)
            clearTimeout(lockTimerHandler);
        lockTimerHandler = null;
        ClearPin();
        $('#pin-code').val("");
        $('.UnLocked').addClass('hidden');
        $('.Locked').removeClass('hidden');
    }
    function refreshDisplay(response) {
        ClearPin();
        if (!isAuthorized(response))
            Lock();
        else {
            Unlock();
            displayState(response.State);
        }

    }
    function isAuthorized(response) {
        return response.authorized == true;
    }
    displayState($('#startup-state').val());
    $("form[ajax=true]").submit(function (e) {

        e.preventDefault();

        var form_data = $(this).serialize();
        var form_url = $(this).attr("action");
        var form_method = $(this).attr("method").toUpperCase();

        $("#loadingimg").show();

        $.ajax({
            url: form_url,
            type: form_method,
            data: form_data,
            cache: false,
            success: function (returnhtml) {
                $("#result").html(returnhtml);
                $("#loadingimg").hide();
            }
        });

    });

});