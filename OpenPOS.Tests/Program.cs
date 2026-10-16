using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenPOS.CONEXION;
using OpenPOS.Presentacion.Asistenteinstalacion;
using OpenPOS.UI;

namespace OpenPOS.Tests
{
    /// <summary>
    /// Ejecutor de pruebas mínimo, sin dependencias de NuGet, para poder
    /// validar la lógica crítica en cualquier equipo con MSBuild/.NET 4.8.
    /// Devuelve un código de salida distinto de cero si alguna prueba falla.
    /// </summary>
    internal static class Program
    {
        private static int _fallos;
        private static int _ejecutadas;

        [STAThread]
        private static int Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--preview-console")
            {
                string destino = args.Length > 1
                    ? args[1]
                    : System.IO.Path.Combine(System.IO.Path.GetTempPath(), "openpos_consola.png");
                VistaPreviaConsola(destino);
                Console.WriteLine("Vista previa guardada en " + destino);
                return 0;
            }
            if (args != null && args.Length > 0 && args[0] == "--preview-dialog")
            {
                string destino = args.Length > 1
                    ? args[1]
                    : System.IO.Path.Combine(System.IO.Path.GetTempPath(), "openpos_dialogo.png");
                VistaPreviaDialogo(destino);
                Console.WriteLine("Vista previa guardada en " + destino);
                return 0;
            }

            Console.WriteLine("== Pruebas openPOS ==");

            Check("SqlScriptRunner.Dividir separa por GO", () =>
            {
                string script = "SELECT 1\r\nGO\r\nSELECT 2\ngo\r\nSELECT 3";
                List<string> lotes = SqlScriptRunner.Dividir(script);
                Assert(lotes.Count == 3, "Se esperaban 3 lotes, se obtuvieron " + lotes.Count);
                Assert(lotes[0] == "SELECT 1", "Lote 1 incorrecto: " + lotes[0]);
                Assert(lotes[2] == "SELECT 3", "Lote 3 incorrecto: " + lotes[2]);
            });

            Check("SqlScriptRunner.Dividir trata GO con número como un separador", () =>
            {
                int noVacios = 0;
                foreach (string lote in SqlScriptRunner.Dividir("SELECT 1\nGO 2\nSELECT 2"))
                {
                    if (lote.Length > 0) noVacios++;
                }
                Assert(noVacios == 2, "Se esperaban 2 lotes, se obtuvieron " + noVacios);
            });

            Check("SqlScriptRunner.Preparar reemplaza el nombre de la base", () =>
            {
                string resultado = SqlScriptRunner.Preparar(
                    "CREATE DATABASE BASEADACURSO; USE BASEADACURSO;", "MiBase");
                Assert(resultado == "CREATE DATABASE MiBase; USE MiBase;",
                    "Resultado inesperado: " + resultado);
            });

            Check("SqlScriptRunner.Preparar reescribe la carpeta de datos", () =>
            {
                string script = @"CREATE DATABASE X ON (FILENAME = N'C:\Viejo\BASEADACURSO.mdf')";
                string resultado = SqlScriptRunner.Preparar(script, null, @"D:\Datos");
                Assert(resultado.Contains(@"D:\Datos\BASEADACURSO.mdf"),
                    "No se reescribió la ruta: " + resultado);
            });

            Check("SqlScriptRunner.IdentificadorSeguro valida y escapa", () =>
            {
                Assert(SqlScriptRunner.IdentificadorSeguro("MiBase_1") == "[MiBase_1]",
                    "Identificador válido rechazado");
                Assert(Lanza(() => SqlScriptRunner.IdentificadorSeguro("x]; DROP TABLE y--")),
                    "Se aceptó un identificador con inyección SQL");
                Assert(Lanza(() => SqlScriptRunner.IdentificadorSeguro("con espacio")),
                    "Se aceptó un identificador con espacios");
            });

            Check("KeyProvider expone una clave no vacía", () =>
            {
                Assert(!string.IsNullOrEmpty(KeyProvider.MasterKey), "La clave maestra está vacía");
            });

            Check("Desencryptacion usa la misma clave que KeyProvider", () =>
            {
                Assert(Desencryptacion.appPwdUnique == KeyProvider.MasterKey,
                    "La clave de Desencryptacion no coincide con KeyProvider");
            });

            Check("AES cifra y descifra correctamente (ida y vuelta)", () =>
            {
                var aes = new AES();
                const string original = "Server=localhost;Database=MiBase;";
                string cifrado = aes.Encrypt(original, KeyProvider.MasterKey, 256);
                string descifrado = aes.Decrypt(cifrado, KeyProvider.MasterKey, 256);
                Assert(descifrado == original, "El descifrado no coincide con el original");
            });

