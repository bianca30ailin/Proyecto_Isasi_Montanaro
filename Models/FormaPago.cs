using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.Models
{
    public partial class FormaPago
    {
        public int IdFormaPago { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }

        // Relación: una forma de pago puede estar en muchas ventas
        public virtual ICollection<Ventum> Ventas { get; set; } = new List<Ventum>();
    }
}
