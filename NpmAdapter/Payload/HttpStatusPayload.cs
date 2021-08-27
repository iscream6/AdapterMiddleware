//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;

namespace NpmAdapter.Payload
{
    [Serializable]
    public class HttpStatusPayload : IPayload
    {
        #region Enums

        public enum ID
        {
            /// <summary>
            /// 시리얼번호
            /// </summary>
            code,
            /// <summary>
            /// 입차종류 int 1:방문,2:정기,3:회차
            /// </summary>
            message,
            /// <summary>
            /// 차량종류  int 0:경차,1:일반,2:대형   
            /// </summary>
            description,
        }
        public enum BodyStatus
        {
            None,
            /// <summary>
            /// 400으로 응답왔는대 데이터 없는경우
            /// </summary>
            BADREQUEST,
            /// <summary>
            /// 500으로 응답왔는대 데이터 없는경우
            /// </summary>
            ERROR,
            /// <summary>
            /// 000200 응답리턴시 통신포맷정상
            /// </summary>
            ReturnSuccessFormat,
            /// <summary>
            /// 000500 응답리턴시 통신포맷오류
            /// </summary>
            ReturnFailFormat,
            /// <summary>
            /// 011200 차량정보 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessCar,
            /// <summary>
            /// 011500 차량정보 받았을때 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailCar,
            //021200 원격차량정보 받았을때 무인이 정상상태일시 응답
            ReturnSuccessRemoteCar,
            /// <summary>
            /// 021500 원격차량정보 받았을때 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailRemoteCar,
            /// <summary>
            ///  031200 원격할인을 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessRemote_discounts,
            /// <summary>
            ///  031500 원격할인을 받았을때 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailRemote_discountst,
            /// <summary>
            /// 031502 등록된 정보에 이상이 있습니다. (이게 나올떄는 결제시 그냥 처리해줘야한다
            /// </summary>
            ReturnFailDataError,
            /// <summary>
            ///  041200 LPR에서 받은 요금요청시 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessPayment,
            /// <summary>
            ///  041500 LPR에서 받은 요금요청시 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailPayment,
            /// <summary>
            ///  051200 원격요금조회 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessRemotePayment,
            /// <summary>
            ///  051500 원격요금조회 받았을때 무인이  비정상상태일시 응답
            /// </summary>
            ReturnFailRemotePayment,
            /// <summary>
            ///  061200 원격카드취소 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessRemoteCanclePaymnet,
            /// <summary>
            ///  061500 원격카드취소 받았을때 무인이  비정상상태일시 응답
            /// </summary>
            ReturnFailRemoteCanclePaymnet,
            /// <summary>
            ///  071200 원격영수증발급 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessRemoteReceiptPaymnet,
            /// <summary>
            ///  071500 원격영수증발급 받았을때 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailRemoteReceiptPaymnet,
            /// <summary>
            ///  081200 원격마감요청 받았을때 무인이 정상상태일시 응답
            /// </summary>
            ReturnSuccessRemoteClose,
            /// <summary>
            ///  081500 원격영수증발급 받았을때 무인이 비정상상태일시 응답
            /// </summary>
            ReturnFailRemoteClose,


            /// <summary>
            ///  "012200" 차량을 조회성공
            /// </summary>
            CarSearch_Success,
            /// <summary>
            /// "012404" 차량데이터가 없음
            /// </summary>
            CarSearch_NotFound,
            /// <summary>
            /// 131201 장비에러전송성공
            /// </summary>
            DeviceErrorSet_Success,
            /// <summary>
            /// 133200 장비에러전송성공
            /// </summary>
            DeviceErrorSet_Update,
            /// <summary>
            /// "012501"  등록되지않은장비
            /// </summary>
            CarSearch_BadRequest,
            /// <summary>
            /// 032200 결제조회성공
            /// </summary>
            PaySearch_Success,
            /// <summary>
            /// 032404 결제조회자료 없음
            /// </summary>
            PaySearch_NotFound,
            /// <summary>
            ///  032501 결제조회실패
            /// </summary>
            PaySearch_Error,
            /// <summary>
            /// 032502 이미 출차한 차량
            /// </summary>
            PaySearch_AlreadExit,
            /// <summary>
            /// 032503 입차자료없음
            /// </summary>
            PaySearch_NotIndata,
            /// <summary>
            ///021201   할인생성
            /// </summary>
            Discount_Success, //
            /// <summary>
            /// 021501 할인권정보누락
            /// </summary>
            Discount_Not, //
            /// <summary>
            /// 021502 등록되지 않은 할인권
            /// </summary>
            Discount_NotDcInfo,
            /// <summary>
            /// 021503 이미 사용한 할인권
            /// </summary>
            Discount_PreUsed,
            /// <summary>
            /// 021504 존재하지않은 차량입니다.
            /// </summary>
            Discount_NotCar,
            /// <summary>
            /// "021505" 이미출차한 차량입니다.
            /// </summary>
            Discount_AlreadExit,
            /// <summary>
            /// "021506" 유효기간 오류.
            /// </summary>
            Discount_NotDate,

