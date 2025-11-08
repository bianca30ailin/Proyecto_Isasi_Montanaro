using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public class DataGridColumnVisibilityBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty ColumnHeaderProperty =
            DependencyProperty.Register(
                nameof(ColumnHeader),
                typeof(string),
                typeof(DataGridColumnVisibilityBehavior),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                nameof(IsVisible),
                typeof(bool),
                typeof(DataGridColumnVisibilityBehavior),
                new PropertyMetadata(true, OnIsVisibleChanged));

        public string ColumnHeader
        {
            get => (string)GetValue(ColumnHeaderProperty);
            set => SetValue(ColumnHeaderProperty, value);
        }

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnDataGridLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnDataGridLoaded;
        }

        private void OnDataGridLoaded(object sender, RoutedEventArgs e)
        {
            UpdateColumnVisibility();
        }

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumnVisibilityBehavior behavior)
            {
                behavior.UpdateColumnVisibility();
            }
        }

        private void UpdateColumnVisibility()
        {
            if (AssociatedObject == null) return;

            foreach (var column in AssociatedObject.Columns)
            {
                if (column.Header?.ToString() == ColumnHeader)
                {
                    column.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                }
            }
        }
    }
}