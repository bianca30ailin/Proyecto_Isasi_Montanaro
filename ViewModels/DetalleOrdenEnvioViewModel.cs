using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using Proyecto_Isasi_Montanaro.Models;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class DetalleOrdenEnvioViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;

        public DetalleOrdenEnvioViewModel(Envio envio)
        {
            _context = new ProyectoTallerContext();

            // Cargar datos completos
            Envio = _context.Envios
                .Include(e => e.IdNroVentaNavigation)
                    .ThenInclude(v => v.DniClienteNavigation)
                .Include(e => e.IdDireccionNavigation)
                    .ThenInclude(d => d.IdCiudadNavigation)
                        .ThenInclude(c => c.IdProvinciaNavigation)
                .Include(e => e.IdTransporteNavigation)
                .Include(e => e.IdEstadoNavigation)
                .FirstOrDefault(e => e.IdEnvio == envio.IdEnvio);

            Cliente = Envio.IdNroVentaNavigation.DniClienteNavigation;
            Direccion = Envio.IdDireccionNavigation;

            GenerarPDFCommand = new RelayCommand(_ => GenerarPDF());
        }

        public Envio Envio { get; set; }
        public Cliente Cliente { get; set; }
        public Direccion Direccion { get; set; }

        public ICommand GenerarPDFCommand { get; set; }

        private void GenerarPDF()
        {
            if (Envio == null)
            {
                MessageBox.Show("No se pudo cargar la información del envío.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GeneradorOrdenEnvioPDF.Generar(Envio, _context);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
