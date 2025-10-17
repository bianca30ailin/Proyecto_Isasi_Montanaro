using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public VentasViewModel()
        {
            _context = new ProyectoTallerContext();

            ClienteVM = new ClienteViewModel();
            DetalleVM = new DetalleVentaViewModel(_context);

            ListaEstadosVenta = new ObservableCollection<EstadoVenta>(_context.EstadoVenta.ToList()); //lista de estados de venta

            // --- Inicialización de envío ---
            EnvioHabilitado = false;

           
            // Calcular número próximo (solo para mostrar)
            int ultimoId = _context.Venta.Any() ? _context.Venta.Max(v => v.IdNroVenta) : 0;
            ProximoIdVenta = ultimoId + 1;

            // Inicializar venta
            VentaActual = new Ventum
            {
                FechaHora = DateOnly.FromDateTime(DateTime.Now),
                Total = 0,
                EstadoVentaId = 1
            };

            ConfirmarVentaCommand = new RelayCommand(_ => ConfirmarVenta(), _ => DetalleVM.DetalleProductos.Any());
            CargarVentas();
        }

        public ClienteViewModel ClienteVM { get; set; }
        public DetalleVentaViewModel DetalleVM { get; set; }

        public ObservableCollection<EstadoVenta> ListaEstadosVenta { get; set; }
        public Ventum VentaActual { get; set; }

        private ObservableCollection<Ventum> _ventas;
        public ObservableCollection<Ventum> Ventas
        {
            get => _ventas;
            set { _ventas = value; OnPropertyChanged(nameof(Ventas)); }
        }


        // --- COMANDOS ---
        public ICommand ConfirmarVentaCommand { get; set; }


        // --- METODOS ---
        private void ConfirmarVenta()
        {
            // Guardar cliente si no existe
            ClienteVM.GuardarClienteSiNoExiste();

            // Asociar cliente y detalles
            VentaActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
            VentaActual.FechaHora = DateOnly.FromDateTime(DateTime.Now);
            VentaActual.Total = DetalleVM.Total;

            foreach (var d in DetalleVM.DetalleProductos)
                VentaActual.DetalleVentaProductos.Add(d);

            VentaActual.IdUsuario = Sesion.UsuarioActual.IdUsuario;


            // Guardar venta
            _context.Venta.Add(VentaActual);
            _context.SaveChanges(); // primero guardamos la venta

            // Si el envío está habilitado, crear dirección + envío
            if (EnvioHabilitado)
            {
                // Validar que haya dirección cargada
                if (ClienteVM.DireccionActual != null)
                {
                    // Asociar la dirección al cliente
                    ClienteVM.DireccionActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
                    _context.Direccions.Add(ClienteVM.DireccionActual);
                    _context.SaveChanges();

                    // Crear registro de envío
                    var envio = new Envio
                    {
                        IdNroVenta = VentaActual.IdNroVenta,
                        Costo = 0, // Podés calcularlo después si tenés lógica
                        IdEstado = 1, // Ejemplo: "Pendiente"
                        FechaDespacho = null
                    };

                    VentaActual.IdUsuario = Sesion.UsuarioActual.IdUsuario;

                    VentaActual.EstadoVentaId = 1; // Activa

                    _context.Envios.Add(envio);
                    _context.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Debe ingresar una dirección para el envío.", "Aviso",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // No continúa si no hay dirección
                }
            }

            // Mostrar mensaje de éxito
            MessageBox.Show($"Venta registrada correctamente. N° {VentaActual.IdNroVenta}",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Recargar lista de ventas
            CargarVentas();

            // Reiniciar formularios
            DetalleVM.Reiniciar();
            ClienteVM.Reiniciar();
            VentaActual = new Ventum();
        }


        public void CargarVentas()
        {
            Ventas = new ObservableCollection<Ventum>(
                _context.Venta
                    .Include(v => v.DniClienteNavigation)
                    .Include(v => v.EstadoVenta)
                    .Include(v => v.IdUsuarioNavigation)
                    .Include(v => v.DetalleVentaProductos)
                    .ThenInclude(d => d.IdProductoNavigation)
                    .ToList()
            );
        }


        // --- Envío ---
        private bool _envioHabilitado;
        public bool EnvioHabilitado
        {
            get => _envioHabilitado;
            set
            {
                if (_envioHabilitado != value)
                {
                    _envioHabilitado = value;
                    OnPropertyChanged();
                }
            }
        }

        //calcular nro de venta
        private int _proximoIdVenta;
        public int ProximoIdVenta
        {
            get => _proximoIdVenta;
            set
            {
                _proximoIdVenta = value;
                OnPropertyChanged(nameof(ProximoIdVenta));
            }
        }

        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
}
