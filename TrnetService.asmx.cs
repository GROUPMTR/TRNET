using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.Services;
using System.Xml;


namespace Trnet
{
    /// <summary>
    /// Summary description for SALES_INVOICES
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SALES_INVOICES : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        [WebMethod(Description = "FATUR TÜRLERİ (SALES_INVOICES,PURCHASE_INVOICES) "
           + "RETURN ( XML YAPISI HATALI,HEADER HATALI,BU NUMARA İLE FATURA VAR (T-FATURANO),"
           + "HEADER HATALI KAYIT SILINDI,HEADER HATALI KAYDA ALINMADI,FATURA EKLENDI ) ")]
        ///
        ///  WeSource YAZILIYOR 
        /// 
        public string  INVOICE_INSERT(string xmlString)
        {
            String xmlStrings = (String)xmlString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            Guid g = Guid.NewGuid();
            string V_RETURN = null, INVOICE_REF = null, INVOICE_NUMBER = null, FIRMA_KODU = null, DOC_TRACK_NR = "", TYPE = "", _CODE = "";

            V_RETURN = "XML YAPISI HATALI";
            XmlDocument xd = new XmlDocument();
            try
            {
                String s = xmlString;   //Convert XML output into String    
                xd.LoadXml(s);
                V_RETURN = "XML YAPISI DOGRU";
            }
            catch (Exception e)
            {
                V_RETURN = e.Message.ToString();
            }

            string myInsertQuery = string.Empty, InsertFIELD = string.Empty, InsertPARAMETER = string.Empty;
            if (V_RETURN == "XML YAPISI DOGRU")
            {
                V_RETURN = "HEADER HATALI";
                try
                {
                    using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                    {
                        SqlCommand myCommand = new SqlCommand();
                        myConnection.Open();
                        XmlNode INVOICE = xd.SelectSingleNode("/" + xd.DocumentElement.Name + "/INVOICE");
                        if (INVOICE != null)
                        {
                            XmlNodeReader readers = new XmlNodeReader(INVOICE);
                            DataSet dsX = new DataSet();
                            dsX.ReadXml(readers);
                            DataTable dtINVOICE = dsX.Tables["INVOICE"];
                            TYPE = dtINVOICE.Rows[0]["TYPE"].ToString();
                            INVOICE_NUMBER = dtINVOICE.Rows[0]["NUMBER"].ToString();
                            FIRMA_KODU = dtINVOICE.Rows[0]["FIRMA_KODU"].ToString();
                            _CODE = dtINVOICE.Rows[0]["CODE"].ToString();

                            V_RETURN = "FATURA YOK";
                            using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                            {
                                myConnections.Open();
                                SqlCommand myCommands = new SqlCommand();
                                myCommands.Connection = myConnections;
                                myCommands.CommandText = "SELECT  NUMBER  from dbo.FTR_LG_INVOICE WHERE NUMBER=@NUMBER and TAX_ID=@TAX_ID and SIRKET_KODU=@SIRKET_KODU AND AKTARIM_DURUMU=@AKTARILDI";
                                myCommands.Parameters.AddWithValue("@NUMBER", dtINVOICE.Rows[0]["NUMBER"].ToString().Replace(" ", "").Trim());
                                myCommands.Parameters.AddWithValue("@TAX_ID", dtINVOICE.Rows[0]["TAX_ID"].ToString().Replace(" ", "").Trim());
                                myCommands.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                                myCommands.Parameters.AddWithValue("@AKTARILDI", "AKTARILDI");
                                SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                                while (sqlreaders.Read())
                                {
                                    V_RETURN = String.Format("BU NUMARA İLE FATURA VAR ({0})", dtINVOICE.Rows[0]["NUMBER"]);
                                }
                                sqlreaders.Close();
                                myCommands.Connection.Close();
                            }
                            //if (V_RETURN == "FATURA YOK")
                            //{
                            //    using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                            //    {
                            //        myConnections.Open();
                            //        SqlCommand myCommands = new SqlCommand();
                            //        myCommands.Connection = myConnections;
                            //        myCommands.CommandText = String.Format("SELECT  NUMBER  from dbo.FTR_LG_INVOICE WHERE SIRKET_KODU='{0}' and NUMBER=@NUMBER ", FIRMA_KODU);
                            //        myCommands.Parameters.AddWithValue("@NUMBER", dtINVOICE.Rows[0]["NUMBER"].ToString().Replace(" ", "").Trim());
                            //        myCommands.Parameters.AddWithValue("@AKTARILDI", "AKTARILDI");
                            //        SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                            //        while (sqlreaders.Read())
                            //        {
                            //            V_RETURN = String.Format("BU NUMARA İLE FATURA VAR ({0})", dtINVOICE.Rows[0]["NUMBER"]);
                            //        }
                            //        sqlreaders.Close();
                            //        myCommands.Connection.Close();
                            //    }
                            //}
                            if (V_RETURN != "FATURA YOK") return V_RETURN;
                            if (V_RETURN == "FATURA YOK")
                            {
                                for (int i = 0; i < dtINVOICE.Rows[0].Table.Columns.Count - 1; i++)
                                {
                                    string FIELD = dtINVOICE.Rows[0].Table.Columns[i].ColumnName;
                                    InsertFIELD += FIELD + ",";
                                    InsertPARAMETER += String.Format("@{0},", FIELD);
                                    switch (FIELD)
                                    {
                                        case "POSTAL_CODE":
                                        case "TELEPHONE1":
                                        case "FAX":
                                        case "TAX_ID":
                                        case "NUMBER":
                                            myCommand.Parameters.AddWithValue("@" + FIELD, dtINVOICE.Rows[0][FIELD].ToString().Replace(" ", "").Trim());
                                            break;
                                        case "PROCDATE":
                                        case "DATE":
                                        case "DOC_DATE":
                                        case "DISCOUNT_DUEDATE":
                                            DateTime DATES = Convert.ToDateTime(dtINVOICE.Rows[0][FIELD]);
                                            myCommand.Parameters.AddWithValue("@" + FIELD, DATES.ToString("yyyy-MM-dd"));
                                            break;
                                        case "DOC_TRACK_NR":
                                            myCommand.Parameters.AddWithValue("@" + FIELD, dtINVOICE.Rows[0][FIELD].ToString().Trim());
                                            DOC_TRACK_NR = dtINVOICE.Rows[0][FIELD].ToString().Trim();
                                            break;
                                        default:
                                            myCommand.Parameters.AddWithValue("@" + FIELD, dtINVOICE.Rows[0][FIELD].ToString().Trim());
                                            break;
                                    }
                                }

                                XmlNode DEFNFLDSLIST = xd.SelectSingleNode("/" + xd.DocumentElement.Name + "/INVOICE/DEFNFLDSLIST/DEFNFLD");
                                if (DEFNFLDSLIST != null)
                                {
                                    XmlNodeReader readerDEFNFLDSLIST = new XmlNodeReader(DEFNFLDSLIST);
                                    DataSet dsXDEFNFLDSLIS = new DataSet();
                                    dsXDEFNFLDSLIS.ReadXml(readerDEFNFLDSLIST);
                                    DataTable dtDEFNFLD = dsXDEFNFLDSLIS.Tables["DEFNFLD"];
                                    for (int xi = 0; xi < dtDEFNFLD.Rows.Count; xi++)
                                    {
                                        for (int i = 0; i < dtDEFNFLD.Rows[xi].Table.Columns.Count - 1; i++)
                                        {
                                            string FIELD = dtDEFNFLD.Rows[xi].Table.Columns[i].ColumnName;
                                            switch (FIELD)
                                            {
                                                case "PROCDATE":
                                                case "DOC_DATE":
                                                case "DISCOUNT_DUEDATE":
                                                case "DATE":
                                                    InsertFIELD += "DEFNFLD_" + FIELD + ",";
                                                    InsertPARAMETER += "@DEFNFLD_" + FIELD + ",";
                                                    DateTime DATES = Convert.ToDateTime(dtDEFNFLD.Rows[xi][FIELD]);
                                                    myCommand.Parameters.AddWithValue("@DEFNFLD_" + FIELD, DATES.ToString("yyyy-MM-dd"));
                                                    break;

                                                default:
                                                    InsertFIELD += "DEFNFLD_" + FIELD + ",";
                                                    InsertPARAMETER += "@DEFNFLD_" + FIELD + ",";
                                                    myCommand.Parameters.AddWithValue("@DEFNFLD_" + FIELD, dtDEFNFLD.Rows[xi][FIELD]);
                                                    break;
                                            }
                                        }
                                    }
                                }

                                XmlNode PAYMENT_LIST = xd.SelectSingleNode("/" + xd.DocumentElement.Name + "/INVOICE/PAYMENT_LIST/PAYMENT");
                                if (PAYMENT_LIST != null)
                                {
                                    XmlNodeReader readersPAYMENT_LIST = new XmlNodeReader(PAYMENT_LIST);
                                    DataSet dsXPAYMENT_LIST = new DataSet();
                                    dsXPAYMENT_LIST.ReadXml(readersPAYMENT_LIST);

                                    DataTable dtPAYMENT = dsXPAYMENT_LIST.Tables["PAYMENT"];
                                    for (int xi = 0; xi < dtPAYMENT.Rows.Count; xi++)
                                    {
                                        for (int i = 0; i < dtPAYMENT.Rows[xi].Table.Columns.Count - 1; i++)
                                        {
                                            string FIELD = dtPAYMENT.Rows[xi].Table.Columns[i].ColumnName;
                                            switch (FIELD)
                                            {
                                                case "DATE":
                                                case "PROCDATE":
                                                case "DOC_DATE":
                                                case "DISCOUNT_DUEDATE":
                                                    InsertFIELD += String.Format("PAYMENT_{0},", FIELD);
                                                    InsertPARAMETER += String.Format("@PAYMENT_{0},", FIELD);
                                                    DateTime DATES = Convert.ToDateTime(dtPAYMENT.Rows[xi][FIELD]);
                                                    myCommand.Parameters.AddWithValue("@PAYMENT_" + FIELD, DATES.ToString("yyyy-MM-dd"));
                                                    break;

                                                default:
                                                    InsertFIELD += String.Format("PAYMENT_{0},", FIELD);
                                                    InsertPARAMETER += String.Format("@PAYMENT_{0},", FIELD);
                                                    myCommand.Parameters.AddWithValue("@PAYMENT_" + FIELD, dtPAYMENT.Rows[xi][FIELD]);
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (InsertFIELD != "") InsertFIELD = InsertFIELD.Substring(0, InsertFIELD.Length - 1);
                                if (InsertPARAMETER != "") InsertPARAMETER = InsertPARAMETER.Substring(0, InsertPARAMETER.Length - 1);

                                myInsertQuery = "INSERT INTO dbo.FTR_LG_INVOICE (GUID,SIRKET_KODU,TASLAK_FATURA_NO," + InsertFIELD + ") VALUES (@GUID,@SIRKET_KODU,@TASLAK_FATURA_NO," + InsertPARAMETER + " ) SELECT @@IDENTITY AS ID ";
                                myCommand.Connection = myConnection;
                                myCommand.CommandText = myInsertQuery;
                                myCommand.Parameters.AddWithValue("@TASLAK_FATURA_NO", INVOICE_NUMBER);
                                myCommand.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                                myCommand.Parameters.AddWithValue("@GUID", g);
                                SqlDataReader sqlreader = myCommand.ExecuteReader(CommandBehavior.CloseConnection);
                                while (sqlreader.Read())
                                {
                                    INVOICE_REF = sqlreader["ID"].ToString();
                                    V_RETURN = "HEADER EKLENDI";
                                }
                                sqlreader.Close();
                                myCommand.Connection.Close();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    V_RETURN = e.Message.ToString();
                }
            }
            myInsertQuery = string.Empty; InsertFIELD = string.Empty; InsertPARAMETER = string.Empty;
            if (V_RETURN == "HEADER EKLENDI")
            {
                try
                {
                    using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                    {
                        SqlCommand myCommand = new SqlCommand();
                        myConnection.Open();
                        XmlNode TRANSACTIONS = xd.SelectSingleNode("/" + xd.DocumentElement.Name + "/INVOICE/TRANSACTIONS");
                        if (TRANSACTIONS != null)
                        {
                            XmlNodeReader readerm = new XmlNodeReader(TRANSACTIONS);
                            DataSet dsX = new DataSet();
                            dsX.ReadXml(readerm);

                            DataTable dtTRANSACTION = dsX.Tables["TRANSACTION"];
                            DataTable dtDEFNFLD = dsX.Tables["DEFNFLD"];
                            for (int xi = 0; xi < dtTRANSACTION.Rows.Count; xi++)
                            {
                                myInsertQuery = string.Empty;
                                InsertFIELD = string.Empty;
                                InsertPARAMETER = string.Empty;

                                for (int i = 0; i < dtTRANSACTION.Rows[xi].Table.Columns.Count - 1; i++)
                                {
                                    string FIELDTRANSACTION = dtTRANSACTION.Rows[xi].Table.Columns[i].ColumnName;
                                    InsertFIELD += FIELDTRANSACTION + ",";
                                    InsertPARAMETER += "@T" + FIELDTRANSACTION + ",";
                                    switch (FIELDTRANSACTION)
                                    {
                                        case "PROCDATE":
                                        case "DATE":
                                        case "DOC_DATE":
                                        case "DISCOUNT_DUEDATE":
                                            DateTime DATES = Convert.ToDateTime(dtTRANSACTION.Rows[xi][FIELDTRANSACTION]);
                                            myCommand.Parameters.AddWithValue("@T" + FIELDTRANSACTION, DATES.ToString("yyyy-MM-dd"));
                                            break;
                                        case "NAME":
                                            if (dtTRANSACTION.Rows[xi][FIELDTRANSACTION].ToString().Length > 50)
                                            {
                                                string sub = dtTRANSACTION.Rows[xi][FIELDTRANSACTION].ToString().Substring(0, 49);
                                                myCommand.Parameters.AddWithValue("@T" + FIELDTRANSACTION, sub);
                                            }
                                            else
                                            {
                                                myCommand.Parameters.AddWithValue("@T" + FIELDTRANSACTION, dtTRANSACTION.Rows[xi][FIELDTRANSACTION]);
                                            }
                                            break;
                                        default:
                                            myCommand.Parameters.AddWithValue("@T" + FIELDTRANSACTION, dtTRANSACTION.Rows[xi][FIELDTRANSACTION]);
                                            break;
                                    }
                                }
                                if (xi <= dtDEFNFLD.Rows.Count - 1)
                                {
                                    for (int i = 0; i < dtDEFNFLD.Rows[xi].Table.Columns.Count - 1; i++)
                                    {
                                        string FIELDDEFNFLD = dtDEFNFLD.Rows[xi].Table.Columns[i].ColumnName;
                                        InsertFIELD += FIELDDEFNFLD + ",";
                                        InsertPARAMETER += "@LD" + FIELDDEFNFLD + ",";
                                        switch (FIELDDEFNFLD)
                                        {
                                            case "PROCDATE":
                                            case "DATE":
                                            case "DOC_DATE":
                                            case "DISCOUNT_DUEDATE":
                                                DateTime DATES = Convert.ToDateTime(dtDEFNFLD.Rows[xi][FIELDDEFNFLD]);
                                                myCommand.Parameters.AddWithValue("@LD" + FIELDDEFNFLD, DATES.ToString("yyyy-MM-dd"));
                                                break;
                                            case "NAME":
                                                if (dtDEFNFLD.Rows[xi][FIELDDEFNFLD].ToString().Length > 50)
                                                {
                                                    string sub = dtDEFNFLD.Rows[xi][FIELDDEFNFLD].ToString().Substring(0, 49);
                                                    myCommand.Parameters.AddWithValue("@LD" + FIELDDEFNFLD, sub);
                                                }
                                                else
                                                {
                                                    myCommand.Parameters.AddWithValue("@LD" + FIELDDEFNFLD, dtDEFNFLD.Rows[xi][FIELDDEFNFLD]);
                                                }
                                                break;
                                            default:
                                                myCommand.Parameters.AddWithValue("@LD" + FIELDDEFNFLD, dtDEFNFLD.Rows[xi][FIELDDEFNFLD]);
                                                break;
                                        }
                                    }
                                }
                                if (InsertFIELD != "") InsertFIELD = InsertFIELD.Substring(0, InsertFIELD.Length - 1);
                                if (InsertPARAMETER != "") InsertPARAMETER = InsertPARAMETER.Substring(0, InsertPARAMETER.Length - 1);
                                myInsertQuery = "INSERT INTO dbo.FTR_LG_STLINE (GUID,SIRKET_KODU,INVOICE_REF,INVOICE_NUMBER,PO_CODE," + InsertFIELD + ") VALUES (@GUID,@SIRKET_KODU, @INVOICE_REF,@INVOICE_NUMBER,@PO_CODE," + InsertPARAMETER + " ) SELECT @@IDENTITY AS ID   ";
                                myCommand.Connection = myConnection;
                                myCommand.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                                myCommand.Parameters.AddWithValue("@INVOICE_REF", INVOICE_REF);
                                myCommand.Parameters.AddWithValue("@INVOICE_NUMBER", INVOICE_NUMBER);
                                myCommand.Parameters.AddWithValue("@FIRMA_KODU", FIRMA_KODU);
                                myCommand.Parameters.AddWithValue("@PO_CODE", DOC_TRACK_NR);
                                myCommand.Parameters.AddWithValue("@GUID", g);
                                myCommand.CommandText = myInsertQuery;
                                myCommand.ExecuteNonQuery();
                                myCommand.Parameters.Clear();
                                V_RETURN = "FATURA EKLENDI";
                            }
                        }
                        myConnection.Close();
                    }


                    string OZEL_DURUM_VAR = "YOK";
                    string HEADER_RERERANS = "YOK";

                    using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                    {
                        myConnections.Open();
                        SqlCommand myCommands = new SqlCommand();
                        myCommands.Connection = myConnections;
                        myCommands.CommandText = String.Format("SELECT  *  from  dbo.FTR_PARAMETER_TRANSFER   WHERE(SIRKET_KODU ='{0}' and OZEL_KOD='{1}' ) ", FIRMA_KODU, _CODE);
                        SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                        while (sqlreaders.Read())
                        {
                            OZEL_DURUM_VAR = "VAR";
                            HEADER_RERERANS = "HEADER";

                        }

                    }

                    using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                    {
                        myConnections.Open();
                        SqlCommand myCommands = new SqlCommand();
                        myCommands.Connection = myConnections;


                        myCommands.CommandText = String.Format("SELECT  dbo.FTR_LG_INVOICE.ID, dbo.FTR_LG_INVOICE.SIRKET_KODU, dbo.FTR_LG_INVOICE.FIRMA_KODU, dbo.FTR_LG_STLINE.SIRKET_KODU AS LNSIRKET, dbo.FTR_LG_STLINE.FIRMA_KODU AS LNFIRMA, " +
                                                             "    dbo.FTR_PARAMETER_TRANSFER.AKTARILACAK_SIRKET_KODU " +
                                                             "    FROM  dbo.FTR_LG_INVOICE INNER JOIN " +
                                                             "    dbo.FTR_LG_STLINE ON dbo.FTR_LG_INVOICE.ID = dbo.FTR_LG_STLINE.INVOICE_REF INNER JOIN " +
                                                             "    dbo.FTR_PARAMETER_TRANSFER ON dbo.FTR_LG_INVOICE.CODE  = dbo.FTR_PARAMETER_TRANSFER.OZEL_KOD  AND " +
                                                             "    dbo.FTR_LG_STLINE.SIRKET_KODU = dbo.FTR_PARAMETER_TRANSFER.SIRKET_KODU " +
                                                             "    WHERE(dbo.FTR_LG_INVOICE.SIRKET_KODU ='{0}' ) ", FIRMA_KODU);


                        SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                        while (sqlreaders.Read())
                        {
                            OZEL_DURUM_VAR = "VAR";
                            HEADER_RERERANS = "LINE";
                        }

                    }




                    if (OZEL_DURUM_VAR == "VAR")
                    {
                        if (HEADER_RERERANS == "LINE")
                        {
                            using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                            {
                                myConnections.Open();
                                SqlCommand myCommands = new SqlCommand();
                                myCommands.Connection = myConnections;
                                myCommands.CommandText = String.Format("SELECT  dbo.FTR_LG_INVOICE.ID, dbo.FTR_LG_INVOICE.SIRKET_KODU, dbo.FTR_LG_INVOICE.FIRMA_KODU, dbo.FTR_LG_STLINE.SIRKET_KODU AS LNSIRKET, dbo.FTR_LG_STLINE.FIRMA_KODU AS LNFIRMA, " +
                                                                                "    dbo.FTR_PARAMETER_TRANSFER.AKTARILACAK_SIRKET_KODU " +
                                                                                "    FROM  dbo.FTR_LG_INVOICE INNER JOIN " +
                                                                                "    dbo.FTR_LG_STLINE ON dbo.FTR_LG_INVOICE.ID = dbo.FTR_LG_STLINE.INVOICE_REF INNER JOIN " +
                                                                                "    dbo.FTR_PARAMETER_TRANSFER ON dbo.FTR_LG_INVOICE.CODE  = dbo.FTR_PARAMETER_TRANSFER.OZEL_KOD  AND " +
                                                                                "    dbo.FTR_LG_STLINE.SIRKET_KODU = dbo.FTR_PARAMETER_TRANSFER.SIRKET_KODU " +
                                                                                "    WHERE(dbo.FTR_LG_INVOICE.SIRKET_KODU ='{0}' ) ", FIRMA_KODU);
                                SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                                while (sqlreaders.Read())
                                {
                                    if (INVOICE_REF != String.Empty)
                                    {
                                        using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                                        {
                                            myConnection.Open();
                                            string query = string.Empty;
                                            using (SqlCommand myCommand = new SqlCommand(query))
                                            {
                                                query = " update  dbo.FTR_LG_STLINE set SIRKET_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "', FIRMA_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "'  WHERE SIRKET_KODU=@SIRKET_KODU and INVOICE_REF= @INVOICE_REF   " +
                                                        " update  dbo.FTR_LG_INVOICE set SIRKET_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "', FIRMA_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "'  WHERE SIRKET_KODU=@SIRKET_KODU and ID= @INVOICE_REF ";

                                                myCommand.Connection = myConnection;
                                                myCommand.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                                                myCommand.Parameters.AddWithValue("@INVOICE_REF", INVOICE_REF);
                                                myCommand.CommandText = query;
                                                myCommand.ExecuteNonQuery();
                                                myCommand.Parameters.Clear();
                                            }
                                        }
                                    }
                                }
                                sqlreaders.Close();
                                myCommands.Connection.Close();
                            }
                        }
                        if (HEADER_RERERANS == "HEADER")
                        {
                            using (SqlConnection myConnections = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                            {
                                myConnections.Open();
                                SqlCommand myCommands = new SqlCommand();
                                myCommands.Connection = myConnections;
                                myCommands.CommandText = String.Format("SELECT  dbo.FTR_LG_INVOICE.ID, dbo.FTR_LG_INVOICE.SIRKET_KODU, dbo.FTR_LG_INVOICE.FIRMA_KODU, dbo.FTR_LG_STLINE.SIRKET_KODU AS LNSIRKET, dbo.FTR_LG_STLINE.FIRMA_KODU AS LNFIRMA, " +
                                                                                "    dbo.FTR_PARAMETER_TRANSFER.AKTARILACAK_SIRKET_KODU " +
                                                                                "    FROM  dbo.FTR_LG_INVOICE INNER JOIN " +
                                                                                "    dbo.FTR_LG_STLINE ON dbo.FTR_LG_INVOICE.ID = dbo.FTR_LG_STLINE.INVOICE_REF INNER JOIN " +
                                                                                "    dbo.FTR_PARAMETER_TRANSFER ON dbo.FTR_LG_STLINE.AUXIL_CODE = dbo.FTR_PARAMETER_TRANSFER.OZEL_KOD  AND " +
                                                                                "    dbo.FTR_LG_STLINE.SIRKET_KODU = dbo.FTR_PARAMETER_TRANSFER.SIRKET_KODU " +
                                                                                "    WHERE(dbo.FTR_LG_INVOICE.SIRKET_KODU ='{0}' ) ", FIRMA_KODU);
                                SqlDataReader sqlreaders = myCommands.ExecuteReader(CommandBehavior.CloseConnection);
                                while (sqlreaders.Read())
                                {
                                    if (INVOICE_REF != String.Empty)
                                    {
                                        using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                                        {
                                            myConnection.Open();
                                            string query = string.Empty;
                                            using (SqlCommand myCommand = new SqlCommand(query))
                                            {
                                                query = " update  dbo.FTR_LG_STLINE set SIRKET_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "', FIRMA_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "'  WHERE SIRKET_KODU=@SIRKET_KODU and INVOICE_REF= @INVOICE_REF   " +
                                                        " update  dbo.FTR_LG_INVOICE set SIRKET_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "', FIRMA_KODU='" + sqlreaders["AKTARILACAK_SIRKET_KODU"].ToString() + "'  WHERE SIRKET_KODU=@SIRKET_KODU and ID= @INVOICE_REF ";

                                                myCommand.Connection = myConnection;
                                                myCommand.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                                                myCommand.Parameters.AddWithValue("@INVOICE_REF", INVOICE_REF);
                                                myCommand.CommandText = query;
                                                myCommand.ExecuteNonQuery();
                                                myCommand.Parameters.Clear();
                                            }
                                        }
                                    }
                                }
                                sqlreaders.Close();
                                myCommands.Connection.Close();
                            }

                        }
                    }

                }
                catch (Exception e)
                {
                    V_RETURN = e.Message.ToString();
                }
            }

            if (V_RETURN != "FATURA EKLENDI" && INVOICE_REF != null)
            {
                if (INVOICE_REF != String.Empty)
                {
                    using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                    {
                        myConnection.Open();
                        string query = string.Empty;
                        using (SqlCommand myCommand = new SqlCommand(query))
                        {
                            query = "delete dbo.FTR_LG_INVOICE  WHERE SIRKET_KODU=@SIRKET_KODU and ID= @INVOICE_REF  ";
                            myCommand.Connection = myConnection;
                            myCommand.Parameters.AddWithValue("@SIRKET_KODU", FIRMA_KODU);
                            myCommand.Parameters.AddWithValue("@INVOICE_REF", INVOICE_REF);
                            myCommand.CommandText = query;
                            myCommand.ExecuteNonQuery();
                            myCommand.Parameters.Clear();
                            V_RETURN = "HEADER HATALI KAYIT SILINDI";
                        }
                    }
                }
                else
                {
                    V_RETURN = "HEADER HATALI KAYDA ALINMADI";
                }
            }
            return V_RETURN;
        }

        ///
        ///  LOGODAN OKUNUYOR
        ///
        [WebMethod(Description = "INPUT: FIRMA_KODU,TASLAK_FATURA_NO || RETURN :FATURA_NO,FATURA_TARIHI,ODEME_TARIHI ")]
        public DataTable  INVOICE_NUMBER(string FIRMA_KODU, string TASLAK_FATURA_NO)
        {
            string FIRMA_KODU_OLD = "";

            //if (FIRMA_KODU == "GRM") { FIRMA_KODU = "516"; }
            //if (FIRMA_KODU == "MAX") { FIRMA_KODU = "116"; }
            //if (FIRMA_KODU == "MCM") { FIRMA_KODU = "416"; }
            //if (FIRMA_KODU == "MDS") { FIRMA_KODU = "016"; }
            //if (FIRMA_KODU == "MEC") { FIRMA_KODU = "316"; }
            //if (FIRMA_KODU == "QUI") { FIRMA_KODU = "616"; }




            if (FIRMA_KODU == "GRM") { FIRMA_KODU_OLD = "517"; FIRMA_KODU = "518"; }
            if (FIRMA_KODU == "MAX") { FIRMA_KODU_OLD = "117"; FIRMA_KODU = "118"; }
            if (FIRMA_KODU == "MCM") { FIRMA_KODU_OLD = "417"; FIRMA_KODU = "418"; }
            if (FIRMA_KODU == "MDS") { FIRMA_KODU_OLD = "017"; FIRMA_KODU = "018"; }
            if (FIRMA_KODU == "MEC") { FIRMA_KODU_OLD = "317"; FIRMA_KODU = "318"; }
            if (FIRMA_KODU == "CON") { FIRMA_KODU_OLD = "617"; FIRMA_KODU = "618"; }
            if (FIRMA_KODU == "AMV") { FIRMA_KODU_OLD = "717"; FIRMA_KODU = "718"; }
            if (FIRMA_KODU == "WMG") { FIRMA_KODU_OLD = "318"; FIRMA_KODU = "328"; }

            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringLogo"].ToString()))
            {
                string SQL = " SELECT  FICHENO AS FATURA_NO , DATE_ AS FATURA_TARIHI, DATE_ AS ODEME_TARIHI FROM dbo.[LG_" + FIRMA_KODU + "_01_INVOICE] WHERE (DOCODE=@TASLAK_FATURA_NO and FICHENO<>@TASLAK_FATURA_NO)";
                SQL += " UNION ALL SELECT  FICHENO AS FATURA_NO , DATE_ AS FATURA_TARIHI, DATE_ AS ODEME_TARIHI FROM dbo.[LG_" + FIRMA_KODU_OLD + "_01_INVOICE] WHERE (DOCODE=@TASLAK_FATURA_NO and FICHENO<>@TASLAK_FATURA_NO)";
                if (FIRMA_KODU == "017") SQL += " UNION ALL SELECT  FICHENO AS FATURA_NO , DATE_ AS FATURA_TARIHI, DATE_ AS ODEME_TARIHI FROM dbo.[LG_517_01_INVOICE] WHERE (DOCODE=@TASLAK_FATURA_NO and FICHENO<>@TASLAK_FATURA_NO)";

                SqlDataAdapter da = new SqlDataAdapter(SQL, myConnection);
                da.SelectCommand.Parameters.AddWithValue("@TASLAK_FATURA_NO", TASLAK_FATURA_NO);
                DataTable dt = new DataTable("LG_INVOICE_NUMBER");
                da.Fill(dt);
                return dt;
            }
        }
        ///
        ///  EINVDEN OKUNUYOR
        ///
        [WebMethod(Description = "INPUT:  FIRMA_KODU,BAS_TARIHI (yyyy.MM.dd) ,BIT_TARIHI (yyyy.MM.dd) || RETURN :FATURA_TARIHI ,FATURA_UUID,MUSTERI_KODU,FATURA_TIPI ")]
        public DataTable  INVOICE_LIST(string FIRMA_KODU, DateTime BAS_TARIHI, DateTime BIT_TARIHI, string FATURA_TURU)
        {
            DataTable dt = null;
            if (FATURA_TURU == "ALIS")
            {
                using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                {
                    string SQL = "";

                    switch (FIRMA_KODU)
                    {

                        case "MDS":
                            SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                            break;

                        case "MCM":
                            SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='CNT' OR SIRKET_KODU='MCM') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                            break;

                        case "WMG":
                            SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='MEC' OR SIRKET_KODU='WMG') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                            break;

                        case "MAX":
                            SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='MAX' OR SIRKET_KODU='WMG') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                            break;

                        default:
                            SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI,ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE (SIRKET_KODU='" + FIRMA_KODU + "') AND (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                            break;
                    }



                    //if (FIRMA_KODU == "MDS")
                    //    SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                    //else
                    //    SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI,ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE (SIRKET_KODU='" + FIRMA_KODU + "') AND (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";

                    SqlDataAdapter da = new SqlDataAdapter(SQL, myConnection);
                    da.SelectCommand.Parameters.AddWithValue("@BAS_TARIHI", BAS_TARIHI.ToString("yyyy.MM.dd"));
                    da.SelectCommand.Parameters.AddWithValue("@BIT_TARIHI", BIT_TARIHI.ToString("yyyy.MM.dd"));
                    dt = new DataTable("LG_INVOICE_LIST");
                    da.Fill(dt);
                }
            }
            if (FATURA_TURU == "SATIS")
            {
                using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                {
                    string SQL = "";
                    if (FIRMA_KODU == "MDS")



                        switch (FIRMA_KODU)
                        {
                            case "MDS":
                                SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU, FATURA_TIPI,ID AS FATURA_NO  FROM  dbo.FTR_GIB_TRANSFER WHERE  (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') AND   (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";// and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False'))";
                                break;

                            case "MCM":
                                SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU, FATURA_TIPI,ID AS FATURA_NO  FROM  dbo.FTR_GIB_TRANSFER WHERE  (SIRKET_KODU='CNT' OR SIRKET_KODU='MCM') AND   (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";// and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False'))";
                                break;

                            case "WMG":
                                SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU, FATURA_TIPI,ID AS FATURA_NO  FROM  dbo.FTR_GIB_TRANSFER WHERE  (SIRKET_KODU='MEC' OR SIRKET_KODU='WMG') AND   (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";// and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False'))";
                                break;

                            case "MAX":
                                SQL = "SELECT ID, IssueDate AS FATURA_TARIHI, UUID AS FATURA_UUID, MUSTERI_KODU,FATURA_TIPI, ID AS FATURA_NO FROM  dbo.FTR_GELEN_FATURALAR WHERE  (SIRKET_KODU='MAX' OR SIRKET_KODU='WMG') AND    (IssueDate >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (IssueDate <= CONVERT(DATETIME, @BIT_TARIHI , 102)) and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) and (SignatureSchemeID is not null )";
                                break;

                            default:
                                SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU ,FATURA_TIPI,ID AS FATURA_NO FROM  dbo.FTR_GIB_TRANSFER WHERE (SIRKET_KODU='" + FIRMA_KODU + "') AND (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";//and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) ";
                                break;

                        }




                    //    SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU, FATURA_TIPI,ID AS FATURA_NO  FROM  dbo.FTR_GIB_TRANSFER WHERE  (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') AND   (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";// and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False'))";
                    //else
                    //    SQL = "SELECT ID, Tarih AS FATURA_TARIHI, GUID AS FATURA_UUID, MUSTERI AS MUSTERI_KODU ,FATURA_TIPI,ID AS FATURA_NO FROM  dbo.FTR_GIB_TRANSFER WHERE (SIRKET_KODU='" + FIRMA_KODU + "') AND (Tarih >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (Tarih <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";//and ((ERP_AKTARILDI IS NULL) OR (ERP_AKTARILDI = 'False')) ";

                    SqlDataAdapter da = new SqlDataAdapter(SQL, myConnection);
                    da.SelectCommand.Parameters.AddWithValue("@BAS_TARIHI", BAS_TARIHI.ToString("yyyy.MM.dd"));
                    da.SelectCommand.Parameters.AddWithValue("@BIT_TARIHI", BIT_TARIHI.ToString("yyyy.MM.dd"));
                    dt = new DataTable("LG_INVOICE_LIST");
                    da.Fill(dt);
                }
            }
            return dt;
        }

        [WebMethod(Description = " INPUT : FIRMA_KODU,ID,UUID,FATURA_TURU (ALIS / SATIS),FATURA_TIPI(E / P)  || RETURN : FATURA_UUID,FATURA_NO,FATURA_TARIHI,FATURA_ACIKLAMASI,FATURA_KESEN_VERGI_NO,FATURA_KESEN_UNVANI, FATURA_PARA_BIRIMI,FATURA_KDV_ORANI,FATURA_KDV_HARIC_TUTAR,FATURA_KDV_DAHIL_TUTAR, FATURA_ODEME_ACIKLAMA,FATURA_ODEME_TARIHI,FATURA_HTML,MUSTERI_KODU,MECRA_TURU,MECRA_ADI,PLAN_KODU,FATURA_PDF")]
        public DataTable INVOICE_DETAIL(string FIRMA_KODU, string ID, string FATURA_TURU, string FATURA_TIPI)
        {
            DataTable dt = new DataTable("INVOICE_DETAIL");
            if (FATURA_TURU == "ALIS")
            {
                dt.Columns.Add("FATURA_UUID", typeof(System.String));
                dt.Columns.Add("FATURA_NO", typeof(System.String));
                dt.Columns.Add("FATURA_TARIHI", typeof(System.DateTime));
                dt.Columns.Add("FATURA_ACIKLAMASI", typeof(System.String));
                dt.Columns.Add("FATURA_KESEN_VERGI_NO", typeof(System.String));
                dt.Columns.Add("FATURA_KESEN_UNVANI", typeof(System.String));
                dt.Columns.Add("FATURA_PARA_BIRIMI", typeof(System.String));
                dt.Columns.Add("FATURA_KDV_ORANI", typeof(System.String));
                dt.Columns.Add("FATURA_KDV_TUTARI", typeof(System.String));
                dt.Columns.Add("FATURA_KDV_HARIC_TUTAR", typeof(System.String));
                dt.Columns.Add("FATURA_KDV_DAHIL_TUTAR", typeof(System.String));
                dt.Columns.Add("FATURA_ODEME_ACIKLAMA", typeof(System.String));
                dt.Columns.Add("FATURA_ODEME_TARIHI", typeof(System.DateTime));
                dt.Columns.Add("FATURA_HTML", typeof(System.String));
                dt.Columns.Add("MUSTERI_KODU", typeof(System.String));
                dt.Columns.Add("MECRA_TURU", typeof(System.String));
                dt.Columns.Add("MECRA_ADI", typeof(System.String));
                dt.Columns.Add("PLAN_KODU", typeof(System.String));
                dt.Columns.Add("FATURA_PDF", typeof(Byte[]));

                using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                {
                    myConnection.Open();
                    string myInsertQuery = string.Empty, InsertFIELD = string.Empty, InsertPARAMETER = string.Empty;
                    using (SqlCommand myCommand = new SqlCommand(myInsertQuery))
                    {
                        switch (FIRMA_KODU)
                        {
                            case "MDS":
                                myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  or ID=@UUID ";
                                break;
                            case "MCM":
                                myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='CNT' OR SIRKET_KODU='MCM') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  or ID=@UUID   ";
                                break;

                            case "WMG":
                                myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='MEC' OR SIRKET_KODU='WMG') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  or ID=@UUID   ";
                                break;

                            case "MAX":
                                myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='MAX' OR SIRKET_KODU='WMG') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  or ID=@UUID   ";
                                break;
                            //case "CNT":
                            //    myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='CNT') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  or ID=@UUID   ";
                            //    break; 
                            default:
                                myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR WHERE  SIRKET_KODU=@FIRMA_KODU and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID   or ID=@UUID ";
                                break;
                        }

                        // if (FIRMA_KODU == "MDS")
                        //    myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR  WHERE (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID   ";
                        //else
                        //    myInsertQuery = "SELECT * FROM dbo.FTR_GELEN_FATURALAR WHERE  SIRKET_KODU=@FIRMA_KODU and FATURA_TIPI=@FATURA_TIPI AND  UUID=@UUID  ";

                        myCommand.Connection = myConnection;
                        myCommand.Parameters.AddWithValue("@UUID", ID);
                        myCommand.Parameters.AddWithValue("@FIRMA_KODU", FIRMA_KODU);
                        myCommand.Parameters.AddWithValue("@FATURA_TIPI", FATURA_TIPI);
                        myCommand.CommandText = myInsertQuery;
                        SqlDataReader myReader = myCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                        while (myReader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            dr["FATURA_UUID"] = myReader["UUID"].ToString();
                            dr["FATURA_NO"] = myReader["ID"].ToString();
                            dr["FATURA_TARIHI"] = Convert.ToDateTime(myReader["IssueDate"].ToString());
                            dr["FATURA_ACIKLAMASI"] = myReader["Note"].ToString();
                            dr["FATURA_KESEN_VERGI_NO"] = myReader["AccSupplierPartyIdentificationSchemeID"].ToString();
                            dr["FATURA_KESEN_UNVANI"] = myReader["AccSupplierPartyName"].ToString();
                            dr["FATURA_PARA_BIRIMI"] = myReader["DocumentCurrencyCode"].ToString();
                            dr["FATURA_KDV_ORANI"] = myReader["TaxSubtotalPercent"].ToString();
                            dr["FATURA_KDV_TUTARI"] = myReader["TaxTotalTaxAmount"].ToString();
                            if (dr["FATURA_KDV_TUTARI"].ToString() == "0,0000") dr["FATURA_KDV_TUTARI"] = myReader["TaxSubtotalTaxAmount"].ToString();
                            if (dr["FATURA_KDV_TUTARI"].ToString() != "0,0000" && myReader["TaxSubtotalTaxCategoryTaxSchemeCode"].ToString() == "0015") dr["FATURA_KDV_ORANI"] = "18,00";
                            dr["FATURA_KDV_HARIC_TUTAR"] = myReader["LegalMonetaryTaxExclusiveAmount"].ToString();
                            if (myReader["LegalMonetaryTaxExclusiveAmount"].ToString() == myReader["LegalMonetaryTaxInclusiveAmount"].ToString()) dr["FATURA_KDV_HARIC_TUTAR"] = myReader["TaxSubtotalTaxableAmount"];
                            dr["FATURA_KDV_DAHIL_TUTAR"] = myReader["LegalMonetaryTaxInclusiveAmount"].ToString();
                            dr["FATURA_ODEME_ACIKLAMA"] = myReader["PaymentTermsNote"].ToString();
                            dr["MUSTERI_KODU"] = myReader["MUSTERI_KODU"].ToString();
                            dr["MECRA_TURU"] = myReader["MECRA_TURU"].ToString();
                            dr["MECRA_ADI"] = myReader["MECRA_ADI"].ToString();
                            dr["PLAN_KODU"] = myReader["PLAN_KODU"].ToString();
                            if (myReader["PaymentMeansDueDate"] != DBNull.Value)
                            {
                                dr["FATURA_ODEME_TARIHI"] = Convert.ToDateTime(myReader["PaymentMeansDueDate"].ToString());
                            }
                            else
                            {
                                dr["FATURA_ODEME_TARIHI"] = Convert.ToDateTime(myReader["IssueDate"].ToString());
                            }
                            DateTime dtm = Convert.ToDateTime(dr["FATURA_TARIHI"]);
                            if (FATURA_TIPI == "E")
                            {
                                string strdocPath = string.Format(@"\\10.219.168.92\efatura$\_INBOX\{0}\{1}_{2}.xml", myReader["SIRKET_KODU"], dr["FATURA_NO"], dr["FATURA_UUID"]);
                                if (File.Exists(strdocPath))
                                {
                                    string HtmlFiles;
                                    using (StreamReader sr = new StreamReader(strdocPath, System.Text.Encoding.UTF8, true))
                                    {
                                        HtmlFiles = sr.ReadToEnd();//.Replace("￿  ","").Replace("￿","") ;
                                    }
                                    dr["FATURA_HTML"] = HtmlFiles;
                                }
                            }
                            if (FATURA_TIPI == "P")
                            {
                                string strdocPath = @"\\10.219.168.92\efatura$\_INBOX_PRINT\" + myReader["SIRKET_KODU"].ToString() + "\\" + myReader["SIRKET_KODU"].ToString() + "_" + dtm.Year.ToString() + "_" + dr["FATURA_UUID"] + ".pdf";
                                if (File.Exists(strdocPath))
                                {
                                    FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
                                    int len = (int)objfilestream.Length;
                                    Byte[] documentcontents = new Byte[len];
                                    objfilestream.Read(documentcontents, 0, len);
                                    objfilestream.Close();
                                    // dr["FATURA_PDF"] = documentcontents ;
                                    byte[] newValue = (byte[])documentcontents;
                                    dr["FATURA_PDF"] = newValue;
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                        myReader.Close();
                        myCommand.Connection.Close();
                    }
                }
            }

            if (FATURA_TURU == "SATIS")
            {
                dt.Columns.Add("FATURA_UUID", typeof(String));
                dt.Columns.Add("FATURA_NO", typeof(String));
                dt.Columns.Add("FATURA_TARIHI", typeof(DateTime));
                dt.Columns.Add("FATURA_ACIKLAMASI", typeof(String));
                dt.Columns.Add("FATURA_KESEN_VERGI_NO", typeof(String));
                dt.Columns.Add("FATURA_KESEN_UNVANI", typeof(String));
                dt.Columns.Add("FATURA_PARA_BIRIMI", typeof(String));
                dt.Columns.Add("FATURA_KDV_ORANI", typeof(String));
                dt.Columns.Add("FATURA_KDV_TUTARI", typeof(String));
                dt.Columns.Add("FATURA_KDV_HARIC_TUTAR", typeof(String));
                dt.Columns.Add("FATURA_KDV_DAHIL_TUTAR", typeof(String));
                dt.Columns.Add("FATURA_ODEME_ACIKLAMA", typeof(String));
                dt.Columns.Add("FATURA_ODEME_TARIHI", typeof(DateTime));
                dt.Columns.Add("FATURA_HTML", typeof(String));
                dt.Columns.Add("MUSTERI_KODU", typeof(String));
                dt.Columns.Add("MECRA_TURU", typeof(String));
                dt.Columns.Add("MECRA_ADI", typeof(String));
                dt.Columns.Add("PLAN_KODU", typeof(String));
                dt.Columns.Add("FATURA_PDF", typeof(Byte[]));
                using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
                {
                    myConnection.Open();
                    string myInsertQuery = string.Empty, InsertFIELD = string.Empty, InsertPARAMETER = string.Empty;
                    using (SqlCommand myCommand = new SqlCommand(myInsertQuery))
                    {
                        switch (FIRMA_KODU)
                        {
                            case "MDS":
                                myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER  WHERE (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID   ";
                                break;

                            case "MCM":
                                myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER  WHERE (SIRKET_KODU='CNT' OR SIRKET_KODU='MCM') and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID   ";
                                break;
                            case "WMG":
                                myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER  WHERE (SIRKET_KODU='MEC' OR SIRKET_KODU='WMG') and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID   ";
                                break;

                            case "MAX":
                                myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER  WHERE (SIRKET_KODU='MAX' OR SIRKET_KODU='WMG') and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID   ";
                                break;

                            default:
                                myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER WHERE  SIRKET_KODU=@FIRMA_KODU and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID  ";
                                break;
                        }
                        // if (FIRMA_KODU == "MDS")
                        //    myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER  WHERE (SIRKET_KODU='GRM' OR SIRKET_KODU='MDS') and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID   ";
                        //else
                        //    myInsertQuery = "SELECT  GUID,ID,SIRKET_KODU,FATURATARIH,ACIKLAMA,VKN,MUSTERI,HESAPLANANKDV,TOPLAMTUTAR,GENELTOPLAM FROM dbo.FTR_GIB_TRANSFER WHERE  SIRKET_KODU=@FIRMA_KODU and FATURA_TIPI=@FATURA_TIPI AND  GUID=@GUID  ";

                        myCommand.Connection = myConnection;
                        myCommand.Parameters.AddWithValue("@GUID", ID);
                        myCommand.Parameters.AddWithValue("@FIRMA_KODU", FIRMA_KODU);
                        myCommand.Parameters.AddWithValue("@FATURA_TIPI", FATURA_TIPI);
                        myCommand.CommandText = myInsertQuery;
                        SqlDataReader myReader = myCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                        while (myReader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            dr["FATURA_UUID"] = myReader["GUID"].ToString();
                            dr["FATURA_NO"] = myReader["ID"].ToString();
                            dr["FATURA_TARIHI"] = Convert.ToDateTime(myReader["FATURATARIH"].ToString());
                            dr["FATURA_ACIKLAMASI"] = myReader["ACIKLAMA"].ToString();
                            dr["FATURA_KESEN_VERGI_NO"] = myReader["VKN"].ToString();
                            dr["FATURA_KESEN_UNVANI"] = myReader["MUSTERI"].ToString();
                            //dr["FATURA_PARA_BIRIMI"] = myReader["DocumentCurrencyCode"].ToString();
                            //dr["FATURA_KDV_ORANI"] = myReader["TaxSubtotalPercent"].ToString();
                            dr["FATURA_KDV_TUTARI"] = myReader["HESAPLANANKDV"].ToString();
                            dr["FATURA_KDV_HARIC_TUTAR"] = myReader["TOPLAMTUTAR"].ToString();
                            dr["FATURA_KDV_DAHIL_TUTAR"] = myReader["GENELTOPLAM"].ToString();
                            //dr["FATURA_ODEME_ACIKLAMA"] = myReader["PaymentTermsNote"].ToString();
                            //dr["MUSTERI_KODU"] = myReader["MUSTERI_KODU"].ToString();
                            //dr["MECRA_TURU"] = myReader["MECRA_TURU"].ToString();
                            //dr["MECRA_ADI"] = myReader["MECRA_ADI"].ToString();
                            //dr["PLAN_KODU"] = myReader["PLAN_KODU"].ToString();                       
                            //dr["FATURA_ODEME_TARIHI"] = Convert.ToDateTime(myReader["FATURATARIH"].ToString());    
                            if (FATURA_TIPI == "E")
                            {
                                string strdocPath = @"\\10.219.168.92\\efatura$\\_OUTBOX\\" + myReader["SIRKET_KODU"].ToString() + "\\" + dr["FATURA_NO"] + ".xml";
                                if (File.Exists(strdocPath))
                                {
                                    string HtmlFiles;
                                    using (StreamReader sr = new StreamReader(@"" + strdocPath))
                                    {
                                        HtmlFiles = sr.ReadToEnd();
                                    }
                                    dr["FATURA_HTML"] = HtmlFiles;
                                }
                            }

                            if (FATURA_TIPI == "P")
                            {
                                string strdocPath = @"\\10.219.168.92\\efatura$\\_OUTBOX_PRINT\\" + myReader["SIRKET_KODU"].ToString() + "\\" + dr["FATURA_NO"] + ".pdf";
                                if (File.Exists(strdocPath))
                                {
                                    FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
                                    int len = (int)objfilestream.Length;
                                    Byte[] documentcontents = new Byte[len];
                                    objfilestream.Read(documentcontents, 0, len);
                                    objfilestream.Close();
                                    byte[] newValue = (byte[])documentcontents;
                                    dr["FATURA_PDF"] = newValue;
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                        myReader.Close();
                        myCommand.Connection.Close();
                    }
                }

            }
            return dt;
        }
        ///
        ///  LOGO'DAN TASLAK VE ORJINAL FATURA LISTESI OKUNUYOR
        ///    
        [WebMethod(Description = "INPUT:  FIRMA_KODU,BAS_TARIHI (yyyy.MM.dd) ,BIT_TARIHI (yyyy.MM.dd) || RETURN :FATURA_TARIHI, TASLAK_FATURA_NO ,ORJINAL_FATURA_NO ")]
        public DataTable  INVOICE_MATCH_LIST(string FIRMA_KODU, DateTime BAS_TARIHI, DateTime BIT_TARIHI)
        {
            string FIRMA_KODU_OLD = "";
            if (FIRMA_KODU == "GRM") { FIRMA_KODU_OLD = "517"; FIRMA_KODU = "518"; }
            if (FIRMA_KODU == "MAX") { FIRMA_KODU_OLD = "117"; FIRMA_KODU = "118"; }
            if (FIRMA_KODU == "MCM") { FIRMA_KODU_OLD = "417"; FIRMA_KODU = "418"; }
            if (FIRMA_KODU == "MDS") { FIRMA_KODU_OLD = "017"; FIRMA_KODU = "018"; }
            if (FIRMA_KODU == "MEC") { FIRMA_KODU_OLD = "317"; FIRMA_KODU = "318"; }
            if (FIRMA_KODU == "CON") { FIRMA_KODU_OLD = "617"; FIRMA_KODU = "618"; }
            if (FIRMA_KODU == "AMV") { FIRMA_KODU_OLD = "717"; FIRMA_KODU = "718"; }
            if (FIRMA_KODU == "WMG") { FIRMA_KODU_OLD = "318"; FIRMA_KODU = "328"; }

            //if (FIRMA_KODU == "GRM") { FIRMA_KODU = "516"; }
            //if (FIRMA_KODU == "MAX") { FIRMA_KODU = "116"; }
            //if (FIRMA_KODU == "MCM") { FIRMA_KODU = "416"; }
            //if (FIRMA_KODU == "MDS") { FIRMA_KODU = "016"; }
            //if (FIRMA_KODU == "MEC") { FIRMA_KODU = "316"; }
            //if (FIRMA_KODU == "QUI") { FIRMA_KODU = "616"; }  


            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringLogo"].ToString()))
            {
                string SQL = " SELECT DATE_ AS FATURA_TARIHI, DOCODE AS TASLAK_FATURA_NO , FICHENO AS ORJINAL_FATURA_NO  FROM dbo.[LG_" + FIRMA_KODU + "_01_INVOICE]  WHERE   (TRCODE=6 OR TRCODE=8) and (DATE_ >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (DATE_ <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";
                SQL += " UNION ALL SELECT DATE_ AS FATURA_TARIHI, DOCODE AS TASLAK_FATURA_NO , FICHENO AS ORJINAL_FATURA_NO  FROM dbo.[LG_" + FIRMA_KODU_OLD + "_01_INVOICE]  WHERE   (TRCODE=6 OR TRCODE=8) and (DATE_ >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (DATE_ <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";
                if (FIRMA_KODU == "018") SQL += " UNION  SELECT  DATE_ AS FATURA_TARIHI ,DOCODE AS TASLAK_FATURA_NO ,FICHENO AS ORJINAL_FATURA_NO  FROM dbo.[LG_517_01_INVOICE] WHERE  (TRCODE=6 OR TRCODE=8) and (DATE_ >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (DATE_ <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";
                if (FIRMA_KODU_OLD == "017") SQL += " UNION ALL SELECT  DATE_ AS FATURA_TARIHI ,DOCODE AS TASLAK_FATURA_NO ,FICHENO AS ORJINAL_FATURA_NO  FROM dbo.[LG_516_01_INVOICE] WHERE   (TRCODE=6 OR TRCODE=8) and (DATE_ >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (DATE_ <= CONVERT(DATETIME, @BIT_TARIHI , 102)) ";
                SqlDataAdapter da = new SqlDataAdapter(SQL, myConnection);
                da.SelectCommand.Parameters.AddWithValue("@BAS_TARIHI", BAS_TARIHI.ToString("yyyy.MM.dd"));
                da.SelectCommand.Parameters.AddWithValue("@BIT_TARIHI", BIT_TARIHI.ToString("yyyy.MM.dd"));
                DataTable dt = new DataTable("LG_INVOICE_MATCH_LIST");
                da.Fill(dt);
                return dt;
            }
        }

        ///
        ///  BÜTÇE ONAY FORMU
        ///    

        [WebMethod(Description = "INPUT : MECRA_TURU, OLCULEN_OLCULMEYEN,  PAZARLAMA_SIRKETI_KODU, YEREL_ULUSAL, ANAYANIN_KODU, MECRA_KODU, ADI, UNVANI, PRIME_TIME_START, PRIME_TIME_END,   ADRESI, POSTA_KODU, IL, ILCE, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,  KODU, VERGIDAIRESI, VERGINO,  CARI_HESAP_KODU, MUHASEBE_HESAP_KODU")]
        public string SET_BUTCE_ONAY_FORMU(DateTime TARIH, string SIRKET_KODU, string ISTEKTE_BULUNAN, string MUSTERI_MAIL, string MUSTERI_KODU, string CARI_ADI, string KAMPANYA_KODU, string PLAN_KODLARI, string URUN_KAMPANYA, string DONEMI, bool TV, bool GAZETE, bool DERGI, bool RADYO, bool SINEMA, bool OUTDOOR, bool OZEL_CALISMA, bool DIGER, string ACIKLAMA, float BUTCE_TL, float BUTCE_USD, string RAPOR_DONEMI)
        {
            string V_RETURN = "False";
            DateTime ISTEK_TARIHI = DateTime.Now;
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                SqlCommand myCommand = new SqlCommand();
                myConnection.Open();

                string SQL = " INSERT INTO   dbo.MDY_PLAN_BUDGET_ONAY (TARIH, SIRKET_KODU, ISTEK_TARIHI, ISTEK_SAATI,ISTEKTE_BULUNAN,MUSTERI_MAIL, MUSTERI_KODU, CARI_ADI, KAMPANYA_KODU, PLAN_KODLARI, URUN_KAMPANYA, DONEMI, TV, GAZETE, DERGI, RADYO, SINEMA, OUTDOOR, OZEL_CALISMA, DIGER, ACIKLAMA, BUTCE_TL, BUTCE_USD  ) VALUES " +
                       "( @TARIH, @SIRKET_KODU, @ISTEK_TARIHI,@ISTEK_SAATI,@ISTEKTE_BULUNAN,@MUSTERI_MAIL,@MUSTERI_KODU,@CARI_ADI,@KAMPANYA_KODU,@PLAN_KODLARI,@URUN_KAMPANYA, @DONEMI, @TV, @GAZETE, @DERGI, @RADYO, @SINEMA, @OUTDOOR, @OZEL_CALISMA, @DIGER, @ACIKLAMA, @BUTCE_TL, @BUTCE_USD) ";

                myCommand.Connection = myConnection;
                myCommand.Parameters.AddWithValue("@TARIH", TARIH.ToString("yyyy-MM-dd"));
                myCommand.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
                myCommand.Parameters.AddWithValue("@ISTEK_TARIHI", ISTEK_TARIHI.ToString("yyyy-MM-dd"));
                myCommand.Parameters.AddWithValue("@ISTEK_SAATI", ISTEK_TARIHI.ToString("hh.MM.ss"));
                myCommand.Parameters.AddWithValue("@ISTEKTE_BULUNAN", ISTEKTE_BULUNAN);
                myCommand.Parameters.AddWithValue("@MUSTERI_MAIL", MUSTERI_MAIL);
                myCommand.Parameters.AddWithValue("@MUSTERI_KODU", MUSTERI_KODU);
                myCommand.Parameters.AddWithValue("@CARI_ADI", CARI_ADI);
                myCommand.Parameters.AddWithValue("@KAMPANYA_KODU", KAMPANYA_KODU);
                myCommand.Parameters.AddWithValue("@PLAN_KODLARI", PLAN_KODLARI);
                myCommand.Parameters.AddWithValue("@URUN_KAMPANYA", URUN_KAMPANYA);
                myCommand.Parameters.AddWithValue("@DONEMI", DONEMI);
                myCommand.Parameters.AddWithValue("@TV", TV);
                myCommand.Parameters.AddWithValue("@GAZETE", GAZETE);
                myCommand.Parameters.AddWithValue("@DERGI", DERGI);
                myCommand.Parameters.AddWithValue("@RADYO", RADYO);
                myCommand.Parameters.AddWithValue("@SINEMA", SINEMA);
                myCommand.Parameters.AddWithValue("@OUTDOOR", OUTDOOR);
                myCommand.Parameters.AddWithValue("@OZEL_CALISMA", OZEL_CALISMA);
                myCommand.Parameters.AddWithValue("@DIGER", DIGER);
                myCommand.Parameters.AddWithValue("@ACIKLAMA", ACIKLAMA);
                myCommand.Parameters.AddWithValue("@BUTCE_TL", BUTCE_TL);
                myCommand.Parameters.AddWithValue("@BUTCE_USD", BUTCE_USD);
                myCommand.CommandText = SQL;
                myCommand.ExecuteNonQuery();
                myCommand.Parameters.Clear();
                V_RETURN = "True";
                string BODY = "";
                var appData = Server.MapPath("~");
                var file = Path.Combine(appData, Path.GetFileName("BUTCE_ONAY_FORMU.html"));
                NumberFormatInfo MyNFIUDS = new NumberFormatInfo();
                MyNFIUDS.NegativeSign = "-";
                MyNFIUDS.CurrencyDecimalSeparator = ",";
                MyNFIUDS.CurrencyGroupSeparator = ".";
                MyNFIUDS.CurrencySymbol = "$";
                NumberFormatInfo MyNFITL = new NumberFormatInfo();
                MyNFITL.NegativeSign = "-";
                MyNFITL.CurrencyDecimalSeparator = ".";
                MyNFITL.CurrencyGroupSeparator = ",";
                MyNFITL.CurrencySymbol = "TL";
                double _NumericTurkiye = Convert.ToDouble(BUTCE_TL);
                CultureInfo currentTR = CultureInfo.CurrentCulture;
                CultureInfo Turkiye = new CultureInfo("tr-TR");
                double _NumericUSD = Convert.ToDouble(BUTCE_USD);
                CultureInfo currentUSD = CultureInfo.CurrentCulture;
                CultureInfo USD = new CultureInfo("en-US");
                StreamReader sr = File.OpenText(file);
                {
                    BODY = sr.ReadToEnd();
                    BODY = BODY.Replace("KAMPANYA_KODU", KAMPANYA_KODU);
                    BODY = BODY.Replace("PLAN_KODLARI", PLAN_KODLARI);
                    BODY = BODY.Replace("FIRMA_UNVANI", CARI_ADI);
                    BODY = BODY.Replace("URUN_KAMPANYA", URUN_KAMPANYA);
                    BODY = BODY.Replace("DONEMI", DONEMI);
                    if (TV) { BODY = BODY.Replace("TELEVIZYON", "✔"); } else { BODY = BODY.Replace("TELEVIZYON", ""); }
                    if (GAZETE) { BODY = BODY.Replace("GAZETE", "✔"); } else { BODY = BODY.Replace("GAZETE", ""); }
                    if (DERGI) { BODY = BODY.Replace("DERGI", "✔"); } else { BODY = BODY.Replace("DERGI", ""); }
                    if (RADYO) { BODY = BODY.Replace("RADYO", "✔"); } else { BODY = BODY.Replace("RADYO", ""); }
                    if (SINEMA) { BODY = BODY.Replace("SINEMA", "✔"); } else { BODY = BODY.Replace("SINEMA", ""); }
                    if (OUTDOOR) { BODY = BODY.Replace("OUTDOOR", "✔"); } else { BODY = BODY.Replace("OUTDOOR", ""); }
                    if (OZEL_CALISMA) { BODY = BODY.Replace("OZEL_CALISMA", "✔"); } else { BODY = BODY.Replace("OZEL_CALISMA", ""); }
                    if (DIGER) { BODY = BODY.Replace("DIGER", "✔"); } else { BODY = BODY.Replace("DIGER", ""); }
                    //float dUSD = float.Parse(BUTCE_USD.ToString(), NumberStyles.Currency, MyNFIUDS);
                    //float dTL = float.Parse(BUTCE_TL.ToString(), NumberStyles.Currency, MyNFITL);
                    BODY = BODY.Replace("TOPLAM_TUTAR_TL", _NumericTurkiye.ToString("n2", Turkiye));
                    BODY = BODY.Replace("TOPLAM_TUTAR_USD", _NumericUSD.ToString("n2", USD));
                    // BODY = BODY.Replace("TOPLAM_TUTAR_TL", dTL.ToString());
                    BODY = BODY.Replace("ACIKLAMA", ACIKLAMA.ToString());
                }
                SendMail(ISTEKTE_BULUNAN, MUSTERI_MAIL, "Bütçe Onay Formu", BODY, "");
            }
            return V_RETURN;
        }

        ///
        ///  Mail Sender
        ///    
        [WebMethod(Description = "fromAddress, toAddress, subject, body,fileList")]
        public bool SendMail(string fromAddress, string toAddress, string subject, string body, string FileList)
        {
            MemoryStream ms;
            Attachment attached;
            try
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.From = new MailAddress(fromAddress);
                msg.To.Add(new MailAddress(toAddress));
                //msg.CC.Add(new MailAddress(CCAddress));     
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                string[] arr = FileList.ToString().Split(';');
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!String.IsNullOrEmpty(arr[i].ToString().Trim()))
                    {
                        ms = new MemoryStream(File.ReadAllBytes("C:\\DocumentDirectory\\" + arr[i].ToString().Trim()));
                        attached = new Attachment(ms, arr[i].ToString().Trim(), MediaTypeNames.Application.Octet);
                        msg.Attachments.Add(attached);
                    }
                }
                SmtpClient smtp = new SmtpClient("relay.nlbint.insidemedia.net");
                smtp.UseDefaultCredentials = true;
                smtp.Send(msg);
                return true;
            }
            catch (Exception exp)
            {
                //log exception here..  
                return false;
            }

        }

        [WebMethod]
        public bool DeleteDocument(string FileList)
        {
            string[] arr = FileList.ToString().Split(';');
            for (int i = 0; i < arr.Length; i++)
            {
                if (!String.IsNullOrEmpty(arr[i].ToString().Trim()))
                {
                    if (File.Exists("C:\\DocumentDirectory\\" + arr[i].ToString().Trim())) File.Delete("C:\\DocumentDirectory\\" + arr[i].ToString().Trim());
                }
            }
            return true;
        }

        [WebMethod]
        public bool SaveDocument(Byte[] docbinaryarray, string docname)
        {
            string strdocPath = "C:\\DocumentDirectory\\" + docname;
            FileStream objfilestream = new FileStream(strdocPath, FileMode.Create, FileAccess.ReadWrite);
            objfilestream.Write(docbinaryarray, 0, docbinaryarray.Length);
            objfilestream.Close();
            return true;
        }

        [WebMethod]
        public int GetDocumentLen(string DocumentName)
        {
            string strdocPath = "C:\\DocumentDirectory\\" + DocumentName;
            FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
            int len = (int)objfilestream.Length;
            objfilestream.Close();
            return len;
        }

        [WebMethod]
        public Byte[] GetDocument(string DocumentName)
        {
            string strdocPath = "C:\\DocumentDirectory\\" + DocumentName;
            FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
            int len = (int)objfilestream.Length;
            Byte[] documentcontents = new Byte[len];
            objfilestream.Read(documentcontents, 0, len);
            objfilestream.Close();
            return documentcontents;
        }

        [WebMethod]
        public string GetHtmlFile(string FIRMA_KODU, string FileName)
        {
            // string strdocPath = HttpContext.Current.Server.MapPath("~//_INBOX//" + FIRMA_KODU + "//" + FileName);
            string strdocPath = @"\\10.219.168.92\\d$\\efatura$\\_INBOX\\" + FIRMA_KODU + "\\" + FileName;
            string HtmlFiles = null;
            if (File.Exists(strdocPath))
            {
                using (StreamReader sr = new StreamReader(@"" + strdocPath, System.Text.Encoding.UTF8, true))
                {
                    HtmlFiles = sr.ReadToEnd();
                }
            }
            return HtmlFiles;

            //string strdocPaths = "\\10.219.168.92\\d$\\E-FATURA\\GRM\\0012014006324713_75f10a06-4f79-420e-8e4b-7b3ecc5da107.xml";
            //string HtmlFiless;
            //using (StreamReader sr = new StreamReader(@"" + strdocPaths))
            //{
            //    HtmlFiless = sr.ReadToEnd();
            //}        
        }
        [WebMethod]
        public Byte[] GetpdfFile(string FIRMA_KODU, string FileName)
        {
            string strdocPath = @"\\10.219.168.92\\d$\\efatura$\\_INBOX_PRINT\\" + FIRMA_KODU + "\\" + FileName;
            Byte[] documentcontents = null;
            if (File.Exists(strdocPath))
            {
                FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
                int len = (int)objfilestream.Length;
                documentcontents = new Byte[len];
                objfilestream.Read(documentcontents, 0, len);
                objfilestream.Close();
            }
            return documentcontents;
        }
        ///
        ///  SMART'dan GIB USER LIST ALINIYOR 
        ///    
        [WebMethod(Description = "RETURN :IDENTIFIER,TITLE")]
        public DataTable  GIB_USER_LIST()
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                string SQL = " SELECT [IDENTIFIER],[TITLE] FROM [VISION].[dbo].[FTR_GIB_FIRMA_LISTESI] group by [IDENTIFIER],[TITLE]";
                SqlDataAdapter da = new SqlDataAdapter(SQL, myConnection);
                DataTable dt = new DataTable("FTR_GIB_FIRMA_LISTESI");
                da.Fill(dt);
                return dt;
            }
        } 

 
        [WebMethod(Description = "INSERT : REFID,  SIRKET_KODU,  TALEBIN_TURU,  KULLANICI_MAIL_ADRESI,  PAZARLAMA_SIRKETI_ADI,  ADRES,  TELNO,  TELEFON_IKI,  FAX,  VERGIDAIRESI,  VERGI_NO,  TCK_NO_TCK,  ACIKLAMA,   CHK_TELEVIZYON, CHK_GAZETE, CHK_DERGI, CHK_SINEMA, CHK_RADYO, CHK_OUTDOOR, CHK_INTERNET, CHK_SPONSORLUK, RD_TUZEL, RD_SAHIS, RD_YERLI, RD_YABANCI,    SEMT,  ILCE,  ILCE_KODU,  IL,  IL_KODU,  ULKE,  ULKE_KODU,  HESAP_NO,  BANKA_ADI1,  IBAN,  EMAIL,  CARI_HESAP_KODU,  MUHASEBE_HESAP_KODU,  KL_KISISEL_BAG,  KL_KISISEL_BAG_ACIKLAMASI,  KL_MUSTERI_TALEBI,  KL_MUSTERI_ADI, ABA,  SWIFT_CODE")]
        public string PAZARLAMA_SIRKETI_INSERT(int REFID, string SIRKET_KODU, string TALEBIN_TURU, string KULLANICI_MAIL_ADRESI, string PAZARLAMA_SIRKETI_ADI, string ADRES, string TELNO, string TELEFON_IKI, string FAX, string VERGIDAIRESI, string VERGI_NO, string TCK_NO_TCK, string ACIKLAMA, bool CHK_TELEVIZYON, bool CHK_GAZETE, bool CHK_DERGI, bool CHK_SINEMA, bool CHK_RADYO, bool CHK_OUTDOOR, bool CHK_INTERNET, bool CHK_SPONSORLUK, bool RD_TUZEL, bool RD_SAHIS, bool RD_YERLI, bool RD_YABANCI, string SEMT, string ILCE, string ILCE_KODU, string IL, string IL_KODU, string ULKE, string ULKE_KODU, string HESAP_NO, string BANKA_ADI1, string IBAN, string EMAIL, string CARI_HESAP_KODU, string MUHASEBE_HESAP_KODU, string KL_KISISEL_BAG, string KL_KISISEL_BAG_ACIKLAMASI, string KL_MUSTERI_TALEBI, string KL_MUSTERI_ADI, string ABA, string SWIFT_CODE)
        {

            string YABANCI_UYRUKLU = "YERLI";
            string SAHIS_SIRKETI = "TUZEL";
            if (RD_TUZEL) SAHIS_SIRKETI = "TUZEL";
            if (RD_SAHIS) SAHIS_SIRKETI = "SAHIS";
            if (RD_YERLI) YABANCI_UYRUKLU = "YERLI";
            if (RD_YABANCI) YABANCI_UYRUKLU = "YABANCI";
            int _ID = 0;
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString());
            string myInsertQuery = @"INSERT INTO dbo.ADM_PAZARLAMA_SIRKETI(REF_ID, SIRKET_KODU, TALEBIN_TURU,ISTEKTE_BULUNAN_MAIL_ADRESI, UNVANI, ADRESI, TELEFON,TELEFON_IKI, FAX, VERGI_DAIRESI, VERGI_NO,TC_KIMLIK_NO,
                                     ACIKLAMA, TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,ISTEK_TARIHI,ISTEK_SAATI,EMAIL,  YABANCI_UYRUKLU,SAHIS_SIRKETI,SEMT,ILCE,ILCE_CODE,IL,IL_CODE,ULKE,ULKE_CODE,HESAP_NO,BANKA_ADI1,IBAN,
                                     CARI_HESAP_KODU,MUHASEBE_HESAP_KODU, KL_KISISEL_BAG,KL_KISISEL_BAG_ACIKLAMASI,KL_MUSTERI_TALEBI,KL_MUSTERI_TALEBI_ADI,
                                     FILE_SOZLESME_BYTE,FILE_SOZLESME_TYPE,FILE_SOZLESME_FILE_NAME,FILE_BIR_BYTE,FILE_BIR_TYPE,FILE_BIR_FILE_NAME,FILE_IKI_BYTE,FILE_IKI_TYPE,FILE_IKI_FILE_NAME,FILE_UC_BYTE,FILE_UC_TYPE,FILE_UC_FILE_NAME,FILE_DORT_BYTE,FILE_DORT_TYPE,FILE_DORT_FILE_NAME,FILE_BES_BYTE,FILE_BES_TYPE,FILE_BES_FILE_NAME,FILE_ALTI_BYTE,FILE_ALTI_TYPE,FILE_ALTI_FILE_NAME,ABA,SWIFT_CODE )                                      
                                     Values(@REF_ID,@SIRKET_KODU,@TALEBIN_TURU, @ISTEKTE_BULUNAN_MAIL_ADRESI, @UNVANI, @ADRESI, @TELEFON,@TELEFON_IKI,@FAX, @VERGI_DAIRESI, @VERGI_NO,@TC_KIMLIK_NO, @ACIKLAMA, @TV, @GAZETE, @DERGI, @SINEMA, @RADYO, @OUTDOOR, @INTERNET, @SPONSORLUK,@ISTEK_TARIHI,@ISTEK_SAATI,@EMAIL,@YABANCI_UYRUKLU,@SAHIS_SIRKETI,@SEMT,@ILCE,@ILCE_CODE,@IL,@IL_CODE,@ULKE,@ULKE_CODE,@HESAP_NO,@BANKA_ADI1,@IBAN,
                                     @CARI_HESAP_KODU,@MUHASEBE_HESAP_KODU, @KL_KISISEL_BAG,@KL_KISISEL_BAG_ACIKLAMASI,@KL_MUSTERI_TALEBI,@KL_MUSTERI_TALEBI_ADI,
                                     @ABA,@SWIFT_CODE)   SELECT @@IDENTITY AS ID";

            SqlCommand cmd = new SqlCommand(myInsertQuery);
            cmd.Parameters.AddWithValue("@REF_ID", REFID);
            cmd.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
            cmd.Parameters.AddWithValue("@TALEBIN_TURU", TALEBIN_TURU);
            cmd.Parameters.AddWithValue("@ISTEKTE_BULUNAN_MAIL_ADRESI", KULLANICI_MAIL_ADRESI);
            cmd.Parameters.AddWithValue("@UNVANI", PAZARLAMA_SIRKETI_ADI.ToUpper());
            cmd.Parameters.AddWithValue("@ADRESI", ADRES.ToUpper());
            cmd.Parameters.AddWithValue("@TELEFON", TELNO);
            cmd.Parameters.AddWithValue("@TELEFON_IKI", TELEFON_IKI);
            cmd.Parameters.AddWithValue("@FAX", FAX);
            cmd.Parameters.AddWithValue("@VERGI_DAIRESI", VERGIDAIRESI.ToUpper());
            cmd.Parameters.AddWithValue("@VERGI_NO", VERGI_NO);
            cmd.Parameters.AddWithValue("@TC_KIMLIK_NO", TCK_NO_TCK);
            cmd.Parameters.AddWithValue("@ACIKLAMA", ACIKLAMA);
            cmd.Parameters.AddWithValue("@TV", Convert.ToBoolean(CHK_TELEVIZYON));
            cmd.Parameters.AddWithValue("@GAZETE", Convert.ToBoolean(CHK_GAZETE));
            cmd.Parameters.AddWithValue("@DERGI", Convert.ToBoolean(CHK_DERGI));
            cmd.Parameters.AddWithValue("@SINEMA", Convert.ToBoolean(CHK_SINEMA));
            cmd.Parameters.AddWithValue("@RADYO", Convert.ToBoolean(CHK_RADYO));
            cmd.Parameters.AddWithValue("@OUTDOOR", Convert.ToBoolean(CHK_OUTDOOR));
            cmd.Parameters.AddWithValue("@INTERNET", Convert.ToBoolean(CHK_INTERNET));
            cmd.Parameters.AddWithValue("@SPONSORLUK", Convert.ToBoolean(CHK_SPONSORLUK));
            cmd.Parameters.AddWithValue("@YABANCI_UYRUKLU", YABANCI_UYRUKLU);
            cmd.Parameters.AddWithValue("@SAHIS_SIRKETI", SAHIS_SIRKETI);
            cmd.Parameters.AddWithValue("@SEMT", SEMT.ToUpper());
            cmd.Parameters.AddWithValue("@ILCE", ILCE);
            cmd.Parameters.AddWithValue("@ILCE_CODE", ILCE_KODU);
            cmd.Parameters.AddWithValue("@IL", IL);
            cmd.Parameters.AddWithValue("@IL_CODE", IL_KODU);
            cmd.Parameters.AddWithValue("@ULKE", ULKE);
            cmd.Parameters.AddWithValue("@ULKE_CODE", ULKE_KODU);
            cmd.Parameters.AddWithValue("@HESAP_NO", HESAP_NO);
            cmd.Parameters.AddWithValue("@BANKA_ADI1", BANKA_ADI1);
            cmd.Parameters.AddWithValue("@IBAN", IBAN);
            cmd.Parameters.AddWithValue("@ISTEK_TARIHI", myDT.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ISTEK_SAATI", myDT.ToString("hh:mm:ss"));
            cmd.Parameters.AddWithValue("@EMAIL", EMAIL);
            cmd.Parameters.AddWithValue("@CARI_HESAP_KODU", CARI_HESAP_KODU);
            cmd.Parameters.AddWithValue("@MUHASEBE_HESAP_KODU", MUHASEBE_HESAP_KODU);
            cmd.Parameters.AddWithValue("@KL_KISISEL_BAG", KL_KISISEL_BAG);
            cmd.Parameters.AddWithValue("@KL_KISISEL_BAG_ACIKLAMASI", KL_KISISEL_BAG_ACIKLAMASI);
            cmd.Parameters.AddWithValue("@KL_MUSTERI_TALEBI", KL_MUSTERI_TALEBI);
            cmd.Parameters.AddWithValue("@KL_MUSTERI_TALEBI_ADI", KL_MUSTERI_ADI);
            cmd.Parameters.AddWithValue("@ABA", ABA);
            cmd.Parameters.AddWithValue("@SWIFT_CODE", SWIFT_CODE);

            //cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = filename;
            //cmd.Parameters.Add("@ContentType", SqlDbType.VarChar).Value = "application/vnd.ms-word";
            //cmd.Parameters.Add("@Data", SqlDbType.Binary).Value = bytes;
            foreach (SqlParameter parameter in cmd.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }

            //if (FILE_SOZLESME_BYTE == null)
            //    cmd.Parameters.AddWithValue("@FILE_SOZLESME_BYTE", SqlBinary.Null);
            //else
            //    cmd.Parameters.AddWithValue("@FILE_SOZLESME_BYTE", FILE_SOZLESME_BYTE);

            cmd.Connection = myConnection;
            myConnection.Open();
            SqlDataReader myReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (myReader.Read())
            {
                _ID = Convert.ToInt16(myReader["ID"].ToString());
            }
            myReader.Close();
            cmd.Connection.Close();
            return _ID.ToString (); 
        } 

        [WebMethod(Description = "INSERT : REFID,  SIRKET_KODU,  TALEBIN_TURU,  KULLANICI_MAIL_ADRESI,  PAZARLAMA_SIRKETI_ADI,  ADRES,  TELNO,  TELEFON_IKI,  FAX,  VERGIDAIRESI,  VERGI_NO,  TCK_NO_TCK,  ACIKLAMA,   CHK_TELEVIZYON, CHK_GAZETE, CHK_DERGI, CHK_SINEMA, CHK_RADYO, CHK_OUTDOOR, CHK_INTERNET, CHK_SPONSORLUK, RD_TUZEL, RD_SAHIS, RD_YERLI, RD_YABANCI,    SEMT,  ILCE,  ILCE_KODU,  IL,  IL_KODU,  ULKE,  ULKE_KODU,  HESAP_NO,  BANKA_ADI1,  IBAN,  EMAIL,  CARI_HESAP_KODU,  MUHASEBE_HESAP_KODU,  KL_KISISEL_BAG,  KL_KISISEL_BAG_ACIKLAMASI,  KL_MUSTERI_TALEBI,  KL_MUSTERI_ADI, ABA,  SWIFT_CODE")]
        public string PAZARLAMA_SIRKETI_UPDATE(int REFID, string SIRKET_KODU, string TALEBIN_TURU, string KULLANICI_MAIL_ADRESI, string PAZARLAMA_SIRKETI_ADI, string ADRES, string TELNO, string TELEFON_IKI, string FAX, string VERGIDAIRESI, string VERGI_NO, string TCK_NO_TCK, string ACIKLAMA, bool CHK_TELEVIZYON, bool CHK_GAZETE, bool CHK_DERGI, bool CHK_SINEMA, bool CHK_RADYO, bool CHK_OUTDOOR, bool CHK_INTERNET, bool CHK_SPONSORLUK, bool RD_TUZEL, bool RD_SAHIS, bool RD_YERLI, bool RD_YABANCI, string SEMT, string ILCE, string ILCE_KODU, string IL, string IL_KODU, string ULKE, string ULKE_KODU, string HESAP_NO, string BANKA_ADI1, string IBAN, string EMAIL, string CARI_HESAP_KODU, string MUHASEBE_HESAP_KODU, string KL_KISISEL_BAG, string KL_KISISEL_BAG_ACIKLAMASI, string KL_MUSTERI_TALEBI, string KL_MUSTERI_ADI, string ABA, string SWIFT_CODE)
        {

            string YABANCI_UYRUKLU = "YERLI";
            string SAHIS_SIRKETI = "TUZEL";
            if (RD_TUZEL) SAHIS_SIRKETI = "TUZEL";
            if (RD_SAHIS) SAHIS_SIRKETI = "SAHIS";
            if (RD_YERLI) YABANCI_UYRUKLU = "YERLI";
            if (RD_YABANCI) YABANCI_UYRUKLU = "YABANCI";
            int _ID = 0;
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString());
            string myInsertQuery = @"INSERT INTO dbo.ADM_PAZARLAMA_SIRKETI(REF_ID, SIRKET_KODU, TALEBIN_TURU,ISTEKTE_BULUNAN_MAIL_ADRESI, UNVANI, ADRESI, TELEFON,TELEFON_IKI, FAX, VERGI_DAIRESI, VERGI_NO,TC_KIMLIK_NO,
                                     ACIKLAMA, TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,ISTEK_TARIHI,ISTEK_SAATI,EMAIL,  YABANCI_UYRUKLU,SAHIS_SIRKETI,SEMT,ILCE,ILCE_CODE,IL,IL_CODE,ULKE,ULKE_CODE,HESAP_NO,BANKA_ADI1,IBAN,
                                     CARI_HESAP_KODU,MUHASEBE_HESAP_KODU, KL_KISISEL_BAG,KL_KISISEL_BAG_ACIKLAMASI,KL_MUSTERI_TALEBI,KL_MUSTERI_TALEBI_ADI,
                                     FILE_SOZLESME_BYTE,FILE_SOZLESME_TYPE,FILE_SOZLESME_FILE_NAME,FILE_BIR_BYTE,FILE_BIR_TYPE,FILE_BIR_FILE_NAME,FILE_IKI_BYTE,FILE_IKI_TYPE,FILE_IKI_FILE_NAME,FILE_UC_BYTE,FILE_UC_TYPE,FILE_UC_FILE_NAME,FILE_DORT_BYTE,FILE_DORT_TYPE,FILE_DORT_FILE_NAME,FILE_BES_BYTE,FILE_BES_TYPE,FILE_BES_FILE_NAME,FILE_ALTI_BYTE,FILE_ALTI_TYPE,FILE_ALTI_FILE_NAME,ABA,SWIFT_CODE )                                      
                                     Values(@REF_ID,@SIRKET_KODU,@TALEBIN_TURU, @ISTEKTE_BULUNAN_MAIL_ADRESI, @UNVANI, @ADRESI, @TELEFON,@TELEFON_IKI,@FAX, @VERGI_DAIRESI, @VERGI_NO,@TC_KIMLIK_NO, @ACIKLAMA, @TV, @GAZETE, @DERGI, @SINEMA, @RADYO, @OUTDOOR, @INTERNET, @SPONSORLUK,@ISTEK_TARIHI,@ISTEK_SAATI,@EMAIL,@YABANCI_UYRUKLU,@SAHIS_SIRKETI,@SEMT,@ILCE,@ILCE_CODE,@IL,@IL_CODE,@ULKE,@ULKE_CODE,@HESAP_NO,@BANKA_ADI1,@IBAN,
                                     @CARI_HESAP_KODU,@MUHASEBE_HESAP_KODU, @KL_KISISEL_BAG,@KL_KISISEL_BAG_ACIKLAMASI,@KL_MUSTERI_TALEBI,@KL_MUSTERI_TALEBI_ADI,
                                     @ABA,@SWIFT_CODE)   SELECT @@IDENTITY AS ID";

            SqlCommand cmd = new SqlCommand(myInsertQuery);
            cmd.Parameters.AddWithValue("@REF_ID", REFID);
            cmd.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
            cmd.Parameters.AddWithValue("@TALEBIN_TURU", TALEBIN_TURU);
            cmd.Parameters.AddWithValue("@ISTEKTE_BULUNAN_MAIL_ADRESI", KULLANICI_MAIL_ADRESI);
            cmd.Parameters.AddWithValue("@UNVANI", PAZARLAMA_SIRKETI_ADI.ToUpper());
            cmd.Parameters.AddWithValue("@ADRESI", ADRES.ToUpper());
            cmd.Parameters.AddWithValue("@TELEFON", TELNO);
            cmd.Parameters.AddWithValue("@TELEFON_IKI", TELEFON_IKI);
            cmd.Parameters.AddWithValue("@FAX", FAX);
            cmd.Parameters.AddWithValue("@VERGI_DAIRESI", VERGIDAIRESI.ToUpper());
            cmd.Parameters.AddWithValue("@VERGI_NO", VERGI_NO);
            cmd.Parameters.AddWithValue("@TC_KIMLIK_NO", TCK_NO_TCK);
            cmd.Parameters.AddWithValue("@ACIKLAMA", ACIKLAMA);
            cmd.Parameters.AddWithValue("@TV", Convert.ToBoolean(CHK_TELEVIZYON));
            cmd.Parameters.AddWithValue("@GAZETE", Convert.ToBoolean(CHK_GAZETE));
            cmd.Parameters.AddWithValue("@DERGI", Convert.ToBoolean(CHK_DERGI));
            cmd.Parameters.AddWithValue("@SINEMA", Convert.ToBoolean(CHK_SINEMA));
            cmd.Parameters.AddWithValue("@RADYO", Convert.ToBoolean(CHK_RADYO));
            cmd.Parameters.AddWithValue("@OUTDOOR", Convert.ToBoolean(CHK_OUTDOOR));
            cmd.Parameters.AddWithValue("@INTERNET", Convert.ToBoolean(CHK_INTERNET));
            cmd.Parameters.AddWithValue("@SPONSORLUK", Convert.ToBoolean(CHK_SPONSORLUK));
            cmd.Parameters.AddWithValue("@YABANCI_UYRUKLU", YABANCI_UYRUKLU);
            cmd.Parameters.AddWithValue("@SAHIS_SIRKETI", SAHIS_SIRKETI);
            cmd.Parameters.AddWithValue("@SEMT", SEMT.ToUpper());
            cmd.Parameters.AddWithValue("@ILCE", ILCE);
            cmd.Parameters.AddWithValue("@ILCE_CODE", ILCE_KODU);
            cmd.Parameters.AddWithValue("@IL", IL);
            cmd.Parameters.AddWithValue("@IL_CODE", IL_KODU);
            cmd.Parameters.AddWithValue("@ULKE", ULKE);
            cmd.Parameters.AddWithValue("@ULKE_CODE", ULKE_KODU);
            cmd.Parameters.AddWithValue("@HESAP_NO", HESAP_NO);
            cmd.Parameters.AddWithValue("@BANKA_ADI1", BANKA_ADI1);
            cmd.Parameters.AddWithValue("@IBAN", IBAN);
            cmd.Parameters.AddWithValue("@ISTEK_TARIHI", myDT.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ISTEK_SAATI", myDT.ToString("hh:mm:ss"));
            cmd.Parameters.AddWithValue("@EMAIL", EMAIL);
            cmd.Parameters.AddWithValue("@CARI_HESAP_KODU", CARI_HESAP_KODU);
            cmd.Parameters.AddWithValue("@MUHASEBE_HESAP_KODU", MUHASEBE_HESAP_KODU);
            cmd.Parameters.AddWithValue("@KL_KISISEL_BAG", KL_KISISEL_BAG);
            cmd.Parameters.AddWithValue("@KL_KISISEL_BAG_ACIKLAMASI", KL_KISISEL_BAG_ACIKLAMASI);
            cmd.Parameters.AddWithValue("@KL_MUSTERI_TALEBI", KL_MUSTERI_TALEBI);
            cmd.Parameters.AddWithValue("@KL_MUSTERI_TALEBI_ADI", KL_MUSTERI_ADI);
            cmd.Parameters.AddWithValue("@ABA", ABA);
            cmd.Parameters.AddWithValue("@SWIFT_CODE", SWIFT_CODE);

            //cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = filename;
            //cmd.Parameters.Add("@ContentType", SqlDbType.VarChar).Value = "application/vnd.ms-word";
            //cmd.Parameters.Add("@Data", SqlDbType.Binary).Value = bytes;
            foreach (SqlParameter parameter in cmd.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }

            //if (FILE_SOZLESME_BYTE == null)
            //    cmd.Parameters.AddWithValue("@FILE_SOZLESME_BYTE", SqlBinary.Null);
            //else
            //    cmd.Parameters.AddWithValue("@FILE_SOZLESME_BYTE", FILE_SOZLESME_BYTE);

            cmd.Connection = myConnection;
            myConnection.Open();
            SqlDataReader myReader = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (myReader.Read())
            {
                _ID = Convert.ToInt16(myReader["ID"].ToString());
            }
            myReader.Close();
            cmd.Connection.Close();
            return _ID.ToString();
        }
          
        [WebMethod(Description = "DELETE :KODU,UNVANI")]
        public string PAZARLAMA_SIRKETI_DELETE(string KODU, string UNVANI)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" SELECT  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,  SAHIS_SIRKETI , TC_KIMLIK_NO , VERGI_NO   ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString()); 
 
                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        } 
         
         
        [WebMethod(Description = "RETURN :KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,VERGI_NO,CARI_HESAP_KODU,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU")]
        public DataTable  PAZARLAMA_SIRKETI_LIST(DateTime BAS_TARIHI, DateTime BIT_TARIHI)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" SELECT  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI, Case When SAHIS_SIRKETI='SAHIS' then TC_KIMLIK_NO else VERGI_NO end AS VERGI_NO ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);
                BAS_TARIHI = BIT_TARIHI;
                BAS_TARIHI = BAS_TARIHI.AddDays(-50);
                da.SelectCommand.Parameters.AddWithValue("@BAS_TARIHI", BAS_TARIHI.ToString("yyyy.MM.dd"));
                da.SelectCommand.Parameters.AddWithValue("@BIT_TARIHI", BIT_TARIHI.ToString("yyyy.MM.dd"));
                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return dt;
            }
        } 


        /// <summary>
        /// /
        /// </summary>
        /// <param name="REFID"></param>
        /// <param name="SIRKET_KODU"></param>
        /// <param name="KULLANICI_MAIL_ADRESI"></param>
        /// <param name="MECRATURU"></param>
        /// <param name="YAYINPERIYODU"></param>
        /// <param name="PAZARLAMASIRKETI_KODU"></param>
        /// <param name="PAZARLAMA_SIRKETI_UNVANI"></param>
        /// <param name="MECRA_ADI"></param>
        /// <param name="WEB_ADRESI"></param>
        /// <param name="ADRESI"></param>
        /// <param name="TELEFON"></param>
        /// <param name="FAX"></param>
        /// <param name="EMAIL"></param>
        /// <param name="VERGIDAIRESI"></param>
        /// <param name="VERGINO"></param>
        /// <param name="TCKNO"></param>
        /// <param name="ACIKLAMA"></param>
        /// <returns></returns>

        [WebMethod(Description = "INSERT :  REFID,   SIRKET_KODU,   KULLANICI_MAIL_ADRESI,   CMB_MECRATURU,   CMB_YAYINPERIYODU,    PAZARLAMASIRKETI_KODU,    PAZARLAMASIRKETI,   MECRA_ADI,   WEB_ADRESI,    ADRESI,   TELEFON,   FAX,   EMAIL,    VERGIDAIRESI,   VERGINO,    TCKNO,     ACIKLAMA")]

        public string MECRA_INSERT(int REFID, string SIRKET_KODU, string KULLANICI_MAIL_ADRESI, string MECRATURU, string YAYINPERIYODU, string PAZARLAMASIRKETI_KODU, string PAZARLAMA_SIRKETI_UNVANI, string MECRA_ADI, string WEB_ADRESI, string ADRESI, string TELEFON, string FAX, string EMAIL, string VERGIDAIRESI, string VERGINO, string TCKNO, string ACIKLAMA)
        {
            string RETURNS = ""; 
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString());
            string SQL = "INSERT INTO dbo.ADM_MECRA ( REF_ID,SIRKET_KODU, ISTEKTE_BULUNAN_MAIL_ADRESI,ISTEK_TARIHI, ISTEK_SAATI, MECRA_TURU, YAYIN_PERIYODU,PAZARLAMA_SIRKETI_KODU ,PAZARLAMA_SIRKETI_UNVANI, UNVANI, WEBADRESI,ADRESI, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,TC_KIMLIK_NO, ACIKLAMA,EMAIL) " +
                                                "Values(@REF_ID,@SIRKET_KODU, @ISTEKTE_BULUNAN_MAIL_ADRESI,@ISTEK_TARIHI, @ISTEK_SAATI, @MECRA_TURU, @YAYIN_PERIYODU,@PAZARLAMA_SIRKETI_KODU, @PAZARLAMA_SIRKETI_UNVANI, @UNVANI,@WEBADRESI, @ADRESI, @TELEFON, @FAX, @VERGI_DAIRESI, @VERGI_NO,@TC_KIMLIK_NO, @ACIKLAMA,@EMAIL)  SELECT @@IDENTITY AS ID ";
            SqlCommand myCommand = new SqlCommand(SQL);
            myCommand.Parameters.AddWithValue("@REF_ID", REFID);
            myCommand.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
            myCommand.Parameters.AddWithValue("@ISTEKTE_BULUNAN_MAIL_ADRESI", KULLANICI_MAIL_ADRESI);
            myCommand.Parameters.AddWithValue("@MECRA_TURU", MECRATURU);
            myCommand.Parameters.AddWithValue("@YAYIN_PERIYODU", YAYINPERIYODU);
            myCommand.Parameters.AddWithValue("@PAZARLAMA_SIRKETI_KODU", PAZARLAMASIRKETI_KODU);
            myCommand.Parameters.AddWithValue("@PAZARLAMA_SIRKETI_UNVANI", PAZARLAMA_SIRKETI_UNVANI);
            myCommand.Parameters.AddWithValue("@UNVANI", MECRA_ADI);
            myCommand.Parameters.AddWithValue("@WEBADRESI", WEB_ADRESI);
            myCommand.Parameters.AddWithValue("@ADRESI", ADRESI);
            myCommand.Parameters.AddWithValue("@TELEFON", TELEFON);
            myCommand.Parameters.AddWithValue("@FAX", FAX);
            myCommand.Parameters.AddWithValue("@EMAIL", EMAIL);
            myCommand.Parameters.AddWithValue("@VERGI_DAIRESI", VERGIDAIRESI);
            myCommand.Parameters.AddWithValue("@VERGI_NO", VERGINO);
            myCommand.Parameters.AddWithValue("@TC_KIMLIK_NO", TCKNO);
            myCommand.Parameters.AddWithValue("@ACIKLAMA", ACIKLAMA);
            myCommand.Parameters.AddWithValue("@ISTEK_TARIHI", myDT.ToString("yyyy-MM-dd"));
            myCommand.Parameters.AddWithValue("@ISTEK_SAATI", myDT.ToString("hh:mm:ss"));
            foreach (SqlParameter parameter in myCommand.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }
            myCommand.Connection = myConnection;
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (myReader.Read())
            {
                RETURNS +="ID:" + myReader["ID"].ToString() +";TYPE: OK";
            }
            myReader.Close();
            myCommand.Connection.Close();
            return RETURNS;
        }
         

