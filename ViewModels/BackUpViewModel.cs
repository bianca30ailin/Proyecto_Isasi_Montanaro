using Microsoft.Data.SqlClient;           
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;                    // SaveFileDialog (WPF)
using Proyecto_Isasi_Montanaro.Commands;  
using Proyecto_Isasi_Montanaro.Models;    // ProyectoTallerContext
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class BackUpViewModel : INotifyPropertyChanged
    {
        private string _nombreBaseDatos;
        private string _rutaArchivoBackup;

        public string NombreBaseDatos
        {
            get => _nombreBaseDatos;
            set { _nombreBaseDatos = value; OnPropertyChanged(nameof(NombreBaseDatos)); }
        }

        // Guardamos la ruta completa del archivo .bak 
        public string RutaArchivoBackup
        {
            get => _rutaArchivoBackup;
            set { _rutaArchivoBackup = value; OnPropertyChanged(nameof(RutaArchivoBackup)); }
        }

        public ICommand ConectarCommand { get; }
        public ICommand SeleccionarRutaCommand { get; }
        public ICommand RealizarBackupCommand { get; }

        public BackUpViewModel()
        {
            ConectarCommand = new RelayCommand(Conectar);
            SeleccionarRutaCommand = new RelayCommand(SeleccionarRuta);
            RealizarBackupCommand = new RelayCommand(RealizarBackup);
        }

        // Lee la cadena de conexion desde tu DbContext y prueba la conexion. Rellena NombreBaseDatos.
        private void Conectar(object obj)
        {
            try
            {
                // Obtiene la connection string desde tu DbContext configurado
                var context = new ProyectoTallerContext();
                var connStr = context.Database.GetConnectionString();

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    NombreBaseDatos = conn.Database; // nombre de la BD
                    MessageBox.Show($"Conectado correctamente a la base de datos '{NombreBaseDatos}'", "Conexión", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo conectar a la base de datos:\n{ex.Message}", "Error conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Abre SaveFileDialog (WPF) para elegir archivo .bak y guarda la ruta en RutaArchivoBackup
        private void SeleccionarRuta(object obj)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Guardar copia de seguridad como",
                Filter = "Backup files (*.bak)|*.bak",
                FileName = string.IsNullOrEmpty(NombreBaseDatos)
                           ? $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
                           : $"{NombreBaseDatos}_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                RutaArchivoBackup = dlg.FileName;
            }
        }

        // Ejecuta BACKUP DATABASE usando la ruta seleccionada
        private void RealizarBackup(object obj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NombreBaseDatos))
                {
                    MessageBox.Show("Primero presione 'Conectar' para detectar la base de datos.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(RutaArchivoBackup))
                {
                    MessageBox.Show("Primero seleccione la ruta / nombre de archivo donde guardar el .bak.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Asegurarse de que la carpeta existe
                var carpeta = Path.GetDirectoryName(RutaArchivoBackup);
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var context = new ProyectoTallerContext();
                var connStr = context.Database.GetConnectionString();

                string sql = $@"BACKUP DATABASE [{NombreBaseDatos}] TO DISK = '{RutaArchivoBackup}' WITH INIT, FORMAT";

                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandTimeout = 600; // aumenta timeout si la DB es grande
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"Backup creado correctamente en:\n{RutaArchivoBackup}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el backup:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}

