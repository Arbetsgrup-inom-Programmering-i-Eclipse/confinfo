using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms; 
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vapi = VMS.TPS.Common.Model.API;
using vtype = VMS.TPS.Common.Model.Types;
using ConfInfo;


namespace SlingShot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (vapi.Application app = vapi.Application.CreateApplication())
            {
                vapi.Patient pat = app.OpenPatientById("test_ScriptEQD2_001"); //QC_Checklista

                //vapi.ExternalPlanSetup plan = pat.Courses.First().ExternalPlanSetups.First();
                
                IEnumerable<vapi.PlanSetup> plans = pat.Courses.SelectMany(p => p.PlanSetups);

                //CentralControl cc = new CentralControl(pat, plans, app.CurrentUser);//, this); Change to this when running i eclipse 
                CentralControl cc = new CentralControl(pat, plans, app.CurrentUser, this); //Add window here for running outside eclipse. 
                this.ShowDialog();
            }
        }
    }
}