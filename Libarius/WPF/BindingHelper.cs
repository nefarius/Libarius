using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Libarius.WPF
{
    public static class BindingHelper
    {
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject dependencyObject)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                yield return VisualTreeHelper.GetChild(dependencyObject, i);
            }
        }

        public static IEnumerable<DependencyObject> EnumerateVisualDescendents(this DependencyObject dependencyObject)
        {
            yield return dependencyObject;

            foreach (var child in dependencyObject.EnumerateVisualChildren())
            {
                foreach (var descendent in child.EnumerateVisualDescendents())
                {
                    yield return descendent;
                }
            }
        }

        public static void UpdateBindingSources(this DependencyObject dependencyObject)
        {
            foreach (var element in dependencyObject.EnumerateVisualDescendents())
            {
                var localValueEnumerator = element.GetLocalValueEnumerator();
                while (localValueEnumerator.MoveNext())
                {
                    var bindingExpression = BindingOperations.GetBindingExpressionBase(element,
                        localValueEnumerator.Current.Property);
                    if (bindingExpression != null)
                    {
                        bindingExpression.UpdateSource();
                    }
                }
            }
        }

        public static void UpdateBindingSources(this Application application)
        {
            foreach (Window window in application.Windows)
            {
                window.UpdateBindingSources();
            }
        }
    }
}