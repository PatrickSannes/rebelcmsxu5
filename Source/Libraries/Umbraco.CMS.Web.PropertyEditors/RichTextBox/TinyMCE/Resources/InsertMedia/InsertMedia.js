/// <reference path="/Areas/Umbraco/Scripts/Umbraco.System/NamespaceManager.js" />

Umbraco.System.registerNamespace("Umbraco.Controls.TinyMCE");

(function ($) {

    Umbraco.Controls.TinyMCE.InsertMedia = function () {

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

        function showAdditionalParams()
        {
                //only needs to happen when extension is different from previous 
                $("#media-params").show();

                $("#params").children().remove();
                $("#params").append("<div class='progress-spinner'/>");

                $.ajax({
                   type: "POST",
                   dataType:  "json",
                   url: _opts.ajaxAdditionalParamsUrl,
                   data: JSON.stringify({mediaId: viewModel.localUrl()}),
                   success: function(data){
                    
                   
                     if(data.viewParams.length > 0){
                         $("#params").children().remove();
                          $( "#paramTemplate" ).tmpl( data.viewParams ).appendTo( "#params" );
                     }

                   },
                   error: function(xhr, ajaxOptions, thrownError){
                         console.log("Insert Media: Error getting params : " + xhr.status);
                         console.log("Insert Media: Error getting params : " +thrownError);   
                    }
                 });
        }

        function fillPossibleMediaTypesDropdown(mediaId)
        {
            $.ajax({
                type: "POST",
                dataType:  "json",
                url: _opts.ajaxPossibleMediaTypesUrl,
                data: JSON.stringify({mediaId: mediaId}),
                success: function(data){
                    
                    var listItems= "";
                    for (var i = 0; i < data.possibleMediaTypes.length; i++){
                        listItems+= "<option value='" + data.possibleMediaTypes[i].value + "'>" + data.possibleMediaTypes[i].display + "</option>";
                    }
                    $("#docTypeId").html(listItems);
                }
                });
        }
        function showPreview()
        {
                $("#media-preview").children().remove();
                $("#media-preview").append("<div class='progress-spinner'/>");

                $.ajax({
                   type: "POST",
                   dataType:  "html",
                   url: _opts.ajaxPreviewUrl,
                   data: JSON.stringify({mediaId: viewModel.localUrl()}),
                   success: function(data){
                     $("#media-preview").children().remove();
                    
                     $(data).appendTo("#media-preview");
                   },
                   error: function(xhr, ajaxOptions, thrownError){
                         $("#media-preview").children().remove();
                         $("#media-preview").append("<span>Not possible to get preview</span>");
                    }
                 });

        }

        function uploadFile (callback){

            var parentId = viewModel.localUrl();
           

            $uploadContainer = $(".upload");
         
            var $tempForm = $('<form enctype="multipart/form-data" method="post" />').
                hide().
                appendTo("body");
                
            $('<input type="hidden" value="' + parentId + '" name="parentId" />').
                appendTo($tempForm );

            $uploadContainer.
            find("input[type=file]").
            each(function () {
                $(this).attr("name", "file"); 
                var filename = $(this).val().split('\\').pop();
                $('<input type="hidden" value="' + filename + '" name="name" />').
                appendTo($tempForm );
            }).
            end().
            children().
            appendTo($tempForm);

            $uploadContainer.append("<div class='progress-spinner'/>");

            $tempForm.
            ajaxSubmit({
                url: _opts.ajaxUploadHandlerUrl,
                type: "POST",
                dataType: "text",
                success: function (text) {
                   
                    var data = $.parseJSON(text);
                    $("#media-upload").hide();
                    selectMediaItem(data.mediaId, data.title);
                   
                },
                error: function(xhr, ajaxOptions, thrownError){
                    console.log("Insert Media: Error uploading file: " + xhr.status);
                    console.log("Insert Media: Error uploading file: " + thrownError);   
                }
            });
        }

        function selectMediaItem(mediaId, title){
                viewModel.localUrl(mediaId);
                viewModel.href("#");
                viewModel.title(title);
                
                $.post(_opts.ajaxNiceUrlUrl, ko.toJSON({ id: viewModel.localUrl() }), function (e) {
                    viewModel.href(e.niceUrl);
                    
                    
                });
                //additional params
                showAdditionalParams();
                //show preview
                showPreview();
        }
        var mediaTreeInitialized = false;

        var viewModel = {
            localUrl: ko.observable(),
            href: ko.observable(),
            title: ko.observable(),
            insert: function () {

                var inst = tinyMCEPopup.editor;
                var elm, elementArray, i;


               var mediaParameters = {};
               $("#params .property-editor").each(function(index){
                    var name = $("input",this).attr('name');
                    var value = $("input",this).val();
                   
                    mediaParameters[name] = value;
               });

               if(viewModel.localUrl() != null){

                    $.ajax({
                       type: "POST",
                       dataType:  "html",
                       url: _opts.ajaxDisplayTemplateUrl,
                       data: JSON.stringify({mediaId: viewModel.localUrl(), mediaParameters: ko.toJSON(mediaParameters).base64Encode()}),
                       success: function(data){
                          tinyMCEPopup.execCommand("mceBeginUndoLevel");  
                          tinyMCEPopup.editor.execCommand("mceInsertContent", false, data);
                          tinyMCEPopup.execCommand("mceEndUndoLevel");
                          tinyMCEPopup.close();
                          return false;
                       },
                       error: function(xhr, ajaxOptions, thrownError){
                             console.log("Insert Media: Error inserting markup: " + xhr.status);
                             console.log("Insert Media: Error inserting markup: " + thrownError);   
                        }
                     });
                 }  

                return false;
            }
        };

        return {
            init: function (o) {
               
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
                        if (data.activeIndex == 0 && !mediaTreeInitialized) {
                            $("#media-tree").umbracoTreeApi().createJsTree();
                            mediaTreeInitialized = true;
                        }
                    }
                });

               

                //cleanup if url is changed by hand
                $("#href").change(function () {
                    viewModel.localUrl("");
                    if ($(this).val().substr(0, 1) == "#") {
                        viewModel.selectedAnchor($(this).val().substr(1));
                    } else {
                        viewModel.selectedAnchor(undefined);
                    }
                });

               //upload 
               $(".upload").
                   delegate("input[type=file]", "change", function () {
                       // Check if the user is supposed to be manually uploading
                       var manualUpload = $(".upload").find(".upload-button").is(":visible");
                       if ($(this).val() && !manualUpload) {
	                        uploadFile(null);
                       }
                   });

                tinyMCEPopup.restoreSelection();

                // Fixes crash in Safari
                if (tinymce.isWebKit)
                    tinyMCEPopup.editor.getWin().focus();

                var node = ed.selection.getNode();
                var el = ed.dom.getParent(node, "A");


                //apply knockout js bindings
                ko.applyBindings(viewModel);

            },
            onNodeClick: function (e, el) {
                
                $("#media-upload").hide();
                $("#media-params").hide();

                //depending on type of node we'll show upload possiblity or select the item
                $.ajax({
                   type: "POST",
                   dataType:  "json",
                   url: _opts.ajaxAllowsUploadUrl,
                   data: JSON.stringify({mediaId: el.metaData.jsonId.rawValue}),
                   success: function(data){
                    
                     viewModel.localUrl(el.metaData.jsonId.rawValue);

                     if(data.allowsUpload){
                         //show upload 
                         $("#media-preview").children().remove();
                         $("#media-upload").show();
                         fillPossibleMediaTypesDropdown(el.metaData.jsonId.rawValue);
                     }else{
                        //set selected item
                          $.ajax({
                           type: "POST",
                           dataType:  "json",
                           url: _opts.ajaxIsChosenMediaItemValidUrl,
                           data: JSON.stringify({mediaId: el.metaData.jsonId.rawValue}),
                           success: function(data){
                                if(data.valid){
                                    selectMediaItem(el.metaData.jsonId.rawValue, $.trim(el.node[0].innerText));
                                }else
                                {
                                    //not a valid selection like when media item doesn't have an uploaded file
                                    $("#media-preview").children().remove();
                                    $("#params").children().remove();
                                    $("#media-preview").append("<span>Not a valid media item (might not have an uploaded file).</span>");
                                }
                            }
                           });
			         }

                   }
                 });
                
            }

        };

    } (); //singleton    

})(jQuery);