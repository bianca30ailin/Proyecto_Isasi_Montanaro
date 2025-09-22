using Proyecto_Isasi_Montanaro.Commands;
using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        {
            get => _nuevoUsuario;
            set
            {
                _nuevoUsuario = value;
                OnPropertyChanged(nameof(NuevoUsuario));
            }
        }

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

        // ======== CONSTRUCTOR ======== //
        public UsuarioFormViewModel()
        {
            _context = new ProyectoTallerContext();

            NuevoUsuario = new Usuario
            {
                Baja = "no" // valor por defecto
            };

            GuardarUsuarioCommand = new RelayCommand(GuardarUsuario);
            CancelarCommand = new RelayCommand(Cancelar);

            CargarPerfiles();
        }

        // ======== MÉTODOS ======== //
        private void CargarPerfiles()
        {
            try
            {
                // Trae los perfiles desde la base
                var tipos = _context.TipoUsuarios.AsNoTracking().ToList();

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
                // Validación mínima: por ejemplo nombre o email
                if (string.IsNullOrWhiteSpace(NuevoUsuario.Nombre))
                {
                    MessageBox.Show("El nombre es obligatorio.",
                                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Agregar usuario
                _context.Usuarios.Add(NuevoUsuario);

                // Agregar relaciones muchos a muchos
                foreach (var perfil in PerfilesDisponibles.Where(p => p.IsSelected))
                {
                    var tipoUsuario = _context.TipoUsuarios.Find(perfil.TipoUsuario.IdTipoUsuario);
                    NuevoUsuario.IdTipoUsuarios.Add(tipoUsuario);
                }

                // Guardar en la base
                _context.SaveChanges();

                MessageBox.Show(
                    "Usuario guardado con éxito..",
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Cerrar ventana
                CerrarVentana(parameter);
            }
            catch (Exception ex)
            {
                // Mostrar mensaje más detallado si hay InnerException
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
