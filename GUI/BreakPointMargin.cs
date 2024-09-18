using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VM;

namespace SimpleCompiler.GUI;

internal class BreakPointMargin(FrmSimpleCompiler form) : AbstractMargin
{
    private const int margin = 20;

    private readonly FrmSimpleCompiler form = form;
    private readonly Dictionary<int, Breakpoint> breakpoints = [];

    public KnownLayer Layer => KnownLayer.Background;

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new(margin, 0);
    }

    #region OnTextViewChanged
    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView != null)
        {
            oldTextView.VisualLinesChanged -= OnRedrawRequested;
        }

        base.OnTextViewChanged(oldTextView, newTextView);

        if (newTextView != null)
        {
            newTextView.VisualLinesChanged += OnRedrawRequested;
        }

        InvalidateVisual();
    }

    void OnRedrawRequested(object sender, EventArgs e)
    {
        // Don't invalidate the IconBarMargin if it'll be invalidated again once the
        // visual lines become valid.
        if (TextView != null && TextView.VisualLinesValid)
        {
            InvalidateVisual();
        }
    }

    public virtual void Dispose()
    {
        TextView = null; // detach from TextView (will also detach from manager)
    }
    #endregion

    protected override void OnRender(DrawingContext drawingContext)
    {
        var textView = TextView;
        var renderSize = RenderSize;
        drawingContext.DrawRectangle(SystemColors.ControlBrush, null, new Rect(0, 0, renderSize.Width, renderSize.Height));

        if (textView != null && textView.VisualLinesValid)
        {
            foreach (var line in textView.VisualLines)
            {
                if (breakpoints.ContainsKey(line.FirstDocumentLine.LineNumber))
                    drawingContext.DrawImage(form.enabledImage, new Rect(0, line.VisualTop - textView.VerticalOffset, form.enabledImage.Width, form.enabledImage.Height));
            }
        }
    }

    public void AddBreakpoint(int line, Breakpoint breakpoint)
    {
        breakpoints[line] = breakpoint;
        InvalidateVisual();
    }

    public void ToggleBreakpoint(int line, Breakpoint breakpoint)
    {
        if (!breakpoints.ContainsKey(line))
            breakpoints[line] = breakpoint;
        else if (breakpoints[line] != breakpoint)
            breakpoints[line] = breakpoint;
        else
            breakpoints.Remove(line);

        InvalidateVisual();
    }

    public void RemoveBreakpoint(int line)
    {
        breakpoints.Remove(line);
        InvalidateVisual();
    }

    public void ClearBreakpoints()
    {
        breakpoints.Clear();
        InvalidateVisual();
    }
}