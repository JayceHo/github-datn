using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DATN_CHUYEN_DE_REVITAPI
{
    [TransactionAttribute(TransactionMode.Manual)]

    class Sample_Schedule : IExternalCommand
    {
        private static BuiltInParameter[] s_skipParameters = new BuiltInParameter[] { BuiltInParameter.REBAR_BAR_DIAMETER, 
            BuiltInParameter.REBAR_NUMBER,  BuiltInParameter.REBAR_ELEM_QUANTITY_OF_BARS, BuiltInParameter.REBAR_ELEM_BAR_SPACING,
        BuiltInParameter.REBAR_ELEM_TOTAL_LENGTH , BuiltInParameter.REBAR_ELEM_LENGTH};
        UIApplication uiapp;
        UIDocument uidoc;
        Autodesk.Revit.ApplicationServices.Application app;
        public Document doc;
        
        const string Folder = "Resource";
        const string r = "Revit";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            app = uiapp.Application;
            doc = uidoc.Document;

            try
            {
                CreateAndAddSchedules(uidoc);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {

                message = ex.Message;
                return Result.Failed;
            }




            
        }

        public void CreateAndAddSchedules(UIDocument uiDocument)
        {
            TransactionGroup tGroup = new TransactionGroup(uiDocument.Document, "Create schedules and sheets");
            tGroup.Start();

            ICollection<ViewSchedule> schedules = CreateSchedules(uiDocument);

            foreach (ViewSchedule schedule in schedules)
            {
                AddScheduleToNewSheet(uiDocument.Document, schedule);
            }

            tGroup.Assimilate();
        }

        /// <summary>
        /// Create a view schedule of rebar category and add schedule field, filter and sorting/grouping field to it.
        /// </summary>
        
        /// <returns>ICollection of created view schedule(s).</returns>
        private ICollection<ViewSchedule> CreateSchedules(UIDocument uiDocument)
        {
            
            Transaction t = new Transaction(doc, "Create Schedules");
            t.Start();

            List<ViewSchedule> schedules = new List<ViewSchedule>();

            //Create an empty view schedule of rebar category.
            ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Rebar), ElementId.InvalidElementId);
            schedule.Name = "Thống kê thép dầm 1";
            schedules.Add(schedule);

            //Iterate all the schedulable field gotten from the rebar view schedule. Lặp lại tất cả trường có thể lập lịch nhận được từ schedule xem tường.
            foreach (SchedulableField schedulableField in schedule.Definition.GetSchedulableFields())
            {
                //Judge (xét) if the FieldType is ScheduleFieldType.Instance.
                if (schedulableField.FieldType == ScheduleFieldType.Instance)
                {
                    //Get ParameterId of SchedulableField.
                    ElementId parameterId = schedulableField.ParameterId;

                    //If the ParameterId is id of BuiltInParameter.ALL_MODEL_MARK then ignore(bỏ qua) next operation.
                    if (ShouldSkip(parameterId)) // Kiểm tra parameterID vừa nhập vào có trong biến hệ thống có điều kiện kh cần skip
                        continue;

                    //Add a new schedule field to the view schedule by using the SchedulableField as argument of AddField method of Autodesk.Revit.DB.ScheduleDefinition class.
                    ScheduleField field = schedule.Definition.AddField(schedulableField);

                    if (field.ColumnHeading == "Rebar Number")
                    {
                        field.ColumnHeading = "Số hiệu";
                    }else
                    if (field.ColumnHeading == "Bar Length")
                    {
                        field.ColumnHeading = "Chiều dài thanh";
                    }
                    else
                    if (field.ColumnHeading == "Spacing")
                    {
                        field.ColumnHeading = "Khoảng cách thép";
                    }
                    else
                    if (field.ColumnHeading == "Quantity")
                    {
                        field.ColumnHeading = "Số lượng";
                    }
                    else
                    if (field.ColumnHeading == "Total Bar Length")
                    {
                        field.ColumnHeading = "Tổng chiều dài";
                    }
                    else
                    if (field.ColumnHeading == "Bar Diameter")
                    {
                        field.ColumnHeading = "Đường kính";
                    }
                    
                    //Judge if the parameterId is a BuiltInParameter.
                    if (Enum.IsDefined(typeof(BuiltInParameter), parameterId.IntegerValue))
                    {
                        BuiltInParameter bip = (BuiltInParameter)parameterId.IntegerValue;
                        //Get the StorageType of BuiltInParameter.
                        StorageType st = doc.get_TypeOfStorage(bip);
                        //if StorageType is String or ElementId, set GridColumnWidth of schedule field to three times of current GridColumnWidth. 
                        //And set HorizontalAlignment property to left.
                        if (st == StorageType.String || st == StorageType.ElementId)
                        {
                            field.GridColumnWidth = 3 * field.GridColumnWidth;
                            field.HorizontalAlignment = ScheduleHorizontalAlignment.Left;
                        }
                        //For other StorageTypes, set HorizontalAlignment property to center.
                        else
                        {
                            field.HorizontalAlignment = ScheduleHorizontalAlignment.Center;
                        }
                    }


                    //Filter the view schedule by volume
                    if (field.ParameterId == new ElementId(BuiltInParameter.HOST_VOLUME_COMPUTED))
                    {
                        double volumeFilterInCubicFt = 0.8 * Math.Pow(3.2808399, 3.0);
                        ScheduleFilter filter = new ScheduleFilter(field.FieldId, ScheduleFilterType.GreaterThan, volumeFilterInCubicFt);
                        schedule.Definition.AddFilter(filter);
                    }

                    //Group and sort the view schedule by type
                    if (field.ParameterId == new ElementId(BuiltInParameter.ELEM_TYPE_PARAM))
                    {
                        ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                        sortGroupField.ShowHeader = true;
                        schedule.Definition.AddSortGroupField(sortGroupField);
                    }
                }
            }

            t.Commit();

            uiDocument.ActiveView = schedule;

            return schedules;
        }

        private bool ShouldSkip(ElementId parameterId)// Kiểm tra ElementID vừa nhập vào có thuộc điều kiện ở trên cùng kh (skipParameter)
        {
            foreach (BuiltInParameter bip in s_skipParameters)
            {
                if (new ElementId(bip) == parameterId)
                    return false;
            }
            return true;
        }

        private void AddScheduleToNewSheet(Document document, ViewSchedule schedule)
        {
            //Create a filter to get all the title block types.
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
            collector.WhereElementIsElementType();

            Transaction t = new Transaction(document, "Create and populate sheet");
            t.Start();

            //Get ElementId of first title block type.
            ElementId titleBlockId = collector.FirstElementId();

            //Create sheet by gotten title block type.
            ViewSheet newSheet = ViewSheet.Create(document, titleBlockId);
            newSheet.Name = "Sheet for " + schedule.Name;

            document.Regenerate();

            //Declare a XYZ to be used as the upperLeft point of schedule sheet instance to be created.
            XYZ upperLeft = new XYZ();

            //If there is an existing title block.
            if (titleBlockId != ElementId.InvalidElementId)
            {
                //Find titleblock of the newly created sheet.
                collector = new FilteredElementCollector(document);
                collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
                collector.OwnedByView(newSheet.Id);
                Element titleBlock = collector.FirstElement();

                //Get bounding box of the title block.
                BoundingBoxXYZ bbox = titleBlock.get_BoundingBox(newSheet);

                //Get upperLeft point of the bounding box.
                upperLeft = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);
                //Move the point to the postion that is 2 inches right and 2 inches down from the original upperLeft point.
                upperLeft = upperLeft + new XYZ(2.0 / 12.0, -2.0 / 12.0, 0);
            }

            //Create a new schedule sheet instance that makes the sheet to show the data of wall view schedule at upperLeft point.
            ScheduleSheetInstance placedInstance = ScheduleSheetInstance.Create(document, newSheet.Id, schedule.Id, upperLeft);

            t.Commit();
        }
    }
}
