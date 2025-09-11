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
        private object _vistaActual = new Views.UserControl1();
        public object VistaActual
        {
            get => _vistaActual;
            set
            {
                if (_vistaActual != value)
                {
                    _vistaActual = value;
                    OnPropertyChanged(nameof(VistaActual));
                }
            }
        }
       
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

        private void ActivarVista(string vista)
        {
            // Resetear todos los botones
            _isUsuariosActive = false;
            _isBackUpActive = false;
            _isInventarioActive = false;
            _isClientesActive = false;
            _isVentasActive = false;
            _isEnviosActive = false;
            _isInformesActive = false;

            // Activamos solo el seleccionado
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
            }

            // Notificar cambios para que los botones se refresquen
            OnPropertyChanged(nameof(IsUsuariosActive));
            OnPropertyChanged(nameof(IsBackUpActive));
            OnPropertyChanged(nameof(IsInventarioActive));
            OnPropertyChanged(nameof(IsClientesActive));
            OnPropertyChanged(nameof(IsVentasActive));
            OnPropertyChanged(nameof(IsEnviosActive));
            OnPropertyChanged(nameof(IsInformesActive));
            OnPropertyChanged(nameof(VistaActual));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string nombre) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
