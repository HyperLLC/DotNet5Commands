using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Project.Extensions;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder
{
    /// <summary>
    /// Code factory command for automation of a project folder when selected from solution explorer.
    /// </summary>
    public class CountLinesOfCode : ProjectFolderCommandBase
    {
        private static readonly string commandTitle = "Count Lines of Code";
        private static readonly string commandDescription = "Gets a count of all of the lines of code at this level or any children beneath it.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public CountLinesOfCode(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region Overrides of VsCommandBase<VsProjectFolder>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsProjectFolder result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                //Check to see there are any code files (source code with extensions of .cs and cshtml
                //Verify that there are view files within this folder so that we can count the lines of code
                var children = await result.GetChildrenAsync(true);
                if (children.Any(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.Name.Contains(".cshtml")) || p.Name.Contains(".cs")))
                {
                    isEnabled = true;
                }
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while checking if the solution explorer project folder command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsProjectFolder result)
        {
            try
            {
                int totalLinesofCode = await result.CountLinesOfCodeAsync();
                MessageBox.Show(totalLinesofCode.ToString());
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer project folder command {commandTitle}. ",
                    unhandledError);

            }
        }

        #endregion
    }
}
