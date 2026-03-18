using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        // HttpClient estático — reutilizado em toda a aplicação
        private static readonly HttpClient _httpClient = new HttpClient();

        // URL base da API — mude aqui se o endereço mudar
        private const string URL_BASE = "http://localhost:5184";

        // Usuário que fez login (recebido do Form1)
        private UsuarioResponse _usuarioLogado;

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
            ConfigurarBotoes();
            // _ = descarta o Task retornado — suprimi o warning CS4014 corretamente
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

            // Nome e perfil do usuário logado
            panelCabecalho.Controls.Add(new Label
            {
                Text = $"👤  {_usuarioLogado.Nome}  |  {_usuarioLogado.Perfil}",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelCabecalho.Width - 380, 20)
            });

            // Botão Sair
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
                var confirmar = MessageBox.Show(
                    "Deseja sair da conta?",
                    "Confirmar saída",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar == DialogResult.Yes)
                {
                    // Mostra o Form1 que estava oculto e fecha este
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f is Form1) { f.Show(); break; }
                    }
                    this.Close();
                }
            };

            panelCabecalho.Controls.Add(btnSair);
        }

        // ══════════════════════════════════════════════
        // BOTÕES DA LATERAL
        // ══════════════════════════════════════════════
        private void ConfigurarBotoes()
        {
            EstilizarBotao(btnDashboard, "🏠  Dashboard");
            EstilizarBotao(btnCriarRelatorio, "➕  Criar OS");
            EstilizarBotao(btnAssinar, "✅  Assinar OS");
            EstilizarBotao(btnAdmin, "⚙️  Administração");

            // btnAdmin só aparece para Administrador
            bool ehAdmin = (_usuarioLogado.Perfil ?? "").Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                        || (_usuarioLogado.Perfil ?? "").Equals("Admin", StringComparison.OrdinalIgnoreCase);
            btnAdmin.Visible = ehAdmin;

            btnDashboard.Click += async (s, e) => { await MostrarDashboard(); DestaqueBotao(btnDashboard); };
            btnCriarRelatorio.Click += (s, e) => { MostrarCriarOS(); DestaqueBotao(btnCriarRelatorio); };
            btnAssinar.Click += async (s, e) => { await MostrarAssinar(); DestaqueBotao(btnAssinar); };
            btnAdmin.Click += async (s, e) => { await MostrarAdmin(); DestaqueBotao(btnAdmin); };
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
            Button[] todos = { btnDashboard, btnCriarRelatorio, btnAssinar, btnAdmin };
            foreach (var btn in todos)
            {
                if (btn == botaoClicado)
                {
                    btn.BackColor = corBotaoAtivo;
                    btn.ForeColor = corTextoAtivo;
                }
                else
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = corTextoNormal;
                }
            }
            btnAtivo = botaoClicado;
        }

        // ══════════════════════════════════════════════
        // UTILITÁRIO: limpa o panelConteudo
        // ══════════════════════════════════════════════
        private void LimparConteudo()
        {
            foreach (Control c in panelConteudo.Controls)
                c.Dispose();
            panelConteudo.Controls.Clear();
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 1 — DASHBOARD
        // GET /osc → deserializa → tabela com paginação
        // ══════════════════════════════════════════════════════════════
        const int ITENS_POR_PAGINA = 5;
        int paginaAtual = 1;
        List<OscResponse> todasAsOscs = new List<OscResponse>();

        private async System.Threading.Tasks.Task MostrarDashboard()
        {
            LimparConteudo();
            paginaAtual = 1;

            panelConteudo.Controls.Add(new Label
            {
                Text = "Dashboard — Ordens de Serviço",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });

            var lblCarregando = new Label
            {
                Text = "⏳  Buscando dados da API...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 60)
            };
            panelConteudo.Controls.Add(lblCarregando);

            try
            {
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/osc");

                if (!resposta.IsSuccessStatusCode)
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}: {erro}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }

                string json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                todasAsOscs = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes) ?? new List<OscResponse>();
            }
            catch (HttpRequestException ex)
            {
                lblCarregando.Text = $"❌  Sem conexão com a API: {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }
            catch (Exception ex)
            {
                lblCarregando.Text = $"❌  Erro inesperado: {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }

            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            var grid = CriarGridOsc();
            grid.Location = new Point(0, 52);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 140;
            panelConteudo.Controls.Add(grid);

            var painelPag = CriarPaginacao(grid);
            panelConteudo.Controls.Add(painelPag);

            CarregarPagina(grid, paginaAtual);
        }

        private DataGridView CriarGridOsc()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                // Altura maior para acomodar as 3 linhas de assinatura
                RowTemplate = { Height = 52 }
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 241, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(40, 55, 100);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight = 42;
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);

            AdicionarColuna(grid, "ID", "id", 55, DataGridViewAutoSizeColumnMode.None);
            AdicionarColuna(grid, "Descrição", "descricao", 160, DataGridViewAutoSizeColumnMode.Fill);
            AdicionarColuna(grid, "Equipamento", "equipamento", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Emitente", "emitente", 110, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Data", "data", 90, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Status", "status", 150, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Assinaturas", "assinaturas", 90, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Qualidade", "qualidade", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Engenharia", "engenharia", 130, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Produção", "producao", 130, DataGridViewAutoSizeColumnMode.AllCells);

            return grid;
        }

        private void AdicionarColuna(DataGridView grid, string titulo, string nome,
                                     int largura, DataGridViewAutoSizeColumnMode modo)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = titulo,
                Name = nome,
                Width = largura,
                AutoSizeMode = modo,
                DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) }
            });
        }

        private void CarregarPagina(DataGridView grid, int pagina)
        {
            grid.Rows.Clear();

            int inicio = (pagina - 1) * ITENS_POR_PAGINA;
            int fim = Math.Min(inicio + ITENS_POR_PAGINA, todasAsOscs.Count);

            for (int i = inicio; i < fim; i++)
            {
                var osc = todasAsOscs[i];

                // Formata a data (vem como "2026-03-14T00:49:04Z", mostra só "14/03/2026")
                string dataFormatada = "—";
                if (!string.IsNullOrEmpty(osc.dataEmissao) &&
                    DateTime.TryParse(osc.dataEmissao, out DateTime dt))
                    dataFormatada = dt.ToString("dd/MM/yyyy");

                // Indicador de assinaturas: "2/3" com emoji de progresso
                string progresso = $"{osc.TotalAssinaturas}/3";

                // Cada gerente: mostra "✅ Nome" se assinou ou "⏳ Nome" se pendente
                string qualidade = $"{(osc.qualidadeAssinou ? "✅" : "⏳")} {osc.gerenteQualidade?.nome ?? "—"}";
                string engenharia = $"{(osc.engenhariaAssinou ? "✅" : "⏳")} {osc.gerenteEngenharia?.nome ?? "—"}";
                string producao = $"{(osc.producaoAssinou ? "✅" : "⏳")} {osc.gerenteProducao?.nome ?? "—"}";

                int linha = grid.Rows.Add(
                    osc.id,
                    osc.descricao,
                    osc.equipamento,
                    osc.emitenteNome ?? "—",
                    dataFormatada,
                    osc.status ?? "—",
                    progresso,
                    qualidade,
                    engenharia,
                    producao
                );

                // ── Cor da célula Status conforme o valor ──────────────
                var celulaStatus = grid.Rows[linha].Cells["status"];
                switch ((osc.status ?? "").ToLower())
                {
                    case "aprovado":
                    case "concluido":
                    case "concluído":
                        celulaStatus.Style.ForeColor = Color.FromArgb(0, 130, 70);
                        celulaStatus.Style.BackColor = Color.FromArgb(220, 255, 235);
                        break;
                    case "aguardandoassinaturas":
                    case "pendente":
                    case "em andamento":
                        celulaStatus.Style.ForeColor = Color.FromArgb(160, 90, 0);
                        celulaStatus.Style.BackColor = Color.FromArgb(255, 244, 205);
                        break;
                    case "rejeitado":
                    case "cancelado":
                        celulaStatus.Style.ForeColor = Color.FromArgb(170, 30, 30);
                        celulaStatus.Style.BackColor = Color.FromArgb(255, 222, 222);
                        break;
                }

                // ── Cor da célula Assinaturas conforme progresso ───────
                var celulaAssign = grid.Rows[linha].Cells["assinaturas"];
                celulaAssign.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                if (osc.TotalAssinaturas == 3)
                {
                    celulaAssign.Style.ForeColor = Color.FromArgb(0, 130, 70);
                    celulaAssign.Style.BackColor = Color.FromArgb(220, 255, 235);
                }
                else if (osc.TotalAssinaturas > 0)
                {
                    celulaAssign.Style.ForeColor = Color.FromArgb(160, 90, 0);
                    celulaAssign.Style.BackColor = Color.FromArgb(255, 244, 205);
                }
                else
                {
                    celulaAssign.Style.ForeColor = Color.FromArgb(170, 30, 30);
                    celulaAssign.Style.BackColor = Color.FromArgb(255, 222, 222);
                }
            }
        }

        private Panel CriarPaginacao(DataGridView grid)
        {
            int totalPaginas = (int)Math.Ceiling((double)todasAsOscs.Count / ITENS_POR_PAGINA);
            if (totalPaginas < 1) totalPaginas = 1;

            var painelPag = new Panel { Height = 44, Dock = DockStyle.Bottom, BackColor = Color.White };

            var btnAnterior = CriarBotaoPagina("◀", false);
            btnAnterior.Location = new Point(0, 6);
            btnAnterior.Click += (s, e) =>
            {
                if (paginaAtual > 1)
                {
                    paginaAtual--;
                    CarregarPagina(grid, paginaAtual);
                    AtualizarBotoesPagina(painelPag, totalPaginas);
                }
            };
            painelPag.Controls.Add(btnAnterior);

            for (int p = 1; p <= totalPaginas; p++)
            {
                int numPagina = p;
                var btn = CriarBotaoPagina(p.ToString(), p == paginaAtual);
                btn.Name = "pag_" + p;
                btn.Location = new Point(36 * p, 6);
                btn.Click += (s, e) =>
                {
                    paginaAtual = numPagina;
                    CarregarPagina(grid, paginaAtual);
                    AtualizarBotoesPagina(painelPag, totalPaginas);
                };
                painelPag.Controls.Add(btn);
            }

            var btnProximo = CriarBotaoPagina("▶", false);
            btnProximo.Name = "btnProximo";
            btnProximo.Location = new Point(36 * (totalPaginas + 1), 6);
            btnProximo.Click += (s, e) =>
            {
                if (paginaAtual < totalPaginas)
                {
                    paginaAtual++;
                    CarregarPagina(grid, paginaAtual);
                    AtualizarBotoesPagina(painelPag, totalPaginas);
                }
            };
            painelPag.Controls.Add(btnProximo);

            return painelPag;
        }

        private void AtualizarBotoesPagina(Panel painel, int totalPaginas)
        {
            for (int p = 1; p <= totalPaginas; p++)
            {
                var btn = painel.Controls["pag_" + p] as Button;
                if (btn == null) continue;
                btn.BackColor = (p == paginaAtual) ? corPrimaria : Color.White;
                btn.ForeColor = (p == paginaAtual) ? Color.White : Color.FromArgb(70, 70, 100);
                btn.Font = new Font("Segoe UI", 9, p == paginaAtual ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        private Button CriarBotaoPagina(string texto, bool ativo)
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
        // GET gerentes por setor em paralelo → ComboBoxes → POST /osc
        // ══════════════════════════════════════════════════════════════

        // Busca gerentes de um setor na API
        // OBS: rota sem /auth porque [HttpGet("/usuarios/gerentes/{setor}")] é absoluta
        private async System.Threading.Tasks.Task<List<UsuarioResponse>> BuscarGerentesPorSetor(string setor)
        {
            try
            {
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/usuarios/gerentes/{setor}");
                if (!resposta.IsSuccessStatusCode)
                    return new List<UsuarioResponse>();

                string json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<UsuarioResponse>>(json, opcoes)
                       ?? new List<UsuarioResponse>();
            }
            catch
            {
                return new List<UsuarioResponse>();
            }
        }

        private ComboBox AdicionarComboBox(string rotulo, ref int y,
                                           List<UsuarioResponse> itens, string msgErro)
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

            var combo = new ComboBox
            {
                Width = 420,
                Location = new Point(0, y),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(247, 250, 255),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };

            if (itens.Count == 0)
            {
                combo.Items.Add(msgErro);
                combo.SelectedIndex = 0;
                combo.Enabled = false;
            }
            else
            {
                combo.DisplayMember = "Nome";
                combo.ValueMember = "Id";
                foreach (var u in itens)
                    combo.Items.Add(u);
                combo.SelectedIndex = 0;
            }

            panelConteudo.Controls.Add(combo);
            y += 34 + 18;
            return combo;
        }

        private async void MostrarCriarOS()
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

            var lblCarregando = new Label
            {
                Text = "⏳  Carregando gerentes...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 46)
            };
            panelConteudo.Controls.Add(lblCarregando);

            // Dispara os 3 GETs em paralelo — mais rápido do que sequencial
            var taskQualidade = BuscarGerentesPorSetor("Qualidade");
            var taskEngenharia = BuscarGerentesPorSetor("Engenharia");
            var taskProducao = BuscarGerentesPorSetor("Producao");

            await System.Threading.Tasks.Task.WhenAll(taskQualidade, taskEngenharia, taskProducao);

            List<UsuarioResponse> gerentesQualidade = taskQualidade.Result ?? new List<UsuarioResponse>();
            List<UsuarioResponse> gerentesEngenharia = taskEngenharia.Result ?? new List<UsuarioResponse>();
            List<UsuarioResponse> gerentesProducao = taskProducao.Result ?? new List<UsuarioResponse>();

            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            int y = 46;

            AdicionarCampo("Descrição:", ref y, out TextBox txtDescricao);
            AdicionarCampo("Equipamento:", ref y, out TextBox txtEquipamento);

            var cmbQualidade = AdicionarComboBox("Gerente de Qualidade:", ref y, gerentesQualidade, "❌ Nenhum gerente encontrado");
            var cmbEngenharia = AdicionarComboBox("Gerente de Engenharia:", ref y, gerentesEngenharia, "❌ Nenhum gerente encontrado");
            var cmbProducao = AdicionarComboBox("Gerente de Produção:", ref y, gerentesProducao, "❌ Nenhum gerente encontrado");

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
                {
                    MessageBox.Show("Informe a descrição!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEquipamento.Text))
                {
                    MessageBox.Show("Informe o equipamento!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!cmbQualidade.Enabled || !cmbEngenharia.Enabled || !cmbProducao.Enabled)
                {
                    MessageBox.Show("Não foi possível carregar todos os gerentes. Tente novamente.",
                        "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var gerenteQualidade = (UsuarioResponse)cmbQualidade.SelectedItem;
                var gerenteEngenharia = (UsuarioResponse)cmbEngenharia.SelectedItem;
                var gerenteProducao = (UsuarioResponse)cmbProducao.SelectedItem;

                var payload = new
                {
                    descricao = txtDescricao.Text,
                    equipamento = txtEquipamento.Text,
                    gerenteQualidadeId = gerenteQualidade.Id,
                    gerenteEngenhariaId = gerenteEngenharia.Id,
                    gerenteProducaoId = gerenteProducao.Id,
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
                        MessageBox.Show("Ordem de Serviço criada com sucesso!", "Sucesso",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await MostrarDashboard();
                        DestaqueBotao(btnDashboard);
                    }
                    else
                    {
                        string corpoErro = await resposta.Content.ReadAsStringAsync();
                        MessageBox.Show($"Erro da API:\nStatus: {(int)resposta.StatusCode}\n\n{corpoErro}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Sem conexão com a API.\nVerifique se está rodando em {URL_BASE}\n\nDetalhe: {ex.Message}",
                        "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro inesperado:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSalvar.Enabled = true;
                    btnSalvar.Text = "💾  Salvar";
                }
            };

            panelConteudo.Controls.Add(btnSalvar);
        }

        // Cria Label + TextBox empilhados e avança "y" automaticamente
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

        // ══════════════════════════════════════════════════════════════
        // TELA 3 — ASSINAR OS
        // GET /osc → filtra pendentes → ListBox → confirmação
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
                Text = "Selecione uma OS pendente e clique em Assinar",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 36)
            });

            var lblCarregando = new Label
            {
                Text = "⏳  Buscando OSCs pendentes...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 72)
            };
            panelConteudo.Controls.Add(lblCarregando);

            var pendentes = new List<OscResponse>();
            try
            {
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/osc");

                if (resposta.IsSuccessStatusCode)
                {
                    string json = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var todas = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes)
                                  ?? new List<OscResponse>();

                    foreach (var osc in todas)
                    {
                        string status = (osc.status ?? "").ToLower().Trim();

                        // Inclui OSCs que ainda aguardam assinaturas
                        // "aguardandoassinaturas" é o status real retornado pela API
                        bool aguardando = status == "aguardandoassinaturas"
                                       || status == "pendente"
                                       || status == "em andamento"
                                       || status == "";

                        if (aguardando)
                            pendentes.Add(osc);
                    }
                }
                else
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}: {erro}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }
            }
            catch (Exception ex)
            {
                lblCarregando.Text = $"❌  Erro ao buscar: {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }

            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            var lista = new ListBox
            {
                Location = new Point(0, 68),
                Width = 560,
                Height = 220,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 255)
            };

            // Dicionário texto → id para recuperar o ID ao clicar
            var mapaItens = new Dictionary<string, int>();

            if (pendentes.Count == 0)
            {
                lista.Items.Add("Nenhuma OS pendente encontrada.");
            }
            else
            {
                foreach (var osc in pendentes)
                {
                    // Mostra progresso de assinaturas e quais gerentes ainda precisam assinar
                    string q = osc.qualidadeAssinou ? "✅" : "⏳";
                    string e = osc.engenhariaAssinou ? "✅" : "⏳";
                    string p = osc.producaoAssinou ? "✅" : "⏳";
                    string texto = $"[OS-{osc.id:D3}]  {osc.descricao}  |  {osc.TotalAssinaturas}/3  Q:{q} E:{e} P:{p}";
                    lista.Items.Add(texto);
                    mapaItens[texto] = osc.id;
                }
            }

            panelConteudo.Controls.Add(lista);

            var btnAssinarOk = new Button
            {
                Text = "✅  Assinar Selecionada",
                Width = 210,
                Height = 40,
                Location = new Point(0, 302),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 135, 75),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

            btnAssinarOk.Click += async (s, e) =>
            {
                if (lista.SelectedItem == null || pendentes.Count == 0)
                {
                    MessageBox.Show("Selecione uma OS para assinar.", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string itemSelecionado = lista.SelectedItem?.ToString() ?? "";

                var confirmacao = MessageBox.Show(
                    $"Confirma a assinatura?\n\n{itemSelecionado}",
                    "Confirmar Assinatura", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmacao != DialogResult.Yes) return;

                if (string.IsNullOrEmpty(itemSelecionado)) return;
                int idOsc = mapaItens[itemSelecionado];
                btnAssinarOk.Enabled = false;
                btnAssinarOk.Text = "⏳  Assinando...";

                try
                {
                    // TODO: substitua pela chamada real de assinatura quando tiver o endpoint
                    // Exemplo: await _httpClient.PutAsync($"{URL_BASE}/osc/{idOsc}/assinar", null);
                    MessageBox.Show($"OS-{idOsc:D3} assinada com sucesso!", "Sucesso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await MostrarDashboard();
                    DestaqueBotao(btnDashboard);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao assinar: {ex.Message}", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnAssinarOk.Enabled = true;
                    btnAssinarOk.Text = "✅  Assinar Selecionada";
                }
            };

            panelConteudo.Controls.Add(btnAssinarOk);
        }

        // ══════════════════════════════════════════════════════════════
        // TELA 4 — ADMINISTRAÇÃO DE USUÁRIOS
        // Só acessível por Administrador
        // GET /usuarios → tabela com botões Editar / Excluir
        // ══════════════════════════════════════════════════════════════
        List<UsuarioResponse> todosUsuarios = new List<UsuarioResponse>();

        private async System.Threading.Tasks.Task MostrarAdmin()
        {
            LimparConteudo();

            panelConteudo.Controls.Add(new Label
            {
                Text = "⚙️  Administração de Usuários",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 35, 70),
                AutoSize = true,
                Location = new Point(0, 0)
            });

            // Botão "Novo Usuário" no canto superior direito
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
            btnNovo.Click += (s, e) => AbrirFormularioUsuario(null);
            panelConteudo.Controls.Add(btnNovo);

            var lblCarregando = new Label
            {
                Text = "⏳  Buscando usuários...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 60)
            };
            panelConteudo.Controls.Add(lblCarregando);

            // GET /usuarios
            try
            {
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/usuarios");

                if (!resposta.IsSuccessStatusCode)
                {
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }

                string json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                todosUsuarios = JsonSerializer.Deserialize<List<UsuarioResponse>>(json, opcoes)
                                ?? new List<UsuarioResponse>();
            }
            catch (Exception ex)
            {
                lblCarregando.Text = $"❌  Erro: {ex.Message}";
                lblCarregando.ForeColor = Color.Red;
                return;
            }

            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            var grid = CriarGridUsuarios();
            grid.Location = new Point(0, 52);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 100;

            foreach (var u in todosUsuarios)
                grid.Rows.Add(u.Id, u.Nome, u.Email, u.Perfil, u.Setor);

            // Clique nas colunas de ação (Editar / Excluir)
            grid.CellClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;

                int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["col_id"].Value);
                var usuario = todosUsuarios.Find(u => u.Id == id);
                if (usuario == null) return;

                if (e.ColumnIndex == grid.Columns["col_editar"].Index)
                {
                    AbrirFormularioUsuario(usuario);
                }
                else if (e.ColumnIndex == grid.Columns["col_excluir"].Index)
                {
                    var confirmar = MessageBox.Show(
                        $"Excluir o usuário \"{usuario.Nome}\"?\nEsta ação não pode ser desfeita.",
                        "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (confirmar == DialogResult.Yes)
                        await ExcluirUsuario(id);
                }
            };

            panelConteudo.Controls.Add(grid);
        }

        private DataGridView CriarGridUsuarios()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                RowTemplate = { Height = 40 }
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 241, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(40, 55, 100);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight = 42;
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col_id", HeaderText = "ID", Width = 50, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col_nome", HeaderText = "Nome", Width = 200, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col_email", HeaderText = "Email", Width = 220, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col_perfil", HeaderText = "Perfil", Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col_setor", HeaderText = "Setor", Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) } });

            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "col_editar",
                HeaderText = "",
                Text = "✏️  Editar",
                UseColumnTextForButtonValue = true,
                Width = 110,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(30, 80, 160), ForeColor = Color.White, Font = new Font("Segoe UI", 9) }
            });

            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "col_excluir",
                HeaderText = "",
                Text = "🗑️  Excluir",
                UseColumnTextForButtonValue = true,
                Width = 110,
                FlatStyle = FlatStyle.Flat,
                DefaultCellStyle = { BackColor = Color.FromArgb(180, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 9) }
            });

            return grid;
        }

        // Formulário de criar/editar usuário em janela secundária
        // usuario == null → CRIAR    |    usuario != null → EDITAR
        private void AbrirFormularioUsuario(UsuarioResponse? usuario)
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
            var txtSenha = AdicionarCampoDialog(dialog, "Senha (deixe em branco para não alterar):", ref y, senha: true);

            // ComboBox Perfil
            dialog.Controls.Add(new Label { Text = "Perfil:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(50, 60, 100), AutoSize = true, Location = new Point(20, y) });
            y += 22;
            var cmbPerfil = new ComboBox { Width = 400, Location = new Point(20, y), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 250, 255) };
            // Ajuste esses valores para bater com o enum Perfil do seu backend
            cmbPerfil.Items.AddRange(new object[] { "Administrador", "Gerente", "Emitente" });
            cmbPerfil.SelectedIndex = 0;
            dialog.Controls.Add(cmbPerfil);
            y += 38;

            // ComboBox Setor
            dialog.Controls.Add(new Label { Text = "Setor:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(50, 60, 100), AutoSize = true, Location = new Point(20, y) });
            y += 22;
            var cmbSetor = new ComboBox { Width = 400, Location = new Point(20, y), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 250, 255) };
            // Ajuste esses valores para bater com o enum Setor do seu backend
            cmbSetor.Items.AddRange(new object[] { "Qualidade", "Engenharia", "Producao", "Administrativo" });
            cmbSetor.SelectedIndex = 0;
            dialog.Controls.Add(cmbSetor);
            y += 38;

            // Preenche os campos se for edição
            if (editando)
            {
                txtNome.Text = usuario.Nome;
                txtEmail.Text = usuario.Email;

                // Perfil e Setor já são string — seleciona o item correspondente no ComboBox
                if (usuario.Perfil != null && cmbPerfil.Items.Contains(usuario.Perfil))
                    cmbPerfil.SelectedItem = usuario.Perfil;

                if (usuario.Setor != null && cmbSetor.Items.Contains(usuario.Setor))
                    cmbSetor.SelectedItem = usuario.Setor;
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
                if (string.IsNullOrWhiteSpace(txtNome.Text))
                { MessageBox.Show("Informe o nome.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                { MessageBox.Show("Informe o email.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!editando && string.IsNullOrWhiteSpace(txtSenha.Text))
                { MessageBox.Show("Informe a senha para novo usuário.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                btnSalvar.Enabled = false;
                btnSalvar.Text = "⏳  Salvando...";

                string perfilSel = cmbPerfil.SelectedItem?.ToString() ?? "";
                string setorSel = cmbSetor.SelectedItem?.ToString() ?? "";

                bool sucesso = editando
                    ? await EditarUsuario(usuario.Id, txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel)
                    : await CriarUsuario(txtNome.Text, txtEmail.Text, txtSenha.Text, perfilSel, setorSel);

                if (sucesso)
                {
                    dialog.Close();
                    await MostrarAdmin();
                    DestaqueBotao(btnAdmin);
                }
                else
                {
                    btnSalvar.Enabled = true;
                    btnSalvar.Text = "💾  Salvar";
                }
            };

            dialog.Controls.Add(btnSalvar);
            dialog.Controls.Add(btnCancelar);
            dialog.ShowDialog(this);
        }

        // Cria Label + TextBox dentro da janela de diálogo
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

        // POST /usuarios — cria novo usuário
        private async System.Threading.Tasks.Task<bool> CriarUsuario(
            string nome, string email, string senha, string perfil, string setor)
        {
            var payload = new { nome, email, senha, perfil, setor };
            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resposta = await _httpClient.PostAsync($"{URL_BASE}/usuarios", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Usuário criado com sucesso!", "Sucesso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                string erro = await resposta.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ao criar usuário:\n{erro}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // PUT /usuarios/{id} — edita usuário existente
        // Não inclui a senha no payload se o campo estiver vazio
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
                var resposta = await _httpClient.PutAsync($"{URL_BASE}/usuarios/{id}", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }

                string erro = await resposta.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ao atualizar usuário:\nStatus: {(int)resposta.StatusCode}\n\n{erro}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // DELETE /usuarios/{id} — exclui usuário
        private async System.Threading.Tasks.Task ExcluirUsuario(int id)
        {
            try
            {
                var resposta = await _httpClient.DeleteAsync($"{URL_BASE}/usuarios/{id}");

                if (resposta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Usuário excluído com sucesso!", "Sucesso",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await MostrarAdmin();
                    DestaqueBotao(btnAdmin);
                }
                else
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro ao excluir:\n{erro}", "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // MODELOS DE DADOS
    // Espelham o JSON retornado pela API
    // ══════════════════════════════════════════════════════════════

    // Modelo que espelha exatamente o JSON retornado por GET /osc e GET /osc/{id}
    public class OscResponse
    {
        public int id { get; set; }
        public string? descricao { get; set; }
        public string? equipamento { get; set; }
        public string? acaoTomada { get; set; }
        public string? dataEmissao { get; set; }
        public string? status { get; set; }

        // Dados do emitente
        public int emitenteId { get; set; }
        public string? emitenteNome { get; set; }
        public string? emitenteSetor { get; set; }

        // Objetos aninhados com nome e dados dos gerentes
        public GerenteInfo? gerenteQualidade { get; set; }
        public GerenteInfo? gerenteEngenharia { get; set; }
        public GerenteInfo? gerenteProducao { get; set; }

        // Status de assinatura de cada gerente
        public bool qualidadeAssinou { get; set; }
        public bool engenhariaAssinou { get; set; }
        public bool producaoAssinou { get; set; }

        // Quantas assinaturas já foram feitas (calculado no cliente)
        public int TotalAssinaturas =>
            (qualidadeAssinou ? 1 : 0) +
            (engenhariaAssinou ? 1 : 0) +
            (producaoAssinou ? 1 : 0);
    }

    // Representa o objeto de gerente aninhado no JSON da OS
    public class GerenteInfo
    {
        public int id { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? perfil { get; set; }
        public string? setor { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // JsonConverter que aceita tanto string quanto int para Perfil/Setor
    //
    // O endpoint /auth/login retorna: "perfil": "Administrador" (string)
    // O endpoint /usuarios     retorna: "perfil": 1             (int)
    //
    // Este converter lida com os dois casos automaticamente.
    // ══════════════════════════════════════════════════════════════
    public class StringOrIntConverter : System.Text.Json.Serialization.JsonConverter<string>
    {
        // Mapas de int → string para cada enum do backend
        // IMPORTANTE: ajuste a ordem conforme seus enums no backend (conta de 0)
        private static readonly string[] _perfis = { "Administrador", "Gerente", "Emitente" };
        private static readonly string[] _setores = { "Qualidade", "Engenharia", "Producao", "Administrativo" };

        private readonly string[] _map;

        public StringOrIntConverter(string[] map) { _map = map; }
        public StringOrIntConverter() { _map = _perfis; } // padrão

        public override string Read(ref System.Text.Json.Utf8JsonReader reader,
                                    Type typeToConvert,
                                    System.Text.Json.JsonSerializerOptions options)
        {
            // Se vier como número, converte para texto usando o mapa
            if (reader.TokenType == System.Text.Json.JsonTokenType.Number)
            {
                int index = reader.GetInt32();
                return (index >= 0 && index < _map.Length) ? _map[index] : $"({index})";
            }

            // Se vier como string, retorna direto
            return reader.GetString() ?? "";
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer,
                                   string value,
                                   System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public class PerfilConverter : StringOrIntConverter
    {
        private static readonly string[] _map = { "Administrador", "Gerente", "Emitente" };
        public PerfilConverter() : base(_map) { }
    }

    public class SetorConverter : StringOrIntConverter
    {
        private static readonly string[] _map = { "Qualidade", "Engenharia", "Producao", "Administrativo" };
        public SetorConverter() : base(_map) { }
    }

    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }

        // [JsonConverter] permite que este campo aceite string OU int da API
        // O PerfilConverter converte int → "Administrador", "Gerente", etc.
        // Se já vier como string, passa direto
        [JsonConverter(typeof(PerfilConverter))]
        public string? Perfil { get; set; }

        [JsonConverter(typeof(SetorConverter))]
        public string? Setor { get; set; }

        // ComboBox usa ToString() para exibir o item na lista
        public override string? ToString() => $"{Nome}  ({Email})";
    }
}