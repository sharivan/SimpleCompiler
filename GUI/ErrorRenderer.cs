using System.Windows;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Rendering;

namespace SimpleCompiler.GUI;

internal class ErrorRenderer(TextView view, bool enabled = false, int start = -1, int end = -1) : IBackgroundRenderer
{
    private static readonly Pen pen;
    private static readonly SolidColorBrush errorBackground;

    static ErrorRenderer()
    {
        errorBackground = new SolidColorBrush(Color.FromArgb(0x22, 0xff, 0x00, 0x00));
        errorBackground.Freeze();

        var blackBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x00, 0x00));
        blackBrush.Freeze();
        pen = new Pen(blackBrush, 0.0);
    }

    private readonly TextView view = view;
    private bool enabled = enabled;
    private int start = start;
    private int end = end;
    private string message;

    public bool Enabled
    {
        get => enabled;

        set
        {
            enabled = value;
            view.InvalidateLayer(KnownLayer.Background);
        }
    }

    public int Start
    {
        get => start;

        set
        {
            start = value;
            view.InvalidateLayer(KnownLayer.Background);
        }
    }

    public int End
    {
        get => end;

        set
        {
            end = value;
            view.InvalidateLayer(KnownLayer.Background);
        }
    }

    public string Message
    {
        get => message;

        set
        {
            message = value;
            view.InvalidateLayer(KnownLayer.Background);
        }
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!enabled)
            return;

        foreach (var v in textView.VisualLines)
        {
            int s = -1;
            int e = -1;

            if (v.FirstDocumentLine.Offset <= start && start < v.FirstDocumentLine.EndOffset)
            {
                s = start;
                e = end <= v.FirstDocumentLine.EndOffset ? end : v.FirstDocumentLine.EndOffset;
            }
            else if (v.FirstDocumentLine.Offset >= start && v.FirstDocumentLine.Offset < end)
            {
                s = v.FirstDocumentLine.Offset;
                e = end <= v.FirstDocumentLine.EndOffset ? end : v.FirstDocumentLine.EndOffset;
            }

            if (s != -1)
            {
                var rcs = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, s - v.FirstDocumentLine.Offset, e - v.FirstDocumentLine.Offset);
                foreach (var rc in rcs)
                {
                    var brush = errorBackground;
                    drawingContext.DrawRectangle(brush, pen, new Rect(rc.Left, rc.Top, rc.Width, rc.Height));
                }
            }
        }
    }
}