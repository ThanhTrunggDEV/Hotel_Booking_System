using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hotel_Booking_System.Behaviors
{
    /// <summary>
    /// Provides an attached property that keeps a <see cref="ListBox"/>
    /// scrolled to the bottom while the user is viewing the latest messages.
    /// Scrolling is suppressed once the user scrolls away from the bottom so
    /// manual navigation feels natural.
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
                if (scrollViewer == null)
                    return;

                bool autoScroll = true;

                scrollViewer.ScrollChanged += (_, args) =>
                {
                    if (args.ExtentHeightChange == 0)
                    {
                        // user-initiated scroll: track if we are at the bottom
                        autoScroll = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
                    }
                    else if (autoScroll)
                    {
                        // content changed while we were at the bottom
                        scrollViewer.ScrollToEnd();
                    }
                };
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

