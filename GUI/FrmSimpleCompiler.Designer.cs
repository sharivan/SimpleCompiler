namespace SimpleCompiler.GUI;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSimpleCompiler));
            this.scMain = new System.Windows.Forms.SplitContainer();
            this.scCode = new System.Windows.Forms.SplitContainer();
            this.tcSources = new System.Windows.Forms.TabControl();
            this.tcRight = new System.Windows.Forms.TabControl();
            this.tpAssembly = new System.Windows.Forms.TabPage();
            this.wpfAssemblyHost = new System.Windows.Forms.Integration.ElementHost();
            this.scBottom = new System.Windows.Forms.SplitContainer();
            this.txtConsole = new System.Windows.Forms.RichTextBox();
            this.mnuConsole = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuConsoleClear = new System.Windows.Forms.ToolStripMenuItem();
            this.tcDebugView = new System.Windows.Forms.TabControl();
            this.tpVariables = new System.Windows.Forms.TabPage();
            this.dgvVariables = new System.Windows.Forms.DataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewButtonColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnRA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnHA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tpMemory = new System.Windows.Forms.TabPage();
            this.splitContainerData = new System.Windows.Forms.SplitContainer();
            this.gbStack = new System.Windows.Forms.GroupBox();
            this.dgvStack = new System.Windows.Forms.DataGridView();
            this.RA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewButtonColumn();
            this.View = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mnuStackView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuStackViewAlign16 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStackViewAlign8 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStackViewAlign4 = new System.Windows.Forms.ToolStripMenuItem();
            this.gbStrings = new System.Windows.Forms.GroupBox();
            this.dgvStrings = new System.Windows.Forms.DataGridView();
            this.StringHA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringRefCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringLen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringValue = new System.Windows.Forms.DataGridViewButtonColumn();
            this.gbRegisters = new System.Windows.Forms.GroupBox();
            this.lblRegisters = new System.Windows.Forms.Label();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.progress = new System.Windows.Forms.ToolStripProgressBar();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnReload = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.btnCompile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStepOver = new System.Windows.Forms.ToolStripButton();
            this.btnStepInto = new System.Windows.Forms.ToolStripButton();
            this.btnStepOut = new System.Windows.Forms.ToolStripButton();
            this.btnRunToCursor = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnToggleBreakpoint = new System.Windows.Forms.ToolStripButton();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.bwCompiler = new System.ComponentModel.BackgroundWorker();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReload = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCloseFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewCode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewAssembly = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewVariables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewMemory = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRun = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPause = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStepOver = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStepInto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStepOut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRunToCursor = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuToggleBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
            this.scMain.Panel1.SuspendLayout();
            this.scMain.Panel2.SuspendLayout();
            this.scMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scCode)).BeginInit();
            this.scCode.Panel1.SuspendLayout();
            this.scCode.Panel2.SuspendLayout();
            this.scCode.SuspendLayout();
            this.tcRight.SuspendLayout();
            this.tpAssembly.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).BeginInit();
            this.scBottom.Panel1.SuspendLayout();
            this.scBottom.Panel2.SuspendLayout();
            this.scBottom.SuspendLayout();
            this.mnuConsole.SuspendLayout();
            this.tcDebugView.SuspendLayout();
            this.tpVariables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).BeginInit();
            this.tpMemory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).BeginInit();
            this.splitContainerData.Panel1.SuspendLayout();
            this.splitContainerData.Panel2.SuspendLayout();
            this.splitContainerData.SuspendLayout();
            this.gbStack.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStack)).BeginInit();
            this.mnuStackView.SuspendLayout();
            this.gbStrings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStrings)).BeginInit();
            this.gbRegisters.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.mnuMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // scMain
            // 
            this.scMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scMain.Location = new System.Drawing.Point(0, 51);
            this.scMain.Name = "scMain";
            this.scMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scMain.Panel1
            // 
            this.scMain.Panel1.Controls.Add(this.scCode);
            // 
            // scMain.Panel2
            // 
            this.scMain.Panel2.Controls.Add(this.scBottom);
            this.scMain.Size = new System.Drawing.Size(1462, 545);
            this.scMain.SplitterDistance = 343;
            this.scMain.TabIndex = 12;
            // 
            // scCode
            // 
            this.scCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scCode.Location = new System.Drawing.Point(0, 0);
            this.scCode.Name = "scCode";
            // 
            // scCode.Panel1
            // 
            this.scCode.Panel1.Controls.Add(this.tcSources);
            // 
            // scCode.Panel2
            // 
            this.scCode.Panel2.Controls.Add(this.tcRight);
            this.scCode.Size = new System.Drawing.Size(1462, 343);
            this.scCode.SplitterDistance = 793;
            this.scCode.SplitterWidth = 10;
            this.scCode.TabIndex = 8;
            // 
            // tcSources
            // 
            this.tcSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSources.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tcSources.Location = new System.Drawing.Point(0, 0);
            this.tcSources.Name = "tcSources";
            this.tcSources.SelectedIndex = 0;
            this.tcSources.Size = new System.Drawing.Size(793, 343);
            this.tcSources.TabIndex = 2;
            this.tcSources.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TcSources_DrawItem);
            this.tcSources.SelectedIndexChanged += new System.EventHandler(this.TcSources_SelectedIndexChanged);
            this.tcSources.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TcSources_MouseDown);
            // 
            // tcRight
            // 
            this.tcRight.Controls.Add(this.tpAssembly);
            this.tcRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcRight.Location = new System.Drawing.Point(0, 0);
            this.tcRight.Name = "tcRight";
            this.tcRight.SelectedIndex = 0;
            this.tcRight.Size = new System.Drawing.Size(659, 343);
            this.tcRight.TabIndex = 4;
            // 
            // tpAssembly
            // 
            this.tpAssembly.Controls.Add(this.wpfAssemblyHost);
            this.tpAssembly.Location = new System.Drawing.Point(4, 22);
            this.tpAssembly.Name = "tpAssembly";
            this.tpAssembly.Padding = new System.Windows.Forms.Padding(3);
            this.tpAssembly.Size = new System.Drawing.Size(651, 317);
            this.tpAssembly.TabIndex = 0;
            this.tpAssembly.Text = "Assembly";
            this.tpAssembly.UseVisualStyleBackColor = true;
            // 
            // wpfAssemblyHost
            // 
            this.wpfAssemblyHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpfAssemblyHost.Location = new System.Drawing.Point(3, 3);
            this.wpfAssemblyHost.Name = "wpfAssemblyHost";
            this.wpfAssemblyHost.Size = new System.Drawing.Size(645, 311);
            this.wpfAssemblyHost.TabIndex = 0;
            this.wpfAssemblyHost.Child = null;
            // 
            // scBottom
            // 
            this.scBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scBottom.Location = new System.Drawing.Point(0, 0);
            this.scBottom.Name = "scBottom";
            // 
            // scBottom.Panel1
            // 
            this.scBottom.Panel1.Controls.Add(this.txtConsole);
            // 
            // scBottom.Panel2
            // 
            this.scBottom.Panel2.Controls.Add(this.tcDebugView);
            this.scBottom.Size = new System.Drawing.Size(1462, 198);
            this.scBottom.SplitterDistance = 793;
            this.scBottom.SplitterWidth = 10;
            this.scBottom.TabIndex = 7;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtConsole.ContextMenuStrip = this.mnuConsole;
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsole.ForeColor = System.Drawing.Color.Gainsboro;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.Size = new System.Drawing.Size(793, 198);
            this.txtConsole.TabIndex = 5;
            this.txtConsole.Text = "";
            this.txtConsole.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtConsole_KeyPress);
            // 
            // mnuConsole
            // 
            this.mnuConsole.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuConsoleClear});
            this.mnuConsole.Name = "mnuConsole";
            this.mnuConsole.Size = new System.Drawing.Size(112, 26);
            // 
            // mnuConsoleClear
            // 
            this.mnuConsoleClear.Name = "mnuConsoleClear";
            this.mnuConsoleClear.Size = new System.Drawing.Size(111, 22);
            this.mnuConsoleClear.Text = "Limpar";
            this.mnuConsoleClear.Click += new System.EventHandler(this.MnuConsoleClear_Click);
            // 
            // tcDebugView
            // 
            this.tcDebugView.Controls.Add(this.tpVariables);
            this.tcDebugView.Controls.Add(this.tpMemory);
            this.tcDebugView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcDebugView.Location = new System.Drawing.Point(0, 0);
            this.tcDebugView.Name = "tcDebugView";
            this.tcDebugView.SelectedIndex = 0;
            this.tcDebugView.Size = new System.Drawing.Size(659, 198);
            this.tcDebugView.TabIndex = 2;
            // 
            // tpVariables
            // 
            this.tpVariables.Controls.Add(this.dgvVariables);
            this.tpVariables.Location = new System.Drawing.Point(4, 22);
            this.tpVariables.Name = "tpVariables";
            this.tpVariables.Padding = new System.Windows.Forms.Padding(3);
            this.tpVariables.Size = new System.Drawing.Size(651, 172);
            this.tpVariables.TabIndex = 0;
            this.tpVariables.Text = "Variáveis";
            this.tpVariables.UseVisualStyleBackColor = true;
            // 
            // dgvVariables
            // 
            this.dgvVariables.AllowUserToAddRows = false;
            this.dgvVariables.AllowUserToDeleteRows = false;
            this.dgvVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVariables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName,
            this.columnValue,
            this.columnType,
            this.columnRA,
            this.columnHA});
            this.dgvVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVariables.Location = new System.Drawing.Point(3, 3);
            this.dgvVariables.Name = "dgvVariables";
            this.dgvVariables.ReadOnly = true;
            this.dgvVariables.Size = new System.Drawing.Size(645, 166);
            this.dgvVariables.TabIndex = 0;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnName.HeaderText = "Nome";
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            this.columnName.Width = 60;
            // 
            // columnValue
            // 
            this.columnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnValue.HeaderText = "Valor";
            this.columnValue.Name = "columnValue";
            this.columnValue.ReadOnly = true;
            this.columnValue.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.columnValue.Width = 56;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.HeaderText = "Tipo";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.Width = 53;
            // 
            // columnRA
            // 
            this.columnRA.HeaderText = "RA";
            this.columnRA.Name = "columnRA";
            this.columnRA.ReadOnly = true;
            // 
            // columnHA
            // 
            this.columnHA.HeaderText = "HA";
            this.columnHA.Name = "columnHA";
            this.columnHA.ReadOnly = true;
            // 
            // tpMemory
            // 
            this.tpMemory.Controls.Add(this.splitContainerData);
            this.tpMemory.Controls.Add(this.gbRegisters);
            this.tpMemory.Location = new System.Drawing.Point(4, 22);
            this.tpMemory.Name = "tpMemory";
            this.tpMemory.Padding = new System.Windows.Forms.Padding(3);
            this.tpMemory.Size = new System.Drawing.Size(651, 172);
            this.tpMemory.TabIndex = 1;
            this.tpMemory.Text = "Memória";
            this.tpMemory.UseVisualStyleBackColor = true;
            // 
            // splitContainerData
            // 
            this.splitContainerData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerData.Location = new System.Drawing.Point(3, 49);
            this.splitContainerData.Name = "splitContainerData";
            // 
            // splitContainerData.Panel1
            // 
            this.splitContainerData.Panel1.Controls.Add(this.gbStack);
            // 
            // splitContainerData.Panel2
            // 
            this.splitContainerData.Panel2.Controls.Add(this.gbStrings);
            this.splitContainerData.Size = new System.Drawing.Size(645, 120);
            this.splitContainerData.SplitterDistance = 391;
            this.splitContainerData.TabIndex = 3;
            // 
            // gbStack
            // 
            this.gbStack.Controls.Add(this.dgvStack);
            this.gbStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStack.Location = new System.Drawing.Point(0, 0);
            this.gbStack.Name = "gbStack";
            this.gbStack.Size = new System.Drawing.Size(391, 120);
            this.gbStack.TabIndex = 0;
            this.gbStack.TabStop = false;
            this.gbStack.Text = "Pilha";
            // 
            // dgvStack
            // 
            this.dgvStack.AllowUserToAddRows = false;
            this.dgvStack.AllowUserToDeleteRows = false;
            this.dgvStack.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStack.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RA,
            this.HA,
            this.Value,
            this.View});
            this.dgvStack.ContextMenuStrip = this.mnuStackView;
            this.dgvStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStack.Location = new System.Drawing.Point(3, 16);
            this.dgvStack.Name = "dgvStack";
            this.dgvStack.ReadOnly = true;
            this.dgvStack.Size = new System.Drawing.Size(385, 101);
            this.dgvStack.TabIndex = 1;
            this.dgvStack.Scroll += new System.Windows.Forms.ScrollEventHandler(this.DgvStack_Scroll);
            this.dgvStack.Resize += new System.EventHandler(this.DgvStack_Resize);
            // 
            // RA
            // 
            this.RA.HeaderText = "RA";
            this.RA.Name = "RA";
            this.RA.ReadOnly = true;
            this.RA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.RA.ToolTipText = "Endereço Residente";
            // 
            // HA
            // 
            this.HA.HeaderText = "HA";
            this.HA.Name = "HA";
            this.HA.ReadOnly = true;
            this.HA.ToolTipText = "Endereço Host";
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Value.HeaderText = "Valor";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Width = 37;
            // 
            // View
            // 
            this.View.HeaderText = "Visualização em caracteres";
            this.View.Name = "View";
            this.View.ReadOnly = true;
            this.View.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.View.Width = 200;
            // 
            // mnuStackView
            // 
            this.mnuStackView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStackViewAlign16,
            this.mnuStackViewAlign8,
            this.mnuStackViewAlign4});
            this.mnuStackView.Name = "mnuStackView";
            this.mnuStackView.Size = new System.Drawing.Size(193, 70);
            // 
            // mnuStackViewAlign16
            // 
            this.mnuStackViewAlign16.Name = "mnuStackViewAlign16";
            this.mnuStackViewAlign16.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign16.Text = "Alinhamento: 16 bytes";
            this.mnuStackViewAlign16.Click += new System.EventHandler(this.MnuStackViewAlign16_Click);
            // 
            // mnuStackViewAlign8
            // 
            this.mnuStackViewAlign8.Name = "mnuStackViewAlign8";
            this.mnuStackViewAlign8.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign8.Text = "Alinhamento: 8 bytes";
            this.mnuStackViewAlign8.Click += new System.EventHandler(this.MnuStackViewAlign8_Click);
            // 
            // mnuStackViewAlign4
            // 
            this.mnuStackViewAlign4.Name = "mnuStackViewAlign4";
            this.mnuStackViewAlign4.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign4.Text = "Alinhamento: 4 bytes";
            this.mnuStackViewAlign4.Click += new System.EventHandler(this.MnuStackViewAlign4_Click);
            // 
            // gbStrings
            // 
            this.gbStrings.Controls.Add(this.dgvStrings);
            this.gbStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStrings.Location = new System.Drawing.Point(0, 0);
            this.gbStrings.Name = "gbStrings";
            this.gbStrings.Size = new System.Drawing.Size(250, 120);
            this.gbStrings.TabIndex = 0;
            this.gbStrings.TabStop = false;
            this.gbStrings.Text = "Textos";
            // 
            // dgvStrings
            // 
            this.dgvStrings.AllowUserToAddRows = false;
            this.dgvStrings.AllowUserToDeleteRows = false;
            this.dgvStrings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStrings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StringHA,
            this.StringRefCount,
            this.StringLen,
            this.StringValue});
            this.dgvStrings.ContextMenuStrip = this.mnuStackView;
            this.dgvStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStrings.Location = new System.Drawing.Point(3, 16);
            this.dgvStrings.Name = "dgvStrings";
            this.dgvStrings.ReadOnly = true;
            this.dgvStrings.Size = new System.Drawing.Size(244, 101);
            this.dgvStrings.TabIndex = 3;
            // 
            // StringHA
            // 
            this.StringHA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringHA.HeaderText = "HA";
            this.StringHA.Name = "StringHA";
            this.StringHA.ReadOnly = true;
            this.StringHA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.StringHA.ToolTipText = "Endereço Host";
            this.StringHA.Width = 28;
            // 
            // StringRefCount
            // 
            this.StringRefCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringRefCount.HeaderText = "#Referências";
            this.StringRefCount.Name = "StringRefCount";
            this.StringRefCount.ReadOnly = true;
            this.StringRefCount.ToolTipText = "Número de Referências";
            this.StringRefCount.Width = 96;
            // 
            // StringLen
            // 
            this.StringLen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringLen.HeaderText = "Tamanho";
            this.StringLen.Name = "StringLen";
            this.StringLen.ReadOnly = true;
            this.StringLen.ToolTipText = "Comprimento do Texto";
            this.StringLen.Width = 77;
            // 
            // StringValue
            // 
            this.StringValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringValue.HeaderText = "Valor";
            this.StringValue.Name = "StringValue";
            this.StringValue.ReadOnly = true;
            this.StringValue.Width = 37;
            // 
            // gbRegisters
            // 
            this.gbRegisters.Controls.Add(this.lblRegisters);
            this.gbRegisters.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbRegisters.Location = new System.Drawing.Point(3, 3);
            this.gbRegisters.Name = "gbRegisters";
            this.gbRegisters.Size = new System.Drawing.Size(645, 46);
            this.gbRegisters.TabIndex = 2;
            this.gbRegisters.TabStop = false;
            this.gbRegisters.Text = "Registradores";
            // 
            // lblRegisters
            // 
            this.lblRegisters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRegisters.Location = new System.Drawing.Point(7, 16);
            this.lblRegisters.Name = "lblRegisters";
            this.lblRegisters.Size = new System.Drawing.Size(632, 24);
            this.lblRegisters.TabIndex = 3;
            // 
            // statusBar
            // 
            this.statusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progress,
            this.statusText});
            this.statusBar.Location = new System.Drawing.Point(0, 596);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1462, 22);
            this.statusBar.TabIndex = 13;
            // 
            // progress
            // 
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(100, 16);
            this.progress.Step = 1;
            this.progress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progress.Visible = false;
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(0, 17);
            // 
            // toolBar
            // 
            this.toolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnReload,
            this.btnSave,
            this.toolStripSeparator,
            this.btnCompile,
            this.toolStripSeparator2,
            this.btnRun,
            this.btnPause,
            this.btnStop,
            this.toolStripSeparator3,
            this.btnStepOver,
            this.btnStepInto,
            this.btnStepOut,
            this.btnRunToCursor,
            this.toolStripSeparator4,
            this.btnToggleBreakpoint});
            this.toolBar.Location = new System.Drawing.Point(0, 24);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(1462, 27);
            this.toolBar.TabIndex = 14;
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(24, 24);
            this.btnNew.Text = "&Novo";
            this.btnNew.Click += new System.EventHandler(this.BtnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(24, 24);
            this.btnOpen.Text = "&Abrir";
            this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            // 
            // btnReload
            // 
            this.btnReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReload.Image = ((System.Drawing.Image)(resources.GetObject("btnReload.Image")));
            this.btnReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(24, 24);
            this.btnReload.Text = "&Recarregar";
            this.btnReload.ToolTipText = "Recarregar";
            this.btnReload.Click += new System.EventHandler(this.BtnReload_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(24, 24);
            this.btnSave.Text = "&Salvar";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 27);
            // 
            // btnCompile
            // 
            this.btnCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCompile.Enabled = false;
            this.btnCompile.Image = ((System.Drawing.Image)(resources.GetObject("btnCompile.Image")));
            this.btnCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(24, 24);
            this.btnCompile.Text = "&Compilar";
            this.btnCompile.ToolTipText = "Compilar";
            this.btnCompile.Click += new System.EventHandler(this.BtnCompile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Enabled = false;
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(24, 24);
            this.btnRun.Text = "&Executar";
            this.btnRun.ToolTipText = "Executar";
            this.btnRun.Click += new System.EventHandler(this.BtnRun_Click);
            // 
            // btnPause
            // 
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPause.Enabled = false;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(24, 24);
            this.btnPause.Text = "&Pausar";
            this.btnPause.ToolTipText = "Pausar";
            this.btnPause.Click += new System.EventHandler(this.BtnPause_Click);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Enabled = false;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(24, 24);
            this.btnStop.Text = "Parar";
            this.btnStop.ToolTipText = "Parar";
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnStepOver
            // 
            this.btnStepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepOver.Enabled = false;
            this.btnStepOver.Image = ((System.Drawing.Image)(resources.GetObject("btnStepOver.Image")));
            this.btnStepOver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepOver.Name = "btnStepOver";
            this.btnStepOver.Size = new System.Drawing.Size(24, 24);
            this.btnStepOver.Text = "Pular Função";
            this.btnStepOver.ToolTipText = "Pular Função";
            this.btnStepOver.Click += new System.EventHandler(this.BtnStepOver_Click);
            // 
            // btnStepInto
            // 
            this.btnStepInto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepInto.Enabled = false;
            this.btnStepInto.Image = ((System.Drawing.Image)(resources.GetObject("btnStepInto.Image")));
            this.btnStepInto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepInto.Name = "btnStepInto";
            this.btnStepInto.Size = new System.Drawing.Size(24, 24);
            this.btnStepInto.Text = "Entrar na Função";
            this.btnStepInto.ToolTipText = "Intervir";
            this.btnStepInto.Click += new System.EventHandler(this.BtnStepInto_Click);
            // 
            // btnStepOut
            // 
            this.btnStepOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepOut.Enabled = false;
            this.btnStepOut.Image = ((System.Drawing.Image)(resources.GetObject("btnStepOut.Image")));
            this.btnStepOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepOut.Name = "btnStepOut";
            this.btnStepOut.Size = new System.Drawing.Size(24, 24);
            this.btnStepOut.Text = "Sair da Função";
            this.btnStepOut.ToolTipText = "Sair da Função";
            this.btnStepOut.Click += new System.EventHandler(this.BtnStepReturn_Click);
            // 
            // btnRunToCursor
            // 
            this.btnRunToCursor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRunToCursor.Enabled = false;
            this.btnRunToCursor.Image = ((System.Drawing.Image)(resources.GetObject("btnRunToCursor.Image")));
            this.btnRunToCursor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRunToCursor.Name = "btnRunToCursor";
            this.btnRunToCursor.Size = new System.Drawing.Size(24, 24);
            this.btnRunToCursor.Text = "Executar até o Cursor";
            this.btnRunToCursor.ToolTipText = "Executar até o Cursor";
            this.btnRunToCursor.Click += new System.EventHandler(this.BtnRunToCursor_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnToggleBreakpoint
            // 
            this.btnToggleBreakpoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnToggleBreakpoint.Image = ((System.Drawing.Image)(resources.GetObject("btnToggleBreakpoint.Image")));
            this.btnToggleBreakpoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnToggleBreakpoint.Name = "btnToggleBreakpoint";
            this.btnToggleBreakpoint.Size = new System.Drawing.Size(24, 24);
            this.btnToggleBreakpoint.Text = "Alternar Ponto de Interrupção";
            this.btnToggleBreakpoint.ToolTipText = "Alternar Ponto de Interrupção";
            this.btnToggleBreakpoint.Click += new System.EventHandler(this.BtnToggleBreakpoint_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Arquivos de código fonte|*.sl|Todos os arquivos|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Arquivos de código fonte|*.sl|Todos os arquivos|*.*";
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "BreakpointEnabled");
            this.imageList.Images.SetKeyName(1, "BreakpointOff");
            // 
            // bwCompiler
            // 
            this.bwCompiler.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BwCompiler_DoWork);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuView,
            this.mnuDebug,
            this.mnuHelp});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(1462, 24);
            this.mnuMain.TabIndex = 6;
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNew,
            this.mnuOpen,
            this.mnuSave,
            this.mnuSaveAs,
            this.mnuReload,
            this.mnuCloseFile,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(61, 20);
            this.mnuFile.Text = "Arquivo";
            // 
            // mnuNew
            // 
            this.mnuNew.Name = "mnuNew";
            this.mnuNew.Size = new System.Drawing.Size(148, 22);
            this.mnuNew.Text = "Novo";
            this.mnuNew.Click += new System.EventHandler(this.MnuNew_Click);
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.Size = new System.Drawing.Size(148, 22);
            this.mnuOpen.Text = "Abir";
            this.mnuOpen.Click += new System.EventHandler(this.MnuOpen_Click);
            // 
            // mnuSave
            // 
            this.mnuSave.Name = "mnuSave";
            this.mnuSave.Size = new System.Drawing.Size(148, 22);
            this.mnuSave.Text = "Salvar";
            this.mnuSave.Click += new System.EventHandler(this.MnuSave_Click);
            // 
            // mnuSaveAs
            // 
            this.mnuSaveAs.Name = "mnuSaveAs";
            this.mnuSaveAs.Size = new System.Drawing.Size(148, 22);
            this.mnuSaveAs.Text = "Salvar como...";
            this.mnuSaveAs.Click += new System.EventHandler(this.MnuSaveAs_Click);
            // 
            // mnuReload
            // 
            this.mnuReload.Name = "mnuReload";
            this.mnuReload.Size = new System.Drawing.Size(148, 22);
            this.mnuReload.Text = "Recarregar";
            this.mnuReload.Click += new System.EventHandler(this.MnuReload_Click);
            // 
            // mnuCloseFile
            // 
            this.mnuCloseFile.Name = "mnuCloseFile";
            this.mnuCloseFile.Size = new System.Drawing.Size(148, 22);
            this.mnuCloseFile.Text = "Fechar";
            this.mnuCloseFile.Click += new System.EventHandler(this.MnuCloseFile_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(148, 22);
            this.mnuExit.Text = "Sair";
            this.mnuExit.Click += new System.EventHandler(this.MnuExit_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditCut,
            this.mnuEditCopy,
            this.mnuEditPaste,
            this.mnuEditDelete,
            this.mnuEditSelectAll});
            this.mnuEdit.Name = "mnuEdit";
            this.mnuEdit.Size = new System.Drawing.Size(49, 20);
            this.mnuEdit.Text = "Editar";
            // 
            // mnuEditCut
            // 
            this.mnuEditCut.Name = "mnuEditCut";
            this.mnuEditCut.Size = new System.Drawing.Size(158, 22);
            this.mnuEditCut.Text = "Recortar";
            this.mnuEditCut.Click += new System.EventHandler(this.MnuEditCut_Click);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Name = "mnuEditCopy";
            this.mnuEditCopy.Size = new System.Drawing.Size(158, 22);
            this.mnuEditCopy.Text = "Copiar";
            this.mnuEditCopy.Click += new System.EventHandler(this.MnuEditCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Name = "mnuEditPaste";
            this.mnuEditPaste.Size = new System.Drawing.Size(158, 22);
            this.mnuEditPaste.Text = "Colar";
            this.mnuEditPaste.Click += new System.EventHandler(this.MnuEditPaste_Click);
            // 
            // mnuEditDelete
            // 
            this.mnuEditDelete.Name = "mnuEditDelete";
            this.mnuEditDelete.Size = new System.Drawing.Size(158, 22);
            this.mnuEditDelete.Text = "Excluir";
            this.mnuEditDelete.Click += new System.EventHandler(this.MnuEditDelete_Click);
            // 
            // mnuEditSelectAll
            // 
            this.mnuEditSelectAll.Name = "mnuEditSelectAll";
            this.mnuEditSelectAll.Size = new System.Drawing.Size(158, 22);
            this.mnuEditSelectAll.Text = "Selecionar Tudo";
            this.mnuEditSelectAll.Click += new System.EventHandler(this.MnuEditSelectAll_Click);
            // 
            // mnuView
            // 
            this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewCode,
            this.mnuViewConsole,
            this.mnuViewAssembly,
            this.mnuViewVariables,
            this.mnuViewMemory});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(48, 20);
            this.mnuView.Text = "Exibir";
            // 
            // mnuViewCode
            // 
            this.mnuViewCode.Checked = true;
            this.mnuViewCode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewCode.Name = "mnuViewCode";
            this.mnuViewCode.Size = new System.Drawing.Size(125, 22);
            this.mnuViewCode.Text = "Código";
            this.mnuViewCode.Click += new System.EventHandler(this.MnuViewCode_Click);
            // 
            // mnuViewConsole
            // 
            this.mnuViewConsole.Checked = true;
            this.mnuViewConsole.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewConsole.Name = "mnuViewConsole";
            this.mnuViewConsole.Size = new System.Drawing.Size(125, 22);
            this.mnuViewConsole.Text = "Console";
            this.mnuViewConsole.Click += new System.EventHandler(this.MnuViewConsole_Click);
            // 
            // mnuViewAssembly
            // 
            this.mnuViewAssembly.Checked = true;
            this.mnuViewAssembly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewAssembly.Name = "mnuViewAssembly";
            this.mnuViewAssembly.Size = new System.Drawing.Size(125, 22);
            this.mnuViewAssembly.Text = "Assembly";
            this.mnuViewAssembly.Click += new System.EventHandler(this.MnuViewAssembly_Click);
            // 
            // mnuViewVariables
            // 
            this.mnuViewVariables.Checked = true;
            this.mnuViewVariables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewVariables.Name = "mnuViewVariables";
            this.mnuViewVariables.Size = new System.Drawing.Size(125, 22);
            this.mnuViewVariables.Text = "Variáveis";
            this.mnuViewVariables.Click += new System.EventHandler(this.MnuViewVariables_Click);
            // 
            // mnuViewMemory
            // 
            this.mnuViewMemory.Checked = true;
            this.mnuViewMemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewMemory.Name = "mnuViewMemory";
            this.mnuViewMemory.Size = new System.Drawing.Size(125, 22);
            this.mnuViewMemory.Text = "Memória";
            this.mnuViewMemory.Click += new System.EventHandler(this.MnuViewMemory_Click);
            // 
            // mnuDebug
            // 
            this.mnuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRun,
            this.mnuPause,
            this.mnuStop,
            this.mnuStepOver,
            this.mnuStepInto,
            this.mnuStepOut,
            this.mnuRunToCursor,
            this.mnuToggleBreakpoint});
            this.mnuDebug.Name = "mnuDebug";
            this.mnuDebug.Size = new System.Drawing.Size(61, 20);
            this.mnuDebug.Text = "Depurar";
            // 
            // mnuRun
            // 
            this.mnuRun.Enabled = false;
            this.mnuRun.Image = global::SimpleCompiler.Properties.Resources.run;
            this.mnuRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuRun.Name = "mnuRun";
            this.mnuRun.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuRun.Size = new System.Drawing.Size(238, 22);
            this.mnuRun.Text = "Executar";
            this.mnuRun.Click += new System.EventHandler(this.MnuRun_Click);
            // 
            // mnuPause
            // 
            this.mnuPause.Enabled = false;
            this.mnuPause.Image = global::SimpleCompiler.Properties.Resources.pause;
            this.mnuPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuPause.Name = "mnuPause";
            this.mnuPause.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.Pause)));
            this.mnuPause.Size = new System.Drawing.Size(238, 22);
            this.mnuPause.Text = "Pausar";
            this.mnuPause.Click += new System.EventHandler(this.MnuPause_Click);
            // 
            // mnuStop
            // 
            this.mnuStop.Enabled = false;
            this.mnuStop.Image = global::SimpleCompiler.Properties.Resources.stop;
            this.mnuStop.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.mnuStop.Name = "mnuStop";
            this.mnuStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.mnuStop.Size = new System.Drawing.Size(238, 22);
            this.mnuStop.Text = "Parar";
            this.mnuStop.Click += new System.EventHandler(this.MnuStop_Click);
            // 
            // mnuStepOver
            // 
            this.mnuStepOver.Enabled = false;
            this.mnuStepOver.Image = global::SimpleCompiler.Properties.Resources.StepOver_6328;
            this.mnuStepOver.Name = "mnuStepOver";
            this.mnuStepOver.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.mnuStepOver.Size = new System.Drawing.Size(238, 22);
            this.mnuStepOver.Text = "Pular Função";
            this.mnuStepOver.Click += new System.EventHandler(this.MnuStepOver_Click);
            // 
            // mnuStepInto
            // 
            this.mnuStepInto.Enabled = false;
            this.mnuStepInto.Image = global::SimpleCompiler.Properties.Resources.StepIn_6326;
            this.mnuStepInto.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.mnuStepInto.Name = "mnuStepInto";
            this.mnuStepInto.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.mnuStepInto.Size = new System.Drawing.Size(238, 22);
            this.mnuStepInto.Text = "Intervir";
            this.mnuStepInto.Click += new System.EventHandler(this.MnuStepInto_Click);
            // 
            // mnuStepOut
            // 
            this.mnuStepOut.Enabled = false;
            this.mnuStepOut.Image = global::SimpleCompiler.Properties.Resources.Stepout_6327;
            this.mnuStepOut.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.mnuStepOut.Name = "mnuStepOut";
            this.mnuStepOut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F11)));
            this.mnuStepOut.Size = new System.Drawing.Size(238, 22);
            this.mnuStepOut.Text = "Sair da Função";
            this.mnuStepOut.Click += new System.EventHandler(this.MnuStepOut_Click);
            // 
            // mnuRunToCursor
            // 
            this.mnuRunToCursor.Enabled = false;
            this.mnuRunToCursor.Image = global::SimpleCompiler.Properties.Resources.run_to_cursor;
            this.mnuRunToCursor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuRunToCursor.Name = "mnuRunToCursor";
            this.mnuRunToCursor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F11)));
            this.mnuRunToCursor.Size = new System.Drawing.Size(238, 22);
            this.mnuRunToCursor.Text = "Executar até o Cursor";
            this.mnuRunToCursor.Click += new System.EventHandler(this.MnuRunToCursor_Click);
            // 
            // mnuToggleBreakpoint
            // 
            this.mnuToggleBreakpoint.Image = global::SimpleCompiler.Properties.Resources.BreakpointEnabled_6584_32x;
            this.mnuToggleBreakpoint.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.mnuToggleBreakpoint.Name = "mnuToggleBreakpoint";
            this.mnuToggleBreakpoint.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.mnuToggleBreakpoint.Size = new System.Drawing.Size(238, 22);
            this.mnuToggleBreakpoint.Text = "Alternar Breakpoint";
            this.mnuToggleBreakpoint.Click += new System.EventHandler(this.MnuToggleBreakpoint_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(50, 20);
            this.mnuHelp.Text = "Ajuda";
            // 
            // mnuAbout
            // 
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(113, 22);
            this.mnuAbout.Text = "Sobre...";
            this.mnuAbout.Click += new System.EventHandler(this.MnuAbout_Click);
            // 
            // FrmSimpleCompiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1462, 618);
            this.Controls.Add(this.scMain);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.mnuMain);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.mnuMain;
            this.Name = "FrmSimpleCompiler";
            this.Text = "Simple Compiler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSimpleCompiler_FormClosing);
            this.Load += new System.EventHandler(this.FrmSimpleCompiler_Load);
            this.scMain.Panel1.ResumeLayout(false);
            this.scMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
            this.scMain.ResumeLayout(false);
            this.scCode.Panel1.ResumeLayout(false);
            this.scCode.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scCode)).EndInit();
            this.scCode.ResumeLayout(false);
            this.tcRight.ResumeLayout(false);
            this.tpAssembly.ResumeLayout(false);
            this.scBottom.Panel1.ResumeLayout(false);
            this.scBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scBottom)).EndInit();
            this.scBottom.ResumeLayout(false);
            this.mnuConsole.ResumeLayout(false);
            this.tcDebugView.ResumeLayout(false);
            this.tpVariables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).EndInit();
            this.tpMemory.ResumeLayout(false);
            this.splitContainerData.Panel1.ResumeLayout(false);
            this.splitContainerData.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).EndInit();
            this.splitContainerData.ResumeLayout(false);
            this.gbStack.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStack)).EndInit();
            this.mnuStackView.ResumeLayout(false);
            this.gbStrings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStrings)).EndInit();
            this.gbRegisters.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.SplitContainer scMain;
    private System.Windows.Forms.SplitContainer scCode;
    internal System.Windows.Forms.TabControl tcSources;
    private System.Windows.Forms.TabControl tcRight;
    private System.Windows.Forms.TabPage tpAssembly;
    private System.Windows.Forms.DataGridView dgvVariables;
    private System.Windows.Forms.RichTextBox txtConsole;
    private System.Windows.Forms.StatusStrip statusBar;
    private System.Windows.Forms.ToolStripProgressBar progress;
    private System.Windows.Forms.ToolStripStatusLabel statusText;
    private System.Windows.Forms.ToolStrip toolBar;
    private System.Windows.Forms.ToolStripButton btnNew;
    private System.Windows.Forms.ToolStripButton btnOpen;
    private System.Windows.Forms.ToolStripButton btnReload;
    private System.Windows.Forms.ToolStripButton btnSave;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
    private System.Windows.Forms.ToolStripButton btnCompile;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripButton btnRun;
    private System.Windows.Forms.ToolStripButton btnPause;
    private System.Windows.Forms.ToolStripButton btnStop;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripButton btnStepOver;
    private System.Windows.Forms.ToolStripButton btnStepInto;
    private System.Windows.Forms.ToolStripButton btnStepOut;
    private System.Windows.Forms.ToolStripButton btnRunToCursor;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    private System.Windows.Forms.ToolStripButton btnToggleBreakpoint;
    internal System.Windows.Forms.SaveFileDialog saveFileDialog;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.ImageList imageList;
    private System.ComponentModel.BackgroundWorker bwCompiler;
    private System.Windows.Forms.Integration.ElementHost wpfAssemblyHost;
    private System.Windows.Forms.SplitContainer scBottom;
    private System.Windows.Forms.DataGridView dgvStack;
    private System.Windows.Forms.ContextMenuStrip mnuStackView;
    private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign16;
    private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign8;
    private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign4;
    private System.Windows.Forms.SplitContainer splitContainerData;
    private System.Windows.Forms.GroupBox gbStack;
    private System.Windows.Forms.GroupBox gbStrings;
    private System.Windows.Forms.DataGridView dgvStrings;
    private System.Windows.Forms.DataGridViewTextBoxColumn RA;
    private System.Windows.Forms.DataGridViewTextBoxColumn HA;
    private System.Windows.Forms.DataGridViewButtonColumn Value;
    private System.Windows.Forms.DataGridViewTextBoxColumn View;
    private System.Windows.Forms.DataGridViewTextBoxColumn StringHA;
    private System.Windows.Forms.DataGridViewTextBoxColumn StringRefCount;
    private System.Windows.Forms.DataGridViewTextBoxColumn StringLen;
    private System.Windows.Forms.DataGridViewButtonColumn StringValue;
    private System.Windows.Forms.GroupBox gbRegisters;
    private System.Windows.Forms.Label lblRegisters;
    private System.Windows.Forms.MenuStrip mnuMain;
    private System.Windows.Forms.ToolStripMenuItem mnuFile;
    private System.Windows.Forms.ToolStripMenuItem mnuEdit;
    private System.Windows.Forms.ToolStripMenuItem mnuView;
    private System.Windows.Forms.ToolStripMenuItem mnuDebug;
    private System.Windows.Forms.ToolStripMenuItem mnuHelp;
    private System.Windows.Forms.ToolStripMenuItem mnuNew;
    private System.Windows.Forms.ToolStripMenuItem mnuOpen;
    private System.Windows.Forms.ToolStripMenuItem mnuSave;
    private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
    private System.Windows.Forms.ToolStripMenuItem mnuReload;
    private System.Windows.Forms.ToolStripMenuItem mnuCloseFile;
    private System.Windows.Forms.ToolStripMenuItem mnuExit;
    private System.Windows.Forms.ToolStripMenuItem mnuEditCut;
    private System.Windows.Forms.ToolStripMenuItem mnuEditCopy;
    private System.Windows.Forms.ToolStripMenuItem mnuEditPaste;
    private System.Windows.Forms.ToolStripMenuItem mnuEditDelete;
    private System.Windows.Forms.ToolStripMenuItem mnuViewCode;
    private System.Windows.Forms.ToolStripMenuItem mnuViewConsole;
    private System.Windows.Forms.ToolStripMenuItem mnuRun;
    private System.Windows.Forms.ToolStripMenuItem mnuPause;
    private System.Windows.Forms.ToolStripMenuItem mnuStop;
    private System.Windows.Forms.ToolStripMenuItem mnuStepOver;
    private System.Windows.Forms.ToolStripMenuItem mnuStepInto;
    private System.Windows.Forms.ToolStripMenuItem mnuStepOut;
    private System.Windows.Forms.ToolStripMenuItem mnuRunToCursor;
    private System.Windows.Forms.ToolStripMenuItem mnuAbout;
    private System.Windows.Forms.ToolStripMenuItem mnuToggleBreakpoint;
    private System.Windows.Forms.TabControl tcDebugView;
    private System.Windows.Forms.TabPage tpVariables;
    private System.Windows.Forms.TabPage tpMemory;
    private System.Windows.Forms.ToolStripMenuItem mnuViewAssembly;
    private System.Windows.Forms.ToolStripMenuItem mnuViewVariables;
    private System.Windows.Forms.ToolStripMenuItem mnuViewMemory;
    private System.Windows.Forms.ToolStripMenuItem mnuEditSelectAll;
    private System.Windows.Forms.ContextMenuStrip mnuConsole;
    private System.Windows.Forms.ToolStripMenuItem mnuConsoleClear;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
    private System.Windows.Forms.DataGridViewButtonColumn columnValue;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnRA;
    private System.Windows.Forms.DataGridViewTextBoxColumn columnHA;
}

