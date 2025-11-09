using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels.Informes
{
    public class InformeVentasViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public InformeVentasViewModel(Action volverAccion)
        {
            _context = new ProyectoTallerContext();
            VolverCommand = new RelayCommand(_ => volverAccion());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());

            CriterioOrdenSeleccionado = "Fecha ↓";

            GenerarReporte();
        }

        // --- PROPIEDADES PARA FILTROS ---
        private DateTime _fechaDesde = DateTime.Now.AddDays(-30);
        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set { _fechaDesde = value; OnPropertyChanged(); }
        }

        private DateTime _fechaHasta = DateTime.Now;
        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set { _fechaHasta = value; OnPropertyChanged(); }
        }

        public List<string> CriteriosOrden { get; } = new() { "Fecha ↑", "Fecha ↓", "Total ↑", "Total ↓", "Vendedor" };

        // --- DATOS MOSTRADOS ---
        private ObservableCollection<VentaReporteDTO> _ventas;
        public ObservableCollection<VentaReporteDTO> Ventas
        {
            get => _ventas;
            set { _ventas = value; OnPropertyChanged(); }
        }

        private double _totalPeriodo;
        public double TotalPeriodo
        {
            get => _totalPeriodo;
            set { _totalPeriodo = value; OnPropertyChanged(); }
        }

        private int _cantidadVentas;
        public int CantidadVentas
        {
            get => _cantidadVentas;
            set { _cantidadVentas = value; OnPropertyChanged(); }
        }

        private int _cantidadDevoluciones;
        public int CantidadDevoluciones
        {
            get => _cantidadDevoluciones;
            set { _cantidadDevoluciones = value; OnPropertyChanged(); }
        }

        private double _porcentajeDevoluciones;
        public double PorcentajeDevoluciones
        {
            get => _porcentajeDevoluciones;
            set { _porcentajeDevoluciones = value; OnPropertyChanged(); }
        }

        private string _criterioOrdenSeleccionado;
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set
            {
                _criterioOrdenSeleccionado = value;
                OnPropertyChanged();
                GenerarReporte();
            }
        }
        public ICommand ExportarPdfCommand { get; }

        // --- PROPIEDADES PARA GRÁFICOS ---
        private ObservableCollection<EstadoVentaGraficoDTO> _ventasPorEstado;
        public ObservableCollection<EstadoVentaGraficoDTO> VentasPorEstado
        {
            get => _ventasPorEstado;
            set { _ventasPorEstado = value; OnPropertyChanged(); }
        }

        private ObservableCollection<VendedorGraficoDTO> _topVendedores;
        public ObservableCollection<VendedorGraficoDTO> TopVendedores
        {
            get => _topVendedores;
            set { _topVendedores = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ComparativaEnvioDTO> _comparativaEnvios;
        public ObservableCollection<ComparativaEnvioDTO> ComparativaEnvios
        {
            get => _comparativaEnvios;
            set { _comparativaEnvios = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DevolucionGraficoDTO> _devoluciones;
        public ObservableCollection<DevolucionGraficoDTO> Devoluciones
        {
            get => _devoluciones;
            set { _devoluciones = value; OnPropertyChanged(); }
        }

        // --- COMANDOS ---
        public ICommand GenerarReporteCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }

        // --- MÉTODO PRINCIPAL DE CONSULTA ---
        private void GenerarReporte()
        {
            var desde = DateOnly.FromDateTime(FechaDesde);
            var hasta = DateOnly.FromDateTime(FechaHasta);

            // Trae todas las ventas con sus relaciones
            var todasLasVentas = _context.Venta
                .Include(v => v.DniClienteNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.EstadoVenta)
                .Include(v => v.Envios)
                .ToList();

            // Filtro por rango de fechas
            var ventasFiltradas = todasLasVentas
                .Where(v => v.FechaHora >= desde && v.FechaHora <= hasta)
                .ToList();

            var query = ventasFiltradas
                .Select(v => new VentaReporteDTO
                {
                    Fecha = v.FechaHora.ToDateTime(TimeOnly.MinValue),
                    Cliente = v.DniClienteNavigation.Nombre + " " + v.DniClienteNavigation.Apellido,
                    Vendedor = v.IdUsuarioNavigation != null
                        ? v.IdUsuarioNavigation.Nombre + " " + v.IdUsuarioNavigation.Apellido
                        : "Sin asignar",
                    Total = v.Total,
                    Estado = v.EstadoVenta?.NombreEstado ?? "Sin estado"
                })
                .ToList();

            // Aplicar orden dinámico
            var listaOrdenada = CriterioOrdenSeleccionado switch
            {
                "Fecha ↑" => query.OrderBy(v => v.Fecha).ToList(),
                "Fecha ↓" => query.OrderByDescending(v => v.Fecha).ToList(),
                "Total ↑" => query.OrderBy(v => v.Total).ToList(),
                "Total ↓" => query.OrderByDescending(v => v.Total).ToList(),
                "Vendedor" => query.OrderBy(v => v.Vendedor).ToList(),
                _ => query.OrderByDescending(v => v.Fecha).ToList()
            };

            Ventas = new ObservableCollection<VentaReporteDTO>(listaOrdenada);

            // Cálculos resumidos
            TotalPeriodo = query.Sum(v => v.Total);
            CantidadVentas = query.Count;

            // Devoluciones (NotaCredito)
            var notasCredito = _context.Set<NotaCredito>()
                .Include(nc => nc.Venta)
                .Where(nc => nc.Venta.FechaHora >= desde && nc.Venta.FechaHora <= hasta)
                .ToList();

            CantidadDevoluciones = notasCredito.Count;
            PorcentajeDevoluciones = CantidadVentas > 0 ? (CantidadDevoluciones * 100.0 / CantidadVentas) : 0;

            // --- GRÁFICO 1: VENTAS POR ESTADO ---
            var ventasPorEstado = ventasFiltradas
                .GroupBy(v => v.EstadoVenta?.NombreEstado ?? "Sin estado")
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(g => g.Cantidad)
                .ToList();

            double maxEstado = ventasPorEstado.Any() ? ventasPorEstado.Max(e => e.Cantidad) : 1;

            var colores = new[] { "#E35802", "#072741", "#FF8C00", "#234069", "#FFBE78" };

            VentasPorEstado = new ObservableCollection<EstadoVentaGraficoDTO>(
                ventasPorEstado.Select((e, index) => new EstadoVentaGraficoDTO
                {
                    Estado = e.Estado,
                    Cantidad = e.Cantidad,
                    AnchoCalculado = (e.Cantidad / maxEstado) * 250,
                    Color = colores[index % colores.Length]
                })
            );

            // --- GRÁFICO 2: TOP VENDEDORES ---
            var topVendedores = ventasFiltradas
                .Where(v => v.IdUsuarioNavigation != null)
                .GroupBy(v => v.IdUsuarioNavigation)
                .Select(g => new
                {
                    Vendedor = g.Key.Nombre + " " + g.Key.Apellido,
                    CantidadVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .OrderByDescending(v => v.CantidadVentas)
                .Take(5)
                .ToList();

            double maxVentas = topVendedores.Any() ? topVendedores.Max(v => v.CantidadVentas) : 1;

            TopVendedores = new ObservableCollection<VendedorGraficoDTO>(
                topVendedores.Select(v => new VendedorGraficoDTO
                {
                    Vendedor = v.Vendedor,
                    CantidadVentas = v.CantidadVentas,
                    TotalVendido = v.TotalVendido,
                    AlturaCalculada = (v.CantidadVentas / maxVentas) * 180
                })
            );

            // --- GRÁFICO 3: COMPARATIVA ENVÍOS ---
            var ventasConEnvio = ventasFiltradas.Count(v => v.Envios.Any());
            var ventasSinEnvio = ventasFiltradas.Count - ventasConEnvio;

            double totalComparativa = ventasConEnvio + ventasSinEnvio;
            double maxComparativa = Math.Max(ventasConEnvio, ventasSinEnvio);

            ComparativaEnvios = new ObservableCollection<ComparativaEnvioDTO>
            {
                new ComparativaEnvioDTO
                {
                    Tipo = "Con Envío",
                    Cantidad = ventasConEnvio,
                    Porcentaje = totalComparativa > 0 ? (ventasConEnvio * 100.0 / totalComparativa) : 0,
                    AlturaCalculada = maxComparativa > 0 ? (ventasConEnvio / maxComparativa) * 180 : 0,
                    Color = "#E35802"
                },
                new ComparativaEnvioDTO
                {
                    Tipo = "Sin Envío",
                    Cantidad = ventasSinEnvio,
                    Porcentaje = totalComparativa > 0 ? (ventasSinEnvio * 100.0 / totalComparativa) : 0,
                    AlturaCalculada = maxComparativa > 0 ? (ventasSinEnvio / maxComparativa) * 180 : 0,
                    Color = "#072741"
                }
            };

            // --- GRÁFICO 4: DEVOLUCIONES ---
            Devoluciones = new ObservableCollection<DevolucionGraficoDTO>
            {
                new DevolucionGraficoDTO
                {
                    Tipo = "Ventas Exitosas",
                    Cantidad = CantidadVentas - CantidadDevoluciones,
                    Porcentaje = 100 - PorcentajeDevoluciones,
                    Color = "#072741"
                },
                new DevolucionGraficoDTO
                {
                    Tipo = "Devoluciones",
                    Cantidad = CantidadDevoluciones,
                    Porcentaje = PorcentajeDevoluciones,
                    Color = "#E35802"
                }
            };
        }

        private void LimpiarFiltros()
        {
            FechaDesde = DateTime.Now.AddDays(-30);
            FechaHasta = DateTime.Now;
            CriterioOrdenSeleccionado = "Fecha ↓";

            GenerarReporte();
        }

        // --- DTOs ---
        public class VentaReporteDTO
        {
            public DateTime Fecha { get; set; }
            public string Cliente { get; set; }
            public string Vendedor { get; set; }
            public double Total { get; set; }
            public string Estado { get; set; }
        }

        public class EstadoVentaGraficoDTO
        {
            public string Estado { get; set; }
            public int Cantidad { get; set; }
            public double AnchoCalculado { get; set; }
            public string Color { get; set; }
        }

        public class VendedorGraficoDTO
        {
            public string Vendedor { get; set; }
            public int CantidadVentas { get; set; }
            public double TotalVendido { get; set; }
            public double AlturaCalculada { get; set; }
        }

        public class ComparativaEnvioDTO
        {
            public string Tipo { get; set; }
            public int Cantidad { get; set; }
            public double Porcentaje { get; set; }
            public double AlturaCalculada { get; set; }
            public string Color { get; set; }
        }

        public class DevolucionGraficoDTO
        {
            public string Tipo { get; set; }
            public int Cantidad { get; set; }
            public double Porcentaje { get; set; }
            public string Color { get; set; }
        }

        private void ExportarPdf()
        {
            try
            {
                // Crear diálogo para guardar archivo
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Guardar Informe de Ventas"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerarPdfInforme(saveFileDialog.FileName);
                    System.Windows.MessageBox.Show(
                        "Informe exportado correctamente",
                        "Éxito",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error al exportar PDF: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void GenerarPdfInforme(string rutaArchivo)
        {
            // Colores SISIE
            var azulSisie = new BaseColor(7, 39, 65);      // #072741
            var naranjaSisie = new BaseColor(227, 88, 2);  // #E35802
            var grisClaro = new BaseColor(245, 245, 245);
            var grisTexto = new BaseColor(80, 80, 80);

            // Crear documento
            Document document = new Document(PageSize.A4, 45, 45, 80, 60);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));
            document.Open();

            // --- ENCABEZADO ---
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
            if (File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);
                logo.Alignment = Image.ALIGN_LEFT;
                logo.SpacingAfter = 10f;
                document.Add(logo);
            }

            Paragraph empresa = new Paragraph("SISIE - Sistema Integral de Gestión de Ventas",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, azulSisie));
            empresa.Alignment = Element.ALIGN_RIGHT;
            empresa.SpacingAfter = 5;
            document.Add(empresa);

            LineSeparator separator = new LineSeparator(1f, 100f, naranjaSisie, Element.ALIGN_CENTER, -2);
            document.Add(new Chunk(separator));

            document.Add(new Paragraph("\n"));

            // --- TÍTULO PRINCIPAL ---
            Paragraph titulo = new Paragraph("INFORME DE VENTAS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, azulSisie));
            titulo.Alignment = Element.ALIGN_CENTER;
            titulo.SpacingAfter = 5;
            document.Add(titulo);

            Paragraph periodo = new Paragraph($"Período: {FechaDesde:dd/MM/yyyy} - {FechaHasta:dd/MM/yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11, grisTexto));
            periodo.Alignment = Element.ALIGN_CENTER;
            periodo.SpacingAfter = 25;
            document.Add(periodo);

            // --- RESUMEN GENERAL ---
            PdfPTable tablaResumen = new PdfPTable(4);
            tablaResumen.WidthPercentage = 100;
            tablaResumen.SpacingAfter = 20;
            tablaResumen.SetWidths(new float[] { 25, 25, 25, 25 });

            string[] headersResumen = { "Total Vendido", "Cantidad de Ventas", "Devoluciones", "% Devoluciones" };
            foreach (string h in headersResumen)
            {
                PdfPCell cell = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)));
                cell.BackgroundColor = azulSisie;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Padding = 8;
                tablaResumen.AddCell(cell);
            }

            tablaResumen.AddCell(new PdfPCell(new Phrase($"${TotalPeriodo:N2}", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });
            tablaResumen.AddCell(new PdfPCell(new Phrase(CantidadVentas.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });
            tablaResumen.AddCell(new PdfPCell(new Phrase(CantidadDevoluciones.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });
            tablaResumen.AddCell(new PdfPCell(new Phrase($"{PorcentajeDevoluciones:F1}%", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 });

            document.Add(tablaResumen);

            // --- SECCIONES ANALÍTICAS ---
            void AgregarSubtitulo(string texto)
            {
                Paragraph subtitulo = new Paragraph(texto, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, naranjaSisie));
                subtitulo.SpacingBefore = 15;
                subtitulo.SpacingAfter = 10;
                document.Add(subtitulo);
            }

            // Ventas por Estado
            if (VentasPorEstado?.Any() == true)
            {
                AgregarSubtitulo("Ventas por Estado");

                PdfPTable tablaEstados = new PdfPTable(2);
                tablaEstados.WidthPercentage = 60;
                tablaEstados.SetWidths(new float[] { 70, 30 });

                foreach (var e in VentasPorEstado)
                {
                    tablaEstados.AddCell(new PdfPCell(new Phrase(e.Estado, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5 });
                    tablaEstados.AddCell(new PdfPCell(new Phrase(e.Cantidad.ToString(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, azulSisie)))
                    { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                document.Add(tablaEstados);
            }

            // Top Vendedores
            if (TopVendedores?.Any() == true)
            {
                AgregarSubtitulo("Top 5 Vendedores");

                PdfPTable tablaVendedores = new PdfPTable(3);
                tablaVendedores.WidthPercentage = 80;
                tablaVendedores.SetWidths(new float[] { 50, 25, 25 });

                string[] headersVend = { "Vendedor", "Cantidad", "Total Vendido" };
                foreach (string h in headersVend)
                {
                    tablaVendedores.AddCell(new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)))
                    { BackgroundColor = azulSisie, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                foreach (var v in TopVendedores)
                {
                    tablaVendedores.AddCell(new PdfPCell(new Phrase(v.Vendedor, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5 });
                    tablaVendedores.AddCell(new PdfPCell(new Phrase(v.CantidadVentas.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaVendedores.AddCell(new PdfPCell(new Phrase($"${v.TotalVendido:N2}", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                }
                document.Add(tablaVendedores);
            }

            // Comparativa Envíos
            if (ComparativaEnvios?.Any() == true)
            {
                AgregarSubtitulo("Comparativa Con/Sin Envío");

                PdfPTable tablaEnvios = new PdfPTable(3);
                tablaEnvios.WidthPercentage = 60;
                tablaEnvios.SetWidths(new float[] { 40, 30, 30 });

                string[] headersEnvio = { "Tipo", "Cantidad", "Porcentaje" };
                foreach (string h in headersEnvio)
                {
                    tablaEnvios.AddCell(new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)))
                    { BackgroundColor = azulSisie, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                foreach (var e in ComparativaEnvios)
                {
                    tablaEnvios.AddCell(new PdfPCell(new Phrase(e.Tipo, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5 });
                    tablaEnvios.AddCell(new PdfPCell(new Phrase(e.Cantidad.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaEnvios.AddCell(new PdfPCell(new Phrase($"{e.Porcentaje:F1}%", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                document.Add(tablaEnvios);
            }

            // --- DETALLE DE VENTAS ---
            AgregarSubtitulo("Detalle de Ventas");

            PdfPTable tablaVentas = new PdfPTable(5);
            tablaVentas.WidthPercentage = 100;
            tablaVentas.SetWidths(new float[] { 15, 30, 25, 15, 15 });

            string[] headersDet = { "Fecha", "Cliente", "Vendedor", "Estado", "Total" };
            foreach (string h in headersDet)
            {
                tablaVentas.AddCell(new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)))
                { BackgroundColor = azulSisie, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            foreach (var v in Ventas)
            {
                tablaVentas.AddCell(new PdfPCell(new Phrase(v.Fecha.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)))
                { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                tablaVentas.AddCell(new PdfPCell(new Phrase(v.Cliente, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                tablaVentas.AddCell(new PdfPCell(new Phrase(v.Vendedor, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                tablaVentas.AddCell(new PdfPCell(new Phrase(v.Estado, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)))
                { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                tablaVentas.AddCell(new PdfPCell(new Phrase($"${v.Total:N2}", FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)))
                { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            document.Add(tablaVentas);

            // --- PIE DE PÁGINA ---
            document.Add(new Paragraph("\n"));
            document.Add(new Chunk(separator));

            Paragraph footer = new Paragraph($"Informe generado automáticamente por SISIE | {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY));
            footer.Alignment = Element.ALIGN_RIGHT;
            footer.SpacingBefore = 5;
            document.Add(footer);

            document.Close();
            writer.Close();
        }
        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}