        /// <summary>
        /// 
        /// </summary>
        /// <param name="REFID"></param>
        /// <param name="SIRKET_KODU"></param>
        /// <param name="KULLANICI_MAIL_ADRESI"></param>
        /// <param name="CMB_MECRATURU"></param>
        /// <param name="CMB_YAYINPERIYODU"></param>
        /// <param name="TXT_PAZARLAMASIRKETI_KODU"></param>
        /// <param name="CMB_PAZARLAMASIRKETI"></param>
        /// <param name="TXT_MECRA_ADI"></param>
        /// <param name="TXT_WEB_ADRESI"></param>
        /// <param name="txt_ADRESI"></param>
        /// <param name="txt_TELEFON"></param>
        /// <param name="txt_FAX"></param>
        /// <param name="TXT_EMAIL"></param>
        /// <param name="txt_VERGIDAIRESI"></param>
        /// <param name="txt_VERGINO"></param>
        /// <param name="txt_TCKNO"></param>
        /// <param name="txt_ACIKLAMA"></param>
        /// <returns></returns>
        [WebMethod(Description = "UPDATE : REFID,   SIRKET_KODU,   KULLANICI_MAIL_ADRESI,   CMB_MECRATURU,   CMB_YAYINPERIYODU,    PAZARLAMASIRKETI_KODU,    PAZARLAMASIRKETI,   MECRA_ADI,   WEB_ADRESI,    ADRESI,   TELEFON,   FAX,   EMAIL,    VERGIDAIRESI,   VERGINO,    TCKNO,     ACIKLAMA")]
        public string MECRA_UPDATE(int REFID, string SIRKET_KODU, string KULLANICI_MAIL_ADRESI, string CMB_MECRATURU, string CMB_YAYINPERIYODU, string TXT_PAZARLAMASIRKETI_KODU, string CMB_PAZARLAMASIRKETI, string TXT_MECRA_ADI, string TXT_WEB_ADRESI, string txt_ADRESI, string txt_TELEFON, string txt_FAX, string TXT_EMAIL, string txt_VERGIDAIRESI, string txt_VERGINO, string txt_TCKNO, string txt_ACIKLAMA)
        {
            string RETURNS = "";
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString());
            string SQL = " UPDATE INTO dbo.ADM_MECRA ( REF_ID,SIRKET_KODU=@SIRKET_KODU, ISTEKTE_BULUNAN_MAIL_ADRESI,ISTEK_TARIHI, ISTEK_SAATI, MECRA_TURU, YAYIN_PERIYODU,PAZARLAMA_SIRKETI_KODU ,PAZARLAMA_SIRKETI_UNVANI, UNVANI, WEBADRESI,ADRESI, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,TC_KIMLIK_NO, ACIKLAMA,EMAIL) " +
                                                "Values(@REF_ID, @ISTEKTE_BULUNAN_MAIL_ADRESI,@ISTEK_TARIHI, @ISTEK_SAATI, @MECRA_TURU, @YAYIN_PERIYODU,@PAZARLAMA_SIRKETI_KODU, @PAZARLAMA_SIRKETI_UNVANI, @UNVANI,@WEBADRESI, @ADRESI, @TELEFON, @FAX, @VERGI_DAIRESI, @VERGI_NO,@TC_KIMLIK_NO, @ACIKLAMA,@EMAIL)  SELECT @@IDENTITY AS ID ";
            SqlCommand myCommand = new SqlCommand(SQL);
            myCommand.Parameters.AddWithValue("@REF_ID", REFID);
            myCommand.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
            myCommand.Parameters.AddWithValue("@ISTEKTE_BULUNAN_MAIL_ADRESI", KULLANICI_MAIL_ADRESI);
            myCommand.Parameters.AddWithValue("@MECRA_TURU", CMB_MECRATURU);
            myCommand.Parameters.AddWithValue("@YAYIN_PERIYODU", CMB_YAYINPERIYODU);
            myCommand.Parameters.AddWithValue("@PAZARLAMA_SIRKETI_KODU", TXT_PAZARLAMASIRKETI_KODU);
            myCommand.Parameters.AddWithValue("@PAZARLAMA_SIRKETI_UNVANI", CMB_PAZARLAMASIRKETI);
            myCommand.Parameters.AddWithValue("@UNVANI", TXT_MECRA_ADI);
            myCommand.Parameters.AddWithValue("@WEBADRESI", TXT_WEB_ADRESI);
            myCommand.Parameters.AddWithValue("@ADRESI", txt_ADRESI);
            myCommand.Parameters.AddWithValue("@TELEFON", txt_TELEFON);
            myCommand.Parameters.AddWithValue("@FAX", txt_FAX);
            myCommand.Parameters.AddWithValue("@EMAIL", TXT_EMAIL);
            myCommand.Parameters.AddWithValue("@VERGI_DAIRESI", txt_VERGIDAIRESI);
            myCommand.Parameters.AddWithValue("@VERGI_NO", txt_VERGINO);
            myCommand.Parameters.AddWithValue("@TC_KIMLIK_NO", txt_TCKNO);
            myCommand.Parameters.AddWithValue("@ACIKLAMA", txt_ACIKLAMA);
            myCommand.Parameters.AddWithValue("@ISTEK_TARIHI", myDT.ToString("yyyy-MM-dd"));
            myCommand.Parameters.AddWithValue("@ISTEK_SAATI", myDT.ToString("hh:mm:ss"));
            foreach (SqlParameter parameter in myCommand.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }
            myCommand.Connection = myConnection;
            myConnection.Open();
            SqlDataReader myReader = myCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (myReader.Read())
            {
                RETURNS += "ID:" + myReader["ID"].ToString()  +";TYPE: OK";
            }
            myReader.Close();
            myCommand.Connection.Close();
            return RETURNS;
        }
         

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MECRA_TURU"></param>
        /// <param name="KODU"></param>
        /// <param name="UNVANI"></param>
        /// <returns></returns>
        [WebMethod(Description = "DELETE :MECRA_TURU,KODU,UNVANI")]
        public string MECRA_DELETE(string MECRA_TURU, string KODU, string UNVANI)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" UPDATE  KODU,UNVANI FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@MECRA_TURU", MECRA_TURU.ToString());  
                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        }
         
        [WebMethod(Description = "RETURN : MECRA_TURU, OLCULEN_OLCULMEYEN,  PAZARLAMA_SIRKETI_KODU, YEREL_ULUSAL, ANAYANIN_KODU, MECRA_KODU, ADI, UNVANI, PRIME_TIME_START, PRIME_TIME_END,   ADRESI, POSTA_KODU, IL, ILCE, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,  KODU, VERGIDAIRESI, VERGINO,  CARI_HESAP_KODU, MUHASEBE_HESAP_KODU")]
        public DataTable MECRA_LIST(DateTime BAS_TARIHI, DateTime BIT_TARIHI)
        {

            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" SELECT   MECRA_TURU, OLCULEN_OLCULMEYEN,  PAZARLAMA_SIRKETI_KODU, YEREL_ULUSAL, ANAYANIN_KODU, MECRA_KODU, ADI, UNVANI, PRIME_TIME_START, PRIME_TIME_END,   ADRESI, POSTA_KODU, IL, ILCE, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,  KODU, VERGIDAIRESI, VERGINO,  CARI_HESAP_KODU, MUHASEBE_HESAP_KODU FROM [VISION].[dbo].[ADM_MECRA]  where  (ISTEK_TARIHI >= CONVERT(ISTEK_TARIHI, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(ISTEK_TARIHI, @BIT_TARIHI , 102)) ", myConnection);
                BAS_TARIHI = BIT_TARIHI;
                BAS_TARIHI = BAS_TARIHI.AddDays(-50);
                da.SelectCommand.Parameters.AddWithValue("@BAS_TARIHI", BAS_TARIHI.ToString("yyyy.MM.dd"));
                da.SelectCommand.Parameters.AddWithValue("@BIT_TARIHI", BIT_TARIHI.ToString("yyyy.MM.dd"));
                DataTable dt = new DataTable("GET_MECRA_LIST");
                da.Fill(dt);
                return dt;
            }

        }


        [WebMethod(Description = "INSERT :int REFID,  SIRKET_KODU,  KULLANICI_MAIL_ADRESI, CMB_DIRECTOR,  RDMEDYAAJANSI,  RDCREATIVEAJANSI,  RDTUZEL,  RDSAHIS,  RDYERLI,  RDYABANCI,  MUSTERI_ADI,  MUSTERI_MARKALARI, CMB_ULKE_NAME,  CMB_ULKE_CODE,  CMB_IL_NAME,  CMB_IL_CODE,  CMB_ILCE_NAME,  CMB_ILCE_CODE,  CMB_SEMT,  ADRES_BIR,              ADRES_IKI,  TEL_NO_BIR,  TEL_NO_IKI,  FAX_NO,  EFATURA,  PO,  FATURA_MUSTERI_ADI,              CMB_FATURA_ULKE_NAME,  CMB_FATURA_ULKE_CODE,  CMB_FATURA_IL_NAME,  CMB_FATURA_IL_CODE,  CMB_FATURA_ILCE_NAME,  CMB_FATURA_ILCE_CODE,  CMB_FATURA_SEMT,              FATURA_ADRES_BIR,  FATURA_ADRES_IKI,  VERGI_DAIRESI,  VERGI_NO,  EMAIL,  TC_KIMLIK_NO,  HESAP_NO,  IBAN,              AY_KAPANISI,  SON_FATURA_KABUL_TARIHI,  RDAJANSA,  RDMUSTERIYE,  CHKBACKUP,  CHKONAYMAILI,  CHKYAYINKANITI,              CARI_HESAP_KODU,  MUHASEBE_HESAP_KODU,  SWIFT_CODE,  WIRE_ABA" )]
      

        public string MUSTERI_INSERT(int REFID, string SIRKET_KODU, string KULLANICI_MAIL_ADRESI,
             string DIRECTOR, bool RDMEDYAAJANSI, bool RDCREATIVEAJANSI, bool RDTUZEL, bool RDSAHIS, bool RDYERLI, bool RDYABANCI, string MUSTERI_ADI, string MUSTERI_MARKALARI,
             string ULKE, string ULKE_CODE, string IL, string IL_CODE, string ILCE, string ILCE_CODE, string SEMT, string ADRES_BIR,
             string ADRES_IKI, string TEL_NO_BIR, string TEL_NO_IKI, string FAX_NO, bool EFATURA, bool PO,
            
             string VERGI_DAIRESI, string VERGI_NO, string EMAIL, string TC_KIMLIK_NO, string HESAP_NO, string IBAN,
             string AY_KAPANISI, string SON_FATURA_KABUL_TARIHI, bool RDAJANSA, bool RDMUSTERIYE, bool CHKBACKUP, bool CHKONAYMAILI, bool CHKYAYINKANITI,
             string CARI_HESAP_KODU, string MUHASEBE_HESAP_KODU, string SWIFT_CODE, string WIRE_ABA 
        ) 
        {





            string RETURN = "";
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
            {
                Guid gg = Guid.NewGuid();

                SqlCommand Cmd = new SqlCommand();


                //, CALISMA_SEKLI_KOMISYON, CALISMA_SEKLI_FEE, KOMISYON_ACIKLAMASI, FEE_ACIKLAMASI, MUHASEBE_YETKILISI, MUHASEBE_YETKILISI_TELEFON, ODEME_YETKILISI, ODEME_YETKILISI_TELEFON, CARIHESAP_YETKILISI, CARIHESAP_YETKILISI_TELEFON, ODEME_VADELERI_TV, ODEME_VADELERI_BASIN, ODEME_VADELERI_RADYO, ODEME_VADELERI_OUTDOOR, ODEME_VADELERI_SINEMA, ODEME_VADELERI_INTERNET, MVD_ACIKLAMASI, FATURA_EKI_BACKUP, FATURA_EKI_ONAYMAILI, FATURA_EKI_YAYIN_KANITI, SOZLESME_DURUMU, MEDYA_AJANS_MEDYA_SOX_SORUMLUSU, MEDYA_AJANS_MEDPLAN_SORUMLUSU, MEDYA_AJANS_MECRA_FTR_TESLIM, MEDYA_AJANS_TRADING_SORUMLUSU, MEDYA_AJANS_MUSTERI_FATURA_SORUMLUSU, MEDYA_AJANS_SOX_SORUMLUSU, MEDYA_AJANS_FIRMA_UNVANI, MEDYA_AJANS_ADRESI, MEDYA_AJANS_POSTA_ADRESI, MEDYA_AJANS_MUHASEBE_YETKILISI, MEDYA_AJANS_TICARI_FIN_YETKILISI, MEDYA_AJANS_ODEME_TAHSILAT_SORUMLUSU, MEDYA_AJANS_MUSTERI_FTR_SORUMLUSU, MEDYA_AJANS_CARI_HESAP_SORUMLUSU, MEDYA_AJANS_MECRA_FTR_SORUMLUSU, MEDYA_AJANS_MUHASEBE_FAX, MEDYA_AJANS_MUHASEBE_TEL) " +
                // ,@CALISMA_SEKLI_KOMISYON,@CALISMA_SEKLI_FEE,@KOMISYON_ACIKLAMASI,@FEE_ACIKLAMASI,@MUHASEBE_YETKILISI,@MUHASEBE_YETKILISI_TELEFON,@ODEME_YETKILISI,@ODEME_YETKILISI_TELEFON,@CARIHESAP_YETKILISI,@CARIHESAP_YETKILISI_TELEFON,@ODEME_VADELERI_TV,@ODEME_VADELERI_BASIN,@ODEME_VADELERI_RADYO,@ODEME_VADELERI_OUTDOOR,@ODEME_VADELERI_SINEMA,@ODEME_VADELERI_INTERNET,@MVD_ACIKLAMASI,@FATURA_EKI_BACKUP,@FATURA_EKI_ONAYMAILI,@FATURA_EKI_YAYIN_KANITI,@SOZLESME_DURUMU,@MEDYA_AJANS_MEDYA_SOX_SORUMLUSU,@MEDYA_AJANS_MEDPLAN_SORUMLUSU,@MEDYA_AJANS_MECRA_FTR_TESLIM,@MEDYA_AJANS_TRADING_SORUMLUSU,@MEDYA_AJANS_MUSTERI_FATURA_SORUMLUSU,@MEDYA_AJANS_SOX_SORUMLUSU,@MEDYA_AJANS_FIRMA_UNVANI,@MEDYA_AJANS_ADRESI,@MEDYA_AJANS_POSTA_ADRESI,@MEDYA_AJANS_MUHASEBE_YETKILISI,@MEDYA_AJANS_TICARI_FIN_YETKILISI,@MEDYA_AJANS_ODEME_TAHSILAT_SORUMLUSU,@MEDYA_AJANS_MUSTERI_FTR_SORUMLUSU,@MEDYA_AJANS_CARI_HESAP_SORUMLUSU,@MEDYA_AJANS_MECRA_FTR_SORUMLUSU,@MEDYA_AJANS_MUHASEBE_FAX,@MEDYA_AJANS_MUHASEBE_TEL

                Cmd.CommandText = " INSERT INTO dbo.ADM_MUSTERI ( GUID,REF_ID, ISTEK_TARIHI, ISTEK_SAATI, ISTEKTE_BULUNAN, DIRECTOR, MEDYA_AJANS_MUSTERISI,CREATIVE_AJANS_MUSTERISI,TUZEL_SIRKET, SAHIS_SIRKET, YABANCI_SIRKET, YERLI_SIRKET, MECRA_FATURASI_AJANSA, MECRA_FATURASI_MUSTERIYE, SIRKET_KODU, UNVANI, ULKE, IL, ILCE, ADRESI, VERGI_DAIRESI, VERGI_NO, TC_KIMLIK_NO, TELEFON, FAX, HESAP_NO, IBAN,EMAIL, FATURA_TESLIM_ULKE, FATURA_TESLIM_IL, FATURA_TESLIM_ILCE, FATURA_TESLIM_ADRESI, FATURA_TESLIM_ALAN_I, FATURA_TESLIM_ALAN_II, FATURA_TESLIM_ALAN_I_TEL, FATURA_TESLIM_ALAN_II_TEL) " +
                                                             " Values( @GUID,@REF_ID,@ISTEK_TARIHI,@ISTEK_SAATI,@ISTEKTE_BULUNAN,@DIRECTOR,@MEDYA_AJANS_MUSTERISI,@CREATIVE_AJANS_MUSTERISI,@TUZEL_SIRKET,@SAHIS_SIRKET,@YABANCI_SIRKET,@YERLI_SIRKET,@MECRA_FATURASI_AJANSA,@MECRA_FATURASI_MUSTERIYE,@SIRKET_KODU,@UNVANI,@ULKE,@IL,@ILCE,@ADRESI,@VERGI_DAIRESI,@VERGI_NO,@TC_KIMLIK_NO,@TELEFON,@FAX,@HESAP_NO,@IBAN,@EMAIL,@FATURA_TESLIM_ULKE,@FATURA_TESLIM_IL,@FATURA_TESLIM_ILCE,@FATURA_TESLIM_ADRESI,@FATURA_TESLIM_ALAN_I,@FATURA_TESLIM_ALAN_II,@FATURA_TESLIM_ALAN_I_TEL,@FATURA_TESLIM_ALAN_II_TEL  ) SELECT @@IDENTITY AS ID   ";

                Cmd.Parameters.AddWithValue("@GUID", gg);
                Cmd.Parameters.AddWithValue("@REF_ID", REFID);
                Cmd.Parameters.AddWithValue("@ISTEK_TARIHI", myDT.ToString("yyyy-MM-dd"));
                Cmd.Parameters.AddWithValue("@ISTEK_SAATI", myDT.ToString("hh:mm:ss"));
                Cmd.Parameters.AddWithValue("@SIRKET_KODU", SIRKET_KODU);
                Cmd.Parameters.AddWithValue("@ISTEKTE_BULUNAN", KULLANICI_MAIL_ADRESI);
                Cmd.Parameters.AddWithValue("@DIRECTOR", DIRECTOR);
                Cmd.Parameters.AddWithValue("@MEDYA_AJANS_MUSTERISI", RDMEDYAAJANSI);
                Cmd.Parameters.AddWithValue("@CREATIVE_AJANS_MUSTERISI", RDCREATIVEAJANSI);
                Cmd.Parameters.AddWithValue("@TUZEL_SIRKET", RDTUZEL);
                Cmd.Parameters.AddWithValue("@SAHIS_SIRKET", RDSAHIS);
                Cmd.Parameters.AddWithValue("@YERLI_SIRKET", RDYERLI);
                Cmd.Parameters.AddWithValue("@YABANCI_SIRKET", RDYABANCI);
                Cmd.Parameters.AddWithValue("@MECRA_FATURASI_AJANSA", RDAJANSA);
                Cmd.Parameters.AddWithValue("@MECRA_FATURASI_MUSTERIYE", RDMUSTERIYE);
                Cmd.Parameters.AddWithValue("@UNVANI", MUSTERI_ADI);
                Cmd.Parameters.AddWithValue("@ULKE", ULKE);
                Cmd.Parameters.AddWithValue("@IL", IL);
                Cmd.Parameters.AddWithValue("@ILCE", ILCE);
                Cmd.Parameters.AddWithValue("@ADRESI", ADRES_BIR);
                Cmd.Parameters.AddWithValue("@VERGI_NO", VERGI_NO);
                Cmd.Parameters.AddWithValue("@VERGI_DAIRESI", VERGI_DAIRESI);
                Cmd.Parameters.AddWithValue("@TELEFON", TEL_NO_BIR);
                Cmd.Parameters.AddWithValue("@FAX", FAX_NO);
                Cmd.Parameters.AddWithValue("@TC_KIMLIK_NO", TC_KIMLIK_NO);
                Cmd.Parameters.AddWithValue("@HESAP_NO", HESAP_NO);
                Cmd.Parameters.AddWithValue("@IBAN", IBAN);
                Cmd.Parameters.AddWithValue("@EMAIL", EMAIL);
 



                foreach (SqlParameter parameter in Cmd.Parameters)
                {
                    if (parameter.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                }
                Cmd.Connection = conn;
                conn.Open();
                SqlDataReader myReader = Cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                while (myReader.Read())
                {
                    RETURN = myReader["ID"].ToString();
                }
                myReader.Close();
                Cmd.Connection.Close();

                return RETURN;


            }
        }

        [WebMethod(Description = "INSERT :KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,VERGI_NO,CARI_HESAP_KODU,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU")]
        public string MUSTERI_UPDATE(DateTime _TARIHI, string KODU, string UNVANI, string ADRESI, string TELEFON, string FAX, string VERGI_DAIRESI, string SAHIS_SIRKETI, string TC_KIMLIK_NO, string VERGI_NO, string CARI_HESAP_KODU, bool TV, bool GAZETE, bool DERGI, bool SINEMA, bool RADYO, bool OUTDOOR, bool INTERNET, bool SPONSORLUK, string MUHASEBE_HESAP_KODU)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" UPDATE  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,  SAHIS_SIRKETI , TC_KIMLIK_NO , VERGI_NO   ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@ADRESI", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TELEFON", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@FAX", FAX.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_DAIRESI", VERGI_DAIRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SAHIS_SIRKETI", SAHIS_SIRKETI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TC_KIMLIK_NO", TC_KIMLIK_NO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_NO", VERGI_NO.ToString()); 
                da.SelectCommand.Parameters.AddWithValue("@MUHASEBE_HESAP_KODU", MUHASEBE_HESAP_KODU.ToString());
                da.SelectCommand.Parameters.AddWithValue("@CARI_HESAP_KODU", CARI_HESAP_KODU.ToString());

                DataTable dt = new DataTable("GET_MUSTERI_UPDATE");
                da.Fill(dt);
                return KODU;
            }
        }

        [WebMethod(Description = "DELETE :FIRMA_KODU,MUSTERI_KODU")]
        public string MUSTERI_DELETE(  string FIRMA_KODU, string KODU )
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" UPDATE  set AKTIF_PASIF=false  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE KODU=@KODU and  FIRMA_KODU=@FIRMA_KODU ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@FIRMA_KODU", FIRMA_KODU.ToString());
              
                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        }

        [WebMethod(Description = "RETURN : MECRA_TURU, OLCULEN_OLCULMEYEN,  PAZARLAMA_SIRKETI_KODU, YEREL_ULUSAL, ANAYANIN_KODU, MECRA_KODU, ADI, UNVANI, PRIME_TIME_START, PRIME_TIME_END,   ADRESI, POSTA_KODU, IL, ILCE, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,  KODU, VERGIDAIRESI, VERGINO,  CARI_HESAP_KODU, MUHASEBE_HESAP_KODU")]
        public DataTable MUSTERI_LIST(string FIRMA_KODU)
        {

            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" SELECT   MECRA_TURU, OLCULEN_OLCULMEYEN,  PAZARLAMA_SIRKETI_KODU, YEREL_ULUSAL, ANAYANIN_KODU, MECRA_KODU, ADI, UNVANI, PRIME_TIME_START, PRIME_TIME_END,   ADRESI, POSTA_KODU, IL, ILCE, TELEFON, FAX, VERGI_DAIRESI, VERGI_NO,  KODU, VERGIDAIRESI, VERGINO,  CARI_HESAP_KODU, MUHASEBE_HESAP_KODU FROM [VISION].[dbo].[ADM_MECRA]  where  (ISTEK_TARIHI >= CONVERT(ISTEK_TARIHI, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(ISTEK_TARIHI, @BIT_TARIHI , 102)) ", myConnection);
        
                DataTable dt = new DataTable("GET_MECRA_LIST");
                da.Fill(dt);
                return dt;
            }

        }


        


        [WebMethod(Description = "INSERT : SIRKET_KODU,DIREKTOR,ADI,SOYADI,UNVANI,DEPARTMAN,KULLANICI_KODU,KULLANICI_GRUBU,ISE_GIRIS_TARIHI,CALISMA_SEKLI,KISISEL_EPOSTA,KISISEL_CEP_TEL,IS_CEPTEL,MAIL,NOTUM,ERP_USER,TIME_SHEET_KULLANICISI,AYRILDI")]
        public string KULLANICI_INSERT(int  REFID, string SIRKET_KODU, string DIREKTOR, string ADI, string SOYADI, string UNVANI, string DEPARTMAN, string  KODU, string GRUBU, string ISE_GIRIS_TARIHI, string CALISMA_SEKLI, string KISISEL_EPOSTA, string KISISEL_CEP_TEL, string IS_CEPTEL, string MAIL, string NOTUM, string ERP_USER, string TIME_SHEET_KULLANICISI, string AYRILDI)
        {
         

            string RETURN = "";  
            DateTime myDT = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
            {
                Guid gg = Guid.NewGuid(); 
                SqlCommand Cmd = new SqlCommand();  
                Cmd.CommandText = " INSERT INTO dbo.ADM_MUSTERI ( GUID,REF_ID, ISTEK_TARIHI, ISTEK_SAATI, ISTEKTE_BULUNAN, DIRECTOR, MEDYA_AJANS_MUSTERISI,CREATIVE_AJANS_MUSTERISI,TUZEL_SIRKET, SAHIS_SIRKET, YABANCI_SIRKET, YERLI_SIRKET, MECRA_FATURASI_AJANSA, MECRA_FATURASI_MUSTERIYE, SIRKET_KODU, UNVANI, ULKE, IL, ILCE, ADRESI, VERGI_DAIRESI, VERGI_NO, TC_KIMLIK_NO, TELEFON, FAX, HESAP_NO, IBAN,EMAIL, FATURA_TESLIM_ULKE, FATURA_TESLIM_IL, FATURA_TESLIM_ILCE, FATURA_TESLIM_ADRESI, FATURA_TESLIM_ALAN_I, FATURA_TESLIM_ALAN_II, FATURA_TESLIM_ALAN_I_TEL, FATURA_TESLIM_ALAN_II_TEL) " +
                                                             " Values( @GUID,@REF_ID,@ISTEK_TARIHI,@ISTEK_SAATI,@ISTEKTE_BULUNAN,@DIRECTOR,@MEDYA_AJANS_MUSTERISI,@CREATIVE_AJANS_MUSTERISI,@TUZEL_SIRKET,@SAHIS_SIRKET,@YABANCI_SIRKET,@YERLI_SIRKET,@MECRA_FATURASI_AJANSA,@MECRA_FATURASI_MUSTERIYE,@SIRKET_KODU,@UNVANI,@ULKE,@IL,@ILCE,@ADRESI,@VERGI_DAIRESI,@VERGI_NO,@TC_KIMLIK_NO,@TELEFON,@FAX,@HESAP_NO,@IBAN,@EMAIL,@FATURA_TESLIM_ULKE,@FATURA_TESLIM_IL,@FATURA_TESLIM_ILCE,@FATURA_TESLIM_ADRESI,@FATURA_TESLIM_ALAN_I,@FATURA_TESLIM_ALAN_II,@FATURA_TESLIM_ALAN_I_TEL,@FATURA_TESLIM_ALAN_II_TEL  ) SELECT @@IDENTITY AS ID   ";

                Cmd.Parameters.AddWithValue("@GUID", gg);
                Cmd.Parameters.AddWithValue("@REF_ID", REFID);
                Cmd.Parameters.AddWithValue("@DIRECTOR", DIREKTOR);
                Cmd.Parameters.AddWithValue("@ADI", ADI);
                Cmd.Parameters.AddWithValue("@SOYADI", SOYADI);
                Cmd.Parameters.AddWithValue("@UNVANI", UNVANI);
                Cmd.Parameters.AddWithValue("@DEPARTMANI", DEPARTMAN);
                Cmd.Parameters.AddWithValue("@KODU",  KODU);
                Cmd.Parameters.AddWithValue("@KULLANICI_GRUBU", GRUBU);  
                Cmd.Parameters.AddWithValue("@GIRIS_TARIHI", myDT.ToString("yyyy-MM-dd"));
                Cmd.Parameters.AddWithValue("@CALISMA_SEKLI", CALISMA_SEKLI);
                Cmd.Parameters.AddWithValue("@KISISEL_MAIL_ADRESI", KISISEL_EPOSTA);
                Cmd.Parameters.AddWithValue("@KISISEL_CEP_TEL", KISISEL_CEP_TEL);
                Cmd.Parameters.AddWithValue("@IS_CEPTEL", IS_CEPTEL);
                Cmd.Parameters.AddWithValue("@MAIL_ADRESI", MAIL);
                Cmd.Parameters.AddWithValue("@NOTUM", NOTUM);  
                Cmd.Parameters.AddWithValue("@ERP_USER", ERP_USER);
                Cmd.Parameters.AddWithValue("@TIMESHEET_KULLANICISI", TIME_SHEET_KULLANICISI);
                Cmd.Parameters.AddWithValue("@AKTIF", AYRILDI); 

                foreach (SqlParameter parameter in Cmd.Parameters)
                {
                    if (parameter.Value == null)
                    {
                        parameter.Value = DBNull.Value;
                    }
                }
                Cmd.Connection = conn;
                conn.Open();
                SqlDataReader myReader = Cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                while (myReader.Read())
                {
                    RETURN = myReader["ID"].ToString();
                }
                myReader.Close();
                Cmd.Connection.Close();

                return RETURN;


            }
        }

        [WebMethod(Description = "INSERT :KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,VERGI_NO,CARI_HESAP_KODU,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU")]
        public string KULLANICI_UPDATE(DateTime _TARIHI, string KODU, string UNVANI, string ADRESI, string TELEFON, string FAX, string VERGI_DAIRESI, string SAHIS_SIRKETI, string TC_KIMLIK_NO, string VERGI_NO, string CARI_HESAP_KODU, bool TV, bool GAZETE, bool DERGI, bool SINEMA, bool RADYO, bool OUTDOOR, bool INTERNET, bool SPONSORLUK, string MUHASEBE_HESAP_KODU)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" UPDATE  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,  SAHIS_SIRKETI , TC_KIMLIK_NO , VERGI_NO   ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@ADRESI", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TELEFON", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@FAX", FAX.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_DAIRESI", VERGI_DAIRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SAHIS_SIRKETI", SAHIS_SIRKETI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TC_KIMLIK_NO", TC_KIMLIK_NO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_NO", VERGI_NO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TV", TV.ToString());
                da.SelectCommand.Parameters.AddWithValue("@GAZETE", GAZETE.ToString());
                da.SelectCommand.Parameters.AddWithValue("@DERGI", DERGI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SINEMA", SINEMA.ToString());
                da.SelectCommand.Parameters.AddWithValue("@RADYO", RADYO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@OUTDOOR", OUTDOOR.ToString());
                da.SelectCommand.Parameters.AddWithValue("@INTERNET", INTERNET.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SPONSORLUK", SPONSORLUK.ToString());
                da.SelectCommand.Parameters.AddWithValue("@MUHASEBE_HESAP_KODU", MUHASEBE_HESAP_KODU.ToString());
                da.SelectCommand.Parameters.AddWithValue("@CARI_HESAP_KODU", CARI_HESAP_KODU.ToString());

                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        }

        [WebMethod(Description = "DELETE :KODU,UNVANI")]
        public string KULLANICI_DELETE(DateTime _TARIHI, string KODU, string UNVANI, string ADRESI, string TELEFON, string FAX, string VERGI_DAIRESI, string SAHIS_SIRKETI, string TC_KIMLIK_NO, string VERGI_NO, string CARI_HESAP_KODU, bool TV, bool GAZETE, bool DERGI, bool SINEMA, bool RADYO, bool OUTDOOR, bool INTERNET, bool SPONSORLUK, string MUHASEBE_HESAP_KODU)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" UPDATE  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,  SAHIS_SIRKETI , TC_KIMLIK_NO , VERGI_NO   ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString()); 
                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        }
         
        [WebMethod(Description = "INSERT :KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,VERGI_NO,CARI_HESAP_KODU,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU")]
        public string KULLANICI_LIST(DateTime _TARIHI, string KODU, string UNVANI, string ADRESI, string TELEFON, string FAX, string VERGI_DAIRESI, string SAHIS_SIRKETI, string TC_KIMLIK_NO, string VERGI_NO, string CARI_HESAP_KODU, bool TV, bool GAZETE, bool DERGI, bool SINEMA, bool RADYO, bool OUTDOOR, bool INTERNET, bool SPONSORLUK, string MUHASEBE_HESAP_KODU)
        {
            using (SqlConnection myConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["connectionstringVISION"].ToString()))
            {
                myConnection.Open();
                SqlDataAdapter da = new SqlDataAdapter(" SELECT  KODU,UNVANI,ADRESI,TELEFON,FAX,VERGI_DAIRESI,  SAHIS_SIRKETI , TC_KIMLIK_NO , VERGI_NO   ,TV, GAZETE, DERGI, SINEMA, RADYO, OUTDOOR, INTERNET, SPONSORLUK,MUHASEBE_HESAP_KODU,CARI_HESAP_KODU  FROM [VISION].[dbo].[ADM_PAZARLAMA_SIRKETI] WHERE LOGO_DURUMU='AÇILDI' and  MEDPLAN_DURUMU='AKTARILSIN'  AND KODU<>'' AND (ISTEK_TARIHI >= CONVERT(DATETIME, @BAS_TARIHI , 102)) AND (ISTEK_TARIHI <= CONVERT(DATETIME, @BIT_TARIHI , 102))  ", myConnection);

                da.SelectCommand.Parameters.AddWithValue("@KODU", KODU);
                da.SelectCommand.Parameters.AddWithValue("@UNVANI", UNVANI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@ADRESI", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TELEFON", ADRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@FAX", FAX.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_DAIRESI", VERGI_DAIRESI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SAHIS_SIRKETI", SAHIS_SIRKETI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TC_KIMLIK_NO", TC_KIMLIK_NO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@VERGI_NO", VERGI_NO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@TV", TV.ToString());
                da.SelectCommand.Parameters.AddWithValue("@GAZETE", GAZETE.ToString());
                da.SelectCommand.Parameters.AddWithValue("@DERGI", DERGI.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SINEMA", SINEMA.ToString());
                da.SelectCommand.Parameters.AddWithValue("@RADYO", RADYO.ToString());
                da.SelectCommand.Parameters.AddWithValue("@OUTDOOR", OUTDOOR.ToString());
                da.SelectCommand.Parameters.AddWithValue("@INTERNET", INTERNET.ToString());
                da.SelectCommand.Parameters.AddWithValue("@SPONSORLUK", SPONSORLUK.ToString());
                da.SelectCommand.Parameters.AddWithValue("@MUHASEBE_HESAP_KODU", MUHASEBE_HESAP_KODU.ToString());
                da.SelectCommand.Parameters.AddWithValue("@CARI_HESAP_KODU", CARI_HESAP_KODU.ToString());

                DataTable dt = new DataTable("GET_PAZARLAMA_SIRKETI_LIST");
                da.Fill(dt);
                return KODU;
            }
        }



    }
}
