using iTextSharp.text;
using iTextSharp.text.pdf;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels.Informes
{
    public class InformeEnviosViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public InformeEnviosViewModel(Action volverAccion)
        {
            _context = new ProyectoTallerContext();

            VolverCommand = new RelayCommand(_ => volverAccion());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());

            CriterioOrdenSeleccionado = "Fecha despacho ↓";
            GenerarReporte();
        }

        // === FILTROS ===
        private DateTime _fechaDesde = new DateTime(2024, 1, 1);
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

        public List<string> CriteriosOrden { get; } = new()
        {
            "Fecha despacho ↑",
            "Fecha despacho ↓",
            "Fecha entrega ↑",
            "Fecha entrega ↓",
            "Costo ↑",
            "Costo ↓",
            "Transporte"
        };

        private string _criterioOrdenSeleccionado;
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set { _criterioOrdenSeleccionado = value; OnPropertyChanged(); }
        }

        // === DATOS PARA TARJETAS ===
        private int _totalEnvios;
        public int TotalEnvios
        {
            get => _totalEnvios;
            set { _totalEnvios = value; OnPropertyChanged(); }
        }

        private double _promedioEntregaDias;
        public double PromedioEntregaDias
        {
            get => _promedioEntregaDias;
            set { _promedioEntregaDias = value; OnPropertyChanged(); }
        }

        private int _sinDespacho;
        public int SinDespacho
        {
            get => _sinDespacho;
            set { _sinDespacho = value; OnPropertyChanged(); }
        }

        // === COLECCIONES PARA GRÁFICOS ===
        private ObservableCollection<EstadoEnvioDTO> _enviosPorEstado;
        public ObservableCollection<EstadoEnvioDTO> EnviosPorEstado
        {
            get => _enviosPorEstado;
            set { _enviosPorEstado = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TransporteDTO> _enviosPorTransporte;
        public ObservableCollection<TransporteDTO> EnviosPorTransporte
        {
            get => _enviosPorTransporte;
            set { _enviosPorTransporte = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CiudadDTO> _enviosPorCiudad;
        public ObservableCollection<CiudadDTO> EnviosPorCiudad
        {
            get => _enviosPorCiudad;
            set { _enviosPorCiudad = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CiudadDTO> _promedioCostoPorCiudad;
        public ObservableCollection<CiudadDTO> PromedioCostoPorCiudad
        {
            get => _promedioCostoPorCiudad;
            set { _promedioCostoPorCiudad = value; OnPropertyChanged(); }
        }

        private ObservableCollection<EnvioDetalleDTO> _enviosDetalle;
        public ObservableCollection<EnvioDetalleDTO> EnviosDetalle
        {
            get => _enviosDetalle;
            set { _enviosDetalle = value; OnPropertyChanged(); }
        }

        // === COMANDOS ===
        public ICommand VolverCommand { get; }
        public ICommand GenerarReporteCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand ExportarPdfCommand { get; }

        // === MÉTODO PRINCIPAL ===
        private void GenerarReporte()
        {
            var desde = DateOnly.FromDateTime(FechaDesde);
            var hasta = DateOnly.FromDateTime(FechaHasta);

            var envios = _context.Envios
                .Include(e => e.IdEstadoNavigation)
                .Include(e => e.IdTransporteNavigation)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdCiudadNavigation)
                .AsNoTracking()
                .Where(e =>
                    e.FechaDespacho == null ||
                    (e.FechaDespacho >= desde && e.FechaDespacho <= hasta))
                .ToList();

            // === TARJETAS ===
            TotalEnvios = envios.Count;

            var entregados = envios.Where(e => e.FechaDespacho != null && e.FechaEntrega != null);
            PromedioEntregaDias = entregados.Any()
                ? entregados.Average(e => Math.Max(0,
                    (e.FechaEntrega.Value.ToDateTime(TimeOnly.MinValue) -
                     e.FechaDespacho.Value.ToDateTime(TimeOnly.MinValue)).TotalDays))
                : 0;

            SinDespacho = envios.Count(e => e.FechaDespacho == null);

            // === GRÁFICO 1: POR ESTADO ===
            var estados = envios
                .GroupBy(e => e.IdEstadoNavigation.Nombre)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToList();

            int maxEstado = estados.Any() ? estados.Max(e => e.Cantidad) : 1;
            EnviosPorEstado = new ObservableCollection<EstadoEnvioDTO>(
                estados.Select((e, i) => new EstadoEnvioDTO
                {
                    Estado = e.Estado,
                    Cantidad = e.Cantidad,
                    AnchoCalculado = (e.Cantidad / (double)maxEstado) * 250,
                    Color = new[] { "#F5A623", "#072741", "#1E90FF", "#FF6B6B" }[i % 4]
                }));

            // === GRÁFICO 2: POR TRANSPORTE ===
            var transportes = envios
                .GroupBy(e => e.IdTransporteNavigation.Nombre)
                .Select(g => new { Transporte = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .ToList();

            double maxTrans = transportes.Any() ? transportes.Max(t => t.Cantidad) : 1;
            EnviosPorTransporte = new ObservableCollection<TransporteDTO>(
                transportes.Select(t => new TransporteDTO
                {
                    Transporte = t.Transporte,
                    Cantidad = t.Cantidad,
                    AlturaCalculada = (t.Cantidad / maxTrans) * 180
                }));

            // === GRÁFICO 3: ENVÍOS POR CIUDAD ===
            var ciudades = envios
                .GroupBy(e => e.IdDireccionNavigation.IdCiudadNavigation.Nombre)
                .Select(g => new { Ciudad = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .Take(10)
                .ToList();

            double maxCiu = ciudades.Any() ? ciudades.Max(c => c.Cantidad) : 1;
            EnviosPorCiudad = new ObservableCollection<CiudadDTO>(
                ciudades.Select(c => new CiudadDTO
                {
                    Ciudad = c.Ciudad,
                    Cantidad = c.Cantidad,
                    AlturaCalculada = (c.Cantidad / maxCiu) * 180
                }));

            // === GRÁFICO 4: PROMEDIO COSTO POR CIUDAD ===
            var costoProm = envios
                .Where(e => e.Costo != null)
                .GroupBy(e => e.IdDireccionNavigation.IdCiudadNavigation.Nombre)
                .Select(g => new { Ciudad = g.Key, Promedio = g.Average(x => x.Costo) })
                .OrderByDescending(g => g.Promedio)
                .Take(10)
                .ToList();

            double maxCosto = costoProm.Any() ? costoProm.Max(c => c.Promedio ?? 0) : 1;
            PromedioCostoPorCiudad = new ObservableCollection<CiudadDTO>(
                costoProm.Select(c => new CiudadDTO
                {
                    Ciudad = c.Ciudad,
                    Promedio = c.Promedio ?? 0,
                    AlturaCalculada = ((c.Promedio ?? 0) / maxCosto) * 180
                }));

            // === TABLA DETALLE ===
            var detalle = envios.Select(e => new EnvioDetalleDTO
            {
                IdEnvio = e.IdEnvio,
                Estado = e.IdEstadoNavigation.Nombre,
                Transporte = e.IdTransporteNavigation.Nombre,
                Ciudad = e.IdDireccionNavigation.IdCiudadNavigation.Nombre,
                FechaDespacho = e.FechaDespacho?.ToString("dd/MM/yyyy") ?? "-",
                FechaEntrega = e.FechaEntrega?.ToString("dd/MM/yyyy") ?? "-",
                Costo = e.Costo ?? 0,
                FechaDespachoValue = e.FechaDespacho,
                FechaEntregaValue = e.FechaEntrega
            }).ToList();

            detalle = CriterioOrdenSeleccionado switch
            {
                "Fecha despacho ↑" => detalle.OrderBy(e => e.FechaDespachoValue).ToList(),
                "Fecha despacho ↓" => detalle.OrderByDescending(e => e.FechaDespachoValue).ToList(),
                "Fecha entrega ↑" => detalle.OrderBy(e => e.FechaEntregaValue).ToList(),
                "Fecha entrega ↓" => detalle.OrderByDescending(e => e.FechaEntregaValue).ToList(),
                "Costo ↑" => detalle.OrderBy(e => e.Costo).ToList(),
                "Costo ↓" => detalle.OrderByDescending(e => e.Costo).ToList(),
                "Transporte" => detalle.OrderBy(e => e.Transporte).ToList(),
                _ => detalle
            };

            EnviosDetalle = new ObservableCollection<EnvioDetalleDTO>(detalle);
        }

        private void LimpiarFiltros()
        {
            FechaDesde = new DateTime(2024, 1, 1);
            FechaHasta = DateTime.Now;
            CriterioOrdenSeleccionado = "Fecha despacho ↓";
            GenerarReporte();
        }

        // === EXPORTAR PDF ===
        private void ExportarPdf()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_Envios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                    GenerarPdfInforme(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al exportar PDF: {ex.Message}");
            }
        }

        private void GenerarPdfInforme(string ruta)
        {
            var azul = new BaseColor(7, 39, 65);
            var amarillo = new BaseColor(245, 166, 35);
            var grisTexto = new BaseColor(60, 60, 60);

            var doc = new Document(PageSize.A4, 45, 45, 80, 60);
            var writer = PdfWriter.GetInstance(doc, new FileStream(ruta, FileMode.Create));
            doc.Open();

            Paragraph titulo = new Paragraph("INFORME DE ENVÍOS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, azul));
            titulo.Alignment = Element.ALIGN_CENTER;
            titulo.SpacingAfter = 10;
            doc.Add(titulo);

            Paragraph periodo = new Paragraph($"Período: {FechaDesde:dd/MM/yyyy} - {FechaHasta:dd/MM/yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11, grisTexto))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 };
            doc.Add(periodo);

            PdfPTable resumen = new PdfPTable(3) { WidthPercentage = 100 };
            string[] h = { "Total Envíos", "Promedio Entrega (días)", "Sin Despacho" };
            foreach (var t in h)
            {
                resumen.AddCell(new PdfPCell(new Phrase(t, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)))
                { BackgroundColor = azul, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            }
            resumen.AddCell(new PdfPCell(new Phrase(TotalEnvios.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            resumen.AddCell(new PdfPCell(new Phrase($"{PromedioEntregaDias:F1}", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            resumen.AddCell(new PdfPCell(new Phrase(SinDespacho.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            doc.Add(resumen);

            Paragraph subtitulo = new Paragraph("Detalle de Envíos",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, amarillo))
            { SpacingBefore = 20, SpacingAfter = 10 };
            doc.Add(subtitulo);

            PdfPTable tabla = new PdfPTable(7) { WidthPercentage = 100 };
            string[] headers = { "ID", "Estado", "Transporte", "Ciudad", "Despacho", "Entrega", "Costo" };
            foreach (var hh in headers)
            {
                tabla.AddCell(new PdfPCell(new Phrase(hh, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)))
                { BackgroundColor = azul, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            foreach (var e in EnviosDetalle)
            {
                tabla.AddCell(new PdfPCell(new Phrase(e.IdEnvio.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER });
                tabla.AddCell(new PdfPCell(new Phrase(e.Estado, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4 });
                tabla.AddCell(new PdfPCell(new Phrase(e.Transporte, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4 });
                tabla.AddCell(new PdfPCell(new Phrase(e.Ciudad, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4 });
                tabla.AddCell(new PdfPCell(new Phrase(e.FechaDespacho, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4 });
                tabla.AddCell(new PdfPCell(new Phrase(e.FechaEntrega, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4 });
                tabla.AddCell(new PdfPCell(new Phrase($"${e.Costo:N2}", FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 4, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            doc.Add(tabla);
            doc.Close();
            writer.Close();
        }

        // === DTOs ===
        public class EstadoEnvioDTO { public string Estado { get; set; } public int Cantidad { get; set; } public double AnchoCalculado { get; set; } public string Color { get; set; } }
        public class TransporteDTO { public string Transporte { get; set; } public int Cantidad { get; set; } public double AlturaCalculada { get; set; } }
        public class CiudadDTO { public string Ciudad { get; set; } public int Cantidad { get; set; } public double? Promedio { get; set; } public double AlturaCalculada { get; set; } }
        public class EnvioDetalleDTO
        {
            public int IdEnvio { get; set; }
            public string Estado { get; set; }
            public string Transporte { get; set; }
            public string Ciudad { get; set; }
            public string FechaDespacho { get; set; }
            public string FechaEntrega { get; set; }
            public double Costo { get; set; }
            public DateOnly? FechaDespachoValue { get; set; }
            public DateOnly? FechaEntregaValue { get; set; }
        }

        // === Notify ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

