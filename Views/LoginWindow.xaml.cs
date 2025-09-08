using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Proyecto_Isasi_Montanaro.Views
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtUsuario.Focus(); //inicia en el input usuario
        }

        private void TxtUsuario_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex: letras y números solamente
            e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-Z0-9]+$");
        }

        // Manejar teclas especiales: flecha arriba/abajo y bloquear espacio
        private void TxtUsuario_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Bloquea la barra espaciadora
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            // Si se presiona la flecha abajo, pasa el foco al PasswordBox
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                txtPassword.Focus();
            }
            /*else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                btnLogin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }*/
        }

        private void TxtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                txtUsuario.Focus();
            }
            /*
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                btnLogin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }*/
        }
    }
}
