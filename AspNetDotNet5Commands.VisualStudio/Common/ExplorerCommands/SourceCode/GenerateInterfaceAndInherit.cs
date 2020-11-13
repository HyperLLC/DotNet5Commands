using AspNetDotNet5Commands.VisualStudio.Common.Extensions;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.SourceCode
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class GenerateInterfaceAndInherit : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Generate Interface and Inherit";
        private static readonly string commandDescription = "Generate Interface based-on Model and add Inheritance.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public GenerateInterfaceAndInherit(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                isEnabled = result.SourceCode.Classes.Any();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while checking if the solution explorer C# document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsCSharpSource result)
        {
            try
            {
                var classData = result.SourceCode.Classes.FirstOrDefault();
                if (classData == null)
                {
                    return;
                }
                var solution = await VisualStudioActions.SolutionActions.GetSolutionAsync();

                //Get the solution projects and create the interface folder if one doesn't exist
                var SolutionProjects = await solution.GetProjectsAsync(true);
                VsProjectFolder interfacesFolder = await SolutionProjects.FirstOrDefault().CheckAddFolder("Interfaces");

                //Add the new interface file.
                var newInterface = await interfacesFolder.AddDocumentAsync("I" + classData.Name + ".cs", classData.GenerateInterface());
                
                //Update Model to Inhert from newly generated Interface
                var oldInterfaceSignature = $"{classData.Security.ToString().ToLower()} class {classData.Name }";
                var newInterfaceSignature = $"{classData.Security.ToString().ToLower()} class {classData.Name}: I{classData.Name}";
                await result.SourceCode.ReplaceAsync(classData.GenerateModel());    
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }
        }

        #endregion
    }
}
