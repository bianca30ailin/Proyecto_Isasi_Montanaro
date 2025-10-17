using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Isasi_Montanaro.Models
{
    public partial class EstadoVenta
    {
        [Key]
        public int IdEstadoVenta { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreEstado { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [MaxLength(255)]
        public string? Motivo { get; set; }

        // Relación uno a muchos con Venta
        public ICollection<Ventum> Ventas { get; set; } = new List<Ventum>();
    }
}
