using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form3 : Form
    {
        // ══════════════════════════════════════════════
        // CORES DO TEMA
        // ══════════════════════════════════════════════
        readonly Color corPrimaria    = Color.FromArgb(30, 80, 160);
        readonly Color corBotaoAtivo  = Color.FromArgb(30, 80, 160);
        readonly Color corTextoAtivo  = Color.White;
        readonly Color corTextoNormal = Color.FromArgb(60, 60, 80);

#pragma warning disable CS0414
        Button? btnAtivo;
#pragma warning restore CS0414

        // Usa o HttpClient compartilhado com token JWT já configurado
        // O token foi salvo em ApiConfig.SalvarToken() durante o login no Form1
        private const string URL_BASE = "https://localhost:7188";

        // Usuário logado — recebido do Form1 após login
        private readonly UsuarioResponse _usuarioLogado;

        public Form3(UsuarioResponse usuarioLogado)
        {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;
            // Token já está configurado em ApiConfig.Http via ApiConfig.SalvarToken() no Form1
        }

        // ══════════════════════════════════════════════
        // LOAD
        // ══════════════════════════════════════════════
        private void Form3_Load(object sender, EventArgs e)
        {
            ConfigurarCabecalho();
            ConfigurarBotoesPorPerfil();
            _ = MostrarDashboard();
            DestaqueBotao(btnDashboard);
        }

        // ══════════════════════════════════════════════
        // CABEÇALHO
        // ══════════════════════════════════════════════
        private void ConfigurarCabecalho()
        {
            panelCabecalho.BackColor = corPrimaria;

            panelCabecalho.Controls.Add(new Label
            {
                Text      = "📋  Gerenciador de Ordens de Serviço",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 15, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(16, 16)
            });

            panelCabecalho.Controls.Add(new Label
            {
                Text      = $"👤  {_usuarioLogado.Nome}  |  {_usuarioLogado.Perfil}",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font      = new Font("Segoe UI", 10),
                AutoSize  = true,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                Location  = new Point(panelCabecalho.Width - 380, 20)
            });

            var btnSair = new Button
            {
                Text           = "⬅  Sair",
                Width          = 90,
                Height         = 32,
                FlatStyle      = FlatStyle.Flat,
                Font           = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor      = Color.FromArgb(200, 30, 30),
                ForeColor      = Color.White,
                Cursor         = Cursors.Hand,
                Anchor         = AnchorStyles.Top | AnchorStyles.Right,
                Location       = new Point(panelCabecalho.Width - 110, 14),
                FlatAppearance = { BorderSize = 0 }
            };
            btnSair.Click += (s, e) =>
            {
                var confirmar = MessageBox.Show("Deseja sair da conta?", "Confirmar saída",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmar == DialogResult.Yes)
                {
                    foreach (Form f in Application.OpenForms)
                        if (f is Form1) { f.Show(); break; }
                    this.Close();
                }
            };
            panelCabecalho.Controls.Add(btnSair);
        }

        // ══════════════════════════════════════════════════════════════
        // BOTÕES LATERAIS — visibilidade controlada por perfil
        //
        // Emitente      → Dashboard + Criar OS
        // Gerente       → Dashboard + Assinar OS
        // Administrador → Dashboard + Criar OS + Gerenciar Usuários + Gerenciar OSCs
        // Executante    → Dashboard (somente visualização)
        // ══════════════════════════════════════════════════════════════
        // Cor de botão bloqueado — vermelho escuro acinzentado
        // Indica claramente "você não tem acesso a esta função"
        readonly Color corBotaoInativo      = Color.FromArgb(80, 40, 40);
        readonly Color corTextoBotaoInativo = Color.FromArgb(180, 130, 130);

        private void ConfigurarBotoesPorPerfil()
        {
            string perfil = _usuarioLogado.Perfil ?? "";

            bool ehEmitente  = perfil.Equals("Emitente",      StringComparison.OrdinalIgnoreCase);
            bool ehGerente   = perfil.Equals("Gerente",       StringComparison.OrdinalIgnoreCase);
            bool ehAdmin     = perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                            || perfil.Equals("Admin",         StringComparison.OrdinalIgnoreCase);

            // Todos os botões sempre visíveis — inativos ficam acinzentados e sem clique
            EstilizarBotao(btnDashboard,      "📈  Dashboard");
            EstilizarBotao(btnCriarRelatorio, "➕  Criar OS");
            EstilizarBotao(btnAssinar,        "✅  Assinar OS");
            EstilizarBotao(btnAdmin,          "👥  Gerenciar Usuários");
            EstilizarBotao(btnGerenciarOscs,  "🛠️  Gerenciar OSs");

            // Dashboard — sempre ativo para todos
            btnDashboard.Click += async (s, e) => { await MostrarDashboard(); DestaqueBotao(btnDashboard); };

            // Criar OS: ativo para Emitente e Admin
            if (ehEmitente || ehAdmin)
                btnCriarRelatorio.Click += (s, e) => { MostrarCriarOS(); DestaqueBotao(btnCriarRelatorio); };
            else
                DesabilitarBotao(btnCriarRelatorio);

            // Assinar OS: ativo só para Gerente
            if (ehGerente)
                btnAssinar.Click += async (s, e) => { await MostrarAssinar(); DestaqueBotao(btnAssinar); };
            else
                DesabilitarBotao(btnAssinar);

            // Gerenciar Usuários: ativo só para Admin
            if (ehAdmin)
                btnAdmin.Click += async (s, e) => { await MostrarAdmin(); DestaqueBotao(btnAdmin); };
            else
                DesabilitarBotao(btnAdmin);

            // Gerenciar OSCs: ativo só para Admin
            if (ehAdmin)
                btnGerenciarOscs.Click += async (s, e) => { await MostrarGerenciarOscs(); DestaqueBotao(btnGerenciarOscs); };
            else
                DesabilitarBotao(btnGerenciarOscs);
        }

        // Aplica visual bloqueado e cursor de "não permitido" — botão visível mas sem função
        private void DesabilitarBotao(Button btn)
        {
            btn.BackColor = corBotaoInativo;
            btn.ForeColor = corTextoBotaoInativo;

            // Cursors.No = cursor de "proibido" (círculo com barra vermelha)
            // Aparece quando o mouse passa por cima do botão
            btn.Cursor = Cursors.No;

            // Tooltip explicando o motivo ao passar o mouse
            var tip = new ToolTip
            {
                InitialDelay = 200,   // aparece rápido
                ShowAlways   = true
            };
            tip.SetToolTip(btn, "⛔  Seu perfil não tem acesso a esta função.");

            // Garante que cliques não façam nada
            btn.Click += (s, e) => { /* bloqueado por perfil */ };
        }

        private void EstilizarBotao(Button btn, string texto)
        {
            btn.Text      = texto;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font      = new Font("Segoe UI", 10);
            btn.ForeColor = corTextoNormal;
            btn.BackColor = Color.Transparent;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding   = new Padding(10, 0, 0, 0);
            btn.Cursor    = Cursors.Hand;
        }

        private void DestaqueBotao(Button botaoClicado)
        {
            Button[] todos = { btnDashboard, btnCriarRelatorio, btnAssinar, btnAdmin, btnGerenciarOscs };
            foreach (var btn in todos)
            {
                btn.BackColor = (btn == botaoClicado) ? corBotaoAtivo : Color.Transparent;
                btn.ForeColor = (btn == botaoClicado) ? corTextoAtivo : corTextoNormal;
            }
            btnAtivo = botaoClicado;
        }

        // ══════════════════════════════════════════════
        // UTILITÁRIO
        // ══════════════════════════════════════════════

        // Painel interno com margem — todos os controles vão aqui
        // Isso garante espaçamento consistente em todas as telas
        Panel _painelInterno = null!;

        private void LimparConteudo()
        {
            foreach (Control c in panelConteudo.Controls)
                c.Dispose();
            panelConteudo.Controls.Clear();

            // Recria o painel interno com margem de 28px em todos os lados
            _painelInterno = new Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(28, 20, 28, 20),
                BackColor = Color.Transparent
            };
            panelConteudo.Controls.Add(_painelInterno);
        }

        // Atalho: adiciona ao painel interno (com margem) em vez de direto no panelConteudo
        private void Add(Control c) => _painelInterno.Controls.Add(c);

        // Largura disponível dentro do painel interno (descontando o padding)
        private int LarguraUtil => _painelInterno.Width - _painelInterno.Padding.Horizontal;

        // Cria Label + TextBox dentro do panelConteudo
        private void AdicionarCampo(string rotulo, ref int y,
                                    out TextBox caixaTexto,
                                    int altura = 32, bool multiline = false)
        {
            Add(new Label
            {
                Text      = rotulo,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 100),
                AutoSize  = true,
                Location  = new Point(0, y)
            });
            y += 22;

            caixaTexto = new TextBox
            {
                Width       = 420,
                Height      = altura,
                Location    = new Point(0, y),
                Font        = new Font("Segoe UI", 10),
                BackColor   = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline   = multiline,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Add(caixaTexto);
            y += altura + 18;
        }

        // Cria Label + TextBox dentro de um Form de diálogo
        private TextBox AdicionarCampoDialog(Form dialog, string rotulo, ref int y, bool senha = false)
        {
            dialog.Controls.Add(new Label
            {
                Text      = rotulo,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 100),
                AutoSize  = true,
                Location  = new Point(20, y)
            });
            y += 22;

            var txt = new TextBox
            {
                Width        = 400,
                Location     = new Point(20, y),
                Font         = new Font("Segoe UI", 10),
                BackColor    = Color.FromArgb(247, 250, 255),
                BorderStyle  = BorderStyle.FixedSingle,
                PasswordChar = senha ? '*' : '\0'
            };
            dialog.Controls.Add(txt);
            y += 38;
            return txt;
        }

        // Cor das linhas pares e ímpares — usada em todas as grids
        // Definida aqui para ser consistente em todo o sistema
        readonly Color corLinhaBase      = Color.FromArgb(255, 255, 255); // branco
        readonly Color corLinhaAlternada = Color.FromArgb(245, 246, 248); // cinza muito claro (quase branco)

        // Cria um DataGridView padrão estilizado
        // Todas as grids do sistema usam este método — garante visual consistente
        private DataGridView CriarGridBase(int alturaLinha = 40)
        {
            var grid = new DataGridView
            {
                BackgroundColor    = corLinhaBase,
                BorderStyle        = BorderStyle.None,
                CellBorderStyle    = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly           = true,
                AllowUserToAddRows = false,
                RowHeadersVisible  = false,
                Font               = new Font("Segoe UI", 10),
                RowTemplate        = { Height = alturaLinha }
            };

            // Cabeçalho — preto conforme escolha do usuário
            grid.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(30, 30, 30);
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding    = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight                      = 42;
            grid.EnableHeadersVisualStyles                = false;
            // Desabilita o realce azul do cabeçalho ao clicar na coluna para ordenar
            grid.ColumnHeaderMouseClick += (s, ev) => { };  // absorve o clique mas não ordena
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 30, 30);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            // Linha base (pares)
            grid.DefaultCellStyle.BackColor = corLinhaBase;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(40, 45, 60);

            // Linha alternada (ímpares) — cinza quase branco
            grid.AlternatingRowsDefaultCellStyle.BackColor = corLinhaAlternada;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(40, 45, 60);

            // Cor de seleção — azul suave que não "apaga" o conteúdo
            grid.DefaultCellStyle.SelectionBackColor          = Color.FromArgb(180, 200, 230);
            grid.DefaultCellStyle.SelectionForeColor          = Color.FromArgb(20, 30, 60);
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(165, 188, 222);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 30, 60);

            return grid;
        }

        private void AddCol(DataGridView g, string name, string header, int w,
                            DataGridViewAutoSizeColumnMode mode = DataGridViewAutoSizeColumnMode.None)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name         = name,
                HeaderText   = header,
                Width        = w,
                AutoSizeMode = mode,
            };
            // Aplica só o padding — sem definir BackColor/ForeColor na coluna
            // para garantir que as cores da linha (base e alternada) sejam herdadas corretamente
            col.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.Columns.Add(col);
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 1 — DASHBOARD
        // Endpoint varia conforme o perfil:
        //   Emitente   → GET /osc/emitente/{id}  (suas OSCs criadas)
        //   Gerente    → GET /osc/gerente/{id}   (pendentes de assinatura)
        //   Admin/Exec → GET /osc                (todas)
        // ══════════════════════════════════════════════════════════════
        List<OscResponse> todasAsOscs   = new List<OscResponse>(); // lista completa da API
        List<OscResponse> oscsFiltradas = new List<OscResponse>(); // lista após aplicar filtros

        // Define a "prioridade" de ordenação de cada status
        // Menor número = aparece primeiro
        private static int OrdemStatus(string? status)
        {
            return (status ?? "").ToLower() switch
            {
                "aguardandoassinaturas" => 0,   // mais urgente — vem primeiro
                "aguardandovalidacao"   => 1,   // já assinou tudo, aguarda admin
                "concluida"            => 2,   // finalizada — vai para o fim
                "cancelada"            => 3,   // cancelada — última
                _                      => 0
            };
        }

        private async System.Threading.Tasks.Task MostrarDashboard()
        {
            LimparConteudo();

            string perfil = _usuarioLogado.Perfil ?? "";
            string urlOsc;
            string subtitulo;

            if (perfil.Equals("Emitente", StringComparison.OrdinalIgnoreCase))
            {
                urlOsc = $"{URL_BASE}/osc/emitente/{_usuarioLogado.Id}";
                subtitulo = "Suas Ordens de Serviço criadas";
            }
            else if (perfil.Equals("Gerente", StringComparison.OrdinalIgnoreCase))
            {
                urlOsc = $"{URL_BASE}/osc";
                subtitulo = $"OSCs do setor {ExibirSetor(_usuarioLogado.Setor)}";
            }
            else
            {
                urlOsc = $"{URL_BASE}/osc";
                subtitulo = "Todas as Ordens de Serviço";
            }

            Add(new Label
            {
                Text = "Dashboard — Ordens de Serviço",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            Add(new Label
            {
                Text = subtitulo,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 34)
            });

            var lblCarregando = new Label
            {
                Text = "⏳  Buscando dados...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 70)
            };
            Add(lblCarregando);

            try
            {
                var resposta = await ApiConfig.Http.GetAsync(urlOsc);
                if (!resposta.IsSuccessStatusCode)
                {
                    string corpo = await resposta.Content.ReadAsStringAsync();
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}: {corpo}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }
                string json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes)
                                 ?? new List<OscResponse>();

                todasAsOscs = lista
                    .OrderBy(o => OrdemStatus(o.status))
                    .ThenByDescending(o => o.id)
                    .ToList();
            }
            catch (Exception ex)
            {
                lblCarregando.Text = $"❌  {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }

            _painelInterno.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            oscsFiltradas = todasAsOscs.ToList();

            // ── Gráficos — logo abaixo do subtítulo ────────────────────
            const int ALTURA_GRAFICOS = 230;
            const int MARGEM_GRAFICOS = 8;

            // Cria gráficos com Func que lê oscsFiltradas dinamicamente
            var painelGraficos = CriarPainelGraficos(() => oscsFiltradas);
            painelGraficos.Location = new Point(0, 56);
            painelGraficos.Width = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            Add(painelGraficos);

            _painelInterno.Resize += (s, ev) =>
                painelGraficos.Width = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            // ── Barra de filtros — abaixo dos gráficos ─────────────────
            int yFiltros = 56 + ALTURA_GRAFICOS + MARGEM_GRAFICOS;

            var painelFiltros = new Panel
            {
                Location = new Point(0, yFiltros),
                Height = 38,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Width = _painelInterno.Width - _painelInterno.Padding.Horizontal,
                BackColor = Color.Transparent
            };

            var txtBusca = new TextBox
            {
                Width = 220,
                Height = 28,
                Location = new Point(0, 4),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "🔍  Buscar por descrição ou equipamento..."
            };
            painelFiltros.Controls.Add(txtBusca);

            var cmbStatus = new ComboBox
            {
                Width = 190,
                Location = new Point(228, 4),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(247, 250, 255),
                FlatStyle = FlatStyle.Flat
            };
            cmbStatus.Items.AddRange(new object[]
            {
        "Todos os status",
        "AguardandoAssinaturas",
        "AguardandoValidacao",
        "Concluida",
        "Cancelada"
            });
            cmbStatus.SelectedIndex = 0;
            painelFiltros.Controls.Add(cmbStatus);

            var btnLimpar = new Button
            {
                Text = "✖  Limpar",
                Width = 90,
                Height = 28,
                Location = new Point(426, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(60, 60, 80),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            painelFiltros.Controls.Add(btnLimpar);
            Add(painelFiltros);

            // ── Grid — abaixo dos filtros ───────────────────────────────
            int yGrid = yFiltros + 44;

            var grid = CriarGridDashboard();
            grid.Location = new Point(0, yGrid);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            grid.Height = _painelInterno.Height - _painelInterno.Padding.Vertical - yGrid - 8;
            grid.ScrollBars = ScrollBars.Vertical;
            Add(grid);

            void AplicarFiltros()
            {
                string busca = txtBusca.Text.Trim().ToLower();
                string status = cmbStatus.SelectedIndex == 0 ? "" : (cmbStatus.SelectedItem?.ToString() ?? "");

                oscsFiltradas = todasAsOscs.Where(o =>
                {
                    bool passaBusca = string.IsNullOrEmpty(busca)
                        || (o.descricao ?? "").ToLower().Contains(busca)
                        || (o.equipamento ?? "").ToLower().Contains(busca)
                        || (o.emitenteNome ?? "").ToLower().Contains(busca);
                    bool passaStatus = string.IsNullOrEmpty(status)
                        || (o.status ?? "").Equals(status, StringComparison.OrdinalIgnoreCase);
                    return passaBusca && passaStatus;
                }).ToList();

                CarregarDashboard(grid);

                // ── Redesenha os gráficos com a lista filtrada ──
                if (painelGraficos.Tag is Action atualizar)
                    atualizar();
            }

            txtBusca.TextChanged += (s, e) => AplicarFiltros();
            cmbStatus.SelectedIndexChanged += (s, e) => AplicarFiltros();
            btnLimpar.Click += (s, e) =>
            {
                txtBusca.Clear();
                cmbStatus.SelectedIndex = 0;
            };

            if (todasAsOscs.Count == 0)
            {
                Add(new Label
                {
                    Text = "Nenhuma OS encontrada.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(0, yGrid + 10)
                });
                return;
            }

            CarregarDashboard(grid);
        }

        private DataGridView CriarGridDashboard()
        {
            var grid = CriarGridBase(44);

            AddCol(grid, "d_id",     "ID",          55);
            AddCol(grid, "d_desc",   "Descrição",   180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "d_equip",  "Equipamento", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_emit",   "Emitente",    110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_data",   "Data",         95, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_status", "Status",      185, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_assig",  "Assinaturas",  90, DataGridViewAutoSizeColumnMode.AllCells);

            // Coluna extra para Gerente: mostra se ELE já assinou esta OS
            if ((_usuarioLogado.Perfil ?? "").Equals("Gerente", StringComparison.OrdinalIgnoreCase))
                AddCol(grid, "d_minha", "Minha assinatura", 130, DataGridViewAutoSizeColumnMode.AllCells);

            return grid;
        }

        // Carrega TODAS as OSCs filtradas na grid — sem paginação, usa scroll vertical
        private void CarregarDashboard(DataGridView grid)
        {
            grid.Rows.Clear();

            foreach (var osc in oscsFiltradas)
            {

                string data = "—";
                if (!string.IsNullOrEmpty(osc.dataEmissao) &&
                    DateTime.TryParse(osc.dataEmissao, out DateTime dt))
                    data = dt.ToString("dd/MM/yyyy");

                // Assinaturas: mostra Q/E/P com ✅ ou ⏳
                string q = osc.qualidadeAssinou  ? "✅" : "⏳";
                string e = osc.engenhariaAssinou ? "✅" : "⏳";
                string p = osc.producaoAssinou   ? "✅" : "⏳";
                string assig = $"{osc.TotalAssinaturas}/3  G{q} M{e} O{p}";

                int row = grid.Rows.Add(
                    osc.id,
                    osc.descricao   ?? "",
                    osc.equipamento ?? "",
                    osc.emitenteNome?? "",
                    data,
                    osc.status      ?? "",
                    assig
                );

                // Cor do status — fundo claro com texto colorido (sobre fundo branco)
                var celStatus = grid.Rows[row].Cells["d_status"];
                switch ((osc.status ?? "").ToLower())
                {
                    case "concluida":
                    case "concluído":
                        celStatus.Style.ForeColor = Color.FromArgb(0, 130, 70);
                        celStatus.Style.BackColor = Color.FromArgb(220, 255, 235);
                        break;
                    case "aguardandoassinaturas":
                        celStatus.Style.ForeColor = Color.FromArgb(160, 90, 0);
                        celStatus.Style.BackColor = Color.FromArgb(255, 244, 205);
                        break;
                    case "aguardandovalidacao":
                        celStatus.Style.ForeColor = Color.FromArgb(30, 80, 160);
                        celStatus.Style.BackColor = Color.FromArgb(220, 235, 255);
                        break;
                    case "cancelada":
                        celStatus.Style.ForeColor = Color.FromArgb(170, 30, 30);
                        celStatus.Style.BackColor = Color.FromArgb(255, 222, 222);
                        break;
                }

                // Cor das assinaturas — fundo claro com texto colorido
                var celAssig = grid.Rows[row].Cells["d_assig"];
                celAssig.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                if (osc.TotalAssinaturas == 3)
                { celAssig.Style.ForeColor = Color.FromArgb(0, 130, 70);  celAssig.Style.BackColor = Color.FromArgb(220, 255, 235); }
                else if (osc.TotalAssinaturas > 0)
                { celAssig.Style.ForeColor = Color.FromArgb(160, 90, 0);  celAssig.Style.BackColor = Color.FromArgb(255, 244, 205); }
                else
                { celAssig.Style.ForeColor = Color.FromArgb(170, 30, 30); celAssig.Style.BackColor = Color.FromArgb(255, 222, 222); }

                // Coluna "Minha assinatura" — visível só para Gerentes
                // Mostra se o gerente logado já assinou esta OS específica
                if ((_usuarioLogado.Perfil ?? "").Equals("Gerente", StringComparison.OrdinalIgnoreCase)
                    && grid.Columns.Contains("d_minha"))
                {
                    bool euAssinei = JaAssinouNaOsc(osc);
                    var celMinha   = grid.Rows[row].Cells["d_minha"];
                    celMinha.Value = euAssinei ? "✅  Assinado" : "⏳  Pendente";
                    celMinha.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    if (euAssinei)
                    {
                        celMinha.Style.ForeColor = Color.FromArgb(0, 130, 70);
                        celMinha.Style.BackColor = Color.FromArgb(220, 255, 235);
                    }
                    else
                    {
                        celMinha.Style.ForeColor = Color.FromArgb(160, 90, 0);
                        celMinha.Style.BackColor = Color.FromArgb(255, 244, 205);
                    }
                }
            }
        }
        // ══════════════════════════════════════════════════════════════
        // GRÁFICOS DO DASHBOARD
        // Exibe um painel horizontal com dois charts:
        //   • Pizza  → distribuição por status
        //   • Barras → progresso de assinaturas (0 / 1–2 / 3)
        // ══════════════════════════════════════════════════════════════
        //    private Panel CriarPainelGraficos(List<OscResponse> lista)
        //    {
        //        // Painel contenedor — faixa horizontal
        //        var painel = new Panel
        //        {
        //            Height = 220,
        //            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        //            BackColor = Color.Transparent
        //        };

        //        // ── GRÁFICO 1: Pizza por status ─────────────────────────────
        //        var chartPizza = new Chart
        //        {
        //            Width = 340,
        //            Height = 210,
        //            Location = new Point(0, 0),
        //            BackColor = Color.White
        //        };
        //        chartPizza.Titles.Add(new Title("Status das OSs")
        //        {
        //            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //            ForeColor = Color.FromArgb(25, 35, 70)
        //        });

        //        var areaPizza = new ChartArea("pizza");
        //        areaPizza.BackColor = Color.White;
        //        chartPizza.ChartAreas.Add(areaPizza);

        //        var seriePizza = new Series("status")
        //        {
        //            ChartType = SeriesChartType.Pie,
        //            IsValueShownAsLabel = true,
        //            LabelFormat = "{0} ({P0})",
        //            Font = new Font("Segoe UI", 8)
        //        };

        //        // Conta por status
        //        var porStatus = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { "AguardandoAssinaturas", 0 },
        //    { "AguardandoValidacao",   0 },
        //    { "Concluida",             0 },
        //    { "Cancelada",             0 }
        //};
        //        foreach (var o in lista)
        //        {
        //            string s = o.status ?? "Desconhecido";
        //            if (porStatus.ContainsKey(s)) porStatus[s]++;
        //            else porStatus[s] = 1;
        //        }

        //        // Labels mais legíveis para a pizza
        //        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { "AguardandoAssinaturas", "Ag. Assinaturas" },
        //    { "AguardandoValidacao",   "Ag. Validação"   },
        //    { "Concluida",             "Concluída"        },
        //    { "Cancelada",             "Cancelada"        }
        //};
        //        var cores = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { "AguardandoAssinaturas", Color.FromArgb(255, 195,  60) },  // amarelo
        //    { "AguardandoValidacao",   Color.FromArgb( 60, 130, 210) },  // azul
        //    { "Concluida",             Color.FromArgb( 40, 170,  90) },  // verde
        //    { "Cancelada",             Color.FromArgb(210,  60,  60) }   // vermelho
        //};

        //        foreach (var kv in porStatus)
        //        {
        //            if (kv.Value == 0) continue;
        //            string lbl = labels.TryGetValue(kv.Key, out var l) ? l : kv.Key;
        //            int idx = seriePizza.Points.AddXY(lbl, kv.Value);
        //            if (cores.TryGetValue(kv.Key, out var cor))
        //                seriePizza.Points[idx].Color = cor;
        //        }

        //        // Se não houver dados, mostra placeholder
        //        if (seriePizza.Points.Count == 0)
        //            seriePizza.Points.AddXY("Sem dados", 1);

        //        chartPizza.Series.Add(seriePizza);
        //        chartPizza.Legends.Clear();  // legenda está nos rótulos da própria fatia
        //        painel.Controls.Add(chartPizza);

        //        // ── GRÁFICO 2: Barras — assinaturas coletadas ───────────────
        //        var chartBarras = new Chart
        //        {
        //            Width = 340,
        //            Height = 210,
        //            Location = new Point(356, 0),
        //            BackColor = Color.White
        //        };
        //        chartBarras.Titles.Add(new Title("Progresso de Assinaturas")
        //        {
        //            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //            ForeColor = Color.FromArgb(25, 35, 70)
        //        });

        //        var areaBarras = new ChartArea("barras");
        //        areaBarras.BackColor = Color.White;
        //        areaBarras.AxisX.MajorGrid.Enabled = false;
        //        areaBarras.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 225, 235);
        //        areaBarras.AxisY.Minimum = 0;
        //        areaBarras.AxisY.Interval = 1;
        //        chartBarras.ChartAreas.Add(areaBarras);

        //        var serieBarras = new Series("assinaturas")
        //        {
        //            ChartType = SeriesChartType.Column,
        //            IsValueShownAsLabel = true,
        //            Font = new Font("Segoe UI", 9, FontStyle.Bold),
        //            Color = Color.FromArgb(30, 80, 160)
        //        };

        //        int s0 = lista.Count(o => o.TotalAssinaturas == 0);
        //        int s1 = lista.Count(o => o.TotalAssinaturas == 1);
        //        int s2 = lista.Count(o => o.TotalAssinaturas == 2);
        //        int s3 = lista.Count(o => o.TotalAssinaturas == 3);

        //        serieBarras.Points.AddXY("0 de 3", s0); serieBarras.Points[0].Color = Color.FromArgb(210, 60, 60);
        //        serieBarras.Points.AddXY("1 de 3", s1); serieBarras.Points[1].Color = Color.FromArgb(210, 130, 40);
        //        serieBarras.Points.AddXY("2 de 3", s2); serieBarras.Points[2].Color = Color.FromArgb(255, 195, 60);
        //        serieBarras.Points.AddXY("3 de 3", s3); serieBarras.Points[3].Color = Color.FromArgb(40, 170, 90);

        //        chartBarras.Series.Add(serieBarras);
        //        chartBarras.Legends.Clear();
        //        painel.Controls.Add(chartBarras);

        //        // ── GRÁFICO 3: Barra empilhada — total geral (card resumo) ──
        //        var painelCard = new Panel
        //        {
        //            Width = 220,
        //            Height = 210,
        //            Location = new Point(712, 0),
        //            BackColor = Color.White,
        //            BorderStyle = BorderStyle.None
        //        };

        //        // Sombra suave via Paint
        //        painelCard.Paint += (s, ev) =>
        //        {
        //            ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        //            var rect = new Rectangle(1, 1, painelCard.Width - 3, painelCard.Height - 3);
        //            using var brush = new SolidBrush(Color.White);
        //            ev.Graphics.FillRectangle(brush, rect);
        //            using var pen = new Pen(Color.FromArgb(220, 225, 235), 1);
        //            ev.Graphics.DrawRectangle(pen, rect);
        //        };

        //        // Cards de contagem por status dentro desse painel
        //        int yCard = 14;
        //        var cardsInfo = new (string Label, int Valor, Color Cor)[]
        //        {
        //    ("Total de OSs",          lista.Count,                              Color.FromArgb(25, 35, 70)      ),
        //    ("Ag. Assinaturas",       porStatus.GetValueOrDefault("AguardandoAssinaturas"), Color.FromArgb(160, 90, 0)   ),
        //    ("Ag. Validação",         porStatus.GetValueOrDefault("AguardandoValidacao"),   Color.FromArgb(30, 80, 160) ),
        //    ("Concluídas",            porStatus.GetValueOrDefault("Concluida"),              Color.FromArgb(0, 130, 70)  ),
        //    ("Canceladas",            porStatus.GetValueOrDefault("Cancelada"),              Color.FromArgb(170, 30, 30) ),
        //        };

        //        painelCard.Controls.Add(new Label
        //        {
        //            Text = "Resumo",
        //            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        //            ForeColor = Color.FromArgb(25, 35, 70),
        //            AutoSize = true,
        //            Location = new Point(12, yCard)
        //        });
        //        yCard += 28;

        //        foreach (var (lbl, val, cor) in cardsInfo)
        //        {
        //            painelCard.Controls.Add(new Label
        //            {
        //                Text = lbl,
        //                Font = new Font("Segoe UI", 8),
        //                ForeColor = Color.Gray,
        //                AutoSize = true,
        //                Location = new Point(12, yCard)
        //            });
        //            painelCard.Controls.Add(new Label
        //            {
        //                Text = val.ToString(),
        //                Font = new Font("Segoe UI", 14, FontStyle.Bold),
        //                ForeColor = cor,
        //                AutoSize = true,
        //                Location = new Point(150, yCard - 4)
        //            });
        //            yCard += 30;
        //        }

        //        painel.Controls.Add(painelCard);

        //        return painel;
        //    }
        // ══════════════════════════════════════════════════════════════
        // GRÁFICOS DO DASHBOARD — desenhados com GDI+ puro
        // Sem dependência de System.Windows.Forms.DataVisualization
        // ══════════════════════════════════════════════════════════════
        // Agora recebe um Func<List<OscResponse>> para poder ser redesenhado com filtros
        private Panel CriarPainelGraficos(Func<List<OscResponse>> obterLista)
        {
            var porStatus = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "AguardandoAssinaturas", 0 },
        { "AguardandoValidacao",   0 },
        { "Concluida",             0 },
        { "Cancelada",             0 }
    };

            var painel = new Panel
            {
                Height = 230,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            painel.Controls.Add(table);

            // ── PIZZA ────────────────────────────────────────────────────
            var pizzaLabels = new[] { "Ag. Assinaturas", "Ag. Validação", "Concluída", "Cancelada" };
            var pizzaKeys = new[] { "AguardandoAssinaturas", "AguardandoValidacao", "Concluida", "Cancelada" };
            var pizzaCores = new[]
            {
        Color.FromArgb(255, 195,  60),
        Color.FromArgb( 60, 130, 210),
        Color.FromArgb( 40, 170,  90),
        Color.FromArgb(210,  60,  60)
    };

            var pnlPizza = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 6, 0) };
            pnlPizza.Paint += (s, ev) =>
            {
                var lista = obterLista();
                var ps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "AguardandoAssinaturas", 0 }, { "AguardandoValidacao", 0 },
            { "Concluida", 0 }, { "Cancelada", 0 }
        };
                foreach (var o in lista)
                    if (ps.ContainsKey(o.status ?? "")) ps[o.status!]++;

                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using var fTit = new Font("Segoe UI", 10, FontStyle.Bold);
                g.DrawString("Status das OSs", fTit, new SolidBrush(Color.FromArgb(25, 35, 70)), 8, 8);

                int total = lista.Count;
                if (total == 0) { g.DrawString("Sem dados", fTit, Brushes.Gray, 40, 90); return; }

                int H = pnlPizza.Height;
                // ── Limita o diâmetro para não crescer demais quando a janela expande
                int diam = Math.Min(160, H - 50);
                if (diam < 20) return;
                int px = 14;
                int py = (H - diam) / 2 + 8;
                var rect = new Rectangle(px, py, diam, diam);

                float startAngle = -90f;
                for (int i = 0; i < pizzaKeys.Length; i++)
                {
                    int val = ps[pizzaKeys[i]];
                    if (val == 0) continue;
                    float sweep = 360f * val / total;
                    using var br = new SolidBrush(pizzaCores[i]);
                    g.FillPie(br, rect, startAngle, sweep);
                    using var pen = new Pen(Color.White, 2);
                    g.DrawPie(pen, rect, startAngle, sweep);
                    startAngle += sweep;
                }

                using var fLeg = new Font("Segoe UI", 8.5f);
                int lx = px + diam + 14;
                int ly = py;
                for (int i = 0; i < pizzaKeys.Length; i++)
                {
                    int val = ps[pizzaKeys[i]];
                    using var br = new SolidBrush(pizzaCores[i]);
                    g.FillRectangle(br, lx, ly + 3, 11, 11);
                    g.DrawString($"{pizzaLabels[i]}: {val}", fLeg,
                        new SolidBrush(Color.FromArgb(40, 45, 60)), lx + 16, ly);
                    ly += 22;
                }
            };
            table.Controls.Add(pnlPizza, 0, 0);

            // ── BARRAS ───────────────────────────────────────────────────
            var barLabels = new[] { "Nenhuma", "1 ass.", "2 ass.", "Todas" };
            var barCores = new[]
            {
        Color.FromArgb(210,  60,  60),
        Color.FromArgb(210, 130,  40),
        Color.FromArgb(255, 195,  60),
        Color.FromArgb( 40, 170,  90)
    };

            var pnlBarras = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6, 0, 6, 0) };
            pnlBarras.Paint += (s, ev) =>
            {
                var lista = obterLista();
                int[] barVals = {
            lista.Count(o => o.TotalAssinaturas == 0),
            lista.Count(o => o.TotalAssinaturas == 1),
            lista.Count(o => o.TotalAssinaturas == 2),
            lista.Count(o => o.TotalAssinaturas == 3)
        };

                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                using var fTit = new Font("Segoe UI", 10, FontStyle.Bold);
                g.DrawString("Progresso de Assinaturas", fTit, new SolidBrush(Color.FromArgb(25, 35, 70)), 8, 8);

                int W = pnlBarras.Width;
                int H = pnlBarras.Height;
                int maxVal = Math.Max(1, barVals.Max());
                int chartX = 32;
                int chartY = 34;
                int chartW = W - chartX - 12;
                int chartH = H - chartY - 38;
                if (chartH < 10) return;

                // Limita a largura da área de barras para não ficarem largas demais
                int maxChartW = 320;
                if (chartW > maxChartW)
                {
                    int offset = (chartW - maxChartW) / 2;
                    chartX += offset;
                    chartW = maxChartW;
                }

                int barW = chartW / barVals.Length;
                int gap = Math.Max(4, barW / 5);

                using var fAxis = new Font("Segoe UI", 8f);
                using var penGrid = new Pen(Color.FromArgb(225, 228, 235), 1);

                for (int i = 0; i <= 4; i++)
                {
                    int yLine = chartY + chartH - (int)(chartH * i / 4.0);
                    int labelVal = (int)Math.Round(maxVal * i / 4.0);
                    g.DrawLine(penGrid, chartX, yLine, chartX + chartW, yLine);
                    var sz = g.MeasureString(labelVal.ToString(), fAxis);
                    g.DrawString(labelVal.ToString(), fAxis,
                        new SolidBrush(Color.FromArgb(150, 155, 165)),
                        chartX - sz.Width - 2, yLine - sz.Height / 2);
                }

                for (int i = 0; i < barVals.Length; i++)
                {
                    int barH = maxVal == 0 ? 0 : (int)((double)barVals[i] / maxVal * chartH);
                    int bx = chartX + i * barW + gap;
                    int bw = barW - gap * 2;
                    int by = chartY + chartH - barH;

                    using var br = new SolidBrush(barCores[i]);
                    if (barH > 4)
                    {
                        int r = Math.Min(5, bw / 3);
                        var path = new GraphicsPath();
                        path.AddArc(bx, by, r * 2, r * 2, 180, 90);
                        path.AddArc(bx + bw - r * 2, by, r * 2, r * 2, 270, 90);
                        path.AddLine(bx + bw, by + r, bx + bw, by + barH);
                        path.AddLine(bx + bw, by + barH, bx, by + barH);
                        path.CloseFigure();
                        g.FillPath(br, path);
                    }
                    else if (barH > 0)
                        g.FillRectangle(br, bx, by, bw, barH);

                    if (barVals[i] > 0)
                    {
                        string txt = barVals[i].ToString();
                        var sz = g.MeasureString(txt, fAxis);
                        g.DrawString(txt, fAxis, new SolidBrush(Color.FromArgb(40, 45, 60)),
                            bx + (bw - sz.Width) / 2, Math.Max(chartY, by - 15));
                    }

                    var szL = g.MeasureString(barLabels[i], fAxis);
                    g.DrawString(barLabels[i], fAxis, new SolidBrush(Color.FromArgb(90, 95, 110)),
                        bx + (bw - szL.Width) / 2, chartY + chartH + 5);
                }
            };
            table.Controls.Add(pnlBarras, 1, 0);

            // ── CARD RESUMO ──────────────────────────────────────────────
            var painelCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6, 0, 0, 0) };
            painelCard.Paint += (s, ev) =>
            {
                using var pen = new Pen(Color.FromArgb(220, 225, 235), 1);
                ev.Graphics.DrawRectangle(pen, 0, 0, painelCard.Width - 1, painelCard.Height - 1);
            };

            // Labels do card são recriados ao invalidar — usamos Paint para texto dinâmico
            var cardLabels = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            cardLabels.Paint += (s, ev) =>
            {
                var lista = obterLista();
                var ps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "AguardandoAssinaturas", 0 }, { "AguardandoValidacao", 0 },
            { "Concluida", 0 }, { "Cancelada", 0 }
        };
                foreach (var o in lista)
                    if (ps.ContainsKey(o.status ?? "")) ps[o.status!]++;

                var g = ev.Graphics;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                var cardsInfo = new (string Label, int Valor, Color Cor)[]
                {
            ("Total de OSs",    lista.Count,                        Color.FromArgb(25,  35,  70)),
            ("Ag. Assinaturas", ps["AguardandoAssinaturas"],        Color.FromArgb(160, 90,   0)),
            ("Ag. Validação",   ps["AguardandoValidacao"],          Color.FromArgb(30,  80, 160)),
            ("Concluídas",      ps["Concluida"],                    Color.FromArgb(0,  130,  70)),
            ("Canceladas",      ps["Cancelada"],                    Color.FromArgb(170, 30,  30)),
                };

                using var fTit = new Font("Segoe UI", 10, FontStyle.Bold);
                g.DrawString("Resumo", fTit, new SolidBrush(Color.FromArgb(25, 35, 70)), 10, 10);

                using var penSep = new Pen(Color.FromArgb(220, 225, 235), 1);
                g.DrawLine(penSep, 10, 30, cardLabels.Width - 10, 30);

                using var fLbl = new Font("Segoe UI", 8f);
                using var fVal = new Font("Segoe UI", 13, FontStyle.Bold);
                int yc = 38;
                foreach (var (lbl, val, cor) in cardsInfo)
                {
                    g.DrawString(lbl, fLbl, new SolidBrush(Color.Gray), 10, yc);
                    g.DrawString(val.ToString(), fVal, new SolidBrush(cor), 10, yc + 12);
                    yc += 34;
                }
            };
            painelCard.Controls.Add(cardLabels);
            table.Controls.Add(painelCard, 2, 0);

            // Método público no painel para forçar redesenho de todos os gráficos
            painel.Tag = (Action)(() =>
            {
                pnlPizza.Invalidate();
                pnlBarras.Invalidate();
                cardLabels.Invalidate();
            });

            return painel;
        }
        // ══════════════════════════════════════════════════════════════
        // TELA 2 — CRIAR OS
        // POST /osc com: descricao, equipamento, acaoTomada, usuarioLogadoId
        // ══════════════════════════════════════════════════════════════
        private void MostrarCriarOS()
        {
            LimparConteudo();

            // Título
            Add(new Label
            {
                Text      = "Criar Nova Ordem de Serviço",
                Font      = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize  = true,
                Location  = new Point(0, 0)
            });

            // Largura real disponível (descontando padding do painel interno)
            int W = _painelInterno.ClientSize.Width - _painelInterno.Padding.Horizontal;
            if (W < 200) W = 500; // fallback caso ainda não tenha sido calculado

            int y = 48;
            int espacoEntreGrupos = 18;

            // ── helpers ──────────────────────────────────────────────
            Label MkLbl(string t, int posY) => new Label
            {
                Text      = t,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 100),
                AutoSize  = true,
                Location  = new Point(0, posY)
            };

            TextBox MkTxt(int posY, int h = 36, bool multi = false) => new TextBox
            {
                Location    = new Point(0, posY),
                Width       = W,
                Height      = h,
                Font        = new Font("Segoe UI", 10),
                BackColor   = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline   = multi,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ── campos ───────────────────────────────────────────────
            Add(MkLbl("Descrição:", y));
            y += 24;
            var txtDescricao = MkTxt(y);
            Add(txtDescricao);
            y += txtDescricao.Height + espacoEntreGrupos;

            Add(MkLbl("Equipamento:", y));
            y += 24;
            var txtEquipamento = MkTxt(y);
            Add(txtEquipamento);
            y += txtEquipamento.Height + espacoEntreGrupos;

            Add(MkLbl("Ação Tomada:", y));
            y += 24;
            var txtAcao = MkTxt(y, h: 120, multi: true);
            Add(txtAcao);
            y += txtAcao.Height + espacoEntreGrupos + 8;

            // ── botão centralizado ────────────────────────────────────
            var btnSalvar = new Button
            {
                Text           = "💾  Salvar",
                Width          = 200,
                Height         = 46,
                FlatStyle      = FlatStyle.Flat,
                BackColor      = corPrimaria,
                ForeColor      = Color.White,
                Font           = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

            // Centraliza o botão e recalcula quando o painel redimensiona
            void CentralizarBotao()
            {
                int wAtual = _painelInterno.ClientSize.Width - _painelInterno.Padding.Horizontal;
                btnSalvar.Location = new Point(Math.Max(0, (wAtual - btnSalvar.Width) / 2), y);
            }
            CentralizarBotao();
            _painelInterno.Resize += (s, ev) =>
            {
                // Atualiza largura dos campos e posição do botão ao redimensionar
                int wAtual = _painelInterno.ClientSize.Width - _painelInterno.Padding.Horizontal;
                txtDescricao.Width   = wAtual;
                txtEquipamento.Width = wAtual;
                txtAcao.Width        = wAtual;
                CentralizarBotao();
            };
            Add(btnSalvar);

            btnSalvar.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtDescricao.Text))
                { MessageBox.Show("Informe a descrição!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtEquipamento.Text))
                { MessageBox.Show("Informe o equipamento!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtAcao.Text))
                { MessageBox.Show("Informe a ação tomada!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // CriarOscRequest: descricao, equipamento, acaoTomada, usuarioLogadoId
                var payload = new
                {
                    descricao      = txtDescricao.Text,
                    equipamento    = txtEquipamento.Text,
                    acaoTomada     = txtAcao.Text,
                    usuarioLogadoId= _usuarioLogado.Id
                };

                string json  = JsonSerializer.Serialize(payload);
                var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

                btnSalvar.Enabled = false;
                btnSalvar.Text    = "⏳  Salvando...";

                try
                {
                    var resposta = await ApiConfig.Http.PostAsync($"{URL_BASE}/osc", conteudo);
                    if (resposta.IsSuccessStatusCode)
                    {
                        MessageBox.Show("OS criada com sucesso!", "Sucesso",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await MostrarDashboard();
                        DestaqueBotao(btnDashboard);
                    }
                    else
                    {
                        string erro = await resposta.Content.ReadAsStringAsync();
                        MessageBox.Show($"Erro da API:\n{(int)resposta.StatusCode}\n\n{erro}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSalvar.Enabled = true;
                    btnSalvar.Text    = "💾  Salvar";
                }
            };

        }

        // ══════════════════════════════════════════════════════════════
        // TELA 3 — ASSINAR OS (só Gerente)
        // GET /osc/gerente/{id} → já vem filtrado pelo backend
        //   (só OSCs com AguardandoAssinaturas onde o setor não assinou)
        // POST /osc/{id}/assinar com body: int usuarioId (JSON puro)
        // Verificação se já assinou: baseada no setor do gerente
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task MostrarAssinar()
        {
            LimparConteudo();

            Add(new Label
            {
                Text      = "Assinar Ordem de Serviço",
                Font      = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize  = true,
                Location  = new Point(0, 0)
            });
            Add(new Label
            {
                Text      = $"OSCs pendentes para o setor {ExibirSetor(_usuarioLogado.Setor)} — {_usuarioLogado.Nome}",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(0, 36)
            });

            var lblLoad = new Label { Text="⏳  Buscando...", Font=new Font("Segoe UI",10), ForeColor=Color.Gray, AutoSize=true, Location=new Point(0,70) };
            Add(lblLoad);

            var oscs = new List<OscResponse>();
            try
            {
                var r = await ApiConfig.Http.GetAsync($"{URL_BASE}/osc/gerente/{_usuarioLogado.Id}");
                if (r.IsSuccessStatusCode)
                {
                    var op = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    oscs = JsonSerializer.Deserialize<List<OscResponse>>(
                               await r.Content.ReadAsStringAsync(), op)
                           ?? new List<OscResponse>();
                }
                else
                {
                    lblLoad.Text = $"❌  Erro {(int)r.StatusCode}";
                    lblLoad.ForeColor = Color.Red;
                    return;
                }
            }
            catch (Exception ex)
            {
                lblLoad.Text = $"❌  {ex.Message}"; lblLoad.ForeColor = Color.Red; return;
            }

            _painelInterno.Controls.Remove(lblLoad);
            lblLoad.Dispose();

            if (oscs.Count == 0)
            {
                Add(new Label
                {
                    Text      = "✅  Nenhuma OS pendente de assinatura para o seu setor.",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(0, 130, 70),
                    AutoSize  = true,
                    Location  = new Point(0, 70)
                });
                return;
            }

            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 68);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width    = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            grid.Height   = _painelInterno.Height - _painelInterno.Padding.Vertical - 130;

            AddCol(grid, "s_id",     "ID",          55);
            AddCol(grid, "s_desc",   "Descrição",  180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "s_equip",  "Equipamento",130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "s_emit",   "Emitente",   110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "s_prog",   "Progresso",   80, DataGridViewAutoSizeColumnMode.AllCells);

            var mapaOscs = new Dictionary<int, OscResponse>();

            foreach (var osc in oscs)
            {
                string prog = $"{osc.TotalAssinaturas}/3";
                int row = grid.Rows.Add(osc.id, osc.descricao ?? "", osc.equipamento ?? "", osc.emitenteNome ?? "", prog);
                mapaOscs[row] = osc;

                // Colorir progresso — fundo claro com texto colorido
                var cel = grid.Rows[row].Cells["s_prog"];
                cel.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                if (osc.TotalAssinaturas == 3)
                { cel.Style.ForeColor = Color.FromArgb(0, 130, 70);  cel.Style.BackColor = Color.FromArgb(220, 255, 235); }
                else
                { cel.Style.ForeColor = Color.FromArgb(160, 90, 0);  cel.Style.BackColor = Color.FromArgb(255, 244, 205); }
            }

            Add(grid);

            // Separador visual entre a grid e o botão
            Add(new Panel
            {
                Location  = new Point(0, _painelInterno.Height - _painelInterno.Padding.Vertical - 68),
                Width     = _painelInterno.Width - _painelInterno.Padding.Horizontal,
                Height    = 1,
                BackColor = Color.FromArgb(220, 225, 235),
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            });

            var btnAssinarOk = new Button
            {
                Text           = "✅  Assinar OS Selecionada",
                Width          = 230, Height = 42,
                Location       = new Point(0, _painelInterno.Height - _painelInterno.Padding.Vertical - 58),
                Anchor         = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatStyle      = FlatStyle.Flat,
                BackColor      = Color.FromArgb(0, 135, 75),
                ForeColor      = Color.White,
                Font           = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 },
                Margin         = new Padding(0, 12, 0, 0)
            };

            btnAssinarOk.Click += async (s, e) =>
            {
                if (grid.SelectedRows.Count == 0)
                { MessageBox.Show("Selecione uma OS.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                int rowIdx = grid.SelectedRows[0].Index;
                if (!mapaOscs.TryGetValue(rowIdx, out var osc)) return;

                // Verificar se o setor do gerente já assinou
                // GET /osc/gerente/{id} já filtra, mas fazemos verificação local também
                if (JaAssinouNaOsc(osc))
                {
                    MessageBox.Show("Seu setor já assinou esta OS.", "Já assinado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Confirma a assinatura da OS-{osc.id:D3}?\n\n{osc.descricao}",
                    "Confirmar Assinatura", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                btnAssinarOk.Enabled = false;
                btnAssinarOk.Text    = "⏳  Assinando...";
                try
                {
                    // POST /osc/{id}/assinar — body é int puro
                    var conteudo = new StringContent(
                        _usuarioLogado.Id.ToString(), Encoding.UTF8, "application/json");

                    var resposta = await ApiConfig.Http.PostAsync(
                        $"{URL_BASE}/osc/{osc.id}/assinar", conteudo);

                    if (resposta.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"OS-{osc.id:D3} assinada com sucesso!", "Sucesso",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await MostrarAssinar();
                        DestaqueBotao(btnAssinar);
                    }
                    else if (resposta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Você não tem permissão para assinar esta OS.",
                            "Não autorizado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        string erro = await resposta.Content.ReadAsStringAsync();
                        MessageBox.Show($"Erro ao assinar:\n{erro}", "Erro",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnAssinarOk.Enabled = true;
                    btnAssinarOk.Text    = "✅  Assinar OS Selecionada";
                }
            };

            Add(btnAssinarOk);
        }

        // ══════════════════════════════════════════════════════════════
        // TRADUÇÃO DE SETORES — apenas visual, não afeta a API
        // Backend usa:    Qualidade  |  Engenharia  |  Producao
        // Front exibe: Garantia de Validação | Mecatrônica | Operação
        // Use ExibirSetor() em tudo que o usuário VÊ.
        // NUNCA use em valores enviados à API.
        // ══════════════════════════════════════════════════════════════
        private static string ExibirSetor(string? setor) => (setor ?? "").ToLower() switch
        {
            "qualidade"  => "Garantia de Validação",
            "engenharia" => "Mecatrônica",
            "producao"   => "Operação",
            var s        => setor ?? s   // Nenhum, etc. passam sem alteração
        };

        // Verifica se o setor do gerente logado já assinou esta OS
        private bool JaAssinouNaOsc(OscResponse osc)
        {
            string setor = (_usuarioLogado.Setor ?? "").ToLower();
            return setor switch
            {
                "qualidade"  => osc.qualidadeAssinou,
                "engenharia" => osc.engenhariaAssinou,
                "producao"   => osc.producaoAssinou,
                _            => false
            };
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 4 — GERENCIAR OSCs (só Admin)
        // Lista OSCs com AguardandoValidacao — pode Concluir ou Cancelar
        // PUT /osc/{id}/concluir  — body: int usuarioId
        // PUT /osc/{id}/cancelar  — body: int usuarioId
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task MostrarGerenciarOscs()
        {
            LimparConteudo();

            Add(new Label
            {
                Text      = "🛠️  Gerenciar OSs",
                Font      = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize  = true,
                Location  = new Point(0, 0)
            });
            Add(new Label
            {
                Text      = "OSs ativas — Admin pode concluir (após todas assinaturas) ou cancelar qualquer OS",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(0, 34)
            });

            var lblLoad = new Label { Text="⏳  Buscando...", Font=new Font("Segoe UI",10), ForeColor=Color.Gray, AutoSize=true, Location=new Point(0,70) };
            Add(lblLoad);

            var todasOscs = new List<OscResponse>();
            try
            {
                var r = await ApiConfig.Http.GetAsync($"{URL_BASE}/osc");
                if (r.IsSuccessStatusCode)
                {
                    var op = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    todasOscs = JsonSerializer.Deserialize<List<OscResponse>>(
                                    await r.Content.ReadAsStringAsync(), op)
                                ?? new List<OscResponse>();
                }
                else
                {
                    lblLoad.Text = $"❌  Erro {(int)r.StatusCode}"; lblLoad.ForeColor = Color.Red; return;
                }
            }
            catch (Exception ex) { lblLoad.Text=$"❌  {ex.Message}"; lblLoad.ForeColor=Color.Red; return; }

            _painelInterno.Controls.Remove(lblLoad);
            lblLoad.Dispose();

            // Mostra todas as OSCs que ainda podem ser gerenciadas (ativas)
            // AguardandoAssinaturas → admin pode cancelar
            // AguardandoValidacao   → admin pode concluir OU cancelar
            var paraValidar = todasOscs.FindAll(o =>
            {
                string s = (o.status ?? "").ToLower();
                return s == "aguardandoassinaturas" || s == "aguardandovalidacao";
            });

            if (paraValidar.Count == 0)
            {
                Add(new Label
                {
                    Text      = "Nenhuma OS ativa no momento.",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    AutoSize  = true,
                    Location  = new Point(0, 70)
                });
                return;
            }

            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 68);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width    = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            grid.Height   = _painelInterno.Height - _painelInterno.Padding.Vertical - 130;

            AddCol(grid, "g_id",     "ID",          55);
            AddCol(grid, "g_desc",   "Descrição",  180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "g_equip",  "Equipamento",130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_emit",   "Emitente",   110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_data",   "Data",        95, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_status", "Status",     160, DataGridViewAutoSizeColumnMode.AllCells);

            // Botão Concluir
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "g_concluir", HeaderText = "", Text = "✅  Concluir",
                UseColumnTextForButtonValue = true, Width = 120, FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(0,135,75), ForeColor = Color.White, Font = new Font("Segoe UI",9,FontStyle.Bold) }
            });

            // Botão Cancelar
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "g_cancelar", HeaderText = "", Text = "❌  Cancelar",
                UseColumnTextForButtonValue = true, Width = 120, FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(180,30,30), ForeColor = Color.White, Font = new Font("Segoe UI",9,FontStyle.Bold) }
            });

            var mapaOscs = new Dictionary<int, OscResponse>();

            foreach (var osc in paraValidar)
            {
                string data = "—";
                if (!string.IsNullOrEmpty(osc.dataEmissao) && DateTime.TryParse(osc.dataEmissao, out DateTime dt))
                    data = dt.ToString("dd/MM/yyyy");

                int row = grid.Rows.Add(osc.id, osc.descricao ?? "", osc.equipamento ?? "",
                                        osc.emitenteNome ?? "", data, osc.status ?? "");
                mapaOscs[row] = osc;
            }

            // CellFormatting: força cores fixas nos botões Concluir/Cancelar
            grid.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0) return;

                if (ev.ColumnIndex == grid.Columns["g_concluir"].Index)
                {
                    // Verde, mas acinzentado se a OS ainda não tem todas as assinaturas
                    bool podeConc = mapaOscs.TryGetValue(ev.RowIndex, out var o) && o != null &&
                                    (o.status ?? "").Equals("AguardandoValidacao", StringComparison.OrdinalIgnoreCase);

                    ev.CellStyle.BackColor          = podeConc ? Color.FromArgb(0, 135, 75) : Color.FromArgb(120, 140, 125);
                    ev.CellStyle.ForeColor          = Color.White;
                    ev.CellStyle.SelectionBackColor = podeConc ? Color.FromArgb(0, 105, 58) : Color.FromArgb(95, 112, 100);
                    ev.CellStyle.SelectionForeColor = Color.White;
                    ev.FormattingApplied = true;
                }
                else if (ev.ColumnIndex == grid.Columns["g_cancelar"].Index)
                {
                    ev.CellStyle.BackColor          = Color.FromArgb(180, 30, 30);
                    ev.CellStyle.ForeColor          = Color.White;
                    ev.CellStyle.SelectionBackColor = Color.FromArgb(140, 20, 20);
                    ev.CellStyle.SelectionForeColor = Color.White;
                    ev.FormattingApplied = true;
                }
            };

            grid.CellClick += async (s, ev) =>
            {
                if (ev.RowIndex < 0) return;
                if (!mapaOscs.TryGetValue(ev.RowIndex, out var osc)) return;

                if (ev.ColumnIndex == grid.Columns["g_concluir"].Index)
                {
                    // Concluir só é permitido quando todas as assinaturas foram coletadas
                    if (!(osc.status ?? "").Equals("AguardandoValidacao", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(
                            $"Nao e possivel concluir a OS-{osc.id:D3}.\n\n" +
                            $"Assinaturas coletadas: {osc.TotalAssinaturas}/3\n" +
                            "A OS precisa de todas as 3 assinaturas antes de ser concluida.",
                            "Nao permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var confirm = MessageBox.Show(
                        $"Concluir a OS-{osc.id:D3}?\n\n{osc.descricao}",
                        "Confirmar Conclusao", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                        await ExecutarAcaoOsc(osc.id, "concluir");
                }
                else if (ev.ColumnIndex == grid.Columns["g_cancelar"].Index)
                {
                    string avisoAssinaturas = osc.TotalAssinaturas > 0
                        ? $"\n\nEsta OS ja tem {osc.TotalAssinaturas}/3 assinatura(s)."
                        : "";

                    var confirm = MessageBox.Show(
                        $"Cancelar a OS-{osc.id:D3}?\nEsta acao nao pode ser desfeita.{avisoAssinaturas}\n\n{osc.descricao}",
                        "Confirmar Cancelamento", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                        await ExecutarAcaoOsc(osc.id, "cancelar");
                }
            };

            Add(grid);
        }

        // Executa PUT /osc/{id}/concluir ou PUT /osc/{id}/cancelar
        private async System.Threading.Tasks.Task ExecutarAcaoOsc(int oscId, string acao)
        {
            try
            {
                // Body é int puro (o usuarioId do admin)
                var conteudo = new StringContent(
                    _usuarioLogado.Id.ToString(), Encoding.UTF8, "application/json");

                var resposta = await ApiConfig.Http.PutAsync(
                    $"{URL_BASE}/osc/{oscId}/{acao}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    string msg = acao == "concluir" ? "OS concluída com sucesso!" : "OS cancelada com sucesso!";
                    MessageBox.Show(msg, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await MostrarGerenciarOscs();
                    DestaqueBotao(btnGerenciarOscs);
                }
                else if (resposta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show(acao == "concluir"
                        ? "Não é possível concluir — nem todas as assinaturas foram coletadas."
                        : "Você não tem permissão para cancelar esta OS.",
                        "Não permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro:\n{erro}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 5 — GERENCIAR USUÁRIOS (só Admin)
        // GET /usuarios → CRUD completo
        // ══════════════════════════════════════════════════════════════
        List<UsuarioResponse> todosUsuarios = new List<UsuarioResponse>();

        private async System.Threading.Tasks.Task MostrarAdmin()
        {
            LimparConteudo();

            Add(new Label
            {
                Text      = "👥  Gerenciar Usuários",
                Font      = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize  = true,
                Location  = new Point(0, 0)
            });

            var btnNovo = new Button
            {
                Text           = "➕  Novo Usuário",
                Width          = 160, Height = 36,
                Location       = new Point(_painelInterno.Width - _painelInterno.Padding.Horizontal - 170, 0),
                Anchor         = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle      = FlatStyle.Flat,
                BackColor      = Color.FromArgb(0, 135, 75),
                ForeColor      = Color.White,
                Font           = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnNovo.Click += (s, e) => AbrirFormUsuario(null);
            Add(btnNovo);

            var lblLoad = new Label { Text="⏳  Buscando usuários...", Font=new Font("Segoe UI",11), ForeColor=Color.Gray, AutoSize=true, Location=new Point(0,60) };
            Add(lblLoad);

            try
            {
                var r = await ApiConfig.Http.GetAsync($"{URL_BASE}/usuarios");
                if (!r.IsSuccessStatusCode)
                { lblLoad.Text=$"❌  Erro {(int)r.StatusCode}"; lblLoad.ForeColor=Color.Red; return; }
                var op = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                todosUsuarios = JsonSerializer.Deserialize<List<UsuarioResponse>>(
                                    await r.Content.ReadAsStringAsync(), op)
                                ?? new List<UsuarioResponse>();
            }
            catch (Exception ex) { lblLoad.Text=$"❌  {ex.Message}"; lblLoad.ForeColor=Color.Red; return; }

            _painelInterno.Controls.Remove(lblLoad);
            lblLoad.Dispose();

            // ── Barra de filtros ────────────────────────────────────────
            var painelFiltrosU = new Panel
            {
                Location  = new Point(0, 46),
                Height    = 38,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Width     = _painelInterno.Width - _painelInterno.Padding.Horizontal,
                BackColor = Color.Transparent
            };

            var txtBuscaU = new TextBox
            {
                Width           = 240,
                Height          = 28,
                Location        = new Point(0, 4),
                Font            = new Font("Segoe UI", 10),
                BackColor       = Color.FromArgb(247, 250, 255),
                BorderStyle     = BorderStyle.FixedSingle,
                PlaceholderText = "🔍  Buscar por nome ou email..."
            };
            painelFiltrosU.Controls.Add(txtBuscaU);

            var cmbPerfilU = new ComboBox
            {
                Width         = 160,
                Location      = new Point(248, 4),
                Font          = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(247, 250, 255),
                FlatStyle     = FlatStyle.Flat
            };
            cmbPerfilU.Items.AddRange(new object[] { "Todos os perfis", "Emitente", "Gerente", "Administrador" });
            cmbPerfilU.SelectedIndex = 0;
            painelFiltrosU.Controls.Add(cmbPerfilU);

            var cmbSetorU = new ComboBox
            {
                Width         = 140,
                Location      = new Point(416, 4),
                Font          = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(247, 250, 255),
                FlatStyle     = FlatStyle.Flat
            };
            // Exibe os nomes traduzidos — a lógica de filtro converte de volta para o valor da API
            cmbSetorU.Items.AddRange(new object[] { "Todos os setores", "Garantia de Validação", "Mecatrônica", "Operação", "Nenhum" });
            cmbSetorU.SelectedIndex = 0;
            painelFiltrosU.Controls.Add(cmbSetorU);

            var btnLimparU = new Button
            {
                Text           = "✖  Limpar",
                Width          = 90,
                Height         = 28,
                Location       = new Point(564, 4),
                FlatStyle      = FlatStyle.Flat,
                Font           = new Font("Segoe UI", 9),
                BackColor      = Color.FromArgb(220, 220, 225),
                ForeColor      = Color.FromArgb(60, 60, 80),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            painelFiltrosU.Controls.Add(btnLimparU);
            Add(painelFiltrosU);

            // ── Grid ─────────────────────────────────────────────────────
            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 90);
            grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width    = _painelInterno.Width - _painelInterno.Padding.Horizontal;
            grid.Height   = _painelInterno.Height - _painelInterno.Padding.Vertical - 98;

            AddCol(grid, "u_id",     "ID",     50);
            AddCol(grid, "u_nome",   "Nome",  200, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "u_email",  "Email", 220, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "u_perfil", "Perfil",130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "u_setor",  "Setor", 130, DataGridViewAutoSizeColumnMode.AllCells);

            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name="u_editar", HeaderText="", Text="✏️  Editar",
                UseColumnTextForButtonValue=true, Width=110, FlatStyle=FlatStyle.Flat,
                DefaultCellStyle={BackColor=Color.FromArgb(30,80,160),ForeColor=Color.White,Font=new Font("Segoe UI",9)}
            });
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name="u_excluir", HeaderText="", Text="🗑️  Excluir",
                UseColumnTextForButtonValue=true, Width=110, FlatStyle=FlatStyle.Flat,
                DefaultCellStyle={BackColor=Color.FromArgb(180,30,30),ForeColor=Color.White,Font=new Font("Segoe UI",9)}
            });

            // Popula a grid com todos os usuários inicialmente
            void PopularGridUsuarios(List<UsuarioResponse> lista)
            {
                grid.Rows.Clear();
                foreach (var u in lista)
                    grid.Rows.Add(u.Id, u.Nome ?? "", u.Email ?? "", u.Perfil ?? "", ExibirSetor(u.Setor));
            }

            PopularGridUsuarios(todosUsuarios);

            // Filtro em tempo real
            void FiltrarUsuarios()
            {
                string busca = txtBuscaU.Text.Trim().ToLower();
                string perfil = cmbPerfilU.SelectedIndex == 0 ? "" : (cmbPerfilU.SelectedItem?.ToString() ?? "");
                string setor  = cmbSetorU.SelectedIndex  == 0 ? "" : (cmbSetorU.SelectedItem?.ToString()  ?? "");

                var filtrados = todosUsuarios.Where(u =>
                {
                    bool passaBusca  = string.IsNullOrEmpty(busca)
                        || (u.Nome  ?? "").ToLower().Contains(busca)
                        || (u.Email ?? "").ToLower().Contains(busca);
                    bool passaPerfil = string.IsNullOrEmpty(perfil)
                        || (u.Perfil ?? "").Equals(perfil, StringComparison.OrdinalIgnoreCase);
                    // Converte o nome exibido de volta para o valor real da API antes de filtrar
                    string setorApi = setor switch
                    {
                        "Garantia de Validação" => "Qualidade",
                        "Mecatrônica"           => "Engenharia",
                        "Operação"              => "Producao",
                        var s                   => s
                    };
                    bool passaSetor  = string.IsNullOrEmpty(setor)
                        || (u.Setor ?? "").Equals(setorApi, StringComparison.OrdinalIgnoreCase);
                    return passaBusca && passaPerfil && passaSetor;
                }).ToList();

                PopularGridUsuarios(filtrados);
            }

            txtBuscaU.TextChanged             += (s, e) => FiltrarUsuarios();
            cmbPerfilU.SelectedIndexChanged   += (s, e) => FiltrarUsuarios();
            cmbSetorU.SelectedIndexChanged    += (s, e) => FiltrarUsuarios();
            btnLimparU.Click                  += (s, e) =>
            {
                txtBuscaU.Clear();
                cmbPerfilU.SelectedIndex = 0;
                cmbSetorU.SelectedIndex  = 0;
            };

            // CellFormatting: garante que os botões Editar/Excluir sempre tenham
            // a mesma cor, independente de a linha ser par ou ímpar (zebra)
            grid.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0) return;

                if (ev.ColumnIndex == grid.Columns["u_editar"].Index)
                {
                    ev.CellStyle.BackColor          = Color.FromArgb(30, 80, 160);   // azul
                    ev.CellStyle.ForeColor          = Color.White;
                    ev.CellStyle.SelectionBackColor = Color.FromArgb(20, 60, 130);   // azul escuro ao selecionar
                    ev.CellStyle.SelectionForeColor = Color.White;
                    ev.FormattingApplied = true;
                }
                else if (ev.ColumnIndex == grid.Columns["u_excluir"].Index)
                {
                    ev.CellStyle.BackColor          = Color.FromArgb(180, 30, 30);   // vermelho
                    ev.CellStyle.ForeColor          = Color.White;
                    ev.CellStyle.SelectionBackColor = Color.FromArgb(140, 20, 20);   // vermelho escuro ao selecionar
                    ev.CellStyle.SelectionForeColor = Color.White;
                    ev.FormattingApplied = true;
                }
            };

            grid.CellClick += async (s, ev) =>
            {
                if (ev.RowIndex < 0) return;
                if (!int.TryParse(grid.Rows[ev.RowIndex].Cells["u_id"].Value?.ToString(), out int id)) return;
                var usuario = todosUsuarios.Find(u => u.Id == id);
                if (usuario == null) return;

                if (ev.ColumnIndex == grid.Columns["u_editar"].Index)
                    AbrirFormUsuario(usuario);
                else if (ev.ColumnIndex == grid.Columns["u_excluir"].Index)
                {
                    var confirm = MessageBox.Show(
                        $"Excluir \"{usuario.Nome ?? "usuário"}\"?\nEsta ação não pode ser desfeita.",
                        "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes) await ExcluirUsuario(id);
                }
            };

            Add(grid);
        }

        private void AbrirFormUsuario(UsuarioResponse? usuario)
        {
            bool editando = usuario != null;
            var dialog = new Form
            {
                Text = editando ? "Editar Usuário" : "Novo Usuário",
                Size = new Size(460, 490), StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, BackColor = Color.White
            };

            int y = 20;
            var txtNome  = AdicionarCampoDialog(dialog, "Nome:",  ref y);
            var txtEmail = AdicionarCampoDialog(dialog, "Email:", ref y);
            var txtSenha = AdicionarCampoDialog(dialog, "Senha (em branco = não altera):", ref y, senha: true);

            // ComboBox Perfil
            dialog.Controls.Add(new Label { Text="Perfil:", Font=new Font("Segoe UI",10,FontStyle.Bold), ForeColor=Color.FromArgb(50,60,100), AutoSize=true, Location=new Point(20,y) });
            y += 22;
            var cmbPerfil = new ComboBox { Width=400, Location=new Point(20,y), Font=new Font("Segoe UI",10), DropDownStyle=ComboBoxStyle.DropDownList, BackColor=Color.FromArgb(247,250,255) };
            cmbPerfil.Items.AddRange(new object[] { "Emitente", "Executante", "Gerente", "Administrador" });
            cmbPerfil.SelectedIndex = 0;
            dialog.Controls.Add(cmbPerfil);
            y += 38;

            // ComboBox Setor — exibe labels traduzidos, mantém mapa para converter → valor da API
            dialog.Controls.Add(new Label { Text="Setor:", Font=new Font("Segoe UI",10,FontStyle.Bold), ForeColor=Color.FromArgb(50,60,100), AutoSize=true, Location=new Point(20,y) });
            y += 22;
            // Mapa: label exibido → valor enviado à API (não alterar os valores à direita)
            var setorOpcoes = new Dictionary<string, string>
            {
                { "Garantia de Validação", "Qualidade"  },
                { "Mecatrônica",           "Engenharia" },
                { "Operação",              "Producao"   },
                { "Nenhum",                "Nenhum"     }
            };
            var cmbSetor = new ComboBox { Width=400, Location=new Point(20,y), Font=new Font("Segoe UI",10), DropDownStyle=ComboBoxStyle.DropDownList, BackColor=Color.FromArgb(247,250,255) };
            foreach (var label in setorOpcoes.Keys) cmbSetor.Items.Add(label);
            cmbSetor.SelectedIndex = 0;
            dialog.Controls.Add(cmbSetor);
            y += 38;

            if (editando && usuario != null)
            {
                txtNome.Text  = usuario.Nome  ?? "";
                txtEmail.Text = usuario.Email ?? "";
                if (usuario.Perfil != null && cmbPerfil.Items.Contains(usuario.Perfil)) cmbPerfil.SelectedItem = usuario.Perfil;
                // Ao editar: converte valor da API → label exibido para selecionar o item correto
                if (usuario.Setor != null)
                {
                    var labelSetor = setorOpcoes.FirstOrDefault(kv =>
                        kv.Value.Equals(usuario.Setor, StringComparison.OrdinalIgnoreCase)).Key;
                    if (labelSetor != null) cmbSetor.SelectedItem = labelSetor;
                }
            }

            var btnSalvar = new Button
            {
                Text="💾  Salvar", Width=130, Height=38, Location=new Point(20,y+10),
                FlatStyle=FlatStyle.Flat, BackColor=corPrimaria, ForeColor=Color.White,
                Font=new Font("Segoe UI",10,FontStyle.Bold), Cursor=Cursors.Hand,
                FlatAppearance={BorderSize=0}
            };
            var btnCancelar = new Button
            {
                Text="Cancelar", Width=100, Height=38, Location=new Point(160,y+10),
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(220,220,225), ForeColor=Color.FromArgb(50,50,70),
                Font=new Font("Segoe UI",10), Cursor=Cursors.Hand, FlatAppearance={BorderSize=0}
            };
            btnCancelar.Click += (s, e) => dialog.Close();

            btnSalvar.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNome.Text))  { MessageBox.Show("Informe o nome.",  "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Informe o email.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!editando && string.IsNullOrWhiteSpace(txtSenha.Text)) { MessageBox.Show("Informe a senha.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                string perfilSel  = cmbPerfil.SelectedItem?.ToString() ?? "";
                // Converte o label exibido de volta para o valor da API antes de enviar
                string setorLabel = cmbSetor.SelectedItem?.ToString() ?? "";
                string setorSel   = setorOpcoes.TryGetValue(setorLabel, out var setorApi2) ? setorApi2 : setorLabel;

                btnSalvar.Enabled = false; btnSalvar.Text = "⏳  Salvando...";

                bool ok = (editando && usuario != null)
                    ? await EditarUsuario(usuario.Id, txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel)
                    : await CriarUsuario(txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel);

                if (ok) { dialog.Close(); await MostrarAdmin(); DestaqueBotao(btnAdmin); }
                else    { btnSalvar.Enabled = true; btnSalvar.Text = "💾  Salvar"; }
            };

            dialog.Controls.Add(btnSalvar);
            dialog.Controls.Add(btnCancelar);
            dialog.ShowDialog(this);
        }

        // POST /usuarios — CriarUsuarioRequest inclui adminId
        private async System.Threading.Tasks.Task<bool> CriarUsuario(
            string nome, string email, string senha, string perfil, string setor)
        {
            var payload = new
            {
                adminId = _usuarioLogado.Id,  // obrigatório para criação
                nome, email, senha, perfil, setor
            };
            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var r = await ApiConfig.Http.PostAsync($"{URL_BASE}/usuarios", conteudo);
                if (r.IsSuccessStatusCode)
                { MessageBox.Show("Usuário criado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information); return true; }
                string erro = await r.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro:\n{erro}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex) { MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
        }

        // PUT /usuarios/{id} — AtualizarUsuarioRequest (sem adminId)
        private async System.Threading.Tasks.Task<bool> EditarUsuario(
            int id, string nome, string email, string senha, string perfil, string setor)
        {
            object payload = string.IsNullOrWhiteSpace(senha)
                ? (object)new { nome, email, perfil, setor }
                : (object)new { nome, email, senha, perfil, setor };

            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var r = await ApiConfig.Http.PutAsync($"{URL_BASE}/usuarios/{id}", conteudo);
                if (r.IsSuccessStatusCode)
                { MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information); return true; }
                string erro = await r.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ao atualizar:\n{(int)r.StatusCode}\n\n{erro}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex) { MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
        }

        // DELETE /usuarios/{id}
        private async System.Threading.Tasks.Task ExcluirUsuario(int id)
        {
            try
            {
                var r = await ApiConfig.Http.DeleteAsync($"{URL_BASE}/usuarios/{id}");
                if (r.IsSuccessStatusCode)
                { MessageBox.Show("Usuário excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information); await MostrarAdmin(); DestaqueBotao(btnAdmin); }
                else
                { string erro = await r.Content.ReadAsStringAsync(); MessageBox.Show($"Erro ao excluir:\n{erro}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            catch (Exception ex) { MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // MODELOS DE DADOS — espelham o JSON da API
    // Enums chegam como string (JsonStringEnumConverter no backend)
    // ══════════════════════════════════════════════════════════════

    public class OscResponse
    {
        public int     id              { get; set; }
        public string? descricao       { get; set; }
        public string? equipamento     { get; set; }
        public string? acaoTomada      { get; set; }
        public string? dataEmissao     { get; set; }
        public string? status          { get; set; }
        public int     emitenteId      { get; set; }
        public string? emitenteNome    { get; set; }
        public string? emitenteSetor   { get; set; }
        public bool    qualidadeAssinou  { get; set; }
        public bool    engenhariaAssinou { get; set; }
        public bool    producaoAssinou   { get; set; }

        public int TotalAssinaturas =>
            (qualidadeAssinou  ? 1 : 0) +
            (engenhariaAssinou ? 1 : 0) +
            (producaoAssinou   ? 1 : 0);
    }

    public class UsuarioResponse
    {
        public int     Id     { get; set; }
        public string? Nome   { get; set; }
        public string? Email  { get; set; }
        public string? Perfil { get; set; }
        public string? Setor  { get; set; }

        public override string? ToString() => $"{Nome}  ({Email})";
    }
}