using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace AnalizadorLexico
{
    public partial class frmPostfijo : Form
    {
        DataGridView[] dtg = null;
        public frmPostfijo(string c, DataGridView[] dtg)
        {
            this.dtg = dtg;
            c = c.Replace(" ", "");
            InitializeComponent();
            txtTokens.Text = c;
        }

        private string Postfijo(string linea)
        {
            int NT = linea.Length / 4 - 3;
            //MessageBox.Show(NT.ToString());
            int N = NT;
            string cade = "";
            DataTable dt;
            bool bandera = false;
            while (N > 3)
            {
                int desp = NT - N;
                for (int j = 0; j <= desp; j++)
                {
                    bandera = false;
                    cade = linea.Substring((j * 4), (N * 4));
                    dt = Conexion.query("Select `LADO IZQUIERDO` From [SHEET$] Where `LADO DERECHO` = '" + cade + "'");
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

        string Estado = "Ninguno";
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            txtPostfijo.Clear();
            //txtPrefijo.Clear();

            string Codigo = txtTokens.Text;
            string[] lineasAsig = Codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Estado = "Ninguno"; //Ninguno, IF, Contenido-IF, ELSE, Contenido-ELSE
            foreach (string linea in lineasAsig)
            {
                if (linea.Contains("PR06") || linea.Contains("PR02"))
                {
                    txtPostfijo.Text += linea + "\r\n";
                }
                else
                {
                    if (!linea.Contains("CE12") && !linea.Contains("PR11") && !linea.Contains("PR07"))
                    {
                        if (linea.Contains("PR10"))
                            Estado = "IF";
                        else if (Estado == "IF")
                            Estado = "Contenido-IF";
                        else if (Estado == "ELSE")
                            Estado = "Contenido-ELSE";

                        string post = AnalizarPos(linea);
                        if (linea.Contains("PR10"))
                            txtPostfijo.Text += "PR10CE01POST" + post.Replace("ASIG", "") + "POSTCE02CE11\r\n";
                        else if (linea.Contains("PR08"))
                            txtPostfijo.Text += "PR08CE01POST" + post.Replace("ASIG", "") + "POSTCE02CE11\r\n";
                        else
                            txtPostfijo.Text += post + "\r\n";
                    }
                    else if (linea.Contains("CE12"))
                    {
                        txtPostfijo.Text += "CE12" + "\r\n";
                        if (Estado == "Contenido-IF")
                            Estado = "ELSE";
                        else
                            Estado = "Ninguno";
                    }
                    else if (linea.Contains("PR11"))
                        txtPostfijo.Text += linea + "\r\n";
                    //txtPrefijo.Text += AnalizarPre(linea) + "\r\n";
                }
            }
        }

        private string AnalizarPos(string linea)
        {
            string CADENA = "";
            int desplazamientos = linea.Length / 4;
            string IDENT = "";
            string[] constantes = new string[] { "EC", "CR", "CD", "XE", "ER" };
            string[] ignorar = new string[] { "PR", "CE" };
            string[] operadores = new string[] { "OPA", "OPL", "OPR" };
            List<string> Pila = new List<string>();
            for (int j = 0; j < desplazamientos; j++)
            {
                string TOKEN = linea.Substring((j * 4), 4);

                if (TOKEN.Substring(0, 2) == "ID")
                {
                    IDENT = TOKEN;
                    CADENA += TOKEN;
                }
                else if (TOKEN == "ASIG")
                {
                    //CADENA += TOKEN;
                }
                else if (constantes.Contains(TOKEN.Substring(0, 2)))
                {
                    CADENA += TOKEN;
                }
                //AGREGAR A LA PILA
                //else if (TOKEN.Substring(0, 3) == "OPA")
                else if (operadores.Contains(TOKEN.Substring(0, 3)))
                {
                    if (TOKEN == "OPA1" || TOKEN == "OPA2") // + -
                    {
                        //No hay nada
                        if (Pila.Count == 0)
                            Pila.Add(TOKEN);
                        //Si en la pila hay un relacional o un lógico
                        else if (Pila[Pila.Count - 1].Substring(0, 3) == "OPR" || Pila[Pila.Count - 1].Substring(0, 3) == "OPL")
                        {
                            Pila.Add(TOKEN);
                        }
                        //llega uno de igual o menor prioridad
                        else if (operadores.Contains(TOKEN.Substring(0, 3)))
                        {
                            //Se vacia la pila y se añade el nuevo
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA += Pila[i];
                                Pila.RemoveAt(i);
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                    if (TOKEN == "OPA3" || TOKEN == "OPA4") // / *
                    {
                        //No hay nada
                        if (Pila.Count == 0)
                            Pila.Add(TOKEN);
                        //Si en la pila hay un relacional o un lógico
                        else if (Pila[Pila.Count - 1].Substring(0, 3) == "OPR" || Pila[Pila.Count - 1].Substring(0, 3) == "OPL")
                        {
                            Pila.Add(TOKEN);
                        }
                        //el que llega es de mayor prioridad
                        else if (Pila[Pila.Count - 1] == "OPA1" || Pila[Pila.Count - 1] == "OPA2")
                            Pila.Add(TOKEN);
                        //Si el que llega uno de igual prioridad
                        else if (Pila[Pila.Count - 1] == "OPA3" || Pila[Pila.Count - 1] == "OPA4")
                        {
                            //Se vacia la pila y se añade el nuevo
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA += Pila[i];
                                Pila.RemoveAt(i);
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                    if (TOKEN.Substring(0, 3) == "OPR")
                    {
                        //Nohay nada
                        if (Pila.Count == 0)
                            Pila.Add(TOKEN);
                        //Si en la pila hay un lógico
                        else if (Pila[Pila.Count - 1].Substring(0, 3) == "OPL")
                        {
                            MessageBox.Show("Se añade el relacional " + TOKEN.Substring(0, 3));
                            Pila.Add(TOKEN);
                        }
                        //Si llega uno de menor o igual prioridad
                        //Si en la pila hay un relaciona
                        else if (Pila[Pila.Count - 1].Substring(0, 3) == "OPA" || Pila[Pila.Count - 1].Substring(0, 3) == "OPR")
                        {
                            //Se vacia la pila y se añade el nuevo
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                if (Pila[i].Substring(0, 3) != "OPL")
                                {
                                    CADENA += Pila[i];
                                    Pila.RemoveAt(i);
                                }
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                    if (TOKEN.Substring(0, 3) == "OPL")
                    {
                        //Nohay nada
                        if (Pila.Count == 0)
                            Pila.Add(TOKEN);
                        //Si llega uno de menor o igual prioridad
                        else
                        {
                            //Se vacia la pila y se añade el nuevo
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA += Pila[i];
                                Pila.RemoveAt(i);
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                }
                else if (TOKEN == "CE01")
                {
                    j++;
                    string sub = linea.Substring((j * 4), linea.Substring(j * 4).IndexOf("CE02"));
                    CADENA += AnalizarPos(sub);
                    j = (linea.Substring((j * 4)).IndexOf("CE02") + (j * 4)) / 4;
                }
            }
            for (int i = Pila.Count - 1; i >= 0; i--)
            {
                CADENA += Pila[i];
                Pila.RemoveAt(i);
            }
            CADENA += "ASIG";
            return CADENA;
        }

        private string AnalizarPre(string linea)
        {
            string CADENA = "";
            int desplazamientos = linea.Length / 4;
            //string IDENT = "";
            string[] constantes = new string[] { "EC", "CR", "XE", "ER" };
            List<string> Pila = new List<string>();
            string PrimerID = "";
            for (int j = 0; j < desplazamientos; j++)
            {
                string TOKEN = linea.Substring((j * 4), 4);

                if (TOKEN.Substring(0, 2) == "ID")
                {
                    if (PrimerID == "")
                    {
                        PrimerID = TOKEN;
                        //MessageBox.Show(PrimerID);
                    }
                    else
                        CADENA += TOKEN;
                }
                else if (TOKEN == "ASIG")
                {
                    PrimerID = TOKEN + PrimerID;
                    //MessageBox.Show(PrimerID);
                }
                else if (constantes.Contains(TOKEN.Substring(0, 2)))
                {
                    CADENA += TOKEN;
                }
                //AGREGAR A LA PILA
                else if (TOKEN.Substring(0, 3) == "OPA")
                {
                    if (Pila.Count == 0)
                    {
                        Pila.Add(TOKEN);
                    }
                    else if (TOKEN == "OPA1" || TOKEN == "OPA2") // + -
                    {
                        if (Pila[Pila.Count - 1] == "OPA1" || Pila[Pila.Count - 1] == "OPA2")
                        {
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA = Pila[Pila.Count - 1] + CADENA; //El ultimo de la pila se añade a l principio
                                Pila.RemoveAt(i); // Se elimina de la pila
                            }
                            Pila.Add(TOKEN); //Se añade el nuevo a la pila
                        }
                        else if (Pila[Pila.Count - 1] == "OPA3" || Pila[Pila.Count - 1] == "OPA4")
                        {
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA = Pila[Pila.Count - 1] + CADENA; //El ultimo de la pila se añade a l principio
                                Pila.RemoveAt(i); // Se elimina de la pila
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                    else if (TOKEN == "OPA3" || TOKEN == "OPA4") // / *
                    {
                        if (Pila[Pila.Count - 1] == "OPA1" || Pila[Pila.Count - 1] == "OPA2")
                        {
                            CADENA = CADENA.Substring(0, CADENA.Length - 4) + TOKEN + CADENA.Substring(CADENA.Length - 4);
                        }
                        else if (Pila[Pila.Count - 1] == "OPA3" || Pila[Pila.Count - 1] == "OPA4")
                        {
                            for (int i = Pila.Count - 1; i >= 0; i--)
                            {
                                CADENA = Pila[Pila.Count - 1] + CADENA; //El ultimo de la pila se añade a l principio
                                Pila.RemoveAt(i); // Se elimina de la pila
                            }
                            Pila.Add(TOKEN);
                        }
                    }
                }
                else if (TOKEN == "CE01")
                {
                    j++;
                    string sub = linea.Substring((j * 4), linea.Substring(j * 4).IndexOf("CE02"));
                    CADENA += AnalizarPos(sub);
                    j = (linea.Substring((j * 4)).IndexOf("CE02") + (j * 4)) / 4;
                }
            }
            for (int i = Pila.Count - 1; i >= 0; i--)
            {
                CADENA = Pila[Pila.Count - 1] + CADENA;
                Pila.RemoveAt(i);
            }
            CADENA = PrimerID + CADENA;
            return CADENA;
        }

        private void btnTripletas_Click(object sender, EventArgs e)
        {
            Tripletas();
        }

        private void Tripletas()
        {
            List<int> PilaIF = new List<int>();
            dtgTRtrue.Rows.Clear();
            dtgTRfalse.Rows.Clear();
            dtgTripletas.Rows.Clear();
            string[] VARI = new string[] { "ID", "CD", "EC", "CR", "TE" };
            List<int> ATRTrue = new List<int>();
            List<int> ATRFalse = new List<int>();
            int TRTrue = 0;
            int TRFalse = 0;

            string CodPostfijo = txtPostfijo.Text;
            string[] lineas = CodPostfijo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            int ContIF = 0;
            int ContWh = 0;
            string Modulo = "inicio";
            int ContTemp = 0;
            foreach (string str in lineas)
            {
                if (Modulo.Contains("IF"))
                {
                    Modulo = "TRtrue" + ContIF;
                    dtgTripletas.Rows.Add("TR" + ContIF, "TRtrue" + ContIF);
                }
                else if (Modulo.Contains("ELSE"))
                {
                    dtgTripletas.Rows.Add("jmp", "Continua" + ContIF);
                    Modulo = "TRfalse" + ContIF;
                    dtgTripletas.Rows.Add("TR" + ContIF,"TRfalse" + ContIF);
                }
                else if (Modulo.Contains("Continua"))
                {
                    dtgTripletas.Rows.Add("jmp", "Continua" + ContIF);
                    Modulo = "inicio" + ContIF;
                    dtgTripletas.Rows.Add("TR" + ContIF, "Continua" + ContIF);
                }
                else if (Modulo.Contains("WHILE"))
                {
                    dtgTripletas.Rows.Add("jmp", "TRtrue" + ContIF);
                    dtgTripletas.Rows.Add("TR" + ContIF, "TRtrue" + ContIF);
                    //dtgTripletas.Rows.Add("TR" + ContIF, "WH" + ContIF);
                }

                string post = str;
                if (post.Contains("PR06CE01"))
                {
                    post = post.Replace("PR06CE01", "");
                    post = post.Replace("CE02", "");
                    dtgTripletas.Rows.Add("PRINT", post.Trim());
                }
                else if (post.Contains("PR02"))
                {
                    post = post.Replace("PR02", "");
                    dtgTripletas.Rows.Add("READ", post.Trim());
                }
                else if (post.Contains("CE12"))
                {
                    if (Modulo.Contains("TRtrue"))
                    {
                        Modulo = "ELSE" + ContIF;
                    }
                    else if (Modulo.Contains("TRfalse"))
                    {
                        Modulo = "Continua" + ContIF;
                    }
                    else if (Modulo.Contains("WHILE"))
                    {
                        Modulo = "inicio" + ContIF;
                        dtgTripletas.Rows.Add("jmp", "Continua" + ContIF);
                    }
                }
                else
                {
                    if (post.Contains("PR10"))
                    {
                        ContIF++;
                        Modulo = "IF" + ContIF;
                        //dtgTripletas.Rows.Add("AQUI INICIA", "IF" + ContIF, "");
                        post = post.Replace("PR10CE01POST", "");
                        post = post.Replace("POSTCE02", "");
                    }
                    else if (post.Contains("PR08"))
                    {
                        ContWh++;
                        Modulo = "WHILE" + ContWh;
                        dtgTripletas.Rows.Add("jmp", "WHILE" + ContWh);
                        dtgTripletas.Rows.Add("TR" + ContWh, "WHILE" + ContWh);
                        //dtgTripletas.Rows.Add("AQUI INICIA", "IF" + ContIF, "");
                        post = post.Replace("PR08CE01POST", "");
                        post = post.Replace("POSTCE02", "");
                    }
                    int cont = 0;
                    for (int i = 0; i < post.Length / 4; i++)
                    {
                        if (VARI.Contains(post.Substring(i * 4, 2)))
                        {
                            cont++;
                        }
                        else if (cont >= 2)
                        {
                            string OPACTUAL = post.Substring(i * 4, 3);
                            StringBuilder sb = null;
                            if (post.Length / 4 > 3)
                            {
                                ContTemp++;
                                dtgTripletas.Rows.Add("TE" + ContTemp.ToString("D2"), post.Substring((i - 2) * 4, 4), "MOV");
                                sb = new StringBuilder(post);
                                sb.Remove((i - 2) * 4, 4);
                                sb.Insert((i - 2) * 4, "TE" + ContTemp.ToString("D2"));
                                post = sb.ToString();
                            }
                            else if (post.Substring((i - 2) * 4, 2) != "TE" && post.Length / 4 > 3)
                            {
                                ContTemp++;
                                dtgTripletas.Rows.Add("TE" + ContTemp.ToString("D2"), post.Substring((i - 2) * 4, 4), "MOV");
                                sb = new StringBuilder(post);
                                sb.Remove((i - 2) * 4, 4);
                                sb.Insert((i - 2) * 4, "TE" + ContTemp.ToString("D2"));
                                post = sb.ToString();
                            }
                            if (post.Substring((i - 1) * 4, 2) != "TE" && post.Length / 4 > 3)
                            {
                                ContTemp++;
                                dtgTripletas.Rows.Add("TE" + ContTemp.ToString("D2"), post.Substring((i - 1) * 4, 4), "MOV");
                                sb = new StringBuilder(post);
                                sb.Remove((i - 1) * 4, 4);
                                sb.Insert((i - 1) * 4, "TE" + ContTemp.ToString("D2"));
                                post = sb.ToString();
                            }
                            dtgTripletas.Rows.Add(post.Substring((i - 2) * 4, 4), post.Substring((i - 1) * 4, 4), post.Substring(i * 4, 4));

                            cont = 0;
                        }
                    }
                }
            }
        }

        private void dtgTripletas_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void btnNoUsados_Click(object sender, EventArgs e)
        {
            List<string> Temporales = new List<string>();
            foreach (DataGridViewRow row in dtgTripletas.Rows)
            {

            }
        }

        private void Addspace()
        {

        }

        private void btnOpt_Click(object sender, EventArgs e)
        {
            Addspace();
            string codigo = txtTokens.Text;

            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //instrucciones dependientes susceptibles a reorganización
            txtTokens.Clear();
            for (int j = 0; j < lineas.Count(); j++)
            {
                txtTokens.Text += lineas[j] + "\r\n";
                if (lineas[j].Contains("ASIG") && lineas[j] != "ASIG")
                {
                    string[] aux = lineas[j].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = j + 1; i < lineas.Count(); i++)
                    {
                        if (lineas[i].Contains(aux[1]))
                        {
                            lineas[i] = lineas[i].Replace(aux[1], aux[0]);
                        }
                    }
                }
            }
            //////
            List<string[]> Asignaciones = new List<string[]>();
            //instrucciones que se repiten sin haber tenido modificacion alguna en uno de sus valores
            codigo = txtTokens.Text;
            lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            txtTokens.Clear();
            for (int j = 0; j < lineas.Count(); j++)
            {
                bool ban = false;
                if (lineas[j].Contains("ASIG") && lineas[j] != "ASIG")
                {
                    string[] aux = lineas[j].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                    string temp = "";
                    foreach (string[] x in Asignaciones)
                    {
                        if (aux[1].Trim() == x[0].Trim())
                        {
                            ban = true;
                            temp = x[0].Trim();
                        }
                    }
                    if (ban)
                    {
                        //Comprobar si el valor cambia en el resto del código
                        for (int i = j + 1; i < lineas.Count(); i++)
                        {
                            string[] aux2 = lineas[i].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                            if (aux2[0].Trim() == aux[0].Trim() && aux2[1].Trim() != aux[1].Trim())
                            {
                                ban = false;
                            }
                        }
                    }
                    else
                        Asignaciones.Add(aux);

                    //Reemplazar código innecesario
                    if (ban)
                    {
                        for (int i = j; i < lineas.Count(); i++)
                        {
                            lineas[i] = lineas[i].Replace(aux[0].Trim(), temp);
                        }
                    }

                    if (!ban)
                        txtTokens.Text += lineas[j] + "\r\n";
                }
                else
                    txtTokens.Text += lineas[j] + "\r\n";
            }
            //instrucciones que generan un resultado que no se utiliza en todo el desarrollo del programa
            codigo = txtTokens.Text;
            lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            txtTokens.Clear();
            for (int j = 0; j < lineas.Count(); j++)
            {
                bool ban = false;
                if (lineas[j].Contains("ASIG") && lineas[j] != "ASIG")
                {
                    string[] aux = lineas[j].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = j; i < lineas.Count(); i++)
                    {
                        if (lineas[i].Contains("ASIG") && lineas[i] != "ASIG")
                        {
                            string[] aux1 = lineas[i].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                            if (aux[0].Trim() != aux1[0].Trim())
                                if (aux1[1].Trim().Contains(aux[0].Trim()))
                                {
                                    ban = true;
                                }
                        }
                        else if (lineas[i].Contains(aux[0].Trim()))
                        {
                            ban = true;
                        }


                    }
                    if (ban)
                        txtTokens.Text += lineas[j] + "\r\n";
                }
                else
                    txtTokens.Text += lineas[j] + "\r\n";
            }
            txtTokens.Text = txtTokens.Text.Trim();
            btnIniciar_Click(sender, e);
            Tripletas();
        }

        private void frmPostfijo_Load(object sender, EventArgs e)
        {

        }

        private void btnCompilar_Click(object sender, EventArgs e)
        {
            #region Declarar Variables
            string dataseg = "";
            List<string[]> Variables = new List<string[]>();
            foreach (DataGridViewRow x in dtg[0].Rows)
            {
                dataseg += x.Cells[0].Value + " db ";
                if (x.Cells[2].Value.ToString() == "CNEE")
                {
                    Variables.Add(new string[] { x.Cells[0].Value.ToString(), "CNEE" });
                    dataseg += "0";
                }
                else if (x.Cells[2].Value.ToString() == "CNRE")
                {
                    Variables.Add(new string[] { x.Cells[0].Value.ToString(), "CNRE" });
                    dataseg += "0.0";
                }
                else if (x.Cells[2].Value.ToString() == "CADE")
                {
                    Variables.Add(new string[] { x.Cells[0].Value.ToString(), "CADE" });
                    dataseg += "\"\",'$'";
                }
                dataseg += "\r\n";
            }
            foreach (DataGridViewRow x in dtg[3].Rows)
            {
                dataseg += x.Cells[0].Value + " db "+ x.Cells[1].Value + ",'$'\r\n";
            }
            #endregion
            //Traducir tripletas
            string codeseg = "";
            bool OPAR = false;
            int UltimoIF = 0;
            foreach (DataGridViewRow x in dtgTripletas.Rows)
            {
                if (x.Cells[2].Value != null)
                {
                    #region //Asignación
                    if (x.Cells[2].Value.ToString() == "ASIG")
                    {
                        string Origen = x.Cells[1].Value.ToString();
                        if (OPAR)
                        {
                            codeseg += "mov " + x.Cells[0].Value.ToString() + ", al\r\n";
                            OPAR = false;
                        }
                        else if (Origen.Contains("CD"))
                        {
                            codeseg += ";GUARDAR " + x.Cells[1].Value.ToString() + " EN " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov cx, 100\r\n";
                            codeseg += "mov di,offset " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov si,offset " + x.Cells[1].Value.ToString() + "\r\n";
                            codeseg += "rep movsb\r\n";
                        }
                        else if (Origen.Substring(0, 2) == "ID" || Origen.Substring(0, 2) == "TE")
                        {
                            codeseg += ";GUARDAR " + x.Cells[1].Value.ToString() + " EN " + x.Cells[0].Value.ToString() + "\r\n";
                            ////////////codeseg += "mov al, " + x.Cells[1].Value.ToString() + "\r\n";
                            codeseg += "mov " + x.Cells[0].Value.ToString() + ",al\r\n";
                        }
                        else
                        {
                            codeseg += ";GUARDAR " + BuscarEnTablaS(x.Cells[1].Value.ToString()) + " EN " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov " + x.Cells[0].Value.ToString() + "," + BuscarEnTablaS(x.Cells[1].Value.ToString()) + "\r\n";
                        }

                    }
                    else if (x.Cells[2].Value.ToString() == "MOV")
                    {
                        dataseg += x.Cells[0].Value.ToString() + " db 0\r\n";

                        if (x.Cells[1].Value.ToString().Substring(0, 2) == "ID")
                        {
                            codeseg += ";GUARDAR " + x.Cells[1].Value.ToString() + " EN " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov al, " + x.Cells[1].Value.ToString() + "\r\n";
                            codeseg += "mov " + x.Cells[0].Value.ToString() + ",al\r\n";
                        }
                        else
                        {
                            codeseg += ";GUARDAR " + BuscarEnTablaS(x.Cells[1].Value.ToString()) + " EN " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov " + x.Cells[0].Value.ToString() + "," + BuscarEnTablaS(x.Cells[1].Value.ToString()) + "\r\n";
                        }
                    }
                    #endregion
                    #region //OPAR
                    else if (x.Cells[2].Value.ToString().Substring(0, 3) == "OPA")
                    {
                        if (x.Cells[2].Value.ToString() == "OPA1") //SUMA
                        {
                            codeseg += ";SUMAR " + x.Cells[1].Value.ToString() + " + " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov al, " + x.Cells[1].Value.ToString() + "\r\n";
                            codeseg += "add al, " + x.Cells[0].Value.ToString() + "";
                            OPAR = true;
                        }
                        else if (x.Cells[2].Value.ToString() == "OPA2") //RESTA
                        {
                            codeseg += ";RESTAR " + x.Cells[1].Value.ToString() + " - " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov al, " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "sub al, " + x.Cells[1].Value.ToString() + "";
                            OPAR = true;
                        }
                        else if (x.Cells[2].Value.ToString() == "OPA3") //DIV
                        {
                            codeseg += ";DIVIDIR " + x.Cells[1].Value.ToString() + " / " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov al, " + x.Cells[1].Value.ToString() + "\r\n";
                            codeseg += "div " + x.Cells[0].Value.ToString() + "";
                            OPAR = true;
                        }
                        else if (x.Cells[2].Value.ToString() == "OPA4") //MUL
                        {
                            codeseg += ";MULTIPLICAR " + x.Cells[1].Value.ToString() + " * " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mov al, " + x.Cells[0].Value.ToString() + "\r\n";
                            codeseg += "mul " + x.Cells[1].Value.ToString() + "";
                            OPAR = true;
                        }
                    }
                    #endregion
                    #region //OPRE
                    else if (x.Cells[2].Value.ToString().Substring(0, 3) == "OPR")
                    {
                        codeseg += ";COMPARAR " + x.Cells[1].Value.ToString() + " CON " + x.Cells[0].Value.ToString() + "\r\n";
                        codeseg += "mov al, " + x.Cells[0].Value.ToString() + "\r\n";
                        codeseg += "cmp al, " + x.Cells[1].Value.ToString() + "\r\n";
                        if (x.Cells[2].Value.ToString() == "OPR1") //MAYOR QUE
                        {
                            codeseg += "jg TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        else if (x.Cells[2].Value.ToString() == "OPR2") //MENOR QUE
                        {
                            codeseg += "jl TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        else if (x.Cells[2].Value.ToString() == "OPR3") //MAYOR O IGUAL QUE
                        {
                            codeseg += "jge TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        else if (x.Cells[2].Value.ToString() == "OPR4") //MENOR O IGUAL QUE
                        {
                            codeseg += "jle TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        else if (x.Cells[2].Value.ToString() == "OPR5") //DIFERENTE DE
                        {
                            codeseg += "jne TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        else if (x.Cells[2].Value.ToString() == "OPR6") //IGUAL QUE
                        {
                            codeseg += "je TRtrue" + (UltimoIF + 1) + "\r\n";
                        }
                        codeseg += "jmp TRfalse" + (UltimoIF + 1);
                    }
                    #endregion

                    codeseg += "\r\n";
                }
                #region //Escribir en pantalla
                else if (x.Cells[0].Value.ToString() == "PRINT")
                {
                    foreach (string[] s in Variables)
                    {
                        if (s[0] == x.Cells[1].Value.ToString())
                        {
                            codeseg += ";IMPRIMIR " + x.Cells[1].Value.ToString() + "\r\n";
                            if (s[1] == "CNEE")
                            {
                                codeseg += "mov dl," + x.Cells[1].Value.ToString() + "\r\n";
                                codeseg += "add dl,30h" + "\r\n";
                                codeseg += "mov ah,02" + "\r\n";
                                codeseg += "int 21h" + "\r\n";
                            }
                            else if (s[1] == "CNRE")
                            {

                            }
                            else if (s[1] == "CADE")
                            {
                                codeseg += "mov ah,09h\r\n";
                                codeseg += "lea dx," + x.Cells[1].Value.ToString() + "\r\n";
                                codeseg += "int 21h" + "\r\n";
                            }
                            codeseg += "mov ah,02h ;Saltar linea\r\n";
                            codeseg += "mov dl,0ah ;Saltar linea\r\n";
                            codeseg += "int 21h ;Saltar linea\r\n";
                            codeseg += "\r\n";
                        }
                    }
                }
                #endregion
                #region //lECTURA TECLADO
                else if (x.Cells[0].Value.ToString() == "READ")
                {
                    foreach (string[] s in Variables)
                    {
                        if (s[0] == x.Cells[1].Value.ToString())
                        {
                            codeseg += ";LEER " + x.Cells[1].Value.ToString() + "\r\n";
                            //codeseg += "mov " + x.Cells[1].Value.ToString() + ",";
                            if (s[1] == "CNEE")
                            {
                                codeseg += "mov ah, 01\r\n";
                                codeseg += "int 21h\r\n";
                                codeseg += "sub al,30h\r\n";
                                codeseg += "mov " + x.Cells[1].Value.ToString() + ",al\r\n";
                            }
                            else if (s[1] == "CNRE")
                            {

                            }
                            else if (s[1] == "CADE")
                            {
                                codeseg += "mov ah,09h\r\n";
                                codeseg += "lea dx," + x.Cells[1].Value.ToString() + "\r\n";
                                codeseg += "int 21h" + "\r\n";
                            }
                            codeseg += "\r\n";
                        }
                    }
                }
                #endregion
                #region//Crear modulos if
                else if (x.Cells[0].Value.ToString().Substring(0, 2) == "TR")
                {
                    string num = x.Cells[1].Value.ToString().Replace("TRtrue", "").Replace("TRfalse", "").Replace("Continua", "");
                    UltimoIF = int.Parse(num);
                    codeseg += ";inicia " + x.Cells[1].Value.ToString() + "\r\n";
                    codeseg += x.Cells[1].Value.ToString() + ":\r\n";
                }
                else if (x.Cells[0].Value.ToString() == "jmp")
                {
                    codeseg += "jmp " + x.Cells[1].Value.ToString() + "\r\n";
                }
                #endregion
            }

            //Leer plantilla//
            StreamReader sr = new StreamReader("PLANTILLA.txt");
            string Plantilla = sr.ReadToEnd();
            sr.Close();
            //Reemplazar dataseg//
            Plantilla = Plantilla.Replace("<DECLARACIONES>", dataseg);
            Plantilla = Plantilla.Replace("<CODIGO>", codeseg);

            //Imprimir plantilla
            //MessageBox.Show(Plantilla);
            //Clipboard.SetText(Plantilla);
            FileInfo fi = new FileInfo("ASM/lya.asm");
            if (File.Exists(fi.FullName))
                File.Delete(fi.FullName);
            StreamWriter sw = new StreamWriter(fi.FullName);
            sw.Write(Plantilla);
            sw.Close();

            Process.Start(fi.FullName);
        }

        private string BuscarEnTablaS(string token)
        {
            int cont = 0;
            foreach (DataGridView d in dtg)
            {
                cont++;
                //buscar en cada fila
                foreach (DataGridViewRow d2 in d.Rows)
                {
                    if (d2.Cells[0].Value.ToString().Trim() == token.Trim())
                    {
                        if (cont == 4)
                            return d2.Cells[1].Value.ToString().Trim()+",'$'";
                        return d2.Cells[1].Value.ToString().Trim();
                    }
                }
            }
            return "";
        }


        //Ensamblador
        private void BuscarVariables()
        {
            //Buscar Asignaciones//
            string dataseg = "";
            string codigo = txtTokens.Text;
            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            /*buscar ids en la tabla*/
            foreach (DataGridViewRow x in dtgTripletas.Rows)
            {
                bool bandera = false;
                //verificar que no se haya asignado
                if (x.Cells[0].Value.ToString().Contains("ID") && x.Cells[2].Value.ToString().Trim() == "ASIG")
                {
                    //Buscar en el código
                    foreach (string asig in lineas)
                    {
                        if (!bandera)
                        {
                            if (asig.Contains("ASIG") && asig != "ASIG")
                            {
                                string[] aux = asig.Split(new string[] { "ASIG" }, StringSplitOptions.RemoveEmptyEntries);
                                if (aux[0].ToString().Trim() == x.Cells[0].Value.ToString().Trim())
                                {
                                    //buscar en cada dtg
                                    int cont = 0;
                                    foreach (DataGridView d in dtg)
                                    {
                                        if (!bandera)
                                        {
                                            cont++;
                                            //buscar en cada fila
                                            foreach (DataGridViewRow d2 in d.Rows)
                                            {
                                                if (!bandera)
                                                {
                                                    if (d2.Cells[0].Value.ToString().Trim() == aux[1].Trim())
                                                    {
                                                        if (cont == 4)
                                                            dataseg += aux[0] + " db " + d2.Cells[1].Value.ToString().Trim() + ",'$'\r\n";
                                                        else
                                                            dataseg += aux[0] + " db " + d2.Cells[1].Value.ToString().Trim() + "\r\n";
                                                        bandera = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
