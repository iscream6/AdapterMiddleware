using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseEzVisitDelPayload
    {
        //<start=0000&0>$version=3.0$cmd=21$copy=1-10$dongho=100&900$target=parking #mode=2

        public EZV_VISIT_MODE mode;

        public override string ToString()
        {
            return $"#mode={(int)mode}";
        }
    }
}
