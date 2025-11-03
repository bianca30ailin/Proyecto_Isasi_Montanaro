using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class FormaPagoViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public FormaPagoViewModel(ProyectoTallerContext context)
        {
            _context = context;
            CargarFormasDePago();
        }

        private ObservableCollection<FormaPago> _formasPago;
        public ObservableCollection<FormaPago> FormasPago
        {
            get => _formasPago;
            set { _formasPago = value; OnPropertyChanged(nameof(FormasPago)); }
        }

        private FormaPago _formaPagoSeleccionada;
        public FormaPago FormaPagoSeleccionada
        {
            get => _formaPagoSeleccionada;
            set
            {
                if (_formaPagoSeleccionada != value)
                {
                    _formaPagoSeleccionada = value;
                    OnPropertyChanged(nameof(FormaPagoSeleccionada));
                    ActualizarCuotasDisponibles();
                }
            }
        }

        private ObservableCollection<int> _cuotasDisponibles;
        public ObservableCollection<int> CuotasDisponibles
        {
            get => _cuotasDisponibles;
            set { _cuotasDisponibles = value; OnPropertyChanged(nameof(CuotasDisponibles)); }
        }

        private int? _cuotaSeleccionada;
        public int? CuotaSeleccionada
        {
            get => _cuotaSeleccionada;
            set { _cuotaSeleccionada = value; OnPropertyChanged(nameof(CuotaSeleccionada)); }
        }

        private bool _modoSoloLectura;
        public bool ModoSoloLectura
        {
            get => _modoSoloLectura;
            set { _modoSoloLectura = value; OnPropertyChanged(); }
        }

        private void CargarFormasDePago()
        {
            var lista = _context.FormaPago.ToList();
            FormasPago = new ObservableCollection<FormaPago>(lista);

            // Por defecto, seleccionar “Débito”
            FormaPagoSeleccionada = FormasPago.FirstOrDefault(f => f.Nombre == "Efectivo");
        }

        private void ActualizarCuotasDisponibles()
        {
            if (FormaPagoSeleccionada != null && FormaPagoSeleccionada.Nombre == "Crédito")
            {
                CuotasDisponibles = new ObservableCollection<int> { 1, 2, 3 };
            }
            else
            {
                CuotasDisponibles = new ObservableCollection<int>(); // Vacía
                CuotaSeleccionada = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

