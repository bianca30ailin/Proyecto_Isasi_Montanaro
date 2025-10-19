using Microsoft.EntityFrameworkCore;
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
    public class EnvioViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public EnvioViewModel(ProyectoTallerContext context)
        {
            _context = context;
            DireccionesCliente = new ObservableCollection<Direccion>();
            NuevaDireccionHabilitada = false;

            NuevaDireccionCommand = new RelayCommand(_ => NuevaDireccion());
            GuardarNuevaDireccionCommand = new RelayCommand(_ => GuardarNuevaDireccion());
        }

        // --- PROPIEDADES ---
        private bool _envioHabilitado;
        public bool EnvioHabilitado
        {
            get => _envioHabilitado;
            set { _envioHabilitado = value; OnPropertyChanged(nameof(EnvioHabilitado)); }
        }

        private ObservableCollection<Direccion> _direccionesCliente;
        public ObservableCollection<Direccion> DireccionesCliente
        {
            get => _direccionesCliente;
            set { _direccionesCliente = value; OnPropertyChanged(nameof(DireccionesCliente)); }
        }

        private Direccion? _direccionSeleccionada;
        public Direccion? DireccionSeleccionada
        {
            get => _direccionSeleccionada;
            set { _direccionSeleccionada = value; OnPropertyChanged(nameof(DireccionSeleccionada)); }
        }

        private bool _nuevaDireccionHabilitada;
        public bool NuevaDireccionHabilitada
        {
            get => _nuevaDireccionHabilitada;
            set { _nuevaDireccionHabilitada = value; OnPropertyChanged(nameof(NuevaDireccionHabilitada)); }
        }

        public Direccion DireccionActual { get; set; } = new Direccion();

        
        
        // --- COMANDOS ---
        public ICommand NuevaDireccionCommand { get; set; }
        public ICommand GuardarNuevaDireccionCommand { get; set; }




        // --- MÉTODOS ---

        private void NuevaDireccion()
        {
            DireccionActual = new Direccion();
            NuevaDireccionHabilitada = true;
        }

        private void GuardarNuevaDireccion()
        {
            if (DireccionActual == null || string.IsNullOrEmpty(DireccionActual.NombreCalle))
            {
                MessageBox.Show("Debe completar los datos de la nueva dirección.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Direccions.Add(DireccionActual);
            _context.SaveChanges();

            DireccionesCliente.Add(DireccionActual);
            DireccionSeleccionada = DireccionActual;

            MessageBox.Show("Dirección guardada correctamente.", "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            NuevaDireccionHabilitada = false;
        }

        //Propiedad del costo del envio
        private double _costo;
        public double Costo
        {
            get => _costo;
            set
            {
                _costo = value;
                OnPropertyChanged(nameof(Costo));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
