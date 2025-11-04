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
    public static class GeneradorNotaCreditoPDF
    {
        public static void Generar(NotaCredito nota, ProyectoTallerContext context)
        {
            try
            {
                // --- COLORES SISIE ---
                var azulSisie = new BaseColor(7, 39, 65);    // #072741
                var naranjaSisie = new BaseColor(227, 88, 2); // #E35802

                // --- RUTA DE DESTINO ---
                string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SISIE");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta, $"NotaCredito_{nota.IdNotaCredito}.pdf");

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

                    // --- TÍTULO Y NÚMERO ---
                    var fontTitulo = FontFactory.GetFont("Helvetica", 20, Font.BOLD, azulSisie);
                    var titulo = new Paragraph("NOTA DE CRÉDITO", fontTitulo) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(titulo);
                    doc.Add(new Paragraph(" "));

                    string nroFormateado = $"NC-0001-{nota.IdNotaCredito:D7}";

                    PdfPTable encabezado = new PdfPTable(2) { WidthPercentage = 100 };
                    encabezado.AddCell(CeldaSinBorde($"Comprobante N°: {nroFormateado}", naranjaSisie, Element.ALIGN_LEFT, true));
                    encabezado.AddCell(CeldaSinBorde($"Fecha: {nota.Fecha:dd/MM/yyyy}", naranjaSisie, Element.ALIGN_RIGHT, true));
                    doc.Add(encabezado);
                    doc.Add(new Paragraph(" "));

                    // --- DATOS CLIENTE ---
                    var venta = context.Venta.FirstOrDefault(v => v.IdNroVenta == nota.IdNroVenta);
                    var cliente = context.Clientes.FirstOrDefault(c => c.DniCliente == venta.DniCliente);

                    PdfPTable datosCliente = new PdfPTable(2) { WidthPercentage = 100 };
                    datosCliente.SetWidths(new float[] { 50, 50 });

                    PdfPCell headerCliente = new PdfPCell(new Phrase("Información del Cliente", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
                    { BackgroundColor = azulSisie, HorizontalAlignment = Element.ALIGN_CENTER };
                    PdfPCell headerDoc = new PdfPCell(new Phrase("Detalle del Documento", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD, BaseColor.WHITE)))
                    { BackgroundColor = azulSisie, HorizontalAlignment = Element.ALIGN_CENTER };
                    datosCliente.AddCell(headerCliente);
                    datosCliente.AddCell(headerDoc);

                    datosCliente.AddCell(new Phrase($"Cliente: {cliente.Nombre} {cliente.Apellido}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Venta asociada: {nota.IdNroVenta}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Email: {cliente.Email}", new Font(Font.FontFamily.HELVETICA, 10)));
                    datosCliente.AddCell(new Phrase($"Motivo: {nota.Motivo}", new Font(Font.FontFamily.HELVETICA, 10)));

                    doc.Add(datosCliente);
                    doc.Add(new Paragraph(" "));

                    // --- DETALLE DE PRODUCTOS ---
                    var detalles = context.DetalleNotaCredito
                        .Where(d => d.IdNotaCredito == nota.IdNotaCredito)
                        .Join(context.Productos, d => d.IdProducto, p => p.IdProducto,
                              (d, p) => new { p.IdProducto, p.Nombre, p.Precio, d.Cantidad, d.Subtotal })
                        .ToList();

                    PdfPTable tabla = new PdfPTable(5) { WidthPercentage = 100 };
                    tabla.SetWidths(new float[] { 15, 45, 10, 15, 15 });

                    string[] headers = { "Código", "Descripción", "Cant.", "Precio Unit. (c/IVA)", "Importe" };
                    foreach (var h in headers)
                    {
                        PdfPCell c = new PdfPCell(new Phrase(h, new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE)))
                        { BackgroundColor = naranjaSisie, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 };
                        tabla.AddCell(c);
                    }

                    double subtotal = 0;
                    foreach (var d in detalles)
                    {
                        double precioConIVA = d.Precio; // ya incluye IVA
                        double importe = d.Cantidad * precioConIVA;
                        subtotal += importe;

                        tabla.AddCell(new Phrase(d.IdProducto.ToString(), new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase(d.Nombre, new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase(d.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase($"${precioConIVA:N2}", new Font(Font.FontFamily.HELVETICA, 9)));
                        tabla.AddCell(new Phrase($"${importe:N2}", new Font(Font.FontFamily.HELVETICA, 9)));
                    }

                    doc.Add(tabla);
                    doc.Add(new Paragraph(" "));

                    // --- IVA INCLUIDO: DESGLOSE ---
                    double netoSinIVA = subtotal / 1.21;
                    double iva = subtotal - netoSinIVA;
                    double totalFinal = subtotal;

                    PdfPTable totales = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
                    totales.SetWidths(new float[] { 60, 40 });

                    totales.AddCell(CeldaSinBorde("Neto Gravado:", azulSisie, Element.ALIGN_RIGHT));
                    totales.AddCell(CeldaSinBorde($"${netoSinIVA:N2}", BaseColor.BLACK, Element.ALIGN_RIGHT));

                    totales.AddCell(CeldaSinBorde("IVA (21%):", azulSisie, Element.ALIGN_RIGHT));
                    totales.AddCell(CeldaSinBorde($"${iva:N2}", BaseColor.BLACK, Element.ALIGN_RIGHT));

                    PdfPCell totalLabel = CeldaSinBorde("TOTAL CRÉDITO:", naranjaSisie, Element.ALIGN_RIGHT, true);
                    PdfPCell totalValor = CeldaSinBorde($"${totalFinal:N2}", naranjaSisie, Element.ALIGN_RIGHT, true);
                    totales.AddCell(totalLabel);
                    totales.AddCell(totalValor);

                    doc.Add(totales);
                    doc.Add(new Paragraph(" "));

                    // --- PIE ---
                    var pie = new Paragraph("Comprobante generado automáticamente por SISIE.",
                        new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC, azulSisie))
                    { Alignment = Element.ALIGN_CENTER };
                    doc.Add(pie);

                    doc.Close();
                }

                MessageBox.Show($"Nota de crédito generada correctamente en:\n{archivo}",
                                "Comprobante generado", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar la nota de crédito: {ex.Message}",
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


