using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPOS.UI
{
    /// <summary>
    /// Global look-and-feel layer. Applies a single, consistent openPOS visual
    /// system to every form/control at runtime, without touching the designers.
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
        public static readonly Color GridSelection = Color.FromArgb(216, 235, 250);
        public static readonly Color Border = Color.FromArgb(214, 220, 230);
        public static readonly Color Text = Color.FromArgb(33, 37, 41);
        public static readonly Color Muted = Color.FromArgb(110, 118, 129);
        public static readonly Color Success = Color.FromArgb(26, 168, 90);      // #1AA85A
        public static readonly Color Warning = Color.FromArgb(245, 166, 11);     // #F5A60B
        public static readonly Color Danger = Color.FromArgb(224, 60, 60);       // #E03C3C

        public const string FontFamily = "Segoe UI";

        private static bool _enabled;
        private static readonly HashSet<Form> _themedForms = new HashSet<Form>();
        private static readonly Dictionary<string, Font> _fontCache = new Dictionary<string, Font>();

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

        private static void OnIdle(object sender, EventArgs e)
        {
            var forms = Application.OpenForms;
            for (int i = 0; i < forms.Count; i++)
            {
                Form f = forms[i];
                if (f == null || _themedForms.Contains(f)) continue;
                _themedForms.Add(f);
                f.FormClosed += (s, args) => _themedForms.Remove((Form)s);
                f.ControlAdded += (s, args) => Apply(args.Control);
                Apply(f);
            }
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

        private static void ApplyButton(Button b)
        {
            // Preserve custom skins / icon buttons and transparent buttons.
            if (b.BackgroundImage != null) return;
            if (b.Image != null) return;
            if (b.BackColor == Color.Transparent) return;
            if (b.FlatStyle == FlatStyle.System) return;

            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Primary;
            b.ForeColor = Color.White;
            b.Cursor = Cursors.Hand;
            b.FlatAppearance.MouseOverBackColor = Accent;
            b.FlatAppearance.MouseDownBackColor = PrimaryDark;
        }

        private static void ApplyTextBox(TextBox t)
        {
            if (t.BorderStyle == BorderStyle.None) return;
            t.BorderStyle = BorderStyle.FixedSingle;
            if (t.ReadOnly) t.BackColor = SurfaceAlt;
        }

        private static void ApplyComboBox(ComboBox c)
        {
            if (c.DropDownStyle == ComboBoxStyle.Simple) return;
            c.BackColor = Surface;
            c.ForeColor = Text;
        }

        private static void ApplyGrid(DataGridView g)
        {
            try
            {
                g.EnableHeadersVisualStyles = false;
                g.BackgroundColor = Surface;
                g.BorderStyle = BorderStyle.None;
                g.GridColor = Border;
                g.RowTemplate.Height = 28;

                DataGridViewCellStyle header = g.ColumnHeadersDefaultCellStyle;
                header.BackColor = Primary;
                header.ForeColor = Color.White;
                header.SelectionBackColor = PrimaryDark;
                header.SelectionForeColor = Color.White;
                header.Font = GetFont(g.Font.SizeInPoints, FontStyle.Bold);
                header.Alignment = DataGridViewContentAlignment.MiddleLeft;

                g.ColumnHeadersDefaultCellStyle = header;

                DataGridViewCellStyle body = g.DefaultCellStyle;
                body.SelectionBackColor = GridSelection;
                body.SelectionForeColor = Text;
                body.ForeColor = Text;
                g.DefaultCellStyle = body;

                g.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow;
                g.AlternatingRowsDefaultCellStyle.SelectionBackColor = GridSelection;
            }
            catch
            {
                // leave grid untouched if a style cannot be applied
            }
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
    }
}
