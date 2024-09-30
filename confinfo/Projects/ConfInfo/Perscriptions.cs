using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using vapi = VMS.TPS.Common.Model.API;
using vtype = VMS.TPS.Common.Model.Types;

namespace ConfInfo
{
    class Perscriptions
    {
        

        public static List<string> GetDoseLevels(vapi.Patient pat ,vapi.PlanSetup plan)
        {
            long patientSer = -1;
            long courseSer = -1;
            long planSetupSer = -1;
            //long prescSer = -1;
            GetPlanSetupSer(pat.Id,plan.Course.Id,plan.Id, out patientSer, out courseSer, out planSetupSer );
             
            DataTable prescription = AriaInterface.Query("select distinct PlanSetupSer, PlanSetup.PrescriptionSer, PrescriptionAnatomy.PrescriptionSer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomyItem.PrescriptionAnatomySer, ItemType, ItemValue, Prescription.Status, Prescription.PrescriptionSer, Prescription.PrescriptionName, Prescription.Notes from PlanSetup, Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PlanSetup.PlanSetupSer = " + planSetupSer.ToString() + " and PlanSetup.PrescriptionSer = PrescriptionAnatomy.PrescriptionSer and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomyItem.ItemType = 'VOLUME ID' and PlanSetup.PrescriptionSer = Prescription.PrescriptionSer");

            //AutoCheckStatus r4_status = AutoCheckStatus.UNKNOWN;
            string r4_value = string.Empty;
        string r4_value_detailed = string.Empty;

        //GetPlanSetupSer(pat.Id, plan.Course.Id, plan.Id, out patientSer, out courseSer, out planSetupSer);
        //string prescriptionString = GetPrescription(planSetupSer); 

        List<int> numberOfFractions = new List<int>();
        List<string> volumeNames = new List<string>(); 
        List<double> dosePerFraction = new List<double>();
        List<string> totalDose = new List<string>();

            switch (prescription.Rows.Count)
            {
                case 0:
                    //r4_status = AutoCheckStatus.WARNING;
                    r4_value = "Ordination: Saknas";
                    break;
                default:
                    foreach (DataRow row in prescription.Rows)
                    {
                        string volumeName = (string)row[6];
                        volumeNames.Add(volumeName); 
                        long ser = (long)row[3];
                        DataTable prescriptionItem = AriaInterface.Query("select NumberOfFractions, ItemType, ItemValue, PrescriptionAnatomyItem.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionAnatomySer, PrescriptionAnatomy.PrescriptionSer, Prescription.PrescriptionSer  from Prescription, PrescriptionAnatomy, PrescriptionAnatomyItem where PrescriptionAnatomy.PrescriptionAnatomySer = " + ser.ToString() + " and PrescriptionAnatomy.PrescriptionAnatomySer = PrescriptionAnatomyItem.PrescriptionAnatomySer and PrescriptionAnatomy.PrescriptionSer = Prescription.PrescriptionSer");
                        double tdose = -1, dosepf = -1;

                        foreach (DataRow itemRow in prescriptionItem.Rows)
                        {
                            numberOfFractions.Add((int)prescriptionItem.Rows[0]["NumberOfFractions"]);
                            if (String.Equals((string)itemRow["ItemType"], "Total dose", StringComparison.OrdinalIgnoreCase))
                            {
                                double.TryParse((string)itemRow["ItemValue"], out tdose);
                                totalDose.Add(tdose.ToString());
                            }
                            if (String.Equals((string)itemRow["ItemType"], "Dose per fraction", StringComparison.OrdinalIgnoreCase))
                            {
                                double.TryParse((string)itemRow["ItemValue"], out dosepf);
                                dosePerFraction.Add(dosepf);
                            }
                        }
                        if (tdose > 0 && dosepf > 0)
                            r4_value_detailed += (r4_value_detailed == string.Empty? "Ordination: \r\n" : "\r\n") + "  • Volym: " + volumeName + "\r\n  • Fraktionsdos: " + dosepf.ToString("0.000") + " Gy \r\n  • Antal fraktioner: " + numberOfFractions.LastOrDefault().ToString() + "\r\n  • Totaldos: " + tdose.ToString("0.000") + " Gy\r\n";
                    }

                    
                    break;
            }
            return totalDose.Distinct().ToList(); 


    }

        internal static vtype.DoseValue GetDoseLevelstotalPrescribedDose(vapi.Patient pat, vapi.PlanSetup plan)
        {
            double totalPrescribedDose; 
            List<string> perscribedDoseLevels = Perscriptions.GetDoseLevels(pat, plan);
            if (perscribedDoseLevels.Count() > 1)
            {
                SelectionDialog selectLevel = new SelectionDialog(perscribedDoseLevels, "Multiple doselevels, select");
                selectLevel.ShowDialog();
                if (!(bool)selectLevel.DialogResult)
                {
                    // SHUTDOWN: was canceled by user.
                    throw new System.InvalidOperationException("Canceled by user.");
                }

                totalPrescribedDose = Convert.ToDouble(perscribedDoseLevels.First(x => x.Equals(selectLevel.listDisplay.SelectedItem)));
            }
            else if (perscribedDoseLevels.Count() == 1)
            {
                totalPrescribedDose = Convert.ToDouble(perscribedDoseLevels.First());

            }
            else
                totalPrescribedDose = plan.TotalDose.Dose;
            return new vtype.DoseValue(totalPrescribedDose,"Gy"); 
        }

        internal static List<double> GetDoseLevelsDoubleList(vapi.Patient pat, vapi.PlanSetup plan)
        {

            throw new NotImplementedException();


        }

        public static void GetPlanSetupSer(string patientId, string courseId, string planSetupId, out long patientSer, out long courseSer, out long planSetupSer)
        {
            patientSer = -1;
            courseSer = -1;
            planSetupSer = -1;

            DataTable dataTableSerials = AriaInterface.Query("select Patient.PatientSer,Course.CourseSer,PlanSetup.PlanSetupSer from Patient,Course,PlanSetup where PatientId='" + patientId + "' and CourseId='" + courseId + "' and PlanSetupId='" + planSetupId + "' and Course.PatientSer=Patient.PatientSer and PlanSetup.CourseSer=Course.CourseSer");
            if (dataTableSerials.Rows.Count == 1)
            {
                patientSer = (long)dataTableSerials.Rows[0][0];
                courseSer = (long)dataTableSerials.Rows[0][1];
                planSetupSer = (long)dataTableSerials.Rows[0][2];
            }
        }
        public long CheckPrescription(long planSetupSer)
        {
            // Rather than checking if a prescription exists this method now returns the prescritption serial.
            long prescSer;
            DataTable prescription = AriaInterface.Query("select PrescriptionSer from PlanSetup where PlanSetupSer=" + planSetupSer.ToString() + " and PrescriptionSer is not null");
            if (prescription.Rows.Count > 0)
                prescSer = (long)prescription.Rows[0]["PrescriptionSer"];
            else
                prescSer = -1;

            return prescSer;
        }
    }
}
