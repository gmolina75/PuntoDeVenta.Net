using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Ada369Csharp.Reportes
{
    public static class ReportPainter
    {
        public static void PrintGrid(ReportModel report, string printerName)
        {
            var documento = new PrintDocument();
            if (!string.IsNullOrEmpty(printerName))
            {
                documento.PrinterSettings.PrinterName = printerName;
            }

            var columnas = Columnas(report);
            int indiceFila = 0;

            documento.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                var fuenteTitulo = new Font("Segoe UI", 13, FontStyle.Bold);
                var fuenteCabecera = new Font("Segoe UI", 8, FontStyle.Bold);
                var fuenteCelda = new Font("Segoe UI", 8);
                int margen = 40;
                float ancho = e.MarginBounds.Width;
                float y = margen;

                if (!string.IsNullOrEmpty(report.Titulo))
                {
                    g.DrawString(report.Titulo, fuenteTitulo, Brushes.Black, margen, y);
                    y += fuenteTitulo.GetHeight(g) + 4;
                }
                if (!string.IsNullOrEmpty(report.Empresa))
                {
                    g.DrawString(report.Empresa, fuenteCelda, Brushes.DimGray, margen, y);
                    y += fuenteCelda.GetHeight(g) + 8;
                }

                if (columnas.Count == 0)
                {
                    g.DrawString("Sin columnas para mostrar.", fuenteCelda, Brushes.Black, margen, y);
                    return;
                }

                float anchoColumna = ancho / columnas.Count;
                float altoFila = fuenteCelda.GetHeight(g) + 4;

                for (int c = 0; c < columnas.Count; c++)
                {
                    g.FillRectangle(Brushes.Gainsboro, margen + c * anchoColumna, y, anchoColumna, altoFila);
                    g.DrawRectangle(Pens.Gray, margen + c * anchoColumna, y, anchoColumna, altoFila);
                    g.DrawString(columnas[c].Header, fuenteCabecera, Brushes.Black,
                        new RectangleF(margen + c * anchoColumna + 2, y + 1, anchoColumna - 4, altoFila), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                }
                y += altoFila;

                DataTable tabla = report.DataSource;
                while (indiceFila < tabla.Rows.Count)
                {
                    if (y + altoFila > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    DataRow fila = tabla.Rows[indiceFila];
                    for (int c = 0; c < columnas.Count; c++)
                    {
                        string valor = Valor(fila, columnas[c].Field);
                        g.DrawRectangle(Pens.LightGray, margen + c * anchoColumna, y, anchoColumna, altoFila);
                        g.DrawString(valor, fuenteCelda, Brushes.Black,
                            new RectangleF(margen + c * anchoColumna + 2, y + 1, anchoColumna - 4, altoFila), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                    }
                    y += altoFila;
                    indiceFila++;
                }

                e.HasMorePages = false;
            };

            using (var dialogo = new PrintPreviewDialog { Document = documento, WindowState = FormWindowState.Maximized })
            {
                dialogo.ShowDialog();
            }
        }

        public static void PrintTicket(ReportModel report, string printerName)
        {
            var documento = new PrintDocument();
            if (!string.IsNullOrEmpty(printerName))
            {
                documento.PrinterSettings.PrinterName = printerName;
            }

            DataTable tabla = report.DataSource;
            int indiceFila = 0;

            documento.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                var normal = new Font("Consolas", 8);
                var negrita = new Font("Consolas", 9, FontStyle.Bold);
                var grande = new Font("Consolas", 11, FontStyle.Bold);
                float margen = 10;
                float ancho = e.MarginBounds.Width;
                float derecho = margen + ancho;
                float y = margen;

                if (tabla == null || tabla.Rows.Count == 0)
                {
                    e.HasMorePages = false;
                    return;
                }

                DataRow primera = tabla.Rows[0];

                if (indiceFila == 0)
                {
                    y = Centrar(g, Valor(primera, "Empresa"), grande, ancho, margen, y);
                    y = Centrar(g, "RUC: " + Valor(primera, "Identificador_fiscal"), normal, ancho, margen, y);
                    y = Centrar(g, Valor(primera, "Direccion"), normal, ancho, margen, y);
                    y += 4;
                    y = Centrar(g, Valor(primera, "tipodoc") + " " + Valor(primera, "Numero_de_doc"), negrita, ancho, margen, y);
                    y += 4;
                    g.DrawString("Fecha : " + Valor(primera, "fecha"), normal, Brushes.Black, margen, y); y += normal.GetHeight(g) + 1;
                    g.DrawString("Cliente: " + Valor(primera, "Nombre"), normal, Brushes.Black, margen, y); y += normal.GetHeight(g) + 1;
                    g.DrawString("Cajero : " + Valor(primera, "Usuario"), normal, Brushes.Black, margen, y); y += normal.GetHeight(g) + 2;
                    g.DrawString(new string('-', 62), normal, Brushes.Black, margen, y); y += normal.GetHeight(g);
                    g.DrawString("Cant  Producto", negrita, Brushes.Black, margen, y);
                    g.DrawString("Importe", negrita, Brushes.Black, derecho - g.MeasureString("Importe", negrita).Width, y);
                    y += negrita.GetHeight(g);
                    g.DrawString(new string('-', 62), normal, Brushes.Black, margen, y); y += normal.GetHeight(g);
                }

                while (indiceFila < tabla.Rows.Count)
                {
                    if (y + normal.GetHeight(g) > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    DataRow fila = tabla.Rows[indiceFila];
                    string cant = Valor(fila, "Cant");
                    string producto = Valor(fila, "Producto");
                    string importe = Valor(fila, "Importe");
                    g.DrawString(cant, normal, Brushes.Black, margen, y);
                    g.DrawString(producto, normal, Brushes.Black, margen + 40, y);
                    g.DrawString(importe, normal, Brushes.Black, derecho - g.MeasureString(importe, normal).Width, y);
                    y += normal.GetHeight(g);
                    indiceFila++;
                }

                g.DrawString(new string('-', 62), normal, Brushes.Black, margen, y); y += normal.GetHeight(g) + 2;
                y = Derecha(g, "IGV: " + Valor(primera, "Moneda") + " " + Valor(primera, "Subtotal_Impuesto"), normal, derecho, y);
                y = Derecha(g, "TOTAL: " + Valor(primera, "Moneda") + " " + Valor(primera, "Monto_total"), negrita, derecho, y);
                y += 2;
                string letras = Valor(primera, "Totalletras");
                if (!string.IsNullOrEmpty(letras))
                {
                    y = Centrar(g, letras, normal, ancho, margen, y);
                }
                y += 4;
                y = Centrar(g, Valor(primera, "Agradecimiento"), normal, ancho, margen, y);
                y = Centrar(g, Valor(primera, "pagina_Web_Facebook"), normal, ancho, margen, y);
                y = Centrar(g, Valor(primera, "Anuncio"), normal, ancho, margen, y);
                y = Centrar(g, Valor(primera, "Datos_fiscales_de_autorizacion"), normal, ancho, margen, y);

                e.HasMorePages = false;
            };

            try
            {
                documento.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo imprimir el comprobante: " + ex.Message, "Impresión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static float Centrar(Graphics g, string texto, Font fuente, float ancho, float margen, float y)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return y;
            }
            float x = margen + (ancho - g.MeasureString(texto, fuente).Width) / 2f;
            g.DrawString(texto, fuente, Brushes.Black, Math.Max(margen, x), y);
            return y + fuente.GetHeight(g);
        }

        private static float Derecha(Graphics g, string texto, Font fuente, float derecho, float y)
        {
            float x = derecho - g.MeasureString(texto, fuente).Width;
            g.DrawString(texto, fuente, Brushes.Black, x, y);
            return y + fuente.GetHeight(g);
        }

        private static List<ReportColumn> Columnas(ReportModel report)
        {
            if (report.Columnas != null && report.Columnas.Count > 0)
            {
                return report.Columnas;
            }

            var columnas = new List<ReportColumn>();
            if (report.DataSource != null)
            {
                foreach (DataColumn col in report.DataSource.Columns)
                {
                    if (col.DataType != typeof(byte[]))
                    {
                        columnas.Add(new ReportColumn(col.ColumnName, col.ColumnName));
                    }
                }
            }
            return columnas;
        }

        private static string Valor(DataRow fila, string campo)
        {
            if (fila == null || string.IsNullOrEmpty(campo) || !fila.Table.Columns.Contains(campo))
            {
                return "";
            }
            object valor = fila[campo];
            if (valor == null || valor == DBNull.Value)
            {
                return "";
            }
            if (valor is decimal || valor is double || valor is float)
            {
                return Convert.ToDecimal(valor).ToString("N2");
            }
            if (valor is DateTime)
            {
                return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
            }
            return valor.ToString();
        }
    }
}
