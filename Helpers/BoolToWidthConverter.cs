using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool mostrar && mostrar)
            {
                return new DataGridLength(1, DataGridLengthUnitType.Auto);
            }

            return new DataGridLength(0); // Width = 0 oculta la columna
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}