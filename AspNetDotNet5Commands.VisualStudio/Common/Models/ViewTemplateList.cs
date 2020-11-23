using System;
using System.Collections.ObjectModel;

namespace AspNetDotNet5Commands.VisualStudio.Common.Models
{
    public class ViewTemplateList
    {
        public ObservableCollection<ViewTemplateItem> ViewTemplates { get; set; }

        public ObservableCollection<ViewTemplateItem> PartialViewTemplates { get; set; }
    }
}

