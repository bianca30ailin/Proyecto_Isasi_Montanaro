using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

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

            CargarUsuario();
            CargarPerfiles();
        }

        private Usuario _usuarioActual;
        public Usuario UsuarioActual
        {
            get => _usuarioActual;
            set { _usuarioActual = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PerfilItem> PerfilesDisponibles { get; set; } = new ObservableCollection<PerfilItem>();

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
            // Traer el usuario en sesión
            UsuarioActual = Sesion.UsuarioEnSesion;

            if (UsuarioActual != null && PerfilesDisponibles.Any())
            {
                foreach (var perfil in PerfilesDisponibles)
                {
                    perfil.IsSelected = UsuarioActual.IdTipoUsuarios.Any(t => t.IdTipoUsuario == perfil.TipoUsuario.IdTipoUsuario);
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
                    TipoUsuario = perfil,
                    IsSelected = UsuarioActual?.IdTipoUsuarios.Any(t => t.IdTipoUsuario == perfil.IdTipoUsuario) ?? false
                });
            }
        }

        private void Modificar(object obj)
        {
            IsEditable = true;
        }

        private bool CanGuardar(object obj)
        {
            return IsEditable;
        }

        private void Guardar(object obj)
        {
            if (UsuarioActual == null) return;

            UsuarioActual.IdTipoUsuarios = PerfilesDisponibles
                .Where(p => p.IsSelected)
                .Select(p => p.TipoUsuario)
                .ToList();

            _context.Usuarios.Update(UsuarioActual);
            _context.SaveChanges();

            IsEditable = false;
        }

        private void Cancelar(object obj)
        {
            _context.Entry(UsuarioActual).Reload();
            CargarPerfiles();
            IsEditable = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PerfilItem : INotifyPropertyChanged
    {
        public TipoUsuario TipoUsuario { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
