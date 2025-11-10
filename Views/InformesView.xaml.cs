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
using Proyecto_Isasi_Montanaro.ViewModels;

namespace Proyecto_Isasi_Montanaro.Views
{
    /// <summary>
    /// Lógica de interacción para InformesView.xaml
    /// </summary>
    public partial class InformesView : UserControl
    {
        public InformesView()
        {
            InitializeComponent();
            DataContext = new InformesMainViewModel();
        }

        private void NuevoInforme(object sender, RoutedEventArgs e)
        {
            var ventana = new Informe_form();
            ventana.ShowDialog();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
        }

    }
}
