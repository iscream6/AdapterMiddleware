using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class HipassPaymentPayload
    {
        public string stx { get; set; }          //0x02                   1
        public string length { get; set; }       //code + data
        public string code { get; set; }         //0102                   4
        public string gamangcode { get; set; }   //                       8
        public string PosAdminID { get; set; }   //                      20
        public string accountfee { get; set; }      //                      8
        public string vat { get; set; }          //                      8
        public string servicefee { get; set; }   //                      8
        public string TID { get; set; }          //                      20
        public string vanregno { get; set; }     //                      8
        public string outtime { get; set; }      //                      14
        public string intime { get; set; }       //                      14  
        public string version { get; set; }      //                      4
        public string parkfee { get; set; }      //                      8
        public string disuse { get; set; }     //                      1 1:할인적용,2:미적용
        public string dummy { get; set; }        //                      369
        public string etx { get; set; }          //0x03
    }
}
