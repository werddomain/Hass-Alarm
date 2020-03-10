class Panel {
    _pin: string;
    private get pin(): string { return this._pin;}
    private set pin(value: string) {
        this._pin = value;
        this.display.val(this._pin);
        $('#pin-code').val(this._pin);

    }
    numbers: JQuery<HTMLElement>;
    display: JQuery;
    constructor() {
        this.display = $('#code');
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
        $('#num-panel .num').click((e) => {
            var num = $(e.target);
            this.pin = this.pin + num.text();
            
        });
        $('#num-panel .back').click((e) => {
            
            this.pin = this.pin.slice(0, -1);
        });
        $('#num-panel .go').click((e) => {
            this.action("unlock");
        });
        
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
                success: (e: app.panelModel, S, R) => {
                    var action = $('#action-field').val();
                    if (e.code_invalid) {
                        me.invalidCode();
                    }
                    else {
                        $('#msg-invalid-code').addClass('.hidden');
                        if (action == me.actions.arm || action == me.actions.arm_home) {
                            me.Lock()
                        }
                        else
                            me.Unlock();
                        me.displayState(e.state);
                    }

                    $("#loadingimg").hide();
                }
            });

        });
        this.displayState($('#msg-state').data("state"));
    }
    bindEvents() {

    }
    invalidCode() {
        $('#msg-invalid-code').removeClass('hidden');
    }
    displayState(name) {
        var StateIcon = $('#msg-state i');
        var StateText = $('#msg-state span');
        var StateColor = $('#msg-state');

        if (name != this.actions.unlock) {
            switch (name) {
                case this.actions.arm:
                    StateIcon.attr('class', 'mdi mdi-bell-alert');
                    StateText.text('Armed');
                    StateColor.attr('class', "alert alert-danger");
                    break;

                case this.actions.arm_home:
                    StateIcon.attr('class', 'mdi mdi-bell-circle');
                    StateText.text('Armed at home');
                    StateColor.attr('class', "alert alert-warning");
                    break;

                case this.actions.disarm:
                    StateIcon.attr('class', 'mdi mdi-bell-off');
                    StateText.text('Disarmed');
                    StateColor.attr('class', "alert alert-success");
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