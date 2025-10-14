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
            set { _clienteActual = value; OnPropertyChanged(); }
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
                existente.Nombre = ClienteActual.Nombre;
                existente.Apellido = ClienteActual.Apellido;
                existente.Telefono = ClienteActual.Telefono;
                existente.Email = ClienteActual.Email;
                _context.Clientes.Update(existente);
                _context.SaveChanges();
            }
        }

        private void CargarDirecciones()
        {
            if (ClienteActual == null) return;

            var direcciones = _context.Direccions
                .Include(d => d.IdCiudadNavigation)
                .Where(d => d.DniCliente == ClienteActual.DniCliente)
                .ToList();

            DireccionesCliente = new ObservableCollection<Direccion>(direcciones);
        }

        public void Reiniciar()
        {
            DniClienteInput = string.Empty;
            ClienteActual = new Cliente();

            OnPropertyChanged(nameof(DniClienteInput));
            OnPropertyChanged(nameof(ClienteActual));
        }


        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

