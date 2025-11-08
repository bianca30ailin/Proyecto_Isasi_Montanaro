using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Direccion
{
    public int IdDireccion { get; set; }

    public string NombreCalle { get; set; } = null!;

    public int Altura { get; set; }

    public int IdCiudad { get; set; }

    // FK opcional hacia cliente
    public int? DniCliente { get; set; } //permite 0:N

    public virtual Ciudad IdCiudadNavigation { get; set; } = null!;

    public virtual Cliente? Cliente { get; set; } //hace la relacion oopcionel

    [NotMapped]
    public string DireccionCompleta => $"{NombreCalle} {Altura} - {IdCiudadNavigation.Nombre} (CP {IdCiudadNavigation.CodigoPostal})\";";
}
