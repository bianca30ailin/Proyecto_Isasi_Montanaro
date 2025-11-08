using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Proyecto_Isasi_Montanaro.Views.Formularios
{
    /// <summary>
    /// Lógica de interacción para Transporte_form.xaml
    /// </summary>
    public partial class Transporte_form : Window
    {
        public Transporte_form()
        {
            InitializeComponent();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
