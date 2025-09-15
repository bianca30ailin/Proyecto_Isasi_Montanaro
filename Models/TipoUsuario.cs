using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class TipoUsuario
{
    public int IdTipoUsuario { get; set; }

    public string Tipo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}
