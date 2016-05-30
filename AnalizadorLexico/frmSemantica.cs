using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalizadorLexico
{
    public partial class frmSemantica : Form
    {
        string Codigo;
        public frmSemantica(string codigo)
        {
            codigo = codigo.Replace(" ", "");
            this.Codigo = codigo;
            LimpiarOperadores();
            InitializeComponent();
        }

        private void frmSemantica_Load(object sender, EventArgs e)
        {
            Conexion.CambiarASemantica();
            string[] lineas = Codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lineas.Count(); i++)
            {
                if (lineas[i] != "S")
                {
                    lineas[i] = Semantica(lineas[i]);
                }
            }
            bool dif = false;
            
            //COMPROBACION
            foreach (string v in lineas)
            {
                txtSemantica.Text += v + "\r\n";
                if (v != "S")
                {
                    dif = true;
                }
            }

            if (dif)
                MessageBox.Show("No se acepta.");
            else
                MessageBox.Show("Se acepta.");
        }

        private string Semantica(string linea)
        {
            int NT = linea.Length / 4;
            int N = NT;
            string cade = "";
            DataTable dt;
            bool bandera = false;
            while (N >= 1)
            {
                int desp = NT - N;
                for (int j = 0; j <= desp; j++)
                {
                    bandera = false;
                    cade = linea.Substring((j * 4), (N * 4));
                    dt = Conexion.query("Select `LADO IZQUIERDO` From [Hoja1$] Where `LADO DERECHO` = '" + cade + "'");
                    if (dt.Rows.Count > 0)
                    {
                        string remp = dt.Rows[0][0].ToString();
                        // MessageBox.Show(string.Format("Remplazo:\n{0}\npor\n{1}", cade, remp));
                        linea = linea.Replace(cade, remp);
                        if (linea == "S")
                        {
                            return linea;
                        }
                        else
                        {
                            N = linea.Length / 4;
                            NT = N;
                            j = desp;
                        }
                        bandera = true;
                    }

                }
                if (!bandera)
                    N--;
            }
            return linea;
        }

        private void LimpiarOperadores()
        {
            Codigo = Codigo.Replace("OPA1", "OPAR").Replace("OPA2", "OPAR").Replace("OPA3", "OPAR").Replace("OPA4", "OPAR");
        }

    }
}
