using CodeFactory.DotNet.CSharp;
using CodeFactory;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetDotNet5Commands.VisualStudio.Common.Extensions;
using AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Document;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Project
{
    /// <summary>
    /// Class that holds extension methods to help with View management.
    /// </summary>
    public static class TemplateConfigExtensions
    {
        /// <summary>
        /// Method that adds a new razor view to the selected folder.  
        /// </summary>
        /// <param name="source">The source VSProjectFolder that this method is extending.</param>
        /// <param name="viewTemplateFile">The VsDocument that represents the html markup template file. If it's null we ignore it.</param>
        /// <param name="viewName">The target name of the razor View you want to create.</param>
        /// <param name="generateModel">Boolean to dictate whether or not to auto-generate a default razor view @model with corresponding model and interface files.</param>
        /// <param name="generateViewData">Boolean to dictate whether or not to auto-generate a default razor view ViewData["Title"] attribute.</param>
        public static async Task<VsDocument> GetConfigTemplateFile(this VsProjectFolder source, VsDocument viewTemplateFile, string viewName, bool generateModel, bool generateViewData)
        {
            //Create the new view containing folder
            VsProjectFolder newViewFolder = await source.CheckAddFolder(viewName);

            //Add the new view file and content                        
            VsDocument viewDocument = await newViewFolder.AddDocumentAsync(viewName + ".cshtml", await viewTemplateFile.GetDocumentContentAsStringAsync());

            //Prepend and auto-generate a default razor view Model
            // -- FUTURE FEATURE --

            //Prepend the ViewData[] Attribute(s)
            if (generateViewData)
                await viewDocument.AddViewDataAttributesAsync(viewName);

            return viewDocument;            
        }        
    }
}
