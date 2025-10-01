using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class UsuarioFormViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        // ======== PROPIEDADES ======== //
        private Usuario _nuevoUsuario;
        public Usuario NuevoUsuario
        {//devuelve el valor del campo
            get => _nuevoUsuario;
            set
            {
                _nuevoUsuario = value;
                OnPropertyChanged(nameof(NuevoUsuario));
            }
        }

        //propiedad para habilitar edicion
        private bool _isEditable;
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged(nameof(IsEditable));

                }
            }
        }

        //propiedad para habilitar boton o ocultarlo
        private bool _esNuevo;
        public bool EsNuevo
        {
            get => _esNuevo;
            set
            {
                if (_esNuevo != value)
                {
                    _esNuevo = value;
                    OnPropertyChanged(nameof(EsNuevo));
                }
            }
        }


        /*Colección observable de perfiles disponibles para el usuario. Cada elemento es un PerfilItem 
         que envuelve un TipoUsuario de la base y agrega la propiedad IsSelected para saber si está tildado en la UI.*/
        private ObservableCollection<PerfilItem> _perfilesDisponibles;
        public ObservableCollection<PerfilItem> PerfilesDisponibles
        {
            get => _perfilesDisponibles;
            set
            {
                _perfilesDisponibles = value;
                OnPropertyChanged(nameof(PerfilesDisponibles));
            }
        }

        // ======== COMANDOS ======== //
        public ICommand GuardarUsuarioCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand ModificarCommand { get; }

       
        // CONSTRUCTOR ALTA //
        public UsuarioFormViewModel()
        {
            _context = new ProyectoTallerContext();

            NuevoUsuario = new Usuario
            {
                Baja = "no" // valor por defecto
            };

            IsEditable = true; //permito editar campos
            EsNuevo = true;//marco que es nuevo usuario

            GuardarUsuarioCommand = new RelayCommand(GuardarUsuario, CanGuardar);
            CancelarCommand = new RelayCommand(Cancelar);
            ModificarCommand = new RelayCommand(Modificar);

            CargarPerfiles();
        }

        // CONSTRUCTOR MODIFICACION //
        public UsuarioFormViewModel(Usuario usuarioExistente)
        {
            _context = new ProyectoTallerContext();

            // Asignás el usuario que recibís
            NuevoUsuario = usuarioExistente;

            IsEditable = false; // modo edición arranca bloqueado
            EsNuevo = false; // marco que no es nuevo usuario

            GuardarUsuarioCommand = new RelayCommand(GuardarUsuario, CanGuardar);
            CancelarCommand = new RelayCommand(Cancelar);
            ModificarCommand = new RelayCommand(Modificar);

            CargarPerfiles();

            // Marcar como seleccionados los perfiles que ya tiene el usuario
            foreach (var perfil in PerfilesDisponibles)
            {
                if (usuarioExistente.IdTipoUsuarios.Any(t => t.IdTipoUsuario == perfil.TipoUsuario.IdTipoUsuario))
                {
                    perfil.IsSelected = true;
                }
            }
        }

        // ======== MÉTODOS ======== //

        private void Modificar(object obj) => IsEditable = true;
        private bool CanGuardar(object obj) => IsEditable;

        private void CargarPerfiles()
        {
            try
            {
                // Trae los perfiles desde la base de datos (tabla TipoUsuarios)
                var tipos = _context.TipoUsuarios.AsNoTracking().ToList();
                // Crea un PerfilItem por cada TipoUsuario, inicializando IsSelected en false
                PerfilesDisponibles = new ObservableCollection<PerfilItem>(
                    tipos.Select(t => new PerfilItem
                    {
                        TipoUsuario = t,
                        IsSelected = false
                    })
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando perfiles: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuardarUsuario(object parameter)
        {
            if (NuevoUsuario == null)
            {
                MessageBox.Show("El usuario no es válido.", "Advertencia",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Validación mínima
                if (string.IsNullOrWhiteSpace(NuevoUsuario.Nombre))
                {
                    MessageBox.Show("El nombre es obligatorio.",
                                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NuevoUsuario.IdUsuario == 0)
                {
                    // === ALTA ===
                    foreach (var perfil in PerfilesDisponibles.Where(p => p.IsSelected))
                    {// Busca el TipoUsuario en la BD y lo agrega a la colección del usuario
                        var tipoUsuario = _context.TipoUsuarios.Find(perfil.TipoUsuario.IdTipoUsuario);
                        if (tipoUsuario != null)
                            NuevoUsuario.IdTipoUsuarios.Add(tipoUsuario);
                    }

                    _context.Usuarios.Add(NuevoUsuario);
                }
                else
                {
                    // === EDICIÓN ===
                    var usuarioEnDb = _context.Usuarios
                                              .Include(u => u.IdTipoUsuarios)
                                              .FirstOrDefault(u => u.IdUsuario == NuevoUsuario.IdUsuario);

                    if (usuarioEnDb != null)
                    {
                        // Actualizar campos básicos
                        usuarioEnDb.Nombre = NuevoUsuario.Nombre;
                        usuarioEnDb.Apellido = NuevoUsuario.Apellido;
                        usuarioEnDb.Dni = NuevoUsuario.Dni;
                        usuarioEnDb.Email = NuevoUsuario.Email;
                        usuarioEnDb.Telefono = NuevoUsuario.Telefono;
                        usuarioEnDb.Direccion = NuevoUsuario.Direccion;
                        usuarioEnDb.Contraseña = NuevoUsuario.Contraseña;
                        usuarioEnDb.Baja = NuevoUsuario.Baja;
                        usuarioEnDb.FechaNacimiento = NuevoUsuario.FechaNacimiento;

                        // Limpia los perfiles actuales y agrega los seleccionados
                        usuarioEnDb.IdTipoUsuarios.Clear();
                        foreach (var perfil in PerfilesDisponibles.Where(p => p.IsSelected))
                        {
                            var tipoUsuario = _context.TipoUsuarios.Find(perfil.TipoUsuario.IdTipoUsuario);
                            if (tipoUsuario != null)
                                usuarioEnDb.IdTipoUsuarios.Add(tipoUsuario);
                        }
                    }
                }

                // Guardar en la base
                _context.SaveChanges();

                MessageBox.Show(
                    "Usuario guardado con éxito.",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                CerrarVentana(parameter);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? string.Empty;
                MessageBox.Show(
                    $"Error al guardar el usuario:\n{ex.Message}\n{inner}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void Cancelar(object parameter) => CerrarVentana(parameter);

        private void CerrarVentana(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }


        // ======== INotifyPropertyChanged ======== //
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
