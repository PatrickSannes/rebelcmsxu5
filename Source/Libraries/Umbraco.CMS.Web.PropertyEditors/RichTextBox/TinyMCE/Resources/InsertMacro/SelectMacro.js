/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.Controls.TinyMCE");

(function ($) {

    Umbraco.Controls.TinyMCE.SelectMacro = function() {
        
        var viewModel = {
            macroAlias: ko.observable("")
        };

        return {            
            init: function() {

                //apply knockout js bindings
                ko.applyBindings(viewModel);
            }
        }

    }(); //singleton    

})(jQuery);