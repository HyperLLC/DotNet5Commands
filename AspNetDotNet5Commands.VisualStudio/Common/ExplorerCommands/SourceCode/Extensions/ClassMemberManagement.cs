using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetDotNet5Commands.VisualStudio.Common.Constants;
using AspNetDotNet5Commands.VisualStudio.ExplorerCommands.SourceCode.Extensions;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Formatting.CSharp;
using CodeFactory.VisualStudio;

namespace AspNetDotNet5Commands.VisualStudio.Common.ExplorerCommands.SourceCode.Extensions
{
    /// <summary>
    /// Logic class that manages logic to maintain members in a class
    /// </summary>
    public static class ClassMemberManagement
    {
        /// <summary>
        /// Adds the missing members to a target class.
        /// </summary>
        /// <param name="source">The source code that contains the target class to be updated.</param>
        /// <param name="targetClass">The target class to have the members added to.</param>
        /// <param name="missingMembers">The missing members to be added to the target class.</param>
        /// <param name="boundsCheckLogic">Optional parameter that determines if bounds checking logic should be added to methods. This will default to true.</param>
        /// <param name="loggingLogic">Optional parameter that determines if enter, exit and exception management logic should be added to methods.</param>
        /// <param name="supportAsyncMethods">Optional parameter that determines if methods will use the async keyword when support multi-threaded operations.</param>
        /// <returns>The updated SourceCode Model once the missing members have been added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if data needed to process a member is not passed.</exception>
        /// <exception cref="CodeFactoryException">Thrown is model data is not valid or a processing error occurs.</exception>
        public static async Task<CsSource> AddMembersToClassAsync(CsSource source, CsClass targetClass,
            IReadOnlyList<CsMember> missingMembers, bool boundsCheckLogic = true, bool loggingLogic = true, bool supportAsyncMethods = true)
        {

            //Bounds checking to confirm all needed data was passed.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetClass == null) throw new ArgumentNullException(nameof(targetClass));
            if (missingMembers == null) throw new ArgumentNullException(nameof(missingMembers));

            //Confirming the target models are loaded and are source code
            if (!source.IsLoaded) throw new CodeFactoryException("Source code model was not loaded.");
            if (!targetClass.IsLoaded) throw new CodeFactoryException("The target class model data is not loaded.");
            if (!targetClass.LoadedFromSource) throw new CodeFactoryException("Target class is not loaded from source code cannot update.");

            //If an empty list of missing members was provided then return the original source code
            if (!missingMembers.Any()) return source;

            //Variables used to hold the most current version of the code factory models that are being updated.
            CsClass currentClassModel = targetClass;
            CsSource currentSourceModel = source;

            currentSourceModel = await currentSourceModel.AddMissingNamespaces(missingMembers, currentClassModel.Namespace);
            currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
            if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            if (loggingLogic)
            {
                //Checking to make sure there is a logger field implemented if not add it to the class.
                currentSourceModel = await currentClassModel.AddMicrosoftExtensionsLoggerFieldAsync(DotNetConstants.FieldNameLogger,
                        currentSourceModel);
            }

            currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
            if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            //Loading a namespace manager that will update types definitions to use the correct namespace format. This reads in all the using statements assigned to the class.
            var namespaceManager = currentSourceModel.LoadNamespaceManager(currentClassModel.Namespace);

            //Processing the missing members
            foreach (var member in missingMembers)
            {
                //Clearing the formatted source code for the member before processing each member
                string sourceCode = null;

                switch (member.MemberType)
                {
                    case CsMemberType.Event:

                        //Validating the member event model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsEvent eventModel)) continue;

                        //Formatting a default event definition.
                        sourceCode = FormatMemberEvent(eventModel, namespaceManager);

                        break;

                    case CsMemberType.Method:

                        //Validating the member event model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsMethod methodModel)) continue;

                        //Formatting the method implementation based on the optional parameters provided.
                        sourceCode = FormatMemberMethod(methodModel, namespaceManager, loggingLogic, boundsCheckLogic,
                            supportAsyncMethods);

                        break;

                    case CsMemberType.Property:

                        //Validating the member property model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsProperty propertyModel)) continue;

                        //Formatting a default property definition.
                        sourceCode = FormatMemberProperty(propertyModel, namespaceManager);

                        break;

                    default:
                        continue;

                }

                if (string.IsNullOrEmpty(sourceCode)) continue;

                //Creating a source formatter and appending the final output with two indent levels to within the body over a class.
                var sourceFormatter = new CodeFactory.SourceFormatter();

                sourceFormatter.AppendCodeBlock(2, sourceCode);

                currentSourceModel = await currentClassModel.AddToEndAsync(sourceFormatter.ReturnSource());
                if (currentSourceModel == null) throw new CodeFactoryException("Cannot load the source code file, cannot complete adding interface members");

                currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
                if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            }

            return currentSourceModel;

        }

