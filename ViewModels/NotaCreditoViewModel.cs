using Proyecto_Isasi_Montanaro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Isasi_Montanaro.ViewModels
{
    public class NotaCreditoViewModel
    {
        private readonly ProyectoTallerContext _context;

        public NotaCreditoViewModel(ProyectoTallerContext context)
        {
            _context = context;
            NotasCredito = new ObservableCollection<NotaCredito>(
                _context.NotaCredito
                        .Include(nc => nc.Venta)
                        .OrderByDescending(nc => nc.Fecha)
                        .ToList());
        }

        public ObservableCollection<NotaCredito> NotasCredito { get; set; }

        /// <summary>
        /// Crea una nueva nota de crédito vinculada a una venta.
        /// </summary>
        public NotaCredito CrearNotaCredito(Ventum venta, string motivo)
        {
            var nota = new NotaCredito
            {
                IdNroVenta = venta.IdNroVenta,
                Fecha = DateTime.Now,
                Motivo = motivo,
                Total = 0
            };

            _context.NotaCredito.Add(nota);
            _context.SaveChanges();
            NotasCredito.Add(nota);

            return nota;
        }

        /// <summary>
        /// Genera los detalles de la nota crédito y devuelve el stock.
        /// </summary>
        public void AgregarDetallesYActualizarStock(NotaCredito nota, Ventum venta)
        {
            // 1️ Obtenemos los detalles de la venta
            var detallesVenta = _context.DetalleVentaProductos
                .Where(d => d.IdNroVenta == venta.IdNroVenta)
                .ToList();

            decimal total = 0;

            foreach (var det in detallesVenta)
            {
                // 2️ Traemos el producto directamente desde la BD
                var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == det.IdProducto);
                if (producto != null)
                {
                    producto.Cantidad += det.Cantidad; // devolvemos al stock
                }

                // 3️ Creamos el detalle de la nota de crédito
                var detalleNC = new DetalleNotaCredito
                {
                    IdNotaCredito = nota.IdNotaCredito,
                    IdProducto = det.IdProducto,
                    Cantidad = det.Cantidad,
                    Subtotal = (decimal)det.Subtotal
                };

                _context.DetalleNotaCredito.Add(detalleNC);
                total += detalleNC.Subtotal;
            }

            // 4️ Actualizamos el total de la nota
            nota.Total = total;
            _context.SaveChanges();
        }

    }
}
