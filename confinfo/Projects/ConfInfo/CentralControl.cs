using System;
using System.Data; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using vapi = VMS.TPS.Common.Model.API;
using vtype = VMS.TPS.Common.Model.Types;

namespace ConfInfo
{
    public class CentralControl
    {
        vapi.StructureSet sSet;
        //vtype.DoseValue doseLevel; 
        List<string> roiNames;
        public double totalPrescribedDose;
        //private long patientSer = -1;
        //private long courseSer = -1;
        //private long planSetupSer = -1;
        //private long prescSer = -1;
        //dont do this
        //public Window MainWin;
        private static string logFile = @"\\mtfs003\VA_Data4$\ProgramData\Vision\PublishedScripts\ConfInfo.txt";
        //public CentralControl(vapi.Patient pat, IEnumerable<vapi.PlanSetup> plans, vapi.User currentUser)//, Window window)
        public CentralControl(vapi.Patient pat, IEnumerable<vapi.PlanSetup> plans, vapi.User currentUser, Window window)
        {
            //Removes stupid plans: 
            string cu = currentUser.Id.Substring(4);
            if (cu == "105231" || cu == "170483" || cu == "165802" || cu == "143919" || cu == "184756" || cu == "153012") //Elinore vill vara speciell tillsammans med Anneli och Malin och per och Annika
            { plans = plans.Where(x => (!x.Id.ToLower().StartsWith("upp") && !x.Id.ToLower().StartsWith("qc") || x.Beams.Count() == 0) && x.IsDoseValid == true); }
            else
            { plans = plans.Where(x => (x.Id.ToLower().StartsWith("p") || (x.Id.ToLower().StartsWith("u") && !x.Id.ToLower().StartsWith("upp")) || x.Beams.Count() == 0) && x.IsDoseValid == true); }
            

        bool addStruct = true;
            // Creates a final resultTable and a tmpTable. 
            DataTable resultTable = new DataTable();
            DataTable tmpTab = new DataTable();
            AriaInterface.Connect();
            while (addStruct)
            {
                // Returns user selected plan if course contains multiple plans.
                vapi.PlanSetup plan = PlanSelection(plans);
                //List<vapi.PlanningItem> plansInSum = new List<vapi.PlanningItem>();
                sSet = plan.StructureSet;
                //GetPlanSetupSer(pat.Id, plan.Course.Id, plan.Id, out patientSer, out courseSer, out planSetupSer);
                //string prescriptionString = GetPrescription(planSetupSer);
                vtype.DoseValue doseLevel = Perscriptions.GetDoseLevelstotalPrescribedDose(pat,plan);
                
                
                //Convert the user selected doselevel to a DoseValue Type, which is further used below. 
                

                
                
            // Contains the names of all structures in the set that are not empty.
            // Is used in the ComboBox of the GUI for manual selection of structure names.
            roiNames = new List<string>();
            foreach (vapi.Structure roi in plan.StructureSet.Structures)
            {
                if (!roi.IsEmpty)
                    roiNames.Add(roi.Id.ToString());
            }

            if (roiNames.Count() == 0)// ERROR -> SHUTDOWN: No structures drawn..
                throw new System.InvalidOperationException("No structures drawn in the set linked to the plan.");
            

            IEnumerable<vapi.Structure> ptvVols = plan.StructureSet.Structures.Where(s => s.Id.ToLower().Contains("ptv") && !s.Id.ToLower().Contains("z_")).ToList();
            IEnumerable<vapi.Structure> bodyVols = plan.StructureSet.Structures.Where(s => s.DicomType.ToLower().StartsWith("external")).ToList();

            if (ptvVols.Count() < 1)
                throw new System.InvalidOperationException("No PTV Found, please define a PTV");

            vapi.Structure selPtv = StructSelection(ptvVols);
            vapi.Structure plannedPtv = ptvVols.Where(s => s.Id == plan.TargetVolumeID).FirstOrDefault(); 
            vapi.Structure selBody = StructSelection(bodyVols);





                //Add DataTable for all indexes.    

                tmpTab = Indexes.GetAllIndexs(selPtv, selBody, plan, doseLevel, 0.95);
                // merge current results if multiple runs are made. 
                resultTable.Merge(tmpTab);


                Window1 resWin = new Window1();
                 


                resWin.dataGrid.ItemsSource = resultTable.DefaultView;
                
                //Generate identical parameters for logging of data to logfile
               
                double CI = Indexes.ConformityIndex(selPtv, selBody, plan, doseLevel, 0.95);
                double PCI = Indexes.PaddickConformityIndex(selPtv, selBody, plan, doseLevel, 0.95);
                //double CN = Indexes.ConformationNumber(selPtv, selBody, plan, doseLevel); // redacted as it is the same result as PCI
                double HI = Indexes.HomoginetyIndex(selPtv, selBody, plan, doseLevel);

                PrimitiveLog(currentUser.Id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),  pat.Id, plan, selPtv.Id,doseLevel.Dose ,CI, PCI, PCI, HI);

                resWin.ResizeMode = ResizeMode.NoResize;
                resWin.SizeToContent = SizeToContent.WidthAndHeight;
                resWin.Title = "Conformitiy Index";
                resWin.ShowDialog();
                
                
                if ((bool)resWin.DialogResult)
                    resWin.Hide();
                else
                {
                    addStruct = false;
                    resWin.Close(); 
                }
                
            }
            //window.Show();

