
namespace SimpleCompiler
{
    partial class FrmSimpleCompiler
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtSource = new System.Windows.Forms.RichTextBox();
            this.lbConsole = new System.Windows.Forms.ListBox();
            this.mnuConsoleContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuConsoleSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConsoleCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClearConsole = new System.Windows.Forms.Button();
            this.btnCompileProgram = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.mnuConsoleContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(2, 0);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(796, 274);
            this.txtSource.TabIndex = 0;
            this.txtSource.Text = "";
            // 
            // lbConsole
            // 
            this.lbConsole.ContextMenuStrip = this.mnuConsoleContext;
            this.lbConsole.FormattingEnabled = true;
            this.lbConsole.Location = new System.Drawing.Point(2, 314);
            this.lbConsole.Name = "lbConsole";
            this.lbConsole.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbConsole.Size = new System.Drawing.Size(796, 277);
            this.lbConsole.TabIndex = 1;
            // 
            // mnuConsoleContext
            // 
            this.mnuConsoleContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuConsoleSelectAll,
            this.mnuConsoleCopy});
            this.mnuConsoleContext.Name = "mnuConsoleContext";
            this.mnuConsoleContext.Size = new System.Drawing.Size(201, 48);
            // 
            // mnuConsoleSelectAll
            // 
            this.mnuConsoleSelectAll.Name = "mnuConsoleSelectAll";
            this.mnuConsoleSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.mnuConsoleSelectAll.Size = new System.Drawing.Size(200, 22);
            this.mnuConsoleSelectAll.Text = "Selecionar Tudo";
            this.mnuConsoleSelectAll.Click += new System.EventHandler(this.mnuConsoleSelectAll_Click);
            // 
            // mnuConsoleCopy
            // 
            this.mnuConsoleCopy.Name = "mnuConsoleCopy";
            this.mnuConsoleCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuConsoleCopy.Size = new System.Drawing.Size(200, 22);
            this.mnuConsoleCopy.Text = "Copiar";
            this.mnuConsoleCopy.Click += new System.EventHandler(this.mnuConsoleCopy_Click);
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(152, 280);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(120, 28);
            this.btnCompile.TabIndex = 2;
            this.btnCompile.Text = "Compilar Expressão";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(404, 280);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(120, 28);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClearConsole
            // 
            this.btnClearConsole.Location = new System.Drawing.Point(530, 280);
            this.btnClearConsole.Name = "btnClearConsole";
            this.btnClearConsole.Size = new System.Drawing.Size(120, 28);
            this.btnClearConsole.TabIndex = 4;
            this.btnClearConsole.Text = "Limpar Console";
            this.btnClearConsole.UseVisualStyleBackColor = true;
            this.btnClearConsole.Click += new System.EventHandler(this.btnClearConsole_Click);
            // 
            // btnCompileProgram
            // 
            this.btnCompileProgram.Location = new System.Drawing.Point(278, 280);
            this.btnCompileProgram.Name = "btnCompileProgram";
            this.btnCompileProgram.Size = new System.Drawing.Size(120, 28);
            this.btnCompileProgram.TabIndex = 5;
            this.btnCompileProgram.Text = "Compilar Programa";
            this.btnCompileProgram.UseVisualStyleBackColor = true;
            this.btnCompileProgram.Click += new System.EventHandler(this.btnCompileProgram_Click);
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(2, 597);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(796, 20);
            this.txtInput.TabIndex = 6;
            this.txtInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInput_KeyPress);
            // 
            // FrmSimpleCompiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 618);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.btnCompileProgram);
            this.Controls.Add(this.btnClearConsole);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.lbConsole);
            this.Controls.Add(this.txtSource);
            this.Name = "FrmSimpleCompiler";
            this.Text = "Simple Compiler";
            this.mnuConsoleContext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtSource;
        private System.Windows.Forms.ListBox lbConsole;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClearConsole;
        private System.Windows.Forms.Button btnCompileProgram;
        private System.Windows.Forms.ContextMenuStrip mnuConsoleContext;
        private System.Windows.Forms.ToolStripMenuItem mnuConsoleCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuConsoleSelectAll;
        private System.Windows.Forms.TextBox txtInput;
    }
}

