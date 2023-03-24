using KmpCrmUwp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmpCrmUwp.Resources
{
    internal class MainTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommentedVisitBatchDataTemplate { get; set; }
        public DataTemplate AddVisitBatchDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is CommentedVisitBatchViewModel)
            {
                return CommentedVisitBatchDataTemplate;
            }
            else if (item is AddVisitBatchViewModel)
            {
                return AddVisitBatchDataTemplate;
            }
            else
            {
                return null;
            }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
