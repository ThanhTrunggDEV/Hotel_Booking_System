using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Hotel_Booking_System.Behaviors
{
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
            if (d is ListBox listBox && (bool)e.NewValue)
            {
                ScrollViewer? scrollViewer = null;
                bool autoScroll = true;

                void EnsureScrollViewer()
                {
                    if (scrollViewer != null)
                        return;

                    scrollViewer = FindDescendant<ScrollViewer>(listBox);
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollChanged += (s, ev) =>
                        {
                            autoScroll = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1;
                        };
                    }
                }

                void ScrollToEnd()
                {
                    if (!autoScroll || listBox.Items.Count == 0)
                        return;

                    var lastItem = listBox.Items[listBox.Items.Count - 1];
                    listBox.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        EnsureScrollViewer();
                        listBox.UpdateLayout();
                        listBox.ScrollIntoView(lastItem);
                        scrollViewer?.ScrollToEnd();
                    }), DispatcherPriority.Render);
                }

                void AttachPropertyChanged(object item)
                {
                    if (item is INotifyPropertyChanged npc)
                    {
                        npc.PropertyChanged += (sender, args) => ScrollToEnd();
                    }
                }

                listBox.Loaded += (s, ev) =>
                {
                    EnsureScrollViewer();
                    foreach (var item in listBox.Items)
                    {
                        AttachPropertyChanged(item);
                    }
                    ScrollToEnd();
                };

                if (listBox.ItemsSource is INotifyCollectionChanged collection)
                {
                    collection.CollectionChanged += (s, ev) =>
                    {
                        if (ev.NewItems != null)
                        {
                            foreach (var item in ev.NewItems)
                            {
                                AttachPropertyChanged(item);
                            }
                        }
                        ScrollToEnd();
                    };
                }
            }
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
