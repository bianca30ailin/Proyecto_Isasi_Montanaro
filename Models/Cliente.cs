using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Cliente
{
    public int DniCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? FechaNacimiento { get; set; }

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();

    //  Un cliente puede tener 0..N direcciones
    public virtual ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();

    [NotMapped] // propiedades no mapeadas, no modifica la base de datos
    public string NombreCompleto => $"{Nombre} {Apellido}"; //nombre y apellido del cliente
    public int CantidadVentas => Venta?.Count ?? 0; //cantidad de ventas del cliente
}
