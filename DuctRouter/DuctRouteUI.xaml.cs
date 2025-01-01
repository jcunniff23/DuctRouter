using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace DuctRouter
{
    /// <summary>
    /// Interaction logic for DuctRouteUI.xaml
    /// </summary>
    public partial class DuctRouteUI : Window, INotifyPropertyChanged
    {
        Document _doc;
        UIDocument _uidoc;
        private DuctRouteHandler _handler;

        List<Element> _ductMain = new List<Element>();
        List<Element> _terminal = new List<Element>();

        private string _DuctCount;
        private string _TerminalCount;
        public List<Element> DuctMain
        {
            get => _ductMain;
            set
            {
                _ductMain = value;
                UpdateDuctCount();
                OnPropertyChanged();
            }
            
        }

        public List<Element> Terminal
        {
            get => _terminal;
            set
            {
                _terminal = value;
                UpdateTerminalCount();
                OnPropertyChanged();
            }

        }

        public string DuctCount
        {
            get => _DuctCount;
            set
            {
                _DuctCount = value;
                OnPropertyChanged();
            }
        }

        public string TerminalCount
        {
            get => _TerminalCount;
            set
            {
                _TerminalCount = value;
                OnPropertyChanged();
            }
        }

        private void UpdateDuctCount()
        {
            int ductCt = _ductMain.Count();

            DuctCount = $"Qty: {ductCt}";
        }

        private void UpdateTerminalCount()
        {
            int terminalCt = _terminal.Count();

            TerminalCount = $"Qty: {terminalCt}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DuctRouteUI(ExternalCommandData commandData)
        {
            InitializeComponent();
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = commandData.Application.ActiveUIDocument.Document;

            DataContext = this;

            _handler = new DuctRouteHandler(commandData);
                
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load all MechanicalSystems in the given document
            //Filtered element collector for duct 

            try
            {
                var mechSystemCollector = new FilteredElementCollector(_doc)
                    .OfClass(typeof(MechanicalSystem));

                //TaskDialog.Show("WINDOW LOADED", $"{mechSystemCollector.Count()}");

                var items = mechSystemCollector.ToList();
                systemComboBox.ItemsSource = items;
            }
            catch (Exception)
            {
                TaskDialog.Show("SYSTEM COLLECTION ERROR", "SYSTEM COLLECTION ERROR");
            }
            

        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Show("DUCT READOUT", "DUCT ELEMENT: " + _ductMain[0].Name.ToString());

            if (_doc == null)
            {
                TaskDialog.Show("NULL DOC", "DOC IS NULL");
            }

            if (_uidoc == null)
            {
                TaskDialog.Show("UINULL DOC", "UIDOC IS NULL");
            }

            //TaskDialog.Show("DUCT READOUT", "DUCT ELEMENT: " + _ductMain.Name +"\n" + "TERMINAL ELEMENT: "+ _terminal.Name);
        }


        private void SelectTerminal_Click(object sender, RoutedEventArgs e)
        {
            //TaskDialog.Show("SELECT TERMINAL", "SELECT TERMINAL CLICKED");
            try
            {
                IList<Reference> selectedRef = _uidoc.Selection.PickObjects(ObjectType.Element, new TerminalSelectionFilter(), "Select terminal(s)");
                var terminalIds = selectedRef.Select(r => r.ElementId).ToList();
                _terminal = terminalIds.Select(id => _uidoc.Document.GetElement(id)).ToList();
                TaskDialog.Show("Duct Router", $"Selected {_terminal.Count} terminals.");

                //Reference selectedRef = _uidoc.Selection.PickObject(ObjectType.Element, new TerminalSelectionFilter(), "Select terminal(s)");
                //var terminalIds = selectedRef.ElementId;
                //var terminals = _uidoc.Document.GetElement(terminalIds);
                Terminal = new List<Element>(_terminal);

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                TaskDialog.Show("DuctRouter", "Selection Canceled.");
                
            }
            catch (Exception ex)
            {
                TaskDialog.Show("DuctRouter", $"{ex.Message}");
            }

        }

        private void SelectDuctMain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Reference selectedRef = _uidoc.Selection.PickObject(ObjectType.Element, new DuctSelectionFilter(), "Select Duct Main");
                var ductIds = selectedRef.ElementId;
                var duct = _uidoc.Document.GetElement(ductIds);
                DuctMain = new List<Element> { duct };

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                TaskDialog.Show("DuctRouter", "Selection Canceled.");

            }
            catch (Exception ex)
            {
                TaskDialog.Show("DuctRouter", $"{ex.Message}");
            }
        }

        private void Route_Click(object sender, RoutedEventArgs e)
        {
            //_handler.ResponseTest();
            _handler.AddTerminalsToHandler(_terminal);
            _handler.AddDuctsToHandler(_ductMain);
            _handler.RouteAllElements();
        }
    }
}
