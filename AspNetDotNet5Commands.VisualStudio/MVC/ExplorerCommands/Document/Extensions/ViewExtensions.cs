using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder.Extensions;
using CodeFactory;
using CodeFactory.VisualStudio;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document.Extensions
{
    /// <summary>
    /// Class that holds extension methods to help with View management.
    /// </summary>
    public static class ViewExtensions
    {
        /// <summary>
        /// Method that adds a new razor view to the selected folder.  
        /// </summary>
        /// <param name="source">The source VSProjectFolder that this method is extending.</param>
        /// <param name="viewTemplateFile">The VsDocument that represents the html markup template file. If it's null we ignore it.</param>
        /// <param name="viewName">The target name of the razor View you want to create.</param>
        /// <param name="generateModel">Boolean to dictate whether or not to auto-generate a default razor view @model with corresponding model and interface files.</param>
        /// <param name="generateViewData">Boolean to dictate whether or not to auto-generate a default razor view ViewData["Title"] attribute.</param>
        public static async Task<VsDocument> AddRazorViewAsync(this VsProjectFolder source, VsDocument viewTemplateFile, string viewName, bool generateModel, bool generateViewData)
        {
            //Create the new view containing folder
            VsProjectFolder newViewFolder = await source.CheckAddFolder(viewName);

            //Add the new view file and content                        
            VsDocument viewDocument = await newViewFolder.AddDocumentAsync(viewName + ".cshtml", await viewTemplateFile.GetDocumentContentAsStringAsync());

            //Prepend and auto-generate a default razor view Model
            // -- FUTURE FEATURE --

            //Prepend the ViewData[] Attribute(s)
            if (generateViewData)
                viewDocument = await viewDocument.AddViewDataAttributesAsync(viewName);

            return viewDocument;            
        }

        /// <summary>
        /// Method that adds a new razor view to the selected folder.  
        /// </summary>
        /// <param name="source">The source VSProjectFolder that this method is extending.</param>
        /// <param name="viewTemplateFile">The VsDocument that represents the html markup template file. If it's null we ignore it.</param>
        /// <param name="viewName">The target name of the razor View you want to create.</param>
        /// <param name="generateModel">Boolean to dictate whether or not to auto-generate a default razor view @model with corresponding model and interface files.</param>
        /// <param name="generateViewData">Boolean to dictate whether or not to auto-generate a default razor view ViewData["Title"] attribute.</param>
        public static async Task<VsDocument> AddRazorViewAsync(this VsDocument source, VsDocument viewTemplateFile, string viewName, bool generateModel, bool generateViewData)
        {
            //Get the parent folder.
            var parentFolders = await source.GetParentFolders();
            VsProjectFolder parentFolder = parentFolders.FirstOrDefault();

            //Add the new view file in the same directory as the selected view.
            VsDocument viewDocument = await parentFolder.AddDocumentAsync(viewName + "Section.cshtml", await viewTemplateFile.GetDocumentContentAsStringAsync());
            
            //Add the ViewData for the partial so that the partials have an associated html id
            await viewDocument.AddContentToBeginningAsync(GenerateViewDataAttributes("SectionName", viewName + "Section"));

            //Add the partial class to the source razor view.
            await source.AddPartialClassToViewAsync(viewName, parentFolder.Name);

            return viewDocument;
        }

        /// <summary>
        /// Method that appends auto-generated default ViewData[] and @page attributes to your razor view.
        /// </summary>
        /// <param name="viewName">The target name of the razor View you want to use in your ViewData[] definition.</param>
        /// <param name="viewName">The target name of the razor View you want to use in your ViewData[] definition.</param>
        public static async Task<VsDocument> AddPartialClassToViewAsync(this VsDocument source, string viewName, string parentFolder)
        {
            string markup = $"\r\n@await Html.PartialAsync(\"~/Views/{parentFolder}/{viewName}Section.cshtml\")";
            await source.AddContentToEndAsync(markup);
            return source;
        }

        /// <summary>
        /// Method that appends a list item to the navigation file for a partial view.
        /// </summary>
        /// <param name="source">The source navigation.cshtml document.</param>
        /// <param name="viewName">The target name of the partial razor View you want to add to the navigation.</param>
        public static async void AddPartialViewNavigation(this VsDocument source, string viewName)
        {
            string markup = $"\r\n<li><a href = \"#{viewName}Section\" class=\"js-scroll-trigger\">{viewName}</a></li>";
            await source.AddContentToEndAsync(markup);
        }

        /// <summary>
        /// Method that appends a list item to the navigation file for a view.
        /// </summary>
        /// <param name="source">The source navigation.cshtml document.</param>
        /// <param name="viewName">The target name of the razor View you want to add to the navigation.</param>
        public static async void AddViewNavigation(this VsDocument source, string viewName)
        {            
            string markup = $"\r\n<li><a class=\"navbar - brand\" asp-controller=\"{viewName}\" asp-action=\"{viewName}\">{viewName}</a></li>";
            await source.AddContentToEndAsync(markup);
        }

        /// <summary>
        /// Method that appends auto-generated default ViewData[] and @page attributes to your razor view.
        /// </summary>
        /// /// <param name="viewName">The target name of the razor View you want to use in your ViewData[] definition.</param>
        public static async Task<VsDocument> AddViewDataAttributesAsync(this VsDocument source, string viewName)
        {
            await source.AddContentToBeginningAsync(GenerateViewDataAttributes("Title", viewName));
            return source;
        }

        /// <summary>
        /// Helper Method that auto-generates the razor view markup that defines a default ViewData[]
        /// </summary>
        /// <param name="viewName">The target name of the razor View you want to associate with the ViewData[] attribute.</param>
        public static string GenerateViewDataAttributes(string viewDataName, string viewName)
        {
            var formatter = new SourceFormatter();
            formatter.AppendCodeLine(0, "@{");
            formatter.AppendCodeLine(1, $"ViewData[\"{viewDataName}\"] = \"{viewName}\";");
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);
            return formatter.ReturnSource();
        }
        
    }
}
