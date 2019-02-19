using System;
using Wisej.Web;


namespace Trnet
{
    public partial class _MASTER : Page
    {
        _LOGIN PN = new _LOGIN();
        //TALEPLER.TALEPLER_YONETIMI TLP = new TALEPLER.TALEPLER_YONETIMI();

        public _MASTER()
        {
            InitializeComponent();

      



       
            //PN.MdiParent = this;
            PN.MaximizeBox = false;
            PN.MinimizeBox = false;
            PN.ControlBox = false;
            PN.MdiTabProperties.ShowThumbnails = false;

            //  PN.FormClosed += new Wisej.Web.FormClosedEventHandler(this._Login_FormClosed);
            PN.FormClosing += new Wisej.Web.FormClosingEventHandler(this._Login_FormClosing);
            PN.Show();




        }

        private void _Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  if (PN != null) PN.Show();

           
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
            //  view.Parent = this.workspacePanel;

            TABCONTROLS.TabPages.Add("Giriş");
            TABCONTROLS.TabPages[TABCONTROLS.TabPages.Count - 1].Controls.Add(view);


            //panels.Controls.Clear(); 
            //panels.Controls.Add(view);
            //sp.Dock = DockStyle.Fill;
            //sp.Show();


            view.Dock = DockStyle.Fill;
            //view.Disposed += (s, e) => { this.lefT_MENU_STANDARTS.UpdateState(); };
            //this.lefT_MENU_STANDARTS.UpdateState();


 


        }


        private void BTN_PAZARLAMA_SIRKETI_Click(object sender, EventArgs e)
        {

        }

        private void BTN_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button.Tag != null)
            { 
                string viewName = "", TabviewName = "";
                viewName = button.Tag.ToString(); TabviewName = button.Text.ToString();  
                if (viewName != "")
                {
                    int TAB_CONTROL = 0;
                    int TabCount = 0;
                    for (int i = 0; i < TABCONTROLS.TabPages.Count; i++)
                    {
                        if (TABCONTROLS.TabPages[i].Text == TabviewName)
                        {
                            TAB_CONTROL = 1; TabCount = i;
                            TABCONTROLS.SelectedIndex = i;
                            break;
                        }
                    }
                    if (TAB_CONTROL == 0)
                    {
                        //Control view = (Control)Application.FindComponent(o => o is Control && ((Control)o).Name == viewName);
                        Type type = Type.GetType("Trnet." + viewName, true);
                        Control view = (Control)Activator.CreateInstance(type,0,"ADD");
                        //  view.Parent = this.workspacePanel; 
                        TABCONTROLS.TabPages.Add(TabviewName);
                        int TABCOUNT = TABCONTROLS.TabPages.Count - 1;
                        TABCONTROLS.TabPages[TABCOUNT].Controls.Add(view);
                        TABCONTROLS.TabPages[TABCOUNT].ShowCloseButton = true;
                        TABCONTROLS.TabPages[TABCOUNT].Dock = DockStyle.Fill;
                        TABCONTROLS.SelectedIndex = TABCOUNT;
                        view.Dock = DockStyle.Fill;
                    }
                }
            }
        }

        private void BTN_MUSTERI_TALEBI_MouseHover(object sender, EventArgs e)
        {
          //  MessageBox.Show("Medplan İçin Müşteri Talebi");
        }
    }
}
