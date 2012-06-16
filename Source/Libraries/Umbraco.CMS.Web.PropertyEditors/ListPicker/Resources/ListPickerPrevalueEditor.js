Umbraco.System.registerNamespace("Umbraco.PropertyEditors");

(function ($) {
    Umbraco.PropertyEditors.ListPickerPrevalueEditor = function () {
        var _opts = null;

        return {
            init: function (o) {
                _opts = o;

                //make sortable
                $(_opts.containerId).sortable();

                //add new color item
                $(_opts.addId).click(function () {

                    var guid = Guid.generate();
                    
                    $(_opts.containerId).append($(_opts.templateId).tmpl({ index: guid }));

                    //client side validation for new input
                    $.validator.unobtrusive.reParse($("form"));

                });

                //delete
                $(".delete", _opts.containerId).live("click", function () {
                    var val = $(".list-item-value", $(this).parent()).val();
                    if (confirm("Are you sure you want to delete " + val)) {
                        $(this).parent().remove();
                    }
                });

            }
        }
    } ();
})(jQuery);

