using AspNetDotNet5Commands.VisualStudio.Common.Constants;
using AspNetDotNet5Commands.VisualStudio.Common.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.Models;
using AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document;
using AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Folder.Dialogs;
using AspNetToDotNet5.Automation.Common.Enums;
using CodeFactory.Document;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Folder
{
    /// <summary>
    /// Code factory command for automation of a project folder when selected from solution explorer.
    /// </summary>
    public class AddNewView : ProjectFolderCommandBase
    {
        private static readonly string commandTitle = "Add New View";
        private static readonly string commandDescription = "Creates a new View and Controller pair.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddNewView(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                //Verify that there are view files within this folder   
                var children = await result.GetChildrenAsync(false);
                if (children.Any(p => p.ModelType.Equals(VisualStudioModelType.Document) && p.Name.Contains(".cshtml")) && result.Name == "Views")
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
                //Hydrate solution and project details                
                var solution = await VisualStudioActions.SolutionActions.GetSolutionAsync();
                IReadOnlyList<VsProject> projectDetails = await solution.GetProjectsAsync(true);

                //Get view templates config file that provides a complete manifest of the view templates
                if (await projectDetails.FindDocumentWithinProjectAsync("viewtemplates.json", true, false, VisualStudioModelType.Document) is VsDocument templateConfig)
                {
                    //Launch the dialog box to allow the user to select a view template
                    var newViewDialog = await VisualStudioActions.UserInterfaceActions.CreateVsUserControlAsync<NewViewDialog>();

                    //Get the content of the json manifest file
                    string jsonString = await templateConfig.GetDocumentContentAsStringAsync();

                    //Pass the view list config to the dialog
                    newViewDialog.ViewList = JsonConvert.DeserializeObject<ViewTemplateList>(jsonString).ViewTemplates;

                    //Display the dialog                
                    await VisualStudioActions.UserInterfaceActions.ShowDialogWindowAsync(newViewDialog);
                    ViewTemplateItem selectedViewTemplate = newViewDialog.SelectedViewTemplate;
                    var viewName = newViewDialog.ViewTitle;

                    if (viewName != null && selectedViewTemplate != null)
                    {
                        //Check to make sure we got everything from the dialog and then go fetch the view template    
                        if (await projectDetails.FindDocumentWithinProjectAsync(selectedViewTemplate.File.ToLower(), true, true, VisualStudioModelType.Document) is VsDocument viewTemplate)
                        {
                            //Since we want all of our views to reside in their separate view folders with a separate controllers, check to see if we're at the root of the Views folder.
                            if (result.Name.ToLower().Equals("views"))
                                await result.AddRazorViewAsync(viewTemplate, viewName, false, true);
                        }

                        //Check to make sure we got everything from the dialog and then go create the corresponding controller              
                        if (await projectDetails.FindDocumentWithinProjectAsync("controllers", true, true, VisualStudioModelType.ProjectFolder) is VsProjectFolder controllerFolder)
                        {
                            //Add the controller class
                            CsSource controllerSourceCode = await controllerFolder.AddControllerAsync(viewName + "Controller", projectDetails.FirstOrDefault().Name);
                            await controllerSourceCode.AddActionResultMethodToControllerAsync(viewName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Oops, you did not enter a View Name and Select a template from the dialog.  Please try again.");
                    }
                }                      
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while executing the solution explorer project folder command {commandTitle}. ", unhandledError);
            }
        }
        #endregion
    }
}
