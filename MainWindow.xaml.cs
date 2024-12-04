// ППЛГОНД && PPLGOND && PPBGHRC 

using SALOON.dbModel;
using SALOON.ViewModel;
using SALOON.Windows;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SALOON
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Navigator<ServiceView> Navigator;
        string SearchText = "";
        int DataCount = 0;
        int PageSize = Int32.MaxValue;
        public static bool IsAdmin = false;

        public MainWindow()
        {
            InitializeComponent();

            btnBack.Click += BtnBack_Click;
            btnNext.Click += BtnNext_Click;
            tbSearch.TextChanged += TbSearch_TextChanged;

            btnAdmin.Click += BtnAdmin_Click;
            btnNew.Click += BtnNew_Click;

            cbSort.SelectionChanged += UpdateNiggers;
            cbPageSize.SelectionChanged += CbPageSize_SelectionChanged;
            cbFilter.SelectionChanged += CbFilter_SelectionChanged;
          
            loadNiggers();
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            new Upsert(null).ShowDialog();
            loadNiggers();
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (tbAdmin.Visibility == Visibility.Hidden)
            {
                tbAdmin.Visibility = Visibility.Visible;
            }
            else
            {
                if (tbAdmin.Text == "0000")
                {
                    IsAdmin = true;
                    btnAdmin.IsEnabled = false;
                    loadNiggers();

                    MessageBox.Show("Вы теперь админ :)");
                }
                else
                {
                    MessageBox.Show("Код неверный!");                       
                }
                tbAdmin.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateNiggers(object sender, SelectionChangedEventArgs e)
        {
            loadNiggers();
        }
        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadNiggers();
        }

        private void CbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbPageSize.SelectedItem != null)
            {
                switch (cbPageSize.SelectedIndex) 
                {
                    case 0:
                        PageSize = Int32.MaxValue;
                        break;
                    case 1:
                        PageSize = 10;
                        break;
                    case 2:
                        PageSize = 50;
                        break;
                    case 3:
                        PageSize = 200;
                        break;
                }
                loadNiggers();
            }
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = tbSearch.Text.Trim();
            loadNiggers();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            Navigator.MoveToPage(Navigator.GetCurrentPage() + 1);
            RecalculateCurrentDataSize();
            lbRabbit.ItemsSource = Navigator.CurrentPage;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigator.MoveToPage(Navigator.GetCurrentPage() - 1);
            RecalculateCurrentDataSize();
            lbRabbit.ItemsSource = Navigator.CurrentPage;
        }

        private void Async(Action action)
        {
            new Thread(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e) 
                { 
                    Dispatcher.Invoke(() => 
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                }
            }).Start();
        }

        private void loadNiggers()
        {
            btnNext.IsEnabled = false;
            btnBack.IsEnabled = false;
            cbSort.IsEnabled = false;
            cbPageSize.IsEnabled = false;

            bool orderByPrice = cbSort.SelectedItem == cbiSortAsk;
            bool orderByPriceD = cbSort.SelectedItem == cbiSortDesk;
            double filter_min = 0, filter_max = 1.1;

            switch (cbFilter.SelectedIndex)
            {
                case 0: break;
                case 1: filter_min = 0; filter_max = 0.05; break;
                case 2: filter_min = 0.05; filter_max = 0.15; break;
                case 3: filter_min = 0.15; filter_max = 0.30; break;
                case 4: filter_min = 0.30; filter_max = 0.70; break;
                case 5: filter_min = 0.70; filter_max = 1.1; break;
            }

            Async(() =>
            {
                using (var db = new Entities())
                {
                    var niggers = db.Service
                    .Where(x => x.Title.Contains(SearchText) || x.Description.Contains(SearchText)).
                    Where(x => x.Discount >= filter_min).
                    Where(x => x.Discount < filter_max)
                    .ToList();

                    var viewList = new List<ServiceView>();

                    foreach (var nigger in niggers)
                    {
                        var c = new ServiceView { Service = nigger };
                        viewList.Add(c);
                    }

                    if (orderByPrice)
                    {
                        viewList = viewList.OrderBy(x => x.PriceWithDiscount).ToList();
                    }
                    else if (orderByPriceD)
                    {
                        viewList = viewList.OrderByDescending(x => x.PriceWithDiscount).ToList();
                    }

                    Navigator = new Navigator<ServiceView>(viewList, PageSize);
                    DataCount = viewList.Count;

                    Dispatcher.Invoke(() => {

                        RecalculateCurrentDataSize();

                        btnNext.IsEnabled = true;
                        btnBack.IsEnabled = true;
                        cbSort.IsEnabled = true;
                        cbPageSize.IsEnabled = true;

                        lbRabbit.ItemsSource = Navigator.CurrentPage;
                    });
                }
            });
        }

        public void RecalculateCurrentDataSize()
        {
            int CurrentDataSize = Navigator.GetDataSize();
            tblDataCount.Text = $"{CurrentDataSize * (Navigator.GetCurrentPage() + 1)}/{DataCount}";
        }


        PropertyInfo btnTag = typeof(Button).GetProperty("Tag");

        /// <summary>
        /// Update
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var service = btnTag.GetValue(sender) as ServiceView;
            new Upsert(service).ShowDialog();
            loadNiggers();
        }

        /// <summary>
        /// Delete
        /// </summary>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var service = btnTag.GetValue(sender) as ServiceView;
            MessageBoxResult result = MessageBox.Show($"Удалить {service.Service.Title}?",
                "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);

            Async(() =>
            {
                using (var db = new Entities())
                {
                    if (result == MessageBoxResult.Yes)
                    {
                        var delItem = db.Service.Where(x => x.Title == service.Service.Title).FirstOrDefault();
                        if (delItem != null)
                        {
                            db.Service.Remove(delItem);
                            db.SaveChanges();

                            Dispatcher.Invoke(() => loadNiggers());
                        }
                    }
                }
            });
        }
    }
}
