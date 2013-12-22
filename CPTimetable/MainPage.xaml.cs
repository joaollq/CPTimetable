using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CPTimetable
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private List<String> stations;

        public MainPage()
        {
            this.InitializeComponent();
            this.initilizeStations();
            this.initializeHour();
        }

        private void initializeHour()
        {
            List<string> hours = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                hours.Add(i.ToString());
            }
            hourPicker.ItemsSource = hours;
        }

        private async void initilizeStations()
        {

            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            String result = "";
            try
            {
                response = await client.GetAsync("http://carlosefonseca.com/cp/estacoes.txt");
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Tem de estar conectado à internet para correr esta app");


                dialog.Commands.Add(new UICommand("Close",new UICommandInvokedHandler(this.CloseApp)));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                dialog.ShowAsync();
            }
            
            

            List<String> stations = ParseTxt(result);
            stationBox1.ItemsSource = stations;
            stationBox2.ItemsSource = stations;
        }

        private void CloseApp(IUICommand command)
        {
            Application.Current.Exit();
        }

        private List<String> ParseTxt(string result)
        {
            List<String> stations = new List<string>();

            foreach (String station in result.Split('\n'))
            {
                stations.Add(station);
            }
            return stations;
        }



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string departure = stationBox1.SelectedValue.ToString();
            string arrival = stationBox2.SelectedItem.ToString();
            string dateDeparture = date.Date.Year + "-" + date.Date.Month + "-" + date.Date.Day;
            string hour = hourPicker.SelectedItem.ToString();

            processRequest(departure, arrival, dateDeparture, hour);

        }

        private async void processRequest(string departure, string arrival, string dateDeparture, string hour)
        {
            HttpClient client = new HttpClient();
            string request = "http://carlosefonseca.com/cp/getdata.php?departure=" + departure + "&arrival=" + arrival + "&day=" + dateDeparture + "&hour=" + hour;
            HttpResponseMessage response = await client.GetAsync(request);
            String result = await response.Content.ReadAsStringAsync();

            trainList.ItemsSource = ProcessJson(result);
        }

        private List<String> ProcessJson(string result)
        {
            List<string> resultList = new List<string>();

            JsonObject results = JsonObject.Parse(result);

            foreach (string key in results.Keys)
            {
                JsonObject train = results[key].GetObject();
                resultList.Add(train["t"].GetString() + " -- " + train["d"].GetString() + " - "+train["a"].GetString());

            }
            return resultList;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
