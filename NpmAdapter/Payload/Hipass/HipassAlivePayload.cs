using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class HipassAlivePayload
    {
        public string stx { get; set; }          //0x02                   1
        public string length { get; set; }       //code + data
        public string code { get; set; }         //0102                   4
        public string OBU_dis { get; set; }      //6종할인율               3
        public string OBU_kind_dis { get; set; } //OBU 종류별 할인률       3
        public string dummy { get; set; }        //439                    3
        public string etx { get; set; }          //0x03                   1
    }
}
