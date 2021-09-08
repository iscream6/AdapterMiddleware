using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseEzVisitListPayload
    {
        //<start=0000&0>$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=parking#mode=1#dongho=101&101#total=4
        //#no=1#inout=0#time=20170101120000#carno=1111
        //#no=2#inout=0#time=20170101120000#carno=1111

        public class CarInfo
        {
            public string no { get; set; }
            public string inout { get; set; }
            public string time { get; set; }
            public string carno { get; set; }
            public string err { get; set; }
        }

        public EZV_VISIT_MODE mode; // 0:일반 입차 통보, 1:입차 예약 서비스, 2:입차예약취소
        public string dongho { get; set; }
        public string total { get; set; }
        public List<CarInfo> list;

        public override string ToString()
        {
            StringBuilder value = new StringBuilder();
            value.Append($"#mode={(int)mode}#dongho={dongho}#total={total}");

            if (list != null)
            {
                foreach (var item in list)
                {
                    value.Append($"#no={item.no}#inout={item.inout}#time={item.time}#carno={item.carno}");
                    if (item.err != null && item.err != "")
                        value.Append($"#err={item.err}");
                }
            }

            return value.ToString();
        }
    }
}
