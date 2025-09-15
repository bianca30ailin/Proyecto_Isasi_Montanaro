using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Direccion
{
    public int IdDireccion { get; set; }

    public string NombreCalle { get; set; } = null!;

    public int Altura { get; set; }

    public int CodigoPostal { get; set; }

    public int IdCiudad { get; set; }

    public virtual Ciudad IdCiudadNavigation { get; set; } = null!;

    public virtual ICollection<Cliente> DniClientes { get; set; } = new List<Cliente>();
}
