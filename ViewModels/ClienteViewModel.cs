using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
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

        public ClienteViewModel()
        {
            _context = new ProyectoTallerContext();
            ClienteActual = new Cliente();
            BuscarClienteCommand = new RelayCommand(_ => BuscarCliente());
        }

        private string _dniClienteInput;
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


        // --- COMANDOS ---
        public ICommand BuscarClienteCommand { get; set; }


        // --- METODOS ---
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

