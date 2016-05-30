using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;

namespace AnalizadorLexico
{
    public partial class Analizador : Form
    {
        List<object[]> ListaTokens = new List<object[]>();
        public Analizador()
        {
            InitializeComponent();
        }

        int Estado = 0;
        int Errores = 0;
        string UltimoID = "";
        //async
        string CodigoFuente = "";
        private async void mnuAbrir_Click(object sender, EventArgs e)
        {
            int delay = 0;

            dtgId.Rows.Clear();
            dtgEc.Rows.Clear();
            dtgCr.Rows.Clear();
            dtgCd.Rows.Clear();
            dtgXe.Rows.Clear();
            dtgXr.Rows.Clear();
            txtSalida.Clear();
            txtSintaxis.Clear();

            int[] Cont = new int[] { 0, 0, 0, 0, 0, 0 };//ID, CE, CR, XE, XR, CD
            List<string> lstId = new List<string>();
            List<string> lstCe = new List<string>();
            List<string> lstCr = new List<string>();
            List<string> lstXe = new List<string>();
            List<string> lstXr = new List<string>();
            List<string> lstCd = new List<string>();
            Conexion.CambiarALexico();
            ListaTokens = new List<object[]>();
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Archivo de texto|*.txt|Archivo Rich|*.br";
            if (file.ShowDialog() != DialogResult.OK)
                return;
            StreamReader sr = new StreamReader(file.FileName);
            string codigo = sr.ReadToEnd();
            CodigoFuente = codigo;
            sr.Close();
            string[] separadores = new string[] { "\r\n" };
            string[] Lineas = codigo.Split(separadores, StringSplitOptions.RemoveEmptyEntries);
            txtNumLineas.Text = Lineas.Count().ToString();
            for (int i = 0; i < Lineas.Count(); i++)
            {
                if (Lineas[i][Lineas[i].Length - 1] != ';')
                    Lineas[i] += ';';
            }
            mnuGuardar.Enabled = false;
            mnuAbrir.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            foreach (Control x in this.Controls)
            {
                if (x is TextBox)
                    x.Cursor = Cursors.WaitCursor;
            }

            int ContadorLinea = 1;
            Errores = 0;
            txtError.Text = "0";
            txtSalida.Clear();
            foreach (string linea in Lineas)
            {
                if (txtSalida.Text != "")
                {
                    txtSalida.Text += "\r\n";
                }
                txtNumLinea.Text = ContadorLinea.ToString();
                txtLinea.Text = linea;
                txtParte.Clear();
                txtToken.Clear();
                Estado = 0;
                for (int i = 0; i < linea.Length; i++)
                {
                    try
                    {
                        if(delay > 0)
                            await Task.Delay(delay);    
                        DataTable dt;
                        if(linea[i] == '.')
                        {
                            dt = Conexion.query("Select `PUNTO` From [Matriz$] Where Estado = '" + Estado + "'");
                        }
                        else if (linea[i] == '^')
                        {
                            dt = Conexion.query("Select `EXP` From [Matriz$] Where Estado = '" + Estado + "'");
                        }
                        else if (char.IsUpper(linea[i]))
                        {
                            dt = Conexion.query("Select `" + linea[i] + "1` From [Matriz$] Where Estado = '" + Estado + "'");
                        }
                        else
                        {
                            dt = Conexion.query("Select `" + linea[i] + "` From [Matriz$] Where Estado = '" + Estado + "'");
                        }
                        if (dt.Rows.Count > 0)
                        {
                            string celda = dt.Rows[0][0].ToString();
                            if (celda != "" && celda != null)
                            {
                                Estado = int.Parse(celda);
                                txtParte.Text = txtParte.Text + linea[i];
                                txtParte.Text = txtParte.Text.Trim();
                                dt = Conexion.query("Select `TOKEN` From [Matriz$] Where Estado = '" + Estado + "'");
                                string Token = dt.Rows[0][0].ToString();
                                if (Token != "" && Token != null)
                                {
                                    Estado = 0;
                                    txtToken.Text = Token;
                                    await Task.Delay(delay + 1);

                                    //ID, CE, CR, XE, CD
                                    Token = Token.Trim();
                                    txtParte.Text = txtParte.Text.Replace(";", "").Replace(" ", "");
                                    if (Token == "ID00")
                                    {
                                        if (!lstId.Contains(txtParte.Text.Replace(";","").Replace(" ", "")))
                                        {
                                            Token = "ID" + (++Cont[0]).ToString("00");
                                            dtgId.Rows.Add(Token, txtParte.Text.Trim(), "object", "");
                                            lstId.Add(txtParte.Text.Replace(" ", ""));
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgId.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text.Trim())
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }
                                    else if (Token == "EC00")
                                    {
                                        if (!lstCe.Contains(txtParte.Text.Replace(";", "").Replace(" ", "")))
                                        {
                                            Token = "EC" + (++Cont[1]).ToString("00");
                                            dtgEc.Rows.Add(Token, txtParte.Text.Replace(";", "").Replace(" ", ""), "int", "");
                                            lstCe.Add(txtParte.Text.Replace(";", "").Replace(" ", ""));
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgEc.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text.Trim())
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }
                                    else if (Token == "CR00")
                                    {
                                        if (!lstCr.Contains(txtParte.Text.Replace(";", "").Replace(" ", "")))
                                        {
                                            Token = "CR" + (++Cont[2]).ToString("00");
                                            dtgCr.Rows.Add(Token, txtParte.Text.Replace(";", "").Replace(" ", ""), "double", "");
                                            lstCr.Add(txtParte.Text.Replace(";", "").Replace(" ", ""));
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgCr.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text)
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }
                                    else if (Token == "XE00")
                                    {
                                        if (!lstXe.Contains(txtParte.Text.Replace(";", "").Replace(" ", "")))
                                        {
                                            Token = "XE" + (++Cont[3]).ToString("00");
                                            dtgXe.Rows.Add(Token, txtParte.Text.Trim(), "int", "");
                                            lstXe.Add(txtParte.Text);
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgXe.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text)
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }
                                    else if (Token == "XR00")
                                    {
                                        if (!lstXe.Contains(txtParte.Text.Replace(";", "").Replace(" ", "")))
                                        {
                                            Token = "XR" + (++Cont[4]).ToString("00");
                                            dtgXr.Rows.Add(Token, txtParte.Text.Trim(), "int", "");
                                            lstXr.Add(txtParte.Text);
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgXr.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text)
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }
                                    else if (Token == "CD00")
                                    {
                                        if (!lstCd.Contains(txtParte.Text.Replace(";", "").Replace(" ", "")))
                                        {
                                            Token = "CD" + (++Cont[5]).ToString("00");
                                            dtgCd.Rows.Add(Token, txtParte.Text.Trim(), "string", "");
                                            lstCd.Add(txtParte.Text);
                                        }
                                        else
                                        {
                                            foreach (DataGridViewRow x in dtgCd.Rows)
                                            {
                                                if (x.Cells[1].Value.ToString() == txtParte.Text)
                                                    Token = x.Cells[0].Value.ToString();
                                            }
                                        }
                                    }

                                    txtToken.Clear();
                                    txtSalida.Text += Token + " ";
                                    ListaTokens.Add(new string[] { (ListaTokens.Count+1).ToString(), Token, null, null });
                                    txtParte.Clear();
                                }
                            }
                            else
                            {
                                Errores++;
                                txtError.Text = Errores.ToString();
                                txtParte.Clear();
                                Estado = 0;
                                lblError.Text = "ERROR";
                                await Task.Delay(500);
                                lblError.Text = "";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Errores++;
                        txtError.Text = Errores.ToString();
                        txtParte.Clear();
                        Estado = 0;
                        lblError.Text = "ERROR";
                        await Task.Delay(500);
                        lblError.Text = "";
                    }
                }
                ContadorLinea++;
            }
            MessageBox.Show("Conversión exitosa.");
            txtLinea.Clear();
            mnuGuardar.Enabled = true;
            mnuAbrir.Enabled = true;
            this.Cursor = Cursors.Default;
            foreach (Control x in this.Controls)
            {
                if (x is TextBox)
                    x.Cursor = Cursors.IBeam;
            }

        }
        
        private void mnuGuardar_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "Archivo de texto|*.txt|Archivo Rich|*.br";
            file.FileName = "Codigo de Salida" + DateTime.Now.ToString("dd MMMM yyyy hh mm ss");
            if (file.ShowDialog() != DialogResult.OK)
            {
                file.Dispose();
                return;
            }
            if (File.Exists(file.FileName))
                File.Delete(file.FileName);

            StreamWriter sw = new StreamWriter(file.FileName);
            file.Dispose();
            sw.Write(txtSalida.Text);
            sw.Close();
            sw.Dispose();
        }

        private void mnuSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Esta seguro de querer salir?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                this.Close();
        }

