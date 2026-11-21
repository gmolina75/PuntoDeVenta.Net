using OpenPOS.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.Restaurante
{
    public partial class PanelMesas : UserControl
    {
        private List<Mesa> mesas = new List<Mesa>();
        private List<Categoria> categorias = new List<Categoria>();
        private int anchoMesa = 80;
        private int altoMesa = 60;
        private int margen = 15;

        public PanelMesas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeMesas();
            InitializeCategorias();
        }

        private void InitializeMesas()
        {
            for (int i = 1; i <= 20; i++)
            {
                mesas.Add(new Mesa
                {
                    Id = i,
                    Nombre = $"Mesa {i}",
                    Capacidad = 4,
                    Estado = EstadoMesa.Libre
                });
            }
            Invalidate();
        }

        private void InitializeCategorias()
        {
            var cierre = new Categoria { Id = 1, Nombre = "CIERRE" };
            var categoria1 = new Categoria { Id = 2, Nombre = "Bebidas" };
            var categoria2 = new Categoria { Id = 3, Nombre = "Comida" };
            var categoria3 = new Categoria { Id = 4, Nombre = "Postres" };
            
            categoria1.Productos.Add(new Producto { Id = 101, Nombre = "Agua Mineral", Precio = 1.50m });
            categoria1.Productos.Add(new Producto { Id = 102, Nombre = "Refresco", Precio = 2.50m });
            
            categoria2.Productos.Add(new Producto { Id = 201, Nombre = "Hamburguesa", Precio = 15.00m });
            categoria2.Productos.Add(new Producto { Id = 202, Nombre = "Papas Fritas", Precio = 5.00m });
            
            categoria3.Productos.Add(new Producto { Id = 301, Nombre = "Gelatina", Precio = 4.00m });
            
            categorias.Add(cierre);
            categorias.Add(categoria1);
            categorias.Add(categoria2);
            categorias.Add(categoria3);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            dibujarMesas(e.Graphics);
        }

        private void dibujarMesas(Graphics g)
        {
            int filas = 4;
            int columnas = 5;

            for (int i = 0; i < mesas.Count; i++)
            {
                int fila = i / columnas;
                int columna = i % columnas;

                int x = margen + columna * (anchoMesa + margen);
                int y = margen + fila * (altoMesa + margen);

                Mesa mesa = mesas[i];
                Color color = obtenerColorEstado(mesa.Estado);

                using (Brush brush = new SolidBrush(color))
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.FillRectangle(brush, x, y, anchoMesa, altoMesa);
                    g.DrawRectangle(pen, x, y, anchoMesa, altoMesa);

                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(mesa.Nombre, Font, Brushes.White, new RectangleF(x, y, anchoMesa, altoMesa), sf);
                }
            }
        }

        private Color obtenerColorEstado(EstadoMesa estado)
        {
            switch (estado)
            {
                case EstadoMesa.Libre: return Theme.Surface;
                case EstadoMesa.Ocupada: return Theme.Primary;
                case EstadoMesa.Reservada: return Color.Gold;
                case EstadoMesa.Limpieza: return Color.LightGray;
                default: return Theme.Surface;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int columnas = 5;
            for (int i = 0; i < mesas.Count; i++)
            {
                int fila = i / columnas;
                int columna = i % columnas;

                int x = margen + columna * (anchoMesa + margen);
                int y = margen + fila * (altoMesa + margen);

                if (e.X >= x && e.X <= x + anchoMesa && e.Y >= y && e.Y <= y + altoMesa)
                {
                    Mesa mesa = mesas[i];
                    if (mesa.Estado == EstadoMesa.Libre || mesa.Estado == EstadoMesa.Limpieza)
                    {
                        abrirVentaMesa(mesa);
                    }
                    break;
                }
            }
        }

        private void abrirVentaMesa(Mesa mesa)
        {
            // Cambiar a estado ocupado
            ActualizarEstado(mesa.Id, EstadoMesa.Ocupada);
            
            // Mostrar ventana de pedidos (simulado)
            var frmPedido = new Form();
            frmPedido.Text = $"Pedido - {mesa.Nombre}";
            frmPedido.Size = new Size(600, 400);
            frmPedido.StartPosition = FormStartPosition.CenterParent;
            
            // Grid de productos
            var dgv = new DataGridView();
            dgv.Dock = DockStyle.Top;
            dgv.Height = 200;
            dgv.ColumnCount = 3;
            dgv.Columns[0].Name = "Producto";
            dgv.Columns[1].Name = "Precio";
            dgv.Columns[2].Name = "Cantidad";
            
            frmPedido.Controls.Add(dgv);
            frmPedido.ShowDialog();
        }

        public void ActualizarEstado(int mesaId, EstadoMesa estado)
        {
            var mesa = mesas.FirstOrDefault(m => m.Id == mesaId);
            if (mesa != null)
            {
                mesa.Estado = estado;
                mesa.HoraUltima = DateTime.Now;
                Invalidate();
            }
        }
    }
}