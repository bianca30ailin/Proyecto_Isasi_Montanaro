using Proyecto_Isasi_Montanaro.Commands;
using Proyecto_Isasi_Montanaro.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Proyecto_Isasi_Montanaro.ViewModels.Informes
{
    public class InformeVentasViewModel : INotifyPropertyChanged
    {
        public ICommand VolverCommand { get; }

        public InformeVentasViewModel(Action volverAccion)
        {
            VolverCommand = new RelayCommand(_ => volverAccion());
        }
    

    // --- Notificación de cambios ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
