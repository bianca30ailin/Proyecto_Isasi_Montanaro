using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Envio
{
    public int IdEnvio { get; set; }

    public DateOnly FechaDespacho { get; set; }

    public string NumSeguimiento { get; set; } = null!;

    public double Costo { get; set; }

    public int IdEstado { get; set; }

    public int IdTransporte { get; set; }

    public int IdNroVenta { get; set; }

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Ventum IdNroVentaNavigation { get; set; } = null!;

    public virtual Transporte IdTransporteNavigation { get; set; } = null!;
}
