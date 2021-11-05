using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class Label
    {
        internal Assembler bindedAssembler;
        internal int bindedIP;
        internal List<Tuple<Assembler, int>> references;

        public Assembler BindedAssembler
        {
            get
            {
                return bindedAssembler;
            }
        }

        public int BindedIP
        {
            get
            {
                return bindedIP;
            }
        }

        public int ReferenceCount
        {
            get
            {
                return references.Count;
            }
        }

        public Tuple<Assembler, int> this[int index]
        {
            get
            {
                return references[index];
            }
        }

        internal Label()
        {
            bindedAssembler = null;
            bindedIP = -1;

            references = new List<Tuple<Assembler, int>>();
        }

        private void UpdateReference(int index)
        {
            Tuple<Assembler, int> reference = references[index];
            Assembler referenceAssembler = reference.Item1;
            int referenceIP = reference.Item2;

            long lastPosition = referenceAssembler.Position;
            referenceAssembler.Position = referenceIP;
            referenceAssembler.EmitData(bindedIP - referenceIP + 1);
            referenceAssembler.Position = lastPosition;
        }

        internal void UpdateReferences()
        {
            for (int i = 0; i < references.Count; i++)
                UpdateReference(i);
        }

        internal void Bind(Assembler assembler, int ip)
        {
            bindedAssembler = assembler;
            bindedIP = ip;
        }

        internal void AddReference(Assembler assembler, int ip)
        {
            references.Add(new Tuple<Assembler, int>(assembler, ip));
        }
    }
}
