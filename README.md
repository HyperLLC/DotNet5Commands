# DotNet5Commands
CodeFactory Commands library that provides a reference implementation of automation for ASP.Net .Net 5 projects. You can use this commands library to automate the code generation of the initial plumbing code for your views, partial views, models, interfaces, and navigation structure within an .Net 5 ASP.NET MVC Web Application.  The best thing about this concept is that all of your plumbing and project artifacts are generated the same way EVERY time!

## New to CodeFactory?
In the simplest terms, CodeFactory is a real time software factory that is triggered from inside Visual Studio during the design and construction of software. CodeFactory allows for development staff to automate repetitive development tasks that take up developer’s time.

Please see the following link for further information and guidance about the [CodeFactory Runtime](https://github.com/CodeFactoryLLC/CodeFactory) or the [CodeFactory SDK](https://www.nuget.org/packages/CodeFactorySDK/). 

Register here for a free trial license of [CodeFactory RT](https://www.codefactory.software/freetrial).

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
  - **Action Description**:  In creating demo-code for this reference implementation, we quickly realized that you can't simply right-click on a file and count the lines of code, to see how many lines of code we've generated within a single session.  Within VS Code, you have an action to count lines of code, but not within Visual Studio.  So when you find yourself in a situation where Microsoft is lacking something you want, you can not go build any command that fits your needs.
  
- Command: Add New View
  - **Enabled**: Anytime you right-click on the Views Folder.
  - **Action Description**:  This command will prompt a dialog box, asking you to select a View Template, give it a Name, and then whether or not to add a reference of your new view to the _Navigation.cshtml view automatically.  This command will create a new folder for your new view, add your new razor view within that folder, create a new controller class within the Controllers folder, prepend the ViewData attribute to your new view, and add your asp-action link markup to the _Navigation.cshtml view.
  
- Command: Add a Section
  - **Enabled**: Anytime you right-click on an existing View cshtml file.
  - **Action Description**:  This command will prompt a dialog box asking you to select a Partial View Template, give it a name, and then select whether or not you want a reference link to this partial view section included within the navigation bar in the header. This command will create a new Partial view (we call them Sections for conversational purposes) within the root folder of the selected View.  it will then add a new ActionResult to the corresponding controller associated to the selected view.  It will also create the new Partial View cshtml file, and add it to the _Navigation.cshml if you've told it to.

