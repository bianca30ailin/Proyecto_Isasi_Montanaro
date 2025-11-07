using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Views.Informes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class InformesMainViewModel : INotifyPropertyChanged
    {
        // --- Propiedades ---
        private object _contenidoActual;
        public object ContenidoActual
        {
            get => _contenidoActual;
            set { _contenidoActual = value; OnPropertyChanged(); }
        }

        // --- Comandos ---
        public ICommand IrAInformeVentasCommand { get; }
        public ICommand IrAInformeEnviosCommand { get; }
        public ICommand IrAInformeInventarioCommand { get; }
        public ICommand IrAInformeClientesCommand { get; }
        public ICommand IrAInformeUsuariosCommand { get; }

        // --- Constructor ---
        public InformesMainViewModel()
        {
            // Muestra inicialmente el dashboard principal
            ContenidoActual = null;

            IrAInformeVentasCommand = new RelayCommand(_ => AbrirVista(() => new InformeVentasView(VolverAlDashboard)));
            IrAInformeEnviosCommand = new RelayCommand(_ => AbrirVista(() => new InformeEnviosView(VolverAlDashboard)));
            IrAInformeInventarioCommand = new RelayCommand(_ => AbrirVista(() => new InformeInventarioView(VolverAlDashboard)));
            IrAInformeClientesCommand = new RelayCommand(_ => AbrirVista(() => new InformeClientesView(VolverAlDashboard)));
            IrAInformeUsuariosCommand = new RelayCommand(_ => AbrirVista(() => new InformeUsuariosView(VolverAlDashboard)));
        }

        // --- Métodos ---
        private void AbrirVista(Func<object> crearVista)
        {
            ContenidoActual = crearVista.Invoke();
        }

        private void VolverAlDashboard()
        {
            ContenidoActual = null; // 🔹 vuelve al panel principal
        }

        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

