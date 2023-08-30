using System.Configuration;

namespace SimpleCompiler.GUI;

public sealed class DocumentCollection : ConfigurationElementCollection
{
    public new DocumentConfigElement this[string filename]
    {
        get
        {
            DocumentConfigElement element;
            if (IndexOf(filename) < 0)
            {
                element = new DocumentConfigElement(filename);
                BaseAdd(element);
            }
            else
            {
                element = (DocumentConfigElement) BaseGet(filename);
            }

            return element;
        }
    }

    public DocumentConfigElement this[int index] => (DocumentConfigElement) BaseGet(index);

    protected override string ElementName => "document";

    public int IndexOf(string name)
    {
        name = name.ToLower();

        for (int idx = 0; idx < Count; idx++)
        {
            if (this[idx].FileName.ToLower() == name)
                return idx;
        }

        return -1;
    }

    public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

    protected override ConfigurationElement CreateNewElement()
    {
        return new DocumentConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
        return ((DocumentConfigElement) element).FileName;
    }

    public void Clear()
    {
        BaseClear();
    }
}