using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPOS.UI
{
    /// <summary>
    /// Global look-and-feel layer. Applies a single, consistent openPOS visual
    /// system to every form/control at runtime, without touching the designers.
    ///
    /// Design language (v2):
    ///   - Tulipa "ébano + tarjetas": shell y barras en ébano, contenido como
    ///     tarjetas claras sobre el lienzo oscuro (estilo Linear/Stripe).
    ///   - Un acento funcional (#08B6C9) sobre el lienzo; acciones en índigo.
    ///   - Jerarquía por peso y color, no por líneas ni sombras duras.
    ///
    /// Conservative by design: it never overrides custom skins (buttons with an
    /// Image/BackgroundImage), monospace fonts (SQL/code boxes) or transparent
    /// backgrounds.
    /// </summary>
    internal static class Theme
    {
        // ---- Brand palette -------------------------------------------------
        public static readonly Color Primary = Color.FromArgb(27, 46, 140);      // #1B2E8C
        public static readonly Color PrimaryDark = Color.FromArgb(18, 32, 102);
        public static readonly Color Accent = Color.FromArgb(8, 182, 201);       // #08B6C9
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(246, 248, 252);
        public static readonly Color GridAltRow = Color.FromArgb(245, 247, 251);
        public static readonly Color GridSelection = Color.FromArgb(8, 182, 201);
        public static readonly Color GridHover = Color.FromArgb(232, 248, 250);
        public static readonly Color Border = Color.FromArgb(214, 220, 230);
        public static readonly Color Text = Color.FromArgb(33, 37, 41);
        public static readonly Color Muted = Color.FromArgb(70, 81, 95);
        public static readonly Color Success = Color.FromArgb(26, 168, 90);      // #1AA85A
        public static readonly Color Warning = Color.FromArgb(245, 166, 11);     // #F5A60B
        public static readonly Color Danger = Color.FromArgb(224, 60, 60);       // #E03C3C

        // ---- Shell (ébano) -------------------------------------------------
        /// <summary>Lienzo principal oscuro (#0B0B0E, igual que el LOGIN).</summary>
        public static readonly Color ShellCanvas = Color.FromArgb(11, 11, 14);
        /// <summary>Superficie de barras, menús e interiores del shell.</summary>
        public static readonly Color ShellChrome = Color.FromArgb(15, 15, 19);
        /// <summary>Un nivel por encima del chrome (hover / elementos activos).</summary>
        public static readonly Color ShellElevated = Color.FromArgb(23, 24, 31);
        /// <summary>Textos sobre superficies ébano.</summary>
        public static readonly Color ShellText = Color.FromArgb(236, 238, 242);
        /// <summary>Textos secundarios sobre superficies ébano.</summary>
        public static readonly Color ShellMuted = Color.FromArgb(154, 161, 172);
        /// <summary>Fondo de foco suave para campos de texto (#F2F9FC).</summary>
        public static readonly Color FocusBack = Color.FromArgb(242, 249, 252);
        /// <summary>Color de botones de navegación sobre el chrome.</summary>
        public static readonly Color NavButton = Color.FromArgb(20, 20, 26);
        /// <summary>Hover de botones de navegación sobre el chrome.</summary>
        public static readonly Color NavButtonHover = Color.FromArgb(30, 31, 39);

        public const string FontFamily = "Segoe UI";

        private const double MinimumSecondaryTextContrast = 7.0;
        private static bool _enabled;
        private static bool _darkShell;
        private static readonly HashSet<Form> _themedForms = new HashSet<Form>();
        private static readonly Dictionary<string, Font> _fontCache = new Dictionary<string, Font>();

        private static readonly ToolStripRenderer _darkRenderer =
            new ToolStripProfessionalRenderer(new DarkColorTable());

        private static readonly string[] MonospaceFamilies =
        {
            "Consolas", "Courier New", "Courier", "Lucida Console",
            "Cascadia Mono", "Cascadia Code", "Lucida Sans Typewriter"
        };

        public static void Enable()
        {
            if (_enabled) return;
            _enabled = true;
            Application.Idle += OnIdle;
        }

        /// <summary>
        /// Activa el lenguaje "ébano + tarjetas": el shell de cada ventana grande
        /// pasa a ébano y las superficies grises neutras se convierten en tarjetas
        /// claras. Idempotente; se puede llamar junto a Enable() arranque.
        /// </summary>
        public static void EnableDarkShell()
        {
            _darkShell = true;
        }

        /// <summary>Pinta un control como tarjeta clara (contenido).</summary>
        public static void Card(Control c)
        {
            if (c == null) return;
            c.BackColor = Surface;
            if (c.ForeColor == Color.Empty || c.ForeColor == SystemColors.ControlText)
                c.ForeColor = Text;
        }

        /// <summary>Cromo ébano para barras, sidebars y cabeceras.</summary>
        public static void ChromeDark(Control c)
        {
            if (c == null) return;
            c.BackColor = ShellChrome;
            c.ForeColor = ShellText;
        }

        /// <summary>Lienzo ébano puro (el "margen" del shell).</summary>
        public static void CanvasDark(Control c)
        {
            if (c == null) return;
            c.BackColor = ShellCanvas;
            if (c.ForeColor == Color.Empty || c.ForeColor == SystemColors.ControlText)
                c.ForeColor = ShellText;
        }

        private static void OnIdle(object sender, EventArgs e)
        {
            var forms = Application.OpenForms;
            for (int i = 0; i < forms.Count; i++)
            {
                Form f = forms[i];
                if (f == null || _themedForms.Contains(f)) continue;
                _themedForms.Add(f);
                f.FormClosed += (s, args) => _themedForms.Remove((Form)s);
                TrackControlTree(f);
                Apply(f);
                if (_darkShell) ApplyShell(f);
            }
        }

        private static void TrackControlTree(Control root)
        {
            if (root == null) return;

            root.ControlAdded -= OnControlAdded;
            root.ControlAdded += OnControlAdded;

            foreach (Control child in root.Controls)
                TrackControlTree(child);
        }

        private static void OnControlAdded(object sender, ControlEventArgs e)
        {
            TrackControlTree(e.Control);
            Apply(e.Control);
            if (_darkShell) ShellWalk(e.Control, IsLightContext(e.Control));
        }

        private static bool IsLightContext(Control control)
        {
            while (control != null)
            {
                if (control.BackColor != Color.Transparent && control.BackgroundImage == null)
                    return IsLightSurface(control.BackColor);

                control = control.Parent;
            }

            return false;
        }

        /// <summary>Applies the theme to a control and all of its children.</summary>
        public static void Apply(Control root)
        {
            if (root == null) return;

            ApplyFont(root);

            Button b = root as Button;
            if (b != null) ApplyButton(b);

            DataGridView grid = root as DataGridView;
            if (grid != null) ApplyGrid(grid);

            TextBox text = root as TextBox;
            if (text != null) ApplyTextBox(text);

            ComboBox combo = root as ComboBox;
            if (combo != null) ApplyComboBox(combo);

            foreach (Control child in root.Controls)
                Apply(child);
        }

        // ---- Shell pass (ébano + tarjetas) --------------------------------
        private static void ApplyShell(Form f)
        {
            if (f == null) return;

            // Las ventanas que ya llegaron en ébano se dejan intactas (ej. LOGIN).
            if (IsLight(f.BackColor))
            {
                bool hasChrome = HasChromeStrip(f);
                bool large = f.ClientSize.Width >= 420 && f.ClientSize.Height >= 420;
                if (hasChrome || large)
                {
                    f.BackColor = ShellCanvas;
                    f.ForeColor = ShellText;
                }
            }

            ShellWalk(f, IsLightSurface(f.BackColor));
        }

        /// <summary>
        /// Recorre el árbol de controles aplicando el lenguaje "ébano + tarjetas":
        ///  - barras de menú/herramientas/estado → cromo ébano;
        ///  - superficies grises neutras → tarjetas claras;
        ///  - etiquetas con gris claro sobre superficies claras → gris legible.
        /// lightContext indica si la superficie efectiva del padre es clara.
        /// </summary>
        private static void ShellWalk(Control root, bool lightContext)
        {
            if (root == null) return;

            // Transparente/imagen: hereda la superficie del padre (contexto actual).
            if (root.BackColor == Color.Transparent || root.BackgroundImage != null)
            {
                ApplyTypo(root, lightContext);
                foreach (Control child in root.Controls)
                    ShellWalk(child, lightContext);
                return;
            }

            bool light = IsLightSurface(root.BackColor);
            ApplyTypo(root, light);
            SwitchBackground(root, root is Panel || root is GroupBox, light);

            foreach (Control child in root.Controls)
                ShellWalk(child, light);
        }

        /// <summary>
        /// Corrige grises de bajo contraste sobre superficies claras sin alterar
        /// los colores de marca ni los estados semánticos.
        /// Los colores de marca saturados (verde OK, cian, índigo) no se tocan.
        /// </summary>
        private static void ApplyTypo(Control c, bool lightSurface)
        {
            if (c is Label || c is LinkLabel || c is CheckBox || c is RadioButton || c is GroupBox)
            {
                if (lightSurface) FixLightForeground(c);
                else FixDarkForeground(c);
            }
        }

        private static void FixLightForeground(Control c)
        {
            Color fg = c.ForeColor;
            if (fg == Color.Empty) return; // hereda de la superficie (ya manejado)
            if (fg.GetSaturation() < 0.15f
                && GetContrastRatio(fg, Color.White) < MinimumSecondaryTextContrast)
                c.ForeColor = Muted;
        }

        private static double GetContrastRatio(Color foreground, Color background)
        {
            double foregroundLuminance = GetRelativeLuminance(foreground);
            double backgroundLuminance = GetRelativeLuminance(background);
            double lighter = Math.Max(foregroundLuminance, backgroundLuminance);
            double darker = Math.Min(foregroundLuminance, backgroundLuminance);
            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double GetRelativeLuminance(Color color)
        {
            double red = ToLinearColorComponent(color.R / 255.0);
            double green = ToLinearColorComponent(color.G / 255.0);
            double blue = ToLinearColorComponent(color.B / 255.0);
            return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
        }

        private static double ToLinearColorComponent(double component)
        {
            return component <= 0.04045
                ? component / 12.92
                : Math.Pow((component + 0.055) / 1.055, 2.4);
        }

        private static void FixDarkForeground(Control c)
        {
            Color fg = c.ForeColor;
            if (fg == Color.Empty) return; // hereda de la superficie (ya manejado)
            if (fg.GetSaturation() < 0.15f && fg.GetBrightness() < 0.45f)
                c.ForeColor = ShellText;
        }

        private static bool IsLightSurface(Color c)
        {
            return c != Color.Transparent && c.GetBrightness() >= 0.35f;
        }

        private static void SwitchBackground(Control c, bool cardTarget, bool light)
        {
            if (c is MenuStrip || c is ToolStrip || c is StatusStrip)
            {
                ToolStrip strip = (ToolStrip)c;
                if (strip.BackgroundImage == null)
                {
                    strip.BackColor = ShellChrome;
                    strip.ForeColor = ShellText;
                    strip.Renderer = _darkRenderer;
                }
                return;
            }

            if (cardTarget && light) ApplySurface(c, true);
        }

        /// <summary>
        /// Decide qué hace el shell con una superficie:
        ///  - gris neutro medio → tarjeta clara (contenido elevado);
        ///  - ébano ya presente → no se toca (decisión explícita del formulario).
        /// Los contenedores de layout (Flow/Table/Split) y los paneles en colores
        /// saturados nunca se recolorean.
        /// </summary>
        private static void ApplySurface(Control c, bool strictPanel)
        {
            if (c.BackgroundImage != null) return;
            if (c.BackColor == Color.Transparent) return;
            if (c.BackColor == SystemColors.ControlDark || c.BackColor == SystemColors.ControlDarkDark) return;

            Color bc = c.BackColor;
            float brightness = bc.GetBrightness();
            float saturation = bc.GetSaturation();

            bool neutral = saturation < 0.15f;
            if (!neutral) return;

            // Layout containers keep their skin (nav trays, tables, splitters).
            if (c is FlowLayoutPanel || c is TableLayoutPanel || c is SplitContainer) return;

            // Grey, mid-brightness surface = a light content card on the dark shell.
            if (brightness >= 0.30f && brightness <= 0.945f && strictPanel)
            {
                Card(c);
            }
        }

        private static bool HasChromeStrip(Form f)
        {
            return FindStrip(f) != null;
        }

        private static ToolStrip FindStrip(Control root)
        {
            if (root == null) return null;
            if (root is MenuStrip || root is ToolStrip || root is StatusStrip)
                return (ToolStrip)root;
            foreach (Control child in root.Controls)
            {
                ToolStrip found = FindStrip(child);
                if (found != null) return found;
            }
            return null;
        }

        private static bool IsLight(Color c)
        {
            return c.GetBrightness() >= 0.30f && c != Color.Transparent;
        }

        // ---- Per-control styling ------------------------------------------
        private static void ApplyFont(Control c)
        {
            Font f = c.Font;
            if (f == null) return;
            if (string.Equals(f.FontFamily.Name, FontFamily, StringComparison.OrdinalIgnoreCase)) return;
            if (IsMonospace(f.FontFamily.Name)) return;
            try
            {
                c.Font = GetFont(f.SizeInPoints, f.Style);
            }
            catch
            {
                // keep original font if it cannot be replaced
            }
        }

    public enum ButtonRole { Auto, Primary, Secondary, Ghost, Danger, Warning }

        internal static ButtonRole InferRole(Button b)
        {
            string t = (b.Text ?? "").Trim().ToUpperInvariant();
            if (t.Contains("ELIMINAR") || t.Contains("BORRAR") || t.Contains("CANCELAR"))
                return ButtonRole.Danger;
            if (t.Contains("GUARDAR") || t.Contains("COBRAR") || t.Contains("CONFIRMAR") || t.Contains("ACEPTAR"))
                return ButtonRole.Primary;
            if (t.Contains("VOLVER") || t.Contains("ATRÁS") || t.Contains("SALIR"))
                return ButtonRole.Ghost;
            return ButtonRole.Auto;
        }

        internal static void StyleByRole(Button b, ButtonRole role)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
            switch (role)
            {
                case ButtonRole.Primary:
                    b.BackColor = Primary;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.MouseOverBackColor = Accent;
                    b.FlatAppearance.MouseDownBackColor = PrimaryDark;
                    break;
                case ButtonRole.Secondary:
                    b.BackColor = SurfaceAlt;
                    b.ForeColor = Text;
                    b.FlatAppearance.BorderColor = Border;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.MouseOverBackColor = GridAltRow;
                    b.FlatAppearance.MouseDownBackColor = Border;
                    break;
                case ButtonRole.Ghost:
                    b.BackColor = Surface;
                    b.ForeColor = Muted;
                    b.FlatAppearance.BorderSize = 0;
                    b.FlatAppearance.MouseOverBackColor = GridAltRow;
                    b.FlatAppearance.MouseDownBackColor = Border;
                    break;
                case ButtonRole.Danger:
                    b.BackColor = Danger;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 80, 80);
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(190, 45, 45);
                    break;
                case ButtonRole.Warning:
                    b.BackColor = Color.FromArgb(255, 193, 7);
                    b.ForeColor = Color.Black;
                    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 210, 20);
                    b.FlatAppearance.MouseDownBackColor = Color.FromArgb(245, 170, 0);
                    break;
                default:
                    b.BackColor = Primary;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.MouseOverBackColor = Accent;
                    b.FlatAppearance.MouseDownBackColor = PrimaryDark;
                    break;
            }
        }

        public static class ButtonStyler
        {
            public static void Apply(Button b, ButtonRole role = ButtonRole.Auto)
            {
                if (b == null) return;
                if (b.BackgroundImage != null || b.Image != null || b.BackColor == Color.Transparent) return;
                if (b.FlatStyle == FlatStyle.System) return;
                if (role == ButtonRole.Auto) role = InferRole(b);
                StyleByRole(b, role);
                b.EnabledChanged -= OnButtonEnabledChanged;
                b.EnabledChanged += OnButtonEnabledChanged;
            }
        }

        private static void ApplyButton(Button b)
        {
            if (b.BackgroundImage != null) return;
            if (b.Image != null) return;
            if (b.BackColor == Color.Transparent) return;
            if (b.FlatStyle == FlatStyle.System) return;

            ButtonRole role = InferRole(b);
            StyleByRole(b, role);

            b.EnabledChanged -= OnButtonEnabledChanged;
            b.EnabledChanged += OnButtonEnabledChanged;
        }

        private static void OnButtonEnabledChanged(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;
            if (b.BackgroundImage != null || b.Image != null || b.BackColor == Color.Transparent) return;
            if (!b.Enabled)
            {
                b.BackColor = Color.FromArgb(159, 168, 184);
                b.ForeColor = Color.FromArgb(245, 247, 250);
            }
            else if (b.FlatStyle == FlatStyle.Flat)
            {
                b.BackColor = Primary;
                b.ForeColor = Color.White;
            }
        }

        private static void ApplyTextBox(TextBox t)
        {
            if (t.BorderStyle == BorderStyle.None) return;
            t.BorderStyle = BorderStyle.FixedSingle;
            if (t.ReadOnly) t.BackColor = SurfaceAlt;

            // Foco visible: un borde/relleno cian-tenue marca el campo activo.
            t.GotFocus -= OnTextBoxFocus;
            t.LostFocus -= OnTextBoxBlur;
            t.GotFocus += OnTextBoxFocus;
            t.LostFocus += OnTextBoxBlur;
        }

        private static void OnTextBoxFocus(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null || t.ReadOnly) return;
            t.BackColor = FocusBack;
        }

        private static void OnTextBoxBlur(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null) return;
            t.BackColor = t.ReadOnly ? SurfaceAlt : Surface;
        }

        private static void ApplyComboBox(ComboBox c)
        {
            if (c.DropDownStyle == ComboBoxStyle.Simple) return;
            c.BackColor = Surface;
            c.ForeColor = Text;

            c.GotFocus -= OnComboFocus;
            c.LostFocus -= OnComboBlur;
            c.GotFocus += OnComboFocus;
            c.LostFocus += OnComboBlur;
        }

        private static void OnComboFocus(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;
            cb.BackColor = FocusBack;
        }

        private static void OnComboBlur(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb == null) return;
            cb.BackColor = Surface;
        }

        private static void ApplyGrid(DataGridView g)
        {
            try
            {
                g.EnableHeadersVisualStyles = false;
                g.BackgroundColor = Surface;
                g.BorderStyle = BorderStyle.None;
                g.GridColor = Border;
                g.RowTemplate.Height = 32;

                DataGridViewCellStyle header = g.ColumnHeadersDefaultCellStyle;
                header.BackColor = Primary;
                header.ForeColor = Color.White;
                header.SelectionBackColor = PrimaryDark;
                header.SelectionForeColor = Color.White;
                header.Font = GetFont(g.Font.SizeInPoints, FontStyle.Bold);
                header.Alignment = DataGridViewContentAlignment.MiddleLeft;
                header.Padding = new Padding(8, 6, 8, 6);
                g.ColumnHeadersDefaultCellStyle = header;
                g.ColumnHeadersHeight = 36;

                DataGridViewCellStyle body = g.DefaultCellStyle;
                body.SelectionBackColor = GridSelection;
                body.SelectionForeColor = Color.White;
                body.ForeColor = Text;
                body.Padding = new Padding(8, 4, 8, 4);
                g.DefaultCellStyle = body;

                g.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow;
                g.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelection;
                g.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

                // Grillas claras: el texto de fila nunca queda blanco sobre blanco.
                DataGridViewCellStyle rowStyle = g.RowTemplate.DefaultCellStyle;
                if (rowStyle.ForeColor == Color.Empty
                    || rowStyle.ForeColor == SystemColors.ControlText
                    || (rowStyle.ForeColor.ToArgb() & 0xFFFFFF) >= 0xE8E8E8)
                {
                    rowStyle.ForeColor = Text;
                    g.RowTemplate.DefaultCellStyle = rowStyle;
                }

                g.CellMouseEnter -= OnGridCellEnter;
                g.CellMouseLeave -= OnGridCellLeave;
                g.CellMouseEnter += OnGridCellEnter;
                g.CellMouseLeave += OnGridCellLeave;
            }
            catch
            {
            }
        }

        private static void OnGridCellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView g = sender as DataGridView;
            if (g == null || e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
            if (g.Rows[e.RowIndex].Selected) return;
            g.Rows[e.RowIndex].DefaultCellStyle.BackColor = GridHover;
        }

        private static void OnGridCellLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView g = sender as DataGridView;
            if (g == null || e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
            if (g.Rows[e.RowIndex].Selected) return;
            bool alt = (e.RowIndex % 2 == 1);
            g.Rows[e.RowIndex].DefaultCellStyle.BackColor = alt ? GridAltRow : Color.Empty;
        }

        // ---- Helpers -------------------------------------------------------
        private static Font GetFont(float sizeInPoints, FontStyle style)
        {
            sizeInPoints = (float)Math.Round(sizeInPoints * 2f) / 2f;
            if (sizeInPoints <= 0f) sizeInPoints = 9f;
            string key = sizeInPoints.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + (int)style;
            Font f;
            if (!_fontCache.TryGetValue(key, out f))
            {
                f = new Font(FontFamily, sizeInPoints, style, GraphicsUnit.Point);
                _fontCache[key] = f;
            }
            return f;
        }

        private static bool IsMonospace(string family)
        {
            for (int i = 0; i < MonospaceFamilies.Length; i++)
                if (string.Equals(family, MonospaceFamilies[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        /// <summary>Paleta de render oscura para menús y barras de herramientas.</summary>
        private sealed class DarkColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin { get { return ShellChrome; } }
            public override Color ToolStripGradientMiddle { get { return ShellChrome; } }
            public override Color ToolStripGradientEnd { get { return ShellChrome; } }
            public override Color MenuStripGradientBegin { get { return ShellChrome; } }
            public override Color MenuStripGradientEnd { get { return ShellChrome; } }
            public override Color ToolStripBorder { get { return ShellCanvas; } }
            public override Color MenuItemSelected { get { return ShellElevated; } }
            public override Color MenuItemBorder { get { return ShellElevated; } }
            public override Color MenuItemSelectedGradientBegin { get { return ShellElevated; } }
            public override Color MenuItemSelectedGradientEnd { get { return ShellElevated; } }
            public override Color MenuItemPressedGradientBegin { get { return ShellElevated; } }
            public override Color MenuItemPressedGradientMiddle { get { return ShellElevated; } }
            public override Color MenuItemPressedGradientEnd { get { return ShellElevated; } }
            public override Color CheckBackground { get { return Accent; } }
            public override Color CheckSelectedBackground { get { return Accent; } }
            public override Color StatusStripGradientBegin { get { return ShellChrome; } }
            public override Color StatusStripGradientEnd { get { return ShellChrome; } }
            public override Color SeparatorDark { get { return ShellCanvas; } }
            public override Color SeparatorLight { get { return ShellElevated; } }
            public override Color ButtonSelectedHighlight { get { return ShellElevated; } }
            public override Color ButtonSelectedHighlightBorder { get { return ShellElevated; } }
            public override Color ImageMarginGradientBegin { get { return ShellChrome; } }
            public override Color ImageMarginGradientMiddle { get { return ShellChrome; } }
            public override Color ImageMarginGradientEnd { get { return ShellChrome; } }
        }
    }
}