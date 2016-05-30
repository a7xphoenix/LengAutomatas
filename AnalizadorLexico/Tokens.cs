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
    public partial class Tokens : Form
    {
        public Tokens(List<object[]> ListaTokens)
        {
            InitializeComponent();
            foreach (object[] x in ListaTokens)
            {
                dtgTokens.Rows.Add(x);
            }
        }
    }
}
