using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
//using System.Drawing;
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
    /// Interaction logic for EmployeeDetails.xaml
    /// </summary>
    public partial class EmployeeDetails : Window
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string Dob { get; set; }

        public EmployeeDetails(string name, string dob)
        {
            InitializeComponent();
            fioTxt.Text = name;
            string[] fio = name.Split(' ');
            FirstName = fio[1];
            LastName = fio[0];
            MiddleName = fio[2];
            Dob = dob;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();
            Employee emp = employeesDBEntities.Employees.Where(em => em.FirstName == FirstName && em.LastName == LastName && em.MiddleName == MiddleName).FirstOrDefault();

            LoadImage(emp);

            arrivDateTxt.Text = employeesDBEntities.Orders.Where(o => o.EmployeeId == emp.Id && o.EmploymentDate != null).FirstOrDefault()?.EmploymentDate.ToString() + ", приказ № " + employeesDBEntities.Orders.Where(o => o.EmployeeId == emp.Id && o.EmploymentDate != null).FirstOrDefault()?.Id.ToString();

            if(employeesDBEntities.Orders.Where(o => o.EmployeeId == emp.Id && o.FiredDate != null).FirstOrDefault() != null)
            {
                firedDateTxt.Text = employeesDBEntities.Orders.Where(o => o.EmployeeId == emp.Id && o.FiredDate != null).FirstOrDefault().FiredDate.ToString() + ", приказ № " + employeesDBEntities.Orders.Where(o => o.EmployeeId == emp.Id && o.FiredDate != null).FirstOrDefault().Id.ToString();
            }
            else
            {
                firedDateTxt.Text = "По настоящее время работает";
            }
            

        }
        private void ChangePhotoBtnClick(object sender, RoutedEventArgs e)
        {
            EmployeesDBEntities employeesDBEntities = new EmployeesDBEntities();
            Employee empl = employeesDBEntities.Employees.Where(em => em.FirstName == FirstName && em.LastName == LastName && em.MiddleName == MiddleName).FirstOrDefault();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(dialog.FileName))
            {
                byte[] imageData;
                using (System.IO.FileStream fs = new System.IO.FileStream(dialog.FileName, FileMode.Open))
                {
                    imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, imageData.Length);
                }
                empl.Photo = imageData;
                employeesDBEntities.SaveChanges();

                MessageBox.Show("Успешно!");

                LoadImage(empl);
                
            }

        }

        private void LoadImage(Employee emp)
        {
            try
            {
                if (emp.Photo != null)
                {
                    using (FileStream fs = new FileStream($"{Environment.CurrentDirectory}\\tmp\\{LastName}{emp.Id}.png", FileMode.OpenOrCreate))
                    {
                        fs.Write(emp.Photo, 0, emp.Photo.Length);
                        fs.Close();
                    }
                    BitmapImage bi = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\tmp\\{LastName}{emp.Id}.png"));
                    photo.Source = bi;
                }
                else
                {
                    BitmapImage bi = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\noimg.png"));
                    photo.Source = bi;
                }

            }
            catch
            {
                if (File.Exists($"{Environment.CurrentDirectory}\\tmp\\{LastName}{emp.Id}.png"))
                {
                    BitmapImage bi = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\tmp\\{LastName}{emp.Id}.png"));
                    photo.Source = bi;
                }
                else
                {
                    BitmapImage bi = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\noimg.png"));
                    photo.Source = bi;
                }
            }
        }
    }
}
