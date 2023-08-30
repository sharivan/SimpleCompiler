using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SimpleCompiler.GUI;

public class SourceTab
{
    internal FrmSimpleCompiler form;
    internal TabPage page;
    internal int id;
    internal string fileName;
    internal bool saved;

    internal string m_SourceCode;
    internal List<Line> m_Lines;

    internal ElementHost wpfHost;
    internal TextEditor txtSource;

    internal ErrorRenderer errorBackgroundRenderer;
    internal SteppingRenderer lineBackgroundRenderer;
    internal BreakPointMargin breakpointMargin;

    public int ID => id;

    public string FileName => fileName;

    public bool Saved => saved;

    public SourceTab(FrmSimpleCompiler form)
    {
        Initialize(form);
    }

    public SourceTab(FrmSimpleCompiler form, string fileName)
    {
        Initialize(form);
        Load(fileName);
    }

    private void Initialize(FrmSimpleCompiler form)
    {
        this.form = form;

        saved = false;
        id = form.sourceTabNextID++;

        m_Lines = new List<Line>();

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
        txtSource.FontSize = 14;
        txtSource.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#DCDCDC"));
        txtSource.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#1E1E1E"));
        txtSource.Document.Changed += TxtSource_Document_OnChanged;

        errorBackgroundRenderer = new ErrorRenderer(txtSource.TextArea.TextView);
        txtSource.TextArea.TextView.BackgroundRenderers.Add(errorBackgroundRenderer);

        lineBackgroundRenderer = new SteppingRenderer(txtSource.TextArea.TextView);
        txtSource.TextArea.TextView.BackgroundRenderers.Add(lineBackgroundRenderer);

        breakpointMargin = new BreakPointMargin(form);
        txtSource.TextArea.LeftMargins.Insert(0, breakpointMargin);
    }

    private void TxtSource_Document_OnChanged(object sender, DocumentChangeEventArgs e)
    {
        int delta = e.InsertionLength - e.RemovalLength;
        Interval intersection = new Interval(e.Offset, e.Offset + delta) & new Interval(errorBackgroundRenderer.Start, errorBackgroundRenderer.End);

        if (intersection.Start < intersection.End)
        {
            errorBackgroundRenderer.Enabled = false;
        }
        else if (e.Offset + delta < errorBackgroundRenderer.Start)
        {
            errorBackgroundRenderer.Start += delta;
            errorBackgroundRenderer.End += delta;
        }

        saved = false;
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
        this.fileName = fileName;
        if (File.Exists(fileName))
        {
            page.Text = Path.GetFileName(fileName) + "    ";
            txtSource.Load(fileName);
            saved = true;
        }
        else
        {
            MessageBox.Show($"Não foi possível recarregar o arquivo '{fileName}'.");
        }
    }

    public void Reload()
    {
        if (fileName != null)
            Load(fileName);
    }

    public void Save()
    {
        if (fileName != null)
        {
            page.Text = $"{Path.GetFileName(fileName)}    ";
            txtSource.Save(fileName);
            saved = true;
        }
        else
        {
            OpenSaveDialog();
        }
    }

    public void OpenSaveDialog()
    {
        DialogResult result = form.saveFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            string sSourceFileName = form.saveFileDialog.FileName;

            if (sSourceFileName != FileName && File.Exists(sSourceFileName))
                form.CloseSourceTab(sSourceFileName);

            fileName = sSourceFileName;
            Save();
        }
    }

    public void Clear()
    {
        fileName = null;
        page.Text = "Sem nome    ";
        txtSource.Clear();
        saved = false;
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
        form.sourceTabsMapByID.Remove(id);

        if (fileName != null)
            form.sourceTabsMapByFileName.Remove(fileName);
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