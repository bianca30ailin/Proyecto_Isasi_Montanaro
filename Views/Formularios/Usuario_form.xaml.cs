using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;



namespace Proyecto_Isasi_Montanaro.Views.Formularios
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Usuario_form : Window
    {
        public Usuario_form()
        {
            InitializeComponent();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                EmailError.Text = "El correo es obligatorio";
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                EmailError.Text = "Formato de correo inválido";
            }
            else
            {
                EmailError.Text = ""; // limpio el error
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb)
            {// ir al siguiente control con derecha o abajo
                if (e.Key == Key.Right || e.Key == Key.Down)
                {
                    e.Handled = true;
                    tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }//ir al siguiente control con izquierda o arriba
                else if (e.Key == Key.Left || e.Key == Key.Up)
                {
                    e.Handled = true;
                    tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                }
            }
        }
    }
}
