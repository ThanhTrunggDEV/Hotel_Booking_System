using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
                void ScrollToEnd()
                {
                    if (listBox.Items.Count == 0)
                        return;

                    var lastItem = listBox.Items[listBox.Items.Count - 1];
                    listBox.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        listBox.UpdateLayout();
                        listBox.ScrollIntoView(lastItem);
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
    }
}
