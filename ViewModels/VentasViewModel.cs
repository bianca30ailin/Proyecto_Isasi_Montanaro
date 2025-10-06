using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public VentasViewModel()
        {
            _context = new ProyectoTallerContext();

            ClienteVM = new ClienteViewModel();
            DetalleVM = new DetalleVentaViewModel(_context);

            // --- Inicialización de envío ---
            EnvioHabilitado = false;

            // Inicializar venta
            int ultimoId = _context.Venta.Any() ? _context.Venta.Max(v => v.IdNroVenta) : 0;
            VentaActual = new Ventum
            {
                FechaHora = DateOnly.FromDateTime(DateTime.Now),
                Total = 0
            };

            ConfirmarVentaCommand = new RelayCommand(_ => ConfirmarVenta(), _ => DetalleVM.DetalleProductos.Any());
        }

        public ClienteViewModel ClienteVM { get; set; }
        public DetalleVentaViewModel DetalleVM { get; set; }

        public Ventum VentaActual { get; set; }


        // --- COMANDOS ---
        public ICommand ConfirmarVentaCommand { get; set; }


        // --- METODOS ---
        private void ConfirmarVenta()
        {
            // 1️⃣ Guardar cliente si no existe
            ClienteVM.GuardarClienteSiNoExiste();

            // 2️⃣ Asociar cliente y detalles
            VentaActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
            VentaActual.FechaHora = DateOnly.FromDateTime(DateTime.Now);
            VentaActual.Total = DetalleVM.Total;

            foreach (var d in DetalleVM.DetalleProductos)
                VentaActual.DetalleVentaProductos.Add(d);

            // 3️⃣ Guardar venta
            _context.Venta.Add(VentaActual);
            _context.SaveChanges(); // 👈 primero guardamos la venta

            // 4️⃣ Si el envío está habilitado, crear dirección + envío
            if (EnvioHabilitado)
            {
                // Validar que haya dirección cargada
                if (ClienteVM.DireccionActual != null)
                {
                    // Asociar la dirección al cliente
                    ClienteVM.DireccionActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
                    _context.Direccions.Add(ClienteVM.DireccionActual);
                    _context.SaveChanges();

                    // Crear registro de envío
                    var envio = new Envio
                    {
                        IdNroVenta = VentaActual.IdNroVenta,
                        Costo = 0, // Podés calcularlo después si tenés lógica
                        IdEstado = 1, // Ejemplo: "Pendiente"
                        FechaDespacho = null
                    };

                    _context.Envios.Add(envio);
                    _context.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Debe ingresar una dirección para el envío.", "Aviso",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // No continúa si no hay dirección
                }
            }

            // 5️⃣ Mostrar mensaje de éxito
            MessageBox.Show($"Venta registrada correctamente. N° {VentaActual.IdNroVenta}",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // 6️⃣ Reiniciar formularios
            DetalleVM.Reiniciar();
            ClienteVM.Reiniciar();
            VentaActual = new Ventum();
        }


        // --- Envío ---
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


        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
}
