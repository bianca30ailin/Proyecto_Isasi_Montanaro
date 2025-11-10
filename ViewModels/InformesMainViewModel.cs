using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Views.Informes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        // --- Propiedades del dashboard ---
        private int _ventasDelDia;
        public int VentasDelDia
        {
            get => _ventasDelDia;
            set { _ventasDelDia = value; OnPropertyChanged(); }
        }

        private int _enviosDelDia;
        public int EnviosDelDia
        {
            get => _enviosDelDia;
            set { _enviosDelDia = value; OnPropertyChanged(); }
        }

        private int _devolucionesDelDia;
        public int DevolucionesDelDia
        {
            get => _devolucionesDelDia;
            set { _devolucionesDelDia = value; OnPropertyChanged(); }
        }

        private double _totalVendidoDia;
        public double TotalVendidoDia
        {
            get => _totalVendidoDia;
            set { _totalVendidoDia = value; OnPropertyChanged(); }
        }

        private ObservableCollection<object> _registrosDelDia;
        public ObservableCollection<object> RegistrosDelDia
        {
            get => _registrosDelDia;
            set { _registrosDelDia = value; OnPropertyChanged(); }
        }


        // --- Comandos ---
        public ICommand IrAInformeVentasCommand { get; }
        public ICommand IrAInformeEnviosCommand { get; }
        public ICommand IrAInformeInventarioCommand { get; }
        public ICommand IrAInformeClientesCommand { get; }
        public ICommand IrAInformeUsuariosCommand { get; }

        // --- Comandos de dashboard ---
        public ICommand MostrarVentasCommand { get; }
        public ICommand MostrarEnviosCommand { get; }
        public ICommand MostrarDevolucionesCommand { get; }

        // --- Constructor ---
        public InformesMainViewModel()
        {
            
            // Muestra inicialmente el dashboard principal
            ContenidoActual = null;
            // Comandos de navegación
            IrAInformeVentasCommand = new RelayCommand(_ => AbrirVista(() => new InformeVentasView(VolverAlDashboard)));
            IrAInformeEnviosCommand = new RelayCommand(_ => AbrirVista(() => new InformeEnviosView(VolverAlDashboard)));
            IrAInformeInventarioCommand = new RelayCommand(_ => AbrirVista(() => new InformeInventarioView(VolverAlDashboard)));
            IrAInformeClientesCommand = new RelayCommand(_ => AbrirVista(() => new InformeClientesView(VolverAlDashboard)));
            IrAInformeUsuariosCommand = new RelayCommand(_ => AbrirVista(() => new InformeUsuariosView(VolverAlDashboard)));

            // Comandos de resumen
            MostrarVentasCommand = new RelayCommand(_ => CargarVentasDelDia());
            MostrarEnviosCommand = new RelayCommand(_ => CargarEnviosDelDia());
            MostrarDevolucionesCommand = new RelayCommand(_ => CargarDevolucionesDelDia());

            // Cargar datos iniciales
            CargarResumenDelDia();
        }

        // --- Métodos de navegación ---
        private void AbrirVista(Func<object> crearVista)
        {
            ContenidoActual = crearVista.Invoke();
        }

        private void VolverAlDashboard()
        {
            ContenidoActual = null; // 🔹 vuelve al panel principal
        }

        // --- Métodos del dashboard ---
        private void CargarResumenDelDia()
        {
            using var context = new ProyectoTallerContext();
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            VentasDelDia = context.Venta.Count(v => v.FechaHora == hoy);
            EnviosDelDia = context.Envios.Count(e => e.FechaDespacho == hoy);
            DevolucionesDelDia = context.Venta.Count(v => v.EstadoVenta.NombreEstado == "Cancelada" && v.FechaHora == hoy);
            TotalVendidoDia = context.Venta
                .Where(v => v.FechaHora == hoy)
                .Sum(v => (double?)v.Total) ?? 0;
        }

        private void CargarVentasDelDia()
        {
            using var context = new ProyectoTallerContext();
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var ventas = context.Venta
                .Where(v => v.FechaHora == hoy)
                .Select(v => new
                {
                    v.IdNroVenta,
                    Cliente = v.DniClienteNavigation.Nombre + " " + v.DniClienteNavigation.Apellido,
                    v.Total,
                    v.FechaHora
                })
                .ToList();

            RegistrosDelDia = new ObservableCollection<object>(ventas);
        }

        private void CargarEnviosDelDia()
        {
            using var context = new ProyectoTallerContext();
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var envios = context.Envios
                .Where(e => e.FechaDespacho == hoy)
                .Select(e => new
                {
                    e.IdEnvio,
                    e.NumSeguimiento,
                    e.Costo,
                    Estado = e.IdEstadoNavigation.Nombre,
                    e.FechaDespacho
                })
                .ToList();

            RegistrosDelDia = new ObservableCollection<object>(envios);
        }

        private void CargarDevolucionesDelDia()
        {
            using var context = new ProyectoTallerContext();
            var hoy = DateTime.Today;

            var devoluciones = context.NotaCredito
                .Where(nc => nc.Fecha.Date == hoy)
                .Select(nc => new
                {
                    nc.IdNotaCredito,
                    Venta = nc.Venta.IdNroVenta,
                    Cliente = nc.Venta.DniClienteNavigation.Nombre + " " + nc.Venta.DniClienteNavigation.Apellido,
                    nc.Motivo,
                    Total = nc.Total,
                    Fecha = nc.Fecha
                })
                .ToList();

            RegistrosDelDia = new ObservableCollection<object>(devoluciones);
        }



        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    }

