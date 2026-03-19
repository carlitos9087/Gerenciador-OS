using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Windows.Forms;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form3 : Form
    {
        // ══════════════════════════════════════════════
        // CORES DO TEMA
        // ══════════════════════════════════════════════
        readonly Color corPrimaria = Color.FromArgb(30, 80, 160);
        readonly Color corBotaoAtivo = Color.FromArgb(30, 80, 160);
        readonly Color corTextoAtivo = Color.White;
        readonly Color corTextoNormal = Color.FromArgb(60, 60, 80);

        Button? btnAtivo;

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string URL_BASE = "http://localhost:5184";

        // Usuário logado — recebido do Form1 após login
        private readonly UsuarioResponse _usuarioLogado;

        public Form3(UsuarioResponse usuarioLogado)
        {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;
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
                Text = "📋  Gerenciador de Ordens de Serviço",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 16)
            });

            panelCabecalho.Controls.Add(new Label
            {
                Text = $"👤  {_usuarioLogado.Nome}  |  {_usuarioLogado.Perfil}",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelCabecalho.Width - 380, 20)
            });

            var btnSair = new Button
            {
                Text = "⬅  Sair",
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 30, 30),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelCabecalho.Width - 110, 14),
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
        readonly Color corBotaoInativo = Color.FromArgb(80, 40, 40);
        readonly Color corTextoBotaoInativo = Color.FromArgb(180, 130, 130);

        private void ConfigurarBotoesPorPerfil()
        {
            string perfil = _usuarioLogado.Perfil ?? "";

            bool ehEmitente = perfil.Equals("Emitente", StringComparison.OrdinalIgnoreCase);
            bool ehGerente = perfil.Equals("Gerente", StringComparison.OrdinalIgnoreCase);
            bool ehAdmin = perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                            || perfil.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            // Todos os botões sempre visíveis — inativos ficam acinzentados e sem clique
            EstilizarBotao(btnDashboard, "🏠  Dashboard");
            EstilizarBotao(btnCriarRelatorio, "➕  Criar OS");
            EstilizarBotao(btnAssinar, "✅  Assinar OS");
            EstilizarBotao(btnAdmin, "👥  Gerenciar Usuários");
            EstilizarBotao(btnGerenciarOscs, "⚙️  Gerenciar OSCs");

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

        // Aplica visual cinza e tooltip explicativo — botão visível mas não funcional
        private void DesabilitarBotao(Button btn)
        {
            btn.BackColor = corBotaoInativo;
            btn.ForeColor = corTextoBotaoInativo;
            btn.Cursor = Cursors.Default;  // cursor normal, não "mãozinha"

            // Tooltip explicando o motivo
            var tip = new ToolTip();
            tip.SetToolTip(btn, "Seu perfil não tem acesso a esta função.");

            // Garante que cliques não façam nada
            btn.Click += (s, e) => { /* inativo */ };
        }

        private void EstilizarBotao(Button btn, string texto)
        {
            btn.Text = texto;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10);
            btn.ForeColor = corTextoNormal;
            btn.BackColor = Color.Transparent;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(10, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
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
        private void LimparConteudo()
        {
            foreach (Control c in panelConteudo.Controls)
                c.Dispose();
            panelConteudo.Controls.Clear();
        }

        // Cria Label + TextBox dentro do panelConteudo
        private void AdicionarCampo(string rotulo, ref int y,
                                    out TextBox caixaTexto,
                                    int altura = 32, bool multiline = false)
        {
            panelConteudo.Controls.Add(new Label
            {
                Text = rotulo,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 100),
                AutoSize = true,
                Location = new Point(0, y)
            });
            y += 22;

            caixaTexto = new TextBox
            {
                Width = 420,
                Height = altura,
                Location = new Point(0, y),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = multiline
            };
            panelConteudo.Controls.Add(caixaTexto);
            y += altura + 18;
        }

        // Cria Label + TextBox dentro de um Form de diálogo
        private TextBox AdicionarCampoDialog(Form dialog, string rotulo, ref int y, bool senha = false)
        {
            dialog.Controls.Add(new Label
            {
                Text = rotulo,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 100),
                AutoSize = true,
                Location = new Point(20, y)
            });
            y += 22;

            var txt = new TextBox
            {
                Width = 400,
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(247, 250, 255),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = senha ? '*' : '\0'
            };
            dialog.Controls.Add(txt);
            y += 38;
            return txt;
        }

        // Cor das linhas pares e ímpares — usada em todas as grids
        // Definida aqui para ser consistente em todo o sistema
        readonly Color corLinhaBase = Color.FromArgb(255, 255, 255); // branco
        readonly Color corLinhaAlternada = Color.FromArgb(241, 245, 255); // azul muito claro

        // Cria um DataGridView padrão estilizado
        // Todas as grids do sistema usam este método — garante visual consistente
        private DataGridView CriarGridBase(int alturaLinha = 40)
        {
            var grid = new DataGridView
            {
                BackgroundColor = corLinhaBase,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = alturaLinha }
            };

            // Cabeçalho
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 80, 160);  // azul primário
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight = 42;
            grid.EnableHeadersVisualStyles = false;

            // Linha base (pares)
            grid.DefaultCellStyle.BackColor = corLinhaBase;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(40, 45, 60);

            // Linha alternada (ímpares) — mesma família de cor, um tom mais escuro
            grid.AlternatingRowsDefaultCellStyle.BackColor = corLinhaAlternada;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(40, 45, 60);

            // Cor de seleção
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 80, 160);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;

            return grid;
        }

        private void AddCol(DataGridView g, string name, string header, int w,
                            DataGridViewAutoSizeColumnMode mode = DataGridViewAutoSizeColumnMode.None)
        {
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = w,
                AutoSizeMode = mode,
                DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) }
            });
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 1 — DASHBOARD
        // Endpoint varia conforme o perfil:
        //   Emitente   → GET /osc/emitente/{id}  (suas OSCs criadas)
        //   Gerente    → GET /osc/gerente/{id}   (pendentes de assinatura)
        //   Admin/Exec → GET /osc                (todas)
        // ══════════════════════════════════════════════════════════════
        const int ITENS_POR_PAGINA = 6;
        int paginaAtual = 1;
        List<OscResponse> todasAsOscs = new List<OscResponse>();

        // Define a "prioridade" de ordenação de cada status
        // Menor número = aparece primeiro
        private static int OrdemStatus(string? status)
        {
            return (status ?? "").ToLower() switch
            {
                "aguardandoassinaturas" => 0,   // mais urgente — vem primeiro
                "aguardandovalidacao" => 1,   // já assinou tudo, aguarda admin
                "concluida" => 2,   // finalizada — vai para o fim
                "cancelada" => 3,   // cancelada — última
                _ => 0
            };
        }

        private async System.Threading.Tasks.Task MostrarDashboard()
        {
            LimparConteudo();
            paginaAtual = 1;

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
                urlOsc = $"{URL_BASE}/osc/gerente/{_usuarioLogado.Id}";
                subtitulo = "OSCs pendentes de assinatura do seu setor";
            }
            else
            {
                // Administrador e Executante veem todas
                urlOsc = $"{URL_BASE}/osc";
                subtitulo = "Todas as Ordens de Serviço";
            }

            panelConteudo.Controls.Add(new Label
            {
                Text = "Dashboard — Ordens de Serviço",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            panelConteudo.Controls.Add(new Label
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
            panelConteudo.Controls.Add(lblCarregando);

            try
            {
                var resposta = await _httpClient.GetAsync(urlOsc);
                if (!resposta.IsSuccessStatusCode)
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}: {erro}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }
                string json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes)
                            ?? new List<OscResponse>();

                // Ordena: ativas primeiro (AguardandoAssinaturas → AguardandoValidacao)
                // e finalizadas (Concluida, Cancelada) por último
                todasAsOscs = lista
                    .OrderBy(o => OrdemStatus(o.status))
                    .ThenByDescending(o => o.id)   // dentro do mesmo status, mais recente primeiro
                    .ToList();
            }
            catch (Exception ex)
            {
                lblCarregando.Text = $"❌  {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }

            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            if (todasAsOscs.Count == 0)
            {
                panelConteudo.Controls.Add(new Label
                {
                    Text = "Nenhuma OS encontrada.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(0, 70)
                });
                return;
            }

            var grid = CriarGridDashboard();
            grid.Location = new Point(0, 62);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 150;
            panelConteudo.Controls.Add(grid);

            var painelPag = CriarPaginacao(grid);
            panelConteudo.Controls.Add(painelPag);

            CarregarPaginaDashboard(grid, paginaAtual);
        }

        private DataGridView CriarGridDashboard()
        {
            var grid = CriarGridBase(44);

            AddCol(grid, "d_id", "ID", 55);
            AddCol(grid, "d_desc", "Descrição", 180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "d_equip", "Equipamento", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_emit", "Emitente", 110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_data", "Data", 95, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_status", "Status", 185, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "d_assig", "Assinaturas", 90, DataGridViewAutoSizeColumnMode.AllCells);

            return grid;
        }

        private void CarregarPaginaDashboard(DataGridView grid, int pagina)
        {
            grid.Rows.Clear();
            int inicio = (pagina - 1) * ITENS_POR_PAGINA;
            int fim = Math.Min(inicio + ITENS_POR_PAGINA, todasAsOscs.Count);

            for (int i = inicio; i < fim; i++)
            {
                var osc = todasAsOscs[i];

                string data = "—";
                if (!string.IsNullOrEmpty(osc.dataEmissao) &&
                    DateTime.TryParse(osc.dataEmissao, out DateTime dt))
                    data = dt.ToString("dd/MM/yyyy");

                // Assinaturas: mostra Q/E/P com ✅ ou ⏳
                string q = osc.qualidadeAssinou ? "✅" : "⏳";
                string e = osc.engenhariaAssinou ? "✅" : "⏳";
                string p = osc.producaoAssinou ? "✅" : "⏳";
                string assig = $"{osc.TotalAssinaturas}/3  Q{q} E{e} P{p}";

                int row = grid.Rows.Add(
                    osc.id, osc.descricao, osc.equipamento,
                    osc.emitenteNome, data,
                    osc.status, assig
                );

                // Cor do status
                var celStatus = grid.Rows[row].Cells["d_status"];
                switch ((osc.status ?? "").ToLower())
                {
                    case "concluida":
                    case "concluído":
                        celStatus.Style.ForeColor = Color.FromArgb(0, 130, 70);
                        celStatus.Style.BackColor = Color.FromArgb(220, 255, 235);
                        break;
                    case "aguardandoassinaturas":
                    case "aguardandovalidacao":
                        celStatus.Style.ForeColor = Color.FromArgb(160, 90, 0);
                        celStatus.Style.BackColor = Color.FromArgb(255, 244, 205);
                        break;
                    case "cancelada":
                        celStatus.Style.ForeColor = Color.FromArgb(170, 30, 30);
                        celStatus.Style.BackColor = Color.FromArgb(255, 222, 222);
                        break;
                }

                // Cor das assinaturas
                var celAssig = grid.Rows[row].Cells["d_assig"];
                celAssig.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                if (osc.TotalAssinaturas == 3)
                { celAssig.Style.ForeColor = Color.FromArgb(0, 130, 70); celAssig.Style.BackColor = Color.FromArgb(220, 255, 235); }
                else if (osc.TotalAssinaturas > 0)
                { celAssig.Style.ForeColor = Color.FromArgb(160, 90, 0); celAssig.Style.BackColor = Color.FromArgb(255, 244, 205); }
                else
                { celAssig.Style.ForeColor = Color.FromArgb(170, 30, 30); celAssig.Style.BackColor = Color.FromArgb(255, 222, 222); }
            }
        }

        private Panel CriarPaginacao(DataGridView grid)
        {
            int total = (int)Math.Ceiling((double)todasAsOscs.Count / ITENS_POR_PAGINA);
            if (total < 1) total = 1;

            var painel = new Panel { Height = 44, Dock = DockStyle.Bottom, BackColor = Color.White };

            var btnAnt = CriarBotaoPag("◀", false);
            btnAnt.Location = new Point(0, 6);
            btnAnt.Click += (s, e) =>
            {
                if (paginaAtual > 1) { paginaAtual--; CarregarPaginaDashboard(grid, paginaAtual); AtualizarPag(painel, total); }
            };
            painel.Controls.Add(btnAnt);

            for (int p = 1; p <= total; p++)
            {
                int num = p;
                var btn = CriarBotaoPag(p.ToString(), p == paginaAtual);
                btn.Name = "pag_" + p;
                btn.Location = new Point(36 * p, 6);
                btn.Click += (s, e) => { paginaAtual = num; CarregarPaginaDashboard(grid, paginaAtual); AtualizarPag(painel, total); };
                painel.Controls.Add(btn);
            }

            var btnProx = CriarBotaoPag("▶", false);
            btnProx.Name = "btnProx";
            btnProx.Location = new Point(36 * (total + 1), 6);
            btnProx.Click += (s, e) =>
            {
                if (paginaAtual < total) { paginaAtual++; CarregarPaginaDashboard(grid, paginaAtual); AtualizarPag(painel, total); }
            };
            painel.Controls.Add(btnProx);

            return painel;
        }

        private void AtualizarPag(Panel painel, int total)
        {
            for (int p = 1; p <= total; p++)
            {
                var btn = painel.Controls["pag_" + p] as Button;
                if (btn == null) continue;
                btn.BackColor = (p == paginaAtual) ? corPrimaria : Color.White;
                btn.ForeColor = (p == paginaAtual) ? Color.White : Color.FromArgb(70, 70, 100);
                btn.Font = new Font("Segoe UI", 9, p == paginaAtual ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        private Button CriarBotaoPag(string texto, bool ativo)
        {
            return new Button
            {
                Text = texto,
                Width = 32,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, ativo ? FontStyle.Bold : FontStyle.Regular),
                BackColor = ativo ? corPrimaria : Color.White,
                ForeColor = ativo ? Color.White : Color.FromArgb(70, 70, 100),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderColor = Color.FromArgb(210, 216, 230) }
            };
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 2 — CRIAR OS
        // POST /osc com: descricao, equipamento, acaoTomada, usuarioLogadoId
        // Qualquer perfil com acesso pode criar — sem seleção de gerentes
        // ══════════════════════════════════════════════════════════════
        private void MostrarCriarOS()
        {
            LimparConteudo();

            panelConteudo.Controls.Add(new Label
            {
                Text = "Criar Nova Ordem de Serviço",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });

            int y = 50;
            AdicionarCampo("Descrição:", ref y, out TextBox txtDescricao);
            AdicionarCampo("Equipamento:", ref y, out TextBox txtEquipamento);
            AdicionarCampo("Ação Tomada:", ref y, out TextBox txtAcao, altura: 70, multiline: true);

            var btnSalvar = new Button
            {
                Text = "💾  Salvar",
                Width = 160,
                Height = 40,
                Location = new Point(0, y + 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = corPrimaria,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

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
                    descricao = txtDescricao.Text,
                    equipamento = txtEquipamento.Text,
                    acaoTomada = txtAcao.Text,
                    usuarioLogadoId = _usuarioLogado.Id
                };

                string json = JsonSerializer.Serialize(payload);
                var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

                btnSalvar.Enabled = false;
                btnSalvar.Text = "⏳  Salvando...";

                try
                {
                    var resposta = await _httpClient.PostAsync($"{URL_BASE}/osc", conteudo);
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
                    btnSalvar.Text = "💾  Salvar";
                }
            };

            panelConteudo.Controls.Add(btnSalvar);
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

            panelConteudo.Controls.Add(new Label
            {
                Text = "Assinar Ordem de Serviço",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            panelConteudo.Controls.Add(new Label
            {
                Text = $"OSCs pendentes para o setor {_usuarioLogado.Setor} — {_usuarioLogado.Nome}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 36)
            });

            var lblLoad = new Label { Text = "⏳  Buscando...", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 70) };
            panelConteudo.Controls.Add(lblLoad);

            var oscs = new List<OscResponse>();
            try
            {
                var r = await _httpClient.GetAsync($"{URL_BASE}/osc/gerente/{_usuarioLogado.Id}");
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

            panelConteudo.Controls.Remove(lblLoad);
            lblLoad.Dispose();

            if (oscs.Count == 0)
            {
                panelConteudo.Controls.Add(new Label
                {
                    Text = "✅  Nenhuma OS pendente de assinatura para o seu setor.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.FromArgb(0, 130, 70),
                    AutoSize = true,
                    Location = new Point(0, 70)
                });
                return;
            }

            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 68);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 160;

            AddCol(grid, "s_id", "ID", 55);
            AddCol(grid, "s_desc", "Descrição", 180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "s_equip", "Equipamento", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "s_emit", "Emitente", 110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "s_prog", "Progresso", 80, DataGridViewAutoSizeColumnMode.AllCells);

            var mapaOscs = new Dictionary<int, OscResponse>();

            foreach (var osc in oscs)
            {
                string prog = $"{osc.TotalAssinaturas}/3";
                int row = grid.Rows.Add(osc.id, osc.descricao, osc.equipamento, osc.emitenteNome, prog);
                mapaOscs[row] = osc;

                // Colorir progresso
                var cel = grid.Rows[row].Cells["s_prog"];
                cel.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                if (osc.TotalAssinaturas == 3)
                { cel.Style.ForeColor = Color.FromArgb(0, 130, 70); cel.Style.BackColor = Color.FromArgb(220, 255, 235); }
                else
                { cel.Style.ForeColor = Color.FromArgb(160, 90, 0); cel.Style.BackColor = Color.FromArgb(255, 244, 205); }
            }

            panelConteudo.Controls.Add(grid);

            var btnAssinarOk = new Button
            {
                Text = "✅  Assinar OS Selecionada",
                Width = 230,
                Height = 40,
                Location = new Point(0, panelConteudo.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 135, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
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
                btnAssinarOk.Text = "⏳  Assinando...";
                try
                {
                    // POST /osc/{id}/assinar — body é int puro
                    var conteudo = new StringContent(
                        _usuarioLogado.Id.ToString(), Encoding.UTF8, "application/json");

                    var resposta = await _httpClient.PostAsync(
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
                    btnAssinarOk.Text = "✅  Assinar OS Selecionada";
                }
            };

            panelConteudo.Controls.Add(btnAssinarOk);
        }

        // Verifica se o setor do gerente logado já assinou esta OS
        private bool JaAssinouNaOsc(OscResponse osc)
        {
            string setor = (_usuarioLogado.Setor ?? "").ToLower();
            return setor switch
            {
                "qualidade" => osc.qualidadeAssinou,
                "engenharia" => osc.engenhariaAssinou,
                "producao" => osc.producaoAssinou,
                _ => false
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

            panelConteudo.Controls.Add(new Label
            {
                Text = "⚙️  Gerenciar OSCs",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });
            panelConteudo.Controls.Add(new Label
            {
                Text = "OSCs ativas — Admin pode concluir (após todas assinaturas) ou cancelar qualquer OS",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 34)
            });

            var lblLoad = new Label { Text = "⏳  Buscando...", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 70) };
            panelConteudo.Controls.Add(lblLoad);

            var todasOscs = new List<OscResponse>();
            try
            {
                var r = await _httpClient.GetAsync($"{URL_BASE}/osc");
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
            catch (Exception ex) { lblLoad.Text = $"❌  {ex.Message}"; lblLoad.ForeColor = Color.Red; return; }

            panelConteudo.Controls.Remove(lblLoad);
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
                panelConteudo.Controls.Add(new Label
                {
                    Text = "Nenhuma OS ativa no momento.",
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(0, 70)
                });
                return;
            }

            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 68);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 160;

            AddCol(grid, "g_id", "ID", 55);
            AddCol(grid, "g_desc", "Descrição", 180, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "g_equip", "Equipamento", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_emit", "Emitente", 110, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_data", "Data", 95, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "g_status", "Status", 160, DataGridViewAutoSizeColumnMode.AllCells);

            // Botão Concluir
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "g_concluir",
                HeaderText = "",
                Text = "✅  Concluir",
                UseColumnTextForButtonValue = true,
                Width = 120,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(0, 135, 75), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            });

            // Botão Cancelar
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "g_cancelar",
                HeaderText = "",
                Text = "❌  Cancelar",
                UseColumnTextForButtonValue = true,
                Width = 120,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(180, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }
            });

            var mapaOscs = new Dictionary<int, OscResponse>();

            foreach (var osc in paraValidar)
            {
                string data = "—";
                if (!string.IsNullOrEmpty(osc.dataEmissao) && DateTime.TryParse(osc.dataEmissao, out DateTime dt))
                    data = dt.ToString("dd/MM/yyyy");

                int row = grid.Rows.Add(osc.id, osc.descricao, osc.equipamento,
                                        osc.emitenteNome, data, osc.status);
                mapaOscs[row] = osc;
            }

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
                            $"Não é possível concluir a OS-{osc.id:D3}." +$"Assinaturas coletadas: {osc.TotalAssinaturas}/3" +"A OS precisa de todas as 3 assinaturas antes de ser concluída.",
                            "Não permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    var confirm = MessageBox.Show(
                        $"Concluir a OS-{osc.id:D3}?{ osc.descricao}","Confirmar Conclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes)
                        await ExecutarAcaoOsc(osc.id, "concluir");
                }
                else if (ev.ColumnIndex == grid.Columns["g_cancelar"].Index)
                {
                    // Admin pode cancelar em qualquer status ativo — com ou sem assinaturas
                    string avisoAssinaturas = osc.TotalAssinaturas > 0? $"⚠️ Esta OS já tem { osc.TotalAssinaturas}/ 3 assinatura(s)."
                        : "";

                    var confirm = MessageBox.Show(
                        $"Cancelar a OS-{osc.id:D3}?Esta ação não pode ser desfeita.{ avisoAssinaturas}{ osc.descricao}","Confirmar Cancelamento", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                        await ExecutarAcaoOsc(osc.id, "cancelar");
                }
            };

            panelConteudo.Controls.Add(grid);
        }

        // Executa PUT /osc/{id}/concluir ou PUT /osc/{id}/cancelar
        private async System.Threading.Tasks.Task ExecutarAcaoOsc(int oscId, string acao)
        {
            try
            {
                // Body é int puro (o usuarioId do admin)
                var conteudo = new StringContent(
                    _usuarioLogado.Id.ToString(), Encoding.UTF8, "application/json");

                var resposta = await _httpClient.PutAsync(
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

            panelConteudo.Controls.Add(new Label
            {
                Text = "👥  Gerenciar Usuários",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });

            var btnNovo = new Button
            {
                Text = "➕  Novo Usuário",
                Width = 160,
                Height = 36,
                Location = new Point(panelConteudo.Width - 210, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 135, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnNovo.Click += (s, e) => AbrirFormUsuario(null);
            panelConteudo.Controls.Add(btnNovo);

            var lblLoad = new Label { Text = "⏳  Buscando usuários...", Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 60) };
            panelConteudo.Controls.Add(lblLoad);

            try
            {
                var r = await _httpClient.GetAsync($"{URL_BASE}/usuarios");
                if (!r.IsSuccessStatusCode)
                { lblLoad.Text = $"❌  Erro {(int)r.StatusCode}"; lblLoad.ForeColor = Color.Red; return; }
                var op = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                todosUsuarios = JsonSerializer.Deserialize<List<UsuarioResponse>>(
                                    await r.Content.ReadAsStringAsync(), op)
                                ?? new List<UsuarioResponse>();
            }
            catch (Exception ex) { lblLoad.Text = $"❌  {ex.Message}"; lblLoad.ForeColor = Color.Red; return; }

            panelConteudo.Controls.Remove(lblLoad);
            lblLoad.Dispose();

            var grid = CriarGridBase(40);
            grid.Location = new Point(0, 52);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 100;

            AddCol(grid, "u_id", "ID", 50);
            AddCol(grid, "u_nome", "Nome", 200, DataGridViewAutoSizeColumnMode.Fill);
            AddCol(grid, "u_email", "Email", 220, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "u_perfil", "Perfil", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AddCol(grid, "u_setor", "Setor", 130, DataGridViewAutoSizeColumnMode.AllCells);

            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "u_editar",
                HeaderText = "",
                Text = "✏️  Editar",
                UseColumnTextForButtonValue = true,
                Width = 110,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(30, 80, 160), ForeColor = Color.White, Font = new Font("Segoe UI", 9) }
            });
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "u_excluir",
                HeaderText = "",
                Text = "🗑️  Excluir",
                UseColumnTextForButtonValue = true,
                Width = 110,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(180, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 9) }
            });

            foreach (var u in todosUsuarios)
                grid.Rows.Add(u.Id, u.Nome, u.Email, u.Perfil, u.Setor);

            grid.CellClick += async (s, ev) =>
            {
                if (ev.RowIndex < 0) return;
                int id = Convert.ToInt32(grid.Rows[ev.RowIndex].Cells["u_id"].Value);
                var usuario = todosUsuarios.Find(u => u.Id == id);
                if (usuario == null) return;

                if (ev.ColumnIndex == grid.Columns["u_editar"].Index)
                    AbrirFormUsuario(usuario);
                else if (ev.ColumnIndex == grid.Columns["u_excluir"].Index)
                {
                    var confirm = MessageBox.Show(
                        $"Excluir \"{usuario.Nome}\"?\nEsta ação não pode ser desfeita.",
                        "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes) await ExcluirUsuario(id);
                }
            };

            panelConteudo.Controls.Add(grid);
        }

        private void AbrirFormUsuario(UsuarioResponse? usuario)
        {
            bool editando = usuario != null;
            var dialog = new Form
            {
                Text = editando ? "Editar Usuário" : "Novo Usuário",
                Size = new Size(460, 490),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            int y = 20;
            var txtNome = AdicionarCampoDialog(dialog, "Nome:", ref y);
            var txtEmail = AdicionarCampoDialog(dialog, "Email:", ref y);
            var txtSenha = AdicionarCampoDialog(dialog, "Senha (em branco = não altera):", ref y, senha: true);

            // ComboBox Perfil
            dialog.Controls.Add(new Label { Text = "Perfil:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(50, 60, 100), AutoSize = true, Location = new Point(20, y) });
            y += 22;
            var cmbPerfil = new ComboBox { Width = 400, Location = new Point(20, y), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 250, 255) };
            cmbPerfil.Items.AddRange(new object[] { "Emitente", "Executante", "Gerente", "Administrador" });
            cmbPerfil.SelectedIndex = 0;
            dialog.Controls.Add(cmbPerfil);
            y += 38;

            // ComboBox Setor
            dialog.Controls.Add(new Label { Text = "Setor:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(50, 60, 100), AutoSize = true, Location = new Point(20, y) });
            y += 22;
            var cmbSetor = new ComboBox { Width = 400, Location = new Point(20, y), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 250, 255) };
            cmbSetor.Items.AddRange(new object[] { "Qualidade", "Engenharia", "Producao", "Nenhum" });
            cmbSetor.SelectedIndex = 0;
            dialog.Controls.Add(cmbSetor);
            y += 38;

            if (editando)
            {
                txtNome.Text = usuario!.Nome;
                txtEmail.Text = usuario.Email;
                if (usuario.Perfil != null && cmbPerfil.Items.Contains(usuario.Perfil)) cmbPerfil.SelectedItem = usuario.Perfil;
                if (usuario.Setor != null && cmbSetor.Items.Contains(usuario.Setor)) cmbSetor.SelectedItem = usuario.Setor;
            }

            var btnSalvar = new Button
            {
                Text = "💾  Salvar",
                Width = 130,
                Height = 38,
                Location = new Point(20, y + 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = corPrimaria,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 100,
                Height = 38,
                Location = new Point(160, y + 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(50, 50, 70),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnCancelar.Click += (s, e) => dialog.Close();

            btnSalvar.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Informe o nome.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtEmail.Text)) { MessageBox.Show("Informe o email.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!editando && string.IsNullOrWhiteSpace(txtSenha.Text)) { MessageBox.Show("Informe a senha.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                string perfilSel = cmbPerfil.SelectedItem?.ToString() ?? "";
                string setorSel = cmbSetor.SelectedItem?.ToString() ?? "";

                btnSalvar.Enabled = false; btnSalvar.Text = "⏳  Salvando...";

                bool ok = editando
                    ? await EditarUsuario(usuario!.Id, txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel)
                    : await CriarUsuario(txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel);

                if (ok) { dialog.Close(); await MostrarAdmin(); DestaqueBotao(btnAdmin); }
                else { btnSalvar.Enabled = true; btnSalvar.Text = "💾  Salvar"; }
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
                nome,
                email,
                senha,
                perfil,
                setor
            };
            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var r = await _httpClient.PostAsync($"{URL_BASE}/usuarios", conteudo);
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
                var r = await _httpClient.PutAsync($"{URL_BASE}/usuarios/{id}", conteudo);
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
                var r = await _httpClient.DeleteAsync($"{URL_BASE}/usuarios/{id}");
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
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? equipamento { get; set; }
        public string? acaoTomada { get; set; }
        public string? dataEmissao { get; set; }
        public string? status { get; set; }
        public int emitenteId { get; set; }
        public string? emitenteNome { get; set; }
        public string? emitenteSetor { get; set; }
        public bool qualidadeAssinou { get; set; }
        public bool engenhariaAssinou { get; set; }
        public bool producaoAssinou { get; set; }

        public int TotalAssinaturas =>
            (qualidadeAssinou ? 1 : 0) +
            (engenhariaAssinou ? 1 : 0) +
            (producaoAssinou ? 1 : 0);
    }

    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Perfil { get; set; }
        public string? Setor { get; set; }

        public override string? ToString() => $"{Nome}  ({Email})";
    }
}