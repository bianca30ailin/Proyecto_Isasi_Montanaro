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

    public virtual ICollection<DetalleVentaProducto> DetalleVentaProductos { get; set; } = new List<DetalleVentaProducto>();

    public virtual Cliente DniClienteNavigation { get; set; } = null!;

    public virtual ICollection<Envio> Envios { get; set; } = new List<Envio>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public int? EstadoVentaId { get; set; }

    [ForeignKey("EstadoVentaId")]
    public EstadoVenta? EstadoVenta { get; set; }
}
