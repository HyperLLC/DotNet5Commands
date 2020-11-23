using CodeFactory.Document;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Project.Extensions
{
    /// <summary>
    /// Class that holds extension methods used to get Visual Studio project details.
    /// </summary>
    public static class ProjectExtensions
    {
        /// <summary>
        /// Extension method that loads all <see cref="VsProjectFolder"/> , <see cref="VsDocument"/>, and <see cref="VsCSharpSource"/>
        /// </summary>
        public static async Task<IReadOnlyList<VsModel>> LoadAllProjectData(this VsProject source, bool loadSourceCode = true)
        {
            return source != null ? await source.GetChildrenAsync(true, loadSourceCode) : ImmutableList<VsModel>.Empty;
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
        public static string CopyProjectFile(this VsDocument source, string sourceProjectDirectory,
            string rootDestinationDirectory)
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

        /// <summary>
        /// Returns VsModel object from the VsProject that has a matching name.
        /// </summary>
        /// <param name="source">The source visual studio project to search.</param>
        /// <param name="documentName">The document name to search for.</param>
        /// <param name="searchChildren">Flag that determines if nested project folders should also be searched for files.</param>
        /// <param name="exactMatch">Flag that determines whether we search on equals or contains.</param>
        /// <param name="visualStudioModelType">Defines what type of artifact through Visual Studio we are search for.</param>
        /// <returns>The first document that meets the criteria.</returns>
        public static async Task<VsModel> FindDocumentWithinProjectAsync(this VsProject source, string documentName, bool searchChildren, bool exactMatch, VisualStudioModelType visualStudioModelType)
        {
            var projectDocuments = await source.GetChildrenAsync(searchChildren);

            //Find the first document that meets our criteria where it's not an exact match
            if (!exactMatch)
                return projectDocuments.FirstOrDefault(c => c.ModelType == visualStudioModelType & c.Name.ToLower().Contains(documentName.ToLower()));            

            // OR Find the first document that meets our criteria where it's an exact match
            if (exactMatch)
                return projectDocuments.FirstOrDefault(c => c.ModelType == visualStudioModelType & c.Name.ToLower().Equals(documentName.ToLower()));
                         
            return null;
        }

        /// <summary>
        /// Returns an int representing the number of lines of code within each code file within the source project folder.
        /// </summary>
        /// <param name="source">The source VsProject object</param>
        /// <returns>Returns a count of all children .cshtml or .cs file's lines of code.</returns>
        public static async Task<int> CountLinesOfCodeAsync(this VsProjectFolder source)
        {
            int count = 0;
            IReadOnlyList<VsModel> codeFiles = await source.GetChildrenAsync(true);
            IEnumerable<VsModel> children = codeFiles.Where(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.Name.Contains(".cshtml")) || p.Name.Contains(".cs"));
            foreach(VsDocument codeFile in children)
            {
                IDocumentContent content = await codeFile.GetDocumentContentAsContentAsync();
                count += content.Count;
            }
            return count;
        }

        /// <summary>
        /// Returns an int representing the number of lines of code within each code file within the source project folder.
        /// </summary>
        /// <param name="source">The source VsProject object</param>
        /// <returns>Returns a count of all children .cshtml or .cs file's lines of code.</returns>
        public static async Task<int> CountLinesOfCodeAsync(this VsDocument source)
        {
            var content = await source.GetDocumentContentAsContentAsync();
            return content.Count();
        }

        /// <summary>
        /// Returns an int representing the number of lines of code within each code file within the source project folder.
        /// </summary>
        /// <param name="source">The source VsProject object</param>
        /// <returns>Returns a count of all children .cshtml or .cs file's lines of code.</returns>
        public static async Task<int> CountLinesOfCodeAsync(this VsProject source)
        {
            int count = 0;
            IReadOnlyList<VsModel> codeFiles = await source.GetChildrenAsync(true);
            IEnumerable<VsModel> children = codeFiles.Where(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.Name.Contains(".cshtml")) || p.Name.Contains(".cs"));
            foreach (VsDocument codeFile in children)
            {
                IDocumentContent content = await codeFile.GetDocumentContentAsContentAsync();
                count += content.Count;
            }
            return count;
        }
    }
}