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
using Proyecto_Isasi_Montanaro.ViewModels.Informes;
using Proyecto_Isasi_Montanaro.ViewModels;

namespace Proyecto_Isasi_Montanaro.Views.Informes
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class InformeVentasView : UserControl
    {
        public InformeVentasView()
        {
            InitializeComponent();
        }

        public InformeVentasView(Action volverAccion)
        {
            InitializeComponent();
            DataContext = new InformeVentasViewModel(volverAccion);
        }
    }
}
