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
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Views;


namespace Proyecto_Isasi_Montanaro.Views
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class PerfilUsuarioView : UserControl
    {
        public PerfilUsuarioView()
        {
            InitializeComponent();
        }

        private void CargarDatosUsuario()
        {
            if (Sesion.UsuarioActual != null)
            {
                txtNombre.Text = Sesion.UsuarioActual.Nombre;
                txtApellido.Text = Sesion.UsuarioActual.Apellido;
                txtDni.Text = Sesion.UsuarioActual.Dni.ToString();
                txtCorreo.Text = Sesion.UsuarioActual.Email;    
            }
        }
    }
}
