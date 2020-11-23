using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Document.Extensions
{
    /// <summary>
    /// Class that holds extension methods that are used with Folders.
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Returns a list of non-source code documents from VsProject that have a matching extension.
        /// </summary>
        /// <param name="source">The source document within the project.</param>
        /// <param name="extension">The file extension to search for</param>
        /// <param name="searchChildren">Flag that determines if nested project folders should also be searched for files.</param>
        /// <param name="excludeKnownExternalFolders">Flag that determines if a content filter should be applied.</param>
        /// <returns>List of documents that meet the criteria.</returns>
        public static async Task<VsProject> GetCurrentProjectAsync(this VsDocument source)
        {
            //Bounds check if no instance of the model is provided returning null.
            if (source == null) return null;

            //Loading the project system version of the document.
            var document = source;

            //If the project system version of the document could not be loaded then return null.
            if (document == null) return null;

            //Models to store information about lookup results.
            VsProject result = null;
            VsModel currentModel = document;

            while (result == null)
            {
                //Confirming a model was returned otherwise there is no parent project to return, so break out of the while loop.
                if (currentModel == null) break;

                switch (currentModel.ModelType)
                {
                    //switching between each standard model types. loading model data.
                    case VisualStudioModelType.Project:
                        result = currentModel as VsProject;

                        break;

                    case VisualStudioModelType.ProjectFolder:

                        currentModel = currentModel is VsProjectFolder projectFolder ? await projectFolder.GetParentAsync() : null;
                        break;

                    case VisualStudioModelType.Document:

                        currentModel = currentModel is VsDocument documentModel ? await documentModel.GetParentAsync() : null;
                        break;

                    default:
                        currentModel = null;
                        break;

                }
            }

            //return the project model
            return result ;
        }

        /// <summary>
        /// Returns a list of non-source code documents from VsProject that have a matching extension.
        /// </summary>
        /// <param name="source">The source document within the project.</param>
        /// <param name="extension">The file extension to search for</param>
        /// <param name="searchChildren">Flag that determines if nested project folders should also be searched for files.</param>
        /// <param name="excludeKnownExternalFolders">Flag that determines if a content filter should be applied.</param>
        /// <returns>List of documents that meet the criteria.</returns>
        public static async Task<VsProject> GetCurrentProjectAsync(this VsProjectFolder source)
        {
            //Bounds check if no instance of the model is provided returning null.
            if (source == null) return null;

            //Models to store information about lookup results.
            VsProject result = null;
            VsModel currentModel = source;

            while (result == null)
            {
                //Confirming a model was returned otherwise there is no parent project to return, so break out of the while loop.
                if (currentModel == null) break;

                switch (currentModel.ModelType)
                {
                    //switching between each standard model types. loading model data.
                    case VisualStudioModelType.Project:
                        result = currentModel as VsProject;

                        break;

                    case VisualStudioModelType.ProjectFolder:

                        currentModel = currentModel is VsProjectFolder projectFolder ? await projectFolder.GetParentAsync() : null;
                        break;                    

                    default:
                        currentModel = null;
                        break;

                }
            }

            //return the project model
            return result;
        }

        /// <summary>
        /// Returns a list of non-source code documents from VsProject that have a matching extension.
        /// </summary>
        /// <param name="source">The source visual studio project to search.</param>
        /// <param name="extension">The file extension to search for</param>
        /// <param name="searchChildren">Flag that determines if nested project folders should also be searched for files.</param>
        /// <param name="excludeKnownExternalFolders">Flag that determines if a content filter should be applied.</param>
        /// <returns>List of documents that meet the criteria.</returns>
        public static async Task<IReadOnlyList<VsDocument>> GetDocumentsWithExtensionAsync(this VsProject source, string extension, bool searchChildren, bool excludeKnownExternalFolders)
        {
            //If no source is found return an empty list.
            if (source == null) return ImmutableList<VsDocument>.Empty;

            //If no file extension is provided return an empty list.
            if (string.IsNullOrEmpty(extension)) return ImmutableList<VsDocument>.Empty;

            List<VsDocument> result = new List<VsDocument>();

            //Making sure we start with a period for the extension for searching purposes.
            if (!extension.StartsWith(".")) extension = $".{extension}";

            //Calling the CodeFactory project system api to get the children of the project.
            var children = await source.GetChildrenAsync(searchChildren);

            //Filtering out to just 
            var sourceFiles = children.Where(p => p.ModelType.Equals(VisualStudioModelType.Document))
                .Cast<VsDocument>().Where(d => !d.IsSourceCode);

            return sourceFiles.Where(s =>
            {
                //If we are excluding external folders just check for the extension.
                if (!excludeKnownExternalFolders) return s.Name.EndsWith(extension);

                //Checking to make sure the file is not in the excluded list.
                var documentPath = s.Path;
                if (string.IsNullOrEmpty(documentPath)) return false;
                return !documentPath.ToLower().Contains("\\content\\") && s.Name.EndsWith(extension);
            }).ToImmutableList();
        }

        /// <summary>
        /// Returns a list of non-source code documents from VsProject that have a matching extension.
        /// </summary>
        /// <param name="source">The list of visual studio models to search for documents in.</param>
        /// <param name="projectDirectory">The fully qualified path to the project directory.</param>
        /// <param name="extension">The file extension to search for</param>
        /// <param name="excludeKnownExternalFolders">Flag that determines if a content filter should be applied.</param>
        /// <returns>List of documents that meet the criteria.</returns>
        public static IReadOnlyList<VsDocument> GetDocumentsWithExtension(this IReadOnlyList<VsModel> source, string projectDirectory, string extension, bool excludeKnownExternalFolders)
        {
            //If no source is found return an empty list.
            if (source == null) return ImmutableList<VsDocument>.Empty;

            //If no file extension is provided return an empty list.
            if (string.IsNullOrEmpty(extension)) return ImmutableList<VsDocument>.Empty;

            List<VsDocument> result = new List<VsDocument>();

            //Making sure we start with a period for the extension for searching purposes.
            if (!extension.StartsWith(".")) extension = $".{extension}";

            //Filtering out to just 
            var sourceFiles = source.Where(p => p.ModelType.Equals(VisualStudioModelType.Document))
                .Cast<VsDocument>().Where(d => !d.IsSourceCode);

            return sourceFiles.Where(s =>
            {
                //If we are excluding external folders just check for the extension.
                if (!excludeKnownExternalFolders) return s.Name.EndsWith(extension);

                //Checking to make sure the file is not in the excluded list.
                var documentPath = s.Path;
                if (string.IsNullOrEmpty(documentPath)) return false;
                return !documentPath.ToLower().Contains("\\content\\") && s.Name.EndsWith(extension);
            }).ToImmutableList();
        }

        /// <summary>
        /// Gets the c# source code files from a target provided lists.
        /// </summary>
        /// <param name="source">The source list of files.</param>
        /// <param name="excludeKnownExternalFolders">Flag that determines if target files by directory location are excluded.</param>
        /// <returns>List of the found files.</returns>
        public static IReadOnlyList<VsCSharpSource> GetSourceCodeDocumentsAsync(this IReadOnlyList<VsModel> source, bool excludeKnownExternalFolders)
        {

            string extension = ".cs";
            var sourceFiles = source.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource)).Cast<VsCSharpSource>();

            var result = sourceFiles.Where(s =>
            {
                if (excludeKnownExternalFolders)
                {
                    var folderChain = s.SourceCode.SourceDocument;

                    //repeat this section for anything else that might qualify.  This is an attempt to give the caller an option
                    //to *not* bring over bootstrap artifacts or the like.
                    if (folderChain.ToLower().Contains("\\app_data\\")) return false;
                    if (folderChain.ToLower().Contains("\\app_start\\")) return false;
                    if (folderChain.ToLower().Contains("\\app_readme\\")) return false;
                    if (folderChain.ToLower().Contains("\\content\\")) return false;
                }

                if (s.Name.EndsWith(extension)) return true;
                return false;
            }).ToImmutableList();

            return result;
        }

        /// <summary>
        /// Extension method that copies a <see cref="VsDocument"/> from a source project to a target location in a supplied destination directory.
        /// Will replace the source project directory path with a new root destination path.
        /// This will overwrite the existing file.
        /// </summary>
        /// <param name="source">The document to be copied</param>
        /// <param name="sourceProjectDirectory">The source project directory to be replaced.</param>
        /// <param name="rootDestinationDirectory">The new target destination path for the file.</param>
        /// <returns>Null if the file was not copied, or the fully qualified path where the file was copied to.</returns>
        public static string CopyProjectFile(this VsDocument source, string sourceProjectDirectory, string rootDestinationDirectory)
        {
            //Bounds checking to make sure all data has been passed in correctly. If not return null.
            if (source == null) return null;
            if (string.IsNullOrEmpty(sourceProjectDirectory)) return null;
            if (string.IsNullOrEmpty(rootDestinationDirectory)) return null;

            //Setting the result variable.
            string result = null;

            try
            {
                //Loading the source file path from the visual studio document.
                var sourceFile = source.Path;

                //Replacing the source path with the target destination directory. 
                var destinationFile = sourceFile.Replace(sourceProjectDirectory, rootDestinationDirectory);

                //Making sure the directory already exists in the target project, if it does not go ahead and add it to the project.
                var destinationDirectory = Path.GetDirectoryName(destinationFile);

                if (string.IsNullOrEmpty(destinationDirectory)) return null;

                if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

                //Copying the project file to the new project.
                File.Copy(sourceFile, destinationFile, true);

                //Returning the new file location of the project file in the new project.
                result = destinationFile;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                //An error occurred we are going to swallow the exception and return a null return type.
                return null;
            }

            return result;
        }        
    }
}
