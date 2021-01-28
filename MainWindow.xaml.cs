using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace Employees_efwpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // initialize canvas date number lables
            int x = 0;
            for (int i = 1; i <= 31; i++)
            {
                Label label = new Label();
                label.Content = i;
                Canvas.SetLeft(label, 35 + x);
                Canvas.SetTop(label, 275);
                x += 15;
                canvas.Children.Add(label);
            }

            // initialize canvas hour number lables
            x = 0;
            for (int i = 6; i <= 19; i++)
            {
                Label label = new Label();
                label.Content = i;
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, 240 - x);
                x += 15;
                canvas.Children.Add(label);
            }

            // initialize monthes combo box
            for(int i = 1; i <= 12; i++)
            {
                monthList.Items.Add(i);
            }
            monthList.SelectedIndex = DateTime.Now.Month - 1;

            countOfCanvasItems = canvas.Children.Count;
        }

        readonly int countOfCanvasItems;

        private void Btn_Click(Object sender, EventArgs e)
        {
            string cellContent = (empGrid.SelectedCells[0].Column.GetCellContent(empGrid.Items[empGrid.SelectedIndex]) as TextBlock).Text;
            string cellContent1 = (empGrid.SelectedCells[1].Column.GetCellContent(empGrid.Items[empGrid.SelectedIndex]) as TextBlock).Text;

            EmployeeDetails employeeDetails = new EmployeeDetails(cellContent, cellContent1);
            employeeDetails.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();

            var emlployeeQuery =
                from emp in employeesDBEntities.Employees.AsEnumerable()
                select new
                {
                    Fio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}",
                    DateOfBirth = emp.BirthDate,
                    Id = emp.Id
                };
            empGrid.ItemsSource = emlployeeQuery.ToList();
            //////////////////////////////////////////////////
            var emlItemsQuery =
                from emp in employeesDBEntities.Employees.AsEnumerable()
                select new
                {
                    Fio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}",
                    Id = emp.Id
                };
            foreach(var item in emlItemsQuery)
            {
                empList.Items.Add($"{item.Fio}/ Таб. номер: {item.Id}");
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DirectoryInfo dirInfo = new DirectoryInfo($"{Environment.CurrentDirectory}\\tmp\\");

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch{}
            }
        }

        private void empList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canvas.Children.Count > countOfCanvasItems)
            {
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
                canvas.Children.RemoveAt(canvas.Children.Count - 1);
            }

            ComboBox comboBox = sender as ComboBox;
            string selectedEmp = comboBox.SelectedItem.ToString();

            int id = GetId(selectedEmp);
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();

            int selectedMonth = Convert.ToInt32(monthList.SelectedItem.ToString());

            IEnumerable<ToWork> toWorks = employeesDBEntities.ToWorks.Where(x => x.EmployeeId == id && x.ArrivalTime.Month == selectedMonth);
            IEnumerable<FromWork> fromWorks = employeesDBEntities.FromWorks.Where(x => x.EmployeeId == id && x.LeavingTime.Month == selectedMonth);

            Polyline toWorkGraph = new Polyline();

            foreach (ToWork tw in toWorks)
            {
                toWorkGraph.Points.Add(new System.Windows.Point(30 + tw.ArrivalTime.Day * 15, 270 - ((tw.ArrivalTime.Hour - 5)*15 + tw.ArrivalTime.Minute/10 * 2.5) ));
            }
            toWorkGraph.Stroke = System.Windows.Media.Brushes.Red;
            canvas.Children.Add(toWorkGraph);

            Polyline fromWorkGraph = new Polyline();

            foreach (FromWork tw in fromWorks)
            {
                fromWorkGraph.Points.Add(new System.Windows.Point(30 + tw.LeavingTime.Day * 15, 270 - ((tw.LeavingTime.Hour - 5) * 15 + tw.LeavingTime.Minute / 10 * 2.5)));
            }
            fromWorkGraph.Stroke = System.Windows.Media.Brushes.Blue;
            canvas.Children.Add(fromWorkGraph);
            int test = canvas.Children.Count;
        }

        private int GetId(string selectedEmp)
        {
            int startFindIndex = selectedEmp.IndexOf("номер: ");
            string strId = selectedEmp.Substring(startFindIndex + 7, selectedEmp.Length - (startFindIndex + 7));
            int id = 0;
            int.TryParse(strId, out id);
            return id;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(tabNumTB.Text))
            {
                try
                {
                    EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();
                    int employeeId = 0;
                    int.TryParse(tabNumTB.Text, out employeeId);
                    
                    if (!employeesDBEntities.ToWorks.Any(x => x.ArrivalTime.Year == DateTime.Now.Year && x.ArrivalTime.Month == DateTime.Now.Month && x.ArrivalTime.Day == DateTime.Now.Day && x.EmployeeId == employeeId))
                    {
                        ToWork newRecord = new ToWork()
                        {
                            EmployeeId = employeeId,
                            ArrivalTime = DateTime.Now
                        };
                        employeesDBEntities.ToWorks.Add(newRecord);
                        MessageBox.Show("Отмечено время прибытия");
                    }
                    else if (!employeesDBEntities.FromWorks.Any(x => x.LeavingTime.Year == DateTime.Now.Year && x.LeavingTime.Month == DateTime.Now.Month && x.LeavingTime.Day == DateTime.Now.Day && x.EmployeeId == employeeId))
                    {
                        FromWork newRecord = new FromWork()
                        {
                            EmployeeId = employeeId,
                            LeavingTime = DateTime.Now
                        };
                        employeesDBEntities.FromWorks.Add(newRecord);
                        MessageBox.Show("Отмечено время ухода");
                    }

                    employeesDBEntities.SaveChanges();
                }
                catch {}
            }
        }

        private void AddBtnClick(object sender, RoutedEventArgs e)
        {
            AddEmployee newEmp = new AddEmployee();
            newEmp.Owner = this;
            newEmp.ShowDialog();
        }

        public void RefreshDataGrid()
        {
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();

            var emlployeeQuery =
                from emp in employeesDBEntities.Employees.AsEnumerable()
                select new
                {
                    Fio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}",
                    DateOfBirth = emp.BirthDate,
                    Id = emp.Id
                };
            empGrid.ItemsSource = null;
            empGrid.ItemsSource = emlployeeQuery.ToList();
        }
    }
}