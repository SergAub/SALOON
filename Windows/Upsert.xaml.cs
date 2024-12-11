using Microsoft.Win32;
using SALOON.dbModel;
using SALOON.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                spID.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbId.Text = "" + service.Service.ID;
                tbCost.Text = "" + service.Service.Cost;
                tbDescr.Text = $"{service.Service.Description}";
                tbDiscount.Text = service.Service.Discount == null ? "" : service.Service.Discount * 100 + "";
                tbLastSeconds.Text = "" + service.Service.DurationInSeconds;
                tbTitle.Text = "" + service.Service.Title;

                LoadPhotos();

                try
                {
                    imgMain.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(service.Service.MainImagePath)));
                }
                catch
                {

                }
            }   

            btnAddPhoto.Click += BtnAddPhoto_Click;
            btnChooseMainPhoto.Click += BtnChooseMainPhoto_Click;
            btnRemovePhoto.Click += BtnRemovePhoto_Click;
            btnCancel.Click += (h, i) => Close();
            btnSave.Click += BtnSave_Click;
        }

        private void LoadPhotos()
        {
            using (var db = new Entities())
            {
                var photos = db.ServicePhoto.Where(x => x.ServiceID == serviceView.Service.ID).ToList();
                var zatknis = new List<ServicePhotoView>();

                foreach (var photo in photos)
                {
                    zatknis.Add(new ServicePhotoView { ServicePhoto = photo });
                }

                lbPhoto.ItemsSource = zatknis;
            }
        }

        private Service getServiceFromFields()
        {
            var s = serviceView.Service;
            decimal cost;
            double disc = 0;
            int dur = 0;

            if (tbTitle.Text.Length <= 2) throw new Exception("No title defined"); 
            s.Title = tbTitle.Text.Trim();

            if(!decimal.TryParse(tbCost.Text.Replace(" ", ""), out cost)) throw new Exception("Неверно указана цена");
            if (!double.TryParse(tbDiscount.Text.Replace(" ", ""), out disc)) throw new Exception("Неверно указана скидка");
            if (!int.TryParse(tbLastSeconds.Text.Replace(" ", ""), out dur)) throw new Exception("Недопустимое время");

            s.Description = tbDescr.Text.Trim();

            if (dur <= 60 || dur > 60 * 60 * 4) throw new Exception("Неверно указано время");
            if (disc < 0) throw new Exception("Неверно указана скидка");
            if (cost <= 0) throw new Exception("Неверно указана цена");

            s.Cost = cost;
            s.Discount = disc / 100.0;
            s.DurationInSeconds = dur;

            return s;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new dbModel.Entities())
            {
                try
                {
                    db.Service.AddOrUpdate(getServiceFromFields());
                    db.SaveChanges();
                    Close();
                }
                catch (Exception sex)
                {
                    MessageBox.Show(sex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }          
        }

        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (lbPhoto.SelectedItem == null) return;
            var x = MessageBox.Show("Delete this photo? Cannot be canseled ater Yes", "Tugrik", MessageBoxButton.YesNo);          
            if (x != MessageBoxResult.Yes) return;

            try
            {
                var photo = (lbPhoto.SelectedItem as ServicePhotoView).ServicePhoto;

                if (createNew)
                {
                    serviceView.Service.ServicePhoto.Remove(photo);
                    lbPhoto.Items.RemoveAt(lbPhoto.SelectedIndex);
                }
                else
                {
                    using (var db = new Entities())
                    {
                        db.ServicePhoto.Attach(photo); 
                        db.ServicePhoto.Remove(photo);
                        db.SaveChanges();

                        serviceView.Service = db.Service.Where(xy => xy.ID == serviceView.Service.ID).First();
                    }
                    LoadPhotos();
                }                              
            }
            catch (Exception fs)
            {
                MessageBox.Show(fs.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                    if (!File.Exists("Images/" + newfilename))
                    {
                        File.Copy(openFileDialog.FileName, "Images/" + newfilename);
                    }
                }
                catch (Exception e3) {
                    MessageBox.Show(e3.Message);
                }
                
                imgMain.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void BtnAddPhoto_Click(object sender, RoutedEventArgs e)
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

                    if (!File.Exists("Images/" + newfilename)) {
                        File.Copy(openFileDialog.FileName, "Images/" + newfilename);
                    }

                    var h = new ServicePhoto
                    {
                        PhotoPath = "Images/" + newfilename
                    };

                    if (createNew)
                    {
                        if (serviceView.Service.ServicePhoto == null || serviceView.Service.ServicePhoto.Count == 0)
                        {
                            serviceView.Service.ServicePhoto = new List<ServicePhoto> { h };
                        }
                        else
                        {
                            serviceView.Service.ServicePhoto.Add(h);
                        }

                        lbPhoto.Items.Add(new ServicePhotoView { ServicePhoto = h });
                    }
                    else
                    {
                        using (var db = new Entities())
                        {
                            h.ServiceID = serviceView.Service.ID;
                            db.ServicePhoto.AddOrUpdate(h);

                            db.SaveChanges();                           
                        }
                        LoadPhotos();
                    }

                    
                }
                catch (Exception e3)
                {
                    MessageBox.Show(e3.Message);
                }
            }
        }
    }
}
