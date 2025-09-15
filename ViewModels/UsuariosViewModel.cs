using System.ComponentModel;
using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proyecto_Isasi_Montanaro.Models;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class UsuariosViewModel : INotifyPropertyChanged
    {
        // Propiedad para la lista de usuarios que se mostrará en la vista
        public ObservableCollection<Usuario> Usuarios { get; set; }

        // Propiedad para el contexto de la base de datos (para simplificar)
        private readonly ProyectoTallerContext _context;

        public UsuariosViewModel()
        {
            _context = new ProyectoTallerContext(); 
            Usuarios = new ObservableCollection<Usuario>();
            CargarUsuarios();
        }

        // Método para cargar los datos de la base de datos
        private void CargarUsuarios()
        {
            try
            {
                // Limpiamos la colección por si se recarga
                Usuarios.Clear();

                // Consultamos la base de datos para obtener todos los usuarios
                // y sus colecciones relacionadas (Venta, IdTipoUsuarios)
                var usuariosDb = _context.Usuarios
                                      .Include(u => u.Venta) // Carga las ventas asociadas
                                      .Include(u => u.IdTipoUsuarios) // Carga los tipos de usuario asociados
                                      .ToList();

                // Agregamos cada usuario a la colección observable
                foreach (var usuario in usuariosDb)
                {
                    Usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                // Manejo básico de errores
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
                // Aquí podrías mostrar un mensaje al usuario si ocurre un error.
            }
        }

        // Implementación de INotifyPropertyChanged (si no la tienes ya)
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
