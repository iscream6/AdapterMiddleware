using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPAutoBoothControl
{
    public class Controller
    {
        private UserFunction mUserFunction = new UserFunction();
        private ParkInfo mCurrentSelectParkInfo = null; // 현재 선택한 주차장
        private NormalCarInfo mCurrentSyncNormalCarInfo = new NormalCarInfo();
        private FormInCar mFormInCar = new FormInCar();
        private FormImage mFormImage = new FormImage();
        private FormBarControl mFormBarControl = new FormBarControl();
        private FormResult mFormResult = new FormResult();
        private FormPreDiscount mFormPreDiscount = null;
        private NPINCarConfig m_npincarConfig;
        private bool mDiscountSuccess = false;
        NetCam.NetCamCtrl netCamCtrl1 = null;
        NetCam.NetCamCtrl netCamCtrl2 = null;
        private bool mIsIoBarOpenMode = false;  //차단기 자동수동모드 사용여부 false면 수동
        private FormSecondBarMode mFormBarMode = null;
        private FormSMSPowerControl mFormSMSPowerControl = null;
        private Dictionary<string, string> cbxQParkData;
        /// <summary>
        /// 견광등이 정상연결여부
        /// </summary>
        public static bool gIsUseKungandong = false;

        private bool mPreDcFlag = false; // 사전할인버튼 사용유무

        public Controller()
        {
            m_npincarConfig = NPSYS.Config;
        }

        public void Initialize()
        {
            try
            {

                cbxQParkData = mUserFunction.SetComboboxParkingData(); // 전체주차장정보를가져온다
                SetcbxQOutType(ref cbxQOutType);
                cbxQOutType.SelectedIndex = 1;
                lblCarType.Text = "소형";
                SetcbxCarType(ref cbxModifyCarType);
                cbxModifyCarType.Text = "소형";
                ClearCurrentDIsplay();
                mFormImage.Show();
                mFormImage.Visible = false;
                mFormImage.EventexitFormImage += new FormImage.exitFormImage(mFormImage_EventexitFormImage);

                mFormInCar.Show();
                mFormInCar.Visible = false;
                mFormInCar.EventExitPoint += new FormInCar.exitPoint(mFormInCar_EventExitPoint);
                mFormInCar.EventSendInData += new FormInCar.sendInData(mFormInCar_EventSendInData);

                mFormBarControl.Show();
                mFormBarControl.Visible = false;
                mFormBarControl.EventexitBarControl += new FormBarControl.exitBarControl(mFormBarControl_EventexitBarControl);
                mFormBarControl.EventsendBarControlData += new FormBarControl.sendBarControlData(mFormBarControl_EventsendBarControlData);

                mFormResult.Show();
                mFormResult.Visible = false;
                mUserFunction.SetCombo(cbxCurrentBooth, DBActionDML.GetAutoboothUnitInfo(NPCOMMON.GetParkInfo(ParkInfo.ParkInfoSetting.SettingParkNo), Convert.ToInt32(selected.Value.ToString()), m_npincarConfig), false);
                mFormResult.EventExitFormResultPoint += new FormResult.exitFormResultPoint(mFormResult_EventExitFormResultPoint);
                if (cbxCurrentBooth != null && cbxCurrentBooth.Items.Count > 0)
                {
                    cbxCurrentBooth.SelectedIndex = 0;
                }
                ComboboxItem parkData = new ComboboxItem();
                parkData.Value = cbxQParkData.Keys.First();
                parkData.Text = cbxQParkData[cbxQParkData.Keys.First()];

                //카메라처리 로직분리 적용
                if (m_npincarConfig.UseCamera)
                {
                    btnCameraView.Visible = true;
                    DataTable dtCameraInfo = DBActionDML.GetCameraInfo(NPCOMMON.GetParkInfo(Convert.ToInt32(parkData.Value)), cbxCurrentBooth.SelectedValue.ToString());
                    if (dtCameraInfo != null && dtCameraInfo.Rows.Count > 0)
                    {
                        npBevel7.Text = "신분증촬영 카메라";
                        netCamCtrl1 = new NetCam.NetCamCtrl();
                        netCamCtrl2 = new NetCam.NetCamCtrl();
                        groupBox2.Controls.Add(netCamCtrl1);
                        groupBox2.Controls.Add(netCamCtrl2);
                        netCamCtrl1.Location = new Point(438, 41);
                        netCamCtrl2.Location = new Point(7, 41);
                        netCamCtrl1.Size = new Size(411, 222);
                        netCamCtrl2.Size = new Size(411, 222);
                        //카메라 접속정보 DB옵션화 적용
                        netCamCtrl1.CameraID = dtCameraInfo.Rows[0]["CameraID"].ToString();
                        netCamCtrl1.CameraPW = dtCameraInfo.Rows[0]["CameraPW"].ToString();
                        netCamCtrl1.CameraIP = dtCameraInfo.Rows[0]["IpNo"].ToString();
                        netCamCtrl1.CameraPort = Convert.ToInt32(dtCameraInfo.Rows[0]["PortNo"].ToString());
                        //카메라 접속정보 DB옵션화 적용완료

                        netCamCtrl1.SetMode(true);

                        // 신규카메라 관련 변경
                        if (m_npincarConfig.CurrentCameraType == NPINCarConfig.CameraType.OLD)
                        {
                            netCamCtrl1.CurrentVersionType = NetCam.NetCamCtrl.VersionType.OldType;
                        }
                        else
                        {
                            netCamCtrl1.CurrentVersionType = NetCam.NetCamCtrl.VersionType.NewType;
                        }
                        if (m_npincarConfig.CurrentCameraStream == NPINCarConfig.CameraSTREAM.HD)
                        {
                            netCamCtrl1.CurrentSTREAMType = NetCam.NetCamCtrl.STREAM.HD;
                        }
                        else
                        {
                            netCamCtrl1.CurrentSTREAMType = NetCam.NetCamCtrl.STREAM.VGA;
                        }

                        netCamCtrl1.Size = new Size(411, 222);
                        // 신규카메라 관련 변경완료
                        netCamCtrl2.SetMode(true);
                        // 신규카메라 관련 변경
                        if (m_npincarConfig.CurrentCameraType == NPINCarConfig.CameraType.OLD)
                        {
                            netCamCtrl2.CurrentVersionType = NetCam.NetCamCtrl.VersionType.OldType;
                        }

                        else
                        {
                            netCamCtrl2.CurrentVersionType = NetCam.NetCamCtrl.VersionType.NewType;
                        }
                        if (m_npincarConfig.CurrentCameraStream == NPINCarConfig.CameraSTREAM.HD)
                        {
                            netCamCtrl2.CurrentSTREAMType = NetCam.NetCamCtrl.STREAM.HD;
                        }
                        else
                        {
                            netCamCtrl2.CurrentSTREAMType = NetCam.NetCamCtrl.STREAM.VGA;
                        }

                        netCamCtrl2.Size = new Size(411, 222);
                        // 신규카메라 관련 변경완료
                        netCamCtrl1.Visible = false;
                        netCamCtrl2.Visible = false;


                        netCamCtrl1.SendToBack();
                        netCamCtrl2.SendToBack();


                        //netCamCtrl1.Connect(m_npincarConfig.CarmeraIp, m_npincarConfig.CarmeraPort);
                    }
                }
                //카메라처리 로직분리 적용완료
                if (m_npincarConfig.UseInCarBOpenMode)
                {
                    btnBarOpenMode.Visible = true;
                }
                if (m_npincarConfig.UseElecAwning)
                {
                    btnElecAwningCtl.Visible = true;
                }
                //원격 영수증발행 옵션처리 적용
                if (m_npincarConfig.UseReceiptPrint)
                {
                    btnReceiptSend.Visible = true;
                }
                btnGosiSer.Visible = m_npincarConfig.UseGosiser;
                //원격 영수증발행 옵션처리 적용완료

                // 사전할인 버튼 사용유무
                if (m_npincarConfig.UsePreDiscount)
                {
                    btnUsePreDC.Visible = true;

                    SendBarOpenMode("NORMAL_OPEN");
                }
                //무인정산기 개방모드 적용
                btnOpenMode.Visible = m_npincarConfig.UseOpenMode;
                btnOpenMode.Tag = string.Empty;
                //무인정산기 개방모드 적용완료

                //2020-04 이재영 : 신용카드 취소기능 추가
                btnPayCancel.Visible = m_npincarConfig.UseCardCancel;
                //2020-04 이재영 : 신용카드 취소기능 추가완료

                //IO보드 통신적용
                if (m_npincarConfig.UseIObarControl)
                {

                    //KunangDongTcp = new KunangDongTcp();
                    //KunangDongTcp.Ip = m_npincarConfig.IOBoardIP;
                    //KunangDongTcp.Port = Convert.ToInt32(m_npincarConfig.IOBoardPORT);
                    //KunangDongTcp.EventSendKunangDongSignal += new KunangDongTcp.SendKunangDongSignal(ReciveSinHoGhanJe);
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "FormMain | FormMain_Load", "[견광등연동]" + "  ip:" + m_npincarConfig.IOBoardIP + " port:" + m_npincarConfig.IOBoardPORT);
                    //KunangDongTcp.IsConnect();
                    //KunangDongTcp.KunangDongThreadStart();

                    mFormBarMode = new FormSecondBarMode(false);
                    mFormBarMode.EventsendIOBarControl += new FormSecondBarMode.sendIOBarControl(frmMain.KungangSend);

                    btnSecondBarControl.Visible = m_npincarConfig.UseIObarOnOffMode;

                }
                //IO보드 통신적용 완료
                //SMS전원제어장비기능 적용
                if (m_npincarConfig.UseSMSPowerControl)
                {
                    mFormSMSPowerControl = new FormSMSPowerControl();

                    bool bOK = InitSMSPowerControl();
                    if (bOK)
                    {
                        btnSMSPowerControl.Visible = true;
                    }
                }
                //SMS전원제어장비기능 적용완료
                lblCurrentModeName.Text = "현재연결장비" + System.Environment.NewLine + "(단축키 F1~F9)";
            }
            catch (Exception ex)
            {
                TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "frmPaymentAndCarModify | rptCardData_Load", "[프로그램 로딩오류]" + ex.ToString());
                MessageBox.Show(new Form() { TopMost = true }, "예외사항:" + ex.ToString());
            }
        }
    }
}
