using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form1 : Form
    {
        // Usa o HttpClient compartilhado — mesmo que Form3
        // (O /auth/login não exige token, mas usar o mesmo cliente é mais limpo)
        private const string URL_BASE = "https://localhost:7188";

        public Form1()
        {
            InitializeComponent();
        }

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    this.Text = "Login - Gerenciador de OS";
        //    this.Size = new Size(1100, 700);
        //    this.MinimumSize = new Size(1000, 600);
        //    this.StartPosition = FormStartPosition.CenterScreen;
        //    this.FormBorderStyle = FormBorderStyle.Sizable;
        //    this.MaximizeBox = true;

        //    // Mascara a senha com asteriscos
        //    textBox2.PasswordChar = '*';

        //    // Pressionar Enter no campo senha também loga
        //    textBox2.KeyDown += (s, ev) =>
        //    {
        //        if (ev.KeyCode == Keys.Enter)
        //            _ = FazerLogin();
        //    };

        //    // Quando o Form1 volta a aparecer (usuário saiu do Form3),
        //    // limpa os campos para nova sessão
        //    this.VisibleChanged += (s, ev) =>
        //    {
        //        if (this.Visible)
        //        {
        //            textBox1.Clear();
        //            textBox2.Clear();
        //            textBox1.Focus();
        //            button1.Enabled = true;
        //            button1.Text = "Entrar";
        //        }
        //    };
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Login - Gerenciador de OS";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            textBox2.PasswordChar = '*';

            textBox2.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                    _ = FazerLogin();
            };

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

            // ── Botão arredondado ──────────────────────────────────
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.FromArgb(30, 80, 160);
            button1.ForeColor = Color.White;
            button1.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            button1.Cursor = Cursors.Hand;
            button1.Paint += Button1_Paint;
            button1.SizeChanged += (s, ev) => button1.Invalidate();

            // ── TextBoxes sem borda nativa (a borda será desenhada pelo painel wrapper) ──
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Font = new Font("Segoe UI", 10);
            textBox1.BackColor = Color.White;

            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Font = new Font("Segoe UI", 10);
            textBox2.BackColor = Color.White;

            // ── Wrappers arredondados nos TextBoxes ────────────────
            AplicarWrapperArredondado(textBox1, 8);
            AplicarWrapperArredondado(textBox2, 8);

            // ── Panel2 arredondado com sombra ──────────────────────
            panel2.Paint += Panel2_Paint;
            panel2.BackColor = Color.White;
        }

        // Evento do botão de login
        private async void button1_Click_1(object sender, EventArgs e)
        {
            await FazerLogin();
        }

        // ══════════════════════════════════════════════════════════════
        // LÓGICA DE LOGIN
        // POST /auth/login → recebe UsuarioResponse → abre Form3
        // ══════════════════════════════════════════════════════════════
        private async System.Threading.Tasks.Task FazerLogin()
        {
            string email = textBox1.Text.Trim();
            string senha = textBox2.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Digite o e-mail e a senha.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Monta o payload: { "email": "...", "senha": "..." }
            var payload = new { email, senha };
            string json = JsonSerializer.Serialize(payload);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            button1.Enabled = false;
            button1.Text = "Entrando...";

            try
            {
                var resposta = await ApiConfig.Http.PostAsync($"{URL_BASE}/auth/login", conteudo);

                if (resposta.IsSuccessStatusCode)
                {
                    string corpoJson = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // API retorna LoginResponse: { "token": "eyJ...", "usuario": { ... } }
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(corpoJson, opcoes);

                    if (loginResponse == null || loginResponse.Usuario == null)
                    {
                        MessageBox.Show("Resposta inválida da API.", "Erro",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Salva o token e configura o header no HttpClient compartilhado
                    // A partir daqui, ApiConfig.Http envia Bearer em TODAS as requisições
                    ApiConfig.SalvarToken(loginResponse.Token);

                    var telaPrincipal = new Form3(loginResponse.Usuario);
                    telaPrincipal.Show();
                    this.Hide();
                }
                else if (resposta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("E-mail ou senha incorretos.", "Acesso negado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string erro = await resposta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro da API: {(int)resposta.StatusCode}\n\n{erro}",
                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException ex)
            {
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
                button1.Enabled = true;
                button1.Text = "Entrar";
            }
        }

        // Necessário — Form1.Designer.cs registra este método no evento Click do label4
        private void label4_Click(object sender, EventArgs e) { }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        // Método utilitário para desenhar caminhos de borda arredondada
        public static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // Topo-esquerdo
            path.AddArc(arc, 180, 90);

            // Topo-direito
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Baixo-direito
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Baixo-esquerdo
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}