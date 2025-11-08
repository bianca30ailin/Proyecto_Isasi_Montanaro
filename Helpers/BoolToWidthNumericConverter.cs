using System;
using System.Globalization;
using System.Windows.Data;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public class BoolToWidthNumericConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool mostrar && mostrar)
            {
                return double.NaN; // Auto width (equivale a Width="Auto")
            }

            return 0.0; // Width = 0 oculta la columna
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}