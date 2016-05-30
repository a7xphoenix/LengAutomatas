using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace AnalizadorLexico
{
    static class Conexion
    {
        static string connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"Matrizzz.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
        //static string connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"Sintaxisss.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
        static public OleDbConnection Conn = new OleDbConnection(connectionString);
        static public OleDbCommand cmd;
        static public OleDbDataAdapter da = new OleDbDataAdapter();
        static public OleDbDataReader dr;

        static public DataTable query(string query)
        {
            cmd = new OleDbCommand(query, Conn);
            Conn.Open();
            da.SelectCommand = cmd;
            DataTable dt = new DataTable();
            da.Fill(dt);
            Conn.Close();
            return dt;
        }

        public static void CambiarASintaxis()
        {
            connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"Sintaxisss.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
            Conn = new OleDbConnection(connectionString);
        }

        public static void CambiarALexico()
        {
            connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"Matrizzz.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
            Conn = new OleDbConnection(connectionString);
        }

        public static void CambiarAAsignacion()
        {
            connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"asignacion.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
            Conn = new OleDbConnection(connectionString);
        }
        public static void CambiarASemantica()
        {
            connectionString = "Provider=\"Microsoft.ACE.OLEDB.12.0\";Data Source=\"semantica.xlsx\";Extended Properties=\"Excel 12.0 Xml;HDR=YES\";";
            Conn = new OleDbConnection(connectionString);
        }
    }
}
