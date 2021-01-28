using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Employees_efwpf
{
    /// <summary>
    /// Interaction logic for AddEmployee.xaml
    /// </summary>
    public partial class AddEmployee : Window
    {
        string imgPath;
        public AddEmployee()
        {
            InitializeComponent();
        }

        private void posList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SelectBtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog imgSelect = new OpenFileDialog();

            imgSelect.ShowDialog();
            imgPath = imgSelect.FileName;
        }

        private void AddBtnClick(object sender, RoutedEventArgs e)
        {
            string firstName = fNameTb.Text;
            string lastName = lNameTb.Text;
            string middleName = mNameTb.Text;
            int positionId = -1;
            DateTime dateOfBirth = dobPicker.DisplayDate;
            string position = posList.SelectedItem?.ToString();

            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();

            var query = from pos in employeesDBEntities.Positions.AsEnumerable()
                        where pos.PositionName == position
                        select pos.Id;

            try
            {
                positionId = Convert.ToInt32(query.First());
            }
            catch {}
            

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName) && positionId != -1)
            {
                Employee newEmp = new Employee()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    BirthDate = dateOfBirth,
                    PositionId = positionId
                };

                try
                {
                    byte[] imageData;
                    using (System.IO.FileStream fs = new System.IO.FileStream(imgPath, FileMode.Open))
                    {
                        imageData = new byte[fs.Length];
                        fs.Read(imageData, 0, imageData.Length);
                    }
                    newEmp.Photo = imageData;
                }
                catch {}

                MainWindow mw = this.Owner as MainWindow;

                employeesDBEntities.Employees.Add(newEmp);
                employeesDBEntities.SaveChanges();
                MessageBox.Show("Успешно!");
                mw.RefreshDataGrid();
                this.Close();
            }


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();

            var query = from pos in employeesDBEntities.Positions.AsEnumerable()
                        select pos.PositionName;

            foreach(var p in query.ToList())
            {
                posList.Items.Add(p);
            }

            
        }
    }
}
