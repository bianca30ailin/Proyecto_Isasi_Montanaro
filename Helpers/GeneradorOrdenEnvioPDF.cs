using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Proyecto_Isasi_Montanaro.Helpers
{
    public static class GeneradorOrdenEnvioPDF
    {
        public static void Generar(Envio envio, ProyectoTallerContext context)
        {
            try
            {
                // --- COLORES SISIE ---
                var azulSisie = new BaseColor(7, 39, 65);      // #072741
                var verdeSisie = new BaseColor(83, 186, 131);  // #53BA83
                var grisLabel = BaseColor.GRAY;

                // --- RUTA DE DESTINO ---
                string carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SISIE", "Ordenes_Envio");
                Directory.CreateDirectory(carpeta);
                string archivo = Path.Combine(carpeta, $"OrdenEnvio_{envio.IdEnvio}.pdf");

                // --- OBTENER DATOS ---
                var cliente = envio.IdNroVentaNavigation.DniClienteNavigation;
                var direccion = envio.IdDireccionNavigation;
                var transporte = envio.IdTransporteNavigation;

                using (var doc = new Document(PageSize.A4, 50f, 50f, 50f, 50f))
                {
                    PdfWriter.GetInstance(doc, new FileStream(archivo, FileMode.Create));
                    doc.Open();

                    // --- LOGO (Opcional) ---
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
                    if (File.Exists(logoPath))
                    {
                        var logo = Image.GetInstance(logoPath);
                        logo.ScaleAbsolute(80f, 80f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        doc.Add(logo);
                        doc.Add(new Paragraph(" "));
                    }

                    // --- TÍTULO ---
                    var fontTitulo = FontFactory.GetFont("Helvetica", 24, Font.BOLD, azulSisie);
                    var titulo = new Paragraph("ORDEN DE ENVÍO", fontTitulo)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    doc.Add(titulo);

                    // --- NÚMERO DE ORDEN ---
                    var fontNumero = FontFactory.GetFont("Helvetica", 16, Font.BOLD, verdeSisie);
                    var numero = new Paragraph($"N° {envio.IdEnvio}", fontNumero)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    doc.Add(numero);

                    // --- LÍNEA SEPARADORA ---
                    var linea = new LineSeparator(2f, 100f, verdeSisie, Element.ALIGN_CENTER, -1);
                    doc.Add(new Chunk(linea));
                    doc.Add(new Paragraph(" "));

                    // --- EMPRESA ---
                    var fontEmpresa = FontFactory.GetFont("Helvetica", 12, Font.BOLD, azulSisie);
                    var empresa = new Paragraph("SISIE - Gabriela Montanaro, Bianca Isasi Vitale", fontEmpresa)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 20f
                    };
                    doc.Add(empresa);

                    // --- DESTINATARIO ---
                    AgregarSeccion(doc, "DESTINATARIO", azulSisie);
                    AgregarCampo(doc, "Nombre Completo:", cliente.NombreCompleto, grisLabel);
                    AgregarCampo(doc, "DNI:", cliente.DniCliente.ToString(), grisLabel);
                    AgregarCampo(doc, "Teléfono:", cliente.Telefono, grisLabel);
                    AgregarCampo(doc, "Email:", cliente.Email, grisLabel);

                    // --- SEPARADOR ---
                    doc.Add(new Paragraph(" "));
                    var separador = new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -1);
                    doc.Add(new Chunk(separador));
                    doc.Add(new Paragraph(" "));

                    // --- DIRECCIÓN DE ENTREGA ---
                    AgregarSeccion(doc, "DIRECCIÓN DE ENTREGA", azulSisie);
                    AgregarCampo(doc, "Dirección:", direccion?.DireccionCompleta ?? "Sin dirección", grisLabel);
                    AgregarCampo(doc, "Código Postal:", direccion.IdCiudadNavigation.CodigoPostal.ToString(), grisLabel);
                    AgregarCampo(doc, "Ciudad:", direccion.IdCiudadNavigation?.Nombre ?? "", grisLabel);
                    AgregarCampo(doc, "Provincia:", direccion.IdCiudadNavigation?.IdProvinciaNavigation?.Nombre ?? "", grisLabel);


                    // --- SEPARADOR ---
                    doc.Add(new Paragraph(" "));
                    doc.Add(new Chunk(separador));
                    doc.Add(new Paragraph(" "));

                    // --- TRANSPORTE ---
                    AgregarSeccion(doc, "TRANSPORTE", azulSisie);
                    AgregarCampo(doc, "Servicio:", transporte.Nombre, grisLabel);

                    // --- ESPACIO ANTES DE FIRMA ---
                    doc.Add(new Paragraph(" "));
                    doc.Add(new Paragraph(" "));
                    doc.Add(new Paragraph(" "));

                    
                    var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}",
                        FontFactory.GetFont("Helvetica", 10, Font.NORMAL, grisLabel))
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 5f
                    };
                    doc.Add(fecha);

                    doc.Close();
                }

                MessageBox.Show($"Orden de envío generada correctamente en:\n{archivo}",
                    "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);

                // Abrir el PDF automáticamente
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar la orden de envío: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTODOS AUXILIARES ---
        private static void AgregarSeccion(Document doc, string titulo, BaseColor color)
        {
            var fontSeccion = FontFactory.GetFont("Helvetica", 14, Font.BOLD, color);
            var seccion = new Paragraph(titulo, fontSeccion)
            {
                SpacingAfter = 10f
            };
            doc.Add(seccion);
        }

        private static void AgregarCampo(Document doc, string label, string valor, BaseColor colorLabel)
        {
            var fontLabel = FontFactory.GetFont("Helvetica", 10, Font.BOLD, colorLabel);
            var fontValor = FontFactory.GetFont("Helvetica", 11, Font.NORMAL, BaseColor.BLACK);

            var parrafo = new Paragraph();
            parrafo.Add(new Chunk(label + " ", fontLabel));
            parrafo.Add(new Chunk(valor, fontValor));
            parrafo.SpacingAfter = 8f;
            doc.Add(parrafo);
        }
    }
}