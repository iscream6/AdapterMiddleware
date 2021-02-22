using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestEzVisitRegPayload
    {
        //<start=0000&0>$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=parking#mode=1#dongho=101&101#no=1#inout=0#time=20070101120000#carno=1111

        public EZV_VISIT_MODE mode;
        public string dong { get; set; }
        public string ho { get; set; }
        public string no { get; set; }
        public string inout { get; set; }
        public string time { get; set; }
        public string carno { get; set; }

        public void BindData(string msg)
        {
            Dictionary<string, string> dicBody = GetBodyMessage(msg).DoubleSplit('#', '=');
            mode = (EZV_VISIT_MODE)int.Parse(dicBody["mode"]);
            dong = dicBody["dongho"].Split('&')[0];
            ho = dicBody["dongho"].Split('&')[1];
            no = dicBody["no"];
            inout = dicBody["inout"];
            time = dicBody["time"];
            carno = dicBody["carno"];
        }

        private string GetBodyMessage(string origin)
        {
            if (!origin.Contains("#")) return "";
            else
            {
                int iStartIdx = origin.IndexOf("#");
                return origin.Substring(iStartIdx + 1);
            }
        }
    }
}
