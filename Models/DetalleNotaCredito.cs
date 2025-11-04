using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Isasi_Montanaro.Models
{
    public class DetalleNotaCredito
    {
        [Key]
        public int IdDetalleNotaCredito { get; set; }

        [ForeignKey(nameof(NotaCredito))]
        public int IdNotaCredito { get; set; }
        public virtual NotaCredito NotaCredito { get; set; }

        [ForeignKey(nameof(Producto))]
        public int IdProducto { get; set; }
        public virtual Producto Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
