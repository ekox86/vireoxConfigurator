using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VireoxConfigurator
{
    public static class VisualTreeHelperExtension
    {
        struct StackElement
        {
            public FrameworkElement Element { get; set; }
            public int Position { get; set; }
        }
        public static IEnumerable<FrameworkElement> FindAllVisualDescendants(this FrameworkElement parent)
        {
            if (parent == null)
                yield break;
            Stack<StackElement> stack = new Stack<StackElement>();
            int i = 0;
            while (true)
            {
                if (i < VisualTreeHelper.GetChildrenCount(parent))
                {
                    FrameworkElement child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                    if (child != null)
                    {
                        if (child != null)
                            yield return child;
                        stack.Push(new StackElement { Element = parent, Position = i });
                        parent = child;
                        i = 0;
                        continue;
                    }
                    ++i;
                }
                else
                {
                    // back at the root of the search
                    if (stack.Count == 0)
                        yield break;
                    StackElement element = stack.Pop();
                    parent = element.Element;
                    i = element.Position;
                    ++i;
                }
            }
        }
        public static ScrollViewer FindScroll(Visual visual)
        {
            if (visual is ScrollViewer)
                return visual as ScrollViewer;

            ScrollViewer searchChiled = null;
            DependencyObject chiled;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                chiled = VisualTreeHelper.GetChild(visual, i);
                if (chiled is Visual)
                    searchChiled = FindScroll(chiled as Visual);
                if (searchChiled != null)
                    return searchChiled;
            }

            return null;
        }

        public static T GetChildOfType<T>(this DependencyObject depObj)
    where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
