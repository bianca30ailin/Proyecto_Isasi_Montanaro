using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
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
    public class DetalleVentaViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public DetalleVentaViewModel(ProyectoTallerContext context)
        {
            _context = context;
            ProductosDisponibles = new ObservableCollection<Producto>(_context.Productos.ToList());
            DetalleProductos = new ObservableCollection<DetalleVentaProducto>();

            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto(), _ => ProductoSeleccionado != null && CantidadSeleccionada > 0);
            EditarProductoCommand = new RelayCommand(p => EditarProducto(p as DetalleVentaProducto));
            EliminarProductoCommand = new RelayCommand(p => EliminarProducto(p as DetalleVentaProducto));
        }

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


        // --- COMANDOS ---
        public ICommand AgregarProductoCommand { get; set; }
        public ICommand EditarProductoCommand { get; set; }
        public ICommand EliminarProductoCommand { get; set; }


        // --- METODOS ---
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
        }

        private void EditarProducto(DetalleVentaProducto detalle)
        {
            if (detalle == null) return;
            ProductoSeleccionado = detalle.IdProductoNavigation;
            CantidadSeleccionada = detalle.Cantidad;
            DetalleProductos.Remove(detalle);
            CalcularTotal();
        }

        private void EliminarProducto(DetalleVentaProducto detalle)
        {
            if (detalle == null) return;
            DetalleProductos.Remove(detalle);
            CalcularTotal();
        }

        public void Reiniciar()
        {
            DetalleProductos.Clear();
            ProductoSeleccionado = null;
            CantidadSeleccionada = 0;
            Total = 0;
            OnPropertyChanged(nameof(DetalleProductos));
        }


        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
