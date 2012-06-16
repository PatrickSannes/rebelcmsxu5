/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />
/// <reference path="/Areas/Umbraco/Scripts/Umbraco.Umbraco.System/UrlEncoder.js" />

Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($, Base) {

    // Singleton IntegerPropertyEditor class to encapsulate the management of Integer PropertyEditors.
    Umbraco.PropertyEditors.MultipleTextstringPropertyEditor = Base.extend({
        
        viewModel: {
            umbracoPropertyId: "",
            textstrings: ko.observableArray([]),
            addRow: function (idx) {
                this.textstrings.splice(idx, 0, { value: ko.observable("") });
                $("#" + this.umbracoPropertyId + " li input[type=text]:eq(" + idx + ")").focus();
            }
        },
            
        constructor: function () {
            this.viewModel.value = ko.dependentObservable(function() {
                var result = [];
                $.each(this.textstrings(), function(idx, itm)
                {
                    result.push(itm.value());
                });
                return result.join("\n");
            }, this.viewModel);
        },
            
        _focusInput: function (input) {
            if ('selectionStart' in input) {
                input.selectionStart = 0;
                input.selectionEnd = input.value.length;
                input.focus();
            }
            else {  // Internet Explorer before version 9
                var inputRange = input.createTextRange ();
                inputRange.moveStart("character", 0);
                inputRange.collapse();
                inputRange.moveEnd("character", input.value.length);
                inputRange.select();
            }  
        },
            
        init: function (o) {

            var _this = this;
            
            this.viewModel.umbracoPropertyId = o.umbracoPropertyId;

            var textstrings = $("#" + o.umbracoPropertyId +"_Value").val().split("\n");
            for(var i = 0; i < textstrings.length; i++) {
                this.viewModel.textstrings.push({ value: ko.observable(textstrings[i]) });
            }

            $(".multiple-textstring-inputs").sortable({
                update: function(event, ui) {
                    //retrieve our actual data item
                    var item = ui.item.tmplItem().data;
                    //figure out its new position
                    var position = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
                    //remove the item and add it back in the right spot
                    if (position >= 0) {
                        _this.viewModel.textstrings.remove(item);
                        _this.viewModel.textstrings.splice(position, 0, item);
                    }
                }
            });
            
            $("#" + o.umbracoPropertyId + " li input[type=text]").live("keydown", function(e) {

                var idx = $(e.target).parent().index();
                var keyCode = e.keyCode;
                var flag = false;

                $(e.target).triggerHandler("change");
                
                // Tab key
                if(keyCode == 9 && !e.shiftKey && idx == _this.viewModel.textstrings().length - 1) {
                    keyCode = 13; // Treat as an enter keypress
                }
                
                // Enter key pressed
                if(keyCode == 13) {
                    _this.viewModel.addRow(idx + 1);
                    flag = true;
                }
                
                // Delete key pressed
                if((keyCode == 8 || keyCode == 46) && $(e.target).val() == "" && _this.viewModel.textstrings().length > 1) {
                    _this.viewModel.textstrings.splice(idx, 1);
                    keyCode = 38; // Treat as up arrow press
                    flag = true;
                }
                
                // Arrow keys pressed
                if(keyCode == 40) { // Down arrow
                    var input = $("#" + _this.viewModel.umbracoPropertyId + " li:eq(" + Math.min(idx + 1, _this.viewModel.textstrings().length - 1) + ") input[type=text]").get(0);
                    _this._focusInput(input);
                    flag = true;
                }
                if(keyCode == 38) { // Up arrow
                    var input = $("#" + _this.viewModel.umbracoPropertyId + " li:eq(" + Math.max(idx - 1, 0) + ") input[type=text]").get(0);
                    _this._focusInput(input);
                    flag = true;
                }
                
                if(flag) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                }
            });
            
            ko.applyBindings(this.viewModel, $("#"+ this.viewModel.umbracoPropertyId).get(0));
        }
            
    }, {
        
        _instance: null,
        
        // Singleton accessor
        getInstance: function () {
            if(this._instance == null)
                this._instance = new Umbraco.PropertyEditors.MultipleTextstringPropertyEditor();
            return this._instance;
        }
        
    });

})(jQuery, base2.Base);

