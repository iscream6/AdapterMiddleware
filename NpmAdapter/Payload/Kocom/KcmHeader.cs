using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class KcmHeader
    {
        //0x12345678
        private const int HeaderKey = 305419896;

        private int _msgType;
        private int _msgLength;
        private int _town;
        private int _dong;
        private int _ho;
        private int _reserved;
    }
}
