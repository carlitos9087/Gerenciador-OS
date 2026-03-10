namespace Gerenciador_de_Ordens_de_servico
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Configurações da JANELA PRINCIPAL
            this.Text = "Login - Ordem de Serviço Crítico";  // Título da janela
            this.Size = new Size(1000, 600);                   // Largura x Altura
            this.StartPosition = FormStartPosition.CenterScreen; // Abre no centro
            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // Não pode redimensionar
            this.MaximizeBox = false;  // Desabilita botão de maximizar
            textBox2.PasswordChar = '*';

            CriarTelaLogin();
        }

        private void CriarTelaLogin()
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Pega o que digitou
            string email = textBox1.Text;
            string senha = textBox2.Text;

            // 2. Verifica se digitou algo (validação simples)
            if (email == "" || senha == "")
            {
                MessageBox.Show("Digite email e senha!");
                return;  // Para aqui se estiver vazio
            }

            // 3. Verifica login (só um exemplo!)
            if (textBox1.Text == "admin" && textBox2.Text == "123")
            {
                // 4. ABRE A PRÓXIMA TELA! 🎉
                Form2 telaPrincipal = new Form2();
                telaPrincipal.Show();  // Mostra a nova tela

                this.Hide();  // Esconde a tela de login (opcional)
            }
            else
            {
                MessageBox.Show("Email ou senha errados!");
            }
        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            // 1. Pega o que digitou
            string email = textBox1.Text;
            string senha = textBox2.Text;

            // 2. Verifica se digitou algo (validação simples)
            if (email == "" || senha == "")
            {
                MessageBox.Show("Digite email e senha!");
                return;  // Para aqui se estiver vazio
            }

            // 3. Verifica login (só um exemplo!)
            if (textBox1.Text == "admin" && textBox2.Text == "123")
            {
                // 4. ABRE A PRÓXIMA TELA! 🎉
                Form2 telaPrincipal = new Form2();
                telaPrincipal.Show();  // Mostra a nova tela

                this.Hide();  // Esconde a tela de login (opcional)
            }
            else
            {
                MessageBox.Show("Email ou senha errados!");
            }
        }
    }
}