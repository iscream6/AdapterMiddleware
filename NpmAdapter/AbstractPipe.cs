using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter
{
    public enum AdapterType
    {
        none,
        nexpa,
        homenet
    }

    public abstract class AbstractPipe
    {
        public abstract bool GeneratePipe();

        public abstract bool StartAdapter(AdapterType type);

        public abstract bool StopAdapter(AdapterType type);
    }
}
