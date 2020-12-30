using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.DataAccess;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Model;
using NpmAdapter.Payload;
using NpmAdapter.Payload.Nexpa.Data.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using static NpmAdapter.Model.TotalContorolProtocol;

namespace NpmAdapter.Adapter
{
    class NPAutoBoothAdapter : IAdapter
    {
        private enum LprType
        {
            NEXPA,
            INOMETRICS
        }

        public enum BarControlStatus
        {
            Open = 0,
            Close = 1,
            OpenLock = 2,
            OpenUnlock = 3
        }

        private const string AuthorizationSeed = "NexpaRegistryHashStoreValidationSeed";
        private const string REQ_POST_STATUS = "/nxmdl/mch";
        private string ValidationAuthoData = string.Empty;
        private object lockObj = new object();
        private LprType lprType = LprType.NEXPA;
        
        private UnitModel _unitMdl;
        private DCModel _dcMdl;
        private DataTable _lprData;
        private DataTable _boothData;
        private DataTable _dcData;
        private bool isRun;

        private Dictionary<string, UnitInfo> _dicLprNetwork;
        private Dictionary<string, INetwork> _dicBoothNetwork;

        public IAdapter TargetAdapter { get; set; }
        private INetwork MyHttpNetwork { get; set; }

        public bool IsRuning => isRun;

        public NPAutoBoothAdapter()
        {
            _dcMdl = new DCModel();
            _unitMdl = new UnitModel();
            ValidationAuthoData = Helper.Base64Encode(AuthorizationSeed);
        }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            _dcData = _dcMdl.GetDCInfo();

            //LPR 장비 네트워크를 Setting 한다. (Key : UnitNo, Value : UnitInfo)
            _dicLprNetwork = new Dictionary<string, UnitInfo>();
            _lprData = _unitMdl.GetLprInfo();

            if(_lprData != null)
            {
                foreach (DataRow dr in _lprData.Rows)
                {
                    var unitNo = dr["UnitNo"].ToString();
                    _dicLprNetwork.Add(unitNo, new UnitInfo(dr));
                }
            }

            //정산기 네트워크를 Setting 한다. (Key : UnitNo, Value : Client Network)
            _dicBoothNetwork = new Dictionary<string, INetwork>();
            _boothData = _unitMdl.GetBoothInfo();

            if(_boothData != null)
            {
                foreach (DataRow dr in _boothData.Rows)
                {
                    var unitNo = dr["UnitNo"].ToString();
                    var ipNo = dr["IPNo"].ToString();
                    var portNo = dr["PortNo"].ToString();
                    _dicBoothNetwork.Add(unitNo, NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, ipNo, portNo));
                }
            }

