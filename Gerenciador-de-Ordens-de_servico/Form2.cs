using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form2 : Form
    {
        public Form2()
        {

            InitializeComponent();

            // No construtor do Form2, depois das configurações das abas:
            tabControl1.TabPages[0].Text = "Listagem";
            tabControl1.TabPages[0].Controls.Add(new UcListagemOSC());

        }

       

    }
}
