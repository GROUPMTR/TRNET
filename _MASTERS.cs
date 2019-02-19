using System;
using Wisej.Web;
using Wisej.Web.Ext.RibbonBar;

namespace Trnet
{
    public partial class _MASTERS : Page
    {
        public _MASTERS()
        {
            InitializeComponent();
           

            LBL_KULLANCI_MAIL.Text = Application.Session._KULLANICI_MAIL_ADRESI;
            LBL_DEPARTMAN.Text = Application.Session._DEPARTMANI;
            LBL_KULLANICI_TURU.Text = Application.Session._KULLANICI_TURU;
            LBL_SIRKET_KODU.Text = Application.Session._SIRKET_KODU;
            LBL_GOREVI.Text = Application.Session._GOREVI;
            LBL_UNVANI.Text = Application.Session._UNVANI; 

            string viewName = "DASHBOARD.DASBOARD";
            Control view = (Control)Application.FindComponent(o => o is Control && ((Control)o).Name == viewName); 
            Type type = Type.GetType("Trnet." + viewName, true);
            view = (Control)Activator.CreateInstance(type); 
            userControls.Controls.Add(view);
           //userControl1.TabPages[TABCONTROLS.TabPages.Count - 1].Controls.Add(view);

 


            view.Dock = DockStyle.Fill;


            //ribbonBar.Height = 100;
        }


        private void ribbonBar_ItemClick(object sender, RibbonBarItemEventArgs e)
        {

            var button = e.Item;

            userControls.Controls.Clear(); 


            string viewName = "FORMLAR.PAZARLAMA_STI_TALEP";
            //Control view = (Control)Application.FindComponent(o => o is Control && ((Control)o).Name == viewName); 
            Type type = Type.GetType("Trnet." + viewName, true);
            //view = (Control)Activator.CreateInstance(type);
            Control view = (Control)Activator.CreateInstance(type, 0, "ADD"); 
            userControls.Controls.Add(view);
            view.Dock = DockStyle.Fill;
            //ribbonBar.Height = 100;
        }
        private void RibbonBar_MenuButtonItemClick(object sender, RibbonBarMenuItemEventArgs e)
        {
            var button = e.Item;
            AlertBox.Show(button.ToString() + " => " + e.Item.ToString());
        }

        private void Bilgilerim_ItemClicked(object sender, RibbonBarMenuItemEventArgs e)
        {
            var button = e.Item;
            AlertBox.Show(button.ToString() + " => " + e.Item.ToString());
        }

        private void vButtons1_MouseHover(object sender, EventArgs e)
        {
            BTN_BIGILERIM.BackColor = System.Drawing.Color.FromName("@toolbar");
            BTN_BIGILERIM.Refresh();
        }

        private void vButtons1_MouseLeave(object sender, EventArgs e)
        {
       
            BTN_BIGILERIM.BackColor = System.Drawing.Color.FromName("White");
            BTN_BIGILERIM.Refresh();
        }

        private void vButtons1_Click(object sender, EventArgs e)
        { 
            var button = sender as Control; 
            userControls.Controls.Clear();
            string viewName = ""; 
            if (button.Name.ToString() == "BTN_BIGILERIM")     { viewName = "KULLANICI.KULLANICI_BILGILERI";   }  
            if (button.Name.ToString() == "BTN_PAZARLAMA_STI_GIRIS")   {   viewName = "FORMLAR.PAZARLAMA_STI_TALEP_FORMU";  }  
            if (button.Name.ToString() == "BTN_MUSTERI_FORMUS")  {   viewName = "FORMLAR.MUSTERI_FORMUS";   } 
            if (button.Name.ToString() == "BTN_TEDARIKCI_TALEP_FORMU")    {  viewName = "FORMLAR.TEDARIKCI_TALEP_FORMUS"; }  
            if (button.Name.ToString() == "BTN_TALEPLERIM")   {  viewName = "TALEPLER.TALEPLER_YONETIMI"; }  

            if (button.Name != null)
            {
                Type type = Type.GetType("Trnet." + viewName, true); 
                Control view = (Control)Activator.CreateInstance(type, 0, "ADD");
                view.Dock = DockStyle.Fill;
                userControls.Controls.Add(view);
                userControls.Refresh();
                view.Visible = true;
            }
        }
    }
}
