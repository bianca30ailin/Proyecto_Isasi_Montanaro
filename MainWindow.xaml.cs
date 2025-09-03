using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proyecto_Isasi_Montanaro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using Proyecto_Isasi_Montanaro.Views;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UsuariosView();
        }

        private void BtnInformes_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}