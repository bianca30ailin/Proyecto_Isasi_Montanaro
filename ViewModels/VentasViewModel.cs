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
            EnvioVM = new EnvioViewModel(_context);
            TransporteVM = new TransporteViewModel(_context);

            // lista de estados de venta
            ListaEstadosVenta = new ObservableCollection<EstadoVenta>(_context.EstadoVenta.ToList());

            ClienteVM.PropertyChanged += (s, e) =>
            {
                // Cuando cambia el ClienteActual, pasamos sus direcciones al EnvioVM
                if (e.PropertyName == nameof(ClienteVM.ClienteActual))
                {
                    ClienteVM.CargarDirecciones();
                    EnvioVM.DireccionesCliente = ClienteVM.DireccionesCliente;
                    OnPropertyChanged(nameof(EnvioVM));
                }

                // Cuando cambia la lista de direcciones, la sincronizamos también
                if (e.PropertyName == nameof(ClienteVM.DireccionesCliente))
                {
                    EnvioVM.DireccionesCliente = ClienteVM.DireccionesCliente;
                    OnPropertyChanged(nameof(EnvioVM));
                }
            };
           
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

        public EnvioViewModel EnvioVM { get; set; }

        public TransporteViewModel TransporteVM { get; set; }

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
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1️⃣ Guardar cliente si no existe
                ClienteVM.GuardarClienteSiNoExiste();

                // 2️⃣ Validar stock actual en base de datos
                foreach (var detalle in DetalleVM.DetalleProductos)
                {
                    var productoDb = _context.Productos.FirstOrDefault(p => p.IdProducto == detalle.IdProducto);
                    if (productoDb == null)
                    {
                        MessageBox.Show($"Producto {detalle.IdProductoNavigation.Nombre} no encontrado.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    if (productoDb.Cantidad < detalle.Cantidad)
                    {
                        MessageBox.Show($"Stock insuficiente para {productoDb.Nombre}. Disponible: {productoDb.Cantidad}",
                                        "Error de stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                        transaction.Rollback();
                        return;
                    }

                    // Descontar stock real
                    productoDb.Cantidad -= detalle.Cantidad;
                    _context.Productos.Update(productoDb);
                }

                // 3️⃣ Crear y guardar la venta
                VentaActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
                VentaActual.FechaHora = DateOnly.FromDateTime(DateTime.Now);
                VentaActual.Total = DetalleVM.Total;
                VentaActual.IdUsuario = Sesion.UsuarioActual.IdUsuario;
                VentaActual.EstadoVentaId = 1; // 1 = "Activa"

                foreach (var d in DetalleVM.DetalleProductos)
                    VentaActual.DetalleVentaProductos.Add(d);

                _context.Venta.Add(VentaActual);
                _context.SaveChanges();

                // 4️⃣ Confirmar transacción
                transaction.Commit();

                // 5️⃣ Registrar envío si corresponde
                if (EnvioVM.EnvioHabilitado)
                    EnvioVM.RegistrarEnvio(VentaActual.IdNroVenta, TransporteVM.TransporteSeleccionado);

                // 6️⃣ Mostrar mensaje de éxito
                MessageBox.Show($"Venta registrada correctamente. N° {VentaActual.IdNroVenta}",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // 7️⃣ Refrescar vistas y limpiar formularios
                CargarVentas();
                DetalleVM.Reiniciar();
                ClienteVM.Reiniciar();
                EnvioVM.Reiniciar();
                VentaActual = new Ventum();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Error al confirmar la venta: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

   

        private void SincronizarDireccionesCliente()
        {
            if (ClienteVM.DireccionesCliente != null)
            {
                EnvioVM.DireccionesCliente = ClienteVM.DireccionesCliente;
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
