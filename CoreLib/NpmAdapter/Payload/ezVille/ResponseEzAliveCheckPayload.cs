using System;

namespace NpmAdapter.Payload
{
    class ResponseEzAliveCheckPayload
    {
        public string dong { get; set; }
        public string ho { get; set; }
        public string ip { get; set; }
        public string status { get; set; }

        public override string ToString()
        {
            return $"#dongho={dong}&{ho}#ip={ip}#status={status}#curtime={DateTime.Now.ToString("yyyyMMddHHmmss")}#hwversion=1.0#swversion=1.1";
        }
    }
}
