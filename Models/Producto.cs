using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public double Precio { get; set; }

    public int Cantidad { get; set; }

    public int StockMinimo { get; set; }

    public string Baja { get; set; } = null!;

    public int IdCategoria { get; set; }

    public virtual ICollection<DetalleVentaProducto> DetalleVentaProductos { get; set; } = new List<DetalleVentaProducto>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;
}
