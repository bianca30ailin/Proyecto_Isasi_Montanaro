using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Isasi_Montanaro.ViewModels.Informes
{
    public class InformeClientesViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public InformeClientesViewModel(Action volverAccion)
        {
            _context = new ProyectoTallerContext();
            VolverCommand = new RelayCommand(_ => volverAccion());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());
            LimpiarFiltrosCommand =new RelayCommand (_ => LimpiarFiltros());


            FechaDesde = DateTime.Now.AddMonths(-6);
            FechaHasta = DateTime.Now;
            CriterioOrdenSeleccionado = "Nombre A-Z";

            GenerarReporte();
        }

        // --- FILTROS ---
        private DateTime _fechaDesde;
        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set { _fechaDesde = value; OnPropertyChanged(); }
        }

        private DateTime _fechaHasta;
        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set { _fechaHasta = value; OnPropertyChanged(); }
        }

        public List<string> CriteriosOrden { get; } = new()
        {
            "Nombre A-Z", "Nombre Z-A", "Cantidad Ventas ↑", "Cantidad Ventas ↓", "Estado"
        };

        private string _criterioOrdenSeleccionado;
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set
            {
                _criterioOrdenSeleccionado = value;
                OnPropertyChanged();
               
            }
        }

        // --- DATOS PRINCIPALES ---
        private ObservableCollection<ClienteReporteDTO> _clientes;
        public ObservableCollection<ClienteReporteDTO> Clientes
        {
            get => _clientes;
            set { _clientes = value; OnPropertyChanged(); }
        }

        public int CantidadActivos { get; set; }
        public int CantidadInactivos { get; set; }
        public int TotalClientes { get; set; }
        public double PromedioVentasPorCliente { get; set; }


        // --- GRÁFICOS ---
        public ObservableCollection<TopClienteDTO> TopClientes { get; set; }
        public ObservableCollection<NuevosClientesMesDTO> NuevosClientesPorMes { get; set; }

        // --- COMANDOS ---
        public ICommand VolverCommand { get; }
        public ICommand GenerarReporteCommand { get; }
        public ICommand ExportarPdfCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }

        // --- MÉTODO PRINCIPAL ---
        private void GenerarReporte()
        {
            var desde = DateOnly.FromDateTime(FechaDesde);
            var hasta = DateOnly.FromDateTime(FechaHasta);

            var clientes = _context.Clientes
                .Include(c => c.Venta)
                .ThenInclude(v => v.EstadoVenta)
                .ToList();

            var clientesFiltrados = clientes
                .Where(c => c.Venta.Any(v => v.FechaHora >= desde && v.FechaHora <= hasta) || c.FechaAlta == null)
                .ToList();

            // --- Estadísticas principales ---
            CantidadActivos = clientesFiltrados.Count(c => c.Activo);
            CantidadInactivos = clientesFiltrados.Count(c => !c.Activo);
            TotalClientes = clientesFiltrados.Count;
            PromedioVentasPorCliente = clientesFiltrados.Any()
                ? clientesFiltrados.Average(c => c.CantidadVentas)
                : 0;

            // --- Tabla de clientes ---
            var lista = clientesFiltrados.Select(c => new ClienteReporteDTO
            {
                NombreCompleto = $"{c.Nombre} {c.Apellido}",
                Email = c.Email,
                Telefono = c.Telefono,
                Estado = c.Activo ? "Activo" : "Inactivo",
                CantidadVentas = c.CantidadVentas,
                FechaAlta = c.FechaAlta?.ToDateTime(TimeOnly.MinValue)
            });

            lista = CriterioOrdenSeleccionado switch
            {
                "Nombre A-Z" => lista.OrderBy(c => c.NombreCompleto),
                "Nombre Z-A" => lista.OrderByDescending(c => c.NombreCompleto),
                "Cantidad Ventas ↑" => lista.OrderBy(c => c.CantidadVentas),
                "Cantidad Ventas ↓" => lista.OrderByDescending(c => c.CantidadVentas),
                "Estado" => lista.OrderByDescending(c => c.Estado),
                _ => lista
            };

            Clientes = new ObservableCollection<ClienteReporteDTO>(lista);

            // --- TOP 5 CLIENTES ---
            var topClientes = clientesFiltrados
                .OrderByDescending(c => c.Venta.Sum(v => v.Total))
                .Take(5)
                .Select(c => new TopClienteDTO
                {
                    Nombre = $"{c.Nombre} {c.Apellido}",
                    TotalComprado = c.Venta.Sum(v => v.Total)
                })
                .ToList();

            TopClientes = new ObservableCollection<TopClienteDTO>(topClientes);

            // --- NUEVOS CLIENTES POR MES ---
            var nuevosPorMes = clientesFiltrados
                .Where(c => c.FechaAlta != null)
                .GroupBy(c => c.FechaAlta.Value.Month)
                .Select(g => new
                {
                    Mes = new DateTime(2024, g.Key, 1).ToString("MMMM"),
                    Cantidad = g.Count()
                })
                .OrderBy(x => DateTime.ParseExact(x.Mes, "MMMM", System.Globalization.CultureInfo.CurrentCulture))
                .ToList();

            double maxMes = nuevosPorMes.Any() ? nuevosPorMes.Max(m => m.Cantidad) : 1;

            NuevosClientesPorMes = new ObservableCollection<NuevosClientesMesDTO>(
                nuevosPorMes.Select(m => new NuevosClientesMesDTO
                {
                    Mes = char.ToUpper(m.Mes[0]) + m.Mes.Substring(1),
                    Cantidad = m.Cantidad,
                    AnchoCalculado = (m.Cantidad / maxMes) * 250
                })
            );

            OnPropertyChanged(nameof(CantidadActivos));
            OnPropertyChanged(nameof(CantidadInactivos));
            OnPropertyChanged(nameof(TotalClientes));
            OnPropertyChanged(nameof(PromedioVentasPorCliente));
        }

        private void LimpiarFiltros()
        {
            FechaDesde = DateTime.Now.AddMonths(-6);
            FechaHasta = DateTime.Now;
            CriterioOrdenSeleccionado = "Nombre A-Z";
            GenerarReporte();
        }

        // --- PDF ---
        private void ExportarPdf()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_Clientes_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Guardar Informe de Clientes"
                };

                if (dialog.ShowDialog() == true)
                {
                    GenerarPdfInforme(dialog.FileName);
                    System.Windows.MessageBox.Show("Informe exportado correctamente", "Éxito",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void GenerarPdfInforme(string rutaArchivo)
        {
            // --- Colores ---
            var azul = new BaseColor(16, 78, 139); // #104E8B
            var gris = new BaseColor(90, 90, 90);
            var grisFondo = new BaseColor(245, 245, 245);
            var mostaza = new BaseColor(245, 158, 11);

            var doc = new Document(PageSize.A4, 45, 45, 80, 60);
            var writer = PdfWriter.GetInstance(doc, new FileStream(rutaArchivo, FileMode.Create));
            doc.Open();

            // --- ENCABEZADO ---
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
            if (File.Exists(logoPath))
            {
                var logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80, 80);
                logo.Alignment = Image.ALIGN_LEFT;
                doc.Add(logo);
            }

            var empresa = new Paragraph("SISIE - Sistema Integral de Gestión de Clientes",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, azul))
            { Alignment = Element.ALIGN_RIGHT, SpacingAfter = 5 };
            doc.Add(empresa);

            doc.Add(new Chunk(new LineSeparator(1f, 100f, mostaza, Element.ALIGN_CENTER, -2)));
            doc.Add(new Paragraph("\n"));

            var titulo = new Paragraph("INFORME DE CLIENTES",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, azul))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10 };
            doc.Add(titulo);

            var periodo = new Paragraph($"Período: {FechaDesde:dd/MM/yyyy} - {FechaHasta:dd/MM/yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11, gris))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 25 };
            doc.Add(periodo);

            // --- RESUMEN ---
            var tablaResumen = new PdfPTable(4)
            {
                WidthPercentage = 100,
                SpacingAfter = 20
            };
            tablaResumen.SetWidths(new float[] { 25, 25, 25, 25 });

            string[] headers = { "Clientes Activos", "Inactivos", "Total Clientes", "Prom. Ventas x Cliente" };
            foreach (var h in headers)
            {
                var cell = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)))
                {
                    BackgroundColor = azul,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8
                };
                tablaResumen.AddCell(cell);
            }

            tablaResumen.AddCell(new Phrase(CantidadActivos.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, gris)));
            tablaResumen.AddCell(new Phrase(CantidadInactivos.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, gris)));
            tablaResumen.AddCell(new Phrase(TotalClientes.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, gris)));
            tablaResumen.AddCell(new Phrase($"{PromedioVentasPorCliente:F2}", FontFactory.GetFont(FontFactory.HELVETICA, 10, gris)));
            doc.Add(tablaResumen);

            // --- TOP CLIENTES ---
            if (TopClientes?.Any() == true)
            {
                var subtitulo = new Paragraph("Top 5 Clientes con Más Compras",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, mostaza))
                { SpacingBefore = 10, SpacingAfter = 10 };
                doc.Add(subtitulo);

                var tablaTop = new PdfPTable(2) { WidthPercentage = 60 };
                tablaTop.SetWidths(new float[] { 60, 40 });

                foreach (var c in TopClientes)
                {
                    tablaTop.AddCell(new PdfPCell(new Phrase(c.Nombre, FontFactory.GetFont(FontFactory.HELVETICA, 10, gris))) { Padding = 5 });
                    tablaTop.AddCell(new PdfPCell(new Phrase($"${c.TotalComprado:N2}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, azul)))
                    { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                }
                doc.Add(tablaTop);
            }

            // --- DETALLE DE CLIENTES ---
            var subtitulo2 = new Paragraph("Detalle de Clientes",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, mostaza))
            { SpacingBefore = 20, SpacingAfter = 10 };
            doc.Add(subtitulo2);

            var tabla = new PdfPTable(5) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 25, 20, 20, 15, 20 });

            string[] cols = { "Nombre", "Email", "Teléfono", "Estado", "Cant. Ventas" };
            foreach (var col in cols)
            {
                var cell = new PdfPCell(new Phrase(col, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)))
                {
                    BackgroundColor = azul,
                    Padding = 6,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                tabla.AddCell(cell);
            }

            foreach (var c in Clientes)
            {
                tabla.AddCell(new Phrase(c.NombreCompleto, FontFactory.GetFont(FontFactory.HELVETICA, 9, gris)));
                tabla.AddCell(new Phrase(c.Email, FontFactory.GetFont(FontFactory.HELVETICA, 9, gris)));
                tabla.AddCell(new Phrase(c.Telefono, FontFactory.GetFont(FontFactory.HELVETICA, 9, gris)));
                tabla.AddCell(new Phrase(c.Estado, FontFactory.GetFont(FontFactory.HELVETICA, 9, gris)));
                tabla.AddCell(new Phrase(c.CantidadVentas.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9, gris)));
            }
            doc.Add(tabla);

            doc.Add(new Paragraph("\n"));
            doc.Add(new Chunk(new LineSeparator(1f, 100f, mostaza, Element.ALIGN_CENTER, -2)));

            var footer = new Paragraph($"Informe generado automáticamente por SISIE | {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY))
            { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 5 };
            doc.Add(footer);

            doc.Close();
            writer.Close();
        }

        // --- DTOs ---
        public class ClienteReporteDTO
        {
            public string NombreCompleto { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
            public string Estado { get; set; }
            public int CantidadVentas { get; set; }
            public DateTime? FechaAlta { get; set; }
        }

        public class TopClienteDTO
        {
            public string Nombre { get; set; }
            public double TotalComprado { get; set; }
        }

        public class NuevosClientesMesDTO
        {
            public string Mes { get; set; }
            public int Cantidad { get; set; }
            public double AnchoCalculado { get; set; }
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
