using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OpenPOS.Logging;

namespace OpenPOS.UI
{
    /// <summary>
    /// Consola de instalación de openPOS. Muestra la lista de pasos con su estado
    /// y su tiempo, una barra de progreso y un registro de actividad en vivo.
    /// Es segura para usarse desde hilos en segundo plano: todos los métodos
    /// marshalizan automáticamente a la interfaz.
    /// </summary>
    internal sealed class InstallConsole : UserControl
    {
        private static readonly Color Ink = Color.FromArgb(11, 16, 32);
        private static readonly Color Header = Color.FromArgb(15, 22, 44);
        private static readonly Color Card = Color.FromArgb(22, 32, 60);
        private static readonly Color Line = Color.FromArgb(38, 54, 94);
        private static readonly Color Txt = Color.FromArgb(230, 236, 247);
        private static readonly Color Muted = Color.FromArgb(143, 160, 196);
        private static readonly Color LogText = Color.FromArgb(198, 210, 232);

        private readonly Label _titulo;
        private readonly Label _estado;
        private readonly TableLayoutPanel _pasos;
        private readonly BarraProgreso _barra;
        private readonly RichTextBox _log;
        private readonly Label _reloj;
        private readonly Timer _tick;
        private readonly Stopwatch _cronometro = new Stopwatch();

        private Label[] _glifo;
        private Label[] _tiempo;
        private string[] _nombres;
        private DateTime[] _inicio;
        private DateTime[] _fin;
        private bool[] _activo;
        private int _total;

