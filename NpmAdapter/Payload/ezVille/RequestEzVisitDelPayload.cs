using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestEzVisitDelPayload
    {
        //<start=0000&0>$version=3.0$cmd=20$copy=1-10$dongho=100&900$target=parking #mode=2#dongho=101&101#no=1#carno=1111#no=2#carno=2222

        public class CarInfo
        {
            public string no { get; set; }
            public string carno { get; set; }
        }

        public EZV_VISIT_MODE mode;
        public string dong { get; set; }
        public string ho { get; set; }
        public List<CarInfo> list;

        public void BindData(string msg)
        {
            string bodyMsg = GetBodyMessage(msg);
            int secondIdx = 0;
            int iList = -1;
            list = new List<CarInfo>();

            while (secondIdx >= 0)
            {
                try
                {
                    secondIdx = bodyMsg.IndexOf('#', 1); //맨 앞에 #이 있음...
                    if (secondIdx == -1)
                    {
                        break;
                    }
                    else
                    {
                        var result = bodyMsg.Substring(1, secondIdx - 1);
                        string[] pieces = result.Split('=');
                        if (pieces == null || pieces.Length != 2)
                        {
                            secondIdx = -1;
                        }
                        else
                        {
                            string key = pieces[0];
                            string value = pieces[1];
                            //#mode=2#dongho=101&101#no=1#carno=1111#no=2#carno=2222
                            switch (key)
                            {
                                case "mode":
                                    mode = (EZV_VISIT_MODE)int.Parse(value);
                                    break;
                                case "dongho":
                                    dong = value.Split('&')[0];
                                    ho = value.Split('&')[1];
                                    break;
                                case "no":
                                    iList += 1;
                                    list.Add(new CarInfo());
                                    list[iList].no = value;
                                    break;
                                case "carno":
                                    list[iList].carno = value;
                                    break;
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    secondIdx = -1;
                }
            }
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
