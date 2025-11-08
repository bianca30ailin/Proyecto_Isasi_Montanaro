using System;
using System.Globalization;
using System.Windows.Data;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public class EditOnlyIfNullConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Si no hay dos valores, no habilitar
            if (values.Length != 2)
                return false;

            // Primer valor → IsEditable
            bool isEditable = values[0] is bool edit && edit;

            // Segundo valor → Fecha (null o no)
            bool fechaExiste = values[1] != null;

            // Solo habilitar edición si está en modo edición y NO existe fecha
            return isEditable && !fechaExiste;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null; // No se usa
        }
    }
}
