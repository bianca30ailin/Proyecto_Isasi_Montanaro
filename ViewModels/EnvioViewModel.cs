using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Views;
using Proyecto_Isasi_Montanaro.Views.Formularios;
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

            // Cargar transportes para el filtro
            // Cargar transportes para el filtro
            Transportes = new ObservableCollection<Transporte>(_context.Transportes.ToList());

            // Insertar placeholder al inicio para que el Combo muestre "Transportes..." por defecto
            var transportePlaceholder = new Transporte
            {
                IdTransporte = 0,           // id reservado para el placeholder
                Nombre = "Transportes..."   // texto que se mostrará
            };

            Transportes.Insert(0, transportePlaceholder);

            // Seleccionar por defecto el placeholder
            TransporteFiltro = transportePlaceholder;


            // Opciones de ordenamiento
            OpcionesOrdenamiento = new ObservableCollection<string>
            {
                "Fecha más reciente",
                "Fecha más antigua",
                "Destinatario (A-Z)",
                "Destinatario (Z-A)",
                "Costo (menor a mayor)",
                "Costo (mayor a menor)",
                "N° Envío (menor a mayor)",
                "N° Envío (mayor a menor)"
            };

            // Cargar estados desde la base
            Estados = new ObservableCollection<Estado>(_context.Estados.ToList());

            OrdenamientoSeleccionado = "Fecha más reciente"; // Por defecto

            NuevaDireccionCommand = new RelayCommand(_ => NuevaDireccion());
            VerOrdenEnvioCommand = new RelayCommand(param => VerOrdenEnvio(param));
            FiltrarPorEstadoCommand = new RelayCommand(p => FiltrarPorEstado(p?.ToString()));
            FiltrarCommand = new RelayCommand(_ => AplicarFiltros());
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            EditarEnvioCommand = new RelayCommand(e => EditarEnvio(e as Envio));
            AbrirTransportesCommand = new RelayCommand(_ => AbrirTransportes());


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
                EnviosFiltrados = _envios;
            }
        }

        private ObservableCollection<Envio> _enviosFiltrados;
        public ObservableCollection<Envio> EnviosFiltrados
        {
            get => _enviosFiltrados;
            set
            {
                _enviosFiltrados = value;
                OnPropertyChanged();
            }
        }

        // --- PROPIEDADES DE FILTRADO ---
        private string _estadoFiltroActual;
        public string EstadoFiltroActual
        {
            get => _estadoFiltroActual;
            set
            {
                _estadoFiltroActual = value;
                OnPropertyChanged();
            }
        }

        private Transporte _transporteFiltro;
        public Transporte TransporteFiltro
        {
            get => _transporteFiltro;
            set
            {
                _transporteFiltro = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Transporte> Transportes { get; set; }
        public ObservableCollection<Estado> Estados { get; set; }


        private string _ordenamientoSeleccionado;
        public string OrdenamientoSeleccionado
        {
            get => _ordenamientoSeleccionado;
            set
            {
                _ordenamientoSeleccionado = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> OpcionesOrdenamiento { get; set; }

        private DateTime? _fechaDesde;
        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set
            {
                _fechaDesde = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _fechaHasta;
        public DateTime? FechaHasta
        {
            get => _fechaHasta;
            set
            {
                _fechaHasta = value;
                OnPropertyChanged();
            }
        }

        private string _busquedaTexto;
        public string BusquedaTexto
        {
            get => _busquedaTexto;
            set
            {
                _busquedaTexto = value;
                OnPropertyChanged();
                AplicarFiltros(); // Filtrar en tiempo real
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
        public ICommand VerOrdenEnvioCommand { get; set; }
        public ICommand FiltrarPorEstadoCommand { get; set; }
        public ICommand FiltrarCommand { get; set; }
        public ICommand LimpiarFiltrosCommand { get; set; }
        public ICommand EditarEnvioCommand { get; }
        public ICommand AbrirTransportesCommand { get; set; }


        // --- MÉTODOS ---
        //cargar envios
        public void CargarEnvios()
        {
            var enviosActualizados = _context.Envios
                .Include(e => e.IdNroVentaNavigation)
                    .ThenInclude(v => v.DniClienteNavigation)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdCiudadNavigation)
                        .ThenInclude(c => c.IdProvinciaNavigation)
                .Include(e => e.IdTransporteNavigation)
                .Include(e => e.IdEstadoNavigation)
                .AsNoTracking()  
                .ToList();

            // Crear NUEVA colección (fuerza actualización en UI)
            Envios = new ObservableCollection<Envio>(enviosActualizados);

            // Aplicar filtros actuales
            AplicarFiltros();
        }

        private void FiltrarPorEstado(string nombreEstado)
        {
            // Si no se pasa parámetro → mostrar todo
            if (string.IsNullOrEmpty(nombreEstado))
            {
                EstadoFiltroActual = null;
                AplicarFiltros();
                return;
            }

            // Si clickea el mismo filtro → desactivar
            if (EstadoFiltroActual == nombreEstado)
                EstadoFiltroActual = null;
            else
                EstadoFiltroActual = nombreEstado;

            AplicarFiltros();
        }

        


        private void AplicarFiltros()
        {
            if (Envios == null)
            {
                EnviosFiltrados = new ObservableCollection<Envio>();
                return;
            }

            var filtrados = Envios.AsEnumerable();

            // 1. Filtro por estado (chips)
            if (!string.IsNullOrEmpty(EstadoFiltroActual))
            {
                filtrados = filtrados.Where(e =>
                    e.IdEstadoNavigation?.Nombre?.Equals(EstadoFiltroActual, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            // 2. Filtro por transporte
            if (TransporteFiltro != null && TransporteFiltro.IdTransporte != 0)
            {
                filtrados = filtrados.Where(e => e.IdTransporte == TransporteFiltro.IdTransporte);
            }

            // 3. Filtro por fecha de despacho
            if (FechaDesde.HasValue)
            {
                filtrados = filtrados.Where(e =>
                    e.FechaDespacho.HasValue &&
                    e.FechaDespacho.Value.ToDateTime(TimeOnly.MinValue) >= FechaDesde.Value.Date
                );
            }

            if (FechaHasta.HasValue)
            {
                filtrados = filtrados.Where(e =>
                    e.FechaDespacho.HasValue &&
                    e.FechaDespacho.Value.ToDateTime(TimeOnly.MinValue) <= FechaHasta.Value.Date
                );
            }

            // 4. Filtro por búsqueda de texto
            if (!string.IsNullOrWhiteSpace(BusquedaTexto))
            {
                var busqueda = BusquedaTexto.ToLower();
                filtrados = filtrados.Where(e =>
                    e.IdEnvio.ToString().Contains(busqueda) ||
                    e.IdNroVenta.ToString().Contains(busqueda) ||
                    (e.IdNroVentaNavigation?.DniClienteNavigation?.NombreCompleto?.ToLower().Contains(busqueda) ?? false) ||
                    (e.IdDireccionNavigation?.IdCiudadNavigation?.Nombre?.ToLower().Contains(busqueda) ?? false) ||
                    (e.IdDireccionNavigation?.IdCiudadNavigation?.IdProvinciaNavigation?.Nombre?.ToLower().Contains(busqueda) ?? false) ||
                    (e.IdDireccionNavigation?.DireccionCompleta?.ToLower().Contains(busqueda) ?? false) ||
                    (e.NumSeguimiento?.ToLower().Contains(busqueda) ?? false)
                );
            }

            // 5. Aplicar ordenamiento
            filtrados = AplicarOrdenamiento(filtrados);

            EnviosFiltrados = new ObservableCollection<Envio>(filtrados);
        }

        
        private IEnumerable<Envio> AplicarOrdenamiento(IEnumerable<Envio> envios)
        {
            return OrdenamientoSeleccionado switch
            {
                "Fecha más reciente" => envios.OrderByDescending(e => e.FechaDespacho ?? DateOnly.MinValue),
                "Fecha más antigua" => envios.OrderBy(e => e.FechaDespacho ?? DateOnly.MaxValue),
                "Destinatario (A-Z)" => envios.OrderBy(e => e.IdNroVentaNavigation?.DniClienteNavigation?.NombreCompleto ?? ""),
                "Destinatario (Z-A)" => envios.OrderByDescending(e => e.IdNroVentaNavigation?.DniClienteNavigation?.NombreCompleto ?? ""),
                "Costo (menor a mayor)" => envios.OrderBy(e => e.Costo),
                "Costo (mayor a menor)" => envios.OrderByDescending(e => e.Costo),
                "N° Envío (menor a mayor)" => envios.OrderBy(e => e.IdEnvio),
                "N° Envío (mayor a menor)" => envios.OrderByDescending(e => e.IdEnvio),
                _ => envios.OrderByDescending(e => e.FechaDespacho ?? DateOnly.MinValue)
            };
        }

        private void LimpiarFiltros()
        {
            EstadoFiltroActual = null;
            // Volver a seleccionar el placeholder (si existe)
            TransporteFiltro = Transportes?.FirstOrDefault(t => t.IdTransporte == 0);

            FechaDesde = null;
            FechaHasta = null;
            BusquedaTexto = string.Empty;
            OrdenamientoSeleccionado = "Fecha más reciente";
            AplicarFiltros();
        }


        private void AbrirTransportes()
        {
            var ventana = new Transporte_form();
            var vm = new Transportes_form_ViewModel();

            

            ventana.DataContext = vm;
            ventana.ShowDialog();

            // Recargar transportes después de cerrar
            Transportes = new ObservableCollection<Transporte>(_context.Transportes.ToList());

            // Re-insertar placeholder
            var transportePlaceholder = new Transporte
            {
                IdTransporte = 0,
                Nombre = "Transportes..."
            };
            Transportes.Insert(0, transportePlaceholder);
            TransporteFiltro = transportePlaceholder;
        }

        private void EditarEnvio(Envio envioSeleccionado)
        {
            if (envioSeleccionado == null) return;

            // Abrimos la ventana flotante
            var ventana = new Editar_envio_form(envioSeleccionado)
            {
                DataContext = new Editar_envio_form_ViewModel(envioSeleccionado),

            };
            

            ventana.ShowDialog();

            //Recargar datos
            CargarEnvios();
            
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

        private void VerOrdenEnvio(object parametro)
        {
            var envioSeleccionado = parametro as Envio;
            if (envioSeleccionado == null)
                return;

            // Traigo el envío con sus relaciones cargadas
            var envio = _context.Envios
                .Include(e => e.IdNroVentaNavigation)
                    .ThenInclude(v => v.DniClienteNavigation)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdCiudadNavigation)
                .Include(e => e.IdTransporteNavigation)
                .Include(e => e.IdEstadoNavigation)
                .FirstOrDefault(e => e.IdEnvio == envioSeleccionado.IdEnvio);

            // Creo la ventana
            var ventana = new DetalleOrdenEnvio();
            ventana.DataContext = new DetalleOrdenEnvioViewModel(envio);
            ventana.ShowDialog();
        }

        
        //notificacion de cambio
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
