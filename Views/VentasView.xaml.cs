using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proyecto_Isasi_Montanaro.Views
{
    /// <summary>
    /// Lógica de interacción para VentasView.xaml
    /// </summary>
    public partial class VentasView : UserControl
    {
        public VentasView()
        {
            InitializeComponent();
        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            // Instanciás la ventana de producto
            var ventana = new Venta_form();

            // Si querés que se abra como modal (bloquea hasta cerrar)
            ventana.ShowDialog();
        }
    }
}