        /// <summary>
        /// Implements a default property implementation for a missing member.
        /// </summary>
        /// <param name="memberData">Property data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <returns>The fully formatted property source code or null if the member could not be implemented.</returns>
        public static string FormatMemberProperty(CsProperty memberData, NamespaceManager manager)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a default property signature.
            string propertySyntax = memberData.CSharpFormatDefaultPropertySignature(manager);

            //If the property syntax was not created return.
            if (string.IsNullOrEmpty(propertySyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the property declaration
            formatter.AppendCodeLine(0, propertySyntax);

            //Adding a extra line feed at the end of the declaration.
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Implements a default event implementation for a missing member.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <returns>The fully formatted event source code or null if the member could not be implemented.</returns>
        public static string FormatMemberEvent(CsEvent memberData, NamespaceManager manager)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a default event signature.
            string eventSyntax = memberData.CSharpFormatEventDeclaration(manager);

            //If the event syntax was not created return.
            if (string.IsNullOrEmpty(eventSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the event declaration
            formatter.AppendCodeLine(0, eventSyntax);

            //Adding a extra line feed at the end of the declaration.
            formatter.AppendCodeLine(0);

            //The source formatter returning the final results.
            return formatter.ReturnSource();
        }

        /// <summary>
        /// Implements a default event implementation for a missing member.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="includeLogging">Flag that determines if enter, exit, and error logging should be included in a method implementation.</param>
        /// <param name="includeBoundsCheck">Flag that determines if string and nullable type bounds checking should be included in a method implementation.</param>
        /// <param name="supportAsyncKeyword">Flag that determines if methods should be implemented with the async keyword when supported by the method implementation.</param>
        /// <returns>The fully formatted method source code or null if the member could not be implemented.</returns>
        public static string FormatMemberMethod(CsMethod memberData, NamespaceManager manager, bool includeLogging,
            bool includeBoundsCheck, bool supportAsyncKeyword)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a method signature.
            string methodSyntax = supportAsyncKeyword
                ? memberData.CSharpFormatStandardMethodSignatureWithAsync(manager)
                : memberData.CSharpFormatStandardMethodSignature(manager);

            //If the method syntax was not created return.
            if (string.IsNullOrEmpty(methodSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the method declaration
            formatter.AppendCodeLine(0, methodSyntax);
            formatter.AppendCodeLine(0, "{");

            //Adding enter logging if logging is enabled.
            if (includeLogging)
            {
                formatter.AppendCodeLine(1, "_logger.LogInformation(\"Entering\");");
                formatter.AppendCodeLine(0);
            }

            //Processing each parameter for bounds checking if bounds checking is enabled.
            if (includeBoundsCheck & memberData.HasParameters)
            {

                foreach (ICsParameter paramData in memberData.Parameters)
                {
                    //If the parameter has a default value then continue will not bounds check parameters with a default value.
                    if (paramData.HasDefaultValue) continue;

                    //If the parameter is a string type add the following bounds check
                    if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if(string.IsNullOrEmpty({paramData.Name}))");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.LogError($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.LogInformation(\"Exiting\");");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ArgumentNullException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }

                    // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                    if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if({paramData.Name} == null)");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.LogError($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.LogInformation(\"Exiting\");");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ArgumentNullException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }
                }
            }

            //Formatting standard try block for method
            formatter.AppendCodeLine(1, "try");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "//TODO: add execution logic here");
            formatter.AppendCodeLine(1, "}");

            //Formatting standard catch block for method
            formatter.AppendCodeLine(1, "catch (Exception unhandledException)");
            formatter.AppendCodeLine(1, "{");

            //If logging is supported capture the exception and log it as an error. Notify that an error has occured and log it . then log exiting the method and throw a scrubbed exception.
            if (includeLogging)
            {
                formatter.AppendCodeLine(2, "_logger.LogError(unhandledException, \"An unhandled exception occured, see the exception for details.Will throw a UnhandledLogicException\");");
                formatter.AppendCodeLine(2, "_logger.LogInformation(\"Exiting\");");
                formatter.AppendCodeLine(2, "throw new Exception(\"An unhandled error occured, check the logs for details.\");");
            }
            //If no logging is supported then add a to do to add exception handling
            else
            {
                formatter.AppendCodeLine(2, "//TODO: Add exception handling for unhandledException");
            }

            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);

            //If logging add a logging exit statement.
            if (includeLogging) formatter.AppendCodeLine(1, "_logger.LogInformation(\"Exiting\");");

            //Add an exception for not implemented until the developer updates the logic.
            formatter.AppendCodeLine(1, "throw new NotImplementedException();");

            //if the return type is not void then add a to do message for the developer to add return logic.
            if (!memberData.IsVoid)
            {
                formatter.AppendCodeLine(0);
                formatter.AppendCodeLine(1, "//TODO: add return logic here");
            }
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            //Returning the fully formatted method.
            return formatter.ReturnSource();

        }


