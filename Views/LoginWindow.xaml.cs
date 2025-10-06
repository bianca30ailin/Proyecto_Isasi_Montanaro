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
using static System.Collections.Specialized.BitVector32;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Helpers;
using Microsoft.EntityFrameworkCore;


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

            // 🔹 Al abrir la ventana, cargar el último usuario recordado
            txtUsuario.Text = Settings.Default.UltimoUsuario;

            // Opcional: si había usuario guardado, marcar el checkbox
            chkRecordarUsuario.IsChecked = !string.IsNullOrEmpty(Settings.Default.UltimoUsuario);
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
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                btnLogin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void TxtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                txtUsuario.Focus();
            }
            
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                btnLogin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Por favor, complete todos los campos.",
                                "Campos requeridos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            string usuarioInput = txtUsuario.Text.Trim();
            string contraseña = txtPassword.Password.Trim();

            using (var context = new ProyectoTallerContext())
            {
                Usuario? usuario;

                if (usuarioInput.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    // 🔹 Caso especial: admin se valida con Nombre
                    usuario = context.Usuarios
                        .Include(u => u.IdTipoUsuarios)             // tabla intermedia
                        .FirstOrDefault(u => u.Nombre == "admin" &&
                                             u.Contraseña == contraseña &&
                                             u.Baja == "NO");
                }
                else
                {
                    // 🔹 Los demás se validan con DNI
                    if (int.TryParse(usuarioInput, out int dni))
                    {
                        usuario = context.Usuarios
                            .Include(u => u.IdTipoUsuarios) // Carga los perfiles
                            .FirstOrDefault(u => u.Dni == dni &&
                                                 u.Contraseña == contraseña &&
                                                 u.Baja == "NO");
                    }
                    else
                    {
                        MessageBox.Show("Debe ingresar un DNI válido.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return;
                    }
                }

                if (usuario != null)
                {
                    // Guardar el usuario si está marcado el checkbox
                    if (chkRecordarUsuario.IsChecked == true)
                    {
                        Settings.Default.UltimoUsuario = txtUsuario.Text.Trim();
                        Settings.Default.Save();
                    }
                    else
                    {
                        Settings.Default.UltimoUsuario = string.Empty;
                        Settings.Default.Save();
                    }
                    Sesion.UsuarioActual = usuario;

                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos.",
                                    "Error de inicio de sesión",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
    }
}