            /// <summary>
            /// 023200  할인생성
            /// </summary>
            DiscountMod_Success,
            /// <summary>
            /// 023501 할인권정보누락
            /// </summary>
            DiscountMod_Not,
            /// <summary>
            /// 023502 이미 취소된 할인권 입니다
            /// </summary>
            DiscountMod_NotDcInfo,
            /// <summary>
            /// 023503 존재하지 않는 차량입니다.
            /// </summary>
            DiscountMod_NotCar,
            /// <summary>
            /// 023504 이미출차한 차량입니다.
            /// </summary>
            DiscountMod_AlreadExit,
            DiscountMod_NotAdd,

            /// <summary>
            /// 031201   결제 생성 성공
            /// </summary>
            PaymentDetail_Success,
            /// <summary>
            /// 031409 중복결제존재
            /// </summary>
            PaymentDetail_Duplicate,
            /// <summary>
            /// 043201 정기차량 수정성공
            /// </summary>
            SeasonTicketPayment_Success,
            /// <summary>
            /// 043409 중복된 정기차량이 존재합니다
            /// </summary>
            SeasonTicketPayment_Duplicate,
            /// <summary>
            /// 042200 정기권 차량조회성공
            /// </summary>
            SeasonTicketSearch_Success,
            /// <summary>
            /// 042204 정기권 차량조회성공
            /// </summary>
            SeasonTicketSearch_NotFound,

            /// <summary>
            /// 050999 Server통신오류(자체제작)
            /// </summary>
            Server_FailConnect,
            /// <summary>
            /// 062200 마감조회성공
            /// </summary>
            CloseInfo_Success,
            /// <summary>
            /// 062204 마감조회자료없음
            /// </summary>
            CloseInfo_NotFound,
            /// <summary>
            /// 062501 마감시 등록되지않은 장비
            /// </summary>
            CloseInfo_BadRequest,
            /// <summary>
            /// 061201 마감생성
            /// </summary>
            CloseSave_Success,
            /// <summary>
            /// 061409 중복된 마감이 존재합니다
            /// </summary>
            CloseSave_Duplicate,
            /// <summary>
            /// 061501 등록되지 않은 장비입니다
            /// </summary>
            CloseSave_BadRequest,
            /// <summary>
            /// 061502 이미 생성된 마감입니다.
            /// </summary>
            CloseSave_Already,
            /// <summary>
            /// 112200 개국성공
            /// </summary>
            Cert_Success,
            /// <summary>
            /// 091201 시제설정성공
            /// </summary>
            SizeSave_Success,
        }

        #endregion

        #region Member Fields

        private string mCode = string.Empty;

        public BodyStatus currentStatus = BodyStatus.None;

        #endregion

        #region Properties

        public string code
        {
            get { return mCode; }
            set
            {
                mCode = value;
                SetStatusCode(value);
            }
        }
        public string message { set; get; }
        public string description { set; get; }
        public bool Success { get; set; }

        #endregion

        #region Methods

