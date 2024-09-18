using ICSharpCode.AvalonEdit.Rendering;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SimpleCompiler.GUI;

internal class SteppingRenderer(TextView view, bool enabled = false, int lineNumber = 1) : IBackgroundRenderer
{
    private static readonly Pen pen;
    private static readonly SolidColorBrush steppingBackground;

    private readonly TextView view = view;
    private bool enabled = enabled;
    private int lineNumber = lineNumber;

    static SteppingRenderer()
    {
        steppingBackground = new SolidColorBrush(Color.FromArgb(0xbe, 0x00, 0x00, 0xff));
        steppingBackground.Freeze();

        var blackBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x00, 0x00));
        blackBrush.Freeze();
        pen = new Pen(blackBrush, 0.0);
    }

    public bool Enabled
    {
        get => enabled;

        set
        {
            enabled = value;
            view.InvalidateLayer(KnownLayer.Background);
        }
    }

    public int LineNumber
    {
        get => lineNumber;

        set
        {
            lineNumber = value;
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
            if (v.FirstDocumentLine.LineNumber != lineNumber)
                continue;

            var rc = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, 0, 1000).First();

            var brush = steppingBackground;
            drawingContext.DrawRectangle(brush, pen, new Rect(0, rc.Top, textView.ActualWidth, rc.Height));

            break;
        }
    }
}