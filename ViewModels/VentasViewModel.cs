using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.ComponentModel;
using System.IO;
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
            EnvioVM = new EnvioViewModel(_context);
            TransporteVM = new TransporteViewModel(_context);
            FormaPagoVM = new FormaPagoViewModel(_context);


            // Recalcular totales al cambiar la forma de pago o cuotas
            FormaPagoVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FormaPagoVM.FormaPagoSeleccionada) ||
                    e.PropertyName == nameof(FormaPagoVM.CuotaSeleccionada))
                {
                    OnPropertyChanged(nameof(EsCredito));
                    OnPropertyChanged(nameof(MostrarMontoPorCuota));
                    RecalcularTotales();
                }
            };

            // Recalcular totales al cambiar el envío
            EnvioVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EnvioVM.EnvioHabilitado) ||
                    e.PropertyName == nameof(EnvioVM.Costo))
                {
                    RecalcularTotales();
                }
            };

            // Recalcular totales al cambiar el detalle (subtotal)
            DetalleVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DetalleVentaViewModel.Total))
                {
                    OnPropertyChanged(nameof(Subtotal));
                    RecalcularTotales();
                }
            };

            // Calcular al iniciar
            RecalcularTotales();

            // lista de estados de venta
            ListaEstadosVenta = new ObservableCollection<EstadoVenta>(_context.EstadoVenta.ToList());

            // comandos
            VerVentaSoloLecturaCommand = new RelayCommand(p => VerVentaSoloLectura(p as Ventum));

            CancelarVentaCommand = new RelayCommand(p => CancelarVenta(p as Ventum));

      
            ClienteVM.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ClienteVM.ClienteActual):
                        // refresco direcciones del cliente nuevo
                        ClienteVM.CargarDirecciones();

                        // paso datos al EnvioVM
                        EnvioVM.DireccionesCliente = ClienteVM.DireccionesCliente;
                        EnvioVM.ClienteActual = ClienteVM.ClienteActual;
                        OnPropertyChanged(nameof(EnvioVM));
                        break;

                    case nameof(ClienteVM.DireccionesCliente):
                        // si cambian las direcciones, reflejo en EnvioVM
                        EnvioVM.DireccionesCliente = ClienteVM.DireccionesCliente;
                        OnPropertyChanged(nameof(EnvioVM));
                        break;
                }
            };

            // Calcular número próximo (solo para mostrar)
            int ultimoId = _context.Venta.Any() ? _context.Venta.Max(v => v.IdNroVenta) : 0;
            ProximoIdVenta = ultimoId + 1;

            // Inicializar venta
            VentaActual = new Ventum
            {
                FechaHora = DateOnly.FromDateTime(DateTime.Now),
                Total = 0,
                EstadoVentaId = 1
            };

            EstadoVentaSeleccionado = ListaEstadosVenta.FirstOrDefault(e => e.NombreEstado == "Activa");

            ConfirmarVentaCommand = new RelayCommand(_ => ConfirmarVenta(), _ => DetalleVM.DetalleProductos.Any());

            CargarVentas();

            // Inicializar comando de filtrado
            FiltrarPorEstadoCommand = new RelayCommand(FiltrarPorEstado);

            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());

            // Establecer filtro por defecto al iniciar
            EstadoFiltroSeleccionado = "Activa";

            CriteriosOrden = new ObservableCollection<string>
            {
                "Número de venta (Ascendente)",
                "Número de venta (Descendente)",
                "Total (Mayor a menor)",
                "Total (Menor a mayor)",
                "Vendedor (A-Z)",
                "Vendedor (Z-A)",
                "Fecha (Más reciente)",
                "Fecha (Más antigua)"
            };

            CriterioOrdenSeleccionado = "Número de venta (Descendente)";
        }

        // Modelos
        public ClienteViewModel ClienteVM { get; set; }

        public DetalleVentaViewModel DetalleVM { get; set; }

        public EnvioViewModel EnvioVM { get; set; }

        public FormaPagoViewModel FormaPagoVM { get; set; }

        public TransporteViewModel TransporteVM { get; set; }

        public ObservableCollection<EstadoVenta> ListaEstadosVenta { get; set; }

        public Ventum VentaActual { get; set; }

        private ObservableCollection<Ventum> _ventas;

        public ObservableCollection<Ventum> Ventas
        {
            get => _ventas;
            set { _ventas = value; OnPropertyChanged(nameof(Ventas)); }
        }

        public ObservableCollection<string> CriteriosOrden { get; set; }

        // --- FILTRADO POR ESTADO ---

        // Ventas originales (sin filtrar)
        private ObservableCollection<Ventum> _todasLasVentas;
        public ObservableCollection<Ventum> TodasLasVentas
        {
            get => _todasLasVentas;
            set { _todasLasVentas = value; OnPropertyChanged(); }
        }

        // Estado actual del filtro
        private string _estadoFiltroSeleccionado;
        public string EstadoFiltroSeleccionado
        {
            get => _estadoFiltroSeleccionado;
            set
            {
                _estadoFiltroSeleccionado = value;
                OnPropertyChanged();
                FiltrarPorEstado(value);
            }
        }

        // --- ORDENAMIENTO DE TABLA ---
        private string _criterioOrdenSeleccionado;
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set
            {
                _criterioOrdenSeleccionado = value;
                OnPropertyChanged();
                AplicarOrdenamiento(value);
            }
        }

        // --- FILTRO POR FECHAS ---
        private DateTime? _fechaDesde;
        public DateTime? FechaDesde
        {
            get => _fechaDesde;
            set
            {
                _fechaDesde = value;
                OnPropertyChanged();
                FiltrarPorPeriodo(); // se actualiza automáticamente
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
                FiltrarPorPeriodo();
            }
        }

        // --- BUSCADOR ---
        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                FiltrarPorTexto(); // se actualiza en tiempo real
            }
        }

        // --- COMANDOS ---
        public ICommand ConfirmarVentaCommand { get; set; }

        public ICommand VerVentaSoloLecturaCommand { get; set; }

        public ICommand CancelarVentaCommand { get; }

        public ICommand FiltrarPorEstadoCommand { get; set; }

        public ICommand LimpiarFiltrosCommand { get; set; }


        // --- TOTALES Y CUOTAS --- //
        public double Subtotal => DetalleVM?.Total ?? 0;

        private double _recargo;
        public double Recargo
        {
            get => _recargo;
            set { _recargo = value; OnPropertyChanged(); }
        }

        private double _totalFinal;
        public double TotalFinal
        {
            get => _totalFinal;
            set { _totalFinal = value; OnPropertyChanged(); OnPropertyChanged(nameof(MontoPorCuota)); }
        }

        private EstadoVenta _estadoVentaSeleccionado;
        public EstadoVenta EstadoVentaSeleccionado
        {
            get => _estadoVentaSeleccionado;
            set
            {
                _estadoVentaSeleccionado = value;
                OnPropertyChanged();
            }
        }

        // Indica si el método de pago es crédito
        public bool EsCredito => FormaPagoVM?.FormaPagoSeleccionada?.Nombre?.Equals("Crédito", StringComparison.OrdinalIgnoreCase) == true;

        // Mostrar monto por cuota sólo si hay cuotas > 1
        public bool MostrarMontoPorCuota => EsCredito && (FormaPagoVM?.CuotaSeleccionada ?? 1) > 1;

        // Monto calculado por cuota
        public double MontoPorCuota =>
            EsCredito && FormaPagoVM.CuotaSeleccionada.HasValue
                ? Math.Round(TotalFinal / FormaPagoVM.CuotaSeleccionada.Value, 2)
                : TotalFinal;

        // --- METODOS ---
        private void ConfirmarVenta()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Guardar cliente si no existe
                ClienteVM.GuardarClienteSiNoExiste();

                // Validar stock actual en base de datos
                foreach (var detalle in DetalleVM.DetalleProductos)
                {
                    var productoDb = _context.Productos.FirstOrDefault(p => p.IdProducto == detalle.IdProducto);
                    if (productoDb == null)
                    {
                        MessageBox.Show($"Producto {detalle.IdProductoNavigation.Nombre} no encontrado.",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        transaction.Rollback();
                        return;
                    }

                    if (productoDb.Cantidad < detalle.Cantidad)
                    {
                        MessageBox.Show($"Stock insuficiente para {productoDb.Nombre}. Disponible: {productoDb.Cantidad}",
                                        "Error de stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                        transaction.Rollback();
                        return;
                    }

                    // Descontar stock real
                    productoDb.Cantidad -= detalle.Cantidad;
                    _context.Productos.Update(productoDb);
                }

                // Crear y guardar la venta
                VentaActual.DniCliente = ClienteVM.ClienteActual.DniCliente;
                VentaActual.FechaHora = DateOnly.FromDateTime(DateTime.Now);
                VentaActual.Total = TotalFinal;
                VentaActual.IdUsuario = Sesion.UsuarioActual.IdUsuario;
                VentaActual.IdFormaPago = FormaPagoVM.FormaPagoSeleccionada.IdFormaPago;
                VentaActual.TotalCuotas = FormaPagoVM.CuotaSeleccionada;
                AsignarEstadoVenta();

                foreach (var d in DetalleVM.DetalleProductos)
                    VentaActual.DetalleVentaProductos.Add(d);

                _context.Venta.Add(VentaActual);
                _context.SaveChanges();

                // Registrar envío si corresponde
                if (!ProcesarEnvio(VentaActual))
                {
                    transaction.Rollback();
                    return;
                }
                
                // Confirmar transacción
                transaction.Commit();

                // Mostrar mensaje de éxito
                MostrarMensajeFinalDeVenta();

                // Generar Factuta
                GeneradorFacturaPDF.Generar(VentaActual);

                // Refrescar vistas y limpiar formularios
                CargarVentas();
                DetalleVM.Reiniciar();
                ClienteVM.Reiniciar();
                EnvioVM.Reiniciar();
                RecalcularTotales();

                // Refrescar bindings visuales
                OnPropertyChanged(nameof(EsCredito));
                OnPropertyChanged(nameof(MostrarMontoPorCuota));
                VentaActual = new Ventum();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Error al confirmar la venta: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void CargarVentas()
        {
            var lista = _context.Venta
                .Include(v => v.DniClienteNavigation)
                .Include(v => v.EstadoVenta)
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVentaProductos)
                .ThenInclude(d => d.IdProductoNavigation)
                .ToList();

            // Guardamos todas las ventas originales
            TodasLasVentas = new ObservableCollection<Ventum>(lista);

            // Aplicamos filtro por defecto
            FiltrarPorEstado("Activa");
        }

        // Filtrado
        private void FiltrarPorEstado(object estado)
        {
            string filtro = estado?.ToString() ?? "Activa";

            var ventasFiltradas = TodasLasVentas
                .Where(v => v.EstadoVenta != null &&
                            v.EstadoVenta.NombreEstado.Equals(filtro, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Ventas = new ObservableCollection<Ventum>(ventasFiltradas);
        }

        // Ordenado
        private void AplicarOrdenamiento(string criterio)
        {
            if (Ventas == null || !Ventas.Any()) return;

            IEnumerable<Ventum> ordenadas = Ventas;

            switch (criterio)
            {
                case "Número de venta (Ascendente)":
                    ordenadas = Ventas.OrderBy(v => v.IdNroVenta);
                    break;

                case "Número de venta (Descendente)":
                    ordenadas = Ventas.OrderByDescending(v => v.IdNroVenta);
                    break;

                case "Total (Mayor a menor)":
                    ordenadas = Ventas.OrderByDescending(v => v.Total);
                    break;

                case "Total (Menor a mayor)":
                    ordenadas = Ventas.OrderBy(v => v.Total);
                    break;

                case "Vendedor (A-Z)":
                    ordenadas = Ventas.OrderBy(v => v.IdUsuarioNavigation.Nombre);
                    break;

                case "Vendedor (Z-A)":
                    ordenadas = Ventas.OrderByDescending(v => v.IdUsuarioNavigation.Nombre);
                    break;

                case "Fecha (Más reciente)":
                    ordenadas = Ventas.OrderByDescending(v => v.FechaHora);
                    break;

                case "Fecha (Más antigua)":
                    ordenadas = Ventas.OrderBy(v => v.FechaHora);
                    break;
            }

            Ventas = new ObservableCollection<Ventum>(ordenadas);
        }

        // Filtrado periodos
        private void FiltrarPorPeriodo()
        {
            if (TodasLasVentas == null || !TodasLasVentas.Any()) return;

            var listaFiltrada = TodasLasVentas.AsEnumerable();

            // Filtrar por estado actual (mantiene el chip)
            if (!string.IsNullOrEmpty(EstadoFiltroSeleccionado))
                listaFiltrada = listaFiltrada
                    .Where(v => v.EstadoVenta != null &&
                                v.EstadoVenta.NombreEstado.Equals(EstadoFiltroSeleccionado, StringComparison.OrdinalIgnoreCase));

            // Filtrar desde
            if (FechaDesde.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaHora >= DateOnly.FromDateTime(FechaDesde.Value));

            // Filtrar hasta
            if (FechaHasta.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaHora <= DateOnly.FromDateTime(FechaHasta.Value));

            // Aplicar resultado
            Ventas = new ObservableCollection<Ventum>(listaFiltrada);
        }

        // Limpiar los filtros
        private void LimpiarFiltros()
        {
            FechaDesde = null;
            FechaHasta = null;
            EstadoFiltroSeleccionado = "Activa";
            FiltrarPorEstado("Activa");
        }

        // Buscador 
        private void FiltrarPorTexto()
        {
            if (TodasLasVentas == null || !TodasLasVentas.Any()) return;

            var listaFiltrada = TodasLasVentas.AsEnumerable();

            // Mantener filtro de estado
            if (!string.IsNullOrEmpty(EstadoFiltroSeleccionado))
                listaFiltrada = listaFiltrada
                    .Where(v => v.EstadoVenta != null &&
                                v.EstadoVenta.NombreEstado.Equals(EstadoFiltroSeleccionado, StringComparison.OrdinalIgnoreCase));

            // Mantener filtro de fechas
            if (FechaDesde.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaHora >= DateOnly.FromDateTime(FechaDesde.Value));

            if (FechaHasta.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaHora <= DateOnly.FromDateTime(FechaHasta.Value));

            // Aplicar búsqueda por texto
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string texto = TextoBusqueda.ToLower();

                listaFiltrada = listaFiltrada.Where(v =>
                    v.IdNroVenta.ToString().Contains(texto) ||
                    (v.DniClienteNavigation?.NombreCompleto?.ToLower().Contains(texto) ?? false) ||
                    (v.IdUsuarioNavigation?.Nombre?.ToLower().Contains(texto) ?? false) ||
                    (v.EstadoVenta?.NombreEstado?.ToLower().Contains(texto) ?? false)
                );
            }

            // Aplicar resultado
            Ventas = new ObservableCollection<Ventum>(listaFiltrada);
        }
        //calcular nro de venta
        private int _proximoIdVenta;
        public int ProximoIdVenta
        {
            get => _proximoIdVenta;
            set
            {
                _proximoIdVenta = value;
                OnPropertyChanged(nameof(ProximoIdVenta));
            }
        }

        // Método de cálculo general
        private void RecalcularTotales()
        {
            double sub = Subtotal;

            //recalcular recargo con credito
            Recargo = EsCredito ? Math.Round(sub * 0.10, 2) : 0;

            // Total parcial (productos + recargo)
            double total = sub + Recargo;

            // Si hay envío habilitado, se suma al total
            if (EnvioVM != null && EnvioVM.EnvioHabilitado)
                total += EnvioVM.Costo;

            //total final redondeado
            TotalFinal = Math.Round(total, 2);
        }

        private void AsignarEstadoVenta()
        {
            bool tieneEnvio = EnvioVM.EnvioHabilitado;
            string medioPago = FormaPagoVM.FormaPagoSeleccionada?.Nombre ?? "";

            // Si no tiene envío
            if (!tieneEnvio)
            {
                switch (medioPago)
                {
                    case "Efectivo":
                    case "Débito":
                    case "Transferencia":
                        VentaActual.EstadoVentaId = ListaEstadosVenta
                            .FirstOrDefault(e => e.NombreEstado == "Completada")?.IdEstadoVenta ?? 1;
                        break;

                    case "Crédito":
                        VentaActual.EstadoVentaId = ListaEstadosVenta
                            .FirstOrDefault(e => e.NombreEstado == "Pendiente de pago")?.IdEstadoVenta ?? 1;
                        break;

                    default:
                        VentaActual.EstadoVentaId = ListaEstadosVenta
                            .FirstOrDefault(e => e.NombreEstado == "Activa")?.IdEstadoVenta ?? 1;
                        break;
                }
            }
            else // Tiene envío
            {
                VentaActual.EstadoVentaId = ListaEstadosVenta
                    .FirstOrDefault(e => e.NombreEstado == "Activa")?.IdEstadoVenta ?? 1;
            }

        }

        private bool ProcesarEnvio(Ventum venta)
        {
            try
            {
                // Si el envío no está habilitado, no hacemos nada
                if (!EnvioVM.EnvioHabilitado)
                    return true;

                // Si el usuario escribió una nueva dirección
                if (EnvioVM.NuevaDireccionHabilitada && EnvioVM.DireccionActual != null)
                {
                    var dir = EnvioVM.DireccionActual;

                    if (string.IsNullOrWhiteSpace(dir.NombreCalle))
                    {
                        MessageBox.Show("Debe completar los datos de la nueva dirección.", "Aviso",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    dir.DniCliente = ClienteVM.ClienteActual.DniCliente;

                    if (EnvioVM.CiudadSeleccionada != null)
                        dir.IdCiudad = EnvioVM.CiudadSeleccionada.IdCiudad; if (EnvioVM.CiudadSeleccionada != null)
                        dir.IdCiudad = EnvioVM.CiudadSeleccionada.IdCiudad;

                    _context.Direccions.Add(dir);
                    _context.SaveChanges();

                    // Agregarla a la lista local y seleccionarla
                    EnvioVM.DireccionesCliente.Add(dir);
                    EnvioVM.DireccionSeleccionada = dir;
                }

                // Registrar el envío con la dirección seleccionada o la nueva
                EnvioVM.RegistrarEnvio(venta.IdNroVenta, TransporteVM.TransporteSeleccionado);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar el envío: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void MostrarMensajeFinalDeVenta()
        {
            string mensajeFinal;

            // Caso 1️ - Venta sin envío
            if (!EnvioVM.EnvioHabilitado)
            {
                mensajeFinal = $"Venta registrada correctamente.\nN° {VentaActual.IdNroVenta}";
            }
            else
            {
                // Caso 2️ — Venta con envío
                mensajeFinal = $"Venta y envío registrados correctamente.\nN° {VentaActual.IdNroVenta}";

                // Caso 3️ — Si además se agregó una nueva dirección
                if (EnvioVM.NuevaDireccionHabilitada && EnvioVM.DireccionActual != null)
                {
                    var cliente = ClienteVM.ClienteActual;
                    mensajeFinal +=
                        $"\n Nueva dirección asignada al cliente {cliente.Nombre} {cliente.Apellido}.";
                }
            }

            MessageBox.Show(mensajeFinal, "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void CancelarVenta(Ventum ventaSeleccionada)
        {
            // if opcional, por seguridad
            if (ventaSeleccionada is null) return;

            var confirmacion = MessageBox.Show("¿Está seguro que desea cancelar esta venta?",
                                               "Confirmar cancelación",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            string motivo = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese el motivo de la cancelación:",
                "Motivo de cancelación",
                "Cancelación por solicitud del cliente"
            );

            try
            {
                using var transaction = _context.Database.BeginTransaction();

                var estadoCancelado = _context.EstadoVenta
                    .FirstOrDefault(e => e.NombreEstado == "Cancelada");

                if (estadoCancelado == null)
                {
                    MessageBox.Show("No se encontró el estado 'Cancelada' en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ventaSeleccionada.EstadoVentaId = estadoCancelado.IdEstadoVenta;
                _context.SaveChanges();

                //  usamos el view model de nota de credito
                var notaVM = new NotaCreditoViewModel(_context);
                var nota = notaVM.CrearNotaCredito(ventaSeleccionada, motivo);
                notaVM.AgregarDetallesYActualizarStock(nota, ventaSeleccionada);

                transaction.Commit();

                MessageBox.Show("Venta cancelada y nota de crédito generada correctamente.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                GeneradorNotaCreditoPDF.Generar(nota, _context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar la venta: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerVentaSoloLectura(Ventum venta)
        {
            if (venta == null)
            {
                MessageBox.Show("No se pudo abrir la venta seleccionada.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventaCompleta = _context.Venta
                .Include(v => v.DniClienteNavigation)
                    .ThenInclude(c => c.Direcciones)
                .Include(v => v.DetalleVentaProductos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(v => v.Envios)
                    .ThenInclude(e => e.IdDireccionNavigation)
                .Include(v => v.Envios)
                    .ThenInclude(e => e.IdTransporteNavigation)
                .Include(v => v.IdFormaPagoNavigation)
                .Include(v => v.EstadoVenta)
                .FirstOrDefault(v => v.IdNroVenta == venta.IdNroVenta);

            if (ventaCompleta == null)
            {
                MessageBox.Show("No se encontraron los datos completos de la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var ventana = new Venta_form();
            var vm = new VentasViewModel();

            // === CLIENTE ===
            vm.ClienteVM.ClienteActual = ventaCompleta.DniClienteNavigation;
            vm.ClienteVM.DniClienteInput = ventaCompleta.DniClienteNavigation?.DniCliente.ToString();

            // Importante: que EnvioVM conozca todas las direcciones del cliente
            vm.EnvioVM.DireccionesCliente = new ObservableCollection<Direccion>(
                ventaCompleta.DniClienteNavigation.Direcciones
            );

            // === DETALLE ===
            vm.DetalleVM.DetalleProductos = new ObservableCollection<DetalleVentaProducto>(
                ventaCompleta.DetalleVentaProductos
            );

            // === ENVÍO ===
            var envio = ventaCompleta.Envios.FirstOrDefault();
            vm.EnvioVM.EnvioHabilitado = envio != null;

            if (envio != null)
            {
                // Dirección seleccionada
                vm.EnvioVM.DireccionSeleccionada = envio.IdDireccionNavigation;
                vm.EnvioVM.Costo = envio.Costo;

                // Provincias/ciudades seleccionadas (aseguramos coincidencia por ID con las listas)
                // 1) Provincia
                var ciudadDir = _context.Ciudads
                    .Include(c => c.IdProvinciaNavigation)
                    .FirstOrDefault(c => c.IdCiudad == envio.IdDireccionNavigation.IdCiudad);

                vm.EnvioVM.ProvinciaSeleccionada =
                    vm.EnvioVM.Provincias.FirstOrDefault(p => p.IdProvincia == ciudadDir.IdProvincia);

                // 2) Ciudades de esa provincia
                vm.EnvioVM.Ciudades = new ObservableCollection<Ciudad>(
                    _context.Ciudads.Where(c => c.IdProvincia == ciudadDir.IdProvincia).ToList()
                );

                vm.EnvioVM.CiudadSeleccionada =
                    vm.EnvioVM.Ciudades.FirstOrDefault(c => c.IdCiudad == ciudadDir.IdCiudad);

                // Transporte
                if (vm.TransporteVM.Transportes == null || vm.TransporteVM.Transportes.Count == 0)
                    vm.TransporteVM.Transportes = new ObservableCollection<Transporte>(_context.Transportes.ToList());

                vm.TransporteVM.TransporteSeleccionado =
                    vm.TransporteVM.Transportes.FirstOrDefault(t => t.IdTransporte == envio.IdTransporte);
            }

            // === FORMA DE PAGO ===
            if (vm.FormaPagoVM.FormasPago == null || vm.FormaPagoVM.FormasPago.Count == 0)
                vm.FormaPagoVM.FormasPago = new ObservableCollection<FormaPago>(_context.FormaPago.ToList());

            // ¡OJO!: el SelectedItem debe existir dentro de FormasPago (mismo Id)
            vm.FormaPagoVM.FormaPagoSeleccionada =
                vm.FormaPagoVM.FormasPago.FirstOrDefault(fp => fp.IdFormaPago == ventaCompleta.IdFormaPago);

            vm.FormaPagoVM.CuotaSeleccionada = ventaCompleta.TotalCuotas ?? 1;

            // === ESTADO Y TOTALES ===
            vm.VentaActual = ventaCompleta;
            vm.EstadoVentaSeleccionado = ventaCompleta.EstadoVenta;

            // forzar totales (ver método 2)
            vm.DetalleVM.RecalcularTotalDesdeColeccion();
            vm.RecalcularTotales();

            // === SOLO LECTURA ===
            vm.DetalleVM.ModoSoloLectura = true;
            vm.EnvioVM.ModoSoloLectura = true;
            vm.ClienteVM.ModoSoloLectura = true;
            vm.TransporteVM.ModoSoloLectura = true;
            vm.FormaPagoVM.ModoSoloLectura = true;

            ventana.DataContext = vm;
            ventana.ShowDialog();
        }


        // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
}
