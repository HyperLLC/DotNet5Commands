# DotNet5Commands
CodeFactory Commands library that provides a reference implementation of automation for ASP.Net .Net 5 projects. You can use this commands library to automate the code generation of the initial plumbing code for your views, partial views, models, interfaces, and navigation structure within an .Net 5 ASP.NET MVC Web Application.  The best thing about this concept is that all of your plumbing and project artifacts are generated the same way EVERY time!

## Quick Video Links
Before you get started, it might be helpful to watch the following short videos (LINKS COMING SOON):
  - [Using CodeFactory Commands to Autogenerate .Net 5 Code (Demo)]() 
    - Quick video demonstrating the benefits of using these commands and how quickly you can spin up a new .Net 5 Public facing, real-world web application in just minutes.
  - [Getting Started with CodeFactory]()
    - Quick video on getting your environment setup using a trial license of CodeFactory and using these .Net 5 Commands. 
  - [Creating Your First CodeFactory Command 101]()
    - Quick video on creating a simple, real-world command that you can use in your every day development routine.
    
## New to CodeFactory?
In the simplest terms, CodeFactory is a real time software factory that is triggered from inside Visual Studio during the design and construction of software. CodeFactory allows for development staff to automate repetitive development tasks that take up developer’s time.

Please see the following link for further information and guidance about the [CodeFactory Runtime](https://github.com/CodeFactoryLLC/CodeFactory) or the [CodeFactory SDK](https://www.nuget.org/packages/CodeFactorySDK/). 

Register here for a free trial license of [CodeFactory for Visual Studio](https://www.codefactory.software/license-setup).

## Core purpose of this Commands Library
This commands library was built using the [CodeFactory SDK](https://www.nuget.org/packages/CodeFactorySDK/) to life as a developer simply easier.  You can literally write thousands of lines of code per hour.  Team leads, Senior Developers, and the Architects on your projects can now define your architecture patterns & design tenants, then go enforce them through a commands library just like this so that your day-to-day developers can simply "color within the lines".  This reference library has the following commands and features avaible to anyone who has a valid copy of [CodeFactory Runtime](http://www.codefactory.software) installed as an extension inside of their local copy of Visual Studio 2019 Preview (since the referring solution is .Net 5). 

## Commands Included
It's probably confusing to know exactly what we mean by Commands, so let me explain that first.  A command is any action you want to perform within the Solution Explorer window within Visual Studio.  We can target and enable commands to be available to your development team based-upon any criteria you can think of.  Below are examples of commands included within this particular reference implementation project:

- Command: Generate Model
  - **Enabled**: Anytime you right-click on an Interface file.
  - **Action Description**: Generates a model with inheritance to the respective Interface class.  The command will name your model.cs file accordingly (while dropping the "I" prefix and  place the new model.cs file within your Models folder, following tenants of an MVC pattern.

- Command: Generate Interface and Inherit
  - **Enabled**: Anytime you right-click on a model class that does not have any interfaces inherited within it.
  - **Action Description**:  Sometimes as developers we forget to create the interface first, so this command just simply does the reverse of the command above, and generates the interface based-upon the selected model. It will also update that model and add the inheritance for you automatically, and any releveant using statements.

- Command: Add Missing Members
  - **Enabled**: Anytime you right-click on a model class that inherits from an interface and is missing members from that interface within the model.
  - **Action Description**:  This command will add any and all missing members to your model class.  If the model class is missing event members, for example, it will add those event members and autogenerate code within your method for bounds checking, entrance & exit logging using the ILogger class, and add try-catch blocks with exception handling all for you.  This is a huge time saver and something every developer faces.  We use a Common Delivery Framework (CDF) to help us enforce patterns like this. 
  
- Command: Count Lines of Code
  - **Enabled**: Anytime you right-click on the Project, a Folder, a .cs Document, or a .cshtml Document.
  - **Action Description**:  In creating demo-code for this reference implementation, we quickly realized that you can't simply right-click on a file and count the lines of code, to see how many lines of code we've generated within a single session.  Within VS Code, you have an action to count lines of code, but not within Visual Studio.  So when you find yourself in a situation where Microsoft is lacking something you want, you can use CodeFactory to go build any command that fits your needs. Problem Solved!
  
- Command: Add New View
  - **Enabled**: Anytime you right-click on the Views Folder.
  - **Action Description**:  This command will prompt a dialog box, asking you to select a View Template, give it a Name, and then whether or not to add a reference of your new view to the _Navigation.cshtml view automatically.  This command will create a new folder for your new view, add your new razor view within that folder, create a new controller class within the Controllers folder, prepend the ViewData attribute to your new view, and add your asp-action link markup to the _Navigation.cshtml view.
  
- Command: Add a Section
  - **Enabled**: Anytime you right-click on an existing View cshtml file.
  - **Action Description**:  This command will prompt a dialog box asking you to select a Partial View Template, give it a name, and then select whether or not you want a reference link to this partial view section included within the navigation bar in the header. This command will create a new Partial view (we call them Sections for conversational purposes) within the root folder of the selected View.  it will then add a new ActionResult to the corresponding controller associated to the selected view.  It will also create the new Partial View cshtml file, and add it to the _Navigation.cshml if you've told it to.

## Extension Methods Included:
Here is a list of additional extension methods we've written for you on top of the existing CodeFactory SDK and Automation Templates.  I've grouped these by heirarchy in the Solution Explorer:
  - Visual Studio Project (VsProject)
    - **AddRazorViewAsync()** - Method that adds a new razor view to the selected folder.
    - **CheckAddFolder()** - Used to check a project model for the existence of a folder at the root level of a given name.  If the folder is missing - create it.
    - **FindClassAsync()** - Extension method that searches a project for a C# class that exists in one of the projects documents.
    - **FindDocumentWithinProjectAsync()** - Returns VsModel object from the VsProject that has a matching name.
    - **GetClassesThatImplementInterfaceAsync()** - Gets target classes that implement a target interface. It will skip classes that implement Page or HttpApplication.
    - **GetDocumentsWithExtensionAsync()** - Returns a list of non-source code documents from VsProject that have a matching file extension.    
    - **LoadAllProjectData()** - Returns a list of VsModels tree that represent the entire Visual Studio Project.
    
  - Visual Studio Project Folder (VsProjectFolder)
    - **AddControllerAsync()** - Method that adds a new controller file to the project within the Contollers Folder.
    - **CheckAddFolder()** - Confirms the target project sub folder exists in the project folder.  If not, it will create it within the sourced project folder.
    - **GetConfigTemplateFile()** - 
    - **GetCurrentProjectAsync()** - Gets a VsProject object referencing the project in which this folder exist.
    
  - Visual Studio Document (VsDocument)
    - **AddPartialClassToViewAsync()** - Method that appends auto-generated default ViewData[] and @page attributes to your razor view.
    - **AddPartialViewNavigation()** - Method that appends a list item to the navigation file for a partial view.
    - **AddViewDataAttributesAsync()** - Method that appends auto-generated default ViewData[] and @page attributes to your razor view.
    - **CopyProjectFile()** - Copies a VsDocument from the source location to a destination location.
    - **GetParentFolders()** - Gets a list of VSProjectFolders parent folders relative to a document.
    - **GetCurrentProjectAsync()** - Returns a VsProject object referencing the project in which this source document exist.
  
  - Visual Studio CSharp SourceCode (VsCSharpSource)
    - **GetCurrentProjectAsync()** - Returns a VsProject object referencing the project in which this CSharp source code resides.
    
  - CSharp SourceCode (CsSource)
    - **AddActionResultMethodToControllerAsync()** - Method that generates an ActionResult Method and Adds it to the list of class Members within your source controller.
    - **AddMembersToClassAsync()** - Adds the missing members to a target class.
    - **AddMemberstoClassWithCDFSupportAsync()** - Adds the missing members to a target class, that support the common delivery framework implementation.
    - **GenerateIActionResultSourceCode()** - Method that auto-generates a string-formatted representation of the ActionResult Method.
    
  - Visual Studio Model (VsModel)
    - **FindClass()** - Extension method that searches a list of project models for a C# class that exists in one of the projects documents.
    - **GetClassesThatImplementInterface()** - Gets target classes that implement a target interface. It will skip classes that implement Page or HttpApplication.
    - **GetClassesThatInheritBase()** - Extension method that searches C# source code files for a base class inheritance.
    - **GetDocumentsWithExtension()** - Returns a list of non-source code documents from VsProject that have a matching file extension.
    - **GetSourceCodeDocumentsAsync()** - Gets a list of the c# source code files from a target provided lists.
    
  - CSharp Container (CsContainer)
    - **GetRootFromNamespace()** - Gets the root of any namespace.
    
  - CSharp Class (CsClass)
    - **AddMicrosoftExtensionsLoggerFieldAsync()** - Extension method that determines if the logger field is implemented in the class. If it exists will return the provided source. Otherwise will add the logging namespace and the logger field.
    - **GenerateInterface()** - Generates a model class based-upon a CsClass sourced model.
    - **HasMicrosoftExtensionsLoggerField()** - Extension method that determines if the class implements a logger field that supports extensions logging from Microsoft.
    - **IsController()** - Helper method that confirms the class model does not implement a controller base class.
    
  - CSharp Method (CsMethod)
    - **CreateInterfaceDefinition()** - Creates a CSharp formatted interface Method Declaration.
    - **FormatMemberMethod()** - Implements a default method implementation for a missing member.
    - **IsControllerAction()** - Helper method that will confirm the method is a controller action.
    - **MethodContent()** - Gets the syntax within the body of a Method.
  
  - CSharp Propert (CsProperty)
    - **CreateInterfaceDefinition()** - Creates a CSharp formatted interface Property Declaration.
    - **FormatMemberProperty()** - Implements a default property implementation for a missing member.
    
  - CSharp Event (CsEvent)
    - **CreateInterfaceDefinition()** - Creates a CSharp formatted interface Event Declaration.
    - **FormatMemberEvent()** - Implements a default event implementation for a missing member.
    
  - CSharp Interface (CsInterface)
    - **GenerateModelFromInterface()** - Generates a model from a CsInterface Class with CDF support.
    
  - Generic Source (string)
    - **RemoveInitialLineBreak()** - This is a temporary hack, but visual studio puts in one initial line break each time we generate new code and try to inject it into the body of a code file.  This helper method removes that for you and it can be called upon any generated string object.
    
    
    
    
## Interested in Other Projects?
The CodeFactory Team has other [community projects](https://github.com/CodeFactoryLLC) you can check out!

## Trying to Migrate your old WebForms Project(s) to Blazor?
We are working on a Webforms to .Net 5 automation project using the same tenants you're seeing within this project; but if you're looking to move straight to Blazor, check out these links below:
  - [CodeFactory WebForms to Blazor Project](https://github.com/CodeFactoryLLC/WebForms2BlazorServer)
  - [Jeff Fritz Blazor Components](https://www.nuget.org/packages/Fritz.BlazorWebFormsComponents/)
  - [Jeff Fritz - Getting Started: Building Custom CodeFactory Dialogs](https://www.youtube.com/watch?v=-8_V78IyMLw&feature=youtu.be)
