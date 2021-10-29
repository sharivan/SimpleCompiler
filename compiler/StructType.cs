using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class StructType : AbstractType
    {
        private string name;
        private List<Field> fields;
        private int size;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public int FieldCount
        {
            get
            {
                return fields.Count;
            }
        }

        public Field this[int index]
        {
            get
            {
                return fields[index];
            }
        }

        public StructType(string name)
        {
            this.name = name;

            fields = new List<Field>();
            size = 0;
        }

        public Field FindField(string name)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                Field field = fields[i];
                if (field.Name == name)
                    return field;
            }

            return null;
        }

        public Field DeclareField(string name, AbstractType type)
        {
            Field result = FindField(name);
            if (result != null)
                return null;

            result = new Field(this, name, type, size);
            size += type.Size();
            fields.Add(result);
            return result;
        }

        public override string ToString()
        {
            string result = "estrutura " + name + "\n{\n";

            for (int i = 0; i < fields.Count; i++)
                result += "  " + fields[i] + "\n";

            return result + "}";
        }

        public override int Size()
        {
            return size;
        }
    }
}
