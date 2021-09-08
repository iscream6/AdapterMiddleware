using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    class HipassReceivePayload
    {
        public string stx { get; set; }            //0x02                   1
        public string length { get; set; }          //code + data
        public string code { get; set; }             //0102                   4
        public string gamangcode { get; set; }      //                       8
        public string PosID { get; set; }            //                      20
        public string PosSeq { get; set; }          //                       8
        public string viocode { get; set; }        //                       2
        public string receipt_resq { get; set; }   //                       1 0: 요청없음, 1: 요청있음
        public string cardsunbul { get; set; }     //                       1 0: 선불,     1: 후불
        public string cardno { get; set; }         //                      16
        public string parkaccountfee { get; set; } //                       8
        public string afterdisaccount { get; set; }//                       8
        public string accountfee { get; set; }     //                       8 실제 카드에서 빠져나간 금액
        public string wongum { get; set; }         //                       8
        public string OBU_Carkind { get; set; }    //                       1 "1"~"6","0":미발급
        public string OBU_Kind { get; set; }       //                       2 "00"~"FF"(장애,국가유공자)
        public string suNap { get; set; }          //                       1 0:할인없음, 1: 6종할인, 2:OBU 종류할인(장애,국가유공)
        public string logicalID { get; set; }      //                       8 도로공사 발급 ID
        public string algID { get; set; }          //                       1 알고리즘ID
        public string psamID { get; set; }         //                       8 psamID
        public string date { get; set; }           //                       4 구매일자(yyyymmdd)
        public string trtpsam { get; set; }        //                       1 거래유형
        public string ppiep { get; set; }          //                       3 전자화폐발행기관ID
        public string iep { get; set; }            //                       5 전자화폐ID
        public string ntiep { get; set; }          //                       4 전자화폐거래 일련번호
        public string ntpsam { get; set; }         //                       4 거래 일련번호
        public string mtotpsam { get; set; }       //                       4 거래 총액
        public string nipp { get; set; }           //                       2 전자화폐 발행기관별 개별 거래수
        public string nc { get; set; }             //                       4 거래수집일련번호
        public string vkpsam_kdindc { get; set; }  //                       1 개별거해 검증을 위한 서명키 (버전1)
        public string sindc { get; set; }          //                       4 psam에서 생성한 서명1
        public string vkpsam_kdinds { get; set; }  //                       1 개별거해 검증을 위한 서명키 (버전2)
        public string sinds { get; set; }          //                       4 psam에서 생성한 서명1
        public string centerID { get; set; }       //                       1 센터ID
        public string sendno { get; set; }         //                       4 카드처리데이터 전소연번과 같음
        public string dummy { get; set; }          //                       335
        public string etx { get; set; }            //0x03                   1

        public JToken SerializeJson()
        {
            JObject json = new JObject();
            json["gamangcode"] = gamangcode;
            json["PosID"] = PosID;
            json["PosSeq"] = PosSeq;
            json["viocode"] = viocode;
            json["receipt_resq"] = receipt_resq;
            json["cardsunbul"] = cardsunbul;
            json["cardno"] = cardno;
            json["parkaccountfee"] = parkaccountfee;
            json["afterdisaccount"] = afterdisaccount;
            json["accountfee"] = accountfee;
            json["wongum"] = wongum;
            json["OBU_Carkind"] = OBU_Carkind;
            json["OBU_Kind"] = OBU_Kind;
            json["suNap"] = suNap;

            return json;
        }

        public void Initialize()
        {
            stx = "";
            length = "";
            code = "";
            gamangcode = "";
            PosID = "";
            PosSeq = "";
            viocode = "";
            receipt_resq = "";
            cardsunbul = "";
            cardno = "";
            parkaccountfee = "";
            afterdisaccount = "";
            accountfee = "";
            wongum = "";
            OBU_Carkind = "";
            OBU_Kind = "";
            suNap = "";
            logicalID = "";
            algID = "";
            psamID = "";
            date = "";
            trtpsam = "";
            ppiep = "";
            iep = "";
            ntiep = "";
            ntpsam = "";
            mtotpsam = "";
            nipp = "";
            nc = "";
            vkpsam_kdindc = "";
            sindc = "";
            vkpsam_kdinds = "";
            sinds = "";
            centerID = "";
            sendno = "";
            dummy = "";
            etx = "";
        }
    }
}