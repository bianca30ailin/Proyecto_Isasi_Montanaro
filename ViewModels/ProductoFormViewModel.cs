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
            GuardarCommand = new RelayCommand(GuardarProducto);
            CancelarCommand = new RelayCommand(Cancelar);
            CargarCategorias();
        }

        private void CargarCategorias()
        {
            using (var db = new ProyectoTallerContext())
            {
                Categorias = new ObservableCollection<Categorium>(db.Categoria.ToList());
            }
        }

        private void GuardarProducto(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NuevoProducto.Nombre))
                {
                    MessageBox.Show("Debe ingresar un nombre para el producto.", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CategoriaSeleccionada == null)
                {
                    MessageBox.Show("Debe seleccionar una categoría antes de guardar.", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Productos.Add(NuevoProducto);
                _context.SaveChanges();

                MessageBox.Show("Producto guardado con éxito.");

                // Limpiar los campos sin cerrar la ventana
                NuevoProducto = new Producto { Baja = "NO" };
                CategoriaSeleccionada = null;
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

