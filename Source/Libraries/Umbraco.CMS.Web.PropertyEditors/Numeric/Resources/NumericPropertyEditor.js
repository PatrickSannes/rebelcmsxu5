/// <reference path="../../Scripts/Umbraco.System/NamespaceManager.js" />
/// <reference path="../../Scripts/Umbraco.Umbraco.System/UrlEncoder.js" />

Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {

    // Singleton IntegerPropertyEditor class to encapsulate the management of Integer PropertyEditors.
    Umbraco.PropertyEditors.NumericPropertyEditor = function () {

        function getCaretRange (ctrl) {
	        var caretPos = { start : 0, end : 0 };	
            // IE Support
	        if (document.selection) {
	            ctrl.focus();
	            var sel = document.selection.createRange();
                var temp = sel.duplicate();
                temp.moveToElementText(ctrl);
                temp.setEndPoint("EndToEnd", sel);
                caretPos.end = temp.text.length;
                caretPos.start = caretPos.end - sel.text.length;
	        }
	        // Firefox support
	        else if (ctrl.selectionStart || ctrl.selectionStart == '0')
	        {
	            caretPos.start = ctrl.selectionStart;
	            caretPos.end = ctrl.selectionEnd;
	        }
            
            return   caretPos;
        }
        
        function getAttemptedValue(ctrl, key) {
            var value = $(ctrl).val();
            var carretRange = getCaretRange(ctrl);
            var attemptedValue = value.substr(0, carretRange.start) + key + value.substr(carretRange.end);
            return attemptedValue;
        }
        
        return {

            init: function (_opts) {
                var _this = this;
                var regxPattern = "^[0-9]*?" + ((_opts.decimalPlaces > 0) ? "(\.[0-9]{0," + _opts.decimalPlaces + "})?$" : "$");
                var _regex = new RegExp(regxPattern, "ig");
                var propertyEditor = new Umbraco.PropertyEditors.PropertyEditor(_opts.umbracoPropertyId);
                Umbraco.PropertyEditors.PropertyEditorManager.getInstance().registerPropertyEditor(propertyEditor, function (ctx) {
                    $('#' + _opts.fieldId).keydown(function(e) {
                        // If delete / backspace / arrows, just continue
                        if(e.keyCode == 8 || e.keyCode == 9 || e.keyCode == 46 || (e.keyCode >= 37 && e.keyCode <= 40)) {
                            return;
                        }
                        // If it's not a number or decimal point, just fail straight away
                        if(!((e.keyCode >= 48 && e.keyCode <= 57) || (e.keyCode >= 96 && e.keyCode <= 105) || ((e.keyCode == 190 || e.keyCode == 110) && _opts.decimalPlaces > 0))) {
                            e.preventDefault();
                            return;
                        }
                        // Convert numpad keypresses to regular keypresses
                        var code = e.keyCode;
                        if(code >= 96 && code <= 105) 
                            code -= 48;
                        if(code == 110)
                            code = 190;
                        // Calculate and test attempted value
                        var attemptedValue = getAttemptedValue(this, String.fromCharCode(code));
                        if(!$.trim(attemptedValue).match(_regex)) {
                            e.preventDefault();
                            return;
                        }
                    });
                });
            }

        }

    } ();

})(jQuery);

