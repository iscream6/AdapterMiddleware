using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestEzAlertIOCarPayload
    {
        //통보 요청(입차 예약 차량)
        //<start=0000&0>$version=3.0$cmd=30$copy=1-10$dongho=100&900$target=parking #mode=1#dongho=101&101#inout=0#time=20070101120000#carno=1111
        //통보 요청(입주민 차량)
        //<start=0000&0>$version=3.0$cmd=30$copy=1-10$dongho=100&900$target=parking #mode=0#dongho=101&101#inout=0#time=20070101120000#carno=1111

        public EZV_VISIT_MODE mode;
        public string dong { get; set; }
        public string ho { get; set; }
        public string inout { get; set; } 
        public string time { get; set; }
        public string carno { get; set; }

        public override string ToString()
        {
            return $"#mode={(int)mode}#dongho={dong}&{ho}#inout={inout}#time={time}#carno={carno}";
        }
    }
}
