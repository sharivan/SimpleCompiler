using System.Configuration;

namespace SimpleCompiler.GUI;

public sealed class DocumentConfigElement : ConfigurationElement
{
    public DocumentConfigElement()
    {
    }

    public DocumentConfigElement(string fileName)
    {
        FileName = fileName;
    }

    [ConfigurationProperty("filename", IsRequired = true, IsKey = true)]
    public string FileName
    {
        get => (string) this["filename"];
        set => this["filename"] = value;
    }

    [ConfigurationProperty("tabindex",
        IsRequired = false,
        DefaultValue = -1
        )]
    public int TabIndex
    {
        get => (int) this["tabindex"];
        set => this["tabindex"] = value;
    }

    [ConfigurationProperty("selected",
        IsRequired = false,
        DefaultValue = false
        )]
    public bool Selected
    {
        get => (bool) this["selected"];
        set => this["selected"] = value;
    }

    [ConfigurationProperty("focused",
        IsRequired = false,
        DefaultValue = false
        )]
    public bool Focused
    {
        get => (bool) this["focused"];
        set => this["focused"] = value;
    }

    [ConfigurationProperty("zoom",
        IsRequired = false,
        DefaultValue = 1.0
        )]
    public double Zoom
    {
        get => (double) this["zoom"];
        set => this["zoom"] = value;
    }
}