#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using System.Windows;
using System.Linq;

#endregion

namespace RAA_Int_Mod01_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document curDoc = uiapp.ActiveUIDocument.Document;

            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create schedule");

                #region Create Schedule

                // get the element Id of the category
                ElementId catId = new ElementId(BuiltInCategory.OST_Doors);

                // create the schedule
                ViewSchedule newSchedule = ViewSchedule.CreateSchedule(curDoc, catId);

                // name the schedule
                newSchedule.Name = "My Door Schedule";

                #endregion

                #region Get Parameters for Fields

                // get all door instances
                FilteredElementCollector colDoors = new FilteredElementCollector(curDoc)
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .WhereElementIsNotElementType();

                // get a door instance to retrieve parameters
                Element doorInst = colDoors.FirstElement();

                // the parameters from the door
                Parameter paramDrNum = doorInst.LookupParameter("Mark");
                Parameter paramDrLevel = doorInst.LookupParameter("Level");

                Parameter paramDrWidth = doorInst.get_Parameter(BuiltInParameter.DOOR_WIDTH);
                Parameter paramDrHeight = doorInst.get_Parameter(BuiltInParameter.DOOR_HEIGHT);
                Parameter paramDrType = doorInst.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM);

                // create the fields
                ScheduleField fieldDrNum = newSchedule.Definition.AddField(ScheduleFieldType.Instance, paramDrNum.Id);
                ScheduleField fieldDrLevel = newSchedule.Definition.AddField(ScheduleFieldType.Instance, paramDrLevel.Id);
                ScheduleField fieldDrWidth = newSchedule.Definition.AddField(ScheduleFieldType.ElementType, paramDrWidth.Id);
                ScheduleField fieldDrHeight = newSchedule.Definition.AddField(ScheduleFieldType.ElementType, paramDrHeight.Id);
                ScheduleField fieldDrType = newSchedule.Definition.AddField(ScheduleFieldType.Instance, paramDrType.Id);

                // set the formatting
                fieldDrLevel.IsHidden = true;
                fieldDrWidth.DisplayType = ScheduleFieldDisplayType.Totals;

                #endregion

                #region Create Filters

                Level levelFilter = Utils.GetLevelByName(curDoc, "First Floor");

                ScheduleFilter filterLevel = new ScheduleFilter(fieldDrLevel.FieldId, ScheduleFilterType.Equal, levelFilter.Id);
                newSchedule.Definition.AddFilter(filterLevel);

                #endregion

                #region Sorting & Grouping

                ScheduleSortGroupField sortType = new ScheduleSortGroupField(fieldDrType.FieldId);
                sortType.ShowHeader = true;
                sortType.ShowFooter = true;
                sortType.ShowBlankLine = true;
                newSchedule.Definition.AddSortGroupField(sortType);

                ScheduleSortGroupField sortMark = new ScheduleSortGroupField(fieldDrNum.FieldId);
                newSchedule.Definition.AddSortGroupField(sortMark);

                #endregion

                #region Totals
                newSchedule.Definition.IsItemized = true;
                newSchedule.Definition.ShowGrandTotal = true;
                newSchedule.Definition.ShowGrandTotalTitle = true;
                newSchedule.Definition.ShowGrandTotalCount = true;

                #endregion

                t.Commit();
            }

            // code snippit for filtering a list for unique items
            List<string> rawStrings = new List<string>() { "a", "a", "d", "c", "c", "d", "b", "d" };
            List<string> uniqueStrings = rawStrings.Distinct().ToList();
            uniqueStrings.Sort();


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