        public InstallConsole()
        {
            BackColor = Ink;
            DoubleBuffered = true;

            // ---- Encabezado ----
            var header = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Header };
            var acento = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = Theme.Accent };
            _titulo = new Label
            {
                Text = "Instalación del Servidor",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(46, 18),
                BackColor = Color.Transparent
            };
            _estado = new Label
            {
                Text = "Iniciando…",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 10.5f),
                AutoSize = true,
                Location = new Point(48, 52),
                BackColor = Color.Transparent
            };
            header.Controls.Add(acento);
            header.Controls.Add(_titulo);
            header.Controls.Add(_estado);

            // ---- Pie ----
            var pie = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Header };
            var ruta = new Label
            {
                Text = "Registro guardado en " + Logger.LogDirectory,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(24, 17)
            };
            var copiar = new Button
            {
                Text = "Copiar registro",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Txt,
                BackColor = Card,
                Size = new Size(150, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            copiar.FlatAppearance.BorderColor = Line;
            copiar.Click += (s, e) =>
            {
                try { if (_log.TextLength > 0) Clipboard.SetText(_log.Text); }
                catch { }
            };
            pie.Controls.Add(ruta);
            pie.Controls.Add(copiar);
            pie.Resize += (s, e) => copiar.Location = new Point(pie.Width - copiar.Width - 24, 10);

            // ---- Cuerpo ----
            var cuerpo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Ink
            };
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));

            var pasosHost = new Panel { Dock = DockStyle.Fill, BackColor = Ink, Padding = new Padding(24, 14, 10, 14) };
            var pasosScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Ink };
            _pasos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Ink
            };
            _pasos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            pasosScroll.Controls.Add(_pasos);
            var pasosTitulo = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = "PASOS DEL PROCESO",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Ink
            };
            pasosHost.Controls.Add(pasosScroll);
            pasosHost.Controls.Add(pasosTitulo);

            var consolaHost = new Panel { Dock = DockStyle.Fill, BackColor = Ink, Padding = new Padding(10, 14, 24, 14) };
            var cab = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Ink };
            var logTitulo = new Label
            {
                Dock = DockStyle.Left,
                Width = 260,
                Text = "REGISTRO DE ACTIVIDAD",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _reloj = new Label
            {
                Dock = DockStyle.Right,
                Width = 130,
                Text = "00:00:00",
                ForeColor = Theme.Accent,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            cab.Controls.Add(logTitulo);
            cab.Controls.Add(_reloj);

            var barraWrap = new Panel { Dock = DockStyle.Top, Height = 24, BackColor = Ink, Padding = new Padding(0, 7, 0, 7) };
            _barra = new BarraProgreso { Dock = DockStyle.Fill };
            barraWrap.Controls.Add(_barra);

            _log = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 12, 26),
                ForeColor = LogText,
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };

            consolaHost.Controls.Add(_log);
            consolaHost.Controls.Add(barraWrap);
            consolaHost.Controls.Add(cab);

            cuerpo.Controls.Add(pasosHost, 0, 0);
            cuerpo.Controls.Add(consolaHost, 1, 0);

            Controls.Add(cuerpo);
            Controls.Add(pie);
            Controls.Add(header);

            _tick = new Timer { Interval = 100 };
            _tick.Tick += (s, e) => ActualizarTiempos();
        }

        public string TextoLog { get { return _log.Text; } }

        // ---- API pública (segura entre hilos) ------------------------------

        public void Iniciar(string subtitulo)
        {
            _cronometro.Restart();
            try { _tick.Start(); } catch { }
            SetEstado(subtitulo);
        }

        public void Detener()
        {
            try { _tick.Stop(); } catch { }
            _cronometro.Stop();
        }

        public void DefinirPasos(params string[] pasos)
        {
            if (pasos == null) pasos = new string[0];
            Ui(() =>
            {
                _pasos.SuspendLayout();
                _pasos.Controls.Clear();
                _pasos.RowStyles.Clear();
                _pasos.RowCount = pasos.Length;
                _total = pasos.Length;
                _glifo = new Label[_total];
                _tiempo = new Label[_total];
                _nombres = new string[_total];
                _inicio = new DateTime[_total];
                _fin = new DateTime[_total];
                _activo = new bool[_total];

                for (int i = 0; i < _total; i++)
                {
                    _pasos.RowStyles.Add(new RowStyle(SizeType.Absolute, 62f));
                    _nombres[i] = pasos[i];
                    _pasos.Controls.Add(CrearFila(i, pasos[i]), 0, i);
                }
                _pasos.ResumeLayout();
            });
        }

        public void PasoEnProceso(int i)
        {
            Ui(() =>
            {
                if (!Valido(i)) return;
                _glifo[i].Text = "▶";
                _glifo[i].ForeColor = Theme.Accent;
                _inicio[i] = DateTime.Now;
                _activo[i] = true;
                AgregarLinea("STEP", "Inicio: " + _nombres[i]);
            });
        }

        public void PasoCompletado(int i)
        {
            Ui(() =>
            {
                if (!Valido(i)) return;
                _activo[i] = false;
                _fin[i] = DateTime.Now;
                _glifo[i].Text = "✓";
                _glifo[i].ForeColor = Theme.Success;
                _tiempo[i].Text = Formato(_fin[i] - _inicio[i]);
                AgregarLinea("OK", "Listo: " + _nombres[i] + " (" + _tiempo[i].Text + ")");
            });
        }

        public void PasoFallo(int i, string detalle)
        {
            Ui(() =>
            {
                if (!Valido(i)) return;
                _activo[i] = false;
                _fin[i] = DateTime.Now;
                _glifo[i].Text = "✕";
                _glifo[i].ForeColor = Theme.Danger;
                _tiempo[i].Text = Formato(_fin[i] - _inicio[i]);
                AgregarLinea("ERROR", "Fallo en: " + _nombres[i] + (string.IsNullOrEmpty(detalle) ? "" : " — " + detalle));
            });
        }

        public void SetEstado(string texto)
        {
            Ui(() => { _estado.Text = texto; });
        }

        public void SetProgreso(int valor)
        {
            Ui(() => { _barra.Valor = valor; });
        }

        public void Info(string mensaje) { Log("INFO", mensaje); }
        public void Exito(string mensaje) { Log("OK", mensaje); }
        public void Aviso(string mensaje) { Log("WARN", mensaje); }
        public void ErrorLog(string mensaje) { Log("ERROR", mensaje); }

        public void Log(string nivel, string mensaje)
        {
            try
            {
                switch (nivel)
                {
                    case "OK": Logger.Info(mensaje); break;
                    case "WARN": Logger.Warn(mensaje); break;
                    case "ERROR": Logger.Error(mensaje); break;
                    default: Logger.Info(mensaje); break;
                }
            }
            catch { }
            Ui(() => AgregarLinea(nivel, mensaje));
        }

        // ---- Interno -------------------------------------------------------

        private Panel CrearFila(int i, string texto)
        {
            var fila = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(14, 0, 14, 0)
            };
            var g = new Label
            {
                Dock = DockStyle.Left,
                Width = 30,
                Text = "○",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            var t = new Label
            {
                Dock = DockStyle.Right,
                Width = 66,
                Text = "",
                ForeColor = Muted,
                Font = new Font("Consolas", 9f),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            var x = new Label
            {
                Dock = DockStyle.Fill,
                Text = texto,
                ForeColor = Txt,
                Font = new Font("Segoe UI", 10.5f),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            fila.Controls.Add(x);
            fila.Controls.Add(t);
            fila.Controls.Add(g);
            _glifo[i] = g;
            _tiempo[i] = t;
            return fila;
        }

        private bool Valido(int i)
        {
            return _glifo != null && i >= 0 && i < _total;
        }

        private void ActualizarTiempos()
        {
            if (!IsHandleCreated) return;
            _reloj.Text = _cronometro.Elapsed.ToString(@"hh\:mm\:ss");
            if (_tiempo == null) return;
            for (int i = 0; i < _total; i++)
            {
                if (_activo[i])
                {
                    _tiempo[i].Text = Formato(DateTime.Now - _inicio[i]);
                }
            }
        }

        private void AgregarLinea(string nivel, string mensaje)
        {
            if (!IsHandleCreated) return;

            Color color;
            string marca;
            switch (nivel)
            {
                case "OK": color = Theme.Success; marca = "OK"; break;
                case "WARN": color = Theme.Warning; marca = "!!"; break;
                case "ERROR": color = Theme.Danger; marca = "XX"; break;
                case "STEP": color = Theme.Accent; marca = ">."; break;
                default: color = LogText; marca = ".."; break;
            }

            _log.SelectionStart = _log.TextLength;
            _log.SelectionLength = 0;
            _log.SelectionColor = Muted;
            _log.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  ");
            _log.SelectionColor = color;
            _log.AppendText(marca + "  " + mensaje + Environment.NewLine);
            _log.SelectionColor = _log.ForeColor;
            _log.SelectionStart = _log.TextLength;
            _log.ScrollToCaret();
        }

        private void Ui(Action accion)
        {
            if (accion == null) return;
            try
            {
                if (!IsHandleCreated) return;
                if (InvokeRequired) BeginInvoke(accion);
                else accion();
            }
            catch
            {
                // la consola nunca debe romper el flujo de instalación
            }
        }

        private static string Formato(TimeSpan t)
        {
            if (t.TotalHours >= 1) return string.Format("{0}h {1:00}m", (int)t.TotalHours, t.Minutes);
            if (t.TotalMinutes >= 1) return string.Format("{0}m {1:00}s", (int)t.TotalMinutes, t.Seconds);
            return string.Format("{0:0.0}s", t.TotalSeconds);
        }
    }

    /// <summary>Barra de progreso plana con esquinas redondeadas.</summary>
    internal sealed class BarraProgreso : Control
    {
        private int _valor;

        public BarraProgreso()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(11, 16, 32);
        }

        public int Valor
        {
            get { return _valor; }
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (v > 100) v = 100;
                if (v == _valor) return;
                _valor = v;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            const int alto = 10;
            var pista = new Rectangle(0, (Height - alto) / 2, Width - 1, alto);
            if (pista.Width <= 1) return;

            using (var path = Redondo(pista, alto / 2))
            using (var b = new SolidBrush(Color.FromArgb(30, 42, 74)))
                e.Graphics.FillPath(b, path);

            int ancho = (int)(pista.Width * (_valor / 100.0));
            if (ancho >= alto)
            {
                var relleno = new Rectangle(0, pista.Y, ancho, alto);
                using (var path = Redondo(relleno, alto / 2))
                using (var b = new LinearGradientBrush(relleno, Theme.Accent, Theme.Primary, LinearGradientMode.Horizontal))
                    e.Graphics.FillPath(b, path);
            }
        }

        private static GraphicsPath Redondo(Rectangle r, int radio)
        {
            int d = radio * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
