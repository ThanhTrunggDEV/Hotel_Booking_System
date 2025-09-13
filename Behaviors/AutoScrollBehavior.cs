using System.Collections.Specialized;
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
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                {
                    if (listBox.ItemsSource is INotifyCollectionChanged collection)
                    {
                        collection.CollectionChanged += (s, ev) =>
                        {
                            if (listBox.Items.Count > 0)
                            {
                                var lastItem = listBox.Items[listBox.Items.Count - 1];

                                listBox.Dispatcher.BeginInvoke(
                                    new System.Action(() =>
                                    {
                                        listBox.UpdateLayout(); 
                                        listBox.ScrollIntoView(lastItem);
                                    }),
                                    DispatcherPriority.Render 
                                );
                            }
                        };
                    }
                }
            }
        }
    }
}
