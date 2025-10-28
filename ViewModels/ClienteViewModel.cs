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


        // --- COMANDOS ---
        public ICommand BuscarClienteCommand { get; set; }

        public ICommand VerClienteCommand { get; }


        // --- METODOS ---
        public void CargarClientes()
        {
            var lista = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Include(c => c.Venta)
                .ToList();

            Clientes = new ObservableCollection<Cliente>(lista);
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




        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

