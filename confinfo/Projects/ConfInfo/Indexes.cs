using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Data;
using vapi = VMS.TPS.Common.Model.API;
using vtype = VMS.TPS.Common.Model.Types;
using System.ComponentModel;
using ConfInfo; 
/// <summary>
///  Indexes taken from: https://en.wikibooks.org/wiki/Radiation_Oncology/Stereotactic_radiosurgery 
/// </summary>
namespace ConfInfo
{
    public class Indexes
        
    {
        public string PlanId { get; }
        public string StructureName { get; }
        public double CI { get; }
        public double CN { get; }
        public double HI { get; }
        
        public static DataTable GetAllIndexs(vapi.Structure strIn, vapi.Structure bodyIn, vapi.PlanSetup planIn, vtype.DoseValue perscDose, double isodoseLvl)
        {
            DataTable datOut = new DataTable();

            datOut.Columns.Add("Plan Name", typeof(string));
            datOut.Columns.Add("Structure Name", typeof(string));
            datOut.Columns.Add("Dose Level", typeof(string));
            datOut.Columns.Add("Conformity Index, V" +(isodoseLvl*100).ToString("N0") + "%", typeof(double));
            datOut.Columns.Add("Paddic Conformity Index, V" + (isodoseLvl * 100).ToString("N0") + "%", typeof(double));
            datOut.Columns.Add("Homogeneity Index", typeof(double));
            //datOut.Columns.Add("Conformation Number", typeof(double)); Redacted as it is the same as PCI


            string PlanId = planIn.Id;
            string StructureName = strIn.Id;
            double CI = ConformityIndex(strIn,bodyIn, planIn, perscDose, isodoseLvl);
            double PCI = PaddickConformityIndex(strIn, bodyIn, planIn, perscDose, isodoseLvl);
            double HI = HomoginetyIndex(strIn,bodyIn, planIn, perscDose);
            datOut.Rows.Add(planIn.Id, strIn.Id, perscDose.Dose.ToString("F2"), CI, PCI, HI);
            //double CN = ConformationNumber(strIn,bodyIn, planIn, perscDose); Redacted as it is the same as PCI 


            return datOut; 

        }
        public static double HomoginetyIndex(vapi.Structure strIn, vapi.Structure bodyIn, vapi.PlanSetup planIn, vtype.DoseValue perscDose)
        {

            double d02 = Convert.ToDouble(planIn.GetDoseAtVolume(strIn, 2, vtype.VolumePresentation.Relative, vtype.DoseValuePresentation.Relative).ValueAsString);
            double d98 = Convert.ToDouble(planIn.GetDoseAtVolume(strIn, 98, vtype.VolumePresentation.Relative, vtype.DoseValuePresentation.Relative).ValueAsString);
            double d50 = Convert.ToDouble(planIn.GetDoseAtVolume(strIn, 50, vtype.VolumePresentation.Relative, vtype.DoseValuePresentation.Relative).ValueAsString);

            return Math.Round((d02 - d98) / d50 * 100, 3);
        }
        public static double ConformityIndex(vapi.Structure strIn, vapi.Structure bodyIn, vapi.PlanSetup planIn, vtype.DoseValue perscDose, double isodoseLvl)
        {

            //Conformity Index requres Body as input structure for dose calc and volume of target 

            double volIsodoseLvl = planIn.GetVolumeAtDose(bodyIn, perscDose * isodoseLvl, vtype.VolumePresentation.AbsoluteCm3);
            
            return Math.Round(volIsodoseLvl / strIn.Volume*100,3);
        }
        
        public static double ConformationNumber(vapi.Structure strIn, vapi.Structure bodyIn, vapi.PlanSetup planIn, vtype.DoseValue perscDose, double isodoseLvl)
        {
            //Conformation number requres both body and PTV as input structures. 
            double TV = strIn.Volume;

            double V_RI = planIn.GetVolumeAtDose(bodyIn, perscDose * isodoseLvl, vtype.VolumePresentation.AbsoluteCm3);

            double TV_RI = planIn.GetVolumeAtDose(strIn, perscDose * isodoseLvl, vtype.VolumePresentation.AbsoluteCm3);

            return Math.Round((TV_RI/TV)*(TV_RI/V_RI),3); 
        }

        public static double PaddickConformityIndex(vapi.Structure strIn, vapi.Structure bodyIn, vapi.PlanSetup planIn, vtype.DoseValue perscDose, double isodoseLvl)
        {
            //Conformation number requres both body and PTV as input structures. 
            double TV = strIn.Volume;

            double PIV = planIn.GetVolumeAtDose(bodyIn, perscDose * isodoseLvl, vtype.VolumePresentation.AbsoluteCm3);

            double TV_PIV = planIn.GetVolumeAtDose(strIn, perscDose * isodoseLvl, vtype.VolumePresentation.AbsoluteCm3);

            return Math.Round((TV_PIV*TV_PIV)/(TV*PIV), 3);
        }

    }
}