using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPOS.UI
{
    public enum DialogResultCustom { OK, Cancel, Yes, No }

    internal static class DialogService
    {
        public static DialogResultCustom Show(string text, string caption = "OpenPOS", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var f = new Form())
            {
                f.FormBorderStyle = FormBorderStyle.None;
                f.StartPosition = FormStartPosition.CenterParent;
                f.BackColor = Theme.ShellCanvas;
                Theme.ChromeDark(f);
                f.Size = new Size(420, 190);
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowInTaskbar = false;
                f.TopMost = true;

                var iconPic = new PictureBox
                {
                    Size = new Size(48, 48),
                    Location = new Point(16, 16),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = GetIconImage(icon)
                };
                f.Controls.Add(iconPic);

                var lblText = new Label
                {
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = text,
                    Font = new Font(Theme.FontFamily, 10f, FontStyle.Regular),
                    ForeColor = Theme.ShellText,
                    Location = new Point(72, 16),
                    Size = new Size(332, 60)
                };
                f.Controls.Add(lblText);

                var lblCap = new Label
                {
                    AutoSize = true,
                    Text = caption ?? "OpenPOS",
                    Font = new Font(Theme.FontFamily, 11f, FontStyle.Bold),
                    ForeColor = Theme.ShellText,
                    Location = new Point(72, 82)
                };
                f.Controls.Add(lblCap);

                int x = 80;
                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        AddButton(f, "OK", DialogResultCustom.OK, 200, ref x);
                        break;
                    case MessageBoxButtons.OKCancel:
                        AddButton(f, "Cancelar", DialogResultCustom.Cancel, 200, ref x);
                        AddButton(f, "OK", DialogResultCustom.OK, 200, ref x);
                        break;
                    case MessageBoxButtons.YesNo:
                        AddButton(f, "No", DialogResultCustom.No, 160, ref x);
                        AddButton(f, "Sí", DialogResultCustom.Yes, 160, ref x);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        AddButton(f, "Cancelar", DialogResultCustom.Cancel, 140, ref x);
                        AddButton(f, "No", DialogResultCustom.No, 140, ref x);
                        AddButton(f, "Sí", DialogResultCustom.Yes, 140, ref x);
                        break;
                }

                var result = DialogResultCustom.Cancel;
                f.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter) { result = DialogResultCustom.OK; f.Close(); }
                    if (e.KeyCode == Keys.Escape) { result = DialogResultCustom.Cancel; f.Close(); }
                };
                f.ShowDialog();
                return result;
            }
        }

        private static void AddButton(Form owner, string text, DialogResultCustom tag, int width, ref int x)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 34),
                Location = new Point(x, 120),
                Tag = tag,
                Font = new Font(Theme.FontFamily, 9.5f, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            Theme.ButtonStyler.Apply(b, "OK".Equals(text, StringComparison.OrdinalIgnoreCase) || "Sí".Equals(text, StringComparison.OrdinalIgnoreCase)
                ? Theme.ButtonRole.Primary
                : Theme.ButtonRole.Secondary);
            b.Click += (s, e) => { owner.Tag = b.Tag; owner.Close(); };
            owner.Controls.Add(b);
            x -= width + 8;
        }

        private static Image GetIconImage(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Error:
                    return SystemIcons.Error.ToBitmap();
                case MessageBoxIcon.Warning:
                    return SystemIcons.Warning.ToBitmap();
                case MessageBoxIcon.Question:
                    return SystemIcons.Question.ToBitmap();
                default:
                    return SystemIcons.Information.ToBitmap();
            }
        }
    }
}