using System;
using System.Collections.Generic;

namespace OpenPOS.Presentacion.Restaurante
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public List<Producto> Productos { get; set; } = new List<Producto>();
    }

    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public string ImagenPath { get; set; }
        public bool Activo { get; set; } = true;
        public List<OpcionProducto> Opciones { get; set; } = new List<OpcionProducto>();
        public List<ComboItem> Combos { get; set; } = new List<ComboItem>();
        public decimal PrecioConDescuento => Precio;
    }

    public class OpcionProducto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioAdicional { get; set; }
        public bool Disponible { get; set; } = true;
    }

    public class Combo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<ComboItem> Items { get; set; } = new List<ComboItem>();
        public decimal Precio { get; set; }
        public decimal PrecioConDescuento => Precio;
        public List<OfertaCombo> Ofertas { get; set; } = new List<OfertaCombo>();
    }

    public class ComboItem
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; } = 1;
    }

    public class OfertaCombo
    {
        public string Tipo { get; set; } // "2x1", "Descuento", "Gratis"
        public string Valor { get; set; }
        public string Condicion { get; set; }
    }

    public class ItemPedido
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public List<OpcionProducto> OpcionesSeleccionadas { get; set; } = new List<OpcionProducto>();
        public decimal PrecioTotal => PrecioUnitario * Cantidad + ObtenerPrecioOpciones();
        private decimal ObtenerPrecioOpciones()
        {
            decimal total = 0;
            if (OpcionesSeleccionadas != null)
                foreach (var op in OpcionesSeleccionadas)
                    total += op.PrecioAdicional;
            return total;
        }
    }
}