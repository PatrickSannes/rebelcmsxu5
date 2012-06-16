Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {

    Umbraco.PropertyEditors.ColorSwatchPickerPrevalueEditor = function () {
        var _opts = null;

        function _generateGuid() {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }
        return {
            init: function (o) {
                _opts = o;

                //make sortable
                $(_opts.containerId).sortable();

                //add new color item
                $(_opts.addId).click(function () {
                    $(_opts.containerId).append($(_opts.templateId).tmpl({ index: _generateGuid() }));
                    //client side validation for new input
                    $.validator.unobtrusive.reParse($("form"));
                });

                //preview colors
                $(".color-item-hexvalue", _opts.containerId).live("change", function () {
                    $(".color-item-preview", $(this).parent()).css("background-color", $(this).val());
              

                });

                //delete
                $(".delete", _opts.containerId).live("click", function () {
                    if (confirm("Are you sure you want to delete this color")) {
                        $(this).parent().remove();
                    }
                });

            }
        }
    } ();
})(jQuery);



