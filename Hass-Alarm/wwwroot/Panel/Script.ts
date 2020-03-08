class Panel {
    pin: string;
    numbers: JQuery<HTMLElement>;

    constructor() {
        var me = this;
        this.pin = "";
        this.numbers = $('#numberInputs .numberinput');
        $('.btn-lock').click(() => { this.Lock(); });
        $('.cmdButton').click(function () {
            var actionName = $(this).data('state');
            me.action(actionName);

        });
        $(".content").click((e) => {
            var $this = $(e.target);
            var $number = $this.find(".number");
            var value = $number.text();

            if ($number.hasClass("accept")) {
                this.action(this.actions.unlock);
                this.ClearPin();
            }
            else if ($number.hasClass("delete")) {
                if (this.pin.length > 0)
                    this.pin = this.pin.substr(0, this.pin.length - 1);
                $($(".numberinput").get().reverse()).each(function () {
                    var a = $(this).text();
                    if (a) {
                        $(this).text("");
                        $(this).removeClass("nocircle");
                        return false;
                    }
                });
            }
            else if (value !== "<" && this.pin.length < this.numbers.length) {
                this.pin += value;
                $(".numberinput").each(function () {
                    var a = $(this).text();
                    if (!a) {
                        $(this).text("*");
                        $(this).addClass("nocircle");
                        return false;
                    }
                });
            }

            $('#pin-code').val(this.pin);
        });

        this.displayState($('#startup-state').val());
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
    }
    displayState(name) {
        var StateIcon = $('#StateIcon');
        var StateText = $('#StateText');
        var StateColor = $('#StateColor');

        if (name != this.actions.unlock) {
            switch (name) {
                case this.actions.arm:
                    StateIcon.attr('class', 'mdi mdi-bell-alert');
                    StateText.text('Armé');
                    StateColor.attr('class', "text-danger");
                    break;

                case this.actions.arm_home:
                    StateIcon.attr('class', 'mdi mdi-bell-circle');
                    StateText.text('Armé à la maison');
                    StateColor.attr('class', "text-warning");
                    break;

                case this.actions.disarm:
                    StateIcon.attr('class', 'mdi mdi-bell-off');
                    StateText.text('Désarmé');
                    StateColor.attr('class', "text-success");
                    break;
                default:
                    break;
            }
        }

    }
    action(name) {
        $('#action-field').val(name);
        $('#send').click();
    }
    ClearPin() {
        this.numbers.removeClass("nocircle");
        this.pin = "";
    }
    actions = {
        arm: "arm",
        disarm: "disarm",
        arm_home: "arm_home",
        unlock: "unlock"
    }
    lockTimerHandler: number = null;
    refreshLock() {
        if (this.lockTimerHandler)
            clearTimeout(this.lockTimerHandler);
        this.lockTimerHandler = setTimeout(() => { this.Lock(); }, 30000);
    }

    Unlock() {
        $('.UnLocked').removeClass('hidden');
        $('.Locked').addClass('hidden');
        this.refreshLock();
    }
    Lock() {
        if (this.lockTimerHandler)
            clearTimeout(this.lockTimerHandler);
        this.lockTimerHandler = null;
        this.ClearPin();
        $('#pin-code').val("");
        $('.UnLocked').addClass('hidden');
        $('.Locked').removeClass('hidden');
    }
    refreshDisplay(response) {
        this.ClearPin();
        if (!this.isAuthorized(response))
            this.Lock();
        else {
            this.Unlock();
            this.displayState(response.State);
        }

    }
    isAuthorized(response) {
        return response.authorized == true;
    }
}
var p = new Panel();