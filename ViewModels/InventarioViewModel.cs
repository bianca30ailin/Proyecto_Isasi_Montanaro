using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
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
        private ObservableCollection<Producto> _productos;

        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set
            {
                _productos = value;
                OnPropertyChanged(nameof(Productos));
            }
        }

        public ICommand NuevoProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }

        public InventarioViewModel()
        {
            CargarProductos();

            NuevoProductoCommand = new RelayCommand(AbrirNuevoProducto);
            EliminarProductoCommand = new RelayCommand(EliminarProducto, PuedeEliminar);
            EditarProductoCommand = new RelayCommand(EditarProducto, PuedeEditar);
        }

        private void CargarProductos()
        {
            try
            {
                using (var db = new ProyectoTallerContext())
                {
                    var productos = db.Productos
                                      .Include(p => p.IdCategoriaNavigation) // para traer el nombre de la categoría
                                      .ToList();

                    Productos = new ObservableCollection<Producto>(productos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AbrirNuevoProducto(object obj)
        {
            var ventana = new Producto_form();
            ventana.ShowDialog();
            CargarProductos(); // recargamos la lista después de cerrar el formulario
        }

        private void EliminarProducto(object obj)
        {
            if (obj is Producto producto)
            {
                var result = MessageBox.Show($"¿Deseas eliminar el producto '{producto.Nombre}'?",
                                             "Confirmar eliminación",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new ProyectoTallerContext())
                        {
                            var eliminar = db.Productos.Find(producto.IdProducto);
                            if (eliminar != null)
                            {
                                db.Productos.Remove(eliminar);
                                db.SaveChanges();
                            }
                        }
                        CargarProductos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool PuedeEliminar(object obj) => obj is Producto;
        private void EditarProducto(object obj)
        {
            // para abrir el formulario con los datos del producto a editar
        }

        private bool PuedeEditar(object obj) => obj is Producto;

        // --- Implementación de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

