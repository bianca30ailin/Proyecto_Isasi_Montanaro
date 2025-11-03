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
    public class TransporteViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public TransporteViewModel(ProyectoTallerContext context)
        {
            _context = context;
            CargarTransportes();
        }

        private ObservableCollection<Transporte> _transportes;
        public ObservableCollection<Transporte> Transportes
        {
            get => _transportes;
            set { _transportes = value; OnPropertyChanged(); }
        }

        private Transporte? _transporteSeleccionado;
        public Transporte? TransporteSeleccionado
        {
            get => _transporteSeleccionado;
            set { _transporteSeleccionado = value; OnPropertyChanged(); }
        }

        private bool _modoSoloLectura;
        public bool ModoSoloLectura
        {
            get => _modoSoloLectura;
            set { _modoSoloLectura = value; OnPropertyChanged(); }
        }

        // --- Método para cargar transportes desde la base ---
        public void CargarTransportes()
        {
            var lista = _context.Transportes
                .OrderBy(t => t.Nombre)
                .ToList();

            Transportes = new ObservableCollection<Transporte>(lista);
        }

        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
