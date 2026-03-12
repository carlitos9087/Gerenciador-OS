using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form3 : Form
    {
        // ══════════════════════════════════════════════
        // CORES DO TEMA — mude aqui para mudar o visual
        // ══════════════════════════════════════════════
        readonly Color corPrimaria = Color.FromArgb(30, 80, 160);
        readonly Color corBotaoAtivo = Color.FromArgb(30, 80, 160);
        readonly Color corTextoAtivo = Color.White;
        readonly Color corTextoNormal = Color.FromArgb(60, 60, 80);

        Button btnAtivo;

        // HttpClient deve ser estático e reutilizado — nunca crie dentro de métodos
        private static readonly HttpClient _httpClient = new HttpClient();

        // URL base da API — mude aqui se o endereço mudar
        private const string URL_BASE = "http://localhost:5184";

        // Usuário que fez login — recebido do Form1
        // Usado para preencher o usuarioLogadoId nos POSTs
        private UsuarioResponse _usuarioLogado;

        // Construtor recebe o usuário logado vindo do Form1
        public Form3(UsuarioResponse usuarioLogado)
        {
            InitializeComponent();
            _usuarioLogado = usuarioLogado;
        }

        // ══════════════════════════════════════════════
        // LOAD: executado quando o form abre
        // ══════════════════════════════════════════════
        private void Form3_Load(object sender, EventArgs e)
        {
            ConfigurarCabecalho();
            ConfigurarBotoes();

            MostrarDashboard();
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

            // Mostra o nome e perfil do usuário logado no canto direito
            panelCabecalho.Controls.Add(new Label
            {
                Text = $"👤  {_usuarioLogado.Nome}  |  {_usuarioLogado.Perfil}",
                ForeColor = Color.FromArgb(180, 210, 255),
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelCabecalho.Width - 380, 20)
            });

            // Botão de sair — volta para o Form1 (tela de login)
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

            // Ao clicar: confirma, fecha o Form3 e mostra o Form1 de volta
            btnSair.Click += (s, e) =>
            {
                var confirmar = MessageBox.Show(
                    "Deseja sair da conta?",
                    "Confirmar saída",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar == DialogResult.Yes)
                {
                    // Mostra o Form1 (login) que estava oculto
                    // e fecha o Form3 atual
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f is Form1)
                        {
                            f.Show();
                            break;
                        }
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

            // MostrarDashboard e MostrarAssinar são async Task,
            // por isso usamos async nos lambdas dos eventos
            btnDashboard.Click += async (s, e) => { await MostrarDashboard(); DestaqueBotao(btnDashboard); };
            btnCriarRelatorio.Click += (s, e) => { MostrarCriarOS(); DestaqueBotao(btnCriarRelatorio); };
            btnAssinar.Click += async (s, e) => { await MostrarAssinar(); DestaqueBotao(btnAssinar); };
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
            Button[] todos = { btnDashboard, btnCriarRelatorio, btnAssinar };

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
        // Faz GET em /osc, deserializa o JSON e exibe na tabela
        // ══════════════════════════════════════════════════════════════
        const int ITENS_POR_PAGINA = 5;
        int paginaAtual = 1;
        List<OscResponse> todasAsOscs;

        // "async Task" em vez de "async void" porque chamamos com await nos botões
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

            // Mensagem enquanto aguarda a resposta da API
            var lblCarregando = new Label
            {
                Text = "⏳  Buscando dados da API...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 60)
            };
            panelConteudo.Controls.Add(lblCarregando);

            // ── GET /osc ──────────────────────────────────────────────────
            try
            {
                // Faz a requisição e aguarda sem travar a janela
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/osc");

                if (!resposta.IsSuccessStatusCode)
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    lblCarregando.Text = $"❌  Erro {(int)resposta.StatusCode}: {erro}";
                    lblCarregando.ForeColor = Color.Red;
                    return;
                }

                // Lê o corpo da resposta como string JSON
                string json = await resposta.Content.ReadAsStringAsync();

                // Deserializa: converte o JSON em List<OscResponse>
                // PropertyNameCaseInsensitive ignora diferença entre "Status" e "status"
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                todasAsOscs = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes);
            }
            catch (HttpRequestException ex)
            {
                // API offline, URL errada, sem rede
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

            // Remove o "carregando" e monta a tabela
            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            var grid = CriarGrid();
            grid.Location = new Point(0, 52);
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                            AnchorStyles.Right | AnchorStyles.Bottom;
            grid.Width = panelConteudo.Width - 48;
            grid.Height = panelConteudo.Height - 140;
            panelConteudo.Controls.Add(grid);

            var painelPag = CriarPaginacao(grid);
            panelConteudo.Controls.Add(painelPag);

            CarregarPagina(grid, paginaAtual);
        }

        // Cria e configura o DataGridView
        private DataGridView CriarGrid()
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
                RowTemplate = { Height = 38 }
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 241, 255);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(40, 55, 100);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.ColumnHeadersHeight = 42;
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);

            // Ajuste os nomes das colunas conforme o JSON da sua API
            AdicionarColuna(grid, "ID", "id", 70, DataGridViewAutoSizeColumnMode.None);
            AdicionarColuna(grid, "Descrição", "descricao", 200, DataGridViewAutoSizeColumnMode.Fill);
            AdicionarColuna(grid, "Equipamento", "equipamento", 150, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Data", "data", 110, DataGridViewAutoSizeColumnMode.AllCells);
            AdicionarColuna(grid, "Status", "status", 120, DataGridViewAutoSizeColumnMode.AllCells);

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

        // Preenche a grid com os itens da página solicitada
        private void CarregarPagina(DataGridView grid, int pagina)
        {
            grid.Rows.Clear();

            // Calcula o intervalo: página 2, 5 por página → índices 5 a 9
            int inicio = (pagina - 1) * ITENS_POR_PAGINA;
            int fim = Math.Min(inicio + ITENS_POR_PAGINA, todasAsOscs.Count);

            for (int i = inicio; i < fim; i++)
            {
                var osc = todasAsOscs[i];

                // A data pode vir como "2024-06-01T00:00:00" — pegamos só os 10 primeiros chars
                string dataFormatada = !string.IsNullOrEmpty(osc.dataEmissao)
                    ? osc.dataEmissao.Substring(0, Math.Min(10, osc.dataEmissao.Length))
                    : "—";

                int linha = grid.Rows.Add(
                    osc.id,
                    osc.descricao,
                    osc.equipamento,
                    dataFormatada,
                    osc.status ?? "Pendente"
                );

                // Colorir a célula de status
                var celula = grid.Rows[linha].Cells["status"];
                switch ((osc.status ?? "").ToLower())
                {
                    case "aprovado":
                    case "concluido":
                    case "concluído":
                        celula.Style.ForeColor = Color.FromArgb(0, 130, 70);
                        celula.Style.BackColor = Color.FromArgb(220, 255, 235);
                        break;
                    case "pendente":
                    case "em andamento":
                        celula.Style.ForeColor = Color.FromArgb(160, 90, 0);
                        celula.Style.BackColor = Color.FromArgb(255, 244, 205);
                        break;
                    case "rejeitado":
                    case "cancelado":
                        celula.Style.ForeColor = Color.FromArgb(170, 30, 30);
                        celula.Style.BackColor = Color.FromArgb(255, 222, 222);
                        break;
                }
            }
        }

        // Cria o rodapé com botões de paginação
        private Panel CriarPaginacao(DataGridView grid)
        {
            int totalPaginas = (int)Math.Ceiling((double)todasAsOscs.Count / ITENS_POR_PAGINA);
            if (totalPaginas < 1) totalPaginas = 1;

            var painelPag = new Panel
            {
                Height = 44,
                Dock = DockStyle.Bottom,
                BackColor = Color.White
            };

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
                int numPagina = p; // captura local — sem isso todos os lambdas usariam o valor final de p
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
                btn.Font = new Font("Segoe UI", 9,
                                    p == paginaAtual ? FontStyle.Bold : FontStyle.Regular);
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
        // Faz GET dos gerentes por setor e exibe ComboBoxes para seleção
        // Depois faz POST em /osc com os IDs escolhidos
        // ══════════════════════════════════════════════════════════════

        // Método auxiliar: busca gerentes de um setor específico na API
        // Retorna lista vazia se der erro (tratado na UI)
        // Setor é passado como string igual ao enum do backend: "Qualidade", "Engenharia", "Producao"
        private async System.Threading.Tasks.Task<List<UsuarioResponse>> BuscarGerentesPorSetor(string setor)
        {
            try
            {
                // GET /usuarios/gerentes/{setor}
                // OBS: a rota NÃO tem /auth porque o backend usa [HttpGet("/usuarios/gerentes/{setor}")]
                // com barra no início — isso sobrescreve o [Route("/auth")] do controller
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
                // Se a API estiver offline, retorna lista vazia
                // O erro será exibido pelo estado do ComboBox na tela
                return new List<UsuarioResponse>();
            }
        }

        // Cria um Label + ComboBox parecido com AdicionarCampo, mas para listas
        // "itens" é a lista de gerentes; o ComboBox guarda o UsuarioResponse como item
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
                DropDownStyle = ComboBoxStyle.DropDownList,  // impede digitação livre
                FlatStyle = FlatStyle.Flat
            };

            if (itens.Count == 0)
            {
                // Se não vieram gerentes, exibe mensagem de erro no ComboBox
                combo.Items.Add(msgErro);
                combo.SelectedIndex = 0;
                combo.Enabled = false;
            }
            else
            {
                // Adiciona cada gerente como item do ComboBox
                // DisplayMember = qual propriedade mostrar como texto
                // ValueMember   = qual propriedade usar como valor interno (o ID)
                combo.DisplayMember = "Nome";
                combo.ValueMember = "Id";
                foreach (var u in itens)
                    combo.Items.Add(u);

                combo.SelectedIndex = 0; // seleciona o primeiro por padrão
            }

            panelConteudo.Controls.Add(combo);
            y += 34 + 18;

            return combo;
        }

        // MostrarCriarOS é async porque precisa aguardar os 3 GETs de gerentes
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

            // Mensagem enquanto carrega os gerentes da API
            var lblCarregando = new Label
            {
                Text = "⏳  Carregando gerentes...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 46)
            };
            panelConteudo.Controls.Add(lblCarregando);

            // ── Busca os 3 listas de gerentes em paralelo ────────────────
            // Task.WhenAll dispara os 3 GETs ao mesmo tempo e aguarda todos
            // terminarem — muito mais rápido do que fazer um por um
            var taskQualidade = BuscarGerentesPorSetor("Qualidade");
            var taskEngenharia = BuscarGerentesPorSetor("Engenharia");
            var taskProducao = BuscarGerentesPorSetor("Producao");

            await System.Threading.Tasks.Task.WhenAll(taskQualidade, taskEngenharia, taskProducao);

            List<UsuarioResponse> gerentesQualidade = taskQualidade.Result;
            List<UsuarioResponse> gerentesEngenharia = taskEngenharia.Result;
            List<UsuarioResponse> gerentesProducao = taskProducao.Result;

            // Remove o "carregando" agora que os dados chegaram
            panelConteudo.Controls.Remove(lblCarregando);
            lblCarregando.Dispose();

            int y = 46;

            // Campos de texto normais
            AdicionarCampo("Descrição:", ref y, out TextBox txtDescricao);
            AdicionarCampo("Equipamento:", ref y, out TextBox txtEquipamento);

            // ComboBoxes carregados com os gerentes vindos da API
            // Se a lista vier vazia, o ComboBox fica desabilitado com mensagem de erro
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
                // ── Validações ────────────────────────────────────
                if (string.IsNullOrWhiteSpace(txtDescricao.Text))
                {
                    MessageBox.Show("Informe a descrição!", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEquipamento.Text))
                {
                    MessageBox.Show("Informe o equipamento!", "Atenção",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!cmbQualidade.Enabled || !cmbEngenharia.Enabled || !cmbProducao.Enabled)
                {
                    MessageBox.Show("Não foi possível carregar todos os gerentes. Tente novamente.",
                        "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Recupera o UsuarioResponse selecionado em cada ComboBox
                // e pega o ID de cada um
                var gerenteQualidade = (UsuarioResponse)cmbQualidade.SelectedItem;
                var gerenteEngenharia = (UsuarioResponse)cmbEngenharia.SelectedItem;
                var gerenteProducao = (UsuarioResponse)cmbProducao.SelectedItem;

                // ── Monta o payload com os IDs reais dos gerentes ──
                var payload = new
                {
                    descricao = txtDescricao.Text,
                    equipamento = txtEquipamento.Text,
                    gerenteQualidadeId = gerenteQualidade.Id,
                    gerenteEngenhariaId = gerenteEngenharia.Id,
                    gerenteProducaoId = gerenteProducao.Id,
                    usuarioLogadoId = _usuarioLogado.Id  // ID do usuário que fez login
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
                        MessageBox.Show("Ordem de Serviço criada com sucesso!",
                            "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        await MostrarDashboard();
                        DestaqueBotao(btnDashboard);
                    }
                    else
                    {
                        string corpoErro = await resposta.Content.ReadAsStringAsync();
                        MessageBox.Show(
                            $"Erro da API:\nStatus: {(int)resposta.StatusCode} {resposta.StatusCode}\n\n{corpoErro}",
                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(
                        $"Sem conexão com a API.\nVerifique se está rodando em {URL_BASE}\n\nDetalhe: {ex.Message}",
                        "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro inesperado:\n{ex.Message}",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        // Busca as OSCs via GET, filtra as pendentes e permite assinar
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

            // Faz um GET para buscar as OSCs e filtra as pendentes
            var pendentes = new List<OscResponse>();
            try
            {
                var resposta = await _httpClient.GetAsync($"{URL_BASE}/osc");

                if (resposta.IsSuccessStatusCode)
                {
                    string json = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var todas = JsonSerializer.Deserialize<List<OscResponse>>(json, opcoes);

                    // Filtra somente as OSCs com status pendente
                    foreach (var osc in todas)
                    {
                        string status = (osc.status ?? "").ToLower();
                        if (status == "pendente" || status == "em andamento" || status == "")
                            pendentes.Add(osc);
                    }
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

            // ListBox com as OSCs pendentes
            var lista = new ListBox
            {
                Location = new Point(0, 68),
                Width = 560,
                Height = 220,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 250, 255)
            };

            // Dicionário para recuperar o ID da OS pelo texto selecionado
            var mapaItens = new Dictionary<string, int>();

            if (pendentes.Count == 0)
            {
                lista.Items.Add("Nenhuma OS pendente encontrada.");
            }
            else
            {
                foreach (var osc in pendentes)
                {
                    string texto = $"[OS-{osc.id:D3}]  {osc.descricao}  —  {osc.equipamento}";
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

                string itemSelecionado = lista.SelectedItem.ToString();

                var confirmacao = MessageBox.Show(
                    $"Confirma a assinatura?\n\n{itemSelecionado}",
                    "Confirmar Assinatura",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacao != DialogResult.Yes) return;

                int idOsc = mapaItens[itemSelecionado];

                btnAssinarOk.Enabled = false;
                btnAssinarOk.Text = "⏳  Assinando...";

                try
                {
                    // TODO: substitua pela chamada real do endpoint de assinatura
                    // Exemplo: await _httpClient.PutAsync($"{URL_BASE}/osc/{idOsc}/assinar", null);
                    MessageBox.Show($"OS-{idOsc:D3} assinada com sucesso!",
                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await MostrarDashboard();
                    DestaqueBotao(btnDashboard);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao assinar: {ex.Message}",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnAssinarOk.Enabled = true;
                    btnAssinarOk.Text = "✅  Assinar Selecionada";
                }
            };

            panelConteudo.Controls.Add(btnAssinarOk);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // OscResponse: espelha exatamente o JSON que a API retorna
    //
    // IMPORTANTE: abra http://localhost:5184/osc no navegador,
    // veja os campos que aparecem e ajuste esta classe para bater.
    // PropertyNameCaseInsensitive já cuida de maiúsculas/minúsculas.
    // ══════════════════════════════════════════════════════════════
    public class OscResponse
    {
        public int id { get; set; }
        public string descricao { get; set; }
        public string equipamento { get; set; }
        public string status { get; set; }
        public string dataEmissao { get; set; }
        public int gerenteQualidadeId { get; set; }
        public int gerenteEngenhariaId { get; set; }
        public int gerenteProducaoId { get; set; }
        public int usuarioLogadoId { get; set; }
        // Adicione outros campos que a API retornar
    }

    // ══════════════════════════════════════════════════════════════
    // UsuarioResponse: espelha o JSON retornado por GET /auth/usuarios/gerentes/{setor}
    // O ToString() é sobrescrito para que o ComboBox exiba Nome — Setor
    // ══════════════════════════════════════════════════════════════
    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Perfil { get; set; }
        public string Setor { get; set; }

        // O ComboBox chama ToString() para exibir o item na lista
        // Sobrescrevemos para mostrar algo mais amigável que o tipo da classe
        public override string ToString() => $"{Nome}  ({Email})";
    }
}