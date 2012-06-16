/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.Controls.TinyMCE");

(function ($) {

    Umbraco.Controls.TinyMCE.InsertMacro = function () {

        var _opts;

        function insert(macroContent) {
            
            var params = ko.toJSON(_opts.macroParams).base64Encode();
            var macroElement = "<div class='umb-macro-holder' data-macro-alias=\"" + _opts.macroAlias + "\" data-macro-params=\""+ params +"\"><!-- start macro -->" + macroContent + "<!-- end macro --></div>";

            tinyMCEPopup.restoreSelection();

            // Fixes crash in Safari
		    if (tinymce.isWebKit)
			    tinyMCEPopup.editor.getWin().focus();
            
            if (!_opts.isNew) {
                //update the content
                var $content = $(tinyMCEPopup.editor.getContent());
                var output = "";
                //find the current macro alias node in the array of root nodes of tinymce content
                $content.each(function() {
                    //clone the element and wrap with div
                    var cloned = $(this).clone().wrap("<div></div>").parent();
                    //find our macro holder and replace its content
                    var $macro = cloned.find("#" + _opts.inlineMacroId).each(function() {                        
                        //set the id back on the element
                        var $m = $(macroElement).attr("id", _opts.inlineMacroId);
                        //set the content
                        $(this).replaceWith($m.wrap("<div></div>").parent().html());                        
                        
                    });                        
                    output += cloned.html();
                });    
                //set the full content of the editor with the replaced macro
                tinyMCEPopup.editor.setContent(output, {skip_undo : 1});                
            }
            else {                  
                //generate a new Id for the macro
                var $macro = $(macroElement).attr("id", "m_" + Guid.generate());
                var output = $macro.wrap("<div></div>").parent().html();
                //insert new content at the cursor  
                tinyMCEPopup.editor.execCommand('mceInsertContent', false, output, {skip_undo : 1});                    
            }

            tinyMCEPopup.editor.undoManager.add();
            tinyMCEPopup.editor.execCommand('mceRepaint');
		    tinyMCEPopup.editor.focus();                		        
            tinyMCEPopup.close();                
        }
        
        return {
            init: function(o) {

                _opts = $.extend({                    
                    ajaxUrl: $("form").attr("action"),
                    isNew: true
                    }, o);
                
                var data = ko.toJSON(_opts);
                $.post(_opts.ajaxUrl, data, function(e) {
                    insert(e.macroContent);
                });
            }
        };

    } (); //singleton    

})(jQuery);