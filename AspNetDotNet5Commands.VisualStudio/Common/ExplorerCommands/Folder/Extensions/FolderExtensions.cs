using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder.Extensions
{
    /// <summary>
    /// Class that holds extension methods that are used with Folders.
    /// </summary>
    public static class FolderExtensions
    {        
        /// <summary>
        /// Used to check a project model for the existence of a folder at the root level of a given name.  If the folder is 
        /// missing - create it.
        /// </summary>
        /// <param name="source">The visual studio project that we are checking exists or creating.</param>
        /// <param name="folderName">The name of the folder to return.</param>
        /// <returns>The existing or created project folder.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either provided parameter is not provided.</exception>
        public static async Task<VsProjectFolder> CheckAddFolder(this VsProject source, string folderName)
        {
            //Bounds checking to make sure all the data needed to get the folder returned is provided.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));

            //Calling the project system in CodeFactory and getting all the children in the root of the project.
            var projectFolders = await source.GetChildrenAsync(false);

            //Searching for the project folder, if it is not found will add the project folder to the root of the project.
            return projectFolders.Where(m => m.ModelType == VisualStudioModelType.ProjectFolder)
                       .Where(m => m.Name.Equals(folderName))
                       .Cast<VsProjectFolder>()
                       .FirstOrDefault()
                   ?? await source.AddProjectFolderAsync(folderName);
        }

        /// <summary>
        /// Gets a list of VSProjectFolders parent folders relative to a document.  
        /// This list is in reverse order from leaf-to-trunk. An empty list is returned if the document lives in the root of the project.
        /// </summary>
        /// <param name="sourceDocument">The visual studio document to search for.</param>
        /// <returns>List of the the parent <see cref="VsProjectFolder"/> found.</returns>
        public static async Task<List<VsProjectFolder>> GetParentFolders(this VsDocument sourceDocument)
        {
            //Stores the list of parents being returned.
            List<VsProjectFolder> folderHierarchy = new List<VsProjectFolder>();

            //Getting the parent of the source document.
            var parentModel = await sourceDocument.GetParentAsync();

            //If no parent was found return the empty list.
            if (parentModel == null) return folderHierarchy;

            //Climb back up the file and folders until you get to the hosting project.
            while (!parentModel.ModelType.Equals(VisualStudioModelType.Project))
            {
                if (parentModel.ModelType.Equals(VisualStudioModelType.ProjectFolder))
                {
                    //Casting the model to the project folder.
                    var parentFolder = parentModel as VsProjectFolder;

                    //checking to make sure the cast ran clean.
                    if (parentFolder == null) return folderHierarchy;

                    //Adding the parent folder to the list to be returned
                    folderHierarchy.Add(parentFolder);

                    //Getting the next parent and confirming it was found.
                    parentModel = await (parentFolder).GetParentAsync();
                    if (parentModel == null) return folderHierarchy;
                }
                else
                {
                    //Casting to the parent document model.
                    var parentDocument = parentModel as VsDocument;

                    //If the cast failed return what was found.
                    if (parentDocument == null) return folderHierarchy;

                    //Getting the next parent and confirming it was found.
                    parentModel = await (parentDocument).GetParentAsync();
                    if (parentModel == null) return folderHierarchy;
                }

            }

            //Returning the found parent models.
            return folderHierarchy;

        }

        /// <summary>
        /// Confirms the target project sub folder exists in the project folder.  If not, it will create it within the sourced project folder.
        /// </summary>
        /// <param name="projectFolder">The source project folder to check.</param>
        /// <param name="folderName">The name of the folder to create or return if it exists.</param>
        /// <returns>The target project folder.</returns>
        public static async Task<VsProjectFolder> CheckAddFolder(this VsProjectFolder projectFolder, string folderName)
        {

            //Call CodeFactory API and get the children of the project folder.
            var projectFolders = await projectFolder.GetChildrenAsync(false);

            //Search for the project folder to confirm it exists, if not create it and return the created folder.
            return projectFolders.Where(m => m.Name.Equals(folderName)).Cast<VsProjectFolder>().FirstOrDefault() ??
                               await projectFolder.AddProjectFolderAsync(folderName);

        }

        /// <summary>
        /// Extension method that runs an async call from a sync thread.
        /// </summary>
        /// <param name="source">Target C# method to evaluate.</param>
        /// <returns>The content of the the method body.</returns>
        public static String MethodContent(this CsMethod source)
        {
            var taskObj = Task.Run(async () => await source.GetBodySyntaxAsync());
            return taskObj.Result;
        }
    }
}
