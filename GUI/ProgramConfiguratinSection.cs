using System.Configuration;

namespace SimpleCompiler.GUI;

public sealed class ProgramConfiguratinSection : ConfigurationSection
{
    public ProgramConfiguratinSection()
    {
    }

    [ConfigurationProperty("left",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Left
    {
        get => (int) this["left"];

        set => this["left"] = value;
    }

    [ConfigurationProperty("top",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Top
    {
        get => (int) this["top"];

        set => this["top"] = value;
    }

    [ConfigurationProperty("width",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Width
    {
        get => (int) this["width"];

        set => this["width"] = value;
    }

    [ConfigurationProperty("height",
        DefaultValue = -1,
        IsRequired = false
        )]
    public int Height
    {
        get => (int) this["height"];

        set => this["height"] = value;
    }

    [ConfigurationProperty("maximized",
        DefaultValue = false,
        IsRequired = false
        )]
    public bool Maximized
    {
        get => (bool) this["maximized"];

        set => this["maximized"] = value;
    }

    [ConfigurationProperty("stackviewalignsize",
        DefaultValue = 16,
        IsRequired = false
        )]
    public int StackViewAlignSize
    {
        get => (int) this["stackviewalignsize"];

        set => this["stackviewalignsize"] = value;
    }

    [ConfigurationProperty("viewCodeChecked",
        DefaultValue = true,
        IsRequired = false
        )]
    public bool ViewCodeChecked
    {
        get => (bool) this["viewCodeChecked"];

        set => this["viewCodeChecked"] = value;
    }

    [ConfigurationProperty("viewConsoleChecked",
        DefaultValue = true,
        IsRequired = false
        )]
    public bool ViewConsoleChecked
    {
        get => (bool) this["viewConsoleChecked"];

        set => this["viewConsoleChecked"] = value;
    }

    [ConfigurationProperty("viewAssemblyChecked",
        DefaultValue = false,
        IsRequired = false
        )]
    public bool ViewAssemblyChecked
    {
        get => (bool) this["viewAssemblyChecked"];

        set => this["viewAssemblyChecked"] = value;
    }

    [ConfigurationProperty("viewVariablesChecked",
        DefaultValue = true,
        IsRequired = false
        )]
    public bool ViewVariablesChecked
    {
        get => (bool) this["viewVariablesChecked"];

        set => this["viewVariablesChecked"] = value;
    }

    [ConfigurationProperty("viewMemoryChecked",
        DefaultValue = false,
        IsRequired = false
        )]
    public bool ViewMemoryChecked
    {
        get => (bool) this["viewMemoryChecked"];

        set => this["viewMemoryChecked"] = value;
    }

    [ConfigurationProperty("consoleZoomFactor",
        DefaultValue = 1F,
        IsRequired = false
        )]
    public float ConsoleZoomFactor
    {
        get => (float) this["consoleZoomFactor"];

        set => this["consoleZoomFactor"] = value;
    }

    [ConfigurationProperty("assemblyZoomFactor",
        DefaultValue = 1.0,
        IsRequired = false
        )]
    public double AssemblyZoomFactor
    {
        get => (double) this["assemblyZoomFactor"];

        set => this["assemblyZoomFactor"] = value;
    }

    [ConfigurationProperty("documents", IsDefaultCollection = true)]
    public DocumentCollection Documents => (DocumentCollection) base["documents"];
}