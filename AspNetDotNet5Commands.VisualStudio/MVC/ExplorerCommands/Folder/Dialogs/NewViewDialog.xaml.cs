using AspNetDotNet5Commands.VisualStudio.Common.Models;
using AspNetToDotNet5.Automation.Common.Enums;
using CodeFactory.DotNet.CSharp;
using CodeFactory.Logging;
using CodeFactory.VisualStudio;
using CodeFactory.VisualStudio.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AspNetDotNet5Commands.VisualStudio.MVC.ExplorerCommands.Folder.Dialogs
{
    /// <summary>
    /// Interaction logic for NewViewDialog.xaml
    /// </summary>
    public partial class NewViewDialog : VsUserControl
    {
        private string _dialogUserMessage = "This is for demo purposes, so there is no validation on this form.  Please make sure you ProperCase your View name and do not include spaces or special characters.";
        private MessageTypeEnum _dialogMessageType = MessageTypeEnum.Information;
        private ViewTemplateItem _selectedViewTemplate = null;
        private CsClass _selectedModelTemplate = null;
        private string _viewTitle = null;
        private bool? _addToNavigation = false;

        /// <summary>
        /// Creates an instance of the user control.
        /// </summary>
        /// <param name="vsActions">The visual studio actions that are accessible by this user control.</param>
        /// <param name="logger">The logger used by this user control.</param>
        public NewViewDialog(IVsActions vsActions, ILogger logger) : base(vsActions, logger)
        {
            //Initializes the controls on the screen and subscribes to all control events (Required for the screen to run properly)
            InitializeComponent();
        }

        #region Public Properties        
        // Dialog Message is used to set the message on the dialog box (ex. if you want to display a message, error, or warning.
        public string DialogUserMessage
        {
            get { return _dialogUserMessage; }
            set { _dialogUserMessage = value; }
        }

        // Dialog Message Type that is bound to the view, this can either be a warning, error, or information.
        public MessageTypeEnum DialogMessageType
        {
            get { return _dialogMessageType; }
            set { _dialogMessageType = value; }
        }

        // Selected View Template selected by the user on the dialog and returned upon clicking Ok.
        public ViewTemplateItem SelectedViewTemplate
        {
            get { return _selectedViewTemplate; }
            set { _selectedViewTemplate = value; }
        }

        // View name to use when creating the new View and Controllers.
        public string ViewTitle
        {
            get { return _viewTitle; }
            set { _viewTitle = value; }
        }

        // Whether or not to add the view to the _Navigation component.
        public bool? AddToNavigation
        {
            get { return _addToNavigation; }
            set { _addToNavigation = value; }
        }

        // Selected View Template selected by the user on the dialog and returned upon clicking Ok.
        public CsClass SelectedModel
        {
            get { return _selectedModelTemplate; }
            set { _selectedModelTemplate = value; }
        }

        #endregion

        #region Dependency Properties
        /// <summary>
        /// The solution projects that will be used by the dialog to select which view template we will use when generating your new view markup.
        /// </summary>
        public IEnumerable<ViewTemplateItem> ViewList
        {
            get { return (IEnumerable<ViewTemplateItem>)GetValue(ViewListProperty); }
            set { SetValue(ViewListProperty, value); }
        }        

        // Using a DependencyProperty as the backing store for View Template List.  
        public static readonly DependencyProperty ViewListProperty = DependencyProperty.Register("ViewList", typeof(IEnumerable<ViewTemplateItem>), typeof(NewViewDialog), null);

        /// <summary>
        /// The solution projects that will be used by the dialog to select which model we will use to bind to your view.
        /// </summary>
        public IEnumerable<CsClass> ModelList
        {
            get { return (IEnumerable<CsClass>)GetValue(modelListProperty); }
            set { SetValue(modelListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for View Template List.  
        public static readonly DependencyProperty modelListProperty = DependencyProperty.Register("ModelList", typeof(IEnumerable<CsClass>), typeof(NewViewDialog), null);
        #endregion

        #region Button Event Management
        /// <summary>
        /// Processes the cancel button click event.
        /// </summary>
        /// <param name="sender">Hosting user control.</param>
        /// <param name="e">Ignored when used in this context.</param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            //Closing the dialog and returning control to visual studio.
            this.Close();
        }

        /// <summary>
        /// Process the ok button click event.
        /// </summary>
        /// <param name="sender">Hosting user control.</param>
        /// <param name="e">We dont use the routing args with this implementation.</param>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedViewTemplate = TemplatesCombo.SelectedItem as ViewTemplateItem;
            SelectedModel = ModelsCombo.SelectedItem as CsClass;
            ViewTitle = ViewTitleTextBox.Text;
            AddToNavigation = AddToNavigationCheckBox.IsChecked;
            this.Close();
        }
        #endregion
    }
}
