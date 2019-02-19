using System;
using System.Web;
using Wisej.Web;
using System.DirectoryServices;
using System.Data.SqlClient;
using System.Data;
namespace Trnet
{
    public partial class _LOGIN : Form
    {
        public _LOGIN()
        {
            InitializeComponent();
      
        }
        private void _LOGIN_Load(object sender, EventArgs e)
        {
            //string current = HttpContext.Current.User.Identity.Name;
            //TXT_KULLANICI_ADI.Text = current.Substring(current.IndexOf('\\') + 1);

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            bool valid = true;
            this.errorProvider.SetError(this.TXT_KULLANICI_ADI, null);
            this.errorProvider.SetError(this.TXT_SIFRE, null);
            if (String.IsNullOrEmpty(this.TXT_KULLANICI_ADI.Text))
            {
                valid = false;
                this.errorProvider.SetError(this.TXT_KULLANICI_ADI, "Enter your user name.");
            }
            if (String.IsNullOrEmpty(this.TXT_SIFRE.Text))
            {
                valid = false;
                this.errorProvider.SetError(this.TXT_SIFRE, "Enter a valid password.");
            }
            if (valid)
            {
                //  LOGIN();

               LOGIN_LOCAL();
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LOGIN()
        {
            if (TXT_KULLANICI_ADI.Text == null || TXT_KULLANICI_ADI.Text == "") { AlertBox.Show("<br> Kullanıcı Adı Giriniz! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; }
            if (TXT_SIFRE.Text == null || TXT_SIFRE.Text == "") { AlertBox.Show("<br> Şifre Giriniz! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; }

            try
            {
                DirectoryEntry de = GetDirectoryObject(TXT_KULLANICI_ADI.Text.Trim(), TXT_SIFRE.Text.Trim());
                DirectorySearcher deSearch = new DirectorySearcher() { SearchRoot = de, Filter = string.Format("(&(objectClass=user)(SAMAccountName={0}))", TXT_KULLANICI_ADI.Text.Trim()) };
                deSearch.PropertiesToLoad.Add("mail");
                deSearch.PropertiesToLoad.Add("userPrincipalName");
                SortOption Srt = new SortOption("mail", System.DirectoryServices.SortDirection.Ascending);
                deSearch.Sort = Srt;
                //Sonuçları bir değişkene atalım.
                var test = deSearch.FindAll();
                SearchResultCollection Results = deSearch.FindAll();
                if (Results != null)
                {
                    foreach (SearchResult Result in Results)
                    {
                        ResultPropertyCollection Rpc = Result.Properties;
                        Application.Session._KULLANICI_MAIL_ADRESI = Rpc["userPrincipalName"][0].ToString(); 
                    }

                    if (Application.Session._KULLANICI_MAIL_ADRESI == null) { AlertBox.Show("<br> Bilgiler Doğrulanamadı! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; };

                    using (SqlConnection SqlCon = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
                    {
                        SqlCon.Open();
                        using (SqlCommand Cmd = new SqlCommand("SELECT top 1 * FROM  ADM_KULLANICI  WHERE MAIL_ADRESI=@MAIL_ADRESI and AKTIF='True' ", SqlCon))
                        {
                            Cmd.Parameters.AddWithValue("@MAIL_ADRESI", Application.Session._KULLANICI_MAIL_ADRESI);
                            SqlDataReader rdr = Cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                Application.Session._KULLANICI_ID = rdr["ID"].ToString();
                                Application.Session._KULLANICI_ADI_SOYADI = rdr["ADI"].ToString()+" "+ rdr["SOYADI"].ToString();
                                Application.Session._SIRKET_KODU = rdr["SIRKET_KODU"].ToString();
                                Application.Session._DEPARTMANI = rdr["DEPARTMANI"].ToString();
                                Application.Session._GOREVI = rdr["GOREVI"].ToString();
                                Application.Session._UNVANI = rdr["UNVANI"].ToString();
                                Application.Session._KULLANICI_TURU = rdr["UNVANI"].ToString();
                                Application.Session._ISE_GIRIS_TARIHI = rdr["GIRIS_TARIHI"].ToString();
                            }
                            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
                            {
                                DateTime RUN_DATE = DateTime.Now;
                                string SQL = " INSERT INTO [dbo].[XLG_HAREKET_KAYITLARI] (SIRKET_KODU,ISLEM_TARIHI,ISLEM_SAATI,YAPILAN_ISLEM,ISLEMI_YAPAN) VALUES ('" + Application.Session._SIRKET_KODU + "','" + RUN_DATE.ToString("yyyy.MM.dd") + "','" + RUN_DATE.ToString("HH:mm:ss") + "','GİRİŞ','" + Application.Session._KULLANICI_MAIL_ADRESI + "')";
                                SqlCommand command = new SqlCommand(SQL, conn);
                                conn.Open();
                                command.CommandTimeout = 0;
                                command.ExecuteReader(CommandBehavior.CloseConnection);
                                conn.Close();
                            }
                            rdr.Close();
                        }
                    }
                    if (Application.Session._SIRKET_KODU != null) { this.Close(); } else { { AlertBox.Show("<br> Bilgiler Doğrulanamadı! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; } }
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                if (ex.Data == null)
                {
                    throw;
                }
                else
                {
                    AlertBox.Show("<br> Bilgiler Doğrulanamadı! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 2000);
                }
            }
        }



        private void LOGIN_LOCAL()
        {
            if (TXT_KULLANICI_ADI.Text == null || TXT_KULLANICI_ADI.Text == "") { AlertBox.Show("<br> Kullanıcı Adı Giriniz! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; }
            if (TXT_SIFRE.Text == null || TXT_SIFRE.Text == "") { AlertBox.Show("<br> Şifre Giriniz! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; }

                    using (SqlConnection SqlCon = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
                    {
                        SqlCon.Open();
                        using (SqlCommand Cmd = new SqlCommand("SELECT top 1 * FROM  ADM_KULLANICI  WHERE KODU=@KODU and AKTIF='True' ", SqlCon))
                        {
                            Cmd.Parameters.AddWithValue("@KODU", TXT_KULLANICI_ADI.Text);
                            SqlDataReader rdr = Cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                Application.Session._KULLANICI_ID = rdr["ID"].ToString();
                                Application.Session._KULLANICI_MAIL_ADRESI = rdr["MAIL_ADRESI"].ToString();
                                Application.Session._KULLANICI_ADI_SOYADI = rdr["ADI"].ToString() + " " + rdr["SOYADI"].ToString();
                                Application.Session._SIRKET_KODU = rdr["SIRKET_KODU"].ToString();
                                Application.Session._DEPARTMANI = rdr["DEPARTMANI"].ToString();
                                Application.Session._GOREVI = rdr["GOREVI"].ToString();
                                Application.Session._UNVANI = rdr["UNVANI"].ToString();
                                Application.Session._KULLANICI_TURU = rdr["UNVANI"].ToString();
                                Application.Session._ISE_GIRIS_TARIHI = rdr["GIRIS_TARIHI"].ToString();
                            }
                            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Connection_VISION"].ToString()))
                            {
                                DateTime RUN_DATE = DateTime.Now;
                                string SQL = " INSERT INTO [dbo].[XLG_HAREKET_KAYITLARI] (SIRKET_KODU,ISLEM_TARIHI,ISLEM_SAATI,YAPILAN_ISLEM,ISLEMI_YAPAN) VALUES ('" + Application.Session._SIRKET_KODU + "','" + RUN_DATE.ToString("yyyy.MM.dd") + "','" + RUN_DATE.ToString("HH:mm:ss") + "','GİRİŞ','" + Application.Session._KULLANICI_MAIL_ADRESI + "')";
                                SqlCommand command = new SqlCommand(SQL, conn);
                                conn.Open();
                                command.CommandTimeout = 0;
                                command.ExecuteReader(CommandBehavior.CloseConnection);
                                conn.Close();
                            }
                            rdr.Close();
                        }
                    }
                    if (Application.Session._SIRKET_KODU != null) { this.Close(); } else { { AlertBox.Show("<br> Bilgiler Doğrulanamadı! </b> ", MessageBoxIcon.Stop, alignment: System.Drawing.ContentAlignment.MiddleCenter, autoCloseDelay: 1000); return; } }
               
        
        }


        private DirectoryEntry GetDirectoryObject(string USERNAME, string PASSWO)
        {
            DirectoryEntry oDE = new DirectoryEntry("LDAP://10.219.168.51", string.Format("{0}", USERNAME), string.Format("{0}", PASSWO), AuthenticationTypes.Secure);  //// LDAP://ISTADCP01101
            return oDE;
        }

   
    }
}