            Check("AES.Decrypt lanza excepción clara con clave incorrecta", () =>
            {
                var aes = new AES();
                string cifrado = aes.Encrypt("dato", "clave-correcta", 256);
                try
                {
                    aes.Decrypt(cifrado, "clave-incorrecta", 256);
                    throw new InvalidOperationException("Decrypt no lanzó excepción con clave incorrecta");
                }
                catch (InvalidOperationException ex)
                {
                    Assert(ex.Message.Contains("descifrar"), "El mensaje no explica el fallo: " + ex.Message);
                }
            });

            Check("AES.Decrypt lanza excepción con dato corrupto", () =>
            {
                var aes = new AES();
                try
                {
                    aes.Decrypt("!!!no-es-base64!!!", KeyProvider.MasterKey, 256);
                    throw new InvalidOperationException("Decrypt no lanzó excepción con Base64 inválido");
                }
                catch (InvalidOperationException ex)
                {
                    Assert(ex.Message.Contains("corrupto"), "El mensaje no explica el fallo: " + ex.Message);
                }
            });

            Check("PasswordHasher genera hash verificable", () =>
            {
                string hash = OpenPOS.Logica.PasswordHasher.Hash("mi contraseña");
                Assert(OpenPOS.Logica.PasswordHasher.EsHash(hash), "El formato no se reconoce como hash: " + hash);
                Assert(OpenPOS.Logica.PasswordHasher.Verificar("mi contraseña", hash),
                    "La contraseña correcta no fue aceptada");
                Assert(!OpenPOS.Logica.PasswordHasher.Verificar("otra contraseña", hash),
                    "Una contraseña incorrecta fue aceptada");
            });

            Check("PasswordHasher usa sal aleatoria y cabe en varchar(50)", () =>
            {
                string h1 = OpenPOS.Logica.PasswordHasher.Hash("repetida");
                string h2 = OpenPOS.Logica.PasswordHasher.Hash("repetida");
                Assert(h1 != h2, "Dos hash de la misma contraseña son iguales (sal fija)");
                Assert(h1.Length <= 50, "El hash no cabe en Password varchar(50): " + h1.Length);
                Assert(!OpenPOS.Logica.PasswordHasher.Verificar("x", "texto-plano-viejo"),
                    "Verificar aceptó un valor que no es hash");
            });

            Check("Theme aplica la marca sin romper skins ni fuentes monoespaciadas", () =>
            {
                using (var f = new Form())
                using (var skinBitmap = new Bitmap(2, 2))
                using (var sans = new Font("Microsoft Sans Serif", 9f))
                using (var mono = new Font("Consolas", 9f))
                {
                    var btn = new Button { Text = "Aceptar" };
                    var skin = new Button { BackgroundImage = skinBitmap };
                    var tb = new TextBox { Font = sans };
                    var code = new TextBox { Font = mono };
                    var grid = new DataGridView();
                    f.Controls.Add(btn);
                    f.Controls.Add(skin);
                    f.Controls.Add(tb);
                    f.Controls.Add(code);
                    f.Controls.Add(grid);

                    Theme.Apply(f);

                    Assert(btn.FlatStyle == FlatStyle.Flat, "El boton no se aplanó");
                    Assert(btn.BackColor == Theme.Primary, "El boton no usa el color de marca");
                    Assert(skin.BackColor != Theme.Primary, "Se sobrescribió un boton con imagen");
                    Assert(tb.Font.FontFamily.Name == "Segoe UI", "El textbox no usa Segoe UI");
                    Assert(code.Font.FontFamily.Name == "Consolas", "Se cambió la fuente monoespaciada");
                    Assert(grid.ColumnHeadersDefaultCellStyle.BackColor == Theme.Primary,
                        "El encabezado de la grilla no usa la marca");
                }
            });

            Check("InstallConsole registra pasos, log y progreso", () =>
            {
                using (var f = new Form())
                using (var consola = new InstallConsole())
                {
                    f.Controls.Add(consola);
                    f.Show();
                    try
                    {
                        consola.Iniciar("Prueba");
                        consola.DefinirPasos("Paso uno", "Paso dos");
                        consola.PasoEnProceso(0);
                        consola.PasoCompletado(0);
                        consola.PasoFallo(1, "fallo simulado");
                        consola.Info("mensaje de prueba");
                        consola.SetProgreso(50);

                        Assert(consola.TextoLog.Contains("mensaje de prueba"),
                            "El registro no capturó el mensaje de prueba");
                        Assert(consola.TextoLog.Contains("Paso uno"),
                            "El registro no capturó el nombre del paso");
                    }
                    finally
                    {
                        consola.Detener();
                        f.Close();
                    }
                }
            });

