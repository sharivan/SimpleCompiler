using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.types
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (obj is StructType s)
            {
                string otherName = s.name;
                return name == otherName;
            }

            return false;
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

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = -1735305858;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Field>>.Default.GetHashCode(fields);
            return hashCode;
        }

        public static bool operator ==(StructType t1, StructType t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (((object) t1) == null || ((object) t2) == null)
                return false;

            return t1.Equals(t2);
        }

        public static bool operator !=(StructType t1, StructType t2)
        {
            return !(t1 == t2);
        }
    }
}
