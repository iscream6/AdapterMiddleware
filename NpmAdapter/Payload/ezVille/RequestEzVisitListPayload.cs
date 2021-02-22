using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestEzVisitListPayload
    {
        //<start=0000&0>$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=parking#mode=1#dongho=101&101#param=1&10

        public EZV_VISIT_MODE mode;
        public string dong { get; set; }
        public string ho { get; set; }
        public string param { get; set; }

        public void BindData(string msg)
        {
            Dictionary<string, string> dicBody = GetBodyMessage(msg).DoubleSplit('#', '=');
            mode = (EZV_VISIT_MODE)int.Parse(dicBody["mode"]);
            dong = dicBody["dongho"].Split('&')[0]; 
            ho = dicBody["dongho"].Split('&')[1]; 
            param = dicBody["param"];
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
