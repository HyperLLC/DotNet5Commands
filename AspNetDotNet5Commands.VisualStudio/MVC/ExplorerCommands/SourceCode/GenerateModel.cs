using AspNetDotNet5Commands.VisualStudio.Common.Constants;
using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Document.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder.Extensions;
using AspNetDotNet5Commands.VisualStudio.ExplorerCommands.SourceCode.Extensions;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.SourceCode
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class GenerateModel : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Generate Model";
        private static readonly string commandDescription = "Generates a model from this interface class.  The model is automatically placed into the Models folder.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public GenerateModel(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                isEnabled = result.SourceCode.Interfaces.FirstOrDefault() != null;
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
                var interfaceData = result.SourceCode.Interfaces.FirstOrDefault();
                var modelName = interfaceData.Name.Substring(1, interfaceData.Name.Length - 1);

                if (interfaceData == null) return;

                var solution = await VisualStudioActions.SolutionActions.GetSolutionAsync();

                //Get the solution projects and create the Model folder if one doesn't exist
                var SolutionProjects = await solution.GetProjectsAsync(true);
                VsProjectFolder modelsFolder = await SolutionProjects.FirstOrDefault().CheckAddFolder("Models");

                //Getting the hosting project for the command.
                var hostingProject = await result.GetHostingProjectAsync();

                //If no hosting project can be found this command should not be executed.
                if (hostingProject == null) return;

                //Add the new Model file.
                await modelsFolder.AddDocumentAsync(modelName + ".cs", result.SourceCode.Interfaces.FirstOrDefault().GenerateModelFromInterface(await hostingProject.HasReferenceLibraryAsync(DotNetConstants.CommonDeliveryFrameworkAspNetLibraryName)));                
            }
            catch (Exception unhandledError)
            {
                MessageBox.Show("The Model with that name already exist");
                _logger.Error($"The following unhandled error occured while executing the solution explorer C# document command {commandTitle}. ", unhandledError);
            }
        }
        #endregion
    }
}
