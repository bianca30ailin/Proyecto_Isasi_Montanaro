using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public class ToLowerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Devuelve siempre en minúsculas
            return value?.ToString().ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Permite que si editás el valor en la tabla, lo guarde como lo escribiste
            return value?.ToString();
        }
    }
}