        private void mnuTablaDeTokens_Click(object sender, EventArgs e)
        {
            Tokens tokens = new Tokens(ListaTokens);
            tokens.ShowDialog();
        }

        private async void mnuAnalizarSintaxis_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                txtSintaxis.Text = txtSalida.Text;
                IndentificarEnumerados();
                txtSintaxis.Text = LimpiarEnumerados(txtSintaxis.Text);
                txtSintaxis.Text = LimpiarOPAR(txtSintaxis.Text);

                #region Sintaxis
                //Sintaxis
                Conexion.CambiarASintaxis();
                string codigo = txtSintaxis.Text;//txtSintaxis.Text;
                string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lineas.Count(); i++)
                {
                    #region BASURA
                    //TRAP
                    if (lineas[i].Contains("PR10") && lineas[i].Contains("CE11"))
                    {
                        lineas[i] = "S";
                    }
                    if (lineas[i].Contains("PR06") && lineas[i].Contains("CE01") && lineas[i].Contains("CE02"))
                    {
                        lineas[i] = "S";
                    }
                    if (lineas[i].Contains("PR08") && lineas[i].Contains("CE01") && lineas[i].Contains("CE02") && lineas[i].Contains("CE11"))
                    {
                        lineas[i] = "S";
                    }
                    //
                    #endregion
                    if (lineas[i] != "S")
                    {
                        await Task.Delay(0);
                        lineas[i] = Sintaxis(lineas[i]);
                    }
                }
                bool dif = false;
                txtSintaxis.Clear();
                //COMPROBACION
                foreach (string v in lineas)
                {
                    await Task.Delay(0);
                    txtSintaxis.Text += v + "\r\n";
                    if (v != "S")
                    {
                        dif = true;
                    }
                }
                if (dif)
                    MessageBox.Show("No se acepta la sintaxis.");
                else
                    MessageBox.Show("Se acepta la sintaxis.");
                #endregion


