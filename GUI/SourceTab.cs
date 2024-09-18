using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleCompiler.GUI;

public class SourceTab
{
    private FrmSimpleCompiler form;
    internal TabPage page;
    internal string m_SourceCode;
    private List<Line> m_Lines;

    internal ElementHost wpfHost;
    internal TextEditor txtSource;

    internal ErrorRenderer errorBackgroundRenderer;
    internal SteppingRenderer lineBackgroundRenderer;
    internal BreakPointMargin breakpointMargin;

    public int ID { get; private set; }

    public string FileName { get; private set; }

    public bool Saved { get; private set; }

    public double Zoom
    {
        get => FrmSimpleCompiler.ComputeZoom(txtSource.FontSize);
        set => txtSource.FontSize = FrmSimpleCompiler.ComputeFontSize(value);
    }

    public SourceTab(FrmSimpleCompiler form, double zoom = 1)
    {
        Initialize(form, zoom);
    }

    public SourceTab(FrmSimpleCompiler form, string fileName, double zoom = 1)
    {
        Initialize(form, zoom);
        Load(fileName);
    }

    private void Initialize(FrmSimpleCompiler form, double zoom = 1)
    {
        this.form = form;

        Saved = false;
        ID = form.sourceTabNextID++;

        m_Lines = [];

        wpfHost = new ElementHost
        {
            Dock = DockStyle.Fill
        };

        page = new TabPage("Sem nome   ");
        form.tcSources.TabPages.Add(page);

        page.Controls.Add(wpfHost);

        form.enabledImage ??= new BitmapImage(new Uri(@"pack://application:,,,/resources/img/BreakpointEnabled_6584_16x.png"));

        form.disabledImage ??= new BitmapImage(new Uri(@"pack://application:,,,/resources/img/breakpoint_Off_16xMD.png"));

        txtSource = new TextEditor();
        wpfHost.Child = txtSource;
        txtSource.ShowLineNumbers = true;
        txtSource.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(txtSource.Options);
        txtSource.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".sl");
        txtSource.FontSize = FrmSimpleCompiler.ComputeFontSize(zoom);
        txtSource.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#DCDCDC"));
        txtSource.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#1E1E1E"));
        txtSource.PreviewMouseWheel += TxtSource_PreviewMouseWheel;
        txtSource.Document.Changed += TxtSource_Document_OnChanged;

        errorBackgroundRenderer = new ErrorRenderer(txtSource.TextArea.TextView);
        txtSource.TextArea.TextView.BackgroundRenderers.Add(errorBackgroundRenderer);

        lineBackgroundRenderer = new SteppingRenderer(txtSource.TextArea.TextView);
        txtSource.TextArea.TextView.BackgroundRenderers.Add(lineBackgroundRenderer);

        breakpointMargin = new BreakPointMargin(form);
        txtSource.TextArea.LeftMargins.Insert(0, breakpointMargin);
    }

    private void TxtSource_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            double fontSize = txtSource.FontSize + e.Delta / 25.0;
            Zoom = FrmSimpleCompiler.ComputeZoom(fontSize);
            e.Handled = true;
        }
    }

    private void TxtSource_Document_OnChanged(object sender, DocumentChangeEventArgs e)
    {
        int delta = e.InsertionLength - e.RemovalLength;
        var intersection = new Interval(e.Offset, e.Offset + delta) & new Interval(errorBackgroundRenderer.Start, errorBackgroundRenderer.End);

        if (intersection.Start < intersection.End)
        {
            errorBackgroundRenderer.Enabled = false;
        }
        else if (e.Offset + delta < errorBackgroundRenderer.Start)
        {
            errorBackgroundRenderer.Start += delta;
            errorBackgroundRenderer.End += delta;
        }

        Saved = false;
    }

    internal void SetSource(string sSource)
    {
        m_Lines.Clear();

        int number = 0;
        int start = 0;
        int n = sSource.Length;

        char c0 = '\0';

        for (int i = 0; i < n; i++)
        {
            char c = sSource[i];

            if (c == '\n')
            {
                m_Lines.Add(new Line(++number, start, i));
                start = i + 1;
            }
            else if (c == '\r')
            {
                if (c0 == '\n')
                    start = i + 1;
            }

            c0 = c;
        }

        m_Lines.Add(new Line(++number, start, n));
    }

    public void Load(string fileName)
    {
        this.FileName = fileName;
        if (File.Exists(fileName))
        {
            page.Text = Path.GetFileName(fileName) + "    ";
            txtSource.Load(fileName);
            Saved = true;
        }
        else
        {
            MessageBox.Show($"Não foi possível recarregar o arquivo '{fileName}'.");
        }
    }

    public void Reload()
    {
        if (FileName != null)
            Load(FileName);
    }

    public void Save()
    {
        if (FileName != null)
        {
            page.Text = $"{Path.GetFileName(FileName)}    ";
            txtSource.Save(FileName);
            Saved = true;
        }
        else
        {
            OpenSaveDialog();
        }
    }

    public void OpenSaveDialog()
    {
        var result = form.saveFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            string sSourceFileName = form.saveFileDialog.FileName;

            if (sSourceFileName != FileName && File.Exists(sSourceFileName))
                form.CloseSourceTab(sSourceFileName);

            FileName = sSourceFileName;
            Save();
        }
    }

    public void Clear()
    {
        FileName = null;
        page.Text = "Sem nome    ";
        txtSource.Clear();
        Saved = false;
    }

    public void Focus()
    {
        form.tcSources.SelectedTab = page;
        txtSource.Focus();
    }

    public void Close()
    {
        form.tcSources.TabPages.Remove(page);

        form.sourceTabs.Remove(this);
        form.sourceTabsMapByID.Remove(ID);

        if (FileName != null)
            form.sourceTabsMapByFileName.Remove(FileName);
    }

    public void Cut()
    {
        txtSource.Cut();
    }

    public void Copy()
    {
        txtSource.Copy();
    }

    public void Paste()
    {
        txtSource.Paste();
    }

    public void EditDelete()
    {
        txtSource.Delete();
    }

    public void SelectAll()
    {
        txtSource.SelectAll();
    }
}