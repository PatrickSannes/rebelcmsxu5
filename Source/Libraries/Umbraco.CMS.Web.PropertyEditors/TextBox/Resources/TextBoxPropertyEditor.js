/// <reference path="../../Scripts/Umbraco.System/NamespaceManager.js" />
/// <reference path="../../Scripts/Umbraco.Umbraco.System/UrlEncoder.js" />

Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {

    // Singleton TextBoxPropertyEditor class to encapsulate the management of PropertyEditors.
    Umbraco.PropertyEditors.TextBoxPropertyEditor = function() {

        return {
            
			charLimit: function (_opts) {
				var _this = this;
				var _limit = _opts.charLimit;
				var _field = $('#' + _opts.fieldId);
				// TODO: i18n/L10n
				var _status = $('<div id="' + _opts.umbracoPropertyId + '_Status" class="text-box-status">You have ' + (_limit - _field.val().length) + ' characters left.</div>');
				_status.insertAfter(_field);
				_field.keyup(function() {
					// strip out the HTML (for use with the 'Simple Editor' controls)
					var _text = _field.val().replace(/(<([^>]+)>)/ig, '');
					if (_text.length > _limit) {
						_status.text('You cannot write more than ' + _limit + ' characters!');
						_field.val(_text.substr(0, _limit));
					} else {
						_status.text('You have ' + (_limit - _text.length) + ' characters left.');
					}
				});
			},

            init: function(_opts) {
                var _this = this;
                var propertyEditor = new Umbraco.PropertyEditors.PropertyEditor(_opts.umbracoPropertyId);
                Umbraco.PropertyEditors.PropertyEditorManager.getInstance().registerPropertyEditor(propertyEditor, function (ctx) {
                    $(ctx).bind('onBold', function () {
                        _this.insertTag($("#" + _opts.fieldId).get(0), "strong");
                    });
                    $(ctx).bind('onItalic', function () {
                        _this.insertTag($("#" + _opts.fieldId).get(0), "em");
                    });
                    $(ctx).bind('onLink', function () {
                        _this.insertLink($("#" + _opts.fieldId).get(0));
                    });

                    $("#" + _opts.umbracoPropertyId + " textarea").mousedown(function (e) {
                        Umbraco.PropertyEditors.PropertyEditorManager.getInstance().setFocus(e, propertyEditor);
                    })
                });
            },

            insertLink : function (element) {
                var theLink = prompt('Enter URL for link here:', 'http://');
                this.insertTag(element, 'a', ' href="' + theLink + '"')
            },

            insertTag : function (element, tag, param) {

                if (param == undefined) param = "";

                start = '<' + tag + param + '>';
                eind = '</' + tag + '>';

                if (document.selection) {
                    element.focus();
                    sel = document.selection.createRange();
                    sel.text = start + sel.text + eind;
                } else if (element.selectionStart || element.selectionStart == '0') {
                    element.focus();
                    var startPos = element.selectionStart;
                    var endPos = element.selectionEnd;
                    element.value = element.value.substring(0, startPos) + start + element.value.substring(startPos, endPos) + eind + element.value.substring(endPos, element.value.length);
                } else {
                    element.value += start + eind;
                }
            }

        }

    }();
    
})(jQuery);

