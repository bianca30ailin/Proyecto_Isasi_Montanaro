using System.ComponentModel;
using System.Windows.Input;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Commands;
using System.Windows;
using System.Net;
using System.Net.Mail;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class UsuarioFormViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        // Propiedad que enlaza a los campos del formulario
        public Usuario NuevoUsuario { get; set; }

        // Comandos para los botones
        public ICommand GuardarUsuarioCommand { get; }
        public ICommand CancelarCommand { get; }

        public UsuarioFormViewModel()
        {
            _context = new ProyectoTallerContext();
            NuevoUsuario = new Usuario();

            GuardarUsuarioCommand = new RelayCommand(GuardarUsuario);

            CancelarCommand = new RelayCommand(Cancelar);
        }

        private void GuardarUsuario(object parameter)
        {
            if (NuevoUsuario != null)
            {
                try
                {
                    // Generamos una contraseña temporal aleatoria
                    string contrasenaTemporal = GenerarContrasenaAleatoria();

                    // Se asigna la contraseña temporal al nuevo usuario
                    NuevoUsuario.Contraseña = contrasenaTemporal;

                    // Enviamos la contraseña por correo electrónico
                    // antes de guardar en la DB para que el usuario reciba la clave.
                    EnviarContrasenaPorEmail(NuevoUsuario.Email, contrasenaTemporal);

                    // Agregar el nuevo usuario al contexto de la base de datos
                    _context.Usuarios.Add(NuevoUsuario);

                    // Guardar los cambios en la base de datos 
                    _context.SaveChanges();

                    MessageBox.Show("Usuario guardado con éxito. Se ha enviado la contraseña temporal por email.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar la ventana del formulario
                    if (parameter is Window window)
                    {
                        window.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Manejar y mostrar cualquier error que ocurra durante el proceso
                    MessageBox.Show($"Ocurrió un error al guardar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancelar(object parameter)
        {
            // Lógica para cerrar el formulario sin guardar
            CerrarVentana(parameter);
        }

        // Método auxiliar para cerrar la ventana desde el ViewModel
        private void CerrarVentana(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        private void EnviarContrasenaPorEmail(string emailDestino, string contrasena)
        {
            try
            {
                // 1. Configurar el cliente SMTP
                var smtpClient = new SmtpClient("smtp.gmail.com") // Reemplaza con tu servidor
                {
                    Port = 587,
                    Credentials = new NetworkCredential("pequenostoques.clo@gmail.com", "Vitale2025"), // Reemplaza con tus credenciales
                    EnableSsl = true, // Usar SSL para una conexión segura
                };

                // 2. Crear el mensaje de correo
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("pequenostoques.clo@gmail.com", "SISIE"),
                    Subject = "Contraseña de acceso a tu cuenta",
                    Body = $"Hola,\n\nTu contraseña temporal para acceder a tu cuenta es: {contrasena}\n\nPor favor, cámbiala en tu primer inicio de sesión.",
                    IsBodyHtml = false, 
                };
                mailMessage.To.Add(emailDestino);

                // 3. Enviar el correo
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Manejar el error si el envío falla
                MessageBox.Show($"Ocurrió un error al enviar el correo: {ex.Message}", "Error de correo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarContrasenaAleatoria()
        {
            // Lógica para generar una contraseña segura.
            // Esto es solo un ejemplo, puedes usar librerías más robustas.
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var contrasena = new char[8];
            var random = new Random();

            for (int i = 0; i < 8; i++)
            {
                contrasena[i] = caracteres[random.Next(caracteres.Length)];
            }
            return new string(contrasena);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}