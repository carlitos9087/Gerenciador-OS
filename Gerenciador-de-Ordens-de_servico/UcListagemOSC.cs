using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class UcListagemOSC : UserControl
    {
        // Classe modelo para os dados
        public class OrdemServico
        {
            public string Numero { get; set; }
            public string Titulo { get; set; }
            public string Setor { get; set; }
            public string Status { get; set; }
            public string Prazo { get; set; }
            public string Emitente { get; set; }
            public Color CorStatus { get; set; }
        }

        private List<OrdemServico> listaOSC = new List<OrdemServico>();
        private int paginaAtual = 1;
        private int itensPorPagina = 5;

        public UcListagemOSC()
        {
            InitializeComponent();
            ConfigurarTela();
            CarregarDadosFake(); // Depois você troca por banco de dados
            MostrarPagina();
        }

        private void ConfigurarTela()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 249, 250); // Cinza claro fundo

            // ===== PAINEL SUPERIOR (Cabeçalho) =====
            Panel painelTopo = new Panel();
            painelTopo.Dock = DockStyle.Top;
            painelTopo.Height = 80;
            painelTopo.BackColor = Color.FromArgb(15, 23, 42); // Azul escuro
            this.Controls.Add(painelTopo);

            // Título pequeno
            Label lblPequeno = new Label();
            lblPequeno.Text = "OSCs RECENTES";
            lblPequeno.ForeColor = Color.Gray;
            lblPequeno.Font = new Font("Segoe UI", 9);
            lblPequeno.Location = new Point(20, 10);
            lblPequeno.AutoSize = true;
            painelTopo.Controls.Add(lblPequeno);

            // Título grande
            Label lblTitulo = new Label();
            lblTitulo.Text = "Listagem e Acompanhamento";
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.Location = new Point(20, 30);
            lblTitulo.AutoSize = true;
            painelTopo.Controls.Add(lblTitulo);

            // Combo status (direita)
            ComboBox cmbStatus = new ComboBox();
            cmbStatus.Text = "Todos os status";
            cmbStatus.Location = new Point(500, 25);
            cmbStatus.Size = new Size(150, 25);
            painelTopo.Controls.Add(cmbStatus);

            // Botão Filtrar
            Button btnFiltrar = new Button();
            btnFiltrar.Text = "Filtrar";
            btnFiltrar.BackColor = Color.FromArgb(0, 122, 204);
            btnFiltrar.ForeColor = Color.White;
            btnFiltrar.FlatStyle = FlatStyle.Flat;
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Location = new Point(660, 25);
            btnFiltrar.Size = new Size(80, 28);
            painelTopo.Controls.Add(btnFiltrar);

            // ===== PAINEL DA LISTA (DataGridView) =====
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvOSC";
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(233, 236, 239);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.RowTemplate.Height = 50;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.LightGray;

            // Adiciona colunas
            dgv.Columns.Add("Numero", "Nº OSC");
            dgv.Columns.Add("Titulo", "TÍTULO / SETOR");
            dgv.Columns.Add("Status", "STATUS");
            dgv.Columns.Add("Prazo", "PRAZO");
            dgv.Columns.Add("Emitente", "EMITENTE");
            dgv.Columns.Add("Acao", ""); // Coluna do botão "Ver →"

            // Larguras proporcionais
            dgv.Columns["Numero"].FillWeight = 10;
            dgv.Columns["Titulo"].FillWeight = 35;
            dgv.Columns["Status"].FillWeight = 20;
            dgv.Columns["Prazo"].FillWeight = 15;
            dgv.Columns["Emitente"].FillWeight = 15;
            dgv.Columns["Acao"].FillWeight = 5;

            this.Controls.Add(dgv);

            // ===== PAINEL RODAPÉ (Paginação) =====
            Panel painelRodape = new Panel();
            painelRodape.Dock = DockStyle.Bottom;
            painelRodape.Height = 50;
            painelRodape.BackColor = Color.White;
            this.Controls.Add(painelRodape);

            // Label "Exibindo X de Y"
            Label lblInfo = new Label();
            lblInfo.Name = "lblInfo";
            lblInfo.Text = "Exibindo 5 de 24 OSCs";
            lblInfo.Location = new Point(20, 15);
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = Color.Gray;
            painelRodape.Controls.Add(lblInfo);

            // Botões de paginação (direita)
            Button btnAnt = new Button();
            btnAnt.Text = "← Ant";
            btnAnt.Location = new Point(450, 10);
            btnAnt.Size = new Size(60, 30);
            btnAnt.FlatStyle = FlatStyle.Flat;
            painelRodape.Controls.Add(btnAnt);

            // Botões número (1, 2, 3...)
            for (int i = 1; i <= 3; i++)
            {
                Button btnNum = new Button();
                btnNum.Text = i.ToString();
                btnNum.Location = new Point(510 + (i - 1) * 35, 10);
                btnNum.Size = new Size(30, 30);
                btnNum.FlatStyle = FlatStyle.Flat;
                if (i == 1) btnNum.BackColor = Color.FromArgb(15, 23, 42);
                if (i == 1) btnNum.ForeColor = Color.White;
                painelRodape.Controls.Add(btnNum);
            }

            Button btnProx = new Button();
            btnProx.Text = "Próx →";
            btnProx.Location = new Point(620, 10);
            btnProx.Size = new Size(60, 30);
            btnProx.FlatStyle = FlatStyle.Flat;
            painelRodape.Controls.Add(btnProx);
        }

        private void CarregarDadosFake()
        {
            // Dados de exemplo (depois você busca do banco!)
            listaOSC.Add(new OrdemServico
            {
                Numero = "012/26",
                Titulo = "Falha sistema de enchimento L3",
                Setor = "Produção • Linha 3",
                Status = "● EM EXECUÇÃO",
                CorStatus = Color.FromArgb(0, 122, 204),
                Prazo = "⚠ 3 dias\n06/03/2026",
                Emitente = "J. Silva"
            });

            listaOSC.Add(new OrdemServico
            {
                Numero = "011/26",
                Titulo = "Desvio temperatura câmara fria",
                Setor = "Qualidade • Armazenagem",
                Status = "● EM REVISÃO",
                CorStatus = Color.Green,
                Prazo = "12 dias\n15/03/2026",
                Emitente = "M. Costa"
            });

            listaOSC.Add(new OrdemServico
            {
                Numero = "010/26",
                Titulo = "Manutenção compressor central",
                Setor = "Engenharia • Utilidades",
                Status = "● AGUARD. APROVAÇÃO",
                CorStatus = Color.Orange,
                Prazo = "— sem prazo —",
                Emitente = "R. Mendes"
            });

            listaOSC.Add(new OrdemServico
            {
                Numero = "009/26",
                Titulo = "Calibração balança linha 1",
                Setor = "Laboratório • Metrologia",
                Status = "● APROVA. FINAL",
                CorStatus = Color.Purple,
                Prazo = "18 dias\n21/03/2026",
                Emitente = "A. Ferreira"
            });

            listaOSC.Add(new OrdemServico
            {
                Numero = "008/26",
                Titulo = "Vazamento válvula tanque 04",
                Setor = "Manutenção • Tanques",
                Status = "● FECHADA",
                CorStatus = Color.Gray,
                Prazo = "✓ Encerrada",
                Emitente = "P. Lima"
            });
        }

        private void MostrarPagina()
        {
            DataGridView dgv = this.Controls["dgvOSC"] as DataGridView;
            dgv.Rows.Clear();

            foreach (var osc in listaOSC)
            {
                int rowIndex = dgv.Rows.Add(
                    osc.Numero,
                    osc.Titulo + "\n" + osc.Setor,
                    osc.Status,
                    osc.Prazo,
                    osc.Emitente,
                    "Ver →"
                );

                // Cor do status
                dgv.Rows[rowIndex].Cells["Status"].Style.ForeColor = osc.CorStatus;
                dgv.Rows[rowIndex].Cells["Status"].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
        }
    }
}