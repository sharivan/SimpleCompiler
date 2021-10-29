using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class ArrayType : AbstractType
    {
        private AbstractType type;
        private List<int> boundaries;

        public AbstractType Type
        {
            get
            {
                return type;
            }
        }

        public int Rank
        {
            get
            {
                return boundaries.Count;
            }
        }

        public int this[int index]
        {
            get
            {
                return boundaries[index];
            }
        }

        public ArrayType(AbstractType type)
        {
            this.type = type;

            boundaries = new List<int>();
        }

        public void AddBoundary(int boundary)
        {
            boundaries.Add(boundary);
        }

        public override string ToString()
        {
            string result = type + "[";
            if (boundaries.Count > 0)
            {
                result += boundaries[0].ToString();
                for (int i = 1; i < boundaries.Count; i++)
                    result += ", " + boundaries[i];
            }

            return result + "]";
        }

        public override int Size()
        {
            int result = type.Size();
            for (int i = 0; i < boundaries.Count; i++)
                result *= boundaries[i];

            return result;
        }
    }
}
