using AspNetDotNet5Commands.VisualStudio.Common.Constants;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.Common.Extensions
{
    /// <summary>
    /// Class that holds extension methods for classes.
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// Extension method that searches C# source code files for a base class inheritance.
        /// </summary>
        /// <param name="source">The source visual studio project to search.</param>
        /// <param name="baseClassName">The name of the base class to search for.</param>
        /// <param name="searchChildren">Flag that determines if you search all child project folders under the project.</param>
        /// <returns>The target source code that meets the criteria or an empty list. </returns>
        public static async Task<IReadOnlyList<CsSource>> GetClassesThatInheritBaseAsync(this VsProject source, string baseClassName, bool searchChildren)
        {
            //If the project is not created return an empty list.
            if (source == null) return ImmutableList<CsSource>.Empty;

            //Calling into the CodeFactory project system api to load all project items, will pre load the source code models.
            var children = await source.GetChildrenAsync(searchChildren, true);

            //Pulling out the list of all code files.
            var sourceCodeFiles = children.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource)).Cast<VsCSharpSource>();

            //Returning the code files that implement the target base class.
            return sourceCodeFiles.Select(codeFile => codeFile.SourceCode)
                .Where(sourceCode => sourceCode.Classes.Any(c => c.BaseClass.Name.Equals(baseClassName)))
                .ToImmutableList();
        }

        /// <summary>
        /// Extension method that searches C# source code files for a base class inheritance.
        /// </summary>
        /// <param name="source">The source visual studio project to search.</param>
        /// <param name="baseClassName">The name of the base class to search for.</param>
        /// <returns>The target source code that meets the criteria or an empty list. </returns>
        public static IReadOnlyList<CsSource> GetClassesThatInheritBase(this IReadOnlyList<VsModel> source, string baseClassName)
        {
            //No source model was provided will return an empty list.
            if (source == null) return ImmutableList<CsSource>.Empty;

            //Pulling out the list of all code files.
            var sourceCodeFiles = source.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource)).Cast<VsCSharpSource>();

            //Returning the code files that meet the criteria.
            return sourceCodeFiles.Select(codeFile => codeFile.SourceCode)
                .Where(sourceCode => sourceCode.Classes.Any(c => c.BaseClass.Name.Equals(baseClassName)))
                .ToImmutableList();
        }

        /// <summary>
        /// Extension method that searches a project for a C# class that exists in one of the projects documents.
        /// </summary>
        /// <param name="source">Source Project to search through</param>
        /// <param name="className">The name of the class to search for.</param>
        /// <param name="searchChildren">Flag that determines if the entire project should be searched or just the root of the project.</param>
        /// <returns>The first instance of the class or null.</returns>
        public static async Task<CsClass> FindClassAsync(this VsProject source, string className, bool searchChildren)
        {
            //Loading the visual studio models from the project and pre creating the source code files.
            var children = await source.GetChildrenAsync(searchChildren, true);

            //Extracting all the c# source code files from the returned models.
            var sourceCodeFiles = children.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource)).Cast<VsCSharpSource>();

            //Getting the first code file that contains the class. Returning either null or the found class.
            return sourceCodeFiles.FirstOrDefault(s => s.SourceCode.Classes.Any(c => c.Name.Equals(className)))
                ?.SourceCode.Classes.FirstOrDefault(c => c.Name.Equals(className));
        }

        /// <summary>
        /// Extension method that searches a list of project models for a C# class that exists in one of the projects documents.
        /// </summary>
        /// <param name="source">List of visual studio models to search</param>
        /// <param name="className">The name of the class to search for.</param>
        /// <returns>The first instance of the class or null.</returns>
        public static CsClass FindClass(this IReadOnlyList<VsModel> source, string className)
        {
            //Extracting all the c# source code files from the returned models.
            var sourceCodeFiles = source.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource)).Cast<VsCSharpSource>();

            //Getting the first code file that contains the class. Returning either null or the found class.
            return sourceCodeFiles.FirstOrDefault(s => s.SourceCode.Classes.Any(c => c.Name.Equals(className)))
                ?.SourceCode.Classes.FirstOrDefault(c => c.Name.Equals(className));
        }

        /// <summary>
        /// Gets target classes that implement a target interface. It will skip classes that implement Page or HttpApplication.
        /// </summary>
        /// <param name="source">The project to search for the classes in.</param>
        /// <param name="interfaceName">The name of the interface to search for.</param>
        /// <param name="searchChildren">Flag to determine if sub folder should be searched or just the root project folder.</param>
        /// <returns>Readonly list of the found source code files with the target classes in them. or an empty list.</returns>
        public static async Task<IReadOnlyList<CsSource>> GetClassesThatImplementInterfaceAsync(this VsProject source, string interfaceName, bool searchChildren)
        {
            //Bounds check will return an empty list if no project was provided.
            if (source == null) return ImmutableList<CsSource>.Empty;

            //Calls into the CodeFactory project system and gets the children of the supplied project. Will load all code files that support C# as CSharpSource files.
            var children = await source.GetChildrenAsync(searchChildren, true);

            //Extracting all the C# code files from the returned project data.
            var codeFiles = children.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource))
                .Cast<VsCSharpSource>();

            //Collection all the code files that meet the criteria and returning the source code models for each.
            return codeFiles.Where(s => s.SourceCode.Classes.Any(c =>
                            (!c.BaseClass.Name.Equals("Page") && !c.BaseClass.Name.Equals("HttpApplication"))
                            &&
                            c.InheritedInterfaces.Any(x => x.Name.Equals(interfaceName))))
                    .Select(s => s.SourceCode)
                    .ToImmutableList();
        }

        /// <summary>
        /// Gets target classes that implement a target interface. It will skip classes that implement Page or HttpApplication.
        /// </summary>
        /// <param name="source">The list of visual studio models to search for the classes in.</param>
        /// <param name="interfaceName">The name of the interface to search for.</param>
        /// <returns>Readonly list of the found source code files with the target classes in them. or an empty list.</returns>
        public static IReadOnlyList<CsSource> GetClassesThatImplementInterface(this IReadOnlyList<VsModel> source, string interfaceName)
        {
            //Bounds check will return an empty list if no project was provided.
            if (source == null) return ImmutableList<CsSource>.Empty;

            //Extracting all the C# code files from the returned project data.
            var codeFiles = source.Where(p => p.ModelType.Equals(VisualStudioModelType.CSharpSource))
                .Cast<VsCSharpSource>();

            //Collection all the code files that meet the criteria and returning the source code models for each.
            return codeFiles.Where(s => s.SourceCode.Classes.Any(c =>
                    (!c.BaseClass.Name.Equals("Page") && !c.BaseClass.Name.Equals("HttpApplication"))
                    &&
                    c.InheritedInterfaces.Any(x => x.Name.Equals(interfaceName))))
                .Select(s => s.SourceCode)
                .ToImmutableList();
        }

        /// <summary>
        /// Extension method that determines if the class implements a logger field that supports extensions logging from Microsoft.
        /// </summary>
        /// <param name="source">Class model to evaluate.</param>
        /// <param name="loggerName">The name of the logger field.</param>
        /// <returns>Boolean flag if the field was found or not.</returns>
        public static bool HasMicrosoftExtensionsLoggerField(this CsClass source, string loggerName)
        {
            //Bounds check to determine if a class model was provided.
            if (source == null) return false;

            //Bounds check to confirm a field name was provided for the logger.
            if (String.IsNullOrEmpty(loggerName)) return false;

            //Bounds check to confirm the target class has fields. 
            if (!source.Fields.Any()) return false;

            //Looking up the field definition by the variable name of the logger.
            var field = source.Fields.FirstOrDefault(f => f.Name == loggerName);

            //If the logger was not found return false.
            if (field == null) return false;

            //Confirming the fields data type is under the logger namespace.
            if (field.DataType.Namespace != DotNetConstants.MicrosoftLoggerNamespace) return false;

            //Confirming the field type is the ILogger interface
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (field.DataType.Name != DotNetConstants.MicrosoftLoggerInterfaceName) return false;

            return true;
        }

        /// <summary>
        /// Extension method that determines if the logger field is implemented in the class. If it exists will return the provided source. Otherwise will add the logging namespace and the logger field.
        /// </summary>
        /// <param name="source">Source class to check for the logger field.</param>
        /// <param name="loggerName">The name of the logger field to check for.</param>
        /// <param name="parentSourceCode">The source code the class was loaded from.</param>
        /// <returns>The existing source code if the field is found, or the updated source code with the logging field added.</returns>
        public static async Task<CsSource> AddMicrosoftExtensionsLoggerFieldAsync(this CsClass source, string loggerName,
            CsSource parentSourceCode)
        {
            //Bounds checking
            if (source == null) return parentSourceCode;
            if (String.IsNullOrEmpty(loggerName)) return parentSourceCode;

            //Checking to see if the logger field already exists. If it does just return the parent source code.
            if (source.HasMicrosoftExtensionsLoggerField(loggerName)) return parentSourceCode;

            //Adding the logging namespace
            var currentSource = await parentSourceCode.AddUsingStatementAsync(DotNetConstants.MicrosoftLoggerNamespace);

            var currentClass = currentSource.GetModel(source.LookupPath) as CsClass;

            if (currentClass == null) throw new CodeFactoryException("Cannot load class data to add the logger field.");

            CodeFactory.SourceFormatter fieldSource = new CodeFactory.SourceFormatter();

            fieldSource.AppendCodeLine(2, "/// <summary>");
            fieldSource.AppendCodeLine(2, "/// Logger for all logging interactions in the class.");
            fieldSource.AppendCodeLine(2, "/// </summary>");
            fieldSource.AppendCodeLine(2, $"private readonly {DotNetConstants.MicrosoftLoggerInterfaceName} {loggerName};");
            fieldSource.AppendCodeLine(0);

            currentSource = await currentClass.AddToBeginningAsync(fieldSource.ReturnSource());

            return currentSource;

        }

        /// <summary>
        /// Helper method that confirms the class model does not implement a controller base class.
        /// </summary>
        /// <param name="classModel">The class model to confirm does not implement a base class.</param>
        /// <returns></returns>
        public static bool IsController(this CsClass classModel)
        {
            var baseClass = classModel?.BaseClass;
            if (baseClass == null) return false;

            var baseClassName = $"{baseClass.Namespace}.{baseClass.Name}";

            if (baseClassName == DotNetConstants.ControllerBaseName) return true;

            bool isBaseClass = false;
            if (baseClass.BaseClass != null) isBaseClass = IsController(baseClass);
            return isBaseClass;
        }
    }
}
