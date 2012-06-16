using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.TypeMapping
{
    public sealed class FrameworkModelMapper : AbstractFluentMappingEngine
    {
        public FrameworkModelMapper(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
        }

        public override void ConfigureMappings()
        {
            ConfigureLocalizationModelMappings();

            this.CreateMap<DateTime, DateTimeOffset>()
                .CreateUsing(x => new DateTimeOffset(x));

            this.CreateMap<DateTimeOffset, DateTime>()
                .CreateUsing(x => new DateTime(x.DateTime.Ticks));
        }

        /// <summary>
        /// Creates mappings for localization models
        /// </summary>
        public void ConfigureLocalizationModelMappings()
        {
            this.SelfMap<LocalizedString>()
                .CreateUsing(x => new LocalizedString(x.Value)); //TODO: Change this so that cultures are copied across too (requires keyvalues inside LocalizedString to be exposed)

            this.CreateMap<string, LocalizedString>()
                .CreateUsing(x =>
                {
                    var converter = new Framework.LocalizedStringConverter();
                    return (converter.ConvertFrom(x) as LocalizedString) ?? new LocalizedString("");
                })
                .MapMemberFrom(x => x.Value, x => x);

            this.CreateMap<LocalizedString, string>()
                .CreateUsing(x =>
                {
                    var converter = new Framework.LocalizedStringConverter();
                    return (string)converter.ConvertTo(x, typeof(string)) ?? "";
                });
        }
    }
}
