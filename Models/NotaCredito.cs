using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.Models
{
    public class NotaCredito
    {
        [Key]
        public int IdNotaCredito { get; set; }

        [ForeignKey(nameof(Venta))]
        public int IdNroVenta { get; set; }
        public virtual Ventum Venta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string Motivo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Relación 1..N con los detalles
        public virtual ICollection<DetalleNotaCredito> Detalles { get; set; } = new List<DetalleNotaCredito>();
    }
}
