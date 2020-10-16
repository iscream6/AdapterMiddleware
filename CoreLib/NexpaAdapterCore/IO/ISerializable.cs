using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NexpaAdapterStandardLib.IO
{
    public interface ISerializable
    {
        int Size { get; }

        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