            Check("El diálogo de instalación acepta base de datos y credenciales editables", () =>
            {
                var inicial = new DatosInstalacion
                {
                    Instancia = "SQLEXPRESS",
                    BaseDatos = "openpos",
                    Usuario = "openPOS",
                    Contrasena = string.Empty
                };
                using (var dlg = new DatosInstalacionDialog(inicial))
                {
                    Assert(dlg.Text.Contains("Datos de instalación"), "Título inesperado: " + dlg.Text);
                    Assert(dlg.Resultado == null, "No debe haber resultado antes de confirmar");
                }
            });

            Console.WriteLine();
            Console.WriteLine(string.Format("Pruebas ejecutadas: {0}, fallos: {1}", _ejecutadas, _fallos));
            return _fallos == 0 ? 0 : 1;
        }

        private static void VistaPreviaConsola(string destino)
        {
            using (var f = new Form())
            {
                f.Text = "Vista previa - Consola de instalacion";
                f.ClientSize = new Size(1360, 820);
                f.StartPosition = FormStartPosition.CenterScreen;

                var consola = new InstallConsole { Dock = DockStyle.Fill };
                f.Controls.Add(consola);
                f.Show();
                Application.DoEvents();

                consola.Iniciar("Instalando SQL Server Express en este equipo…");
                consola.DefinirPasos(
                    "Preparar los archivos de configuración",
                    "Iniciar el instalador de SQL Server Express",
                    "Instalar SQL Server Express (puede tardar varios minutos)",
                    "Verificar la instancia recién instalada",
                    "Eliminar la base de datos previa (si existe)",
                    "Crear la base de datos BASE_TEST",
                    "Ejecutar tablas y procedimientos almacenados",
                    "Guardar la configuración de conexión",
                    "Finalizar");

                consola.PasoEnProceso(0);
                consola.PasoCompletado(0);
                consola.PasoEnProceso(1);
                consola.PasoCompletado(1);
                consola.PasoEnProceso(2);
                consola.Info("Archivo de configuración escrito en «C:\\OpenPOS\\ConfigurationFile.ini».");
                consola.Info("Solicitando permisos de administrador al instalador de SQL Server…");
                consola.Info("Instalador iniciado (deje que la ventana de SQL Server finalice por sí sola).");
                consola.Info("Instalando… 1 min 30 s transcurridos. El instalador continúa trabajando.");
                consola.Aviso("La instancia aún no responde; reintentando (1/24)…");
                consola.SetProgreso(22);

                Application.DoEvents();
                consola.PerformLayout();
                Application.DoEvents();

                using (var bmp = new Bitmap(consola.Width, consola.Height))
                {
                    consola.DrawToBitmap(bmp, new Rectangle(0, 0, consola.Width, consola.Height));
                    bmp.Save(destino, System.Drawing.Imaging.ImageFormat.Png);
                }

                consola.Detener();
                f.Close();
            }
        }

        private static void VistaPreviaDialogo(string destino)
        {
            var inicial = new DatosInstalacion
            {
                Instancia = "SQLEXPRESS",
                BaseDatos = "openpos",
                Usuario = "openPOS",
                Contrasena = string.Empty
            };

            using (var dlg = new DatosInstalacionDialog(inicial))
            {
                dlg.StartPosition = FormStartPosition.Manual;
                dlg.Location = new Point(80, 80);
                dlg.Show();
                Application.DoEvents();

                using (var bmp = new Bitmap(dlg.Width, dlg.Height))
                {
                    dlg.DrawToBitmap(bmp, new Rectangle(0, 0, dlg.Width, dlg.Height));
                    bmp.Save(destino, System.Drawing.Imaging.ImageFormat.Png);
                }
                dlg.Close();
            }
        }

        private static void Check(string nombre, Action prueba)
        {
            _ejecutadas++;
            try
            {
                prueba();
                Console.WriteLine("[OK]   " + nombre);
            }
            catch (Exception ex)
            {
                _fallos++;
                Console.WriteLine("[FALLO] " + nombre + " -> " + ex.Message);
            }
        }

        private static void Assert(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new InvalidOperationException(mensaje);
            }
        }

        private static bool Lanza(Action accion)
        {
            try
            {
                accion();
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }
}
