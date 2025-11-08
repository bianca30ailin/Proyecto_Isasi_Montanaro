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
        public ICommand FiltrarPorFechasCommand { get; }

        // --- Permisos ---
        private bool _puedeCrearProducto;
        private bool _puedeEditarProducto;
        private bool _puedeEliminarProducto;
        private bool _mostrarColumnAcciones;

        public bool PuedeCrearProducto
        {
            get => _puedeCrearProducto;
            set
            {
                _puedeCrearProducto = value;
                OnPropertyChanged(nameof(PuedeCrearProducto));
            }
        }

        public bool PuedeEditarProducto
        {
            get => _puedeEditarProducto;
            set
            {
                _puedeEditarProducto = value;
                OnPropertyChanged(nameof(PuedeEditarProducto));
                OnPropertyChanged(nameof(MostrarColumnAcciones)); // Actualiza también esta
            }
        }

        public bool PuedeEliminarProducto
        {
            get => _puedeEliminarProducto;
            set
            {
                _puedeEliminarProducto = value;
                OnPropertyChanged(nameof(PuedeEliminarProducto));
                OnPropertyChanged(nameof(MostrarColumnAcciones)); 
            }
        }

        public bool MostrarColumnAcciones => PuedeEditarProducto || PuedeEliminarProducto;



        // --- Filtros ---
        private string _estadoFiltro = "Todos";
        private Categorium _categoriaSeleccionada;
        private string _textoBusqueda;
        private DateTime? _fechaDesde;  
        private DateTime? _fechaHasta;  


        public Categorium CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                _categoriaSeleccionada = value;
                OnPropertyChanged(nameof(CategoriaSeleccionada));

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

        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set
            {
                _fechaDesde = value;
                OnPropertyChanged(nameof(FechaDesde));
            }
        }

        public DateTime? FechaHasta
        {
            get => _fechaHasta;
            set
            {
                _fechaHasta = value;
                OnPropertyChanged(nameof(FechaHasta));
            }
        }

        // --- Constructor ---
        public InventarioViewModel()
        {
            _context = new ProyectoTallerContext();
            Productos = new ObservableCollection<Producto>();
            Categorias = new ObservableCollection<Categorium>();

            InicializarPermisos();
            CargarProductos();
            CargarCategorias();

            // Inicializar comandos
            AbrirFormularioProductoCommand = new RelayCommand(AbrirFormularioProducto);
            EditarProductoCommand = new RelayCommand(EditarProducto);
            EliminarProductoCommand = new RelayCommand(EliminarProducto);
            FiltrarActivosCommand = new RelayCommand(FiltrarActivos);
            FiltrarInactivosCommand = new RelayCommand(FiltrarInactivos);
            LimpiarFiltroCommand = new RelayCommand(LimpiarFiltro);
            FiltrarPorFechasCommand = new RelayCommand(FiltrarPorFechas);
        }

        private void InicializarPermisos()
        {
            bool esInventario = Sesion.UsuarioActual?.IdTipoUsuarios.Any(t => t.IdTipoUsuario == 4) ?? false;

            PuedeCrearProducto = esInventario;
            PuedeEditarProducto = esInventario;
            PuedeEliminarProducto = esInventario;

        }


        // --- Carga inicial ---
        private void CargarProductos()
        {
            try
            {
                Productos.Clear();

                var productosDb = _context.Productos
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.UsuarioCreacion)
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
                Categorias.Add(new Categorium { IdCategoria = 0, Nombre = "Todos" });

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

        private void FiltrarPorFechas(object parameter)
        {
            AplicarFiltros();
        }

        private void LimpiarFiltro(object parameter)
        {
            _estadoFiltro = "Todos";
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Nombre == "Todos");
            TextoBusqueda = string.Empty;
            FechaDesde = null;   
            FechaHasta = null;   
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

            // Filtro por fecha desde
            if (FechaDesde.HasValue)
            {
                filtrados = filtrados.Where(p => p.FechaCreacion.Date >= FechaDesde.Value.Date);
            }

            //
            // Filtro por fecha hasta
            if (FechaHasta.HasValue)
            {
                filtrados = filtrados.Where(p => p.FechaCreacion.Date <= FechaHasta.Value.Date);
            }

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