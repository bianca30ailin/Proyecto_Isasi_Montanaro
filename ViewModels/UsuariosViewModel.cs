using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;



namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class UsuariosViewModel : INotifyPropertyChanged
    {
        // Propiedad para la lista de usuarios que se mostrará en la vista
        public ObservableCollection<Usuario> Usuarios { get; set; }
        public ObservableCollection<string> Perfiles { get; set; }
        // Colección maestra para filtrar
        private List<Usuario> _usuariosTodos;

        // --- Comandos ---
        public ICommand AbrirFormularioUsuarioCommand { get; }
        public ICommand EditarUsuarioCommand { get; }
        public ICommand EliminarUsuarioCommand { get; }
        public ICommand FiltrarActivosCommand { get; }
        public ICommand FiltrarInactivosCommand { get; }
        public ICommand LimpiarFiltroCommand { get; }

// Propiedad para el contexto de la base de datos (para simplificar)
        private readonly ProyectoTallerContext _context;

        public UsuariosViewModel()
        {
            _context = new ProyectoTallerContext();
            Usuarios = new ObservableCollection<Usuario>();
            Perfiles = new ObservableCollection<string>();

            CargarUsuarios();
            CargarPerfiles();

            AbrirFormularioUsuarioCommand = new RelayCommand(AbrirFormularioUsuario);
            EditarUsuarioCommand = new RelayCommand(EditarUsuario);
            EliminarUsuarioCommand = new RelayCommand(EliminarUsuario);
            FiltrarActivosCommand = new RelayCommand(FiltrarActivos);
            FiltrarInactivosCommand = new RelayCommand(FiltrarInactivos);
            LimpiarFiltroCommand = new RelayCommand(LimpiarFiltro);
        }

        // Método para cargar los datos de la base de datos
        private void CargarUsuarios()
        {
            try
            {
                // Limpiamos la colección por si se recarga
                Usuarios.Clear();

                // Consultamos la base de datos para obtener todos los usuarios
                // y sus colecciones relacionadas (Venta, IdTipoUsuarios)
                var usuariosDb = _context.Usuarios
                                      .Include(u => u.Venta) // Carga las ventas asociadas
                                      .Include(u => u.IdTipoUsuarios) // Carga los tipos de usuario asociados
                                      .ToList();

                // Guardamos la colección completa para filtrar
                _usuariosTodos = usuariosDb;
                // Agregamos cada usuario a la colección observable
                foreach (var usuario in usuariosDb)
                {
                    Usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                // Manejo básico de errores
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
                
            }
        }

        private void AbrirFormularioUsuario(object parameter)
        {
            //carga el formulario con un nuevo viewmodel
            var formularioVM = new UsuarioFormViewModel();
            var formulario = new Usuario_form { DataContext = formularioVM };
            formulario.ShowDialog();

            // Recargar lista al cerrar formulario
            CargarUsuarios();
        }

        private void CargarPerfiles()
        {
            try
            {
                var perfilesDb = _context.TipoUsuarios
                    .Select(t => t.Tipo)
                    .Distinct()
                    .ToList();

                Perfiles.Clear();
                Perfiles.Add("Todos");

                foreach (var perfil in perfilesDb)
                    Perfiles.Add(perfil);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar perfiles: {ex.Message}");
            }
        }

        private void EditarUsuario(object parameter)
        {
            if (parameter is Usuario usuarioAEditar)
            {
                // Le paso el usuario existente al ViewModel del formulario
                var formularioVM = new UsuarioFormViewModel(usuarioAEditar);

                var formulario = new Usuario_form
                {
                    DataContext = formularioVM
                };

                formulario.ShowDialog();

                // Después de cerrar el formulario, recargo la lista
                CargarUsuarios();
            }
        }

        private void EliminarUsuario(object parameter)
        {
            if (parameter is Usuario usuarioAEliminar)
            {
                var result = MessageBox.Show(
                    $"¿Seguro que deseas dar de baja al usuario {usuarioAEliminar.Nombre} {usuarioAEliminar.Apellido}?",
                    "Confirmar baja",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var usuarioEnDb = _context.Usuarios
                                                  .FirstOrDefault(u => u.IdUsuario == usuarioAEliminar.IdUsuario);

                        if (usuarioEnDb != null)
                        {
                            usuarioEnDb.Baja = "sí"; // baja lógica
                            _context.SaveChanges();

                            MessageBox.Show("Usuario dado de baja con éxito.",
                                            "Éxito",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);

                            CargarUsuarios(); //refrescar lista
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al dar de baja usuario: {ex.Message}",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
        }

        //Filtros combinados
        private string _estadoFiltro = "Todos";
        private string _perfilSeleccionado;
        private string _textoBusqueda;

        public string PerfilSeleccionado
        {
            get => _perfilSeleccionado;
            set
            {
                _perfilSeleccionado = value;
                OnPropertyChanged(nameof(PerfilSeleccionado));
                AplicarFiltros();
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                AplicarFiltros();
            }
        }

        private void FiltrarActivos(object parameter)
        {
            _estadoFiltro = "Activos";
            AplicarFiltros();
        }

        private void FiltrarInactivos(object parameter)
        {
            _estadoFiltro = "Inactivos";
            AplicarFiltros();
        }

        private void LimpiarFiltro(object parameter)
        {
            _estadoFiltro = "Todos";
            PerfilSeleccionado = "Todos";
            TextoBusqueda = string.Empty;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_usuariosTodos == null) return;

            var filtrados = _usuariosTodos.AsEnumerable();

            // Filtro por estado
            if (_estadoFiltro == "Activos")
                filtrados = filtrados.Where(u => u.Baja.Equals("no", StringComparison.OrdinalIgnoreCase));
            else if (_estadoFiltro == "Inactivos")
                filtrados = filtrados.Where(u => u.Baja.Equals("sí", StringComparison.OrdinalIgnoreCase));

            // Filtro por perfil (muchos a muchos)
            if (!string.IsNullOrWhiteSpace(PerfilSeleccionado) && PerfilSeleccionado != "Todos")
            {
                filtrados = filtrados.Where(u =>
                    u.IdTipoUsuarios != null &&
                    u.IdTipoUsuarios.Any(t => t.Tipo.Equals(PerfilSeleccionado, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Filtro por texto (nombre, apellido, DNI)
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string busqueda = TextoBusqueda.ToLower();
                filtrados = filtrados.Where(u =>
                    (!string.IsNullOrEmpty(u.Nombre) && u.Nombre.ToLower().Contains(busqueda)) ||
                    (!string.IsNullOrEmpty(u.Apellido) && u.Apellido.ToLower().Contains(busqueda)) ||
                    u.Dni.ToString().Contains(busqueda)
                );
            }

            // Actualizamos la lista visible
            Usuarios.Clear();
            foreach (var usuario in filtrados)
                Usuarios.Add(usuario);
        }


        // Implementación de INotifyPropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