            //window.Close();
            AriaInterface.Disconnect();
            


        }

        private static vapi.PlanSetup PlanSelection(IEnumerable<vapi.PlanSetup> pSums)
        {
            // Selection of plan sum, if multiple available.
            if (pSums.Count() > 1)
            {
                List<string> pSumNames = new List<string>();

                foreach (vapi.PlanSetup pSumTemp in pSums)
                {
                    pSumNames.Add(pSumTemp.Id);
                }

                SelectionDialog selectPlanSum = new SelectionDialog(pSumNames, "Select Plan");
                selectPlanSum.ShowDialog();

                if (!(bool)selectPlanSum.DialogResult)
                {
                    // SHUTDOWN: was canceled by user.
                    throw new System.InvalidOperationException("Canceled by user.");
                }

                return pSums.First(x => x.Id.Equals(selectPlanSum.listDisplay.SelectedItem));
            }
            else if (pSums.Count() == 1)
            {
                return pSums.First();
            }
            else
            {
                // ERROR -> SHUTDOWN: no plan sum in scope / course.
                //MessageBox.Show("No plan sum found in the current course.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //System.Environment.Exit(0);
                throw new System.InvalidOperationException("No plan found in the current course.");
                //return null;
            }
        }

        private static vapi.Structure StructSelection(IEnumerable<vapi.Structure> structs)
        {
            bool ptvflag = false;
            string dispString; 
            // Selection of plan sum, if multiple available.
            if (structs.Count() > 1)
            {
                List<string> structsNames = new List<string>();

                foreach (vapi.Structure s in structs)
                {
                    structsNames.Add(s.Id.ToString());
                    if (s.Id.ToLower().Contains("ptv"))
                    {
                        if (!ptvflag)
                        ptvflag = true;
                    }
                }
                // Trigger change of name in case there are mutiple body structures in the structure set. 
                if (ptvflag)
                    dispString = "Select PTV"; 
                else
                    dispString = "Select Body";

                SelectionDialog selectStruct = new SelectionDialog(structsNames, dispString);
                selectStruct.ShowDialog();

                if (!(bool)selectStruct.DialogResult)
                {
                    // SHUTDOWN: was canceled by user.
                    throw new System.InvalidOperationException("Canceled by user.");
                }

                return structs.First(x => x.Id.Equals(selectStruct.listDisplay.SelectedItem));
            }
            else if (structs.Count() == 1)
            {
                return structs.First();
            }
            else
            {
                // ERROR -> SHUTDOWN: no structures in scope / course.
                
                throw new System.InvalidOperationException("No structures found in the current course.");
                //return null;
            }
        }
                

        private void PrimitiveLog(string time, string userId, string patId, vapi.PlanningItem plan,string ptvName, double doseLevel, double CI, double PCI, double CN, double HI)
        {
            string logString = String.Empty;
            logString += time + "\t" + userId + "\t" + patId + "\t" + plan.Id + "\t" + ptvName + "\t" + doseLevel.ToString("F2") + "\t" + CI + "\t" + PCI + "\t" + CN + "\t" + HI;
               

            using (System.IO.StreamWriter file =
                System.IO.File.AppendText(logFile))
            {
                file.WriteLine(logString);
            }
        }
                
        
    }
    
}