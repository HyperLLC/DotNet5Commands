using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.Common.Constants
{
    /// <summary>
    /// Data class that contain a number of constants that are used throughout automation with asp.net core.
    /// </summary>
    public static class DotNetConstants
    {
        /// <summary>
        /// The name of the filed that holds the logger
        /// </summary>
        public const string FieldNameLogger = "_logger";

        /// <summary>
        /// The full name of the library for logging.
        /// </summary>
        public const string MicrosoftExtensionLibraryForLoggingName = "Microsoft.Extensions.Logging.Abstractions";

        /// <summary>
        /// Library name for the Microsoft Logger abstractions library.
        /// </summary>
        public const string MicrosoftLoggerLibraryName = "Microsoft.Extensions.Logging.Abstractions";

        /// <summary>
        /// The default namespace for the Microsoft extensions logger implementation
        /// </summary>
        public const string MicrosoftLoggerNamespace = "Microsoft.Extensions.Logging";

        /// <summary>
        /// Library name and default namespace for the common delivery framework library.
        /// </summary>
        public const string CommonDeliveryFrameworkLibraryName = "CommonDeliveryFramework";

        /// <summary>
        /// Library name and default namespace for the aspnet extensions for the common delivery framework library.
        /// </summary>
        public const string CommonDeliveryFrameworkAspNetLibraryName = "CommonDeliveryFramework.Net.Aspnet";

        /// <summary>
        /// The name of the interface for the Microsoft extensions logger.
        /// </summary>
        public const string MicrosoftLoggerInterfaceName = "ILogger";

        /// <summary>
        /// The fully qualified name of the a aspnet core controller.
        /// </summary>
        public const string ControllerBaseName = "Microsoft.AspNetCore.Mvc.ControllerBase";

        /// <summary>
        /// The full namespace for the mvc namespace.
        /// </summary>
        public const string MvcNamespace = "Microsoft.AspNetCore.Mvc";

        /// <summary>
        /// Name for the abstract classes that support action result.
        /// </summary>
        public const string ActionResultClassName = "ActionResult";

        /// <summary>
        /// Name of the interface for action results.
        /// </summary>
        public const string ActionResultInterfaceName = "IActionResult";

        /// <summary>
        /// Namespace for tasks 
        /// </summary>
        public const string SystemThreadingTasksNamespace = "System.Threading.Tasks";

        /// <summary>
        /// Class name for the Task class.
        /// </summary>
        public const string TaskClassName = "Task";
    }
}
