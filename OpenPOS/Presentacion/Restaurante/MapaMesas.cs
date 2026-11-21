using OpenPOS.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.Restaurante
{
    public partial class MapaMesas : UserControl
    {
        private List<Mesa> mesas = new List<Mesa>();
        private int anchoMesa = 80;
        private int altoMesa = 60;
        private int margen = 15;

        public MapaMesas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            InitializeMesas();
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

        public void ActualizarEstado(int mesaId, EstadoMesa estado)
        {
            var mesa = mesas.FirstOrDefault(m => m.Id == mesaId);
            if (mesa != null)
            {
                mesa.Estado = estado;
                Invalidate();
            }
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
                    cambiarEstadoMesa(mesa);
                    break;
                }
            }
        }

        private void cambiarEstadoMesa(Mesa mesa)
        {
            EstadoMesa nuevoEstado;
            switch (mesa.Estado)
            {
                case EstadoMesa.Libre: nuevoEstado = EstadoMesa.Ocupada; break;
                case EstadoMesa.Ocupada: nuevoEstado = EstadoMesa.Limpieza; break;
                case EstadoMesa.Limpieza: nuevoEstado = EstadoMesa.Libre; break;
                default: nuevoEstado = EstadoMesa.Libre; break;
            }

            ActualizarEstado(mesa.Id, nuevoEstado);

            if (EstadoCambiado != null)
                EstadoCambiado(this, new EstadoMesaEventArgs(mesa.Id, nuevoEstado));
        }

        public event EventHandler<EstadoMesaEventArgs> EstadoCambiado;
    }

    public class EstadoMesaEventArgs : EventArgs
    {
        public int MesaId { get; }
        public EstadoMesa NuevoEstado { get; }

        public EstadoMesaEventArgs(int mesaId, EstadoMesa nuevoEstado)
        {
            MesaId = mesaId;
            NuevoEstado = nuevoEstado;
        }
    }
}