// ППЛГОНД && PPLGOND && PPBGHRC 

using SALOON.dbModel;
using SALOON.ViewModel;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            btnBack.Click += BtnBack_Click;
            btnNext.Click += BtnNext_Click;
            btnDelete.Click += BtnDelete_Click;
            tbSearch.TextChanged += TbSearch_TextChanged;

            loadNiggers();
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

                    Navigator = new Navigator<ClientView>(list, 20);
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
                    });
                }
            });
        }

        public void CurrentDataSize()
        {
            int CurrentDataSize = Navigator.GetPageSize();
            tblDataCount.Text = $"{CurrentDataSize}/{DataCount}";
        }
    }
}
