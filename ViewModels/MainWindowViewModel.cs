
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        // Constructor
        public MainWindowViewModel()
        {
            // Inicializar vista por defecto
            _vistaActual = new Views.UserControl1();

            // Configurar permisos si hay usuario en sesión
            if (Sesion.UsuarioActual != null)
            {
                ConfigurarPermisosPorTipo(Sesion.UsuarioActual);
            }
        }
        //se declara un campo privado al que se le asigna el usercontrol1
        private object _vistaActual = new Views.UserControl1();
        public object VistaActual
        {
            //devolver el valor que tenga el campo privado
            get => _vistaActual;
            set
            {//antes de asignar compara si el valor es distinto al actual
                if (_vistaActual != value)
                {//llama a la propiedad para avisar que la vista cambio
                    _vistaActual = value;
                    OnPropertyChanged(nameof(VistaActual));
                }
            }
        }
        // ========== PROPIEDADES DE ACTIVACIÓN DE VISTAS ==========
        //cada boton tiene una propiedad booleana que indica si esta activo o no
        private bool _isUsuariosActive;
        public bool IsUsuariosActive
        {
            get => _isUsuariosActive;
            set
            {
                if (_isUsuariosActive != value)
                {
                    _isUsuariosActive = value;
                    if (value) ActivarVista("Usuarios");
                    OnPropertyChanged(nameof(IsUsuariosActive));
                }
            }
        }

        private bool _isBackUpActive;
        public bool IsBackUpActive
        {
            get => _isBackUpActive;
            set
            {
                if (_isBackUpActive != value)
                {
                    _isBackUpActive = value;
                    if (value) ActivarVista("BackUp");
                    OnPropertyChanged(nameof(IsBackUpActive));
                }
            }
        }

        private bool _isInventarioActive;
        public bool IsInventarioActive
        {
            get => _isInventarioActive;
            set
            {
                if (_isInventarioActive != value)
                {
                    _isInventarioActive = value;
                    if (value) ActivarVista("Inventario");
                    OnPropertyChanged(nameof(IsInventarioActive));
                }
            }
        }

        private bool _isClientesActive;
        public bool IsClientesActive
        {
            get => _isClientesActive;
            set
            {
                if (_isClientesActive != value)
                {
                    _isClientesActive = value;
                    if (value) ActivarVista("Clientes");
                    OnPropertyChanged(nameof(IsClientesActive));
                }
            }
        }

        private bool _isVentasActive;
        public bool IsVentasActive
        {
            get => _isVentasActive;
            set
            {
                if (_isVentasActive != value)
                {
                    _isVentasActive = value;
                    if (value) ActivarVista("Ventas");
                    OnPropertyChanged(nameof(IsVentasActive));
                }
            }
        }

        private bool _isEnviosActive;
        public bool IsEnviosActive
        {
            get => _isEnviosActive;
            set
            {
                if (_isEnviosActive != value)
                {
                    _isEnviosActive = value;
                    if (value) ActivarVista("Envios");
                    OnPropertyChanged(nameof(IsEnviosActive));
                }
            }
        }

        private bool _isInformesActive;
        public bool IsInformesActive
        {
            get => _isInformesActive;
            set
            {
                if (_isInformesActive != value)
                {
                    _isInformesActive = value;
                    if (value) ActivarVista("Informes");
                    OnPropertyChanged(nameof(IsInformesActive));
                }
            }
        }

        private bool _isPerfilActive;
        public bool IsPerfilActive
        {
            get => _isPerfilActive;
            set
            {
                if (_isPerfilActive != value)
                {
                    _isPerfilActive = value;
                    if (value) ActivarVista("Perfil");
                    OnPropertyChanged(nameof(IsPerfilActive));
                }
            }
        }

        // ========== PERMISOS DE ACCESO A SECCIONES (VER) ==========
        private bool _puedeUsuarios;
        public bool PuedeUsuarios
        {
            get => _puedeUsuarios;
            set { _puedeUsuarios = value; OnPropertyChanged(nameof(PuedeUsuarios)); }
        }

        private bool _puedeInventario;
        public bool PuedeInventario
        {
            get => _puedeInventario;
            set { _puedeInventario = value; OnPropertyChanged(nameof(PuedeInventario)); }
        }

        private bool _puedeClientes;
        public bool PuedeClientes
        {
            get => _puedeClientes;
            set { _puedeClientes = value; OnPropertyChanged(nameof(PuedeClientes)); }
        }

        private bool _puedeVentas;
        public bool PuedeVentas
        {
            get => _puedeVentas;
            set { _puedeVentas = value; OnPropertyChanged(nameof(PuedeVentas)); }
        }

        private bool _puedeEnvios;
        public bool PuedeEnvios
        {
            get => _puedeEnvios;
            set { _puedeEnvios = value; OnPropertyChanged(nameof(PuedeEnvios)); }
        }

        private bool _puedeInformes;
        public bool PuedeInformes
        {
            get => _puedeInformes;
            set { _puedeInformes = value; OnPropertyChanged(nameof(PuedeInformes)); }
        }

        private bool _puedeBackUp;
        public bool PuedeBackUp
        {
            get => _puedeBackUp;
            set { _puedeBackUp = value; OnPropertyChanged(nameof(PuedeBackUp)); }
        }


        
        // Método que configura los permisos según los tipos de usuario del usuario actual
        private void ConfigurarPermisosPorTipo(Usuario usuario)
        {

            var tiposIds = usuario.IdTipoUsuarios.Select(t => t.IdTipoUsuario).ToList();

            // Administrador (ID = 1)
            if (tiposIds.Contains(1))
            {
                // Puede VER todas las secciones?
                PuedeUsuarios = true;
                PuedeInventario = true;
                PuedeClientes = true;
                PuedeVentas = true;
                PuedeEnvios = true;
                PuedeInformes = false;
                PuedeBackUp = true;

                
            }

            // Ventas (ID = 2)
            if (tiposIds.Contains(2))
            {
                PuedeClientes = true;
                PuedeVentas = true;
            }

            // Inventario (ID = 4)
            if (tiposIds.Contains(4))
            {
                PuedeInventario = true;
            }

            // Logística / Envíos (ID = 3)
            if (tiposIds.Contains(3))
            {
                PuedeEnvios = true;
            }

            //Supervisor / (ID = 5)
            if (tiposIds.Contains(5))
            {
                PuedeUsuarios = true;
                PuedeInventario = true;
                PuedeClientes = true;
                PuedeVentas = true;
                PuedeEnvios = true;
                PuedeInformes = true;
                PuedeBackUp = true;

                
            }
        }

        private void ActivarVista(string vista)
        {
            //Resetear todos los botones
            _isUsuariosActive = false;
            _isBackUpActive = false;
            _isInventarioActive = false;
            _isClientesActive = false;
            _isVentasActive = false;
            _isEnviosActive = false;
            _isInformesActive = false;
            _isPerfilActive = false;

            //Activar solo el seleccionado
            switch (vista)
            {
                case "Usuarios":
                    _isUsuariosActive = true;
                    VistaActual = new Views.UsuariosView();
                    break;
                case "BackUp":
                    _isBackUpActive = true;
                    VistaActual = new Views.BackUpView();
                    break;
                case "Inventario":
                    _isInventarioActive = true;
                    VistaActual = new Views.InventarioView();
                    break;
                case "Clientes":
                    _isClientesActive = true;
                    VistaActual = new Views.ClientesView();
                    break;
                case "Ventas":
                    _isVentasActive = true;
                    VistaActual = new Views.VentasView();
                    break;
                case "Envios":
                    _isEnviosActive = true;
                    VistaActual = new Views.EnviosView();
                    break;
                case "Informes":
                    _isInformesActive = true;
                    VistaActual = new Views.InformesView();
                    break;
                case "Perfil":
                    _isPerfilActive = true;
                    VistaActual = new Views.PerfilUsuarioView();
                    break;
            }

            // Notificar cambios para que los botones se refresquen
            OnPropertyChanged(nameof(IsUsuariosActive));
            OnPropertyChanged(nameof(IsBackUpActive));
            OnPropertyChanged(nameof(IsInventarioActive));
            OnPropertyChanged(nameof(IsClientesActive));
            OnPropertyChanged(nameof(IsVentasActive));
            OnPropertyChanged(nameof(IsEnviosActive));
            OnPropertyChanged(nameof(IsInformesActive));
            OnPropertyChanged(nameof(IsPerfilActive));
            OnPropertyChanged(nameof(VistaActual));
        }
        //nombre del usuario actual
        public string NombreUsuario => Sesion.UsuarioActual?.Nombre ?? "Usuario";

        

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string nombre) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
