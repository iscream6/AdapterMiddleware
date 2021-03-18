using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    public enum EZV_VISIT_MODE
    {
        일반입차통보 = 0,
        입차예약 = 1,
        입차예약취소 = 2
    }

    public enum EZV_HEAD_CMD
    {
        none = 0,
        조회요청 = 10,
        조회응답 = 11,
        제어요청 = 20,
        제어응답 = 21,
        이벤트전송 = 30,
        이벤트수신 = 31
    }

    class ezHeaderPayload
    {
        public EZV_HEAD_CMD cmd;
        public string version { get; set; }
        public string copy { get; set; }
        public string dongho { get; set; }
        public string target { get; set; }

        public void Initialize()
        {
            cmd = EZV_HEAD_CMD.none;
            version = "";
            copy = "";
            dongho = "";
            target = "";
        }

        public void BindData(string msg)
        {
            string hdr = GetHeaderMessage(msg);
            Dictionary<string, string> dicHdr = hdr.DoubleSplit('$', '=');
            version = dicHdr["VERSION"];
            cmd = (EZV_HEAD_CMD)int.Parse(dicHdr["CMD"]);
            copy = dicHdr["COPY"];
            if (dicHdr.Keys.Contains("DONGHO")) dongho = dicHdr["DONGHO"];
            else dongho = $"{SysConfig.Instance.HC_Id}&{SysConfig.Instance.HC_Pw}";
            target = dicHdr["TARGET"];
        }

        public EZV_VISIT_MODE GetMode(string msg)
        {
            Dictionary<string, string> dicBody = GetBodyMessage(msg).DoubleSplit('#', '=');
            EZV_VISIT_MODE mode = (EZV_VISIT_MODE)int.Parse(dicBody["MODE"]);
            return mode;
        }

        public override string ToString()
        {
            StringBuilder strResult = new StringBuilder();
            strResult.Append($"$version={version}$copy={copy}");
            if (dongho != "") strResult.Append($"$dongho={dongho}");
            else strResult.Append($"{SysConfig.Instance.HC_Id}&{SysConfig.Instance.HC_Pw}");
            strResult.Append($"$cmd={(int)cmd}");
            strResult.Append($"$target={target}");
            return strResult.ToString();
        }

        /// <summary>
        /// 응답 형태로 헤더 메시지를 반환함.
        /// </summary>
        /// <returns></returns>
        public string ResponseToString()
        {
            StringBuilder strResult = new StringBuilder();
            int iCmd = (int)cmd + 1;
            strResult.Append($"$version={version}$copy={copy}");
            if (dongho != "") strResult.Append($"$dongho={dongho}");
            else strResult.Append($"{SysConfig.Instance.HC_Id}&{SysConfig.Instance.HC_Pw}");
            strResult.Append($"$cmd={iCmd}");
            strResult.Append($"$target={target}");
            return strResult.ToString();
        }

        /// <summary>
        /// 헤더 메시지만 가져옴.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        private string GetHeaderMessage(string origin)
        {
            int iStartIdx = origin.IndexOf(">$");
            string tempMsg = origin.Substring(iStartIdx + 2);
            int iLast = tempMsg.Contains('#') ? tempMsg.IndexOf("#") : tempMsg.Length;
            return tempMsg.Substring(0, iLast);
        }

        /// <summary>
        /// 바디 메시지만 가져옴.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
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
