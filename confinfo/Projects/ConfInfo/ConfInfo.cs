using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using vapi = VMS.TPS.Common.Model.API;
using vtype = VMS.TPS.Common.Model.Types;
using ConfInfo;

namespace VMS.TPS
{
  public class Script
  {
        public Script()
        {
        }
        //public void Execute(vapi.ScriptContext context)//, Window window)  // place try catch around Execute 
        public void Execute(vapi.ScriptContext context, Window window)  // place try catch around Execute 
    {
        try
        {

                //CentralControl cc = new CentralControl(context.Patient, context.PlansInScope, context.CurrentUser);//, window);
                CentralControl cc = new CentralControl(context.Patient, context.PlansInScope, context.CurrentUser, window);

                //Sick hack please dont tell anyone!
                //window.Close();
                //throw new System.InvalidOperationException("Closing...");
                //System.Windows.Forms.SendKeys.SendWait("{ENTER}");

            }
        catch (Exception e)
        {
            throw e;
        }
    }
  }
}
