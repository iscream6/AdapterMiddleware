﻿using Newtonsoft.Json.Linq;
using NpmCommon;
using System;
using System.ComponentModel;

namespace NpmAdapter.Payload
{
    public static class CmdHelper
    {
        public enum Category
        {
            none,
            board
        }

        public enum ErrorCode
        {
            none,
            OK = 200,
            CREATE = 201,
            NO_CONTENT = 204,
            BAD_REQUEST = 400,
            UNAUTHORIZED = 401,
            FORBIDDEN = 403
        }

        public enum StatusCode
        {
            [Description("ok")]
            ok = 0,
            [Description("3rd Party 서버와 연결이 끊겨 있습니다")]
            disconnect = 101,
            [Description("3rd Party 서버에서 응답이 없습니다")]
            notresponse = 102,
            [Description("이미 등록된 블랙리스트 차량번호 입니다")]
            already_reg_blacklist_carnumber = 318,
            [Description("이미 즐겨찾기에 등록된 방문차량 번호 입니다")]
            already_reg_favorit_carnumber = 319,
            [Description("BAD REQUEST")]
            bad_request = 400,
            [Description("등록된 방문차량이 없습니다")]
            no_date_visit_car = 404,
            [Description("응용 프로그램이 현재 작업에 대해 잘못된 형식을 가진 값을 사용하고 있습니다")]
            Invaild_Type_Error =405,
            [Description("Internal Server Error")]
            server_error = 500,
            [Description("현재 주차관제 서버와 연동중이지 않습니다")]
            notinterface_kwanje = 503,
            [Description("현재 주차위치 서버와 연동중이지 않습니다")]
            notinterface_udo = 504,
            [Description("현재 원패스 서버와 연동중이지 않습니다")]
            notinterface_onepass = 505,
        }

        public enum Command
        {
            none,
            query_request,
            query_response,
            control_request,
            control_response,
        }

        public enum Type
        {
            none,
            /// <summary>
            /// 입차 리스트
            /// </summary>
            in_car,
            /// <summary>
            /// 방문 차량 리스트
            /// </summary>
            visitor_car_book_list,
            /// <summary>
            /// 방문 차량 등록
            /// </summary>
            visitor_car_book_add,
            /// <summary>
            /// 방문 차량 수정
            /// </summary>
            visitor_car_book_update,
            /// <summary>
            /// 방문 차량 삭제
            /// </summary>
            visitor_car_book_delete,
            /// <summary>
            /// 방문차량 즐겨찾기 리스트
            /// </summary>
            visitor_car_favorites_list,
            /// <summary>
            /// 방문차량 즐겨찾기 등록
            /// </summary>
            visitor_car_favorites_add,
            /// <summary>
            /// 방문차량 즐겨찾기 삭제
            /// </summary>
            visitor_car_favorites_delete,
            /// <summary>
            /// 가족위치 / 차량위치 리스트 요청
            /// </summary>
            location_list,
            /// <summary>
            /// 가족위치 / 차량위치 맵 정보 요청
            /// </summary>
            location_map,
            /// <summary>
            /// 주차위치 별명수정
            /// </summary>
            location_nick,
            /// <summary>
            /// 주차위치 세대등록 차량 조회
            /// </summary>
            location_car_list,
            /// <summary>
            /// 블랙리스트 리스트
            /// </summary>
            blacklist_book_list,
            /// <summary>
            /// 블랙리스트 등록
            /// </summary>
            blacklist_book_add,
            /// <summary>
            /// 블랙리스트 삭제
            /// </summary>
            blacklist_book_delete,
            /// <summary>
            /// 블랙리스트 단일 차량 조회
            /// </summary>
            blacklist_book_car
        }

        public enum CarType
        {
            none,
            all,
            family,
            visitor
        }

        public struct ResultReqInfo
        {
            public IPayload payload;
            public CmdType type;
        }

