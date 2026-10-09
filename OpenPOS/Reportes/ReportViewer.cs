using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPOS.Reportes
{
    public class ReportViewer : UserControl
    {
        private readonly Panel _panelCabecera;
        private readonly PictureBox _logo;
        private readonly Label _titulo;
        private readonly Label _subtitulo;
        private readonly ToolStrip _barra;
        private readonly DataGridView _grid;

        public ReportModel Report { get; set; }

        public ReportModel ReportSource
        {
            get { return Report; }
        }

        public ReportViewer()
        {
            BackColor = Color.White;

            _panelCabecera = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
            _logo = new PictureBox { Dock = DockStyle.Left, Width = 90, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
            _titulo = new Label { Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter, Text = "" };
            _subtitulo = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleCenter, Text = "" };
            _panelCabecera.Controls.Add(_subtitulo);
            _panelCabecera.Controls.Add(_titulo);
            _panelCabecera.Controls.Add(_logo);

            _barra = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.System };
            var btnImprimir = new ToolStripButton("Imprimir") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnImprimir.Click += (s, e) => Imprimir();
            var btnActualizar = new ToolStripButton("Actualizar") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btnActualizar.Click += (s, e) => RefreshReport();
            _barra.Items.Add(btnImprimir);
            _barra.Items.Add(btnActualizar);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false
            };
            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(39, 39, 39),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };
            _grid.DefaultCellStyle.SelectionBackColor = Color.Gainsboro;
            _grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            Controls.Add(_grid);
            Controls.Add(_panelCabecera);
        }

        public void RefreshReport()
        {
            if (Report == null)
            {
                return;
            }

            _titulo.Text = Report.Titulo ?? "";
            string subtitulo = Report.Empresa;
            if (Report.Logo != null)
            {
                try
                {
                    using (var ms = new System.IO.MemoryStream(Report.Logo))
                    {
                        _logo.Image = Image.FromStream(ms);
                    }
                }
                catch { _logo.Image = null; }
            }

            DataTable origen = Report.DataSource;
            if (origen == null)
            {
                _grid.DataSource = null;
                return;
            }

            _grid.DataSource = Proyectar(origen);
            _grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private DataTable Proyectar(DataTable origen)
        {
            if (Report.Columnas == null || Report.Columnas.Count == 0)
            {
                return origen;
            }

            var vista = new DataTable();
            var campos = new System.Collections.Generic.List<string>();
            foreach (var col in Report.Columnas)
            {
                if (origen.Columns.Contains(col.Field))
                {
                    vista.Columns.Add(col.Header, origen.Columns[col.Field].DataType);
                    campos.Add(col.Field);
                }
            }

            foreach (DataRow fila in origen.Rows)
            {
                DataRow nueva = vista.NewRow();
                for (int i = 0; i < campos.Count; i++)
                {
                    nueva[i] = fila[campos[i]];
                }
                vista.Rows.Add(nueva);
            }

            return vista;
        }

        private void Imprimir()
        {
            if (Report == null || Report.DataSource == null)
            {
                MessageBox.Show("No hay datos para imprimir.", "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialogo = new PrintDialog())
            {
                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    ReportPainter.PrintGrid(Report, dialogo.PrinterSettings.PrinterName);
                }
            }
        }
    }
}
