using Microsoft.Win32;
using SALOON.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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

namespace SALOON.Windows
{
    /// <summary>
    /// Логика взаимодействия для Upsert.xaml
    /// </summary>
    public partial class Upsert : Window
    {
        private ServiceView serviceView;
        private bool createNew = false;

        public Upsert(ServiceView service)
        {
            InitializeComponent();

            serviceView = service;
            if (service == null)
            {
                createNew = true;
                serviceView = new ServiceView { Service = new dbModel.Service { } };
                tbId.Text = "" + -1;
            }
            else
            {
                tbId.Text = "" + service.Service.ID;
                tbCost.Text = "" + service.Service.Cost;
                tbDescr.Text = $"{service.Service.Description}";
                tbDiscount.Text = service.Service.Discount == null ? "" : service.Service.Discount * 100 + "";
                tbLastSeconds.Text = "" + service.Service.DurationInSeconds;
                tbTitle.Text = "" + service.Service.Title;

                try
                {
                    imgMain.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(service.Service.MainImagePath)));
                }
                catch
                {
                    MessageBox.Show("Femboy error");
                }
            }   

            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnChooseMainPhoto.Click += BtnChooseMainPhoto_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            btnCancel.Click += (h, i) => Close();
            btnSave.Click += BtnSave_Click;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new dbModel.Entities())
            {
                var h = db.Service.Where(x => x.ID == serviceView.Service.ID).First();
                MessageBox.Show(h.MainImagePath);

                h.MainImagePath = serviceView.Service.MainImagePath;

                MessageBox.Show(h.MainImagePath);
                db.SaveChanges();
                Close();
            }          
        }

        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (lbPhoto.SelectedIndex != -1)
                lbPhoto.Items.RemoveAt(lbPhoto.SelectedIndex); 
        }

        private void BtnChooseMainPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG|*.png|JPG|*.jpg|JPEG|*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string newfilename = openFileDialog.FileName.Replace("/", "").Replace("\\", "").Replace(":", "");

                try
                {
                    if (!Directory.Exists("Images"))
                        Directory.CreateDirectory("Images");

                    serviceView.Service.MainImagePath = "Images/" + newfilename;
                    File.Copy(openFileDialog.FileName, "Images/" + newfilename);
                }
                catch (Exception e3) {
                    MessageBox.Show(e3.Message);
                }
                
                imgMain.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void BtnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
