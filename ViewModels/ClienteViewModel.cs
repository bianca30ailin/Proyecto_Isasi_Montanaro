using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
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
    public class ClienteViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        //Constructos
        public ClienteViewModel()
        {
            _context = new ProyectoTallerContext();
            ClienteActual = new Cliente();
            BuscarClienteCommand = new RelayCommand(_ => BuscarCliente());
            VerClienteCommand = new RelayCommand(p => VerCliente(p as Cliente));
            EliminarClienteCommand = new RelayCommand(p => EliminarCliente(p as Cliente));
            FiltroActivosCommand = new RelayCommand(_ => AplicarFiltroActivos());
            FiltroInactivosCommand = new RelayCommand(_ => AplicarFiltroInactivos());
            AplicarOrdenCommand = new RelayCommand(_ => FiltrarPorOrdenYFechas());
            LimpiarOrdenCommand = new RelayCommand(_ => LimpiarOrdenYFechas());
            CargarClientes();
        }


        //Propiedades
        private string _dniClienteInput;

        public Direccion? DireccionActual { get; set; }
        public string DniClienteInput
        {
            get => _dniClienteInput;
            set { _dniClienteInput = value; OnPropertyChanged(); }
        }

        private Cliente _clienteActual;
        public Cliente ClienteActual
        {
            get => _clienteActual;
            set
            {
                _clienteActual = value;
                OnPropertyChanged();

                if (_clienteActual != null)
                {
                    // 🔹 Sincronizar los campos visibles
                    OnPropertyChanged(nameof(Nombre));
                    OnPropertyChanged(nameof(Apellido));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Telefono));
                    OnPropertyChanged(nameof(DniCliente));
                }
            }
        }

        private bool _modoSoloLectura;
        public bool ModoSoloLectura
        {
            get => _modoSoloLectura;
            set { _modoSoloLectura = value; OnPropertyChanged(); }
        }

        // Lista de direcciones del cliente actual
        private ObservableCollection<Direccion> _direccionesCliente;
        public ObservableCollection<Direccion> DireccionesCliente
        {
            get => _direccionesCliente;
            set { _direccionesCliente = value; OnPropertyChanged(); }
        }

        // Lista de clientes
        private ObservableCollection<Cliente> _clientes;
        public ObservableCollection<Cliente> Clientes
        {
            get => _clientes;
            set { _clientes = value; OnPropertyChanged(); }
        }

        //Preopiedaddes de error para validaciones
        private string _errorNombre;
        public string ErrorNombre
        {
            get => _errorNombre;
            set { _errorNombre = value; OnPropertyChanged(); }
        }

        private string _errorApellido;
        public string ErrorApellido
        {
            get => _errorApellido;
            set { _errorApellido = value; OnPropertyChanged(); }
        }

        private string _errorEmail;
        public string ErrorEmail
        {
            get => _errorEmail;
            set { _errorEmail = value; OnPropertyChanged(); }
        }

        private string _errorTelefono;
        public string ErrorTelefono
        {
            get => _errorTelefono;
            set { _errorTelefono = value; OnPropertyChanged(); }
        }

        private string _errorDni;
        public string ErrorDni
        {
            get => _errorDni;
            set { _errorDni = value; OnPropertyChanged(); }
        }

        // Filtrado

        private bool _filtroActivosPresionado = false;
        private bool _filtroInactivosPresionado = false;

        public bool FiltroActivosPresionado
        {
            get => _filtroActivosPresionado;
            set
            {
                _filtroActivosPresionado = value;
                OnPropertyChanged();
            }
        }

        public bool FiltroInactivosPresionado
        {
            get => _filtroInactivosPresionado;
            set
            {
                _filtroInactivosPresionado = value;
                OnPropertyChanged();
            }
        }

        public string CriterioOrdenamiento { get; set; }

        private DateOnly? _fechaDesde;
        public DateOnly? FechaDesde
        {
            get => _fechaDesde;
            set { _fechaDesde = value; OnPropertyChanged(); }
        }

        private DateOnly? _fechaHasta;
        public DateOnly? FechaHasta
        {
            get => _fechaHasta;
            set { _fechaHasta = value; OnPropertyChanged(); }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();

                // Ejecuta la búsqueda automáticamente cuando cambia el texto
                FiltrarBusqueda();
            }
        }

        // --- COMANDOS ---
        public ICommand BuscarClienteCommand { get; set; }

        public ICommand VerClienteCommand { get; }

        public ICommand EliminarClienteCommand { get; }

        public ICommand FiltroActivosCommand { get; }
        public ICommand FiltroInactivosCommand { get; }


        public ICommand AplicarOrdenCommand { get; }
        public ICommand LimpiarOrdenCommand { get; }


        // --- METODOS ---
        public void CargarClientes()
        {
            var lista = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Include(c => c.Venta)
                .ToList();

            _todosLosClientes = lista;
            Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
        }

        private void VerCliente(Cliente clienteSeleccionado)
        {
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente válido.");
                return;
            }

            // Guardar en la propiedad actual
            ClienteActual = clienteSeleccionado;
            CargarDirecciones(); // ✅ para que se carguen las direcciones también

            // Abrir el formulario de detalle
            var ventana = new Cliente_form
            {
                DataContext = this // vincula el ViewModel actual al formulario
            };

            ventana.ShowDialog(); // abre la ventana modal
        }

        private void BuscarCliente()
        {
            if (string.IsNullOrWhiteSpace(DniClienteInput))
            {
                MessageBox.Show("Por favor, ingrese un DNI válido.");
                return;
            }

            if (!int.TryParse(DniClienteInput, out int dni))
            {
                MessageBox.Show("El DNI debe ser numérico.");
                return;
            }

            var cliente = _context.Clientes.FirstOrDefault(c => c.DniCliente == dni);

            if (cliente != null)
            {
                ClienteActual = cliente;
                CargarDirecciones();
                MessageBox.Show($"Cliente encontrado: {cliente.Nombre} {cliente.Apellido}");
            }
            else
            {
                MessageBox.Show("Cliente no encontrado, estás por registrar uno nuevo.");
                ClienteActual = new Cliente { DniCliente = dni };
            }

            OnPropertyChanged(nameof(ClienteActual));
        }

        public void GuardarClienteSiNoExiste()
        {

            var existente = _context.Clientes.FirstOrDefault(c => c.DniCliente == ClienteActual.DniCliente);

            if (existente == null)
            {
                _context.Clientes.Add(ClienteActual);
                _context.SaveChanges();
                MessageBox.Show($"Cliente nuevo registrado: {ClienteActual.Nombre} {ClienteActual.Apellido}");
            }
            else
            {
                existente.DniCliente = ClienteActual.DniCliente;
                existente.Nombre = ClienteActual.Nombre;
                existente.Apellido = ClienteActual.Apellido;
                existente.Telefono = ClienteActual.Telefono;
                existente.Email = ClienteActual.Email;
                _context.Clientes.Update(existente);
                _context.SaveChanges();
            }
        }

        public void CargarDirecciones()
        {
            if (ClienteActual == null) return;

            var direcciones = _context.Direccions
                .Include(d => d.IdCiudadNavigation)
                 .ThenInclude(c => c.IdProvinciaNavigation)
                .Where(d => d.DniCliente == ClienteActual.DniCliente)
                .ToList();

            DireccionesCliente = new ObservableCollection<Direccion>(direcciones);

            //para notificar a ventas
            OnPropertyChanged(nameof(DireccionesCliente));
        }

        public void Reiniciar()
        {
            DniClienteInput = string.Empty;
            ClienteActual = new Cliente();

            OnPropertyChanged(nameof(DniClienteInput));
            OnPropertyChanged(nameof(ClienteActual));
        }

        private void EliminarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                MessageBox.Show("Debe seleccionar un cliente válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmación antes de dar de baja
            var resultado = MessageBox.Show(
                $"¿Seguro que desea dar de baja al cliente {cliente.Nombre} {cliente.Apellido}?",
                "Confirmar baja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            // Baja lógica
            cliente.Activo = false;
            _context.Clientes.Update(cliente);
            _context.SaveChanges();

            // Actualizar la lista visible
            CargarClientes();

            MessageBox.Show($"El cliente {cliente.Nombre} {cliente.Apellido} fue dado de baja correctamente.",
                            "Baja realizada", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private List<Cliente> _todosLosClientes;
        private void AplicarFiltroActivos()
        {
            // Si el filtro ya estaba activo, lo desactivamos y mostramos todos
            if (FiltroActivosPresionado)
            {
                FiltroActivosPresionado = false;
                Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
                return;
            }

            // Activamos este filtro y desactivamos el otro
            FiltroActivosPresionado = true;
            FiltroInactivosPresionado = false;

            var filtrados = _todosLosClientes.Where(c => c.Activo).ToList();
            Clientes = new ObservableCollection<Cliente>(filtrados);
        }

        private void AplicarFiltroInactivos()
        {
            if (FiltroInactivosPresionado)
            {
                FiltroInactivosPresionado = false;
                Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
                return;
            }

            FiltroInactivosPresionado = true;
            FiltroActivosPresionado = false;

            var filtrados = _todosLosClientes.Where(c => !c.Activo).ToList();
            Clientes = new ObservableCollection<Cliente>(filtrados);
        }

        private void FiltrarPorOrdenYFechas()
        {
            IEnumerable<Cliente> filtrados = _todosLosClientes;

            // --- FILTRO POR FECHAS ---
            if (FechaDesde.HasValue || FechaHasta.HasValue)
            {
                filtrados = filtrados.Where(c =>
                {
                    var fechaAlta = c.Venta.Any() ? c.Venta.Min(v => v.FechaHora) : (DateOnly?)null;
                    return fechaAlta.HasValue &&
                           (!FechaDesde.HasValue || fechaAlta.Value >= FechaDesde.Value) &&
                           (!FechaHasta.HasValue || fechaAlta.Value <= FechaHasta.Value);
                });
            }

            // --- ORDENAMIENTO ---
            switch (CriterioOrdenamiento)
            {
                case "A-Z":
                    filtrados = filtrados.OrderByDescending(c => c.Apellido).ThenByDescending(c => c.Nombre);
                    break;

                case "Z-A":
                    filtrados = filtrados.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);
                    break;

                case "Más ventas":
                    filtrados = filtrados.OrderByDescending(c => c.CantidadVentas);
                    break;

                case "Menos ventas":
                    filtrados = filtrados.OrderBy(c => c.CantidadVentas);
                    break;

                case "Todos":
                default:
                    // no aplicar orden, simplemente mantener la lista original (ordenada por apellido/nombre)
                    filtrados = filtrados.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);
                    break;
            }

            Clientes = new ObservableCollection<Cliente>(filtrados);
        }

        private void LimpiarOrdenYFechas()
        {
            FechaDesde = null;
            FechaHasta = null;
            CriterioOrdenamiento = null;

            Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
        }

        // Validaciones
        public string Nombre
        {
            get => ClienteActual.Nombre;
            set
            {
                ClienteActual.Nombre = value;
                OnPropertyChanged();

                // validación inmediata
                if (string.IsNullOrWhiteSpace(value))
                    ErrorNombre = "El nombre no puede estar vacío.";
                else if (!value.All(c => char.IsLetter(c) || c == ' '))
                    ErrorNombre = "Solo se permiten letras.";
                else
                    ErrorNombre = string.Empty;
            }
        }

        public string Apellido
        {
            get => ClienteActual.Apellido;
            set
            {
                ClienteActual.Apellido = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(value))
                    ErrorApellido = "El apellido no puede estar vacío.";
                else if (!value.All(c => char.IsLetter(c) || c == ' '))
                    ErrorApellido = "Solo se permiten letras.";
                else
                    ErrorApellido = string.Empty;
            }
        }

        public string Email
        {
            get => ClienteActual.Email;
            set
            {
                ClienteActual.Email = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(value))
                    ErrorEmail = "El email es obligatorio.";
                else if (!value.Contains("@"))
                    ErrorEmail = "Debe contener '@'.";
                else
                    ErrorEmail = string.Empty;
            }
        }

        public string Telefono
        {
            get => ClienteActual.Telefono;
            set
            {
                ClienteActual.Telefono = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(value))
                    ErrorTelefono = "El teléfono es obligatorio.";
                else if (!value.All(char.IsDigit))
                    ErrorTelefono = "Solo se permiten números.";
                else
                    ErrorTelefono = string.Empty;
            }
        }

        public int DniCliente
        {
            get => ClienteActual.DniCliente;
            set
            {
                ClienteActual.DniCliente = value;
                OnPropertyChanged();

                if (value.ToString().Length != 8)
                    ErrorDni = "El DNI debe tener 8 dígitos.";
                else
                    ErrorDni = string.Empty;
            }
        }

        private void FiltrarBusqueda()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                // Si no hay texto, mostramos todo
                Clientes = new ObservableCollection<Cliente>(_todosLosClientes);
                return;
            }

            var termino = TextoBusqueda.ToLower();

            var filtrados = _todosLosClientes.Where(c =>
                (!string.IsNullOrEmpty(c.Nombre) && c.Nombre.ToLower().Contains(termino)) ||
                (!string.IsNullOrEmpty(c.Apellido) && c.Apellido.ToLower().Contains(termino)) ||
                c.DniCliente.ToString().Contains(termino)
            ).ToList();

            Clientes = new ObservableCollection<Cliente>(filtrados);
        }



        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

