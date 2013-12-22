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

namespace CPTimetable {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {


        public MainPage() {
            this.InitializeComponent();
            this.initilizeStations();
            this.initilizeHours();
        }

        private void initilizeHours() {
            List<String> hours = new List<string>();
            for (int i = 0; i < 24; i++) {
                hours.Add(i.ToString());
            }
            hourPicker.ItemsSource = hours;
        }

        private async void initilizeStations() {

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

        private List<String> ParseTxt(string result) {
            List<String> stations = new List<string>();

            foreach (String station in result.Split('\n')) {
                stations.Add(station.ToUpper());
            }
            return stations;
        }



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (canEnableButton()) {
                searchButton.IsEnabled = true;
            }
        }

        private bool canEnableButton() {
            return stationBox1.SelectedValue != null && stationBox2.SelectedValue != null && hourPicker.SelectedValue != null;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            string departure = stationBox1.SelectedValue.ToString();
            string arrival = stationBox2.SelectedItem.ToString();
            string dateDeparture = date.Date.Year + "-" + date.Date.Month + "-" + date.Date.Day;
            string hour = hourPicker.SelectedValue.ToString();

            processRequest(departure, arrival, dateDeparture, hour);

        }

        private async void processRequest(string departure, string arrival, string dateDeparture, string hour) {
            HttpClient client = new HttpClient();
            string request = "http://carlosefonseca.com/cp/getdata.php?departure=" + departure + "&arrival=" + arrival + "&day=" + dateDeparture + "&hour=" + hour;
            HttpResponseMessage response = await client.GetAsync(request);
            String result = await response.Content.ReadAsStringAsync();

            trainList.ItemsSource = ProcessJson(result);
        }

        private List<TrainInfoItem> ProcessJson(String result) {
            List<TrainInfoItem> resultList = new List<TrainInfoItem>();
            if (!result.Equals("[]")) {
                JsonObject results = JsonObject.Parse(result);

                foreach (string key in results.Keys) {
                    JsonObject train = results[key].GetObject();
                    TrainInfoItem trainInfo = new TrainInfoItem();
                    trainInfo.Type = train["t"].GetString();
                    trainInfo.Departure = train["d"].GetString();
                    trainInfo.Arrival = train["a"].GetString();
                    trainInfo.Duration = train["l"].GetString();
                    resultList.Add(trainInfo);

                }
            } else {
                TrainInfoItem trainInfo = new TrainInfoItem(false);
                resultList.Add(trainInfo);
            }
            resultList.Sort(new DepartureCompare());
            return resultList;
        }

        public class TrainInfoItem {

            private string type;
            private string departure;
            private string arrival;
            private string duration;
            private bool foundTrain;

            public TrainInfoItem() {
                this.foundTrain = true;
            }

            public TrainInfoItem(bool foundTrain) {
                this.type = "";
                this.departure = "Não foram encontrados comboios";
                this.arrival = "";
                this.duration = "";
                this.foundTrain = foundTrain;
            }

            public string Type {
                get {
                    string toReturn = "";
                    bool foundT = false;
                    foreach (char item in type) {
                        switch (item) {
                            case 'R':
                                toReturn += "Regional";
                                break;
                            case 'U':
                                toReturn += "Suburbano";
                                break;
                            case 'I':
                                toReturn += "Inter";
                                break;
                            case 'C':
                                if (!foundT) {
                                    toReturn += "Cidades";
                                }
                                break;
                            case 'A':
                                toReturn += "Alfa";
                                break;
                            case 'P':
                                toReturn += "Pendular";
                                break;
                            case 'T':
                                toReturn += "Transbordo";
                                foundT = true;
                                break;
                            default:
                                toReturn += item;
                                break;
                        }
                    }
                    return toReturn;
                }
                set { type = value; }
            }

            public string Departure {
                get {
                    if (foundTrain) {
                        return "Partida: " + departure;
                    } else {
                        return departure;
                    }
                }
                set { departure = value; }
            }

            public string Arrival {
                get {
                    if (foundTrain) {
                        return "Chegada: " + arrival;
                    } else { return arrival; }
                }
                set { arrival = value; }
            }

            public string Duration {
                get {
                    if (foundTrain) {
                        return "Duração : " + duration;
                    } else {
                        return duration;
                    }
                }
                set { duration = value; }
            }

        }

        public class DepartureCompare : Comparer<TrainInfoItem> {

            public override int Compare(TrainInfoItem x, TrainInfoItem y) {
                return x.Departure.CompareTo(y.Departure);
            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e) {

        }

        private void hourPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (canEnableButton()) {
                searchButton.IsEnabled = true;
            }
        }

        private void trainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
