using iTextSharp.text;
using iTextSharp.text.pdf;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public static class GeneradorFacturaPDF
    {
        public static void Generar(Ventum venta)
        {
            try
            {
                // --- COLORES SISIE ---
                var azulSisie = new BaseColor(7, 39, 65);    // #072741
                var naranjaSisie = new BaseColor(227, 88, 2); // #E35802

                // --- RUTA DE DESTINO ---
                string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SISIE");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta, $"Factura_{venta.IdNroVenta}.pdf");

                using (var doc = new Document(PageSize.A4, 40f, 40f, 60f, 40f))
                {
                    PdfWriter.GetInstance(doc, new FileStream(archivo, FileMode.Create));
                    doc.Open();

                    // --- LOGO ---
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
                    if (File.Exists(logoPath))
                    {
                        var logo = Image.GetInstance(logoPath);
                        logo.ScaleAbsolute(90f, 90f);
                        logo.Alignment = Element.ALIGN_RIGHT;
                        doc.Add(logo);
                    }

                    // --- TÍTULO ---
                    var fontTitulo = FontFactory.GetFont("Helvetica", 20, Font.BOLD, azulSisie);
                    var titulo = new Paragraph("FACTURA", fontTitulo);
                    titulo.Alignment = Element.ALIGN_LEFT;
                    doc.Add(titulo);
                    doc.Add(new Paragraph(" "));

                    // --- N° FORMATEADO ---
                    string nroFormateado = $"FC-0001-{venta.IdNroVenta:D7}";

                    PdfPTable encabezado = new PdfPTable(2);
                    encabezado.WidthPercentage = 100;
                    encabezado.AddCell(CeldaSinBorde($"Factura N°: {nroFormateado}", naranjaSisie, Element.ALIGN_LEFT, bold: true));
                    encabezado.AddCell(CeldaSinBorde($"Fecha: {venta.FechaHora:dd/MM/yyyy}", naranjaSisie, Element.ALIGN_RIGHT, bold: true));
                    doc.Add(encabezado);
                    doc.Add(new Paragraph(" "));

                    // --- DATOS CLIENTE / DOCUMENTO ---
                    var cliente = venta.DniClienteNavigation;

                    PdfPTable datosCliente = new PdfPTable(2);
                    datosCliente.WidthPercentage = 100;
                    datosCliente.SetWidths(new float[] { 50, 50 });

                    PdfPCell headerCliente = new PdfPCell(new Phrase("Información del Cliente", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)));
                    headerCliente.BackgroundColor = azulSisie;
                    headerCliente.HorizontalAlignment = Element.ALIGN_CENTER;
                    datosCliente.AddCell(headerCliente);

                    PdfPCell headerDoc = new PdfPCell(new Phrase("Detalle del Documento", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)));
                    headerDoc.BackgroundColor = azulSisie;
                    headerDoc.HorizontalAlignment = Element.ALIGN_CENTER;
                    datosCliente.AddCell(headerDoc);

                    datosCliente.AddCell(new Phrase($"Cliente: {cliente.Nombre} {cliente.Apellido}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Forma de pago: {venta.IdFormaPagoNavigation.Nombre}", new Font(Font.FontFamily.HELVETICA, 10)));

                    datosCliente.AddCell(new Phrase($"DNI: {cliente.DniCliente}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Estado: {venta.EstadoVenta?.NombreEstado}", new Font(Font.FontFamily.HELVETICA, 10)));

                    datosCliente.AddCell(new Phrase($"Email: {cliente.Email}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Cuotas: {venta.TotalCuotas}", new Font(Font.FontFamily.HELVETICA, 10)));

                    doc.Add(datosCliente);
                    doc.Add(new Paragraph(" "));

                    // --- DETALLE DE PRODUCTOS ---
                    PdfPTable tabla = new PdfPTable(5);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(new float[] { 15, 45, 10, 15, 15 });

                    string[] headers = { "Código", "Descripción", "Cant.", "Precio Unit. (c/IVA)", "Importe" };
                    foreach (var h in headers)
                    {
                        PdfPCell c = new PdfPCell(new Phrase(h, new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE)));
                        c.BackgroundColor = naranjaSisie;
                        c.HorizontalAlignment = Element.ALIGN_CENTER;
                        c.Padding = 5;
                        tabla.AddCell(c);
                    }

                    double totalFinal = venta.Total;
                    double subtotal = 0;

                    foreach (var d in venta.DetalleVentaProductos)
                    {
                        double precioConIVA = d.IdProductoNavigation.Precio; // Precio final con IVA
                        double subtotalProducto = d.Cantidad * precioConIVA;
                        subtotal += subtotalProducto;

                        tabla.AddCell(new Phrase(d.IdProducto.ToString(), new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase(d.IdProductoNavigation.Nombre, new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase(d.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase($"${precioConIVA:N2}", new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase($"${subtotalProducto:N2}", new Font(Font.FontFamily.HELVETICA, 9)));
                    }

                    doc.Add(tabla);
                    doc.Add(new Paragraph(" "));

                    // --- DATOS ENVÍO ---
                    var envio = venta.Envios.FirstOrDefault();
                    if (envio != null)
                    {
                        PdfPTable envioTabla = new PdfPTable(1);
                        envioTabla.WidthPercentage = 100;

                        PdfPCell headerEnvio = new PdfPCell(new Phrase("Datos de Envío", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
                        {
                            BackgroundColor = azulSisie,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        envioTabla.AddCell(headerEnvio);

                        envioTabla.AddCell(new Phrase($"Dirección: {envio.IdDireccionNavigation.DireccionCompleta}", new Font(Font.FontFamily.HELVETICA, 10)));
                        envioTabla.AddCell(new Phrase($"Transporte: {envio.IdTransporteNavigation.Nombre}", new Font(Font.FontFamily.HELVETICA, 10)));
                        envioTabla.AddCell(new Phrase($"Costo de envío: ${envio.Costo:N2}", new Font(Font.FontFamily.HELVETICA, 10)));

                        doc.Add(envioTabla);
                        doc.Add(new Paragraph(" "));
                    }

                    // --- CÁLCULO IVA (solo mostrar desglose) ---
                    // IVA = 21% del neto sin IVA → si el precio ya incluye IVA, se obtiene así:
                    double netoSinIVA = subtotal / 1.21;
                    double iva = subtotal - netoSinIVA;

                    // --- TABLA DE TOTALES ---
                    PdfPTable totales = new PdfPTable(2);
                    totales.WidthPercentage = 40;
                    totales.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totales.SetWidths(new float[] { 60, 40 });

                    totales.AddCell(CeldaSinBorde("Neto Gravado:", azulSisie, Element.ALIGN_RIGHT));
                    totales.AddCell(CeldaSinBorde($"${netoSinIVA:N2}", BaseColor.BLACK, Element.ALIGN_RIGHT));

                    totales.AddCell(CeldaSinBorde("IVA (21%):", azulSisie, Element.ALIGN_RIGHT));
                    totales.AddCell(CeldaSinBorde($"${iva:N2}", BaseColor.BLACK, Element.ALIGN_RIGHT));

                    totales.AddCell(CeldaSinBorde("TOTAL:", naranjaSisie, Element.ALIGN_RIGHT, bold: true));
                    totales.AddCell(CeldaSinBorde($"${subtotal:N2}", naranjaSisie, Element.ALIGN_RIGHT, bold: true));

                    doc.Add(totales);
                    doc.Add(new Paragraph(" "));

                    var agradecimiento = new Paragraph("¡Gracias por su compra!", new Font(Font.FontFamily.HELVETICA, 11, Font.ITALIC, azulSisie))
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    doc.Add(agradecimiento);

                    doc.Close();
                }

                MessageBox.Show($"Factura generada correctamente en:\n{archivo}",
                                "Factura generada", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar la factura: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static PdfPCell CeldaSinBorde(string texto, BaseColor color, int align, bool bold = false)
        {
            var fuente = new Font(Font.FontFamily.HELVETICA, 10, bold ? Font.BOLD : Font.NORMAL, color);
            var cell = new PdfPCell(new Phrase(texto, fuente))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = align,
                Padding = 4
            };
            return cell;
        }
    }
}
