using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hotel_Booking_System.Behaviors
{
    /// <summary>
    /// Provides an attached property that scrolls a <see cref="ListBox"/>
    /// to the most recent item when it is first loaded. No further automatic
    /// scrolling is performed so users can navigate messages manually.
    /// </summary>
    public static class ListBoxExtensions
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",
                typeof(bool),
                typeof(ListBoxExtensions),
                new PropertyMetadata(false, OnAutoScrollChanged));

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox || (bool)e.NewValue == false)
                return;

            listBox.Loaded += (_, _) =>
            {
                var scrollViewer = FindDescendant<ScrollViewer>(listBox);
                scrollViewer?.ScrollToEnd();
            };
        }

        private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            if (root is T target)
                return target;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                var result = FindDescendant<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