                //COMPROBACION
                txtSintaxis.Text = "";
                foreach (string v in lineas)
                {
                    await Task.Delay(0);
                    txtSintaxis.Text += v + "\r\n";
                    if (v != "S")
                    {
                        dif = true;
                    }
                }
                /**/


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #region Basura
        private string LimpiarEnumerados(string codigo)
        {
            codigo = codigo.Replace("ID00", "IDEN").Replace("EC00", "CNEN").Replace("CR00", "CNRE");
            codigo = codigo.Replace("XE00", "CNEE").Replace("XR00", "CNER").Replace("CD00", "CADE");
            return codigo;
        }

        private string LimpiarOPAR(string codigo)
        {
            codigo = codigo.Replace("OPA1", "OPEA").Replace("OPA2", "OPEA");
            codigo = codigo.Replace("OPA3", "OPEA").Replace("OPA4", "OPEA");
            return codigo;
        }
        #endregion

        private string[] RealizarAsignacion(string linea)
        {
            //MessageBox.Show(linea);
            if(linea.Contains("ASIG") && linea.Length > 8)
                linea = string.Format("{0}CE01{1}CE02", linea.Substring(0, 8), linea.Substring(8));
            int NT = linea.Length / 4;
            int N = NT;
            string cade = "";
            DataTable dt;
            bool bandera = false;
            string[] array = null;
            while (N >= 1)
            {
                int desp = NT - N;
                for (int j = desp; j >= 0; j--)
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
                            array = new string[] { cade.Substring(0, 4), cade.Substring(8,4)};
                            return array;
                        }
                        else
                        {
                            if (linea.Length / 4 == 3)
                            {
                                string id = linea.Substring(0, 4);
                                string Tipo = linea.Substring(8);
                                bool ban = false;
                                foreach (DataGridViewRow x in dtgId.Rows)
                                {
                                    if (x.Cells[0].Value.ToString() == id)
                                    {
                                        x.Cells[0].Value = Tipo;
                                        ban = true;
                                    }
                                }
                                if (ban)
                                {
                                    linea = "S";
                                    array = new string[] { linea };
                                    return array;
                                }
                            }
                            N = linea.Length / 4;
                            NT = N;
                            j = 0;
                        }
                        bandera = true;
                    }

                }
                if (!bandera)
                    N--;

                //MessageBox.Show("N = " + N);
            }
            array = new string[] { linea };
            return array;
        }

        private string Sintaxis(string linea)
        {
            int NT = linea.Length / 4;
            int N = NT;
            string cade = "";
            DataTable dt;
            bool bandera = false;
            while (N >= 1)
            {
                int desp = NT - N;
                for (int j = desp; j >= 0 ; j--)
                {
                    bandera = false;
                    cade = linea.Substring((j * 4), (N * 4));
                    //MessageBox.Show(cade);
                    dt = Conexion.query("Select `LADO IZQUIERDO` From [SHEET$] Where `LADO DERECHO` = '" + cade + "'");
                    if (dt.Rows.Count > 0)
                    {
                        string remp = dt.Rows[0][0].ToString();
                        //MessageBox.Show(remp);
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
                            j = 0;
                        }
                        bandera = true;
                    }

                }
                if (!bandera)
                    N--;
            }
            return linea;
        }

        private void IndentificarEnumerados()
        {
            //ID, CE, CR, XE, XR, CD
            foreach (DataGridViewRow x in dtgId.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "ID00");
            }
            foreach (DataGridViewRow x in dtgEc.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "EC00");
            }
            foreach (DataGridViewRow x in dtgCr.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "CR00");
            }
            foreach (DataGridViewRow x in dtgXe.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "XE00");
            }
            foreach (DataGridViewRow x in dtgXr.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "XR00");
            }
            foreach (DataGridViewRow x in dtgCd.Rows)
            {
                txtSintaxis.Text = txtSintaxis.Text.Replace(x.Cells[0].Value.ToString(), "CD00");
            }
            txtSintaxis.Text = txtSintaxis.Text.Replace(" ", "");
        }

        private void analizarSemanticaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //Asignacion
                Asignación();
                //que el for no tenga reales ni cadenas
                string Codigo = txtSalida.Text;
                string[] lineas = Codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string linea in lineas)
                {
                    if (linea.Contains("PR05"))
                    {
                        if (linea.Contains("CADE"))
                        {

                            throw new Exception("Los ciclos for no pueden contener cadenas");
                        }
                        if (linea.Contains("CNRE"))
                        {
                            throw new Exception("Los ciclos for no pueden contener cadenas");
                        }
                    }
                }
                //Que lO que inicie termine usando contadores
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void postfijoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string codigo = txtSalida.Text;
            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string aux = "";
            foreach (string linea in lineas)
            {
                //if ((linea.Contains("ASIG") && linea != "ASIG") || linea.Contains("PR08") || linea.Contains("PR10") || linea.Contains("PR11") || linea.Contains("CE12"))
                    aux += linea + "\r\n";
            }
            DataGridView[] dtgs = new DataGridView[] { dtgId, dtgEc, dtgCr, dtgCd};
            frmPostfijo newForm = new frmPostfijo(aux, dtgs);
            newForm.Show();
        }

        private void mnuAsignacion_Click(object sender, EventArgs e)
        {
            
        }

        private void Asignación()
        {
            string codigo = CodigoFuente;
            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string linea in lineas)
            {
                if (linea.Contains("=") && linea != "=")
                {
                    string[] Datos = linea.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                    Datos[1] = LeerTiposID(Datos[1]);
                    //Es Real
                    if (Datos[1].Contains(".") || Datos[1].Contains("CNRE"))
                    {
                        EstablecerTipo(Datos[0], "CNRE");
                    }
                    //Es Cadena
                    else if (Datos[1].Contains("\"") || Datos[1].Contains("CADE"))
                    {
                        EstablecerTipo(Datos[0], "CADE");
                    }
                    //Es Entero
                    else// (Datos[1].Contains("."))
                    {
                        EstablecerTipo(Datos[0], "CNEE");
                    }
                }
            }
        }

        private string LeerTiposID(string opar)
        {
            //Separar cada parte
            string[] x = opar.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < x.Count(); i++)
            {
                foreach (DataGridViewRow r in dtgId.Rows)
                {
                    if(x[i].Replace(";","") == r.Cells[1].Value.ToString().Trim())
                    {
                        //MessageBox.Show(x[i] + "\n" + r.Cells[2].Value.ToString());
                        x[i] = r.Cells[2].Value.ToString();
                    }
                }
            }
            //Juntar todas las partes
            string op = "";
            foreach (string s in x)
            {
                op += s + " ";
            }
            return op.Substring(0, op.Length - 1);
        }

        private void EstablecerTipo(string id, string Tipo)
        {
            //MessageBox.Show(id + " = " + Tipo);
            for (int i = 0; i < dtgId.Rows.Count; i++)
            {
                if (dtgId.Rows[i].Cells[1].Value.ToString().Trim() == id.Trim())
                    dtgId.Rows[i].Cells[2].Value = Tipo;
            }
        }

        private void mnuTripletas_Click(object sender, EventArgs e)
        {

        }

        private void mnuOptimizacion_Click(object sender, EventArgs e)
        {
            string codigo = txtSalida.Text;
            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //instrucciones dependientes susceptibles a reorganización
            txtSalida.Clear();
            for (int j = 0; j < lineas.Count(); j++)
            {
                txtSalida.Text += lineas[j] + "\r\n";
                if (lineas[j].Contains("ASIG") && lineas[j] != "ASIG")
                {
                    string[] aux = lineas[j].Split(new string[] { "ASIG " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = j+1; i < lineas.Count(); i++)
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
            codigo = txtSalida.Text;
            lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            txtSalida.Clear();
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
                        for (int i = j+1; i < lineas.Count(); i++)
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
                    
                    if(!ban)
                        txtSalida.Text += lineas[j] + "\r\n";
                }
                else
                    txtSalida.Text += lineas[j] + "\r\n";
            }
            //instrucciones que generan un resultado que no se utiliza en todo el desarrollo del programa
            codigo = txtSalida.Text;
            lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            txtSalida.Clear();
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
                        txtSalida.Text += lineas[j] + "\r\n";
                }
                else
                    txtSalida.Text += lineas[j] + "\r\n";

                
            }
        }

        private void mnuOptBucles_Click(object sender, EventArgs e)
        {
            int contB = 0;
            string codigo = txtSalida.Text;
            string[] lineas = codigo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            txtSalida.Clear();
            for (int j = 0; j < lineas.Count(); j++)
            {
                if (lineas[j].Contains("PR08"))
                {
                    if (lineas[j].Contains("OPA"))
                    {
                        contB++;
                        int idN = dtgId.Rows.Count + 1;
                        int start = lineas[j].IndexOf("OPR") + 5;
                        int end = lineas[j].IndexOf("CE02") - 1;

                        string expresion = lineas[j].Substring(start, end - start);
                        string asignacion = "ID" + idN.ToString("00");

                        txtSalida.Text += asignacion + " ASIG " + expresion + "\r\n";
                        lineas[j] = lineas[j].Replace(expresion, asignacion);

                        dtgId.Rows.Add(asignacion, "bucle" + contB, "object");
                        #region ...
                        if (expresion.Contains("CR"))
                        {
                            EstablecerTipo("bucle" + contB, "CNRE");
                        }
                        else if (expresion.Contains("CD"))
                        {
                            EstablecerTipo("bucle" + contB, "CADE");
                        }
                        else
                        {
                            EstablecerTipo("bucle" + contB, "CNEE");
                        }
                        #endregion
                    }
                }
                txtSalida.Text += lineas[j] + "\r\n";
            }
        }
    }
}
