using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common
{
    public partial class UserFunction
    {
        public void AppendTextData(ref StringBuilder sSQL, params Object[] sText)
        {
            for (int i = 0; i < sText.Length; i++)
            {
                if (((TextBox)sText[i]).Text.Length > 0)
                {
                    sSQL.AppendFormat("'{0}',", ((TextBox)sText[i]).Text);
                }
                else
                {
                    sSQL.Append("null,");
                }
            }
            //if (RightStr(sSQL, 1) != ",") sSQL.Append(",");
        }

        public void AppendTextData(ref String sSQL, params Object[] sText)
        {
            for (int i = 0; i < sText.Length; i++)
            {
                if (((TextBox)sText[i]).Text.Length > 0)
                {
                    sSQL += String.Format("'{0}',", ((TextBox)sText[i]).Text);
                }
                else
                {
                    sSQL += "null,";
                }
            }
            //if (RightStr(sSQL, 1) != ",") sSQL += ",";
        }

        public void AppendTextDataNullCheck(ref StringBuilder sSQL, TextBox sText)
        {
            if (sText.TextLength == 0)
            {
                sSQL.Append("null,");
            }
            else
            {
                sSQL.AppendFormat("'{0}',", sText.Text);
            }
        }

        public void AppendTextDataNullCheck(ref String sSQL, TextBox sText)
        {
            if (sText.TextLength == 0)
            {
                sSQL += "null,";
            }
            else
            {
                sSQL += String.Format("'{0}',", sText.Text);
            }
        }

        public void AppendIntDataBaseZero(ref StringBuilder sSQL, params Object[] sText)
        {
            for (int i = 0; i < sText.Length; i++)
            {
                if (((TextBox)sText[i]).Text.Length > 0)
                {
                    sSQL.AppendFormat("{0},", Convert.ToInt32(((TextBox)sText[i]).Text));
                }
                else
                {
                    sSQL.Append("0,");
                }
            }
        }

        public void AppendIntDataBaseZero(ref String sSQL, params Object[] sText)
        {
            for (int i = 0; i < sText.Length; i++)
            {
                if (((TextBox)sText[i]).Text.Length > 0)
                {
                    sSQL += String.Format("{0},", Convert.ToInt32(((TextBox)sText[i]).Text));
                }
                else
                {
                    sSQL += "0,";
                }
            }
        }

        public Boolean RunDMLExcute(NPDB pNPDB, StringBuilder sSQL)
        {
            Boolean bOK = false;
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = pNPDB.GetCurrentConnection;
                sCmd.CommandText = sSQL.ToString();
                sCmd.ExecuteNonQuery();
                bOK = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Routine: RunDML");
                bOK = false;
            }

            return bOK;
        }


        public Boolean RunDMLExcute(NPDB pNPDB, String sSQL)
        {
            Boolean bOK = false;
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = pNPDB.GetCurrentConnection;
                sCmd.CommandText = sSQL;
                sCmd.ExecuteNonQuery();
                bOK = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Routine: RunDML");
                bOK = false;
            }

            return bOK;
        }

        private String RightStr(String s, int nSize)
        {
            int nLen = s.Length;
            int nGet = nSize;

            if (nLen < 1) return String.Empty;
            if (nLen < nGet) nGet = nLen;
            if (nGet == 0) nGet = 1;

            return s.Substring(nLen - 1, nGet);
        }

        private String RightStr(StringBuilder s, int nSize)
        {
            int nLen = s.Length;
            int nGet = nSize;

            if (nLen < 1) return String.Empty;
            if (nLen < nGet) nGet = nLen;
            if (nGet == 0) nGet = 1;

            return s.ToString().Substring(nLen - 1, nGet);
        }

        public String AddQuoteNULL(String sData)
        {
            String sRet = sData.Trim().Length > 0 ? String.Format("'{0}'", sData.Trim()) : "null";
            return sRet;
        }

        public String AddQuoteSetString(String sData, String sRet)
        {
            return sData.Trim().Length > 0 ? "'" + sData.Trim() + "'" : sRet;
        }

        public String EmptyToZero(String sData)
        {
            return sData.Trim().Length > 0 ? sData.Trim() : "0";
        }

        public String EmptyToValue(String sData, String sValue)
        {
            return sData.Trim().Length > 0 ? sData.Trim() : sValue;
        }

        public String EmptyToNULL(String sData)
        {
            return sData.Trim().Length > 0 ? sData.Trim() : "null";
        }

        /// <summary>
        /// 콤보박스 설정
        /// </summary>
        /// <param name="cbx"></param>
        /// <param name="arr"></param>
        /// <param name="nStart"></param>
        /// <returns></returns>
        public Boolean SetCombo(ComboBox cbx, String[] arr, int nStart)
        {
            if (arr.Length.Equals(0)) return false;

            int nMax = arr.Length;
            for (int i = nStart; i < nMax; i++)
            {
                cbx.Items.Add(arr[i]);
            }

            return true;
        }

        public void SetCombo(ComboBox cbx, String[] arr)
        {
            SetCombo(cbx, arr, 0);
        }

        public void SetCombo(ComboBox cbx, String[] arr, int nStart, int nSelect)
        {
            if (SetCombo(cbx, arr, nStart))
                cbx.SelectedIndex = nSelect;
        }

        /// <summary>
        /// combobox에 text 및 value값을 넣는다
        /// </summary>
        /// <param name="p_ComboBox"></param>
        /// <param name="p_data"></param>
        public void SetCombo(ComboBox p_ComboBox, object p_data, bool p_UseNoSelect)
        {
            p_ComboBox.DisplayMember = "Text";
            p_ComboBox.ValueMember = "Value";
            List<ComboboxItem> BindingData = new List<ComboboxItem>();
            ComboboxItem item = new ComboboxItem();
            if (p_UseNoSelect)
            {
                item.Text = NPDefine.NO_SELECT;
                item.Value = "-1";
                BindingData.Add(item);
            }


            if (p_data is DataTable)
            {
                DataTable l_datatable = (DataTable)p_data;
                foreach (DataRow l_DatatableRow in l_datatable.Rows)
                {
                    item = new ComboboxItem();
                    item.Text = l_DatatableRow[1].ToString();
                    item.Value = l_DatatableRow[0].ToString();
                    BindingData.Add(item);
                }
                p_ComboBox.DataSource = BindingData;
            }
            else if (p_data is DataRow[])
            {

                foreach (DataRow DatarowItem in (DataRow[])p_data)
                {
                    item = new ComboboxItem();
                    item.Text = DatarowItem[1].ToString();
                    item.Value = DatarowItem[0].ToString();
                    BindingData.Add(item);
                }
                p_ComboBox.DataSource = BindingData;
            }
        }



        /// <summary>
        /// 데이터베이스에서 읽은 자료를 ComboBox에 자료와 값으로 넣는다
        /// </summary>
        public void SetComboDT(ComboBox cbx, Boolean bUseNoSelect, SqlConnection Conn, String sSQL, String sCode, String sName)
        {
            String NO_SELECT = "NO_SELECT";

            DataTable dt = new DataTable();
            dt.Columns.Add(sCode);
            dt.Columns.Add(sName);

            if (bUseNoSelect)
            {
                dt.Rows.Add(NO_SELECT, NPDefine.NO_SELECT);
            }

            dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
            (new SqlDataAdapter(sSQL.ToString(), Conn)).Fill(dt);

            cbx.DataSource = dt;

            cbx.ValueMember = sCode;
            cbx.DisplayMember = sName;
        }

        public void SetComboDT(ComboBox cbx, Boolean bUseNoSelect, DataTable pDt, String sCode, String sName)
        {
            String NO_SELECT = "NO_SELECT";

            //pDt.Columns.Add(sCode);
            //pDt.Columns.Add(sName);

            if (bUseNoSelect)
            {
                pDt.Rows.Add(NO_SELECT, NPDefine.NO_SELECT);
            }



            cbx.DataSource = pDt;

            cbx.ValueMember = sCode;
            cbx.DisplayMember = sName;
        }

        /// <summary>
        /// Control에 있는 모든 TextBox 초기화
        /// </summary>
        /// <param name="obj"></param>
        public void ObjectInitText(Control obj)
        {
            foreach (Control c in obj.Controls)
            {
                switch (c.GetType().Name.ToString())
                {
                    case "TextBox":
                        String sTag = (String)c.Tag;
                        if (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("PASS"))
                        {
                            c.Text = String.Empty;
                        }
                        break;
                }
            }
        }
        public void ObjectInitText(Control obj, Boolean bUpdate)
        {
            foreach (Control c in obj.Controls)
            {
                switch (c.GetType().Name.ToString())
                {
                    case "TextBox":
                        String sTag = (String)c.Tag;
                        if (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("PASS"))
                        {
                            if (bUpdate && (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("UPDATE_PASS")))
                            {
                                c.Text = String.Empty;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Control에 있는 Control 초기화
        /// </summary>
        public void ObjectInit(Control obj)
        {
            foreach (Control c in obj.Controls)
            {
                switch (c.GetType().Name.ToString())
                {
                    case "TextBox":
                        String sTag = (String)c.Tag;
                        if (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("PASS"))
                        {
                            c.Text = String.Empty;
                        }
                        break;

                    case "DateTimePicker": ((DateTimePicker)c).Value = DateTime.Now; break;
                    case "ComboBox": ((ComboBox)c).SelectedIndex = -1; break;
                }
            }
        }

        public void ObjectInit(Control obj, Boolean bUpdate)
        {
            foreach (Control c in obj.Controls)
            {
                switch (c.GetType().Name.ToString())
                {
                    case "TextBox":
                        String sTag = (String)c.Tag;
                        if (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("PASS"))
                        {
                            if (bUpdate && (String.IsNullOrEmpty(sTag) || !sTag.ToUpper().Equals("UPDATE_PASS")))
                            {
                                c.Text = String.Empty;
                            }
                        }
                        break;

                    case "DateTimePicker": ((DateTimePicker)c).Value = DateTime.Now; break;
                    case "ComboBox": ((ComboBox)c).SelectedIndex = -1; break;
                }
            }
        }

        /// <summary>
        /// 숫자만 입력가능하게
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AllowNumber(object sender, ref KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }
        public void AllowNumberWithColon(object sender, ref KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar.Equals(Convert.ToChar(Keys.Back)) || e.KeyChar.Equals(Convert.ToChar(':'))))
            {
                e.Handled = true;
            }
        }
        public void AllowNumberWithDot(object sender, ref KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Decimal) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }
        public void AllowNumberWithMinus(object sender, ref KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || e.KeyChar.Equals(Convert.ToChar(Keys.Back)) || e.KeyChar.Equals(Convert.ToChar('-'))))
            {
                e.Handled = true;
            }
        }
        public void AllowNumberWithDotMinus(object sender, ref KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) ||
                e.KeyChar.Equals(Convert.ToChar(Keys.Back)) ||
                e.KeyChar.Equals(Convert.ToChar('-')) ||
                e.KeyChar.Equals(Convert.ToChar('.'))))
            {
                e.Handled = true;
            }
        }

        public DataTable QueryDataTable(SqlConnection Conn, String sSQL)
        {
            DataTable dt = new DataTable();

            try
            {
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                SqlDataAdapter da = new SqlDataAdapter(sSQL, Conn);
                da.Fill(dt);
            }
            catch
            {
                dt = null;
            }

            return dt;
        }
        /// <summary>
        /// 주차장정보를 리턴한다.
        /// </summary>
        /// <param name="pCombobox"></param>
        public Dictionary<string, string> SetComboboxParkingData()
        {
            try
            {
                Dictionary<string, string> dicRtn = new Dictionary<string, string>();

                foreach (ParkInfo parkinfoitem in NPCOMMON.mListParkInfo)
                {
                    dicRtn.Add(parkinfoitem.ParkNo.ToString(), parkinfoitem.ParkName);
                }

                return dicRtn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 주차장정보를 주차번호로 Combobox에가져온다 
        /// </summary>
        /// <param name="pCombobox">받을콤보박스</param>
        /// <param name="ParkNo">주차장번호</param>
        public void SetComboboxParkingData(ref ComboBox pCombobox, int ParkNo)
        {
            try
            {
                foreach (ParkInfo parkinfoitem in NPCOMMON.mListParkInfo)
                {
                    if (parkinfoitem.ParkNo == ParkNo)
                    {
                        ComboboxItem item = new ComboboxItem();
                        item.Text = parkinfoitem.ParkName;
                        item.Value = parkinfoitem.ParkNo.ToString();
                        pCombobox.Items.Add(item);
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 주차번호가 아닌 주차장만 가져온다
        /// </summary>
        /// <param name="pCombobox"></param>
        /// <param name="ParkNo"></param>
        public void SetComboboxMainNotParkingData(ref ComboBox pCombobox, int ParkNo)
        {
            try
            {
                foreach (ParkInfo parkinfoitem in NPCOMMON.mListParkInfo)
                {
                    if (parkinfoitem.ParkNo != ParkNo)
                    {
                        ComboboxItem item = new ComboboxItem();
                        item.Text = parkinfoitem.ParkName;
                        item.Value = parkinfoitem.ParkNo.ToString();
                        pCombobox.Items.Add(item);
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 콤보박스의 value값을가져온다
        /// </summary>
        /// <param name="pCombobox"></param>
        /// <returns></returns>
        public string GetComboxValue(ref ComboBox pCombobox)
        {
            ComboboxItem item = (ComboboxItem)pCombobox.SelectedItem;
            return item.Value.ToString();
        }

        public void TypingOnlyNumber(object sender, KeyPressEventArgs e, bool includeComma, bool includeMinus)
        {
            bool isValidInput = false;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                if (includeComma == true) { if (e.KeyChar == ',') isValidInput = true; }
                if (includeMinus == true) { if (e.KeyChar == '-') isValidInput = true; }

                if (isValidInput == false) e.Handled = true;
            }

        }
    }
}
