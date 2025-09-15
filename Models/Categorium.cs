using System;
using System.Collections.Generic;

namespace Proyecto_Isasi_Montanaro.Models;

public partial class Categorium
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    //propiedad de navegación. Relacion uno a muchos. Una categoria puede tener muchos productos
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    //virual: permite la carga diferida (lazy loading) de la propiedad de navegación.
    //cuando se accede a la propiedad, EF Core carga automaticamente los datos relacionados desde la bd si no hayn sido cargados antes.
    //Icollection : representa una coleccion de objetos que se pueden enumerar, agregar y eliminar.
    //Indica que la categoria puede tener multiples productos asociados y permite acceder a ellos de manera sencilla.
}
