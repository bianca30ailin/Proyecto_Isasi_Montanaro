using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.Commands;
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
    public class InformeInventarioViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public InformeInventarioViewModel(Action volverAccion)
        {
            _context = new ProyectoTallerContext();

            VolverCommand = new RelayCommand(_ => volverAccion());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());

            CriterioOrdenSeleccionado = "Nombre ↑";
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
            "Nombre ↑",
            "Nombre ↓",
            "Precio ↑",
            "Precio ↓",
            "Cantidad ↑",
            "Cantidad ↓"
        };

        private string _criterioOrdenSeleccionado;
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set { _criterioOrdenSeleccionado = value; OnPropertyChanged(); GenerarReporte(); }
        }

        // === TARJETAS ===
        private int _totalProductos;
        public int TotalProductos { get => _totalProductos; set { _totalProductos = value; OnPropertyChanged(); } }

        private int _activos;
        public int Activos { get => _activos; set { _activos = value; OnPropertyChanged(); } }

        private int _dadosDeBaja;
        public int DadosDeBaja { get => _dadosDeBaja; set { _dadosDeBaja = value; OnPropertyChanged(); } }

        private int _sinStock;
        public int SinStock { get => _sinStock; set { _sinStock = value; OnPropertyChanged(); } }

        private int _stockMinimo;
        public int StockMinimo { get => _stockMinimo; set { _stockMinimo = value; OnPropertyChanged(); } }

        public int CantidadSinStock { get; set; }
        public int CantidadStockMinimo { get; set; }

        // === COLECCIONES ===
        public ObservableCollection<CategoriaDTO> ProductosPorCategoria { get; set; } = new();
        public ObservableCollection<ProductoVentaDTO> MasVendidos { get; set; } = new();
        public ObservableCollection<ProductoVentaDTO> MenosVendidos { get; set; } = new();
        public ObservableCollection<ProductoPrecioDTO> ProductosMasCaros { get; set; } = new();
        public ObservableCollection<ProductoPrecioDTO> ProductosMasBaratos { get; set; } = new();
        public ObservableCollection<ProductoDetalleDTO> ProductosDetalle { get; set; } = new();
        public ObservableCollection<ProductoDetalleDTO> ProductosSinStock { get; set; } = new();
        public ObservableCollection<ProductoDetalleDTO> ProductosStockMinimo { get; set; } = new();

        // === COMANDOS ===
        public ICommand VolverCommand { get; }
        public ICommand GenerarReporteCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand ExportarPdfCommand { get; }

        // === MÉTODOS ===
        private void GenerarReporte()
        {
            var desde = FechaDesde.Date;
            var hasta = FechaHasta.Date.AddDays(1).AddSeconds(-1); // Incluir todo el último día

            // Obtener TODOS los productos (sin filtro de fecha para estadísticas generales)
            var todosLosProductos = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.UsuarioCreacion)
                    .ThenInclude(u => u.IdTipoUsuarios)
                .Include(p => p.DetalleVentaProductos)
                    .ThenInclude(d => d.IdNroVentaNavigation)
                .AsNoTracking()
                .ToList();

            // Filtrar productos creados en el rango de fechas seleccionado
            var productosFiltrados = todosLosProductos
                .Where(p => p.FechaCreacion.Date >= desde && p.FechaCreacion.Date <= hasta.Date)
                .ToList();

            // === TARJETAS (usar productos filtrados) ===
            TotalProductos = productosFiltrados.Count;
            Activos = productosFiltrados.Count(p => p.Baja.Trim().ToUpper().StartsWith("N"));
            DadosDeBaja = productosFiltrados.Count(p => p.Baja.Trim().ToUpper().StartsWith("S"));
            SinStock = productosFiltrados.Count(p => p.Cantidad == 0);
            StockMinimo = productosFiltrados.Count(p => p.Cantidad > 0 && p.Cantidad <= p.StockMinimo);

            // === CATEGORÍA (usar todos los productos activos) ===
            var productosActivos = todosLosProductos
                .Where(p => p.Baja.Trim().ToUpper().StartsWith("N"))
                .ToList();

            var porCategoria = productosActivos
                .GroupBy(p => p.IdCategoriaNavigation.Nombre)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                .OrderByDescending(c => c.Cantidad)
                .ToList();

            double maxCat = porCategoria.Any() ? porCategoria.Max(c => c.Cantidad) : 1;
            ProductosPorCategoria = new ObservableCollection<CategoriaDTO>(
                porCategoria.Select((c, i) => new CategoriaDTO
                {
                    Categoria = c.Categoria,
                    Cantidad = c.Cantidad,
                    AlturaCalculada = (c.Cantidad / maxCat) * 180,
                    Color = new[] { "#A67C52", "#5C4033", "#8B6F47", "#C19A6B" }[i % 4]
                }));

            // === PRODUCTOS MÁS / MENOS VENDIDOS (últimos 3 meses) ===
            var limite = DateOnly.FromDateTime(DateTime.Now.AddMonths(-3));

            var ventasRecientes = _context.DetalleVentaProductos
                .Include(d => d.IdProductoNavigation)
                .Include(d => d.IdNroVentaNavigation)
                .Where(d => d.IdNroVentaNavigation.FechaHora >= limite)
                .AsNoTracking()
                .ToList();

            var masVendidos = ventasRecientes
                .GroupBy(v => v.IdProductoNavigation.Nombre)
                .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .OrderByDescending(g => g.Cantidad)
                .Take(5)
                .ToList();

            var menosVendidos = ventasRecientes
                .GroupBy(v => v.IdProductoNavigation.Nombre)
                .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .OrderBy(g => g.Cantidad)
                .Take(5)
                .ToList();

            MasVendidos = new ObservableCollection<ProductoVentaDTO>(
                masVendidos.Select(v => new ProductoVentaDTO { Producto = v.Producto, Cantidad = v.Cantidad }));

            MenosVendidos = new ObservableCollection<ProductoVentaDTO>(
                menosVendidos.Select(v => new ProductoVentaDTO { Producto = v.Producto, Cantidad = v.Cantidad }));

            // === PRECIOS (usar productos filtrados) ===
            var masCaros = productosFiltrados.OrderByDescending(p => p.Precio).Take(5).ToList();
            var masBaratos = productosFiltrados.OrderBy(p => p.Precio).Take(5).ToList();

            ProductosMasCaros = new ObservableCollection<ProductoPrecioDTO>(
                masCaros.Select(p => new ProductoPrecioDTO { Producto = p.Nombre, Precio = p.Precio }));

            ProductosMasBaratos = new ObservableCollection<ProductoPrecioDTO>(
                masBaratos.Select(p => new ProductoPrecioDTO { Producto = p.Nombre, Precio = p.Precio }));

            // === TABLA DETALLE (usar productos filtrados) ===
            var detalle = productosFiltrados.Select(p => new ProductoDetalleDTO
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Categoria = p.IdCategoriaNavigation.Nombre,
                Precio = p.Precio,
                Cantidad = p.Cantidad,
                StockMinimo = p.StockMinimo,
                Estado = p.Baja.Trim().ToUpper().StartsWith("N") ? "Activo" : "Dado de Baja"
            }).ToList();

            detalle = CriterioOrdenSeleccionado switch
            {
                "Nombre ↑" => detalle.OrderBy(p => p.Nombre).ToList(),
                "Nombre ↓" => detalle.OrderByDescending(p => p.Nombre).ToList(),
                "Precio ↑" => detalle.OrderBy(p => p.Precio).ToList(),
                "Precio ↓" => detalle.OrderByDescending(p => p.Precio).ToList(),
                "Cantidad ↑" => detalle.OrderBy(p => p.Cantidad).ToList(),
                "Cantidad ↓" => detalle.OrderByDescending(p => p.Cantidad).ToList(),
                _ => detalle
            };

            ProductosDetalle = new ObservableCollection<ProductoDetalleDTO>(detalle);

            // === SIN STOCK / STOCK MÍNIMO ===
            ProductosSinStock = new ObservableCollection<ProductoDetalleDTO>(
                detalle.Where(p => p.Cantidad == 0).ToList());

            ProductosStockMinimo = new ObservableCollection<ProductoDetalleDTO>(
                detalle.Where(p => p.Cantidad > 0 && p.Cantidad <= p.StockMinimo).ToList());

            CantidadSinStock = ProductosSinStock.Count;
            CantidadStockMinimo = ProductosStockMinimo.Count;
            OnPropertyChanged(nameof(CantidadSinStock));
            OnPropertyChanged(nameof(CantidadStockMinimo));
        }

        private void LimpiarFiltros()
        {
            FechaDesde = new DateTime(2024, 1, 1);
            FechaHasta = DateTime.Now;
            CriterioOrdenSeleccionado = "Nombre ↑";
            GenerarReporte();
        }

        private void ExportarPdf()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerarPdfInforme(saveFileDialog.FileName);
                    System.Windows.MessageBox.Show("PDF exportado correctamente", "Éxito",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void GenerarPdfInforme(string ruta)
        {
            var marron = new BaseColor(166, 124, 82); // #A67C52
            var azul = new BaseColor(7, 39, 65); // #072741
            var gris = new BaseColor(80, 80, 80);
            var grisClarito = new BaseColor(249, 250, 251);
            var verde = new BaseColor(46, 204, 113);
            var rojo = new BaseColor(231, 76, 60);
            var amarillo = new BaseColor(241, 196, 15);

            var doc = new Document(PageSize.A4, 30, 30, 50, 50);
            var writer = PdfWriter.GetInstance(doc, new FileStream(ruta, FileMode.Create));
            doc.Open();

            // --- LOGO SISIE ---
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
            if (File.Exists(logoPath))
            {
                var logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsoluteWidth(90);
                logo.ScaleAbsoluteHeight(45);
                logo.Alignment = Element.ALIGN_CENTER;
                doc.Add(logo);
                doc.Add(new Paragraph(" "));
            }

            // --- TÍTULO ---
            Paragraph titulo = new Paragraph("INFORME DE INVENTARIO",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, azul))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 8 };
            doc.Add(titulo);

            Paragraph periodo = new Paragraph($"Período: {FechaDesde:dd/MM/yyyy} - {FechaHasta:dd/MM/yyyy}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11, gris))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 5 };
            doc.Add(periodo);

            Paragraph fechaGen = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontFactory.GetFont(FontFactory.HELVETICA, 9, gris))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 };
            doc.Add(fechaGen);

            // --- TABLA RESUMEN ---
            PdfPTable resumen = new PdfPTable(5) { WidthPercentage = 100, SpacingAfter = 20 };
            resumen.SetWidths(new float[] { 1f, 1f, 1f, 1f, 1f });

            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            string[] headers = { "Total Productos", "Activos", "Dados de Baja", "Sin Stock", "Stock Mínimo" };
            foreach (var h in headers)
            {
                var cell = new PdfPCell(new Phrase(h, headerFont))
                {
                    BackgroundColor = marron,
                    Padding = 8,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                resumen.AddCell(cell);
            }

            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            var values = new[] { TotalProductos, Activos, DadosDeBaja, SinStock, StockMinimo };
            foreach (var val in values)
            {
                var cell = new PdfPCell(new Phrase(val.ToString(), dataFont))
                {
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                resumen.AddCell(cell);
            }
            doc.Add(resumen);

            // --- PRODUCTOS POR CATEGORÍA ---
            if (ProductosPorCategoria.Any())
            {
                doc.Add(new Paragraph("PRODUCTOS POR CATEGORÍA",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, marron))
                { SpacingBefore = 10, SpacingAfter = 10 });

                PdfPTable tablaCat = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 15 };
                tablaCat.SetWidths(new float[] { 3f, 1f });

                var headerCat = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                tablaCat.AddCell(new PdfPCell(new Phrase("Categoría", headerCat))
                { BackgroundColor = marron, Padding = 6, HorizontalAlignment = Element.ALIGN_LEFT });
                tablaCat.AddCell(new PdfPCell(new Phrase("Cantidad", headerCat))
                { BackgroundColor = marron, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });

                var dataFontCat = FontFactory.GetFont(FontFactory.HELVETICA, 10, gris);
                bool alternate = false;
                foreach (var cat in ProductosPorCategoria)
                {
                    var bgColor = alternate ? grisClarito : BaseColor.WHITE;
                    tablaCat.AddCell(new PdfPCell(new Phrase(cat.Categoria, dataFontCat))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaCat.AddCell(new PdfPCell(new Phrase(cat.Cantidad.ToString(), dataFontCat))
                    { BackgroundColor = bgColor, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                    alternate = !alternate;
                }
                doc.Add(tablaCat);
            }

            // --- TOP 5 MÁS VENDIDOS ---
            if (MasVendidos.Any())
            {
                doc.Add(new Paragraph("TOP 5 PRODUCTOS MÁS VENDIDOS (últimos 3 meses)",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, marron))
                { SpacingBefore = 10, SpacingAfter = 10 });

                PdfPTable tablaMas = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 15 };
                tablaMas.SetWidths(new float[] { 3f, 1f });

                var headerMas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                tablaMas.AddCell(new PdfPCell(new Phrase("Producto", headerMas))
                { BackgroundColor = verde, Padding = 6, HorizontalAlignment = Element.ALIGN_LEFT });
                tablaMas.AddCell(new PdfPCell(new Phrase("Vendidos", headerMas))
                { BackgroundColor = verde, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });

                var dataFontMas = FontFactory.GetFont(FontFactory.HELVETICA, 10, gris);
                bool alternate2 = false;
                foreach (var prod in MasVendidos)
                {
                    var bgColor = alternate2 ? grisClarito : BaseColor.WHITE;
                    tablaMas.AddCell(new PdfPCell(new Phrase(prod.Producto, dataFontMas))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaMas.AddCell(new PdfPCell(new Phrase(prod.Cantidad.ToString(), dataFontMas))
                    { BackgroundColor = bgColor, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                    alternate2 = !alternate2;
                }
                doc.Add(tablaMas);
            }

            // --- PRODUCTOS SIN STOCK ---
            if (ProductosSinStock.Any())
            {
                doc.Add(new Paragraph($"PRODUCTOS SIN STOCK ({CantidadSinStock})",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, rojo))
                { SpacingBefore = 10, SpacingAfter = 10 });

                PdfPTable tablaSin = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 15 };
                tablaSin.SetWidths(new float[] { 2f, 2f, 1f });

                var headerSin = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                tablaSin.AddCell(new PdfPCell(new Phrase("Producto", headerSin))
                { BackgroundColor = rojo, Padding = 6 });
                tablaSin.AddCell(new PdfPCell(new Phrase("Categoría", headerSin))
                { BackgroundColor = rojo, Padding = 6 });
                tablaSin.AddCell(new PdfPCell(new Phrase("Stock Mín.", headerSin))
                { BackgroundColor = rojo, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });

                var dataFontSin = FontFactory.GetFont(FontFactory.HELVETICA, 9, gris);
                bool alternate3 = false;
                foreach (var prod in ProductosSinStock)
                {
                    var bgColor = alternate3 ? grisClarito : BaseColor.WHITE;
                    tablaSin.AddCell(new PdfPCell(new Phrase(prod.Nombre, dataFontSin))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaSin.AddCell(new PdfPCell(new Phrase(prod.Categoria, dataFontSin))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaSin.AddCell(new PdfPCell(new Phrase(prod.StockMinimo.ToString(), dataFontSin))
                    { BackgroundColor = bgColor, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                    alternate3 = !alternate3;
                }
                doc.Add(tablaSin);
            }

            // --- PRODUCTOS CON STOCK MÍNIMO ---
            if (ProductosStockMinimo.Any())
            {
                doc.Add(new Paragraph($"PRODUCTOS EN STOCK MÍNIMO ({CantidadStockMinimo})",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, amarillo))
                { SpacingBefore = 10, SpacingAfter = 10 });

                PdfPTable tablaMin = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 15 };
                tablaMin.SetWidths(new float[] { 2f, 1.5f, 1f, 1f });

                var headerMin = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                tablaMin.AddCell(new PdfPCell(new Phrase("Producto", headerMin))
                { BackgroundColor = amarillo, Padding = 6 });
                tablaMin.AddCell(new PdfPCell(new Phrase("Categoría", headerMin))
                { BackgroundColor = amarillo, Padding = 6 });
                tablaMin.AddCell(new PdfPCell(new Phrase("Stock", headerMin))
                { BackgroundColor = amarillo, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                tablaMin.AddCell(new PdfPCell(new Phrase("Stock Mín.", headerMin))
                { BackgroundColor = amarillo, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });

                var dataFontMin = FontFactory.GetFont(FontFactory.HELVETICA, 9, gris);
                bool alternate4 = false;
                foreach (var prod in ProductosStockMinimo)
                {
                    var bgColor = alternate4 ? grisClarito : BaseColor.WHITE;
                    tablaMin.AddCell(new PdfPCell(new Phrase(prod.Nombre, dataFontMin))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaMin.AddCell(new PdfPCell(new Phrase(prod.Categoria, dataFontMin))
                    { BackgroundColor = bgColor, Padding = 5, Border = 0 });
                    tablaMin.AddCell(new PdfPCell(new Phrase(prod.Cantidad.ToString(), dataFontMin))
                    { BackgroundColor = bgColor, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                    tablaMin.AddCell(new PdfPCell(new Phrase(prod.StockMinimo.ToString(), dataFontMin))
                    { BackgroundColor = bgColor, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                    alternate4 = !alternate4;
                }
                doc.Add(tablaMin);
            }

            // --- DETALLE COMPLETO ---
            doc.NewPage();
            doc.Add(new Paragraph("DETALLE COMPLETO DE PRODUCTOS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, azul))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 15 });

            PdfPTable tablaDetalle = new PdfPTable(6) { WidthPercentage = 100 };
            tablaDetalle.SetWidths(new float[] { 2f, 1.5f, 1f, 0.8f, 0.8f, 1f });

            var headerDet = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            string[] headersDet = { "Nombre", "Categoría", "Precio", "Cant.", "Stock Mín.", "Estado" };
            foreach (var h in headersDet)
            {
                tablaDetalle.AddCell(new PdfPCell(new Phrase(h, headerDet))
                { BackgroundColor = azul, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            var dataFontDet = FontFactory.GetFont(FontFactory.HELVETICA, 8, gris);
            bool alternate5 = false;
            foreach (var prod in ProductosDetalle)
            {
                var bgColor = alternate5 ? grisClarito : BaseColor.WHITE;

                tablaDetalle.AddCell(new PdfPCell(new Phrase(prod.Nombre, dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, Border = 0 });
                tablaDetalle.AddCell(new PdfPCell(new Phrase(prod.Categoria, dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, Border = 0 });
                tablaDetalle.AddCell(new PdfPCell(new Phrase($"${prod.Precio:N2}", dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, HorizontalAlignment = Element.ALIGN_RIGHT, Border = 0 });
                tablaDetalle.AddCell(new PdfPCell(new Phrase(prod.Cantidad.ToString(), dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                tablaDetalle.AddCell(new PdfPCell(new Phrase(prod.StockMinimo.ToString(), dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                tablaDetalle.AddCell(new PdfPCell(new Phrase(prod.Estado, dataFontDet))
                { BackgroundColor = bgColor, Padding = 4, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });

                alternate5 = !alternate5;
            }
            doc.Add(tablaDetalle);

            doc.Close();
            writer.Close();
        }

        // === DTOs ===
        public class CategoriaDTO { public string Categoria { get; set; } public int Cantidad { get; set; } public double AlturaCalculada { get; set; } public string Color { get; set; } }
        public class ProductoVentaDTO { public string Producto { get; set; } public int Cantidad { get; set; } }
        public class ProductoPrecioDTO { public string Producto { get; set; } public double Precio { get; set; } }
        public class ProductoDetalleDTO
        {
            public int IdProducto { get; set; }
            public string Nombre { get; set; }
            public string Categoria { get; set; }
            public double Precio { get; set; }
            public int Cantidad { get; set; }
            public int StockMinimo { get; set; }
            public string Estado { get; set; }
        }

        // === Notify ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
