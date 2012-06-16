/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.Controls.TinyMCE");

(function ($) {

    Umbraco.Controls.TinyMCE.InsertLink = function() {
        
        var _opts = {
            //the hidden field to track the active tab index
            activeTabIndexField: true,
            //the active tab index to show on load
            activeTabIndex: ""
        };
        
        function setAllAttribs(elm) {
            var dom = tinyMCEPopup.editor.dom;
    
	        if (viewModel.localUrl()) {
	            viewModel.href(viewModel.localUrl());
	            dom.setAttrib(elm, 'data-umbraco-link', 'internal');
	        } else {
	            dom.setAttrib(elm, 'data-umbraco-link', '');
	        }

	        dom.setAttrib(elm, 'href', viewModel.href());
	        dom.setAttrib(elm, 'title', viewModel.title());
	        dom.setAttrib(elm, 'target', viewModel.selectedTarget() == '_self' ? '' : viewModel.selectedTarget());

	        // Refresh in old MSIE
	        if (tinyMCE.isMSIE5)
		        elm.outerHTML = elm.outerHTML;
        }
        
        var contentTreeInitialized = false;
        var mediaTreeInitialized = false;
        
        var viewModel = {
            localUrl: ko.observable(),
            href: ko.observable(),
            title: ko.observable(),
            selectedTarget: ko.observable(),
            selectedAnchor: ko.observable(),
            availableTargets: ko.observableArray(["_self", "_blank", "_parent", "_top"]),
            availableAnchors: ko.observableArray([]),
            insert: function () {
                
                var inst = tinyMCEPopup.editor;
	            var elm, elementArray, i;

	            elm = inst.selection.getNode();
                
                // Check to see if link is external, and doesn't start with http://
                if (/^\s*www\./i.test(viewModel.href()) && confirm("The URL you entered seems to be an external link, do you want to add the required http:// prefix?"))
		            viewModel.href('http://' + viewModel.href());

	            elm = inst.dom.getParent(elm, "A");

	            // Remove element if there is no href
	            if (!document.forms[0].href.value) {
		            tinyMCEPopup.execCommand("mceBeginUndoLevel");
		            i = inst.selection.getBookmark();
		            inst.dom.remove(elm, 1);
		            inst.selection.moveToBookmark(i);
		            tinyMCEPopup.execCommand("mceEndUndoLevel");
		            tinyMCEPopup.close();
		            return;
	            }

	            tinyMCEPopup.execCommand("mceBeginUndoLevel");

	            // Create new anchor elements
	            if (elm == null) {
		            inst.getDoc().execCommand("unlink", false, null);
		            tinyMCEPopup.execCommand("CreateLink", false, "#mce_temp_url#", {skip_undo : 1});

		            elementArray = tinymce.grep(inst.dom.select("a"), function(n) {return inst.dom.getAttrib(n, 'href') == '#mce_temp_url#';});
		            for (i=0; i<elementArray.length; i++) {
		                setAllAttribs(elm = elementArray[i]);
		            }
	            } else {
	                setAllAttribs(elm);
	            }

                // Don't move caret if selection was image
	            if (elm.childNodes.length != 1 || elm.firstChild.nodeName != 'IMG') {
		            inst.focus();
		            inst.selection.select(elm);
		            inst.selection.collapse(0);
		            tinyMCEPopup.storeSelection();
	            }

	            tinyMCEPopup.execCommand("mceEndUndoLevel");
	            tinyMCEPopup.close();

                return false;
            }
        };
        
        return {
            init: function(o) {
                
                _opts = $.extend(_opts, o);
                
                var ed = tinyMCEPopup.editor;

                //override the default tab index if it's zero and the query string exists
                if ($u.Sys.QueryStringHelper.getQueryStringValue("tabindex")) {
                    _opts.activeTabIndex = $u.Sys.QueryStringHelper.getQueryStringValue("tabindex");
                }

                //create the tabs			
                $("#tabs").umbracoTabs({
                    content: "#editorContent",
                    activeTabIndex: _opts.activeTabIndex,
                    activeTabIndexField: _opts.activeTabIndexField,
                    onTabChange: function (data) {
                        if (data.activeIndex == 0 && !contentTreeInitialized) {
                            $("#content-tree").umbracoTreeApi().createJsTree();
                            contentTreeInitialized = true;
                        } else if (data.activeIndex == 1 && !mediaTreeInitialized) {
                            $("#media-tree").umbracoTreeApi().createJsTree();
                            mediaTreeInitialized = true;
                        }
                    }
                });
                
                //populate available anchors collection
                $.each(ed.dom.select('a.mceItemAnchor,img.mceItemAnchor'), function(idx, el) {
                    viewModel.availableAnchors.push(ed.dom.getAttrib(el, "name"));
                });

                //cleanup if url is changed by hand
                $("#href").change(function() {
                    viewModel.localUrl("");
                    if($(this).val().substr(0,1) == "#") {
                        viewModel.selectedAnchor($(this).val().substr(1));
                    } else {
                        viewModel.selectedAnchor(undefined);
                    }
                });

                //set url when an anchor is selected
                $("#anchor").change(function() {
                    viewModel.localUrl("");
                    viewModel.href("#"+ $(this).val());
                });

                tinyMCEPopup.restoreSelection();
                
                // Fixes crash in Safari
                if (tinymce.isWebKit)
                    tinyMCEPopup.editor.getWin().focus();
                
                var node = ed.selection.getNode();
                var el = ed.dom.getParent(node, "A");
                
                //TODO: Need to find out why node is FORM in IE9
                
	            if (el != null && el.nodeName == "A") {

	                viewModel.href(ed.dom.getAttrib(el, 'href'));
	                
	                // TODO: Check for data-umbraco-link attribute
	                var flag = ed.dom.getAttrib(el, 'data-umbraco-link');
	                if(flag == 'internal') {
	                    viewModel.localUrl(viewModel.href());
	                    viewModel.href("#");
	                    $.post(_opts.ajaxUrl, ko.toJSON({ id : viewModel.localUrl() }), function(e) {
	                        viewModel.href(e.niceUrl);
                        });
	                }
	                
	                viewModel.title(ed.dom.getAttrib(el, 'title'));
	                viewModel.selectedTarget(ed.dom.getAttrib(el, 'target'));
	                
	                if(viewModel.href().charAt(0) == '#') {
	                    viewModel.selectedAnchor = viewModel.href().substr(1);
	                }
	            }
                
                //apply knockout js bindings
                ko.applyBindings(viewModel);
                
            },
            onNodeClick: function (e, el) {
                viewModel.localUrl(el.metaData.jsonId.rawValue);
                viewModel.href("#");
                viewModel.title($.trim(el.node[0].innerText));
                viewModel.selectedAnchor(undefined);
                $.post(_opts.ajaxUrl, ko.toJSON({ id : viewModel.localUrl() }), function(e) {
	                viewModel.href(e.niceUrl);
                });
            }
        };
        
    }(); //singleton    

})(jQuery);