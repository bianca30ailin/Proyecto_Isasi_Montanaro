using System.Configuration;
using System.Data;
using System.Windows;

namespace Proyecto_Isasi_Montanaro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var culture = new System.Globalization.CultureInfo("es-AR");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }
    }

}
