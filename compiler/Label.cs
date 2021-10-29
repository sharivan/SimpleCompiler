using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class Label
    {
        internal Assembler assembler;
        private int ip;
        private List<int> references;

        public Assembler Assembler
        {
            get
            {
                return assembler;
            }
        }

        public int IP
        {
            get
            {
                return ip;
            }
        }

        public int ReferenceCount
        {
            get
            {
                return references.Count;
            }
        }

        public int this[int index]
        {
            get
            {
                return references[index];
            }
        }

        internal Label(Assembler assembler, int ip = -1)
        {
            this.assembler = assembler;
            this.ip = ip;

            references = new List<int>();
        }

        private void UpdateReference(int index)
        {
            long lastPosition = assembler.Position;
            int referenceIP = references[index];
            assembler.Position = referenceIP;
            assembler.EmitLoadConst(ip - referenceIP - 6);
            assembler.Position = lastPosition;
        }

        private void UpdateReferences()
        {
            long lastPosition = assembler.Position;

            for (int i = 0; i < references.Count; i++)
            {
                int referenceIP = references[i];
                assembler.Position = referenceIP;
                assembler.EmitLoadConst(ip - referenceIP - 6);
            }

            assembler.Position = lastPosition;
        }

        internal void Bind(int ip)
        {
            this.ip = ip;

            UpdateReferences();
        }

        internal void AddReference(int ip, bool update = true)
        {
            references.Add(ip);

            if (update && this.ip != -1)
                UpdateReference(references.Count - 1);
        }
    }
}
