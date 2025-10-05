using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
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

            //constructor
            public VentasViewModel()
            {
                _context = new ProyectoTallerContext();

                // --- Inicialización de envío ---
                EnvioHabilitado = false;

                // --- Inicialización de venta ---
                ProductosDisponibles = new ObservableCollection<Producto>(_context.Productos.ToList());
                DetalleProductos = new ObservableCollection<DetalleVentaProducto>();

                // Calcular próximo número de venta
                int ultimoId = _context.Venta.Any()
                    ? _context.Venta.Max(v => v.IdNroVenta)
                    : 0;
                int proximoId = ultimoId + 1;

                VentaActual = new Ventum
                {
                    IdNroVenta = proximoId,
                    FechaHora = DateOnly.FromDateTime(DateTime.Now),
                    Total = 0
                };

                AgregarProductoCommand = new RelayCommand(_ => AgregarProducto(), _ => ProductoSeleccionado != null && CantidadSeleccionada > 0);
                ConfirmarVentaCommand = new RelayCommand(_ => ConfirmarVenta(), _ => DetalleProductos.Any());
                EditarProductoCommand = new RelayCommand(p => EditarProducto(p as DetalleVentaProducto));
                EliminarProductoCommand = new RelayCommand(p => EliminarProducto(p as DetalleVentaProducto));
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

            // --- Venta y productos ---
            public Ventum VentaActual { get; set; }
            public ObservableCollection<Producto> ProductosDisponibles { get; set; }
            public ObservableCollection<DetalleVentaProducto> DetalleProductos { get; set; }

            private Producto _productoSeleccionado;
            public Producto ProductoSeleccionado
            {
                get => _productoSeleccionado;
                set { _productoSeleccionado = value; OnPropertyChanged(); }
            }

            private int _cantidadSeleccionada;
            public int CantidadSeleccionada
            {
                get => _cantidadSeleccionada;
                set { _cantidadSeleccionada = value; OnPropertyChanged(); }
            }

            private double _total;
            public double Total
            {
                get => _total;
                set { _total = value; OnPropertyChanged(); }
            }

            // --- Commands ---
            public ICommand AgregarProductoCommand { get; set; }
            public ICommand ConfirmarVentaCommand { get; set; }

            public ICommand EditarProductoCommand { get; set; }
            public ICommand EliminarProductoCommand { get; set; }

            // --- Métodos ---
            private void AgregarProducto()
                {
                    if (ProductoSeleccionado == null || CantidadSeleccionada <= 0) return;

                    var subtotal = ProductoSeleccionado.Precio * CantidadSeleccionada;

                    var detalle = new DetalleVentaProducto
                    {
                        IdProducto = ProductoSeleccionado.IdProducto,
                        Cantidad = CantidadSeleccionada,
                        Subtotal = subtotal,
                        IdProductoNavigation = ProductoSeleccionado
                    };

                    DetalleProductos.Add(detalle);
                    CalcularTotal();
                    CantidadSeleccionada = 0;
                }

            private void CalcularTotal()
            {
                Total = DetalleProductos.Sum(d => d.Subtotal);
                VentaActual.Total = Total;
            }

            private void ConfirmarVenta()
            {
                foreach (var detalle in DetalleProductos)
                {
                    VentaActual.DetalleVentaProductos.Add(detalle);

                    var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == detalle.IdProducto);
                    if (producto != null)
                    {
                        producto.Cantidad -= detalle.Cantidad;
                        _context.Productos.Update(producto);
                    }
                }

                _context.Venta.Add(VentaActual);
                _context.SaveChanges();

                DetalleProductos.Clear();
                VentaActual = new Ventum
                {
                    FechaHora = DateOnly.FromDateTime(DateTime.Now),
                    Total = 0
                };
                Total = 0;
            }
            private void EditarProducto(DetalleVentaProducto detalle)
            {
                if (detalle == null) return;

                // Cargar los datos del detalle seleccionado en los campos de edición
                ProductoSeleccionado = detalle.IdProductoNavigation;
                CantidadSeleccionada = detalle.Cantidad;

                // Eliminarlo temporalmente de la lista para volver a agregarlo editado
                DetalleProductos.Remove(detalle);
                CalcularTotal();
            }

            private void EliminarProducto(DetalleVentaProducto detalle)
            {
                if (detalle == null) return;

                DetalleProductos.Remove(detalle);
                CalcularTotal();
            }

        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
}