        #region CommondDeliveryFramework Implementation

        /// <summary>
        /// Adds the missing members to a target class, that support the common delivery framework implementation..
        /// </summary>
        /// <param name="source">The source code that contains the target class to be updated.</param>
        /// <param name="targetClass">The target class to have the members added to.</param>
        /// <param name="missingMembers">The missing members to be added to the target class.</param>
        /// <param name="boundsCheckLogic">Optional parameter that determines if bounds checking logic should be added to methods. This will default to true.</param>
        /// <param name="loggingLogic">Optional parameter that determines if enter, exit and exception management logic should be added to methods.</param>
        /// <param name="supportAsyncMethods">Optional parameter that determines if methods will use the async keyword when support multi-threaded operations.</param>
        /// <param name="supportsCDFAspNet">Optional parameters that determines if add members support aspnet error handling with CommonDeliveryFramework.</param>
        /// <returns>The updated SourceCode Model once the missing members have been added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if data needed to process a member is not passed.</exception>
        /// <exception cref="CodeFactoryException">Thrown is model data is not valid or a processing error occurs.</exception>
        public static async Task<CsSource> AddMembersToClassWithCDFSupportAsync(CsSource source, CsClass targetClass,
            IReadOnlyList<CsMember> missingMembers, bool boundsCheckLogic = true, bool loggingLogic = true, bool supportAsyncMethods = true, bool supportsCDFAspNet = false)
        {

            //Bounds checking to confirm all needed data was passed.
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targetClass == null) throw new ArgumentNullException(nameof(targetClass));
            if (missingMembers == null) throw new ArgumentNullException(nameof(missingMembers));

            //Confirming the target models are loaded and are source code
            if (!source.IsLoaded) throw new CodeFactoryException("Source code model was not loaded.");
            if (!targetClass.IsLoaded) throw new CodeFactoryException("The target class model data is not loaded.");
            if (!targetClass.LoadedFromSource) throw new CodeFactoryException("Target class is not loaded from source code cannot update.");

            //If an empty list of missing members was provided then return the original source code
            if (!missingMembers.Any()) return source;

            //Variables used to hold the most current version of the code factory models that are being updated.
            CsClass currentClassModel = targetClass;
            CsSource currentSourceModel = source;

            currentSourceModel = await currentSourceModel.AddMissingNamespaces(missingMembers, currentClassModel.Namespace);
            currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
            if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            //Checking to make sure there is a logger field implemented if not add it to the class.
            currentSourceModel = await currentClassModel.AddMicrosoftExtensionsLoggerFieldAsync(DotNetConstants.FieldNameLogger,
                    currentSourceModel);

            currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
            if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");


