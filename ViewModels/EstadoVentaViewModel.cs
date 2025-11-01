using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class EstadoVentaViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public ObservableCollection<EstadoVenta> ListaEstadosVenta { get; set; }

        private EstadoVenta _estadoSeleccionado;
        public EstadoVenta EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set
            {
                _estadoSeleccionado = value;
                OnPropertyChanged();
                OnEstadoSeleccionadoChanged();
            }
        }

        public EstadoVentaViewModel(ProyectoTallerContext context)
        {
            _context = context;
            ListaEstadosVenta = new ObservableCollection<EstadoVenta>(_context.EstadoVenta.ToList());
        }

        public event EventHandler<EstadoVenta>? EstadoSeleccionadoChanged;

        private void OnEstadoSeleccionadoChanged()
        {
            EstadoSeleccionadoChanged?.Invoke(this, EstadoSeleccionado);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
