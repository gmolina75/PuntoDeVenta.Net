using System.Collections.Generic;
using System.Data;

namespace OpenPOS.Reportes
{
    public class ReportColumn
    {
        public string Header { get; set; }
        public string Field { get; set; }

        public ReportColumn() { }

        public ReportColumn(string header, string field)
        {
            Header = header;
            Field = field;
        }
    }

    public class ReportModel
    {
        public string Titulo { get; set; }
        public DataTable DataSource { get; set; }
        public List<ReportColumn> Columnas { get; set; }
        public bool EsTicket { get; set; }
        public string Empresa { get; set; }
        public byte[] Logo { get; set; }

        private readonly TablaShim _tabla;

        public TablaShim table1 { get { return _tabla; } }
        public TablaShim Table1 { get { return _tabla; } }

        public ReportModel()
        {
            Columnas = new List<ReportColumn>();
            _tabla = new TablaShim(this);
        }

        public class TablaShim
        {
            private readonly ReportModel _owner;

            public TablaShim(ReportModel owner)
            {
                _owner = owner;
            }

            public DataTable DataSource
            {
                get { return _owner.DataSource; }
                set { _owner.DataSource = value; }
            }
        }
    }
}
