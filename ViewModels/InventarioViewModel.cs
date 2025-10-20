using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class InventarioViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public ObservableCollection<Producto> Productos { get; set; }
        private List<Producto> _productosTodos;

        public ObservableCollection<Categorium> Categorias { get; set; }

        // --- Comandos ---
        public ICommand AbrirFormularioProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand FiltrarActivosCommand { get; }
        public ICommand FiltrarInactivosCommand { get; }
        public ICommand LimpiarFiltroCommand { get; }

        // --- Permisos ---
        public bool PuedeCrearProducto => Sesion.UsuarioActual?.IdTipoUsuarios.Any(t => t.IdTipoUsuario == 4) ?? false;
        public bool PuedeEditarProducto => Sesion.UsuarioActual?.IdTipoUsuarios.Any(t => t.IdTipoUsuario == 4) ?? false;
        public bool PuedeEliminarProducto => Sesion.UsuarioActual?.IdTipoUsuarios.Any(t => t.IdTipoUsuario == 4) ?? false;


        // --- Filtros ---
        private string _estadoFiltro = "Todos";
        private Categorium _categoriaSeleccionada;
        private string _textoBusqueda;

        public Categorium CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                _categoriaSeleccionada = value;
                OnPropertyChanged(nameof(CategoriaSeleccionada));
                AplicarFiltros();
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                AplicarFiltros();
            }
        }

        // --- Constructor ---
        public InventarioViewModel()
        {
            _context = new ProyectoTallerContext();
            Productos = new ObservableCollection<Producto>();
            Categorias = new ObservableCollection<Categorium>();

            CargarProductos();
            CargarCategorias();

            AbrirFormularioProductoCommand = new RelayCommand(AbrirFormularioProducto);
            EditarProductoCommand = new RelayCommand(EditarProducto);
            EliminarProductoCommand = new RelayCommand(EliminarProducto);
            FiltrarActivosCommand = new RelayCommand(FiltrarActivos);
            FiltrarInactivosCommand = new RelayCommand(FiltrarInactivos);
            LimpiarFiltroCommand = new RelayCommand(LimpiarFiltro);
        }

        // --- Validación de permisos ---
        private bool CanCrearProducto(object parameter) => PuedeCrearProducto;
        private bool CanEditarProducto(object parameter) => PuedeEditarProducto;
        private bool CanEliminarProducto(object parameter) => PuedeEliminarProducto;


        // --- Carga inicial ---
        private void CargarProductos()
        {
            try
            {
                Productos.Clear();

                var productosDb = _context.Productos
                    .Include(p => p.IdCategoriaNavigation)
                    .ToList();

                _productosTodos = productosDb;

                foreach (var p in productosDb)
                    Productos.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}");
            }
        }

        private void CargarCategorias()
        {
            try
            {
                Categorias.Clear();
                Categorias.Add(new Categorium { IdCategoria = 0, Nombre = "Todas" });

                var categoriasDb = _context.Categoria.ToList();
                foreach (var c in categoriasDb)
                    Categorias.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}");
            }
        }

        // --- Abrir formulario ---
        private void AbrirFormularioProducto(object parameter)
        {
            var formularioVM = new ProductoFormViewModel();
            var formulario = new Producto_form { DataContext = formularioVM };
            formulario.ShowDialog();

            CargarProductos();
        }

        // --- Editar ---
        private void EditarProducto(object parameter)
        {
            if (parameter is Producto producto)
            {
                var formularioVM = new ProductoFormViewModel(producto);
                var formulario = new Producto_form { DataContext = formularioVM };
                formulario.ShowDialog();
                CargarProductos();
            }
        }

        // --- Eliminar (baja lógica) ---
        private void EliminarProducto(object parameter)
        {
            if (parameter is Producto producto)
            {
                var result = MessageBox.Show(
                    $"¿Deseas dar de baja el producto {producto.Nombre}?",
                    "Confirmar baja",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var prodDb = _context.Productos.FirstOrDefault(p => p.IdProducto == producto.IdProducto);
                        if (prodDb != null)
                        {
                            prodDb.Baja = "SI";
                            _context.SaveChanges();
                            MessageBox.Show("Producto dado de baja con éxito.");
                            CargarProductos();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al dar de baja producto: {ex.Message}");
                    }
                }
            }
        }

        // --- Filtros ---
        private void FiltrarActivos(object parameter)
        {
            _estadoFiltro = "Activos";
            AplicarFiltros();
        }

        private void FiltrarInactivos(object parameter)
        {
            _estadoFiltro = "Inactivos";
            AplicarFiltros();
        }

        private void LimpiarFiltro(object parameter)
        {
            _estadoFiltro = "Todos";
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Nombre == "Todas");
            TextoBusqueda = string.Empty;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (_productosTodos == null) return;

            var filtrados = _productosTodos.AsEnumerable();

            // Estado (baja)
            if (_estadoFiltro == "Activos")
                filtrados = filtrados.Where(p => p.Baja.Equals("NO", StringComparison.OrdinalIgnoreCase));
            else if (_estadoFiltro == "Inactivos")
                filtrados = filtrados.Where(p => p.Baja.Equals("SI", StringComparison.OrdinalIgnoreCase));

            // Categoría
            if (CategoriaSeleccionada != null && CategoriaSeleccionada.IdCategoria != 0)
                filtrados = filtrados.Where(p => p.IdCategoria == CategoriaSeleccionada.IdCategoria);

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string busqueda = TextoBusqueda.ToLower();
                filtrados = filtrados.Where(p =>
                    (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.ToLower().Contains(busqueda)) ||
                    (!string.IsNullOrEmpty(p.Descripcion) && p.Descripcion.ToLower().Contains(busqueda))
                );
            }

            // Actualizar lista visible
            Productos.Clear();
            foreach (var prod in filtrados)
                Productos.Add(prod);
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

