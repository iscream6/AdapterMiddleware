﻿using HttpServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexpaAdapterStandardLib
{
    public enum CmdType
    {
        none, 
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
        /// 방문 차량 등록
        /// </summary>
        visit_reg,
        /// <summary>
        /// 방문 차량 수정
        /// </summary>
        visit_modify,
        /// <summary>
        /// 방문 차량 삭제
        /// </summary>
        visit_del,
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
        bar_open
    }

    public delegate void SendToPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent = null);

    public interface INetwork
    {
        event SendToPeer ReceiveFromPeer;

        public Action OnConnectionAction { get; set; }

        void SendToPeer(byte[] buffer, long offset, long size);

        bool Run();

        bool Down();
    }
}
