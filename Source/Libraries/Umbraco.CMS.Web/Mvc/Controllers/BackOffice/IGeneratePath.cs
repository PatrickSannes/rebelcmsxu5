using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{

    /// <summary>
    /// Represents an object that can generate an Id path for an element given it's Id
    /// </summary>
    public interface IGeneratePath
    {
        /// <summary>
        /// Returns a collection of Id paths for the current entity being edited by the controller instance
        /// </summary>
        /// <returns></returns>
        EntityPathCollection GetPaths();

        /// <summary>
        /// Sets the Id path for the current entity being edited by the controller instance
        /// </summary>
        /// <param name="path"></param>
        void GeneratePathsForCurrentEntity(EntityPathCollection path);

    }
}