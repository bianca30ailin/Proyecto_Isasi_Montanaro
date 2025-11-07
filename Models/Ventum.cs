using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Ventum
{
    public int IdNroVenta { get; set; }
    public DateOnly FechaHora { get; set; }
    public double Total { get; set; }
    public int DniCliente { get; set; }
    public int? IdUsuario { get; set; }
    public int? EstadoVentaId { get; set; }
    public int IdFormaPago { get; set; }
    public int? TotalCuotas { get; set; }


    public virtual Cliente DniClienteNavigation { get; set; } = null!;
    public virtual Usuario? IdUsuarioNavigation { get; set; }

    [ForeignKey("EstadoVentaId")]
    public EstadoVenta? EstadoVenta { get; set; }

    [ForeignKey(nameof(IdFormaPago))]
    public virtual FormaPago IdFormaPagoNavigation { get; set; } = null!;

    public virtual ICollection<DetalleVentaProducto> DetalleVentaProductos { get; set; } = new List<DetalleVentaProducto>();
    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();

   
}
