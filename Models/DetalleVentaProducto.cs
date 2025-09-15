using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class DetalleVentaProducto
{
    public int IdDetalle { get; set; }

    public int Cantidad { get; set; }

    public double Subtotal { get; set; }

    public int IdNroVenta { get; set; }

    public int IdProducto { get; set; }

    public virtual Ventum IdNroVentaNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
