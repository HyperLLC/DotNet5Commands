using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Document.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Project.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.Models;
using AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document.Extensions;
using AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Folder.Dialogs;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.SolutionExplorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document
{
    /// <summary>
    /// Code factory command for automation of a document when selected from a project in solution explorer.
    /// </summary>
    public class AddNewPartialView : ProjectDocumentCommandBase
    {
        private static readonly string commandTitle = "Add a Section";
        private static readonly string commandDescription = "Adds a partial view to the selected view file and controller wire-up.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddNewPartialView(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region Overrides of VsCommandBase<VsProjectDocument>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsDocument result)
        {
            //Result that determines if the the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;            

            try
            {                
                isEnabled = result.Path.Contains(".cshtml");
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occured while checking if the solution explorer project document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsDocument result)
        {
            
            try
            {
                //Get the current project
                var projectDetails = await result.GetCurrentProjectAsync();             

                //Get view templates config file that provides a complete manifest of the view templates.
                if (await projectDetails.FindDocumentWithinProjectAsync("viewtemplates.json", true, false, VisualStudioModelType.Document) is VsDocument templateConfig)
                {                    
                    //Launch the dialog box to allow the user to select a view template.
                    var newViewDialog = await VisualStudioActions.UserInterfaceActions.CreateVsUserControlAsync<NewViewDialog>();

                    //Get the content of the json manifest file.
                    string jsonString = await templateConfig.GetDocumentContentAsStringAsync();

                    //Pass the view list config to the dialog.
                    newViewDialog.ViewList = JsonConvert.DeserializeObject<ViewTemplateList>(jsonString).PartialViewTemplates;

                    //Display the dialog.
                    await VisualStudioActions.UserInterfaceActions.ShowDialogWindowAsync(newViewDialog);
                    ViewTemplateItem selectedViewTemplate = newViewDialog.SelectedViewTemplate;
                    var viewName = newViewDialog.ViewTitle + "Section";

                    if (viewName != null && selectedViewTemplate != null)
                    {
                        //Check to make sure we got everything from the dialog and then go fetch the view template.   
                        if (await projectDetails.FindDocumentWithinProjectAsync(selectedViewTemplate.File.ToLower(), true, true, VisualStudioModelType.Document) is VsDocument viewTemplate)
                        {
                            //Since we want all of our views to reside in their separate view folders with a separate controllers, check to see if we're at the root of the Views folder.
                            if (result.Name.ToLower().Contains(".cshtml"))
                                await result.AddRazorViewAsync(viewTemplate, viewName, false, true);
                        }

                        //Crawl the tree and find the matching controller based-on the naming convention of your parent folder.            
                        var parentFolders = await result.GetParentFolders();
                        VsProjectFolder parentFolder = parentFolders.FirstOrDefault();
                        if (await projectDetails.FindDocumentWithinProjectAsync(parentFolder.Name.ToLower() + "controller.cs", true, true, VisualStudioModelType.Document) is VsDocument controllerModel)
                        {
                            CsSource controllerClass = await controllerModel.GetCSharpSourceModelAsync();
                            await controllerClass.AddActionResultMethodToControllerAsync(viewName);
                        }
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
