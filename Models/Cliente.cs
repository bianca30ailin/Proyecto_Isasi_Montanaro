using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Cliente
{
    public int DniCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();

    // 🔹 Un cliente puede tener 0..N direcciones
    public virtual ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
}