        /// <summary>
        /// 대림코맥스 Json으로 넥스파 요청 Payload를 만들어 반환한다.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static ResultReqInfo ConvertNexpaRequestPayload(JObject json)
        {
            CmdHeader header = new CmdHeader();
            header.Deserialize(json["header"] as JObject);
            string dh; //동호
            ResultReqInfo result = new ResultReqInfo();
            result.payload = null;
            result.type = CmdType.none;

            switch (header.type)
            {
                case Type.in_car: //입차 리스트 요청
                    {
                        RequestPayload<RequestCarListPayload> payload = new RequestPayload<RequestCarListPayload>();
                        payload.command = CmdType.incar_list;

                        RequestCarListPayload data = new RequestCarListPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.type = (NexpaPayloadManager.Type)Enum.Parse(typeof(NexpaPayloadManager.Type), json["data"]["car_type"].ToString());
                        data.page = json["data"]["page"]?.ToString();
                        data.count = json["data"]["count"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.visitor_car_book_list: //방문차량 리스트 요청
                    {
                        RequestPayload<RequestVisitListPayload> payload = new RequestPayload<RequestVisitListPayload>();
                        payload.command = CmdType.visit_list;

                        RequestVisitListPayload data = new RequestVisitListPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if(dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.page = json["data"]["page"]?.ToString();
                        data.count = json["data"]["count"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.visitor_car_book_add: //방문차량 등록 요청
                    {
                        RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                        payload.command = CmdType.visit_reg;

                        RequestVisitRegPayload data = new RequestVisitRegPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.date = json["data"]["reg_date"]?.ToString().Replace("-", string.Empty);
                        data.term = json["data"]["term"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.visitor_car_book_update:
                    {
                        RequestPayload<RequestVisitModifyPayload> payload = new RequestPayload<RequestVisitModifyPayload>();
                        payload.command = CmdType.visit_modify;

                        RequestVisitModifyPayload data = new RequestVisitModifyPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.date = json["data"]["reg_date"]?.ToString().Replace("-", string.Empty);
                        data.term = json["data"]["term"]?.ToString();
                        data.reg_no = json["data"]["reg_num"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.visitor_car_book_delete: //방문차량 삭제 요청
                    {
                        RequestPayload<RequestVisitDelPayload> payload = new RequestPayload<RequestVisitDelPayload>();
                        payload.command = CmdType.visit_del;

                        RequestVisitDelPayload data = new RequestVisitDelPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.reg_no = json["data"]["reg_num"]?.ToString();
                        data.car_number = json["data"]["car_num"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                    
                case Type.visitor_car_favorites_list: //방문차량 즐겨찾기 리스트 요청
                    {
                        RequestPayload<RequestVisitFavoListPayload> payload = new RequestPayload<RequestVisitFavoListPayload>();
                        payload.command = CmdType.visit_favo_list;

                        RequestVisitFavoListPayload data = new RequestVisitFavoListPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                    
                case Type.visitor_car_favorites_add: //방문차량 즐겨찾기 등록 요청
                    {
                        RequestPayload<RequestVisitFavoRegPayload> payload = new RequestPayload<RequestVisitFavoRegPayload>();
                        payload.command = CmdType.visit_favo_reg;

                        RequestVisitFavoRegPayload data = new RequestVisitFavoRegPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.register = json["data"]["register"]?.ToString(); //신규 추가됨.
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                    
                case Type.visitor_car_favorites_delete: //방문차량 즐겨찾기 삭제 요청
                    {
                        RequestPayload<RequestVisitFavoDelPayload> payload = new RequestPayload<RequestVisitFavoDelPayload>();
                        payload.command = CmdType.visit_favo_del;

                        RequestVisitFavoDelPayload data = new RequestVisitFavoDelPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.reg_no = json["data"]["reg_num"]?.ToString();
                        data.car_number = json["data"]["car_num"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.location_list:
                    {
                        //넥스파로 뭘 보내야 할까..
                        RequestPayload<RequestFindCarLocListPayload> payload = new RequestPayload<RequestFindCarLocListPayload>();
                        payload.command = CmdType.location_list;

                        RequestFindCarLocListPayload data = new RequestFindCarLocListPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.location_type = json["data"]["location_type"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.location_map:
                    {
                        RequestPayload<RequestFindCarLocPayload> payload = new RequestPayload<RequestFindCarLocPayload>();
                        payload.command = CmdType.location_map;

                        RequestFindCarLocPayload data = new RequestFindCarLocPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.car_number = json["data"]["tag_num"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.location_nick:
                    {
                        RequestPayload<RequestModifyAliasPayload> payload = new RequestPayload<RequestModifyAliasPayload>();
                        payload.command = CmdType.modify_alias;

                        RequestModifyAliasPayload data = new RequestModifyAliasPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.alias = json["data"]["alias"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.location_car_list:
                    {
                        RequestPayload<RequestCarInfoPayload> payload = new RequestPayload<RequestCarInfoPayload>();
                        payload.command = CmdType.find_car;

                        RequestCarInfoPayload data = new RequestCarInfoPayload();
                        dh = json["data"]["dongho"]?.ToString();//앞 4자리 동, 뒤 4자리 호
                        if (dh != null && dh.Trim() != string.Empty)
                        {
                            data.dong = dh.Substring(0, 4).TrimStart(new Char[] { '0' });
                            data.ho = dh.Substring(4).TrimStart(new Char[] { '0' });
                        }
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.blacklist_book_list:
                    {
                        RequestPayload<RequestBlackListPayload> payload = new RequestPayload<RequestBlackListPayload>();
                        payload.command = CmdType.blacklist_list;

                        RequestBlackListPayload data = new RequestBlackListPayload();
                        data.page = json["data"]["page"]?.ToString();
                        data.count = json["data"]["count"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.blacklist_book_add:
                    {
                        RequestPayload<RequestBlackListRegPayload> payload = new RequestPayload<RequestBlackListRegPayload>();
                        payload.command = CmdType.blacklist_reg;

                        RequestBlackListRegPayload data = new RequestBlackListRegPayload();
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.reason = json["data"]["reason"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.blacklist_book_delete:
                    {
                        RequestPayload<RequestBlackListDelPayload> payload = new RequestPayload<RequestBlackListDelPayload>();
                        payload.command = CmdType.blacklist_del;

                        RequestBlackListDelPayload data = new RequestBlackListDelPayload();
                        data.car_number = json["data"]["car_num"]?.ToString();
                        data.reg_no = json["data"]["reg_num"]?.ToString();
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                case Type.blacklist_book_car:
                    {
                        RequestPayload<RequestBlackListCarPayload> payload = new RequestPayload<RequestBlackListCarPayload>();
                        payload.command = CmdType.blacklist_car;
                        RequestBlackListCarPayload data = new RequestBlackListCarPayload();
                        data.car_number = Helper.NVL(json["data"]["car_num"]);
                        payload.data = data;

                        result.payload = payload;
                        result.type = payload.command;
                        return result;
                    }
                default:
                    return result;
            }
        }

        public static ResultPayload MakeResponseResultPayload(StatusCode status)
        {
            ResultPayload payload = new ResultPayload();
            payload.code = (int)status == 0 ? "000" : ((int)status).ToString();
            payload.message = status.GetDescription();

            return payload;
        }

        public static IPayload MakeResponseDataPayload(Type type, JToken json)
        {
            IPayload payload = null;
            switch (type)
            {
                case Type.in_car:
                    payload = new ResponseCmdListData<ResponseCmdIncarListData>();
                    payload.Deserialize(json);
                    break;
                case Type.visitor_car_book_list:
                    payload = new ResponseCmdListData<ResponseCmdVisitListData>();
                    payload.Deserialize(json);
                    break;
                case Type.visitor_car_favorites_list:
                    payload = new ResponseCmdVisitFavoritListData();
                    payload.Deserialize(json);
                    break;
                case Type.visitor_car_book_add:
                    payload = new ResponseCmdRegNumData();
                    payload.Deserialize(json);
                    break;
                case Type.location_car_list:
                    payload = new ResponseCmdFindCarListData();
                    payload.Deserialize(json);
                    break;
            }

            return payload;
        }
    }
}
