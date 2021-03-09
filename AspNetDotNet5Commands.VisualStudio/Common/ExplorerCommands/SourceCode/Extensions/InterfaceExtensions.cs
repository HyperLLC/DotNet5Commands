using CodeFactory.DotNet.CSharp;
using CodeFactory;
using CodeFactory.VisualStudio;
using System.Linq;
using System.Threading.Tasks;
using CodeFactory.VisualStudio.SolutionExplorer;
using AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.Folder.Extensions;
using AspNetDotNet5Commands.VisualStudio.Common.Constants;

namespace AspNetDotNet5Commands.VisualStudio.ExplorerCommands.SourceCode.Extensions
{
    /// <summary>
    /// Class that holds extension methods for interfaces.
    /// </summary>
    public static class InterfaceExtentions
    {
        public static object VisualStudioActions { get; private set; }

        public static string GenerateInterface(this CsClass source)
        {
            string InterfaceName = source.HasStrongTypesInGenerics ? $"I{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"I{source.Name}";
            var formatter = new SourceFormatter();

            // Check to see if we have any defined attributes, if so, include the DataAnnotations namespace.
            if (source.Properties.Any(s => s.HasAttributes) || source.Methods.Any(s => s.HasAttributes) || source.Events.Any(s => s.HasAttributes))
                formatter.AppendCodeLine(0, $"using {DotNetConstants.SystemComponentModelDataAnnotations};");

            formatter.AppendCodeLine(0, $"namespace {source.GetRootFromNamespace()}.Interfaces");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"{source.Security.ToString().ToLower()} interface {InterfaceName}");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(1);
            
