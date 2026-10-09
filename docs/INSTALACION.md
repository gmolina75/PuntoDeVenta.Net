# Guía de instalación, compilación y puesta en marcha

Esta guía explica cómo dejar **Ada369 C#** compilando y ejecutándose en una máquina Windows.

## 1. Requisitos previos

| Componente | Detalle |
| --- | --- |
| Sistema operativo | Windows 10/11 (x64) |
| IDE | Visual Studio 2019/2022/2026 con carga de trabajo *Desarrollo de escritorio de .NET* |
| .NET Framework | **4.8** (targeting pack y runtime) |
| Base de datos | SQL Server Express / Developer, con *Integrated Security* |
| Permisos | Sin elevación permanente (manifiesto `asInvoker`); UAC solo al instalar SQL Server Express |

> La aplicación accede al **serial físico del disco** (`Win32_PhysicalMedia`) para el licenciamiento;
> esta consulta WMI no requiere elevación permanente.

El proyecto **no requiere paquetes NuGet ni dependencias comerciales**. Usa únicamente ensamblados del
framework .NET y `UIDC.dll` (incluida en el repositorio).

## 2. Compilar

No hay restauración de paquetes. Compila directamente con MSBuild:

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild Ada369Csharp.sln /p:Configuration=Debug
```

Salida: `Ada369Csharp\bin\Debug\Ada369 PE.exe`.

> Si aparece el error `MSB3644: The reference assemblies for .NETFramework,Version=v4.6 were not found`,
> verifica que el proyecto esté apuntando a **v4.8** (*Propiedades del proyecto → Aplicación →
> Framework de destino*). El repositorio ya se entrega con `TargetFrameworkVersion = v4.8`.

## 3. Preparar la base de datos

### Opción A — Asistente de instalación (recomendado)

Ejecuta `Ada369 PE.exe` en la primera ejecución:

1. `LOGIN` intenta leer `ConnectionString.xml`. Si no existe, abre `Opcionesprincipales`.
2. `Instalacionservidor` crea la base de datos y ejecuta el script de tablas/procedimientos.
   - Servidor por defecto: `.\SQLEXPRESS`
   - Base de datos por defecto: `BASEADACURSO`
   - Usuario/contraseña de ejemplo: `pruebas2020` / `pruebas123`
3. `Registroempresa` registra la empresa, la caja y el ticket.
4. `UsuarioPrincipal` crea el usuario administrador.

> El asistente ejecuta el script **directamente desde ADO.NET** (`CONEXION/SqlScriptRunner.cs`),
> por lotes separados con `GO`. **No requiere `sqlcmd`** ni utilidades de línea de comandos.

### Opción B — Manual con `script.sql`

1. Abre `script.sql` en SQL Server Management Studio.
2. Reemplaza el nombre de la base de datos `BASEADACURSO` por el que quieras, y ajusta las rutas
   `.mdf`/`.ldf` a tu instancia.
3. Ejecuta el script (32 tablas, 16 procedimientos almacenados).
4. Elimina o crea `ConnectionString.xml` según convenga (ver sección 4).

## 4. Cadena de conexión (`ConnectionString.xml`)

La cadena de conexión **no** está en `app.config`; se guarda cifrada (AES-256) en
`ConnectionString.xml` en el directorio de la aplicación.

- **Se crea automáticamente** al usar el asistente (sección 3, opción A).
- Para **re-ejecutar el asistente**, borra `ConnectionString.xml` y vuelve a lanzar la aplicación.

### Clave de cifrado

La clave **ya no está incrustada** en el código. `CONEXION/KeyProvider.cs` la resuelve en este orden:

1. Variable de entorno **`ADA369_CRYPTO_KEY`** (recomendado en servidores/CI).
2. appSetting **`Ada369:CryptoKey`** en `app.config`.
3. Archivo **`ada369.key`** protegido con **DPAPI** junto al ejecutable.
4. **Clave heredada** (compatibilidad con instalaciones existentes), registrando una advertencia en el
   log. Genera una clave nueva con `KeyProvider.GenerarYGuardar()`.

> Si cambias la clave, las cadenas de conexión ya cifradas dejarán de descifrarse: vuelve a ejecutar el
> asistente (borrando `ConnectionString.xml`) o re-cifra la cadena con la nueva clave.

## 5. Ejecutar

```powershell
.\Ada369Csharp\bin\Debug\"Ada369 PE.exe"
```

La aplicación se ejecuta como usuario estándar (`asInvoker`); solo verás UAC si el asistente lanza el
instalador de SQL Server Express.

## 6. Reportes e impresión

Los reportes usan la librería propia `Ada369Csharp.Reportes` (visor WinForms + impresión GDI+); **no se
requiere Telerik ni ningún componente externo**. La impresión de tickets y reportes se realiza con
`System.Drawing.Printing`.

## 7. Facturación electrónica (SUNAT)

- Los endpoints del servicio `billService` están en `Ada369Csharp/app.config`.
- Se requiere un **certificado digital** y credenciales `SOL` configuradas en los formularios de SUNAT.
- La compresión de los envíos usa `System.IO.Compression` (framework), sin dependencias externas.

## 8. Solución de problemas

| Síntoma | Causa probable | Solución |
| --- | --- | --- |
| `MSB3644` .NET v4.6 not found | Targeting pack desalineado | Apuntar el proyecto a **v4.8** (sección 2) |
| No se crean las tablas | SQL Server no accesible o credenciales incorrectas | Revisar el log en `%LOCALAPPDATA%\Ada369\logs` e iniciar el servicio |
| "Error de conexión" al iniciar | SQL Server apagado o `ConnectionString.xml` inválido | Iniciar el servicio y borrar el archivo para re-ejecutar el asistente |
| No se generan comprobantes | Certificado o credenciales SUNAT incorrectas | Revisar certificado y datos `SOL` |
| Se pide UAC al abrir la app | Configuración heredada con `requireAdministrator` | Recompilar con el manifiesto actual (`asInvoker`) |
