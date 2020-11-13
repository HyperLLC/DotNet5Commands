using CodeFactory.DotNet.CSharp;
using CodeFactory;
using CodeFactory.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AspNetDotNet5Commands.VisualStudio.Common.Extensions
{
    /// <summary>
    /// Class that holds extension methods for interfaces.
    /// </summary>
    public static class InterfaceExtentions
    {
        public static string GenerateInterface(this CsClass source)
        {
            string InterfaceName = source.HasStrongTypesInGenerics ? $"I{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"I{source.Name}";
            var formatter = new SourceFormatter();
            formatter.AppendCodeLine(0, $"namespace {source.GetRootFromNamespace()}.Interfaces");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"{source.Security.ToString().ToLower()} interface {InterfaceName}");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(1);
            foreach (var propertyData in source.Properties)
            {
                formatter.AppendCodeBlock(2, propertyData.Security.ToString().ToLower() + " " + propertyData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var methodData in source.Methods)
            {
                formatter.AppendCodeBlock(2, methodData.Security.ToString().ToLower() + " " + methodData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var eventsData in source.Events)
            {
                formatter.AppendCodeBlock(2, eventsData.Security.ToString().ToLower() + " " + eventsData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");
            return formatter.ReturnSource();
        }

        public static string GenerateModel(this CsContainer source)
        {
            string interfaceName = null;            

            //Based upon whether or not a class is generating the Model or a Model is rebuilding itself, we'll want to remove the interface I-prefix from the source class name
            if (source.ModelType == CsModelType.Class)
                interfaceName = source.HasStrongTypesInGenerics ? $"I{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"I{source.Name}";            
            else             
                interfaceName = source.HasStrongTypesInGenerics ? $"{source.Name}<{source.GenericParameters.FormatCSharpGenericSignatureSyntax()}>" : $"{source.Name}";

            var formatter = new SourceFormatter();
            formatter.AppendCodeLine(0, $"using {source.GetRootFromNamespace()}.Interfaces;");
            formatter.AppendCodeLine(0, "using System;");
            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0, $"namespace {source.GetRootFromNamespace()}.Models");
            formatter.AppendCodeLine(0, "{");
            formatter.AppendCodeLine(1, $"{source.Security.ToString().ToLower()} class {source.Name.Substring(1, source.Name.Length-1)}: " + GetInterfacesAsString(source) + interfaceName);
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(1);
            foreach (var propertyData in source.Properties)
            {
                formatter.AppendCodeBlock(2, propertyData.Security.ToString().ToLower() + " " + propertyData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var methodData in source.Methods)
            {
                formatter.AppendCodeBlock(2, methodData.Security.ToString().ToLower() + " " + methodData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }

            foreach (var eventsData in source.Events)
            {
                formatter.AppendCodeBlock(2, eventsData.Security.ToString().ToLower() + " " + eventsData.CreateInterfaceDefinition());
                formatter.AppendCodeLine(2);
            }
            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0, "}");
            return formatter.ReturnSource();
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
    }    
}
