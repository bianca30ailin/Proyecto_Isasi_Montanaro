using System;
using System.Collections.Generic;
using System.Linq; 


namespace Proyecto_Isasi_Montanaro.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public int Dni { get; set; } 

    public string Email { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Baja { get; set; } = null!;

    //Propiedad de navegacion. Relacion uno a muchos. Un usuario puede registrar muchas ventas
    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();

    //Propiedad de navegacion. Relacion muchos a muchos. Un usuario puede tener muchos tipos de usuarios
    //Virtual permite la carga diferida (lazy loading) de la propiedad de navegacion.
    //Cuando se accede a la propiedad, EF Core carga automaticamente los datos relacionados desde la bd.
    public virtual ICollection<TipoUsuario> IdTipoUsuarios { get; set; } = new List<TipoUsuario>();

    public string PerfilesAsignados
    {
        get
        {
            // Verifica si la colección de perfiles no es nula y contiene elementos
            if (IdTipoUsuarios != null && IdTipoUsuarios.Any())
            {
                // Combina los nombres de los perfiles con una coma y un espacio
                return string.Join(", ", IdTipoUsuarios.Select(p => p.Tipo));
            }
            // Si no tiene perfiles, retorna un texto
            return "Sin perfiles";
        }
    }

}
