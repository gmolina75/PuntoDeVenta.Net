using OpenPOS.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.Restaurante
{
    public partial class ProgramaFidelizacion : UserControl
    {
        private int puntosAcumulados = 0;
        private int objetivoPuntos = 100;
        private string codigoCliente = string.Empty;

        public ProgramaFidelizacion()
        {
        }

        public void RegistrarCompra(decimal monto, string cliente = null)
        {
            int puntosGanados = (int)(monto / 10); // 1 punto por cada $10
            puntosAcumulados += puntosGanados;
            
            if (cliente != null)
                codigoCliente = cliente;
        }

        public bool CanjearPremio(int puntosNecesarios)
        {
            if (puntosAcumulados >= puntosNecesarios)
            {
                puntosAcumulados -= puntosNecesarios;
                return true;
            }
            return false;
        }

        public void MostrarDialogoFidelidad(string nombreCliente)
        {
            var lblNombre = new Label { Text = $"Cliente: {nombreCliente}", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 20) };
            var lblPuntos = new Label { Text = $"Puntos: {puntosAcumulados}/{objetivoPuntos}", Font = new Font("Segoe UI", 10), Location = new Point(20, 60) };
            var pb = new ProgressBar { Size = new Size(300, 25), Location = new Point(20, 100), Maximum = objetivoPuntos, Value = puntosAcumulados };
            
            var btnCanjear = new Button { Text = "Canjear Premio", Location = new Point(20, 150), Size = new Size(120, 35) };
            
            var frm = new Form();
            frm.Text = "Fidelización";
            frm.Size = new Size(350, 220);
            frm.Controls.AddRange(new Control[] { lblNombre, lblPuntos, pb, btnCanjear });
            
            btnCanjear.Click += (s, e) => {
                if (CanjearPremio(objetivoPuntos))
                {
                    MessageBox.Show("¡Premio canjeado! Gracias por tu compra.");
                    lblPuntos.Text = $"Puntos: {puntosAcumulados}/{objetivoPuntos}";
                    pb.Value = puntosAcumulados;
                }
                else
                {
                    MessageBox.Show($"Necesitas {objetivoPuntos - puntosAcumulados} puntos más para canjear.");
                }
            };
            
            frm.ShowDialog();
        }
    }
}