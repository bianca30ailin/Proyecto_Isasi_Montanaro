using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Models;
using Proyecto_Isasi_Montanaro.ViewModels;
using Proyecto_Isasi_Montanaro.Views.Formularios;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        // Implementar INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ---- Propiedad para habilitar envío ----
        private bool _envioHabilitado;
        public bool EnvioHabilitado
        {
            get => _envioHabilitado;
            set
            {
                if (_envioHabilitado != value)
                {
                    _envioHabilitado = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
