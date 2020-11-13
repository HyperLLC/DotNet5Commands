using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetToDotNet5.Automation.Common.Enums
{
    /// <summary>
    /// Enumeration used to determine the status of a migration step.
    /// </summary>
    public enum ProcessStatusEnum
    {
        /// <summary>
        /// The current migration step is running.
        /// </summary>
        Running = 0,

        /// <summary>
        /// The current migration step has passed.
        /// </summary>
        Passed = 1,

        /// <summary>
        /// The current migration step has failed. 
        /// </summary>
        Failed
    }
}
