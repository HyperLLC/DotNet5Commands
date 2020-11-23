using CodeFactory.DotNet.CSharp;
using CodeFactory;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetDotNet5Commands.VisualStudio.Common.Constants;
using System.CodeDom.Compiler;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document.Extensions
{
    /// <summary>
    /// Class that holds extension methods to help with Controller management.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Method that adds a new controller file to the project within the Contollers Folder.  
        /// </summary>
        /// <param name="source">The source VSProjectFolder that this method is extending.</param>
        /// <param name="controllerName">The Name you wish to give to the controller.</param>
        /// <param name="rootNamespace">The root namespace to use when auto-generating the controller code.</param>
        /// <returns>CsSource that contains the class content for the newly generated controller.</returns>
        public static async Task<CsSource> AddControllerAsync(this VsProjectFolder source, string controllerName, string rootNamespace)
        {
            var controllerSource = GenerateControllerClass(controllerName, rootNamespace);
            VsDocument controllerDocument = await source.AddDocumentAsync(controllerName+".cs", controllerSource);
            return await controllerDocument.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Method that generates an ActionResult Method and Adds it to the list of class Members within your source controller.  
        /// </summary>
        /// <param name="source">The CsSource Controller object your referencing.</param>
        /// <param name="actionName">The string ActionName to be used when auto-generating this method code.</param>
        /// <returns>CsSource that contains the newly generated code snippet for the ActionResult Method</returns>
        public static async Task<CsSource> AddActionResultMethodToControllerAsync(this CsSource source, string actionName)
        {
            return await source.Classes.FirstOrDefault().Members.FirstOrDefault().AddBeforeAsync(source.GenerateIActionResultSourceCode(actionName));
        }

        /// <summary>
        /// Method that auto-generates a string-formatted representation of the ActionResult Method.  
        /// </summary>
        /// <param name="source">The CsSource Controller object your referencing.</param>
        /// <param name="actionName">The string ActionName to be used when auto-generating this method code.</param>
        /// <returns>String represenation of the actual formatted source code for the ActionResult Method</returns>
        public static string GenerateIActionResultSourceCode(this CsSource source, string actionName)
        {
            var formatter = new SourceFormatter();
            formatter.AppendCodeLine(2, $"public IActionResult {actionName}()");
            formatter.AppendCodeLine(2, "{");
            formatter.AppendCodeLine(3, "return View();");
            formatter.AppendCodeLine(2, "}");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2);
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Method that auto-generates a string-formatted representation of the Controller source code.  
        /// </summary>
        /// <param name="controllerName">The Name you wish to give to the controller.</param>
        /// <param name="rootNamespace">The root namespace to use when auto-generating the controller code.</param>
        /// <returns>String represenation of the actual formatted source code for the Controller</returns>
        private static string GenerateControllerClass(string controllerName, string rootNamespace)
        {
            var formatter = new SourceFormatter();
            formatter.AppendCodeLine(0, $"using {rootNamespace}.Models;");
            formatter.AppendCodeLine(0, "using Microsoft.AspNetCore.Mvc;");
            formatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            formatter.AppendCodeLine(0, "using System;");
            formatter.AppendCodeLine(0, "using System.Collections.Generic;");
            formatter.AppendCodeLine(0, "using System.Diagnostics;");
            formatter.AppendCodeLine(0, $"namespace {rootNamespace}.Controllers");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"public class {controllerName} : Controller");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, $"private readonly ILogger<{controllerName}> _logger;");
            formatter.AppendCodeLine(2, $"public {controllerName}(ILogger<{controllerName}> logger)");
            formatter.AppendCodeLine(2, "{");
            formatter.AppendCodeLine(3, " _logger = logger;");
            formatter.AppendCodeLine(2, "}");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2, "[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]");
            formatter.AppendCodeLine(2, "public IActionResult Error()");
            formatter.AppendCodeLine(2, "{");
            formatter.AppendCodeLine(3, "return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });");
            formatter.AppendCodeLine(2, "}");
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");
            return formatter.ReturnSource();
        }
    }
}
