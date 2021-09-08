using System;

namespace NpmAdapter.Payload
{
    class ResponseEzAlertIOCarPayload
    {
        //<start=0000&0>$version=3.0$cmd=31$copy=1-10$target=parking #dongho=101&101#inout=0#time=20070101120000#carno=1111

        public string dong { get; set; }
        public string ho { get; set; }
        public string inout { get => "0"; }
        public string time { get; set; }
        public string carno { get; set; }

        public void BindData(string msg)
        {
            string bodyMsg = GetBodyMessage(msg);
            int secondIdx = 0;

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
                            
                            switch (key)
                            {
                                case "dongho":
                                    dong = value.Split('&')[0];
                                    ho = value.Split('&')[1];
                                    break;
                                case "time":
                                    time = value;
                                    break;
                                case "carno":
                                    carno = value;
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
