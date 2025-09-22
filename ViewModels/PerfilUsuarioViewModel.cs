using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    namespace Proyecto_Isasi_Montanaro.ViewModels
    {
        public class PerfilUsuarioViewModel : INotifyPropertyChanged
        {
            private readonly ProyectoTallerContext _context;

            public PerfilUsuarioViewModel()
            {
                _context = new ProyectoTallerContext();

                IsEditable = false;

                ModificarCommand = new RelayCommand(Modificar);
                GuardarCommand = new RelayCommand(Guardar, CanGuardar);
                CancelarCommand = new RelayCommand(Cancelar);

                CargarPerfiles();  // Primero cargamos los perfiles
                CargarUsuario();   // Luego marcamos los seleccionados
            }

            private Usuario _usuarioActual;
            public Usuario UsuarioActual
            {
                get => _usuarioActual;
                set { _usuarioActual = value; OnPropertyChanged(); }
            }

            public ObservableCollection<PerfilItem> PerfilesDisponibles { get; set; } = new();

            private bool _isEditable;
            public bool IsEditable
            {
                get => _isEditable;
                set { _isEditable = value; OnPropertyChanged(); }
            }

            public ICommand ModificarCommand { get; }
            public ICommand GuardarCommand { get; }
            public ICommand CancelarCommand { get; }

            private void CargarUsuario()
            {
                // Tomar usuario logueado
                UsuarioActual = Sesion.UsuarioActual;

                if (UsuarioActual != null)
                {
                    foreach (var perfil in PerfilesDisponibles)
                    {
                        perfil.IsSelected = UsuarioActual.IdTipoUsuarios
                            .Any(t => t.IdTipoUsuario == perfil.TipoUsuario.IdTipoUsuario);
                    }
                }
            }

            private void CargarPerfiles()
            {
                var perfiles = _context.TipoUsuarios.ToList();

                PerfilesDisponibles.Clear();
                foreach (var perfil in perfiles)
                {
                    PerfilesDisponibles.Add(new PerfilItem
                    {
                        TipoUsuario = perfil
                    });
                }
            }

            private void Modificar(object obj) => IsEditable = true;

            private bool CanGuardar(object obj) => IsEditable;

            private void Guardar(object obj)
            {
                if (UsuarioActual == null)
                {
                    MessageBox.Show("No se encontró un usuario válido.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                try
                {
                    // Traer al usuario desde el contexto con sus perfiles
                    var usuarioEnDb = _context.Usuarios
                                              .Include(u => u.IdTipoUsuarios)
                                              .FirstOrDefault(u => u.IdUsuario == UsuarioActual.IdUsuario);

                    if (usuarioEnDb != null)
                    {
                        // Actualizar datos básicos (opcional)
                        usuarioEnDb.Nombre = UsuarioActual.Nombre;
                        usuarioEnDb.Apellido = UsuarioActual.Apellido;
                        usuarioEnDb.Dni = UsuarioActual.Dni;
                        usuarioEnDb.Email = UsuarioActual.Email;
                        usuarioEnDb.Telefono = UsuarioActual.Telefono;
                        usuarioEnDb.Direccion = UsuarioActual.Direccion;
                        usuarioEnDb.Contraseña = UsuarioActual.Contraseña;
                        usuarioEnDb.Baja = UsuarioActual.Baja;
                        usuarioEnDb.FechaNacimiento = UsuarioActual.FechaNacimiento;

                        // Actualizar perfiles
                        usuarioEnDb.IdTipoUsuarios.Clear();
                        foreach (var perfil in PerfilesDisponibles.Where(p => p.IsSelected))
                        {
                            var tipoUsuario = _context.TipoUsuarios.Find(perfil.TipoUsuario.IdTipoUsuario);
                            if (tipoUsuario != null)
                                usuarioEnDb.IdTipoUsuarios.Add(tipoUsuario);
                        }

                        _context.SaveChanges();

                        MessageBox.Show("Los cambios se guardaron correctamente.",
                                        "Éxito",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo encontrar el usuario en la base de datos.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }

                    IsEditable = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al guardar: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }


            private void Cancelar(object obj)
            {
                _context.Entry(UsuarioActual).Reload();
                CargarPerfiles();
                CargarUsuario();
                IsEditable = false;
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
