using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public class ProductoVenta
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
    }
    public partial class Venta_form : Window
    {
        public ObservableCollection<ProductoVenta> Productos { get; set; }

        public Venta_form()
        {
            InitializeComponent();

            Productos = new ObservableCollection<ProductoVenta>();
            ListaProductos.ItemsSource = Productos;
        }

        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            Productos.Add(new ProductoVenta { NombreProducto = "", Cantidad = 1 });
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
