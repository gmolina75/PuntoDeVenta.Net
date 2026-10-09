# Ada369 C# — Sistema de Punto de Venta con Facturación Electrónica (SUNAT)

Aplicación de escritorio para **punto de venta (POS)** orientada a comercios pequeños y medianos
(minimarkets, bodegas, farmacias, etc.). Integra venta rápida, control de caja por turnos, inventario
(kardex), compras, cartera de créditos y **facturación electrónica SUNAT (Perú)** en un único cliente
de escritorio.

- **Lenguaje / UI:** C# · Windows Forms
- **Framework:** .NET Framework **4.8**
- **Base de datos:** Microsoft SQL Server / SQL Server Express (ADO.NET + procedimientos almacenados)
- **Facturación electrónica:** cliente WCF contra el `billService` de SUNAT (UBL 2.1)
- **Salida:** `Ada369 PE.exe`

> **Proyecto de estudio / demostración.** Elaborado originalmente por **Ing. Franklin J. Bustamante
> Alejandría (codigo369)**. Este repositorio es una copia de trabajo con labores de documentación,
> modernización del build y eliminación de dependencias comerciales.

---

## Tabla de contenido

- [Descripción general](#descripción-general)
- [Funcionalidades](#funcionalidades)
- [Arquitectura](#arquitectura)
- [Stack tecnológico](#stack-tecnológico)
- [Requisitos](#requisitos)
- [Inicio rápido](#inicio-rápido)
- [Configuración](#configuración)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Modelo de datos](#modelo-de-datos)
- [Estado del proyecto](#estado-del-proyecto)
- [Mejoras implementadas](#mejoras-implementadas)
- [Oportunidades pendientes](#oportunidades-pendientes)
- [Créditos y licencia](#créditos-y-licencia)

---

## Descripción general

Ada369 es un POS de escritorio que cubre el ciclo operativo completo de un negocio de venta al
detalle: desde la configuración inicial del negocio y la creación de la base de datos, pasando por la
operación diaria de caja y ventas, hasta la emisión de comprobantes electrónicos aceptados por SUNAT.

El sistema está pensado para operar con **lector de código de barras**, soporta **múltiples cajas**
(conexión a un servidor de base de datos remoto), **balanza electrónica**, **impresión de tickets** y
**respaldos automáticos** de la base de datos.

La conexión a datos **no** reside en `app.config`: se almacena **cifrada (AES-256)** en
`ConnectionString.xml`, generado por el asistente de instalación en el primer arranque.

## Funcionalidades

### Operación comercial

| Módulo | Funcionalidad |
| --- | --- |
| **Punto de venta (POS)** | Venta rápida con búsqueda por código, nombre o lectora; venta por peso (balanza); múltiples comprobantes; cálculo de vuelto; impresión de ticket. |
| **Clientes** | Alta, edición y búsqueda de clientes por DNI/RUC, con datos fiscales para facturación electrónica. |
| **Historial de ventas** | Consulta de ventas registradas y **reimpresión** de comprobantes. |
| **Caja** | Apertura y cierre de caja, **cierre por turno**, consulta de movimientos y arqueo. |
| **Gastos e ingresos varios** | Registro de egresos e ingresos que afectan el flujo de caja. |
| **Compras** | Registro de compras a proveedores, detalle por ítem e historial de compras. |
| **Inventarios** | Kardex de **entradas y salidas**, actualización de stock y precios, inventario de productos, alerta de **productos bajo mínimo** y **productos vencidos**. |
| **Cobros** | Medios de cobro y registro de pagos de créditos. |
| **Apertura de crédito** | Gestión de **cuentas por cobrar** y **cuentas por pagar**. |

### Panel y administración

| Módulo | Funcionalidad |
| --- | --- |
| **Login** | Selección de usuario con foto, teclado numérico en pantalla y roles (**Administrador / Cajero**). |
| **Panel de control (Dashboard)** | Indicadores de ventas del período, ganancias, productos más vendidos, cantidad de productos y clientes, productos bajo mínimo, cuentas por cobrar/pagar y **gráficas** de ventas y gastos. |
| **Asistente de instalación** | Creación de la base de datos, tablas y procedimientos almacenados; registro de empresa, caja, ticket y usuario principal. |
| **Configuraciones** | Empresa, productos (con **códigos SUNAT** y unidades de medida), proveedores, clientes, cajas, impresoras, correo, respaldos, serialización, usuarios/permisos, **diseño de ticket** y **balanza**. |
| **Respaldos de base de datos** | Generación de copias de seguridad (`BACKUP DATABASE`) manuales y **automáticas por temporizador**. |
| **Conexión remota** | Conexión de una **caja secundaria** a un servidor de base de datos remoto (conexión manual). |
| **Licenciamiento** | Licencia de prueba de 30 días asociada al **serial del disco** de la máquina (tabla `Marcan`). |

### Reportes

El sistema incluye reportes operativos: **movimientos de kardex**, **inventario de productos**,
**productos bajo mínimo**, **productos vencidos**, **resumen de ventas**, **cuentas por cobrar/pagar** e
**impresión de comprobantes**. Se presentan en un visor propio con opciones de *Imprimir* y
*Actualizar*.

### Facturación electrónica (SUNAT)

| Comprobante | Descripción |
| --- | --- |
| **Facturas y boletas** | Emisión, firma XML (UBL 2.1) y envío al `billService` de SUNAT. |
| **Notas de crédito** | Emisión con motivo y documento de referencia. |
| **Notas de débito** | Emisión con motivo y documento de referencia. |
| **Resumen diario de boletas** | Envío del resumen diario. |
| **Comunicación de baja** | Anulación de comprobantes. |
| **CDR** | Descarga y lectura del **Constancia de Recepción** (respuesta de SUNAT). |

## Arquitectura

La solución es un único proyecto (`Ada369Csharp`), organizado en capas:

```
Program.cs ──► LOGIN ──► Asistente de instalación (1.ª ejecución)
                  └────► MenuPrincipal ──► PuntoDeVenta / Menús por módulo

Presentacion   Interfaz WinForms, organizada en carpetas por módulo (55 formularios/controles).
Logica         Modelos y parámetros que viajan entre Presentacion y Datos.
Datos          Acceso a datos por entidad (Dventas, Dclientes, Dproductos, ...) vía ADO.NET.
CONEXION       Conexión maestra, cifrado/descifrado (AES), claves (KeyProvider) y ejecución de scripts.
Sunat          Generación de XML/UBL, firma digital y envío al servicio de SUNAT.
Connected Services  Proxy WCF generado para el billService de SUNAT.
Reportes       Librería propia de reportes (modelo + visor + impresión GDI+).
```

**Flujo de primer arranque:** `LOGIN` intenta leer `ConnectionString.xml`. Si no existe o no hay
conexión, deriva a `Opcionesprincipales` → `Instalacionservidor`, que crea la base de datos y ejecuta
los procedimientos almacenados; luego `Registroempresa` y `UsuarioPrincipal`.

## Stack tecnológico

- **Lenguaje:** C# (Windows Forms).
- **Framework:** .NET Framework 4.8.
- **Acceso a datos:** ADO.NET (`System.Data.SqlClient`), `DataTable`/`DataSet` y procedimientos almacenados.
- **Base de datos:** Microsoft SQL Server / SQL Server Express.
- **Facturación electrónica:** cliente WCF (`System.ServiceModel`) contra el `billService` de SUNAT (UBL 2.1).
- **Reportes:** librería propia `Ada369Csharp.Reportes` (visor WinForms + impresión GDI+). **Sin Telerik.**
- **Compresión/ZIP:** `System.IO.Compression` (framework). **Sin DotNetZip.**
- **Otros:** `System.Management` (serial del disco), `System.Net.Mail` (correo), `System.Windows.Forms.DataVisualization` (gráficas).

## Requisitos

- **Windows** 10/11 (x64).
- **Visual Studio 2019/2022/2026** con la carga de trabajo *Desarrollo de escritorio de .NET*.
- **.NET Framework 4.8** (targeting pack y runtime).
- **SQL Server** (Express o Developer) accesible por *Integrated Security*.
- **No requiere `sqlcmd`:** el asistente ejecuta el esquema mediante ADO.NET (`SqlScriptRunner`).
- El manifiesto `ada369m.manifest` usa `asInvoker` (sin elevación permanente). Solo se solicita UAC al
  lanzar el instalador de SQL Server Express.

> No se requieren paquetes NuGet ni dependencias comerciales: el proyecto usa únicamente ensamblados
> del framework y `UIDC.dll` (incluida en el repositorio).

## Inicio rápido

Guía detallada en **[docs/INSTALACION.md](docs/INSTALACION.md)**. Resumen:

```powershell
# 1. Compilar (usar MSBuild de Visual Studio)
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msbuild Ada369Csharp.sln /p:Configuration=Debug

# 2. Ejecutar pruebas (opcional)
.\Ada369Csharp.Tests\bin\Debug\Ada369Csharp.Tests.exe

# 3. Ejecutar la aplicación
.\Ada369Csharp\bin\Debug\"Ada369 PE.exe"
```

En la primera ejecución se lanza el **asistente de instalación**, que crea la base de datos (por
defecto `BASEADACURSO` en `.\SQLEXPRESS`), los procedimientos almacenados, la empresa, la caja y el
usuario principal.

## Configuración

- **Cadena de conexión:** cifrada (AES-256) en `ConnectionString.xml`, generada por el asistente.
- **Clave de cifrado:** ya **no** está incrustada en el código. `CONEXION/KeyProvider.cs` la resuelve,
  en este orden: variable de entorno `ADA369_CRYPTO_KEY` → appSetting `Ada369:CryptoKey` →
  archivo `ada369.key` protegido con DPAPI → clave heredada (con advertencia en el log). Ver
  [docs/INSTALACION.md](docs/INSTALACION.md#clave-de-cifrado).
- **Logs:** `Ada369Csharp/Logging/Logger.cs` escribe un log diario en
  `%LOCALAPPDATA%\Ada369\logs` y muestra errores amigables sin exponer *stack traces*.
- **Endpoints SUNAT:** en `Ada369Csharp/app.config` (servicio `billService`). Los modos de prueba (beta)
  se seleccionan dentro de los formularios de SUNAT.
- **Script de base de datos:** `script.sql` (raíz) contiene el esquema completo. El asistente embebe su
  propia copia en `Presentacion/Asistenteinstalacion/Instalacionservidor.resx`.

## Estructura del proyecto

```
Ada369Csharp.sln              Solución (VS 2019+)
script.sql                    Esquema completo de la base de datos
UIDC.dll                      Dependencia auxiliar (referenciada por el proyecto)
LICENSE                       Licencia del proyecto
.github/workflows/build.yml   Pipeline de CI
docs/INSTALACION.md           Guía de instalación y puesta en marcha
Ada369Csharp.Tests/           Pruebas automatizadas (ejecutor propio, sin NuGet)
Ada369Csharp/
  Program.cs                  Punto de entrada
  app.config                  Configuración WCF (SUNAT)
  ada369m.manifest            Manifiesto (asInvoker)
  Logging/                    Logger centralizado
  CONEXION/                   Conexión maestra, cifrado (KeyProvider) y SqlScriptRunner
  Datos/                      Acceso a datos por entidad
  Logica/                     Modelos / parámetros
  Presentacion/               Formularios WinForms, organizados por módulo
    Asistenteinstalacion/  Caja/  Cobros/  Compras/  Configuraciones/
    Conexion_remota/  CopiasBd/  Dashboard/  Gastos_varios/  HistorialVentas/
    Ingresos_varios/  Inventarios/  Licencia/  LOGIN/  Menu/  Reportes/
    SunatForms/  Ventas/  Apertura_de_credito/
  Reportes/                   Librería de reportes (modelo, visor, impresión)
  Sunat/                      Facturación electrónica (UBL, firma, envíos)
  Connected Services/         Proxy WCF de SUNAT
  Resources/                  Imágenes e íconos
```

## Modelo de datos

El esquema (`script.sql`) define **32 tablas** y **16 procedimientos almacenados**. Las entidades
principales giran en torno a ventas, compras, productos/inventario, caja, clientes/proveedores,
usuarios/permisos y comprobantes electrónicos.

## Estado del proyecto

- **Compila y genera `Ada369 PE.exe`** con .NET Framework 4.8; incluye **pruebas automatizadas** y
  **CI** (GitHub Actions) que compila en Release y ejecuta las pruebas.
- **Sin dependencias comerciales:** se retiró **Telerik Reporting** y **DotNetZip**; los reportes usan
  la librería propia `Ada369Csharp.Reportes` y el ZIP usa `System.IO.Compression`.
- **Seguridad:** la clave AES ya no está incrustada (ver `KeyProvider`); el SQL dinámico sensible está
  parametrizado/saneado; el ejecutable corre como `asInvoker`.
- El asistente de instalación **no depende de `sqlcmd`**: ejecuta el esquema desde ADO.NET.

## Mejoras implementadas

- **Seguridad**
  - Clave AES fuera del código (`CONEXION/KeyProvider.cs`), con soporte de variable de entorno,
    configuración, archivo DPAPI y clave heredada.
  - `BACKUP DATABASE` y `CREATE DATABASE` parametrizados/validados (`SqlScriptRunner.IdentificadorSeguro`).
  - Manifiesto `requireAdministrator` → **`asInvoker`**; el instalador de SQL Server se lanza con UAC
    bajo demanda (`Verb=runas`).
- **Errores y logging**
  - Logger central (`Logging/Logger.cs`) con archivo diario.
  - Reemplazadas **124** llamadas a `MessageBox.Show(ex.StackTrace)` por logging con mensaje amigable.
- **Datos e instalación**
  - Ejecución de scripts T-SQL por lotes `GO` desde ADO.NET (`CONEXION/SqlScriptRunner.cs`), con
    parámetros de nombre de BD y carpeta de datos.
- **Calidad y DevOps**
  - Proyecto de **pruebas** (`Ada369Csharp.Tests`, sin NuGet) que cubre el divisor de lotes, el
    saneamiento de identificadores y el ciclo de cifrado AES.
  - **Pipeline CI** en `.github/workflows/build.yml`.
  - `app.config` alineado a **v4.8**; limpieza de artefactos de VS (`_UpgradeWizard_Files/`,
    `UpgradeWizardLog.xml`); `.gitignore` ampliado; **`LICENSE`** añadida.
  - Eliminados archivos y carpetas con caracteres no ASCII (`Diseñoticket`, `diseño`).

## Oportunidades pendientes

No bloqueantes; quedan para el trabajo continuo.

1. **Capa de datos tipada:** sustituir el uso intensivo de `DataTable`/strings por modelos/DTOs y una
   capa de repositorios (o **Dapper**) para reducir errores en tiempo de ejecución.
2. **Separar lógica de UI:** los formularios concentran lógica de negocio (god-forms); extraer a
   servicios para facilitar pruebas y mantenimiento.
3. **Asincronía:** introducir `async/await` en accesos a red/BD para no bloquear la interfaz.
4. **Reportes:** consolidar la librería propia o evaluar alternativas libres (QuestPDF / RDLC) con
   plantillas desacopladas del código.
5. **Migrar a `.csproj` SDK-style** con `PackageReference`. *Diferido*: los *Connected Services* (WCF
   de SUNAT) y el diseñador WinForms en `net48` no son soportados de forma fiable en formato SDK; el
   cambio conlleva riesgo y no aporta valor inmediato.
6. **Protección del certificado digital SUNAT** y de las credenciales `sol` (almacenamiento seguro y
   rotación).
7. **Migraciones versionadas** del esquema de base de datos.
8. **Internacionalización (i18n)**, accesibilidad y estilos consistentes.

## Créditos y licencia

- **Autor original:** Ing. Franklin J. Bustamante Alejandría — [codigo369.com](https://codigo369.com)
  — Canales: [YouTube](https://www.youtube.com/c/Codigo369) ·
  [Facebook](https://www.facebook.com/codigo.369.official) ·
  [Instagram](https://www.instagram.com/codigo369/)
- **Cursos:** [Versión 1 (sin fact. electrónica)](https://www.udemy.com/course/sistema-de-ventas-profesional-en-c-y-sqlserver/) ·
  [Versión 2 (con fact. electrónica)](https://www.udemy.com/course/facturacion-electronica-sunat-ubl-21-peru-en-c-y-sqlserver/)
- **Licencia:** ver el archivo [`LICENSE`](LICENSE). Es material de estudio del curso de codigo369;
  respeta las condiciones del autor original antes de redistribuirlo o usarlo comercialmente.

> **“Cualquiera puede programar.”**
