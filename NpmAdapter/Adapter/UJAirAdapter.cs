using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Model;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class UJAirAdapter : IAdapter
    {
		public IAdapter TargetAdapter { get; set; }

		private string strUri = null;
		private StringBuilder receiveMessageBuffer = new StringBuilder();
		byte[] arrKey = new byte[16];
		
		public bool IsRuning => true;

		public void Dispose()
		{
			
		}

		public bool Initialize()
		{
			//----- 우정항공 Key 값 ------
			arrKey[0] = 0xD0;
			arrKey[1] = 0x2C;
			arrKey[2] = 0x41;
			arrKey[3] = 0x4A;
			arrKey[4] = 0x6F;
			arrKey[5] = 0x70;
			arrKey[6] = 0xC3;
			arrKey[7] = 0xB0;
			arrKey[8] = 0x0A;
			arrKey[9] = 0x7D;
			arrKey[10] = 0xDB;
			arrKey[11] = 0x87;
			arrKey[12] = 0xC3;
			arrKey[13] = 0x94;
			arrKey[14] = 0xC6;
			arrKey[15] = 0x68;
			//----- 우정항공 Key 값 ------

			strUri = SysConfig.Instance.HW_Domain;
			if (strUri != null && strUri != "") 
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
		{
			//Payload 없이... 바로 JSON 처리를 하자...
			receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
			var jobj = JObject.Parse(receiveMessageBuffer.ToString());
			Thread.Sleep(10);
			receiveMessageBuffer.Clear();

			Log.WriteLog(LogType.Info, $"AptStAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
			JObject data = jobj["data"] as JObject; //응답 데이터
			string strMawb = Helper.NVL(data["mawb_no"]);

			if(strMawb != "")
			{
				//우정항공으로 Request를 한다.
				string responseData = string.Empty;
				string responseHeader = string.Empty;
				string encMawb = AESEncrypt(arrKey, strMawb); //AES 암호화
				Uri uri = new Uri($"{strUri}?mawb={encMawb}");
				if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.GET, ContentType.FormData, new byte[] { }, ref responseData, ref responseHeader))
				{
					JObject responseJson = JObject.Parse(responseData);
					ResponseDataPayload payload = new ResponseDataPayload();
					
					ReponseEtcUJAirPayload dataPayload = new ReponseEtcUJAirPayload();
					dataPayload.Deserialize(responseJson);

					payload.command = CmdType.etc_uj_air;
					payload.data = dataPayload;
					payload.result = ResultType.OK;

					byte[] responseBuffer = payload.Serialize();
					TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
				}
			}
		}

		public bool StartAdapter()
		{
			return true;
		}

		public bool StopAdapter()
		{
			return true;
		}

		private GovInterfaceModel gov;
		public void TestReceive(byte[] buffer)
		{
			string strMawb = "98811111112";
			string responseData = string.Empty;
			string responseHeader = string.Empty;
			string encMawb = AESEncrypt(arrKey, strMawb); //AES 암호화
			Uri uri = new Uri($"{strUri}?mawb={encMawb}");

			if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.GET, ContentType.Text, new byte[] { },
				ref responseData, ref responseHeader)) ;
			{
				Log.WriteLog(LogType.Error, $"UJAirAdapter | TestReceive", $"{responseData}", LogAdpType.HomeNet);
			}
		}

		/// <summary>
		/// MAWB 값 AES 암호화
		/// </summary>
		/// <param name="key"></param>
		/// <param name="s"></param>
		/// <returns></returns>
		private static string AESEncrypt(byte[] key, string s)
		{
			StringBuilder sbResult = new StringBuilder();

			byte[] KeyArray = key;
			byte[] EncryptArray = UTF8Encoding.UTF8.GetBytes(s);

			RijndaelManaged Rdel = new RijndaelManaged();
			Rdel.Mode = CipherMode.ECB;
			Rdel.Padding = PaddingMode.PKCS7;
			Rdel.Key = KeyArray;

			ICryptoTransform CtransForm = Rdel.CreateEncryptor();
			byte[] ResultArray = CtransForm.TransformFinalBlock(EncryptArray, 0, EncryptArray.Length);

			foreach (byte b in ResultArray)
			{
				sbResult.AppendFormat("{0:x2}", b);
			}

			return sbResult.ToString();
		}
	}
}