        public void SetStatusCode(string pCode)
        {
            switch (mCode)
            {
                case "000200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessFormat;
                    break;
                case "000500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailFormat;
                    break;
                case "011200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessCar;
                    break;
                case "011500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailCar;
                    break;
                case "021200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemoteCar;
                    break;
                case "021500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemoteCar;
                    break;
                case "031200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemote_discounts;
                    break;
                case "031500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemote_discountst;
                    break;
                case "031502":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailDataError;
                    break;
                case "041200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessPayment;
                    break;
                case "041500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailPayment;
                    break;

                case "051200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemotePayment;
                    break;

                case "051500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemotePayment;
                    break;
                case "061200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemoteCanclePaymnet;
                    break;
                case "061500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemoteCanclePaymnet;
                    break;
                case "071200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemoteReceiptPaymnet;
                    break;
                case "071500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemoteReceiptPaymnet;
                    break;
                case "081200":
                    Success = true;
                    currentStatus = BodyStatus.ReturnSuccessRemoteClose;
                    break;
                case "081500":
                    Success = false;
                    currentStatus = BodyStatus.ReturnFailRemoteClose;
                    break;

                case "400":
                    Success = false;
                    currentStatus = BodyStatus.BADREQUEST;
                    break;
                case "500":
                    Success = false;
                    currentStatus = BodyStatus.ERROR;
                    break;
                case "012200":
                    Success = true;
                    currentStatus = BodyStatus.CarSearch_Success;
                    break;
                case "012404":
                    Success = false;
                    currentStatus = BodyStatus.CarSearch_NotFound;

                    break;
                case "012501":
                    Success = false;
                    currentStatus = BodyStatus.CarSearch_BadRequest;
                    break;
                case "131201":
                    Success = true;
                    currentStatus = BodyStatus.DeviceErrorSet_Success;
                    break;
                case "133200":
                    Success = true;
                    currentStatus = BodyStatus.DeviceErrorSet_Update; // 시티밸리 추가필요
                    break;

                case "032200":
                    Success = true;
                    currentStatus = BodyStatus.PaySearch_Success;
                    break;
                case "032404":
                    Success = false;
                    currentStatus = BodyStatus.PaySearch_NotFound;
                    break;
                case "032501":
                    Success = false;
                    currentStatus = BodyStatus.PaySearch_Error;
                    break;
                case "032502":
                    Success = false;
                    currentStatus = BodyStatus.PaySearch_AlreadExit;
                    break;
                case "032503":
                    Success = false;
                    currentStatus = BodyStatus.PaySearch_NotIndata;
                    break;
                case "021201":
                    Success = true;
                    currentStatus = BodyStatus.Discount_Success;
                    break;
                case "021501":
                    Success = false;
                    currentStatus = BodyStatus.Discount_Not;
                    break;
                case "021502":
                    Success = false;
                    currentStatus = BodyStatus.Discount_NotDcInfo;
                    break;
                case "021503":
                    Success = false;
                    currentStatus = BodyStatus.Discount_PreUsed;
                    break;
                case "021504":
                    Success = false;
                    currentStatus = BodyStatus.Discount_NotCar;
                    break;


                case "023200":
                    Success = true;
                    currentStatus = BodyStatus.DiscountMod_Success;
                    break;
                case "023501":
                    Success = false;
                    currentStatus = BodyStatus.DiscountMod_Not;
                    break;
                case "023502":
                    Success = false;
                    currentStatus = BodyStatus.DiscountMod_NotDcInfo;
                    break;
                case "023503":
                    Success = false;
                    currentStatus = BodyStatus.DiscountMod_NotCar;
                    break;

                case "023504":
                    Success = false;
                    currentStatus = BodyStatus.DiscountMod_AlreadExit;
                    break;
                case "031201":
                    Success = true;
                    currentStatus = BodyStatus.PaymentDetail_Success;
                    break;
                case "031409":
                    Success = false;
                    currentStatus = BodyStatus.PaymentDetail_Duplicate;
                    break;
                case "043201":
                    Success = true;
                    currentStatus = BodyStatus.SeasonTicketPayment_Success;
                    break;
                case "043409":
                    Success = false;
                    currentStatus = BodyStatus.SeasonTicketPayment_Duplicate;
                    break;
                case "050999":
                    Success = false;
                    currentStatus = BodyStatus.Server_FailConnect;
                    break;
                case "112200":
                    Success = true;
                    currentStatus = BodyStatus.Cert_Success;
                    break;
                case "062200":
                    Success = true;
                    currentStatus = BodyStatus.CloseInfo_Success;
                    break;
                case "062204":
                    Success = false;
                    currentStatus = BodyStatus.CloseInfo_NotFound;
                    break;
                case "061201":
                    Success = true;
                    currentStatus = BodyStatus.CloseSave_Success;
                    break;
                case "061409":
                    Success = false;
                    currentStatus = BodyStatus.CloseSave_Duplicate;
                    break;
                case "061501":
                    Success = false;
                    currentStatus = BodyStatus.CloseSave_BadRequest;
                    break;
                case "061502":
                    Success = false;
                    currentStatus = BodyStatus.CloseSave_Already;
                    break;

                case "042200":
                    Success = true;
                    currentStatus = BodyStatus.SeasonTicketSearch_Success;
                    break;

                case "042204":
                    Success = true;
                    currentStatus = BodyStatus.SeasonTicketSearch_NotFound;
                    break;
                case "091201":
                    Success = true;
                    currentStatus = BodyStatus.SizeSave_Success;

                    break;
                default:
                    Success = false;
                    currentStatus = BodyStatus.None;
                    break;

            }

        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JToken ToJson()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(JToken json)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
