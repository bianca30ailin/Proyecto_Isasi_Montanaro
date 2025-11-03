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

            // Cargar provincias al iniciar
            Provincias = new ObservableCollection<Provincium>(_context.Provincia.ToList());
            Ciudades = new ObservableCollection<Ciudad>(); // se llena cuando elija provincia

            NuevaDireccionCommand = new RelayCommand(_ => NuevaDireccion());
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

        //cliente actual
        private Cliente _clienteActual;
        public Cliente ClienteActual
        {
            get => _clienteActual;
            set
            {
                _clienteActual = value;
                OnPropertyChanged();
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
            set
            {
                if (_direccionSeleccionada != value)
                {
                    _direccionSeleccionada = value;
                    OnPropertyChanged(nameof(DireccionSeleccionada));
                    OnPropertyChanged(nameof(DireccionEditable));

                    if (value != null)
                    {
                        // 🔹 Buscar la ciudad y provincia asociadas
                        var ciudadDir = _context.Ciudads
                            .Include(c => c.IdProvinciaNavigation)
                            .FirstOrDefault(c => c.IdCiudad == value.IdCiudad);

                        if (ciudadDir != null)
                        {
                            // 🔹 Si las listas no están cargadas, las traemos
                            if (Provincias == null || Provincias.Count == 0)
                                Provincias = new ObservableCollection<Provincium>(_context.Provincia.ToList());

                            // 🔹 Seleccionar provincia
                            ProvinciaSeleccionada = Provincias.FirstOrDefault(p => p.IdProvincia == ciudadDir.IdProvincia);

                            // 🔹 Cargar ciudades de esa provincia
                            Ciudades = new ObservableCollection<Ciudad>(
                                _context.Ciudads.Where(c => c.IdProvincia == ciudadDir.IdProvincia).ToList()
                            );
                            OnPropertyChanged(nameof(Ciudades));

                            // 🔹 Seleccionar ciudad
                            CiudadSeleccionada = Ciudades.FirstOrDefault(c => c.IdCiudad == ciudadDir.IdCiudad);
                        }
                    }

                    // 🔹 Validar dirección al final
                    ValidarDireccion();
                }
            }
        }

        //nueva direccion del cliente
        private bool _nuevaDireccionHabilitada;
        public bool NuevaDireccionHabilitada
        {
            get => _nuevaDireccionHabilitada;
            set { _nuevaDireccionHabilitada = value; OnPropertyChanged(nameof(NuevaDireccionHabilitada)); }
        }

        public Direccion DireccionActual { get; set; } = new Direccion();

        public Direccion DireccionEditable
        {
            get => NuevaDireccionHabilitada ? DireccionActual : DireccionSeleccionada;
        }


        private bool _modoSoloLectura;
        public bool ModoSoloLectura
        {
            get => _modoSoloLectura;
            set { _modoSoloLectura = value; OnPropertyChanged(); }
        }

        //lista de provincias y ciudades
        public ObservableCollection<Provincium> Provincias { get; set; }
        public ObservableCollection<Ciudad> Ciudades { get; set; }

        // --- PROVINCIA ---
        private Provincium _provinciaSeleccionada;
        public Provincium ProvinciaSeleccionada
        {
            get => _provinciaSeleccionada;
            set
            {
                _provinciaSeleccionada = value;
                OnPropertyChanged();

                // 🔹 Validación
                if (value == null)
                    ErrorProvincia = "Debe seleccionar una provincia.";
                else
                    ErrorProvincia = string.Empty;

                // 🔹 Filtrar ciudades según provincia elegida
                if (value != null)
                {
                    Ciudades = new ObservableCollection<Ciudad>(
                        _context.Ciudads
                            .Where(c => c.IdProvincia == value.IdProvincia)
                            .ToList()
                    );
                    OnPropertyChanged(nameof(Ciudades));
                }

                // 🔹 Revalidar toda la dirección
                ValidarDireccion();
            }
        }

        // --- CIUDAD ---
        private Ciudad _ciudadSeleccionada;
        public Ciudad CiudadSeleccionada
        {
            get => _ciudadSeleccionada;
            set
            {
                _ciudadSeleccionada = value;
                OnPropertyChanged();

                // 🔹 Validación
                if (value == null)
                    ErrorCiudad = "Debe seleccionar una ciudad.";
                else
                    ErrorCiudad = string.Empty;

                // 🔹 Revalidar dirección completa (por si faltaban otros campos)
                ValidarDireccion();
            }
        }

        // --- COMANDOS ---
        public ICommand NuevaDireccionCommand { get; set; }
    


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
            if (ClienteActual == null)
            {
                MessageBox.Show("Debe seleccionar un cliente antes de agregar una dirección.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DireccionActual = new Direccion
            {
                DniCliente = ClienteActual.DniCliente 
            };

            NuevaDireccionHabilitada = true;

            OnPropertyChanged(nameof(DireccionEditable));

            ErrorCalle = string.Empty;
            ErrorAltura = string.Empty;
            ErrorProvincia = string.Empty;
            ErrorCiudad = string.Empty;
            ValidarDireccion();
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

            return true;
        }

        //reinciar el enivo una vez sea exitoso
        public void Reiniciar()
        {
            DireccionSeleccionada = null;
            DireccionActual = null;
            NuevaDireccionHabilitada = false;
            OnPropertyChanged(nameof(DireccionEditable)); // refresca la vista
            Costo = 0;
            EnvioHabilitado = false;
        }


        // --- ERRORES ---
        private string _errorCalle;
        public string ErrorCalle
        {
            get => _errorCalle;
            set { _errorCalle = value; OnPropertyChanged(); }
        }

        private string _errorAltura;
        public string ErrorAltura
        {
            get => _errorAltura;
            set { _errorAltura = value; OnPropertyChanged(); }
        }

        private string _errorProvincia;
        public string ErrorProvincia
        {
            get => _errorProvincia;
            set { _errorProvincia = value; OnPropertyChanged(); }
        }

        private string _errorCiudad;
        public string ErrorCiudad
        {
            get => _errorCiudad;
            set { _errorCiudad = value; OnPropertyChanged(); }
        }


        // --- VALIDACIÓN CENTRAL ---
        private void ValidarDireccion()
        {
            var dir = DireccionEditable;
            if (dir == null) return;

            ErrorCalle = string.IsNullOrWhiteSpace(dir.NombreCalle)
                ? "La calle es obligatoria."
                : string.Empty;

            ErrorAltura = dir.Altura <= 0
                ? "Debe ingresar una altura válida."
                : string.Empty;

            ErrorProvincia = ProvinciaSeleccionada == null
                ? "Debe seleccionar una provincia."
                : string.Empty;

            ErrorCiudad = CiudadSeleccionada == null
                ? "Debe seleccionar una ciudad."
                : string.Empty;
        }


        //notificacion de cambio
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
