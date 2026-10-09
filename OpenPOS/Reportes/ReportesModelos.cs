using OpenPOS.Reportes;

namespace OpenPOS.Presentacion.REPORTES.Impresion_de_comprobantes
{
    public class Ticket_report : ReportModel
    {
        public Ticket_report()
        {
            Titulo = "Comprobante de venta";
            EsTicket = true;
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.RCobros
{
    public class rptCobros : ReportModel
    {
        public rptCobros()
        {
            Titulo = "Reporte de cobros";
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.ReportePorCobrar
{
    public class ReporteCobrar : ReportModel
    {
        public ReporteCobrar()
        {
            Titulo = "Reporte por cobrar";
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.ReportePorPagar
{
    public class ReportePagar : ReportModel
    {
        public ReportePagar()
        {
            Titulo = "Reporte por pagar";
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.ReporteVentas
{
    public class ResumenVentas : ReportModel
    {
        public ResumenVentas()
        {
            Titulo = "Resumen de ventas";
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.REPORTES_DE_KARDEX_listo.Reporte_de_Kardex_diseno
{
    public class ReportKARDEX_Movimientos_ok : ReportModel
    {
        public ReportKARDEX_Movimientos_ok()
        {
            Titulo = "Movimientos de kardex";
        }
    }
}

namespace OpenPOS.Presentacion.REPORTES.REPORTES_DE_KARDEX_listo.REPORTES_DE_INVENTARIOS_todos
{
    public class ReportePbajomin : ReportModel
    {
        public ReportePbajomin()
        {
            Titulo = "Productos con bajo mínimo";
        }
    }

    public class ReportePVencidos : ReportModel
    {
        public ReportePVencidos()
        {
            Titulo = "Productos vencidos";
        }
    }

    public class Reporte_Movimientos_con_filtros : ReportModel
    {
        public Reporte_Movimientos_con_filtros()
        {
            Titulo = "Movimientos con filtros";
        }
    }

    public class ReportInventarios_Todos : ReportModel
    {
        public ReportInventarios_Todos()
        {
            Titulo = "Inventario de productos";
        }
    }

    public class ReportMovimientosBuscar : ReportModel
    {
        public ReportMovimientosBuscar()
        {
            Titulo = "Movimientos del producto";
        }
    }
}
