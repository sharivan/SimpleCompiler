using System;
using System.Collections.Generic;

namespace assembler
{
    public class Label
    {
        internal Assembler bindedAssembler;
        internal int bindedIP;
        internal List<(Assembler, int)> references;

        public Assembler BindedAssembler => bindedAssembler;

        public int BindedIP => bindedIP;

        public int ReferenceCount => references.Count;

        public (Assembler, int) this[int index] => references[index];

        internal Label()
        {
            bindedAssembler = null;
            bindedIP = -1;

            references = new List<(Assembler, int)>();
        }

        private void UpdateReference(Assembler assembler, int index)
        {
            (Assembler, int) reference = references[index];
            Assembler referenceAssembler = reference.Item1;
            if (assembler != referenceAssembler)
                return;

            int referenceIP = reference.Item2;

            long lastPosition = referenceAssembler.Position;
            referenceAssembler.Position = referenceIP;
            referenceAssembler.EmitData(bindedIP - referenceIP + 1);
            referenceAssembler.Position = lastPosition;
        }

        internal void UpdateReferences(Assembler assembler)
        {
            for (int i = 0; i < references.Count; i++)
                UpdateReference(assembler, i);
        }

        internal void Bind(Assembler assembler, int ip)
        {
            bindedAssembler = assembler;
            bindedIP = ip;
        }

        internal void AddReference(Assembler assembler, int ip) => references.Add((assembler, ip));
    }
}
