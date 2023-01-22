using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_3_3_2
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Reference> selectedElementRefList = null;
            //try-catch отрабатывает механизм отмены программы пр нажатии  Esc
            try
            {
                // 2 строчки ниже это стандартная запись для выбора элементов кликом.
                selectedElementRefList = uidoc.Selection.PickObjects(ObjectType.Face, "Выберете элемент");
                var pipeList = new List<Pipe>();

                // Перебираем спиок выбранных элементов и если в этом списке встречается нужный нам тип элемента,
                // то эти элементы перекладываем в новый список
                foreach (var selectedElement in selectedElementRefList)
                {
                    Element element = doc.GetElement(selectedElement);
                    if (element is Pipe)
                    {
                        Pipe oPipe = (Pipe)element;
                        pipeList.Add(oPipe);
                    }
                }

                //Создаем новый список volumeSelectedWallList
                //куда заносим значения параметра HOST_VOLUME_COMPUTED для элеметов(oWall) списка wallList.
                //Выполняем это перебирая список циклом foreach
                List<double> lengthSelectedWallList = new List<double>();
                foreach (Pipe oPipe in pipeList)
                {
                    //Создаем переменную параметра которая назначается для каждого элемента списка.
                    //Переменной параметра выбирается параметр в Revit отвечающий за вычисления объема CURVE_ELEM_LENGTH.
                    Parameter lengthParametr = oPipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                    //Создаем переменную в которую записываем значение параметра для каждого элемента списка
                    double lengthSelectedWall;

                    //проверяем переменную на соответствие с типом данных (double), чтобы иметь возможность произвести вычисление
                    if (lengthParametr.StorageType == StorageType.Double)
                    {
                        lengthSelectedWall = lengthParametr.AsDouble();
                        // Переводим единицы измерения в кубические метры
                        lengthSelectedWall = UnitUtils.ConvertFromInternalUnits(lengthSelectedWall, /*UnitTypeId.CubicMeters*/DisplayUnitType.DUT_METERS);
                        // Добавляем вычисленные результаты в созданный выше списокб
                        // на выходе имеем список значений объема выбранных элементов (стен) в кубических метрах.
                        lengthSelectedWallList.Add(lengthSelectedWall);
                    }

                }

                // Создаем переменную для в которую рассчитаем значение суммы всех элементов получившегося списка.
                double sumLength = lengthSelectedWallList.ToArray().Sum();
                //Выводим результат в диалоговое окно.
                TaskDialog.Show("Суммарный объем", $"{sumLength}");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            { }

            // If отрабатывает выключение программы в случае когда ничего не выбрано. Это продолжение кода try-catch
            if (selectedElementRefList == null)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