            currentSourceModel = await currentSourceModel.AddUsingStatementAsync(DotNetConstants.CommonDeliveryFrameworkLibraryName);
            currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
            if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            if (supportsCDFAspNet)
            {
                //Confirming CDF ASPnet namespace is added to the code file.
                currentSourceModel = await currentSourceModel.AddUsingStatementAsync(DotNetConstants.CommonDeliveryFrameworkAspNetLibraryName);
                currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
                if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

                //Confirming the mvc namespace is added to the code file.
                currentSourceModel = await currentSourceModel.AddUsingStatementAsync(DotNetConstants.MvcNamespace);
                currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
                if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            }

            if (supportAsyncMethods)
            {
                //Confirming the mvc namespace is added to the code file.
                currentSourceModel = await currentSourceModel.AddUsingStatementAsync(DotNetConstants.SystemThreadingTasksNamespace);
                currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
                if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            }

            //Loading a namespace manager that will update types definitions to use the correct namespace format. This reads in all the using statements assigned to the class.
            var namespaceManager = source.LoadNamespaceManager(currentClassModel.Namespace);

            bool isControllerClass = currentClassModel.IsController();

            //Processing the missing members
            foreach (var member in missingMembers)
            {
                //Clearing the formatted source code for the member before processing each member
                string sourceCode = null;

                switch (member.MemberType)
                {
                    case CsMemberType.Event:

                        //Validating the member event model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsEvent eventModel)) continue;

                        //Formatting a default event definition.
                        sourceCode = FormatMemberEvent(eventModel, namespaceManager);

                        break;

                    case CsMemberType.Method:

                        //Validating the member event model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsMethod methodModel)) continue;

                        bool isControllerAction = false;

                        if (supportsCDFAspNet & isControllerClass)
                        {
                            isControllerAction = IsControllerAction(methodModel);
                        }

                        //Formatting the method implementation based on the optional parameters provided.
                        sourceCode = FormatMemberMethod(methodModel, namespaceManager, loggingLogic, boundsCheckLogic,
                            supportAsyncMethods, isControllerAction);

                        break;

                    case CsMemberType.Property:

                        //Validating the member property model loaded correctly if not return to the top. No exception will be thrown.
                        if (!(member is CsProperty propertyModel)) continue;

                        //Formatting a default property definition.
                        sourceCode = FormatMemberProperty(propertyModel, namespaceManager);

                        break;

                    default:
                        continue;

                }

                if (string.IsNullOrEmpty(sourceCode)) continue;

                //Creating a source formatter and appending the final output with two indent levels to within the body over a class.
                var sourceFormatter = new CodeFactory.SourceFormatter();

                sourceFormatter.AppendCodeBlock(2, sourceCode);

                currentSourceModel = await currentClassModel.AddToEndAsync(sourceFormatter.ReturnSource());
                if (currentSourceModel == null) throw new CodeFactoryException("Cannot load the source code file, cannot complete adding interface members");

                currentClassModel = currentSourceModel.GetModel(currentClassModel.LookupPath) as CsClass;
                if (currentClassModel == null) throw new CodeFactoryException("Cannot load class data, cannot complete adding interface members");

            }

            return currentSourceModel;

        }

