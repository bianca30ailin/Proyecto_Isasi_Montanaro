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
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Views;

namespace Proyecto_Isasi_Montanaro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar sesión
            Sesion.UsuarioActual = null;

            // Abrir nuevamente la ventana de login
            LoginWindow login = new LoginWindow();
            login.Show();

            // Cerrar la ventana principal
            this.Close();
        }
    }
}