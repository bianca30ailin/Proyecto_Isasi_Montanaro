using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class ProductoFormViewModel : INotifyPropertyChanged
    {
        private Categorium _categoriaSeleccionada;
        private readonly ProyectoTallerContext _context;
        private bool _esEdicion = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Propiedad del producto
        private Producto _nuevoProducto;
        public Producto NuevoProducto
        {// Devuelve el valor del campo
            get => _nuevoProducto;
            set
            {
                _nuevoProducto = value;
                OnPropertyChanged(nameof(NuevoProducto));
            }
        }

        // Categoría seleccionada
        public Categorium CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                if (_categoriaSeleccionada != value)
                {
                    _categoriaSeleccionada = value;
                    OnPropertyChanged();
                    if (NuevoProducto != null)
                        NuevoProducto.IdCategoria = value?.IdCategoria ?? 0;
                }
            }
        }

        // Lista de categorías para el ComboBox
        private ObservableCollection<Categorium> _categorias;
        public ObservableCollection<Categorium> Categorias
        {
            get => _categorias;
            set
            {
                _categorias = value;
                OnPropertyChanged();
            }
        }

        // Comandos
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        //CONSTRUCTOR ALTA
        public ProductoFormViewModel()
        {
            _context = new ProyectoTallerContext();
            NuevoProducto = new Producto
            {
                Baja = "NO" // valor por defecto
            };
            _esEdicion = false;
            GuardarCommand = new RelayCommand(GuardarProducto);
            CancelarCommand = new RelayCommand(Cancelar);
            CargarCategorias();
        }

        // CONSTRUCTOR EDICIÓN
        public ProductoFormViewModel(Producto productoExistente)
        {
            _context = new ProyectoTallerContext();

            // Clonamos los datos para no modificar el original directamente
            NuevoProducto = new Producto
            {
                IdProducto = productoExistente.IdProducto,
                Nombre = productoExistente.Nombre,
                Descripcion = productoExistente.Descripcion,
                Precio = productoExistente.Precio,
                Cantidad = productoExistente.Cantidad,
                IdCategoria = productoExistente.IdCategoria,
                Baja = productoExistente.Baja
            };

            _esEdicion = true;
            GuardarCommand = new RelayCommand(GuardarProducto);
            CancelarCommand = new RelayCommand(Cancelar);
            CargarCategorias();

            // Selecciona automáticamente la categoría correspondiente
            CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.IdCategoria == productoExistente.IdCategoria);
        }


        private void CargarCategorias()
        {
            using (var db = new ProyectoTallerContext())
            {
                Categorias = new ObservableCollection<Categorium>(db.Categoria.ToList());
            }
        }

        // <summary>
        /// Valida todos los campos del producto antes de guardar
        /// </summary>
        private bool ValidarProducto()
        {
            // Validación: Nombre obligatorio
            if (string.IsNullOrWhiteSpace(NuevoProducto.Nombre))
            {
                MessageBox.Show("El nombre del producto es obligatorio.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Nombre con longitud mínima
            if (NuevoProducto.Nombre.Trim().Length < 3)
            {
                MessageBox.Show("El nombre del producto debe tener al menos 3 caracteres.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Nombre con longitud máxima 
            if (NuevoProducto.Nombre.Length > 100)
            {
                MessageBox.Show("El nombre del producto no puede exceder los 100 caracteres.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Descripción con longitud máxima
            if (!string.IsNullOrEmpty(NuevoProducto.Descripcion) && NuevoProducto.Descripcion.Length > 500)
            {
                MessageBox.Show("La descripción no puede exceder los 500 caracteres.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Categoría seleccionada
            if (CategoriaSeleccionada == null || CategoriaSeleccionada.IdCategoria == 0)
            {
                MessageBox.Show("Debe seleccionar una categoría válida.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Precio no negativo y mayor a cero
            if (NuevoProducto.Precio <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a cero.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Cantidad (stock) no negativa
            if (NuevoProducto.Cantidad < 0)
            {
                MessageBox.Show("La cantidad en stock no puede ser negativa.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Stock mínimo no negativo
            if (NuevoProducto.StockMinimo < 0)
            {
                MessageBox.Show("El stock mínimo no puede ser negativo.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validación: Stock mínimo debe ser menor o igual al stock actual (opcional, comentar si no aplica)
            if (NuevoProducto.StockMinimo > NuevoProducto.Cantidad && !_esEdicion)
            {
                var result = MessageBox.Show(
                    "El stock mínimo es mayor al stock actual. ¿Desea continuar de todas formas?",
                    "Advertencia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return false;
            }

            // Validación: Nombre duplicado (solo para nuevo producto)
            if (!_esEdicion)
            {
                var nombreExiste = _context.Productos
                    .Any(p => p.Nombre.ToLower() == NuevoProducto.Nombre.Trim().ToLower());

                if (nombreExiste)
                {
                    MessageBox.Show("Ya existe un producto con ese nombre.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            else
            {
                // Para edición, verificar que no exista otro producto con el mismo nombre
                var nombreExiste = _context.Productos
                    .Any(p => p.Nombre.ToLower() == NuevoProducto.Nombre.Trim().ToLower()
                           && p.IdProducto != NuevoProducto.IdProducto);

                if (nombreExiste)
                {
                    MessageBox.Show("Ya existe otro producto con ese nombre.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void GuardarProducto(object parameter)
        {
            try
            {
                // Ejecutar todas las validaciones
                if (!ValidarProducto())
                    return;

                // Limpiar espacios del nombre
                NuevoProducto.Nombre = NuevoProducto.Nombre.Trim();

                // Asignar categoría
                NuevoProducto.IdCategoria = CategoriaSeleccionada.IdCategoria;

                if (NuevoProducto.IdProducto == 0)
                {
                    // --- NUEVO PRODUCTO ---
                    NuevoProducto.Baja = "NO";

                    _context.Productos.Add(NuevoProducto);
                    MessageBox.Show("Producto creado con éxito.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // --- EDITAR PRODUCTO ---
                    var productoExistente = _context.Productos
                        .FirstOrDefault(p => p.IdProducto == NuevoProducto.IdProducto);

                    if (productoExistente != null)
                    {
                        productoExistente.Nombre = NuevoProducto.Nombre;
                        productoExistente.Descripcion = NuevoProducto.Descripcion;
                        productoExistente.Precio = NuevoProducto.Precio;
                        productoExistente.Cantidad = NuevoProducto.Cantidad;
                        productoExistente.StockMinimo = NuevoProducto.StockMinimo;
                        productoExistente.IdCategoria = NuevoProducto.IdCategoria;

                        _context.Productos.Update(productoExistente);
                    }

                    MessageBox.Show("Producto actualizado con éxito.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _context.SaveChanges();
                CerrarVentana(parameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el producto:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Cancelar(object parameter) => CerrarVentana(parameter);
        private void CerrarVentana(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }

    }
}

