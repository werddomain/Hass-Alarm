var Panel = /** @class */ (function () {
    function Panel() {
        var _this = this;
        this.actions = {
            arm: "arm",
            disarm: "disarm",
            arm_home: "arm_home",
            unlock: "unlock"
        };
        this.lockTimerHandler = null;
        var me = this;
        this.pin = "";
        this.numbers = $('#numberInputs .numberinput');
        $('.btn-lock').click(function () { _this.Lock(); });
        $('.cmdButton').click(function () {
            var actionName = $(this).data('state');
            me.action(actionName);
        });
        $(".content").click(function (e) {
            var $this = $(e.target);
            var $number = $this.find(".number");
            var value = $number.text();
            if ($number.hasClass("accept")) {
                _this.action(_this.actions.unlock);
                _this.ClearPin();
            }
            else if ($number.hasClass("delete")) {
                if (_this.pin.length > 0)
                    _this.pin = _this.pin.substr(0, _this.pin.length - 1);
                $($(".numberinput").get().reverse()).each(function () {
                    var a = $(this).text();
                    if (a) {
                        $(this).text("");
                        $(this).removeClass("nocircle");
                        return false;
                    }
                });
            }
            else if (value !== "<" && _this.pin.length < _this.numbers.length) {
                _this.pin += value;
                $(".numberinput").each(function () {
                    var a = $(this).text();
                    if (!a) {
                        $(this).text("*");
                        $(this).addClass("nocircle");
                        return false;
                    }
                });
            }
            $('#pin-code').val(_this.pin);
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
    Panel.prototype.displayState = function (name) {
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
    };
    Panel.prototype.action = function (name) {
        $('#action-field').val(name);
        $('#send').click();
    };
    Panel.prototype.ClearPin = function () {
        this.numbers.removeClass("nocircle");
        this.pin = "";
    };
    Panel.prototype.refreshLock = function () {
        var _this = this;
        if (this.lockTimerHandler)
            clearTimeout(this.lockTimerHandler);
        this.lockTimerHandler = setTimeout(function () { _this.Lock(); }, 30000);
    };
    Panel.prototype.Unlock = function () {
        $('.UnLocked').removeClass('hidden');
        $('.Locked').addClass('hidden');
        this.refreshLock();
    };
    Panel.prototype.Lock = function () {
        if (this.lockTimerHandler)
            clearTimeout(this.lockTimerHandler);
        this.lockTimerHandler = null;
        this.ClearPin();
        $('#pin-code').val("");
        $('.UnLocked').addClass('hidden');
        $('.Locked').removeClass('hidden');
    };
    Panel.prototype.refreshDisplay = function (response) {
        this.ClearPin();
        if (!this.isAuthorized(response))
            this.Lock();
        else {
            this.Unlock();
            this.displayState(response.State);
        }
    };
    Panel.prototype.isAuthorized = function (response) {
        return response.authorized == true;
    };
    return Panel;
}());
var p = new Panel();
//# sourceMappingURL=Script.js.map