// ППЛГОНД && PPLGOND && PPBGHRC 

using SALOON.dbModel;
using SALOON.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
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
        private Navigator<ClientView> Navigator;
        string SearchText = "";
        int DataCount = 0;
        int PageSize = Int32.MaxValue;

        public MainWindow()
        {
            InitializeComponent();

            btnBack.Click += BtnBack_Click;
            btnNext.Click += BtnNext_Click;
            btnDelete.Click += BtnDelete_Click;
            tbSearch.TextChanged += TbSearch_TextChanged;
            cbFilter.SelectionChanged += UpdateNiggers;
            cbSort.SelectionChanged += UpdateNiggers;
            cbPageSize.SelectionChanged += CbPageSize_SelectionChanged;
            loadNiggers();
        }

        private void UpdateNiggers(object sender, SelectionChangedEventArgs e)
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

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(dgNiggers.SelectedItems.Count > 0)
            {
                using (var db = new Entities())
                {
                    foreach (ClientView item in dgNiggers.SelectedItems)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"Вы хотите удалить: {item.Client.LastName} {item.Client.FirstName} {item.Client.Patronymic}",
                            "Удаление пользователя", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            var client = db.Client.FirstOrDefault(x => x.ID == item.Client.ID);
                            db.Client.Remove(client);
                            db.SaveChanges();
                            loadNiggers();
                        }
                    }
                }
            }
        }

        public List<ClientView> SortClients(List<ClientView> list)
        {
            switch (cbSort.SelectedIndex)
            {  
                case 1: 
                    list = (List<ClientView>) list.OrderBy(x => x.Client.LastName);
                    break;
                case 2:
                    list = (List<ClientView>)list.OrderBy(x => x.LastVisit);
                    break;
                case 3:
                    list = (List<ClientView>)list.OrderByDescending(x => x.Visits);
                    break;
            }
            return list;
        }

        public List<ClientView> FilterGender(List<ClientView> list)
        {
            if(cbFilter.SelectedIndex != 0)
            {
                return (List<ClientView>)list.Where(x => x.Client.GenderCode == cbFilter.SelectedIndex.ToString());
            }
            return list;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            Navigator.MoveToPage(Navigator.GetCurrentPage() + 1);
            dgNiggers.ItemsSource = Navigator.CurrentPage;
            CurrentDataSize();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigator.MoveToPage(Navigator.GetCurrentPage() - 1);
            dgNiggers.ItemsSource = Navigator.CurrentPage;
            CurrentDataSize();
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
            btnDelete.IsEnabled = false;
            tbSearch.IsEnabled = false;
            cbFilter.IsEnabled = false;
            cbSort.IsEnabled = false;
            cbPageSize.IsEnabled = false;
            chbClientBDayInMonth.IsEnabled = false;

            Async(() =>
            {
                using (var db = new Entities())
                {
                    var niggers = db.Client.Include("Tag")
                    .Where(x => x.FirstName.Contains(SearchText)
                    || x.LastName.Contains(SearchText)
                    || x.Patronymic.Contains(SearchText)
                    || x.Phone.Contains(SearchText)
                    || x.Email.Contains(SearchText)).ToList();

                    var list = new List<ClientView>();

                    foreach (var nigger in niggers)
                    {
                        var c = new ClientView { Client = nigger };
                        foreach (var t in nigger.Tag)
                        {
                            t.Color = "#" + t.Color;
                            c.Tags.Add(new TagView { Tag = t });
                        }
                        list.Add(c);
                    }

                    list = FilterGender(list);
                    list = SortClients(list);

                    Navigator = new Navigator<ClientView>(list, PageSize);
                    DataCount = list.Count;

                    Dispatcher.Invoke(() => {
                        dgNiggers.ItemsSource = Navigator.CurrentPage;
                        CurrentDataSize();

                        btnNext.IsEnabled = true;
                        btnBack.IsEnabled = true;
                        btnDelete.IsEnabled = true;
                        tbSearch.IsEnabled = true;
                        cbFilter.IsEnabled = true;
                        cbSort.IsEnabled = true;
                        cbPageSize.IsEnabled = true;
                        chbClientBDayInMonth.IsEnabled = true;
                    });
                }
            });
        }

        public void CurrentDataSize()
        {
            int CurrentDataSize = Navigator.GetDataSize();
            tblDataCount.Text = $"{CurrentDataSize}/{DataCount}";
        }
    }
}
