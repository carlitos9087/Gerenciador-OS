using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form1 : Form
    {
        // O mesmo HttpClient do Form3 — estático para ser reutilizado
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string URL_BASE = "http://localhost:5184";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Login - Gerenciador de OS";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            // Mascara a senha com asteriscos
            textBox2.PasswordChar = '*';

            // Permite pressionar Enter no campo de senha para logar
            textBox2.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                    FazerLogin();
            };

            // Quando o Form1 volta a aparecer (usuário saiu do Form3),
            // limpa os campos e reseta o botão para nova sessão
            this.VisibleChanged += (s, ev) =>
            {
                if (this.Visible)
                {
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox1.Focus();
                    button1.Enabled = true;
                    button1.Text = "Entrar";
                }
            };
        }

        // Evento do botão — agora chama FazerLogin()
        // "async void" é obrigatório para eventos que usam await
        private async void button1_Click_1(object sender, EventArgs e)
        {
            await FazerLogin();
        }

        // ══════════════════════════════════════════════════════════════
        // LÓGICA DE LOGIN
        // Faz POST em /auth/login com email e senha
        // Se a API retornar 200, abre o Form3 e guarda o usuário logado
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task FazerLogin()
        {
            string email = textBox1.Text.Trim();
            string senha = textBox2.Text;

            // ── Validação básica antes de chamar a API ──────────────
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Digite o e-mail e a senha.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Monta o payload igual ao LoginRequest do backend ────
            // { "email": "...", "senha": "..." }
            var payload = new { email, senha };
            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            // Desabilita o botão durante a requisição
            button1.Enabled = false;
            button1.Text = "Entrando...";

            try
            {
                // POST /auth/login — aguarda sem travar a tela
                var resposta = await _httpClient.PostAsync($"{URL_BASE}/auth/login", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    // Deserializa o UsuarioResponse que a API retornou
                    string corpoJson = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var usuarioLogado = JsonSerializer.Deserialize<UsuarioResponse>(corpoJson, opcoes);

                    // Abre a tela principal passando o usuário logado
                    // Assim o Form3 sabe quem está logado (para usar o usuarioLogadoId)
                    var telaPrincipal = new Form3(usuarioLogado);
                    telaPrincipal.Show();
                    this.Hide();
                }
                else if (resposta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 401 = credenciais erradas
                    MessageBox.Show("E-mail ou senha incorretos.", "Acesso negado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Outro erro da API (500, 404, etc.)
                    string erro = await resposta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro da API: {(int)resposta.StatusCode}\n\n{erro}",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                // API offline ou URL errada
                MessageBox.Show(
                    $"Não foi possível conectar à API.\nVerifique se está rodando em {URL_BASE}\n\nDetalhe: {ex.Message}",
                    "Erro de Conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado:\n{ex.Message}",
                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Sempre reabilita o botão, mesmo se der erro
                button1.Enabled = true;
                button1.Text = "Entrar";
            }
        }
    }
}