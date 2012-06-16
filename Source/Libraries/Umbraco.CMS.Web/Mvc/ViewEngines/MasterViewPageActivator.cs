using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    public class MasterViewPageActivator : IViewPageActivator
    {
        private readonly IEnumerable<IPostViewPageActivator> _postActivators;
        private readonly HashSet<IFilteredViewPageActivator> _viewPageActivators;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public MasterViewPageActivator()
        {
            _viewPageActivators = new HashSet<IFilteredViewPageActivator>();
        }

        public MasterViewPageActivator(IEnumerable<IFilteredViewPageActivator> viewPageActivators, IEnumerable<IPostViewPageActivator> postActivators)
        {
            var pa = postActivators.ToArray();
            //we need to make sure they execute in the correct order
            var ordered = (from p in pa
                           let att = p.GetType().GetCustomAttributes<PostViewPageActivatorAttribute>(false).ToArray()
                           where att.Any()
                           select new Tuple<int, IPostViewPageActivator>(att.Single().Order, p))
                .ToList();
            var unordered = pa.Where(x => !ordered.Select(y => y.Item2).Contains(x));
            //now we need to add them in order
            var allOrdered = ordered.OrderBy(x => x.Item1).Select(o => o.Item2).ToList();
            allOrdered.AddRange(unordered);
            _postActivators = allOrdered;

            _viewPageActivators = new HashSet<IFilteredViewPageActivator>(viewPageActivators);
        }

        /// <summary>
        /// Registers the activator.
        /// </summary>
        /// <param name="slaveActivator">The slave activator.</param>
        /// <remarks></remarks>
        public void RegisterActivator(IFilteredViewPageActivator slaveActivator)
        {
            using (new WriteLockDisposable(_locker))
            {
                if (slaveActivator != null && !_viewPageActivators.Contains(slaveActivator))
                {
                    _viewPageActivators.Add(slaveActivator);
                }
            }
        }

        public object Create(ControllerContext controllerContext, Type type)
        {
            var activator = _viewPageActivators.FirstOrDefault(x => x.ShouldCreate(controllerContext, type));

            var view = activator != null 
                           ? activator.Create(controllerContext, type) 
                           : Activator.CreateInstance(type);

            foreach(var p in _postActivators)
            {
                p.OnViewCreated(controllerContext, type, view);
            }

            return view;    
        }
    }
}