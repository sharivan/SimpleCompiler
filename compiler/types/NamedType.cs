using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.types
{
    public abstract class NamedType : AbstractType
    {
        private CompilationUnity unity;
        private string name;
        private SourceInterval interval;

        public CompilationUnity Unity => unity;

        public string Name => name;

        public SourceInterval Interval => interval;

        protected NamedType(CompilationUnity unity, string name, SourceInterval interval)
        {
            this.unity = unity;
            this.name = name;
            this.interval = interval;
        }
    }
}
