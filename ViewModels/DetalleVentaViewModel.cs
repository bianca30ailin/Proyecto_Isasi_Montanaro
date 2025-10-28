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
using System.Windows;
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
            ProductosFiltrados = new ObservableCollection<Producto>(ProductosDisponibles);
            DetalleProductos = new ObservableCollection<DetalleVentaProducto>();

            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto(), _ => ProductoSeleccionado != null && CantidadSeleccionada > 0);
            EditarProductoCommand = new RelayCommand(p => EditarProducto(p as DetalleVentaProducto));
            EliminarProductoCommand = new RelayCommand(p => EliminarProducto(p as DetalleVentaProducto));

        }

        public ObservableCollection<Producto> ProductosDisponibles { get; set; }
        public ObservableCollection<DetalleVentaProducto> DetalleProductos { get; set; }

        private ObservableCollection<Producto> _productosFiltrados;

       
        //seleccion de producto
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


        //busqueda y filtro de producto
        public ObservableCollection<Producto> ProductosFiltrados
        {
            get => _productosFiltrados;
            set { _productosFiltrados = value; OnPropertyChanged(); }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                FiltrarProductos();
            }
        }

        // --- COMANDOS ---
        public ICommand AgregarProductoCommand { get; set; }
        public ICommand EditarProductoCommand { get; set; }
        public ICommand EliminarProductoCommand { get; set; }


        // --- METODOS ---
        private void AgregarProducto()
        {
            if (ProductoSeleccionado == null || CantidadSeleccionada <= 0)
                return;

            // Verificar si el producto ya está en el detalle → sumamos cantidades
            var existente = DetalleProductos.FirstOrDefault(d => d.IdProducto == ProductoSeleccionado.IdProducto);
            if (existente != null)
            {
                // Validar stock total (por si intenta sumar más de lo disponible)
                if (ProductoSeleccionado.Cantidad < (existente.Cantidad + CantidadSeleccionada))
                {
                    MessageBox.Show($"Stock insuficiente para {ProductoSeleccionado.Nombre}. Disponible: {ProductoSeleccionado.Cantidad}",
                                    "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                existente.Cantidad += CantidadSeleccionada;
                existente.Subtotal = existente.Cantidad * ProductoSeleccionado.Precio;
            }
            else
            {
                // Validar stock disponible en memoria (sin modificar BD)
                if (ProductoSeleccionado.Cantidad < CantidadSeleccionada)
                {
                    MessageBox.Show($"Stock insuficiente para {ProductoSeleccionado.Nombre}. Disponible: {ProductoSeleccionado.Cantidad}",
                                    "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Crear nuevo detalle
                var subtotal = ProductoSeleccionado.Precio * CantidadSeleccionada;
                var detalle = new DetalleVentaProducto
                {
                    IdProducto = ProductoSeleccionado.IdProducto,
                    Cantidad = CantidadSeleccionada,
                    Subtotal = subtotal,
                    IdProductoNavigation = ProductoSeleccionado
                };

                DetalleProductos.Add(detalle);
            }

            

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

            // Lo quitamos del detalle para luego volver a agregarlo con la cantidad corregida
            DetalleProductos.Remove(detalle);
            CalcularTotal();
        }

        private void EliminarProducto(DetalleVentaProducto detalle)
        {
            if (detalle == null) return;

            var result = MessageBox.Show($"¿Deseas quitar {detalle.IdProductoNavigation.Nombre} del detalle?",
                                         "Confirmar eliminación",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DetalleProductos.Remove(detalle);
                CalcularTotal();
            }
        }

        public void Reiniciar()
        {
            DetalleProductos.Clear();
            ProductoSeleccionado = null;
            CantidadSeleccionada = 0;
            Total = 0;
            OnPropertyChanged(nameof(DetalleProductos));
        }


        //Filtrado de productos
        private void FiltrarProductos()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                ProductosFiltrados = new ObservableCollection<Producto>(ProductosDisponibles);
            }
            else
            {
                var filtrados = ProductosDisponibles
                    .Where(p => p.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                ProductosFiltrados = new ObservableCollection<Producto>(filtrados);
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
