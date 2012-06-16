(function ($) {

    $.fn.colorSwatchPicker = function (options) {
        //create the color selector
        if ($("div#color_selector").length == 0)
            buildSelector();

        //create the color pickers
        return this.each(function (i) {
            buildPicker(this)
        });
    };

    var selectorOwner;
    var selectorShowing = false;

    $(Umbraco.PropertyEditors.PropertyEditorManager.getInstance()).bind("setFocus", function (e, context) {
        hideSelector();
    });

    buildPicker = function (element, settings) {
        //build color picker
        control = $("<div class='color_picker'>&nbsp;</div>")
        control.css('background-color', $(element).val());

        //parse colors
        control.data('colors', []);
        $("option", element).each(function (i) {
            control.data('colors').push($(this).val());
        });

        //bind click event to color picker
        control.bind("click", toggleSelector);

        //add the color picker section
        $(element).after(control);

        //add even listener to input box
        $(element).bind("change", function () {
            selectedValue = toHex($(element).val());
            $(element).next(".color_picker").css("background-color", selectedValue);
        });

        //hide the input box
        $(element).hide();
    };

    buildSelector = function () {
        //build color selector
        selector = $("<div id='color_selector'></div>");

        //add the color selector section
        $("body").append(selector);

        //hide the color selector
        selector.hide();
    };

    createSwatches = function () {
        //remove previous swatches
        $("div.color_swatch").unbind('click').unbind('mouseover').unbind('mouseout');
        selector.empty();

        //add color swatches
        $.each($(selectorOwner).data('colors'), function (i) {
            swatch = $("<div class='color_swatch'>&nbsp;</div>")
            swatch.css("background-color", this);
            swatch.bind("click", function (e) { changeColor($(this).css("background-color")) });
            swatch.bind("mouseover", function (e) {
                $(this).addClass("color_swatch_hover");
                $("input#color_value").val(toHex($(this).css("background-color")));
            });
            swatch.bind("mouseout", function (e) {
                $(this).removeClass("color_swatch_hover");
                $("input#color_value").val(toHex($(selectorOwner).css("background-color")));
            });
            swatch.appendTo(selector);
        });
    }

    checkMouse = function (event) {
        //check the click was on selector itself or on selectorOwner
        var selector = "div#color_selector";
        var selectorParent = $(event.target).parents(selector).length;
        if (event.target == $(selector)[0] || event.target == selectorOwner || selectorParent > 0) return

        //if we get this far, we didn't click on the selector, so hide it
        hideSelector();
    }

    hideSelector = function () {
        var selector = $("div#color_selector");
        $(document).unbind("mousedown", checkMouse);
        selector.hide();
        selectorShowing = false;
    }

    showSelector = function () {
        var selector = $("div#color_selector");

        createSwatches();

        selector.css({
            top: $(selectorOwner).offset().top + ($(selectorOwner).outerHeight()),
            left: $(selectorOwner).offset().left
        });

        selector.show();

        //bind close event handler
        $(document).bind("mousedown", checkMouse);
        selectorShowing = true
    }

    toggleSelector = function (event) {
        selectorOwner = this;
        selectorShowing ? hideSelector() : showSelector();
    }

    changeColor = function (value) {
        if (selectedValue = toHex(value)) {
            $(selectorOwner).css("background-color", selectedValue);
            $(selectorOwner).prev("select").val(selectedValue).change();

            //close the selector
            hideSelector();
        }
        else {
            //tg added support for clearing picker
            $(selectorOwner).removeAttr("style");
            $(selectorOwner).prev("select").val("").change();
            hideSelector();
        }
    };

    //converts RGB string to HEX - inspired by http://code.google.com/p/jquery-color-utils
    toHex = function (color) {
        //valid HEX code is entered
        if (color.match(/[0-9a-fA-F]{3}$/) || color.match(/[0-9a-fA-F]{6}$/)) {
            color = (color.charAt(0) == "#") ? color : ("#" + color);
        }
        //rgb color value is entered (by selecting a swatch)
        else if (color.match(/^rgb\(([0-9]|[1-9][0-9]|[1][0-9]{2}|[2][0-4][0-9]|[2][5][0-5]),[ ]{0,1}([0-9]|[1-9][0-9]|[1][0-9]{2}|[2][0-4][0-9]|[2][5][0-5]),[ ]{0,1}([0-9]|[1-9][0-9]|[1][0-9]{2}|[2][0-4][0-9]|[2][5][0-5])\)$/)) {
            var c = ([parseInt(RegExp.$1), parseInt(RegExp.$2), parseInt(RegExp.$3)]);

            var pad = function (str) {
                if (str.length < 2) {
                    for (var i = 0, len = 2 - str.length; i < len; i++) {
                        str = '0' + str;
                    }
                }
                return str;
            }

            if (c.length == 3) {
                var r = pad(c[0].toString(16)), g = pad(c[1].toString(16)), b = pad(c[2].toString(16));
                color = '#' + r + g + b;
            }
        }
        //not a valid color format
        else color = false;

        return color
    }

})(jQuery);


