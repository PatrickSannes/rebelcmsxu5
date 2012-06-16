using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework
{
    public class CancellableEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the action raising this event should be cancelled.
        /// </summary>
        /// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
        public bool Cancel { get; set; }
    }
}
