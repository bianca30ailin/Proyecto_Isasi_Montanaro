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

        //constructor
        public EnvioViewModel(ProyectoTallerContext context)
        {
            _context = context;
            DireccionesCliente = new ObservableCollection<Direccion>();
            NuevaDireccionHabilitada = false;
            EnvioHabilitado = false;

            NuevaDireccionCommand = new RelayCommand(_ => NuevaDireccion());
            GuardarNuevaDireccionCommand = new RelayCommand(_ => GuardarNuevaDireccion());
            CargarEnvios();
        }

        public EnvioViewModel() : this(new ProyectoTallerContext())
        {
        }


        // --- PROPIEDADES ---
        //habilitar envio
        private bool _envioHabilitado;
        public bool EnvioHabilitado
        {
            get => _envioHabilitado;
            set
            {
                if (_envioHabilitado != value)
                {
                    _envioHabilitado = value;
                    OnPropertyChanged();
                }
            }
        }

        //lista de envio
        private ObservableCollection<Envio> _envios;
        public ObservableCollection<Envio> Envios
        {
            get => _envios;
            set
            {
                _envios = value;
                OnPropertyChanged(nameof(Envios));
            }
        }

        //traer direcciones del cliente
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

        //nueva direccion del cliente
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
        //cargar envios
        public void CargarEnvios()
        {
            Envios = new ObservableCollection<Envio>(
                _context.Envios
                    .Include(e => e.IdNroVentaNavigation)
                    .ThenInclude(v => v.DniClienteNavigation)
                    .Include(e => e.IdDireccionNavigation)
                    .Include(e => e.IdTransporteNavigation)
                    .Include(e => e.IdEstadoNavigation)
                    .ToList()
            );
        }

        //crear direccion nueva
        private void NuevaDireccion()
        {
            DireccionActual = new Direccion();
            NuevaDireccionHabilitada = true;
        }

        //guardar direccion nieva
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

        //Registro de envio
        public bool RegistrarEnvio(int idVenta, Transporte transporteSeleccionado)
        {
            if (!EnvioHabilitado)
                return false;

            var estadoPendiente = _context.Estados.FirstOrDefault(e => e.Nombre == "Pendiente");

            // Validaciones
            if (DireccionSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una dirección para el envío.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (transporteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un transporte para el envío.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Costo <= 0)
            {
                MessageBox.Show("Debe ingresar un costo válido para el envío.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Crear envío
            var nuevoEnvio = new Envio
            {
                IdNroVenta = idVenta,
                IdDireccion = DireccionSeleccionada.IdDireccion,
                IdEstado = estadoPendiente?.IdEstado ?? 4, // Pendiente por defecto
                Costo = Costo,
                FechaDespacho = null,
                NumSeguimiento = null,
                IdTransporte = transporteSeleccionado.IdTransporte
            };

            _context.Envios.Add(nuevoEnvio);
            _context.SaveChanges();

            MessageBox.Show("Envío registrado correctamente.", "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            return true;
        }

        //reinciar el enivo una vez sea exitoso
        public void Reiniciar()
        {
            DireccionSeleccionada = null;
            Costo = 0;
            EnvioHabilitado = false;
        }


        //notificacion de cambio
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