            //웹 서버 소켓을 만든다.
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, SysConfig.Instance.Nexpa_WebPort);
            
            return true;
        }

        public void SendMessage(IPayload payload)
        {

        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {

        }

        public bool StartAdapter()
        {
            MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
            isRun = MyHttpNetwork.Run();
            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;
            MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
            bResult = MyHttpNetwork.Down();
            isRun = !bResult;
            return bResult;
        }

        public void TestReceive(byte[] buffer)
        {
        }

        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null)
        {
            lock (lockObj)
            {
                CmdType cmdType = CmdType.none;
                JObject json = JObject.Parse(SysConfig.Instance.Nexpa_Encoding.GetString(buffer[..(int)size]));
                cmdType = (CmdType)Enum.Parse(typeof(CmdType), Helper.NVL(json["command"]));

                string urlData = e.Request.Uri.PathAndQuery;

                Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | MyHttpNetwork_ReceiveFromPeer", $"URL : {urlData}", LogAdpType.Nexpa);
                Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | MyHttpNetwork_ReceiveFromPeer", $"Authorization : {Helper.NVL(e.Request.Headers["Authorization"].HeaderValue)}", LogAdpType.Nexpa);

                if (urlData != REQ_POST_STATUS)
                {
                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                    e.Response.Reason = "Bad Request";

                    ResponsePayload resultPayload = new ResponsePayload();
                    resultPayload.command = cmdType;
                    resultPayload.result = ResultType.InvalidURL;
                    byte[] result = resultPayload.Serialize();

                    e.Response.Body.Write(result, 0, result.Length);
                }
                else if(e.Request.Headers["Authorization"].HeaderValue != ValidationAuthoData)
                {
                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Status = System.Net.HttpStatusCode.Unauthorized;
                    e.Response.Reason = "Faild Authorization";

                    ResponsePayload resultPayload = new ResponsePayload();
                    resultPayload.command = cmdType;
                    resultPayload.result = ResultType.faild_authorization;
                    byte[] result = resultPayload.Serialize();

                    e.Response.Body.Write(result, 0, result.Length);
                }
                else
                {
                    IPayload resultPayload = null;

                    Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | MyHttpNetwork_ReceiveFromPeer", $"{json}", LogAdpType.Nexpa);
                    
                    try
                    {
                        switch (Helper.NVL(json["command"]))
                        {
                            case "alive_check": //상태 체크
                                {
                                    resultPayload = new ResponseDataPayload();
                                    JObject jresp = MakeResponseJson("alive_check", ResultType.OK);
                                    JObject jdata = new JObject();

                                    jdata["booth_list"] = MakeAliveCheckData(ResponseAutoBoothPayload.divType.Booth);
                                    jdata["lpr_list"] = MakeAliveCheckData(ResponseAutoBoothPayload.divType.Lpr);
                                    jdata["dcinfo_list"] = MakeAliveCheckData(ResponseAutoBoothPayload.divType.Discount);
                                    jresp["data"] = jdata;
                                    resultPayload.Deserialize(jresp);
                                }

                                break;
                            case "bar_open": //차단바 오픈
                                {
                                    string parkNo = Helper.NVL(json["data"]["park_no"]);
                                    string boothNo = Helper.NVL(json["data"]["booth_unit_no"]);
                                    string lprNo = Helper.NVL(json["data"]["lpr_unit_no"]);

                                    resultPayload = new ResponsePayload();
                                    if (BarOpen(parkNo, boothNo, lprNo, BarControlStatus.Open))
                                    {
                                        JObject resultObj = MakeResponseJson("bar_open", ResultType.OK);
                                        resultPayload.Deserialize(resultObj);
                                    }
                                    else
                                    {
                                        JObject resultObj = MakeResponseJson("bar_open", ResultType.notinterface_lpr);
                                        resultPayload.Deserialize(resultObj);
                                    }
                                }
                                
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"NPAutoBoothAdapter | MyHttpNetwork_ReceiveFromPeer", $"{ex.Message}");
                        resultPayload = new ResponsePayload();
                        JObject resultObj = MakeResponseJson("bar_open", ResultType.ExceptionERROR);
                        resultPayload.Deserialize(resultObj);
                    }

                    byte[] result = resultPayload.Serialize();

                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Reason = "OK";
                    e.Response.Body.Write(result, 0, result.Length);
                }
            }
        }

        #region Private Methods

        private JObject MakeResponseJson(string command, ResultType resultType)
        {
            JObject json = new JObject();
            json["command"] = command;
            json["result"] = ResultPayload.GetStatusPayload(resultType).ToJson();
            return json;
        }

        private JArray MakeAliveCheckData(ResponseAutoBoothPayload.divType type)
        {
            JArray arr = new JArray();

            switch (type)
            {
                case ResponseAutoBoothPayload.divType.Booth:
                    if(_boothData != null && _boothData.Rows.Count > 0)
                    {
                        foreach (DataRow dr in _boothData.Rows)
                        {
                            ResponseAutoBoothPayload.AutoBoothPayload payload = ResponseAutoBoothPayload.GetSubPayload(type);
                            payload.park_no = dr["ParkNo"].ToString();
                            payload.div_no = dr["UnitNo"].ToString();
                            payload.div_name = dr["UnitName"].ToString();
                            arr.Add(payload.ToJson());
                        }
                    }
                    break;
                case ResponseAutoBoothPayload.divType.Lpr:
                    foreach (DataRow dr in _lprData.Rows)
                    {
                        ResponseAutoBoothPayload.AutoBoothPayload payload = ResponseAutoBoothPayload.GetSubPayload(type);
                        payload.park_no = dr["ParkNo"].ToString();
                        payload.div_no = dr["UnitNo"].ToString();
                        payload.div_name = dr["UnitName"].ToString();
                        arr.Add(payload.ToJson());
                    }
                    break;
                case ResponseAutoBoothPayload.divType.Discount:
                    foreach (DataRow dr in _dcData.Rows)
                    {
                        ResponseAutoBoothPayload.AutoBoothPayload payload = ResponseAutoBoothPayload.GetSubPayload(type);
                        payload.park_no = dr["ParkNo"].ToString();
                        payload.div_no = dr["DCNo"].ToString();
                        payload.div_name = dr["DCName"].ToString();
                        arr.Add(payload.ToJson());
                    }
                    break;
            }

            return arr;
        }

        private bool BarOpen(string psParkNo, string psBoothUnitNo, string psLprUnitNo, BarControlStatus pBarStatus)
        {
            try
            {
                ProtocolData protocolData = new ProtocolData();
                protocolData.CurrentCommand = ControlCommand.BAROPEN;
                protocolData.ReceiveParkNO = psParkNo;
                protocolData.ReceiveUnitNo = psBoothUnitNo;
                protocolData.SetBarControl(psLprUnitNo, ((int)pBarStatus).ToString());

                byte[] sendData = Encoding.Default.GetBytes(protocolData.SendMakeMessage);

                INetwork client = _dicBoothNetwork[psBoothUnitNo];

                if (client != null && client.Run())
                {
                    client.SendToPeer(sendData, 0, sendData.Length);
                    client.Down();
                    return true;
                }
                else
                {
                    Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | BarOpen", $"Booth : {psBoothUnitNo}, Lpr : {psLprUnitNo}");
                    return false;
                }

                #region 주석처리

                //List<byte> sendMsg = new List<byte>();
                //if (lprType == LprType.NEXPA)
                //{
                //    sendMsg.AddRange(Encoding.Default.GetBytes("BAR_OPEN_1"));
                //}
                //else if (lprType == LprType.INOMETRICS)
                //{
                //    sendMsg.Add(0x02);
                //    sendMsg.AddRange(Encoding.Default.GetBytes("GATE|OPEN"));
                //    sendMsg.Add(0x03);
                //}

                //INetwork client = DicLprNetwork[psUnitNo];

                //if(client != null && client.Run())
                //{
                //    client.SendToPeer(sendMsg.ToArray(), 0, sendMsg.Count);
                //    client.Down();
                //}
                //else
                //{
                //    Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | BarOpen", $"{psUnitNo}번 Lpr이 없습니다.");
                //}

                #endregion
            }
            catch (Exception ex)
            {
                //연결 실패함. 넥스파로 실패 로그처리....
                Log.WriteLog(LogType.Error, $"NPAutoBoothAdapter | BarOpen", $"{ex.Message}");
                return false;
            }
        }

        #endregion

    }

    class UnitInfo
    {
        public string ParkNo { get; }
        public string UnitNo { get;}
        public string UnitName { get;}
        public string IP { get;}
        public string Port { get; }

        public UnitInfo(DataRow dr)
        {
            ParkNo = dr["ParkNo"].ToString();
            UnitNo = dr["UnitNo"].ToString();
            UnitName = dr["UnitName"].ToString();
            IP = dr["IPNo"].ToString();
            Port = dr["PortNo"].ToString();
        }
    }

    /// <summary>
    /// 약속된 프로토콜 정보를 변수로 가지고 있을 클래스 IP,장비구분,명령어,섭명령어 포함
    /// </summary>
    //public class ProtocolData
    //{
    //    /// <summary>
    //    /// 프로토콜 규칙중 stx etx제외하고 {fs}로 분리했을때 나오는 데이터 
    //    /// </summary>
    //    public enum CommandRegulation
    //    {
    //        COMMAND, // 명령어
    //        SENDPARKNO, // 보내는 주차장번호
    //        SENDUNITNO, // 보내는 대상의 장비번호
    //        SENDDATETIME, // 시간(yyyyMMddHHmmss)
    //        RECEIVEPARKNO, // 받는 주차장번호
    //        RECEIVEUNITNO, // 받는 대상의 장비번호
    //        SubCommand // 
    //    }

    //    /// <summary>
    //    /// 명령어
    //    /// </summary>
    //    public enum Command
    //    {
    //        PAYCANCLE,

    //        /// <summary>
    //        /// 사전할인무료
    //        /// </summary>
    //        PREDISCOUNT,
    //        /// <summary>
    //        /// 사전정산차량
    //        /// </summary>
    //        PRECAR,
    //        /// <summary>
    //        /// 회차
    //        /// </summary>
    //        FREECAR,
    //        /// <summary>
    //        /// 정기권
    //        /// </summary>
    //        REG,

    //        /// <summary>
    //        /// 시간오버
    //        /// </summary>
    //        PAYTIMEOUT,

    //        BAROPEN,
    //        /// <summary>
    //        /// 차량정보
    //        /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
    //        /// </summary>
    //        CARINFO,
    //        /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
    //        JUNGSANINFO,
    //        /// <summary>
    //        /// LPR상태
    //        /// LPR상태  (0:비정상/1:정상),차량상태메세지
    //        /// </summary>
    //        LPRSTATE,
    //        /// <summary>
    //        /// 정산기 상태 
    //        /// </summary>
    //        APSTATE,
    //        /// <summary>
    //        /// 수동입차  차량번호,TkNo,입차일자,입차시간,입차장비번호
    //        /// </summary>
    //        CARINSERT,
    //        /// <summary>
    //        /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
    //        /// </summary>
    //        GET_CARPAY,
    //        /// <summary>
    //        /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
    //        /// </summary>
    //        MAKE_CARPAY,
    //        /// <summary>
    //        /// 일반차량 영수증재출력
    //        /// </summary>
    //        RECEIPTNORMAL,

    //        /// <summary>
    //        /// 할인 차량번호,TkNo,입차일자,입차시간,현재할인전송
    //        /// </summary>
    //        DISCOUNT,
    //        /// <summary>
    //        /// 차단기관련
    //        /// </summary>
    //        BARRIER,
    //        /// <summary>
    //        /// 장비리셋
    //        /// </summary>
    //        RESET,
    //        /// <summary>
    //        /// 주차요금 강제표출
    //        /// </summary>
    //        CHARGE,
    //        /// <summary>
    //        /// 주차요금요청(센터에서 요금요청 현재 무인정산기에서 요금을받고있는차량정보)
    //        /// </summary>
    //        GETCURRENT_PAYMONEY,
    //        GETCURRENT_ORIGINAL,
    //        /// <summary>
    //        /// 강제 돈방출
    //        /// </summary>
    //        DISPENSE,
    //        /// <summary>
    //        /// 정상 커맨트카아님
    //        /// </summary>
    //        NONCOMMAND,
    //        /// <summary>
    //        /// 수입금
    //        /// </summary>
    //        INCOME,
    //        /// <summary>
    //        /// 수입금
    //        /// </summary>
    //        OUTCOME,
    //        /// <summary>
    //        /// 보유금
    //        /// </summary>
    //        REVERSE,
    //        /// <summary>
    //        /// 무인정산기 가동
    //        /// </summary>
    //        START,
    //        /// <summary>
    //        /// 무인정산기 비가동
    //        /// </summary>
    //        END,
    //        CENTER,
    //        GOSISER,
    //        //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용
    //        /// <summary>
    //        /// 미,부분인식/일반/정기차량 입차 차단기 개방설정
    //        /// </summary>
    //        BAROPENMODE,
    //        //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용완료
    //        //전동어닝 제어 적용
    //        ELECAWNING_CONTROL,
    //        //전동어닝 제어 적용완료
    //        //무인정산기 개방모드 적용
    //        AUTOBOOTH_OPENMODE
    //        //무인정산기 개방모드 적용완료
    //    }

    //    /// <summary>
    //    /// BRE 지폐리더기, CRE 동전리더기,TRE 교통카드리더기,BCH 지폐방출기,_CC1_동전방출기,CA1 카드리더기 , DID 도어신호 , REP 영수증프린터
    //    /// </summary>
    //    public enum DEVICE
    //    {
    //        /// <summary>
    //        /// 지폐리더기
    //        /// </summary>
    //        BRE,
    //        /// <summary>
    //        /// 동전리더기
    //        /// </summary>
    //        CRE,
    //        /// <summary>
    //        /// 교통카드리더기
    //        /// </summary>
    //        TRE,
    //        /// <summary>
    //        /// 지폐방출기
    //        /// </summary>
    //        BCH,
    //        /// <summary>
    //        /// 50원방출기
    //        /// </summary>
    //        CC1,
    //        /// <summary>
    //        /// 100원방출기
    //        /// </summary>
    //        CC2,
    //        /// <summary>
    //        /// 500원방출기
    //        /// </summary>
    //        CC3,
    //        /// <summary>
    //        /// 카드리더기1
    //        /// </summary>
    //        CA1,
    //        /// <summary>
    //        /// 카드리더기2
    //        /// </summary>
    //        CA2,
    //        /// <summary>
    //        /// LED
    //        /// </summary>
    //        DID,
    //        /// <summary>
    //        /// 영수증
    //        /// </summary>
    //        REP,
    //        /// <summary>
    //        /// 전광판
    //        /// </summary>
    //        DIS,
    //        /// <summary>
    //        /// 전체
    //        /// </summary>
    //        ALL,
    //        /// <summary>
    //        /// 없음
    //        /// </summary>
    //        NONE
    //    }
    //    //미,부분인식/일반/정기차량 차단기 개방설정 적용
    //    public enum BAROPENMODE
    //    {
    //        //미,부분인식 차량 미개방
    //        NOT_PART_NOPEN,
    //        //미,부분인식 차량 개방
    //        NOT_PART_OPEN,
    //        //일반차량 미개방
    //        NORMAL_OPEN,
    //        //일반차량 개방
    //        NORMAL_NOPEN,
    //        //정기차량 개방
    //        REGIST_OPEN,
    //        //정기차량 미개방
    //        REGIST_NOPEN
    //    }
    //    //미,부분인식/일반/정기차량 차단기 개방설정 적용완료
    //    //전동어닝 제어 적용
    //    public enum ELECAWNING_CONTROL
    //    {
    //        //전동어닝 열기
    //        OPEN,
    //        //전동어닝 닫기
    //        CLOSE,
    //        //전동어닝 멈춤
    //        STOP
    //    }
    //    //전동어닝 제어 적용완료
    //    // 주차장번호,전송하는장비번호(프로그램장비번호) , 발생장비번호,발생장비IP,   받을장비UNITNO  , 받을장비IP
    //    private Command mCurrentCommand = Command.NONCOMMAND;
    //    private string mSendParkNO = string.Empty;
    //    private string mSendUnitNo = string.Empty;
    //    private string mSendDateTIme = string.Empty;
    //    private string mReceiveParkNO = string.Empty;
    //    private string mReceiveUnitNo = string.Empty;

    //    private string mSubCommand = string.Empty;
    //    private bool m_IsSuccess = false;
    //    public const char _STX_ = (char)0x02;
    //    public const char _ETX_ = (char)0x03;
    //    public const char _FS_ = (char)0x1C;
    //    public const char _GS_ = (char)0x1D;
    //    public const char _RS_ = (char)0x1E;
    //    public const char _US_ = (char)0x1F;

    //    public ProtocolData()
    //    {
    //        mCurrentCommand = Command.NONCOMMAND;
    //        mSendParkNO = string.Empty;
    //        mSendDateTIme = string.Empty;
    //        mReceiveParkNO = string.Empty;
    //        mReceiveUnitNo = string.Empty;
    //        mSubCommand = string.Empty;
    //        m_IsSuccess = false;

    //    }
    //    public ProtocolData(string p_message)
    //    {
    //        GetProtocolDataInfo(p_message);
    //    }
    //    /// <summary>
    //    /// 들어온데이터가 정상적이면 true반환
    //    /// </summary>
    //    /// <returns></returns>
    //    public bool isCommandRight()
    //    {
    //        if (CurrentCommand == Command.NONCOMMAND)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }
    //    /// <summary>
    //    /// 받은 문자메세지를 ProtocolData 멤버변수 형식으로 변환
    //    /// </summary>
    //    /// <param name="p_message"></param>
    //    private void GetProtocolDataInfo(string p_message)
    //    {
    //        try
    //        {

    //            p_message = p_message.Replace(TotalContorolProtocol._STX_.ToString(), "").Replace(TotalContorolProtocol._ETX_.ToString(), "").Trim();
    //            string[] l_SplitReceiveMessage = p_message.Split(TotalContorolProtocol._FS_); // 데이터 분리
    //            CurrentCommand = TotalContorolProtocol.GetCommandNameAsEnum(l_SplitReceiveMessage[(int)CommandRegulation.COMMAND]);
    //            mSendParkNO = l_SplitReceiveMessage[(int)CommandRegulation.SENDPARKNO];
    //            mSendUnitNo = l_SplitReceiveMessage[(int)CommandRegulation.SENDUNITNO];
    //            mSendDateTIme = l_SplitReceiveMessage[(int)CommandRegulation.SENDDATETIME];
    //            mReceiveParkNO = l_SplitReceiveMessage[(int)CommandRegulation.RECEIVEPARKNO];
    //            mReceiveUnitNo = l_SplitReceiveMessage[(int)CommandRegulation.RECEIVEUNITNO];
    //            SubCommand = l_SplitReceiveMessage[(int)CommandRegulation.SubCommand];

    //        }
    //        catch (Exception ex)
    //        {
    //            TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "TotalContorolProtocol.GetProtocolDataInfo", "예외사항:" + ex.ToString());
    //            CurrentCommand = Command.NONCOMMAND;
    //        }

    //    }
    //    /// <summary>
    //    /// Protocoldata 복사
    //    /// </summary>
    //    /// <param name="pProtocolData"></param>
    //    public void Clone(ProtocolData pProtocolData)
    //    {
    //        mCurrentCommand = pProtocolData.CurrentCommand;
    //        mSendParkNO = pProtocolData.SendParkNo;
    //        mSendUnitNo = pProtocolData.SendUnitNO;
    //        mSendDateTIme = pProtocolData.SendDateTime;
    //        mReceiveParkNO = pProtocolData.ReceiveParkNO;
    //        mReceiveUnitNo = pProtocolData.ReceiveUnitNo;
    //        SubCommand = pProtocolData.SubCommand;
    //    }

    //    public string[] GetSubCommandSplit()
    //    {
    //        return SubCommand.Split(_RS_);
    //    }

    //    /// <summary>
    //    /// 명령어
    //    /// </summary>
    //    public Command CurrentCommand
    //    {
    //        set { mCurrentCommand = value; }
    //        get { return mCurrentCommand; }
    //    }

    //    public string SendParkNo
    //    {
    //        set { mSendParkNO = value; }
    //        get { return mSendParkNO; }

    //    }

    //    public string SendUnitNO
    //    {
    //        set { mSendUnitNo = value; }
    //        get { return mSendUnitNo; }
    //    }



    //    public string SendDateTime
    //    {
    //        set { mSendDateTIme = value; }
    //        get { return mSendDateTIme; }

    //    }




    //    public string ReceiveParkNO
    //    {
    //        set { mReceiveParkNO = value; }
    //        get { return mReceiveParkNO; }
    //    }

    //    public string ReceiveUnitNo
    //    {
    //        set { mReceiveUnitNo = value; }
    //        get { return mReceiveUnitNo; }
    //    }



    //    /// <summary>
    //    /// 보조 명령어
    //    /// </summary>
    //    public string SubCommand
    //    {
    //        set { mSubCommand = value; }
    //        get { return mSubCommand; }
    //    }

    //    public string SendMakeMessage
    //    {
    //        get
    //        {
    //            return (_STX_.ToString() + mCurrentCommand.ToString() + _FS_.ToString()
    //                                     + mSendParkNO + _FS_.ToString()
    //                                     + mSendUnitNo + _FS_.ToString()
    //                                     + mSendDateTIme + _FS_.ToString()
    //                                     + mReceiveParkNO + _FS_.ToString()
    //                                     + mReceiveUnitNo + _FS_.ToString()
    //                                     + SubCommand + _ETX_.ToString()).ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// 성공여부
    //    /// </summary>
    //    public bool IsSuccess
    //    {
    //        set { m_IsSuccess = value; }
    //        get { return m_IsSuccess; }
    //    }
    //    /// <summary>
    //    /// 요금문의전문
    //    /// </summary>
    //    /// <param name="pCarInfo"></param>
    //    /// <returns></returns>
    //    public void SubRequestGET_CARPAY(NormalCarInfo pCarInfo)
    //    {
    //        StringBuilder sendGetPay = new StringBuilder();
    //        sendGetPay.Append(pCarInfo.ParkNo.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkNO);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InCarNumber);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkType);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InYMD.Replace("-", ""));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InHMS.Replace(":", ""));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(DateTime.Now.ToString("yyyyMMdd"));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(DateTime.Now.ToString("HHmmss"));
    //        SubCommand = sendGetPay.ToString();

    //    }

    //    public void SubRequestGET_CARPAY(DataRow pCarInfoDatarow)
    //    {
    //        StringBuilder sendGetPay = new StringBuilder();
    //        sendGetPay.Append(pCarInfoDatarow["parkNO"].ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfoDatarow["tkno"].ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfoDatarow["incarNO1"].ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfoDatarow["TkType"].ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfoDatarow["Procdate"].ToString().Replace("-", ""));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfoDatarow["ProcTime"].ToString().Replace(":", ""));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(DateTime.Now.ToString("yyyyMMdd"));
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(DateTime.Now.ToString("HHmmss"));
    //        SubCommand = sendGetPay.ToString();

    //    }

    //    public void SubRequestGET_RECEIPTNORAML(DataRow pCarInfoDatarow)
    //    {
    //        StringBuilder receiptnormal = new StringBuilder();
    //        receiptnormal.Append(pCarInfoDatarow["parkNO"].ToString());
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pCarInfoDatarow["tkno"].ToString());
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pCarInfoDatarow["OutDate"].ToString().Replace("-", ""));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pCarInfoDatarow["OutTime"].ToString().Replace(":", ""));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("HHmmss"));
    //        SubCommand = receiptnormal.ToString();

    //    }

    //    public void SubRequestGET_GosierNORAML(string pParkNo, string pTkno, string pName, string pPhone, string pRemark)
    //    {
    //        StringBuilder receiptnormal = new StringBuilder();
    //        receiptnormal.Append(pParkNo);
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pTkno);
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pName);
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pPhone);
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(pRemark);
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("HHmmss"));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
    //        receiptnormal.Append(_RS_.ToString());
    //        receiptnormal.Append(DateTime.Now.ToString("HHmmss"));

    //        SubCommand = receiptnormal.ToString();

    //    }
    //    /// <summary>
    //    /// 요금문의 전문 NormalCarInfo클래스로변환
    //    /// </summary>
    //    /// <returns></returns>
    //    public NormalCarInfo RequestGET_CARPAY_AsNormalCarInfo()
    //    {
    //        NormalCarInfo normalCarInfo = new NormalCarInfo();
    //        string[] carPay = SubCommand.Split(_RS_);
    //        int index = 0;
    //        normalCarInfo.ParkNo = Convert.ToInt32(carPay[index]);
    //        index += 1;
    //        normalCarInfo.TkNO = carPay[index];
    //        index += 1;
    //        normalCarInfo.InCarNumber = carPay[index];
    //        index += 1;
    //        normalCarInfo.TkType = carPay[index];
    //        index += 1;
    //        normalCarInfo.InYMD = carPay[index];
    //        index += 1;
    //        normalCarInfo.InHMS = carPay[index];
    //        index += 1;
    //        normalCarInfo.OutYmd = carPay[index];
    //        index += 1;
    //        normalCarInfo.OutHms = carPay[index];

    //        return normalCarInfo;

    //    }

    //    /// <summary>
    //    /// 요금문의 응답전문
    //    /// </summary>
    //    /// <param name="pCarInfo"></param>
    //    /// <returns></returns>
    //    public void SubResponeGET_CARPAY(NormalCarInfo pCarInfo)
    //    {
    //        StringBuilder sendGetPay = new StringBuilder();
    //        sendGetPay.Append(pCarInfo.ParkNo.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InUnitNo.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InCarPath.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InCarNumber.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InRCarPath.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InRCarNumber.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InYMD);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InHMS);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.PreInYmd);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.PreInHms);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.OutYmd);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.OutHms);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.OutChk);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkType);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.CarType);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.OutCarPath);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.ParkMoney.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.DiscountMoney.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.ParkTime.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkNO.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        for (int i = 0; i < pCarInfo.ListDcDetail.Count; i++)
    //        {
    //            sendGetPay.Append(pCarInfo.ListDcDetail[i].DcNo);
    //            if (i + 1 != pCarInfo.ListDcDetail.Count)
    //            {
    //                sendGetPay.Append(_RS_.ToString());
    //            }
    //        }
    //        SubCommand = sendGetPay.ToString();

    //    }

    //    public NormalCarInfo GetCarInfo(ProtocolData pProtocolData)
    //    {
    //        string pasingData = string.Empty;

    //        pasingData += pProtocolData.CurrentCommand.ToString();
    //        string[] rsData = pProtocolData.SubCommand.Split(_RS_);
    //        NormalCarInfo pCarInfo = new NormalCarInfo();
    //        int index = 0;
    //        pCarInfo.ParkNo = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += "[주차장번호]" + pCarInfo.ParkNo;
    //        index += 1;
    //        pCarInfo.OutCarNumber = rsData[index].ToString();
    //        pasingData += " [차량번호]" + pCarInfo.OutCarNumber.ToString();
    //        index += 1;
    //        pCarInfo.OutCarNumber1 = rsData[index].ToString();
    //        pasingData += " [OutCarNumber1]" + pCarInfo.OutCarNumber1.ToString();
    //        index += 1;
    //        pCarInfo.IO = rsData[index].ToString();
    //        pasingData += " [IO]" + pCarInfo.IO.ToString();
    //        index += 1;
    //        pCarInfo.OutYmd = rsData[index].ToString();
    //        pasingData += " [OutYmd]" + pCarInfo.OutYmd.ToString();
    //        index += 1;
    //        pCarInfo.OutHms = rsData[index].ToString();
    //        pasingData += " [OutHms]" + pCarInfo.OutHms.ToString();
    //        index += 1;
    //        pCarInfo.OutCarPath = rsData[index].ToString();
    //        pasingData += " [OutCarPath]" + pCarInfo.OutCarPath.ToString();
    //        index += 1;
    //        pCarInfo.CarKind = (rsData[index].ToString() == "0" ? "N" : "R");
    //        pasingData += " [CarKind]" + pCarInfo.CarKind.ToString();
    //        index += 1;
    //        pCarInfo.GroupName = rsData[index].ToString();
    //        pasingData += " [GroupName]" + pCarInfo.GroupName.ToString();
    //        return pCarInfo;

    //    }
    //    /// <summary>
    //    /// 받은 요금문의 전문 클래스로 변환
    //    /// </summary>
    //    /// <param name="pSubCommand"></param>
    //    /// <returns></returns>
    //    public NormalCarInfo GET_CARPAYAsNormalCarInfo(ProtocolData pProtocolData)
    //    {
    //        //string[] rsData =pSubCommand.Split(_RS_);
    //        //pSubCommand = @"121\\192.168.0.10\MSIMAGE\2016\04\19\CH1_20160419153630_32머9076.JPG32머9076201604191536302016041915531901\\192.168.0.20\MSIMAGE\2016\04\19\CH1_20160419155319_32머9076.JPG6000172016041915363020";
    //        string pasingData = string.Empty;

    //        pasingData += pProtocolData.CurrentCommand.ToString();
    //        string[] rsData = pProtocolData.SubCommand.Split(_RS_);
    //        NormalCarInfo pCarInfo = new NormalCarInfo();
    //        int index = 0;
    //        pCarInfo.ParkNo = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += "[주차장번호]" + pCarInfo.ParkNo;
    //        index += 1;
    //        pCarInfo.InUnitNo = rsData[index].ToString();
    //        pasingData += " [InUnitNo]" + pCarInfo.InUnitNo.ToString();
    //        index += 1;
    //        pCarInfo.InCarPath = rsData[index].ToString();
    //        pasingData += " [InCarPath]" + pCarInfo.InCarPath.ToString();
    //        index += 1;
    //        pCarInfo.InCarNumber = rsData[index].ToString();
    //        pasingData += " [InCarNumber]" + pCarInfo.InCarNumber.ToString();
    //        index += 1;
    //        pCarInfo.InRCarPath = rsData[index].ToString();
    //        pasingData += " [InRCarPath]" + pCarInfo.InRCarPath.ToString();
    //        index += 1;
    //        pCarInfo.InRCarNumber = rsData[index].ToString();
    //        pasingData += " [InRCarNumber]" + pCarInfo.InRCarNumber.ToString();
    //        index += 1;
    //        pCarInfo.InYMD = NPSYS.ConvetYears_Dash(rsData[index].ToString());
    //        pasingData += " [InYMD]" + pCarInfo.InYMD.ToString();
    //        index += 1;
    //        pCarInfo.InHMS = NPSYS.ConvetDay_Dash(rsData[index].ToString());
    //        pasingData += " [InHMS]" + pCarInfo.InHMS.ToString();
    //        index += 1;
    //        pCarInfo.PreInYmd = rsData[index].ToString();
    //        pasingData += " [PreInYmd]" + pCarInfo.PreInYmd.ToString();
    //        index += 1;
    //        pCarInfo.PreInHms = rsData[index].ToString();
    //        pasingData += " [PreInHms]" + pCarInfo.PreInHms.ToString();
    //        index += 1;
    //        pCarInfo.OutYmd = NPSYS.ConvetYears_Dash(rsData[index].ToString());
    //        pasingData += " [OutYmd]" + pCarInfo.OutYmd.ToString();
    //        index += 1;
    //        pCarInfo.OutHms = NPSYS.ConvetDay_Dash(rsData[index].ToString());
    //        pasingData += " [OutHms]" + pCarInfo.OutHms.ToString();
    //        index += 1;
    //        pCarInfo.OutChk = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [OutChk]" + pCarInfo.OutChk.ToString();
    //        index += 1;
    //        pCarInfo.TkType = rsData[index].ToString();
    //        pasingData += " [TkType]" + pCarInfo.TkType.ToString();
    //        index += 1;
    //        pCarInfo.CarType = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [CarType]" + pCarInfo.CarType.ToString();
    //        index += 1;
    //        pCarInfo.OutCarPath = rsData[index].ToString();
    //        pasingData += " [OutCarPath]" + pCarInfo.OutCarPath.ToString();
    //        index += 1;
    //        pCarInfo.ParkMoney = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [ParkMoney]" + pCarInfo.ParkMoney.ToString();
    //        index += 1;
    //        pCarInfo.DiscountMoney = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [DiscountMoney]" + pCarInfo.DiscountMoney.ToString();
    //        index += 1;
    //        pCarInfo.ParkTime = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [ParkTime]" + pCarInfo.ParkTime.ToString();
    //        index += 1;
    //        pCarInfo.TkNO = rsData[index].ToString();
    //        pasingData += " [TkNO]" + pCarInfo.TkNO.ToString();
    //        index += 1;

    //        pCarInfo.CurrentMoney = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [CurrentMoney]" + pCarInfo.CurrentMoney.ToString();
    //        index += 1;
    //        pCarInfo.CardPay = Convert.ToInt32(rsData[index].ToString());
    //        pasingData += " [CardPay]" + pCarInfo.CardPay.ToString();
    //        index += 1;
    //        for (int i = index; i < rsData.Length; i++)
    //        {
    //            if (rsData[i].ToString().Trim() != string.Empty)
    //            {
    //                DcDetail dcdetail = new DcDetail();
    //                dcdetail.DcNo = rsData[i].ToString();
    //                pasingData += " [DCNO]" + dcdetail.DcNo.ToString();
    //                pCarInfo.ListDcDetail.Add(dcdetail);
    //            }
    //        }
    //        TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "TotalContorolProtocol | GET_CARPAYAsNormalCarInfo", "[받은데이터분석]" + pasingData);
    //        return pCarInfo;

    //    }

    //    /// <summary>
    //    /// 할인요청
    //    /// </summary>
    //    /// <param name="pCarInfo"></param>
    //    /// <returns></returns>
    //    public void SubRequestGET_DISCOUNT(NormalCarInfo pCarInfo, string pDcNo, string pIamgePath, string pRemark)
    //    {
    //        StringBuilder sendGetPay = new StringBuilder();
    //        sendGetPay.Append(pCarInfo.ParkNo.ToString());
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkNO);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InCarNumber);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.TkType);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InYMD);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pCarInfo.InHMS);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pDcNo);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pIamgePath);
    //        sendGetPay.Append(_RS_.ToString());
    //        sendGetPay.Append(pRemark);
    //        SubCommand = sendGetPay.ToString();

    //    }



    //    /// <summary>
    //    /// 할인요청
    //    /// </summary>
    //    /// <param name="pCarInfo"></param>
    //    /// <returns></returns>
    //    public void GET_DiscountAsNormalCarInfo(string pSubCommand, ref string pCarNumber, ref string pDcno, ref string pResult)
    //    {
    //        string[] rsData = pSubCommand.Split(_RS_);
    //        int index = 0;
    //        string ParkNo = rsData[index].ToString();
    //        index += 1;
    //        string TkNO = rsData[index].ToString();
    //        index += 1;
    //        string InCarNumber = rsData[index].ToString();
    //        pCarNumber = InCarNumber;
    //        index += 1;
    //        string TkType = rsData[index].ToString().Replace(":", "");
    //        index += 1;
    //        string InYMD = rsData[index].ToString().Replace("-", "");
    //        index += 1;
    //        string InHMS = rsData[index].ToString().Replace(":", "");
    //        index += 1;
    //        pDcno = rsData[index].ToString().Replace(":", "");
    //        index += 1;
    //        pResult = rsData[index].ToString().Replace(":", "");


    //    }

    //    public void SetCarInfo(NormalCarInfo pCarInfo)
    //    {
    //        SubCommand = pCarInfo.OutCarNumber + _RS_.ToString()
    //                    + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + _RS_.ToString()
    //                    + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + _RS_.ToString()
    //                    + NPSYS.ConvetYears_Dash(pCarInfo.OutYmd) + _RS_.ToString()
    //                    + NPSYS.ConvetDay_Dash(pCarInfo.OutHms) + _RS_.ToString()
    //                    + pCarInfo.TkNO + _RS_.ToString()
    //                    + pCarInfo.GroupName + _RS_.ToString()
    //                    + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + " " + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + _RS_.ToString()
    //                    + (pCarInfo.CarKind == "N" ? "0" : "1") + _RS_.ToString()
    //                    + (pCarInfo.OutRecog1 == 1 ? "1" : "0") + _RS_.ToString()
    //                    + (pCarInfo.IsBarOpen == true ? "1" : "0") + _RS_.ToString()
    //                    + pCarInfo.InCarPath + _RS_.ToString()
    //                    + pCarInfo.OutCarPath + _RS_.ToString();


    //        //차량번호	입차일자	입차시간	출차일자	출차시간	TKNO	그룹명	출차시간	정기권유무	LPR판별상태	차단기열기상태
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="pUnitNo"></param>
    //    /// <param name="pBarStatus">0:OPEN,1:CLOSE,2:OPENLOCK,3:UNLOCK</param>
    //    public void SetBarControl(string pUnitNo, string pBarStatus)
    //    {
    //        SubCommand = pUnitNo + _RS_.ToString() + pBarStatus;
    //    }

    //}
}
