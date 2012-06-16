/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {

    Umbraco.PropertyEditors.RTEPreValueEditor = function() {
        
        var _opts = null;

        var defaults = "@[id|class|style|title|dir<ltr?rtl|lang|xml::lang|onclick|ondblclick|"
+ "onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|"
+ "onkeydown|onkeyup],a[rel|rev|charset|hreflang|tabindex|accesskey|type|"
+ "name|href|target|title|class|onfocus|onblur],strong/b,em/i,strike,u,"
+ "#p,-ol[type|compact],-ul[type|compact],-li,br,img[longdesc|usemap|"
+ "src|border|alt=|title|hspace|vspace|width|height|align],-sub,-sup,"
+ "-blockquote,-table[border=0|cellspacing|cellpadding|width|frame|rules|"
+ "height|align|summary|bgcolor|background|bordercolor],-tr[rowspan|width|"
+ "height|align|valign|bgcolor|background|bordercolor],tbody,thead,tfoot,"
+ "#td[colspan|rowspan|width|height|align|valign|bgcolor|background|bordercolor"
+ "|scope],#th[colspan|rowspan|width|height|align|valign|scope],caption,-div,"
+ "-span,-code,-pre,address,-h1,-h2,-h3,-h4,-h5,-h6,hr[size|noshade],-font[face"
+ "|size|color],dd,dl,dt,cite,abbr,acronym,del[datetime|cite],ins[datetime|cite],"
+ "object[classid|width|height|codebase|*],param[name|value|_value],embed[type|width"
+ "|height|src|*],script[src|type],map[name],area[shape|coords|href|alt|target],bdo,"
+ "button,col[align|char|charoff|span|valign|width],colgroup[align|char|charoff|span|"
+ "valign|width],dfn,fieldset,form[action|accept|accept-charset|enctype|method],"
+ "input[accept|alt|checked|disabled|maxlength|name|readonly|size|src|type|value],"
+ "kbd,label[for],legend,noscript,optgroup[label|disabled],option[disabled|label|selected|value],"
+ "q[cite],samp,select[disabled|multiple|name|size],small,"
+ "textarea[cols|rows|disabled|name|readonly],tt,var,big";

        //knockout js view model 
        var rtePreValueViewModel = {
            showAdvanced: ko.observable(false),
            toggleAdvanced: function() {
                if (this.showAdvanced()) {
                    //if there's nothing actually set, set back to null if it hasn't changed:
                    var val = $("#rtePreValueEditor textarea").text();
                    if (!_opts.showAdvanced && val == defaults) {
                        $("#rtePreValueEditor textarea").text("");
                    }
                    this.showAdvanced(false);
                }
                else {
                    //if there's nothing actually set, set the defaults:
                    var val = $("#rtePreValueEditor textarea").text();
                    if (!_opts.showAdvanced && val == "") {
                        $("#rtePreValueEditor textarea").text(defaults);
                    }
                    this.showAdvanced(true);
                }
            }
        }
       
        return {
            init: function (o) {

                _opts = o;            
                
                rtePreValueViewModel.showAdvanced(_opts.showAdvanced);

                //apply knockout js bindings
                ko.applyBindings(rtePreValueViewModel, document.getElementById("rtePreValueEditor"));
            }
        }

    }(); //singleton

})(jQuery);