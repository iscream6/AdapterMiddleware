//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Text;

namespace NpmAdapter.Payload
{
    public enum CmdType
    {
        none,
        /// <summary>
        /// 핸드쉐이크
        /// </summary>
        hello,
        /// <summary>
        /// 상태 체크
        /// </summary>
        status_check,
        /// <summary>
        /// 상태 체크 응답
        /// </summary>
        status_ack,
        /// <summary>
        /// 입차 차량 통보
        /// </summary>
        alert_incar,
        /// <summary>
        /// 출차 차량 통보
        /// </summary>
        alert_outcar,
        /// <summary>
        /// 입차 리스트
        /// </summary>
        incar_list,
        /// <summary>
        /// 방문 차량 리스트
        /// </summary>
        visit_list,
        /// <summary>
        /// 방문 차량 리스트 Type-2
        /// </summary>
        visit_list2,
        /// <summary>
        /// 단일 방문 차량 리스트
        /// </summary>
        visit_single_list,
        /// <summary>
        /// 단일 방문 차량 출입 리스트
        /// </summary>
        visit_single_io,
        /// <summary>
        /// 방문 차량 등록
        /// </summary>
        visit_reg,
        /// <summary>
        /// 방문 차량 등록 Type-2
        /// </summary>
        visit_reg2,
        /// <summary>
        /// 방문 차량 수정
        /// </summary>
        visit_modify,
        /// <summary>
        /// 방문 차량 삭제
        /// </summary>
        visit_del,
        /// <summary>
        /// 방문 차량 삭제 Type-2
        /// </summary>
        visit_del2,
        /// <summary>
        /// 방문 차량 즐겨찾기 리스트
        /// </summary>
        visit_favo_list,
        /// <summary>
        /// 방문 차량 즐겨찾기 등록
        /// </summary>
        visit_favo_reg,
        /// <summary>
        /// 방문 차량 즐겨찾기 삭제
        /// </summary>
        visit_favo_del,
        /// <summary>
        /// 방문 차량 체크
        /// </summary>
        visit_check,
        /// <summary>
        /// 결과 통보
        /// </summary>
        result,
        /// <summary>
        /// 전송 에러
        /// </summary>
        trans_error,
        /// <summary>
        /// 주차위치 맵 정보
        /// </summary>
        location_map,
        /// <summary>
        /// 주차위치 리스트
        /// </summary>
        location_list,
        /// <summary>
        /// 별명 수정
        /// </summary>
        modify_alias,
        /// <summary>
        /// 세대등록 차량찾기
        /// </summary>
        find_car,
        /// <summary>
        /// 블랙리스트 리스트
        /// </summary>
        blacklist_list,
        /// <summary>
        /// 블랙리스트 등록
        /// </summary>
        blacklist_reg,
        /// <summary>
        /// 블랙리스트 삭제
        /// </summary>
        blacklist_del,
        /// <summary>
        /// 블랙리스트 단일차량 조회
        /// </summary>
        blacklist_car,
        /// <summary>
        /// 핸드쉐이크겸...
        /// </summary>
        alive_check,
        /// <summary>
        /// 차단기열기
        /// </summary>
        bar_open,
        /// <summary>
        /// 일반차량 조회
        /// </summary>
        ion_list,
        /// <summary>
        /// 정기차량 조회
        /// </summary>
        ios_list,
        /// <summary>
        /// 정기차량 등록
        /// </summary>
        cust_reg,
        /// <summary>
        /// 정기차량 삭제
        /// </summary>
        cust_del,
        /// <summary>
        /// 정기차량 리스트
        /// </summary>
        cust_list,
        /// <summary>
        /// 정기차량 출입내역 리스트
        /// </summary>
        cust_io_list,
        /// <summary>
        /// 우정항공
        /// </summary>
        etc_uj_air,
        /// <summary>
        /// 세대 포인트 조회
        /// </summary>
        remain_point,
        /// <summary>
        /// 스마트빌리지 전용, 할당 동기화
        /// </summary>
        sync_assign
    }

    class RequestPayload<T> : IPayload where T : IPayload, new()
    {
        public CmdType command { get; set; }
        public T data { get; set; }

        public void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
            var dataJson = json["data"];
            data = new T();
            data.Deserialize(dataJson as JObject);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            json["data"] = data.ToJson();
            return json;
        }
    }

    class RequestEmptyPayload : IPayload
    {
        public CmdType command { get; set; }

        public void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            return json;
        }
    }
}
