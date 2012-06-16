/// <reference path="../../Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {

    Umbraco.PropertyEditors.RichTextBox = function($elem, o) {

        //default options combined with specified options
        var _opts = $.extend({
                currNodeId: null,
                macroContentAjaxUrl: "",
                propertyId: null,
                //these are all of the buttons that tiny mce has, can be overriden by options
                buttons: "newdocument,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,umbracostyles,styleselect,formatselect,fontselect,fontsizeselect,|,cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,umbracolink,link,unlink,umbracoanchor,anchor,image,cleanup,help,code,|,insertdate,inserttime,preview,|,forecolor,backcolor,|,tablecontrols,|,hr,removeformat,visualaid,|,sub,sup,|,umbracocharmap,charmap,emotions,iespell,media,advhr,|,print,|,ltr,rtl,|,fullscreen,|,insertlayer,moveforward,movebackward,absolute,|,styleprops,|,cite,abbr,acronym,del,ins,attribs,|,visualchars,nonbreaking,pagebreak,|,umbracomacro,umbracomedia",
                textareaId: $elem.attr("id"),
                //these are all of the plugisn that tiny mce has, can be overriden by options
                plugins: "pagebreak,style,layer,table,advhr,advimage,advlink,emotions,iespell,inlinepopups,insertdatetime,preview,media,searchreplace,print,paste,directionality,fullscreen,noneditable,visualchars,nonbreaking,xhtmlxtras,advlist,umbracolink,umbracoanchor,umbracostyles,umbracocharmap,umbracomacro,umbracosave,umbracomedia",
                width: 500,
                height: 500,
                stylesheets: "",
                styles: "",
                showContextMenu: false,
                tinyMceControllerPaths: { },
                baseUrl: ""
            }, o);

        //create a property editor object
        var propertyEditor = new Umbraco.PropertyEditors.PropertyEditor(_opts.propertyId);

        //the tiny mce config that gets created
        var config = { };

        //register this property editor
        Umbraco.PropertyEditors.PropertyEditorManager.getInstance().registerPropertyEditor(propertyEditor,
            function(ctx) {
                //disable the UI Panel so it isn't shown
                ctx.enablePanel = false;
            });



        function preInitTinyMCE() {
            ///<summary>
            /// This method is called before all TinyMCE initialization.
            /// We need to tell tinymce that we've already loaded all of the plugins and language files that it thinks it needs
            /// and then also set it's base URL so that it loads everything from the correct spot since we're compressing the output
            ///</summary>
                            
            //currently, we just need to tell it that we've already loaded the language file... other plugins seem to be detected by the tinymce system and not 
            //loaded by default, but we can put anything we want in here to tell tiny mce to not load files.
            var files = ["langs/en.js"];

            tinymce.baseURI = new tinymce.util.URI(tinymce.baseURI.protocol + "://" + tinymce.baseURI.authority + _opts.baseUrl);
            tinymce.baseURL = tinymce.baseURI.toAbsolute();
            for (var i = 0; i < files.length; i++) {
                tinymce.ScriptLoader.markDone(tinymce.baseURI.toAbsolute(files[i]));
            }
        }


        return {

            create: function() {

                $(Umbraco.PropertyEditors.PropertyEditorManager.getInstance()).bind("setFocus", function(e, context) {
                    ///<summary>handle the setFocus event and hide the toolbar when it's not a current context</summary>

                    if (context == null || context.id != propertyEditor.context.id) {
                        $(".umbracoSkin .mceExternalToolbar[id^='" + propertyEditor.id + "']").hide();
                    }
                });

                //get all of the ui-elements for this property editor that we've registered
                var buttons = Umbraco.PropertyEditors.PropertyEditorManager.getInstance().getPropertyEditorUIElements(propertyEditor.id);
                var buttonAliases = [];
                for (var b in buttons) {
                    buttonAliases.push(buttons[b].alias);
                }
                //ok, now we have to filter out the tiny mce buttons we don't want!
                var tinyMceBtnParts = _opts.buttons.split(',');
                //an array of the buttons that have been declared in the pre-value editor
                var btnsToKeep = [];
                for (var i = 0; i < tinyMceBtnParts.length; i++) {
                    if (tinyMceBtnParts[i] != "|") {
                        if ($.inArray(tinyMceBtnParts[i], buttonAliases) != -1) {
                            btnsToKeep.push(tinyMceBtnParts[i]);
                        }
                    }
                    else {
                        //keep the seperators
                        if (btnsToKeep[btnsToKeep.length - 1] != "|") {
                            btnsToKeep.push(tinyMceBtnParts[i]);
                        }
                    }
                }
                //now, put the buttons back together as a comma seperated string to pass to TinyMCE
                var tinyMceBtns = btnsToKeep.join(",");

                //we must pre-init tinymce before even the preInit event of tinymce is fired
                preInitTinyMCE();

                //create the config for this instance of tiny mce
                config = {
                    mode: "exact",
                    elements: _opts.textareaId,

                    //********* CUSTOM UMBRACO OPTIONS/PROPERTIES HERE ****************

                    //We set tiny mce url path in config to reference it in plugins
                    umbraco_mce_controller_paths: _opts.tinyMceControllerPaths,
                    //set the current node id
                    umbraco_curr_node_id: _opts.currNodeId,
                    //set the ajax url to get macro contents as string from
                    umbraco_macro_contents_ajax_url: _opts.macroContentAjaxUrl,


                    //*********** END CUSTOM UMBRACO OPTIONS/PROPERTIES**************

                    //by default this will be an empty string unless an Umbraco user overrides it. if it is blank it will use TinyMCE's defaults
                    valid_elements: _opts.validElements == "" ? "@[id|class|style|title|dir<ltr?rtl|lang|xml::lang|onclick|ondblclick|"
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
                    + "textarea[cols|rows|disabled|name|readonly],tt,var,big" : _opts.validElements,


                    width: _opts.width,
                    height: _opts.height,
                
			        // General options
                    theme: "advanced",
                    skin: "umbraco",
                    inlinepopups_skin: "umbraco",
                    plugins: _opts.plugins + ((_opts.showContextMenu) ? ",contextmenu" : ""),

                    content_css: _opts.stylesheets,
                    theme_umbraco_styles: _opts.styles,

                    theme_advanced_buttons1: tinyMceBtns,
                    theme_advanced_buttons2: "",
                    theme_advanced_buttons3: "",
                        
                    //TO SHOW THE ORIGINAL TOOLBAR, COMMENT OUT "external" and uncomment the two lines following it
                    theme_advanced_toolbar_location: "external",
                    //theme_advanced_toolbar_location : "top",
                    //theme_advanced_toolbar_align : "left",


                    setup: function(ed) {
                        var initToolbar = function(ed) {
                            ///<summary>Handles the click event for the editor</summary>

                            //we need to ensure that the correct config is assigned to this instance if we have multiple configs
                            //tinyMCE.activeEditor.settings = config;
                            //tinyMCE.activeEditor.execCommand('mceAddControl', true, _opts.textareaId);

                            if(ed.editorId == propertyEditor.id + "_Value") 
                            {
                                //on editor click handler, move the toolbar to where it needs to be and show it
                                var movedToolbarInstance = $("#editorBar .container .umbracoSkin").find("*[id^='" + propertyEditor.id + "']")
                                if (movedToolbarInstance.length == 0) {
                                    //we haven't moved the toolbar instance yet, so lets do that now, first create a container
                                    var panelContainer = $("<div class='ui-panel umbracoSkin'/>");
                                    //get the tinymce toolbar for this instance
                                    var tinyMceToolbar = $(".umbracoSkin .mceExternalToolbar[id^='" + propertyEditor.id + "']")
                                    //now move it
                                    panelContainer.appendTo($("#editorBar .container")).append(tinyMceToolbar);
                                }
                                
                                $(".umbracoSkin .mceExternalToolbar[id^='" + propertyEditor.id + "']").show();
                                
                                //tell the prop editor mgr that tiny mce is active
                                Umbraco.PropertyEditors.PropertyEditorManager.getInstance().setFocus(null, propertyEditor);
                            }
                        };
                        ed.onInit.add(function(ed) {
                            ///<summary>Initialization handler for when TinyMCE initializes. We need to bind to its window manager events</summary>

                            ed.windowManager.onOpen.add(function(ed) {
                                ///<summary>Adds event handler to the window manager open event</summary>
                                Umbraco.System.WindowManager.getInstance().toggleTopWindowOverlay(true);
                            });
                            ed.windowManager.onClose.add(function(ed) {
                                ///<summary>Adds event handler to the window manager close event</summary>
                                Umbraco.System.WindowManager.getInstance().toggleTopWindowOverlay(false);
                            });
                        });
                        ed.onClick.add(initToolbar);
                        ed.onMouseUp.add(initToolbar);
                    }

                    //Original options:

                    /* Theme options
			        theme_advanced_buttons1 : "save,newdocument,|,bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,styleselect,formatselect,fontselect,fontsizeselect",
			        theme_advanced_buttons2 : "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,anchor,image,cleanup,help,code,|,insertdate,inserttime,preview,|,forecolor,backcolor",
			        theme_advanced_buttons3 : "tablecontrols,|,hr,removeformat,visualaid,|,sub,sup,|,charmap,emotions,iespell,media,advhr,|,print,|,ltr,rtl,|,fullscreen",
			        theme_advanced_buttons4 : "insertlayer,moveforward,movebackward,absolute,|,styleprops,|,cite,abbr,acronym,del,ins,attribs,|,visualchars,nonbreaking,pagebreak",
			        theme_advanced_toolbar_location : "top",
			        theme_advanced_toolbar_align : "left",
			        theme_advanced_statusbar_location : "bottom",
			        theme_advanced_resizing : true,*/
                    // Example content CSS (should be your site CSS)
                    //content_css : "css/content.css",
                    /* Drop lists for link/image/media/template dialogs
			        template_external_list_url : "lists/template_list.js",
			        external_link_list_url : "lists/link_list.js",
			        external_image_list_url : "lists/image_list.js",
			        media_external_list_url : "lists/media_list.js" */
                };




                //creates the TinyMCE editor
                tinyMCE.init(config);
            }

        };

    };

    //jquery plugin to support our editor
    $.fn.RichTextBox = function(o) {
        return $(this).each(function() {
            var rte = new Umbraco.PropertyEditors.RichTextBox($(this), o);
            rte.create();
        });
    };

})(jQuery);