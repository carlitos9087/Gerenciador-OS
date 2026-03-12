// ============================================================
// Form3.Designer.cs
// Este arquivo é normalmente gerado pelo Visual Studio.
// Cole ele no seu projeto junto com o Form3.cs
// ============================================================

namespace Gerenciador_de_Ordens_de_servico
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        // Liberação de memória quando o form é fechado
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // ── Aqui ficam os controles "fixos" (cabeçalho, lateral, área de conteúdo)
        // Os controles do conteúdo (grid, campos, etc.) são criados em Form3.cs
        private void InitializeComponent()
        {
            this.panelCabecalho = new System.Windows.Forms.Panel();
            this.panelLateral = new System.Windows.Forms.Panel();
            this.btnAssinar = new System.Windows.Forms.Button();
            this.btnCriarRelatorio = new System.Windows.Forms.Button();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.panelConteudo = new System.Windows.Forms.Panel();

            // Suspende o layout enquanto configura (evita "piscadas")
            this.SuspendLayout();
            this.panelCabecalho.SuspendLayout();
            this.panelLateral.SuspendLayout();

            // ────────────────────────────────────────
            // panelCabecalho — faixa do topo
            // ────────────────────────────────────────
            this.panelCabecalho.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCabecalho.Location = new System.Drawing.Point(0, 0);
            this.panelCabecalho.Name = "panelCabecalho";
            this.panelCabecalho.Size = new System.Drawing.Size(1100, 60);
            this.panelCabecalho.TabIndex = 0;

            // ────────────────────────────────────────
            // panelLateral — coluna esquerda
            // ────────────────────────────────────────
            this.panelLateral.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.panelLateral.Controls.Add(this.btnAssinar);
            this.panelLateral.Controls.Add(this.btnCriarRelatorio);
            this.panelLateral.Controls.Add(this.btnDashboard);
            this.panelLateral.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLateral.Location = new System.Drawing.Point(0, 60);
            this.panelLateral.Name = "panelLateral";
            this.panelLateral.Size = new System.Drawing.Size(200, 590);
            this.panelLateral.TabIndex = 1;

            // Linha separadora direita da lateral (desenhada via evento Paint)
            this.panelLateral.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(210, 215, 225), 1),
                    this.panelLateral.Width - 1, 0,
                    this.panelLateral.Width - 1, this.panelLateral.Height);
            };

            // ────────────────────────────────────────
            // btnDashboard
            // ────────────────────────────────────────
            this.btnDashboard.Location = new System.Drawing.Point(12, 20);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(176, 44);
            this.btnDashboard.TabIndex = 0;
            // Texto e estilo são aplicados em Form3.cs (ConfigurarBotoes)

            // ────────────────────────────────────────
            // btnCriarRelatorio
            // ────────────────────────────────────────
            this.btnCriarRelatorio.Location = new System.Drawing.Point(12, 74);
            this.btnCriarRelatorio.Name = "btnCriarRelatorio";
            this.btnCriarRelatorio.Size = new System.Drawing.Size(176, 44);
            this.btnCriarRelatorio.TabIndex = 1;

            // ────────────────────────────────────────
            // btnAssinar
            // ────────────────────────────────────────
            this.btnAssinar.Location = new System.Drawing.Point(12, 128);
            this.btnAssinar.Name = "btnAssinar";
            this.btnAssinar.Size = new System.Drawing.Size(176, 44);
            this.btnAssinar.TabIndex = 2;

            // ────────────────────────────────────────
            // panelConteudo — área que muda de conteúdo
            // DockStyle.Fill = ocupa todo espaço restante
            // ────────────────────────────────────────
            this.panelConteudo.BackColor = System.Drawing.Color.White;
            this.panelConteudo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConteudo.Location = new System.Drawing.Point(200, 60);
            this.panelConteudo.Name = "panelConteudo";
            this.panelConteudo.Padding = new System.Windows.Forms.Padding(24);
            this.panelConteudo.TabIndex = 2;

            // ────────────────────────────────────────
            // Form3 — a janela em si
            // ────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.MinimumSize = new System.Drawing.Size(860, 500);
            this.Text = "Gerenciador de Ordens de Serviço";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // Adiciona os controles principais na janela
            // ATENÇÃO: a ORDEM importa!
            // panelConteudo (Fill) deve ser adicionado ANTES dos outros,
            // e depois chamamos BringToFront() para ele aparecer corretamente
            this.Controls.Add(this.panelConteudo);
            this.Controls.Add(this.panelLateral);
            this.Controls.Add(this.panelCabecalho);

            this.panelCabecalho.ResumeLayout(false);
            this.panelLateral.ResumeLayout(false);
            this.ResumeLayout(false);

            // Evento Load (chamado quando o form abre)
            this.Load += new System.EventHandler(this.Form3_Load);
        }

        // ── Declaração dos controles (o Visual Studio gera isso automaticamente)
        private System.Windows.Forms.Panel panelCabecalho;
        private System.Windows.Forms.Panel panelLateral;
        private System.Windows.Forms.Panel panelConteudo;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnCriarRelatorio;
        private System.Windows.Forms.Button btnAssinar;
    }
}