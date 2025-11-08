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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels.Informes
{
    public class InformeUsuariosViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public InformeUsuariosViewModel(Action volverAccion)
        {
            _context = new ProyectoTallerContext();

            VolverCommand = new RelayCommand(_ => volverAccion());
            GenerarReporteCommand = new RelayCommand(_ => GenerarReporte());
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            ExportarPdfCommand = new RelayCommand(_ => ExportarPdf());

            FechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
            FechaHasta = DateTime.Now;

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

        // --- MÉTRICAS ---
        private int _totalUsuarios;
        public int TotalUsuarios
        {
            get => _totalUsuarios;
            set { _totalUsuarios = value; OnPropertyChanged(); }
        }

        private int _usuariosBaja;
        public int UsuariosBaja
        {
            get => _usuariosBaja;
            set { _usuariosBaja = value; OnPropertyChanged(); }
        }

        private double _promedioEdad;
        public double PromedioEdad
        {
            get => _promedioEdad;
            set { _promedioEdad = value; OnPropertyChanged(); }
        }

        private int _usuariosIncompletos;
        public int UsuariosIncompletos
        {
            get => _usuariosIncompletos;
            set { _usuariosIncompletos = value; OnPropertyChanged(); }
        }

        public List<string> CriteriosOrden { get; } = new()
{
                "Nombre A-Z", "Nombre Z-A",
                "Fecha creación ↑", "Fecha creación ↓",
                "Cantidad roles ↑", "Cantidad roles ↓",
                "Activos primero", "Con baja primero"
};

        private string _criterioOrdenSeleccionado = "Nombre A-Z";
        public string CriterioOrdenSeleccionado
        {
            get => _criterioOrdenSeleccionado;
            set { _criterioOrdenSeleccionado = value; OnPropertyChanged(); }
        }

       
        // --- COLECCIONES ---
        public ObservableCollection<EdadDTO> UsuariosPorEdad { get; set; } = new();
        public ObservableCollection<SectorDTO> UsuariosPorSector { get; set; } = new();
        public ObservableCollection<UsuariosMesDTO> UsuariosPorMes { get; set; } = new();
        public ObservableCollection<UsuarioSinRolDTO> UsuariosSinRol { get; set; } = new();
        public ObservableCollection<UsuarioDetalleDTO> UsuariosDetalle { get; set; } = new();

        // --- COMANDOS ---
        public ICommand VolverCommand { get; }
        public ICommand GenerarReporteCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand ExportarPdfCommand { get; }

        // --- MÉTODOS ---
        private void GenerarReporte()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdTipoUsuarios)
                .AsNoTracking()
                .ToList();

            TotalUsuarios = usuarios.Count;
            UsuariosBaja = usuarios.Count(u => u.Baja.ToLower() == "si");

            var edades = usuarios
                .Where(u => u.FechaNacimiento.HasValue)
                .Select(u => (DateTime.Now - u.FechaNacimiento.Value).Days / 365.0)
                .ToList();

            PromedioEdad = edades.Any() ? Math.Round(edades.Average(), 1) : 0;

            UsuariosIncompletos = usuarios.Count(u =>
                string.IsNullOrEmpty(u.Telefono) ||
                string.IsNullOrEmpty(u.Email) ||
                string.IsNullOrEmpty(u.Direccion) ||
                !u.FechaNacimiento.HasValue);

            // --- POR EDAD (con desconocidos) ---
            var hoy = DateTime.Now;
            int menores25 = 0;
            int entre25y40 = 0;
            int mayores40 = 0;
            int desconocidos = 0;

            foreach (var u in _context.Usuarios.AsNoTracking().ToList())
            {
                if (u.FechaNacimiento == null)
                {
                    desconocidos++;
                    continue;
                }

                var edad = hoy.Year - u.FechaNacimiento.Value.Year;
                if (u.FechaNacimiento.Value > hoy.AddYears(-edad)) edad--;

                if (edad < 25)
                    menores25++;
                else if (edad <= 40)
                    entre25y40++;
                else
                    mayores40++;
            }

            // Calcular máximo para escalar barras
            int maxEdad = new[] { menores25, entre25y40, mayores40, desconocidos }.Max();
            if (maxEdad == 0) maxEdad = 1;

            UsuariosPorEdad = new ObservableCollection<EdadDTO>
{
                new EdadDTO { Rango = "Menores de 25", Cantidad = menores25, Color = "#5B21B6", AnchoCalculado = (menores25 / (double)maxEdad) * 250 },
                new EdadDTO { Rango = "Entre 25 y 40", Cantidad = entre25y40, Color = "#7C3AED", AnchoCalculado = (entre25y40 / (double)maxEdad) * 250 },
                new EdadDTO { Rango = "Mayores de 40", Cantidad = mayores40, Color = "#C4B5FD", AnchoCalculado = (mayores40 / (double)maxEdad) * 250 },
                new EdadDTO { Rango = "Desconocida", Cantidad = desconocidos, Color = "#D1D5DB", AnchoCalculado = (desconocidos / (double)maxEdad) * 250 }
};

            // --- POR SECTOR ---
            var sectores = new[] { "Inventario", "Ventas", "Admin", "Logistica", "Supervisor" };
            var listaSectores = sectores.Select(s => new SectorDTO
            {
                Sector = s,
                Cantidad = usuarios.Count(u => u.IdTipoUsuarios.Any(t => t.Tipo.Contains(s, StringComparison.OrdinalIgnoreCase)))
            }).ToList();

            double maxSector = listaSectores.Any() ? listaSectores.Max(s => s.Cantidad) : 1;

            UsuariosPorSector = new ObservableCollection<SectorDTO>(
                listaSectores.Select(s => new SectorDTO
                {
                    Sector = s.Sector,
                    Cantidad = s.Cantidad,
                    AlturaCalculada = (s.Cantidad / maxSector) * 180
                })
            );

            // --- USUARIOS POR MES ---
            var añoActual = DateTime.Now.Year;
            var usuariosPorMesRaw = usuarios
                .Where(u => u.FechaCreacion.Year == añoActual)
                .GroupBy(u => u.FechaCreacion.Month)
                .Select(g => new
                {
                    Mes = new DateTime(añoActual, g.Key, 1)
                        .ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("es-AR")),
                    Cantidad = g.Count()
                })
                .ToList();

            double maxMes = usuariosPorMesRaw.Any() ? usuariosPorMesRaw.Max(m => m.Cantidad) : 1;

            UsuariosPorMes = new ObservableCollection<UsuariosMesDTO>(
                usuariosPorMesRaw.Select(m => new UsuariosMesDTO
                {
                    Mes = m.Mes,
                    Cantidad = m.Cantidad,
                    AlturaCalculada = (m.Cantidad / maxMes) * 180
                })
            );

            // --- SIN ROL ---
            UsuariosSinRol = new ObservableCollection<UsuarioSinRolDTO>(
                usuarios.Where(u => !u.IdTipoUsuarios.Any())
                        .Select(u => new UsuarioSinRolDTO
                        {
                            NombreCompleto = $"{u.Nombre} {u.Apellido}",
                            Email = u.Email,
                            Telefono = u.Telefono
                        })
            );

            // Filtrado por rango de fechas de creación (opcional, como en otros módulos)
            var desde = FechaDesde.Date;
            var hasta = FechaHasta.Date;

            var detalleQuery = _context.Usuarios
            .Include(u => u.IdTipoUsuarios)
            .AsNoTracking()
            .Where(u => u.FechaCreacion.Date >= desde && u.FechaCreacion.Date <= hasta)
            .AsEnumerable() // <--- ⚠️ importante: pasa a LINQ-to-Objects
            .Select(u => new UsuarioDetalleDTO
            {
                NombreCompleto = u.Nombre + " " + u.Apellido,
                Dni = u.Dni,
                Email = u.Email,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                Estado = (u.Baja?.ToLower() == "si") ? "Baja" : "Activo",
                CantRoles = u.IdTipoUsuarios.Count,
                Roles = u.IdTipoUsuarios.Any()
                    ? string.Join(", ", u.IdTipoUsuarios.Select(r => r.Tipo))
                    : "Sin roles",
                FechaCreacion = u.FechaCreacion
            })
            .ToList();

            // Orden aplicado SOLO al presionar "Generar"
            detalleQuery = CriterioOrdenSeleccionado switch
            {
                "Nombre A-Z" => detalleQuery.OrderBy(u => u.NombreCompleto).ToList(),
                "Nombre Z-A" => detalleQuery.OrderByDescending(u => u.NombreCompleto).ToList(),
                "Fecha creación ↑" => detalleQuery.OrderBy(u => u.FechaCreacion).ToList(),
                "Fecha creación ↓" => detalleQuery.OrderByDescending(u => u.FechaCreacion).ToList(),
                "Cantidad roles ↑" => detalleQuery.OrderBy(u => u.CantRoles).ToList(),
                "Cantidad roles ↓" => detalleQuery.OrderByDescending(u => u.CantRoles).ToList(),
                "Activos primero" => detalleQuery.OrderBy(u => u.Estado == "Baja").ThenBy(u => u.NombreCompleto).ToList(),
                "Con baja primero" => detalleQuery.OrderByDescending(u => u.Estado == "Baja").ThenBy(u => u.NombreCompleto).ToList(),
                _ => detalleQuery
            };

            UsuariosDetalle = new ObservableCollection<UsuarioDetalleDTO>(detalleQuery);
            OnPropertyChanged(nameof(UsuariosDetalle));
        }

        private void LimpiarFiltros()
        {
            FechaDesde = new DateTime(DateTime.Now.Year, 1, 1);
            FechaHasta = DateTime.Now;
            GenerarReporte();
        }

        private void ExportarPdf()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Informe_Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Guardar Informe de Usuarios"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    GenerarPdfInforme(saveFileDialog.FileName);
                    MessageBox.Show("Informe exportado correctamente", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarPdfInforme(string rutaArchivo)
        {
            var violeta = new BaseColor(91, 33, 182);      // #5B21B6
            var grisTexto = new BaseColor(80, 80, 80);
            var grisClaro = new BaseColor(245, 245, 245);
            var azulOscuro = new BaseColor(7, 39, 65);

            Document document = new Document(PageSize.A4, 45, 45, 80, 60);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(rutaArchivo, FileMode.Create));
            document.Open();

            // --- LOGO ---
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "box", "logo_sisie.png");
            if (File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80f, 80f);
                logo.Alignment = Image.ALIGN_LEFT;
                document.Add(logo);
            }

            Paragraph empresa = new Paragraph("SISIE - Sistema Integral de Gestión de Usuarios",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, azulOscuro))
            { Alignment = Element.ALIGN_RIGHT };
            document.Add(empresa);

            LineSeparator separator = new LineSeparator(1f, 100f, violeta, Element.ALIGN_CENTER, -2);
            document.Add(new Chunk(separator));
            document.Add(new Paragraph("\n"));

            // --- TÍTULO ---
            Paragraph titulo = new Paragraph("INFORME DE USUARIOS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, azulOscuro))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10 };
            document.Add(titulo);

            Paragraph periodo = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11, grisTexto))
            { Alignment = Element.ALIGN_CENTER, SpacingAfter = 25 };
            document.Add(periodo);

            // --- RESUMEN ---
            PdfPTable tablaResumen = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 20 };
            string[] headersResumen = { "Total Usuarios", "Dado de Baja", "Promedio Edad", "Incompletos" };

            foreach (var h in headersResumen)
            {
                PdfPCell cell = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE)))
                { BackgroundColor = violeta, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 8 };
                tablaResumen.AddCell(cell);
            }

            tablaResumen.AddCell(new Phrase(TotalUsuarios.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)));
            tablaResumen.AddCell(new Phrase(UsuariosBaja.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)));
            tablaResumen.AddCell(new Phrase($"{PromedioEdad:F1}", FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)));
            tablaResumen.AddCell(new Phrase(UsuariosIncompletos.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)));
            document.Add(tablaResumen);

            void Subtitulo(string texto)
            {
                var subt = new Paragraph(texto, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, violeta))
                { SpacingBefore = 10, SpacingAfter = 10 };
                document.Add(subt);
            }

            // --- POR EDAD ---
            if (UsuariosPorEdad.Any())
            {
                Subtitulo("Distribución por Edad");
                PdfPTable tabla = new PdfPTable(2) { WidthPercentage = 60 };

                foreach (var e in UsuariosPorEdad)
                {
                    var cell1 = new PdfPCell(new Phrase(e.Rango, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)))
                    {
                        Padding = 5
                    };
                    tabla.AddCell(cell1);

                    var cell2 = new PdfPCell(new Phrase(e.Cantidad.ToString(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, azulOscuro)))
                    {
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tabla.AddCell(cell2);
                }
                document.Add(new Paragraph("Nota: la categoría 'Desconocida' incluye usuarios sin fecha de nacimiento registrada.",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY))
                {
                    SpacingBefore = 5,
                    SpacingAfter = 10
                });

                document.Add(tabla);
            }
            

            // --- POR SECTOR ---
            if (UsuariosPorSector.Any())
            {
                Subtitulo("Usuarios por Sector");
                PdfPTable tabla = new PdfPTable(2) { WidthPercentage = 60 };

                foreach (var s in UsuariosPorSector)
                {
                    var cell1 = new PdfPCell(new Phrase(s.Sector, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto)))
                    {
                        Padding = 5
                    };
                    tabla.AddCell(cell1);

                    var cell2 = new PdfPCell(new Phrase(s.Cantidad.ToString(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, azulOscuro)))
                    {
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tabla.AddCell(cell2);
                }

                document.Add(tabla);
            }
            // --- POR MES ---
            if (UsuariosPorMes.Any())
            {
                Subtitulo("Usuarios Creados por Mes (Año Actual)");
                PdfPTable tabla = new PdfPTable(2) { WidthPercentage = 60 };

                foreach (var m in UsuariosPorMes)
                {
                    var cell1 = new PdfPCell(new Phrase(m.Mes, FontFactory.GetFont(FontFactory.HELVETICA, 10, grisTexto))) { Padding = 5 };
                    tabla.AddCell(cell1);

                    var cell2 = new PdfPCell(new Phrase(m.Cantidad.ToString(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, azulOscuro)))
                    {
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tabla.AddCell(cell2);
                }

                document.Add(tabla);
            }
            // --- SIN ROL ---
            if (UsuariosSinRol.Any())
            {
                Subtitulo("Usuarios sin Rol Asignado");
                PdfPTable tabla = new PdfPTable(3) { WidthPercentage = 100 };
                string[] headers = { "Nombre Completo", "Email", "Teléfono" };
                foreach (string h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)))
                    { BackgroundColor = violeta, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER };
                    tabla.AddCell(cell);
                }

                foreach (var u in UsuariosSinRol)
                {
                    tabla.AddCell(new Phrase(u.NombreCompleto, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)));
                    tabla.AddCell(new Phrase(u.Email, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)));
                    tabla.AddCell(new Phrase(u.Telefono, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto)));
                }
                document.Add(tabla);
            }

            // --- DETALLE DE USUARIOS ---
            if (UsuariosDetalle.Any())
            {
                Subtitulo("Detalle de Usuarios");

                PdfPTable tablaDet = new PdfPTable(9) { WidthPercentage = 100 };
                tablaDet.SetWidths(new float[] { 18, 9, 16, 12, 16, 8, 12, 9, 10 });

                string[] headersDet = { "Nombre", "DNI", "Email", "Teléfono", "Dirección", "Estado", "Roles", "Cant.", "Creado" };
                foreach (var h in headersDet)
                {
                    var hc = new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE)))
                    {
                        BackgroundColor = violeta,
                        Padding = 6,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaDet.AddCell(hc);
                }

                foreach (var u in UsuariosDetalle)
                {
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.NombreCompleto, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Dni.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Email, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Telefono, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Direccion, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Estado, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.Roles, FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5 });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.CantRoles.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaDet.AddCell(new PdfPCell(new Phrase(u.FechaCreacion.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 9, grisTexto))) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                }

                document.Add(tablaDet);
            }

            document.Add(new Paragraph("\n"));
            document.Add(new Chunk(separator));
            Paragraph footer = new Paragraph($"Informe generado automáticamente por SISIE | {DateTime.Now:dd/MM/yyyy HH:mm}",
                FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY))
            { Alignment = Element.ALIGN_RIGHT };
            document.Add(footer);

            document.Close();
            writer.Close();
        }

        // --- DTOs ---
        public class EdadDTO
        {
            public string Rango { get; set; }
            public int Cantidad { get; set; }
            public string Color { get; set; }
            public double AnchoCalculado { get; set; }
        }

        public class SectorDTO
        {
            public string Sector { get; set; }
            public int Cantidad { get; set; }
            public double AlturaCalculada { get; set; }
        }

        public class UsuariosMesDTO
        {
            public string Mes { get; set; }
            public int Cantidad { get; set; }
            public double AlturaCalculada { get; set; }
        }

        public class UsuarioSinRolDTO
        {
            public string NombreCompleto { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
        }

        public class UsuarioDetalleDTO
        {
            public string NombreCompleto { get; set; }
            public int Dni { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
            public string Direccion { get; set; }
            public string Estado { get; set; }            // "Activo" / "Baja"
            public int CantRoles { get; set; }            // cantidad de roles
            public string Roles { get; set; }             // "Ventas, Inventario..."
            public DateTime FechaCreacion { get; set; }
        }

        // --- NOTIFICACIONES ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}


