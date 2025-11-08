using Microsoft.EntityFrameworkCore;
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class Transportes_form_ViewModel : INotifyPropertyChanged
    {
        private readonly ProyectoTallerContext _context;
        
        public Transportes_form_ViewModel()
        {
            _context = new ProyectoTallerContext();

            CargarTransportes();

            AgregarTransporteCommand = new RelayCommand(_ => AgregarTransporte(), _ => PuedeAgregar());
            
        }

        // --- PROPIEDADES ---
        private ObservableCollection<Transporte> _transportes;
        public ObservableCollection<Transporte> Transportes
        {
            get => _transportes;
            set
            {
                _transportes = value;
                OnPropertyChanged();
            }
        }

        private string _nuevoTransporteNombre;
        public string NuevoTransporteNombre
        {
            get => _nuevoTransporteNombre;
            set
            {
                _nuevoTransporteNombre = value;
                OnPropertyChanged();
                ErrorNuevoTransporte = string.Empty;

                // Forzar re-evaluación del CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _errorNuevoTransporte;
        public string ErrorNuevoTransporte
        {
            get => _errorNuevoTransporte;
            set
            {
                _errorNuevoTransporte = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneErrorNuevoTransporte));
            }
        }

        // --- COMANDOS ---
        public ICommand AgregarTransporteCommand { get; }
        public bool TieneErrorNuevoTransporte => !string.IsNullOrWhiteSpace(ErrorNuevoTransporte);



        // --- MÉTODOS ---
        private void CargarTransportes()
        {
            var transportes = _context.Transportes
                .AsNoTracking()
                .OrderBy(t => t.Nombre)
                .ToList();

            Transportes = new ObservableCollection<Transporte>(transportes);
        }

        private bool PuedeAgregar()
        {
            return !string.IsNullOrWhiteSpace(NuevoTransporteNombre);
        }

        private void AgregarTransporte()
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(NuevoTransporteNombre))
            {
                ErrorNuevoTransporte = "Debe ingresar un nombre.";
                return;
            }

            // Validar que no exista
            if (_context.Transportes.Any(t => t.Nombre.ToLower() == NuevoTransporteNombre.Trim().ToLower()))
            {
                ErrorNuevoTransporte = "Ya existe un transporte con ese nombre.";
                return;
            }

            try
            {
                var nuevoTransporte = new Transporte
                {
                    Nombre = NuevoTransporteNombre.Trim()
                };

                _context.Transportes.Add(nuevoTransporte);
                _context.SaveChanges();

                MessageBox.Show($"Transporte '{nuevoTransporte.Nombre}' agregado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar y recargar
                NuevoTransporteNombre = string.Empty;
                CargarTransportes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar transporte: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}