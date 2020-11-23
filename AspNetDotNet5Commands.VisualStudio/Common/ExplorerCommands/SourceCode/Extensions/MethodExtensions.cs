using CodeFactory.Formatting.CSharp;
using CodeFactory.DotNet.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDotNet5Commands.VisualStudio.ExplorerCommands.SourceCode.Extensions
{
    public static class MethodExtensions
    {     
        public static string CreateInterfaceDefinition(this CsMethod source)
        {
            return source.CSharpFormatInterfaceMethodSignature();
        }

        public static string CreateInterfaceDefinition(this CsProperty source)
        {            
            return source.CSharpFormatInterfacePropertySignature();
        }

        public static string CreateInterfaceDefinition(this CsEvent source)
        {
            return source.CSharpFormatInterfaceEventDeclaration();
        }

        public static string CreateDefaultDefinition(this CsField source)
        {
            return source.CSharpFormatFieldDeclaration();
        }

        public static string CreateDefaultDefinition(this CsMethod source)
        {
            return source.CSharpFormatMethodSignature();
        }

        public static string CreateDefaultDefinition(this CsProperty source)
        {
            return source.CSharpFormatPropertyDeclaration();
        }

        public static string CreateDefaultDefinition(this CsEvent source)
        {
            return source.CSharpFormatEventDeclaration();
        }
    }
}