        /// <summary>
        /// Implements a common delivery framework method implementation for a missing member.
        /// </summary>
        /// <param name="memberData">Event data to be loaded.</param>
        /// <param name="manager">The namespace manager to use for namespace management with type declarations.</param>
        /// <param name="includeLogging">Flag that determines if enter, exit, and error logging should be included in a method implementation.</param>
        /// <param name="includeBoundsCheck">Flag that determines if string and nullable type bounds checking should be included in a method implementation.</param>
        /// <param name="supportAsyncKeyword">Flag that determines if methods should be implemented with the async keyword when supported by the method implementation.</param>
        /// <param name="isControllerAction">Flag that determines if the the method is a controller action. This will alter the error management logic.</param>
        /// <returns>The fully formatted method source code or null if the member could not be implemented.</returns>
        public static string FormatMemberMethod(CsMethod memberData, NamespaceManager manager, bool includeLogging,
        bool includeBoundsCheck, bool supportAsyncKeyword, bool isControllerAction)
        {
            //Bounds checking to make sure all data that is needed is provided. If any required data is missing will return null.
            if (memberData == null) return null;
            if (!memberData.IsLoaded) return null;
            if (manager == null) return null;

            //C# helper used to format output syntax. 
            var formatter = new CodeFactory.SourceFormatter();

            //Using the formatter helper to generate a method signature.
            string methodSyntax = supportAsyncKeyword
                ? memberData.CSharpFormatStandardMethodSignatureWithAsync(manager)
                : memberData.CSharpFormatStandardMethodSignature(manager);

            //If the method syntax was not created return.
            if (string.IsNullOrEmpty(methodSyntax)) return null;

            //If the member has document then will build the documentation.
            if (memberData.HasDocumentation)
                //Using a documentation helper that will generate an enumerator that will output all XML documentation for the member.
                foreach (var documentation in memberData.CSharpFormatXmlDocumentationEnumerator())
                {
                    //Appending each xml document line to the being of the member definition.
                    formatter.AppendCodeLine(0, documentation);
                }

            //The member has attributes assigned to it, append the attributes.
            if (memberData.HasAttributes)
                //Using a documentation helper that will generate an enumerator that will output each attribute definition.
                foreach (var attributeSyntax in memberData.Attributes.CSharpFormatAttributeDeclarationEnumerator(manager))
                {
                    //Appending each attribute definition before the member definition.
                    formatter.AppendCodeLine(0, attributeSyntax);
                }

            //Adding the method declaration
            formatter.AppendCodeLine(0, methodSyntax);
            formatter.AppendCodeLine(0, "{");

            //Adding enter logging if logging is enabled.
            if (includeLogging)
            {
                formatter.AppendCodeLine(1, "_logger.InformationEnterLog();");
                formatter.AppendCodeLine(0);
            }

            //Processing each parameter for bounds checking if bounds checking is enabled.
            if (includeBoundsCheck & memberData.HasParameters)
            {

                foreach (ICsParameter paramData in memberData.Parameters)
                {
                    //If the parameter has a default value then continue will not bounds check parameters with a default value.
                    if (paramData.HasDefaultValue) continue;

                    //If the parameter is a string type add the following bounds check
                    if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if(string.IsNullOrEmpty({paramData.Name}))");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ValidationException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }

                    // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                    if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType)
                    {
                        //Adding an if check 
                        formatter.AppendCodeLine(1, $"if({paramData.Name} == null)");
                        formatter.AppendCodeLine(1, "{");

                        //If logging was included add error logging and exit logging
                        if (includeLogging)
                        {
                            formatter.AppendCodeLine(2,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                        }

                        //Adding a throw of an argument null exception
                        formatter.AppendCodeLine(2, $"throw new ValidationException(nameof({paramData.Name}));");
                        formatter.AppendCodeLine(1, "}");
                        formatter.AppendCodeLine(0);
                    }
                }
            }

            //Formatting standard try block for method
            formatter.AppendCodeLine(1, "try");
            formatter.AppendCodeLine(1, "{");
            formatter.AppendCodeLine(2, "//TODO: add execution logic here");
            formatter.AppendCodeLine(1, "}");

            //Formatting managed exception block for method
            if (!isControllerAction)
            {
                formatter.AppendCodeLine(1, "catch (ManagedException)");
                formatter.AppendCodeLine(1, "{");
                formatter.AppendCodeLine(2, "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
                formatter.AppendCodeLine(2, "throw;");
                formatter.AppendCodeLine(1, "}");
            }
            else
            {
                formatter.AppendCodeLine(1, "catch (ManagedException managedException)");
                formatter.AppendCodeLine(1, "{");
                formatter.AppendCodeLine(2, "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
                formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                formatter.AppendCodeLine(2, "return this.CreateProblemResult(managedException);");
                formatter.AppendCodeLine(1, "}");
            }

            //Formatting standard catch block for method
            formatter.AppendCodeLine(1, "catch (Exception unhandledException)");
            formatter.AppendCodeLine(1, "{");

            if (!isControllerAction)
            {
                formatter.AppendCodeLine(2,
                    "_logger.ErrorLog(\"An unhandled exception occured, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                formatter.AppendCodeLine(2, "throw new UnhandledException();");
            }
            else
            {
                formatter.AppendCodeLine(2, " _logger.ErrorLog(\"An unhandled exception occured, see the exception for details. Will throw a UnhandledLogicException\", unhandledException);");
                formatter.AppendCodeLine(2, "_logger.InformationExitLog();");
                formatter.AppendCodeLine(2, "var unhandledError = new UnhandledException();");
                formatter.AppendCodeLine(2, "return this.CreateProblemResult(unhandledError);");
            }

            formatter.AppendCodeLine(1, "}");
            formatter.AppendCodeLine(0);

            //If logging add a logging exit statement.
            formatter.AppendCodeLine(1, "_logger.InformationExitLog();");

            //Add an exception for not implemented until the developer updates the logic.
            formatter.AppendCodeLine(1, "throw new NotImplementedException();");

            //if the return type is not void then add a to do message for the developer to add return logic.
            if (!memberData.IsVoid)
            {
                formatter.AppendCodeLine(0);
                formatter.AppendCodeLine(1, "//TODO: add return logic here");
            }
            formatter.AppendCodeLine(0, "}");
            formatter.AppendCodeLine(0);

            //Returning the fully formatted method.
            return formatter.ReturnSource();

        }

        /// <summary>
        /// Helper method that will confirm the method is a controller action.
        /// </summary>
        /// <param name="source">The source method to check.</param>
        /// <returns>True if the method is a controller action.</returns>
        private static bool IsControllerAction(CsMethod source)
        {
            bool result = false;
            //Bounds and default value checking
            if (source == null) return false;
            if (!source.IsLoaded) return false;
            if (source.IsVoid) return false;

            var returnType = source.ReturnType;

            //Checking to see if the return type is a task type
            if (returnType.IsClass & (returnType.Namespace == DotNetConstants.SystemThreadingTasksNamespace &
                                      returnType.Name == DotNetConstants.TaskClassName))
            {
                //Getting the first parameter from the task
                var parameter = returnType.GenericParameters.FirstOrDefault();

                //Setting the return type to check
                returnType = parameter?.Type;

                //If no type data was found then return false.
                if (returnType == null) return false;
            }

            if (returnType.IsInterface)
            {
                result = (returnType.Namespace == DotNetConstants.MvcNamespace &
                          returnType.Name == DotNetConstants.ActionResultInterfaceName);
            }
            else
            {
                result = (returnType.Namespace == DotNetConstants.MvcNamespace &
                          returnType.Name == DotNetConstants.ActionResultClassName);

                if (!result)
                    result = returnType.InheritsBaseClass(DotNetConstants.ActionResultClassName,
                        DotNetConstants.MvcNamespace);
            }

            return result;
        }

        /// <summary>
        /// Method that adds all of the missing members to a class.  
        /// </summary>
        /// <param name="source">The source VSProjectFolder that this method is extending.</param>
        /// <returns>VSCSharp Source code with all missing members.</returns>
        public static async Task<VsCSharpSource> GetMissingMembers(this VsCSharpSource source)
        {
            //Get the missing members and the target class or classes they are to be loaded into.
            var missingMembers = source.SourceMissingInterfaceMembers();

            //Getting the hosting project for the command.
            var hostingProject = await source.GetHostingProjectAsync();

            //If no hosting project can be found this command should not be executed.
            if (hostingProject == null) return null;

            //Determining if the project supports the asp.net extensions for the common delivery framework
            bool supportCDFAspnet = await hostingProject.HasReferenceLibraryAsync(DotNetConstants.CommonDeliveryFrameworkAspNetLibraryName);

            //If the logging abstraction library is loaded then enable logging for members.
            var enableLogging = true;

            //Bounds checking will be supported for this command
            bool boundsChecking = true;

            //Async keyword will be used for this command
            bool supportAsync = true;

            //Process each class missing members 
            foreach (var missingMember in missingMembers)
            {
                //Get the container model that has missing members.
                var container = missingMember.Key;

                //Confirming the container is a class if not continue
                if (container.ContainerType != CsContainerType.Class) continue;

                var targetClass = container as CsClass;

                //Adding the missing members
                await ClassMemberManagement.AddMembersToClassWithCDFSupportAsync(source.SourceCode, targetClass, missingMember.Value, boundsChecking, enableLogging, supportAsync);
            }

            return source;
        }
    
        #endregion
    }
}
