using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.Controllers
{

    public class ExtenderDefinition
    {
        public ExtenderDefinition(Type parentType, Type extenderType)
        {
            ParentType = parentType;
            ExtenderType = extenderType;
        }

        public Type ParentType { get; private set; }
        public Type ExtenderType { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ExtenderDefinition)) return false;
            return Equals((ExtenderDefinition) obj);
        }

        public bool Equals(ExtenderDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ParentType, ParentType) && Equals(other.ExtenderType, ExtenderType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ParentType != null ? ParentType.GetHashCode() : 0)*397) ^ (ExtenderType != null ? ExtenderType.GetHashCode() : 0);
            }
        }
    }

    public class ExtenderData
    {
        public ExtenderData(Func<ControllerBase> extender, object[] additionalParameters)
        {
            Extender = extender;
            AdditionalParameters = additionalParameters;
        }

        public Func<ControllerBase> Extender { get; private set; }
        public object[] AdditionalParameters { get; private set; }
    }

    public class ControllerExtender
    {

        /// <summary>
        /// The key used in RouteValue DataTokens to store a reference to the parent controller of an extender
        /// </summary>
        public const string ParentControllerDataTokenKey = "controller-extender-parent";

        /// <summary>
        /// The key used in RouteValue DataTokens to store a reference to the ExtenderData object
        /// </summary>
        public const string ExtenderParametersDataTokenKey = "controller-extender-data";

        private static readonly ConcurrentDictionary<ExtenderDefinition, ExtenderData> Registrations = new ConcurrentDictionary<ExtenderDefinition, ExtenderData>();     

        public static void RegisterExtender<TController, TExtender>(params object[] additionalParameters)
            where TController : ControllerBase
            where TExtender : ControllerBase
        {
            var t = new ExtenderDefinition(typeof(TController), typeof(TExtender));
            if (Registrations.ContainsKey(t))
                return;

            if (typeof(TExtender).IsAssignableFrom(typeof(TController)))
            {
                throw new InvalidOperationException("Cannot extend a controller by it's same type");
            }

            //if one is already there, then replace it
            Func<ControllerBase> e = () => DependencyResolver.Current.GetService<TExtender>();
            Registrations.AddOrUpdate(t,
                                      new ExtenderData(e, additionalParameters),
                                      (key, orig) => new ExtenderData(e, additionalParameters));
        }

        public static void RegisterExtender<T>(ControllerBase controllerToExtend, params object[] additionalParameters)
            where T : ControllerBase
        {
            RegisterExtender(controllerToExtend, () => (ControllerBase)DependencyResolver.Current.GetService<T>(), additionalParameters);
        }

        public static void RegisterExtender(ControllerBase controllerToExtend, Type controllerExtender, params object[] additionalParameters)
        {
            var t = new ExtenderDefinition(controllerToExtend.GetType(), controllerExtender);
            if (Registrations.ContainsKey(t))
                return;

            if (controllerExtender.IsAssignableFrom(controllerToExtend.GetType()))
            {
                throw new InvalidOperationException("Cannot extend a controller by it's same type");
            }
            
            //if one is already there, then replace it
            Func<ControllerBase> e = () => (ControllerBase)DependencyResolver.Current.GetService(controllerExtender);
            Registrations.AddOrUpdate(t,
                                      new ExtenderData(e, additionalParameters),
                                      (key, orig) => new ExtenderData(e, additionalParameters));
        }

        public static void RegisterExtender<T>(ControllerBase controllerToExtend, Expression<Func<T>> extender, params object[] additionalParameters)
            where T : ControllerBase
        {
            var extenderType = typeof(T);
            var t = new ExtenderDefinition(controllerToExtend.GetType(), extenderType);
            if (Registrations.ContainsKey(t))
                return;

            if (extender.GetType().IsAssignableFrom(controllerToExtend.GetType()))
            {
                throw new InvalidOperationException("Cannot extend a controller by it's same type");
            }

            //if one is already there, then replace it
            Func<ControllerBase> e = extender.Compile();
            Registrations.AddOrUpdate(t,
                new ExtenderData(e, additionalParameters), 
                (key, orig) => new ExtenderData(e, additionalParameters));
        }

        public static IEnumerable<KeyValuePair<ExtenderDefinition, ExtenderData>> GetRegistrations()
        {
            return Registrations;
        }

    }
}