            // Load the properties
            foreach (var propertyData in source.Properties.Where(f => f.LoadedFromSource))
            {
                if(propertyData.HasAttributes)
                {
                    foreach(CsAttribute attribute in propertyData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, propertyData.Security.ToString().ToLower() + propertyData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            // Load the methods
            foreach (var methodData in source.Methods.Where(f => f.LoadedFromSource))
            {
                if (methodData.HasAttributes)
                {
                    foreach (CsAttribute attribute in methodData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, methodData.Security.ToString().ToLower() + methodData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            // Load the events
            foreach (var eventsData in source.Events)
            {
                if (eventsData.HasAttributes)
                {
                    foreach (CsAttribute attribute in eventsData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, eventsData.Security.ToString().ToLower() + eventsData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");

            return RemoveInitialLineBreak(formatter.ReturnSource());
        }

        public static string GenerateModelFromInterface(this CsInterface source, bool supportCDFAspnet)
        {
            //Based upon whether or not a class is generating the Model or a Model is rebuilding itself, we'll want to remove the interface I-prefix from the source class name
            string modelName = source.Name.Substring(1, source.Name.Length - 1);
            string interfaceName = source.HasStrongTypesInGenerics ? $"{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"{source.Name}";
            var formatter = new SourceFormatter();

            // Check to see if we have any defined attributes, if so, include the DataAnnotations namespace.
            if (source.Properties.Any(s => s.HasAttributes) || source.Methods.Any(s => s.HasAttributes) || source.Events.Any(s => s.HasAttributes))
                formatter.AppendCodeLine(0, $"using {DotNetConstants.SystemComponentModelDataAnnotations};");
            formatter.AppendCodeLine(0, $"using System;");
            formatter.AppendCodeLine(0, $"using {source.GetRootFromNamespace()}.Interfaces;");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, $"namespace {source.GetRootFromNamespace()}.Models");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"{source.Security.ToString().ToLower()} class {modelName}: {interfaceName}");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(1);

            foreach (var propertyData in source.Properties.Where(f => f.LoadedFromSource))
            {
                if (propertyData.HasAttributes)
                {
                    foreach (CsAttribute attribute in propertyData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, propertyData.Security.ToString().ToLower() + propertyData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var methodData in source.Methods.Where(f => f.LoadedFromSource))
            {
                if (methodData.HasAttributes)
                {
                    foreach (CsAttribute attribute in methodData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, methodData.Security.ToString().ToLower() + methodData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var eventsData in source.Events)
            {
                if (eventsData.HasAttributes)
                {
                    foreach (CsAttribute attribute in eventsData.Attributes)
                    {
                        formatter.AppendCodeBlock(2, attribute.FormatCSharpAttributeSignatureSyntax());
                    }
                }
                formatter.AppendCodeBlock(2, eventsData.Security.ToString().ToLower() + eventsData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");

            return RemoveInitialLineBreak(formatter.ReturnSource());
        }

        public static async Task<string> RegenerateModel(this CsClass source, bool supportCDFAspnet)
        {         
            //Based upon whether or not a class is generating the Model or a Model is rebuilding itself, we'll want to remove the interface I-prefix from the source class name
            string interfaceName = source.HasStrongTypesInGenerics ? $"{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"I{source.Name}";            
           
            var formatter = new SourceFormatter();

            // Check to see if we have any defined attributes, if so, include the DataAnnotations namespace.
            if (source.Properties.Any(s => s.HasAttributes) || source.Methods.Any(s => s.HasAttributes) || source.Events.Any(s => s.HasAttributes))
                formatter.AppendCodeLine(0, $"using {DotNetConstants.SystemComponentModelDataAnnotations};");

            //If we support CDF for ASP.NET then add the microsofts logging namespace
            //TODOif (supportCDFAspnet)
                formatter.AppendCodeLine(0, $"using {DotNetConstants.MicrosoftLoggerNamespace};");

            formatter.AppendCodeLine(0, $"using {source.GetRootFromNamespace()}.Interfaces;");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, $"namespace {source.GetRootFromNamespace()}.Models");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"{source.Security.ToString().ToLower()} class {source.Name}: " + GetInterfacesAsString(source) + interfaceName);
            formatter.AppendCodeLine(1, "{");

            string body = await source.GetBodySyntaxAsync();

            formatter.AppendCodeBlock(2, RemoveInitialLineBreak(body));
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");

            return RemoveInitialLineBreak(formatter.ReturnSource());
        }

        public static string GetInterfacesAsString(CsContainer source)
        {
            var inheritedInterfaces = source.InheritedInterfaces;
            string returnSource = null;
            foreach(CsInterface interfaceName in inheritedInterfaces)
            {
                returnSource+=interfaceName.Name;
            }
            if (returnSource != null)
                returnSource += ", ";
            return returnSource;
        }

        public static async Task<VsCSharpSource> InheritInterfaceAndRegenerateModel(this VsCSharpSource source, VsProject currentProject)
        {
            var classData = source.SourceCode.Classes.FirstOrDefault();

            if (classData == null) return null;

            //Get the solution projects and create the interface folder if one doesn't exist
            VsProjectFolder interfacesFolder = await currentProject.CheckAddFolder("Interfaces");            

            //Add the new interface file.
            await interfacesFolder.AddDocumentAsync("I" + classData.Name + ".cs", classData.GenerateInterface());

            //Getting the hosting project for the command.
            var hostingProject = await source.GetHostingProjectAsync();

            //If no hosting project can be found this command should not be executed.
            if (hostingProject == null) return null;          
            
            //Update Model to Inhert from newly generated Interface
            await source.SourceCode.ReplaceAsync(await classData.RegenerateModel(await hostingProject.HasReferenceLibraryAsync(DotNetConstants.CommonDeliveryFrameworkAspNetLibraryName)));

            return source;
        }

        public static string RemoveInitialLineBreak(string source)
        {
            //HACK to remove the whitespace at the beginning that the source formatter puts there.
            string resultSource = source;
            resultSource = resultSource.Substring(2, resultSource.Length - 2);
            return resultSource;
        }
    }    
}
