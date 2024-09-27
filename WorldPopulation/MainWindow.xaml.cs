using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using ScottPlot;
using System.Diagnostics;
using ScottPlot.Colormaps;

namespace WorldPopulation
{
    /// <summary>
    /// logic interaction for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List that will contain all the years in the csv
        List<int> listOfYears = new List<int>();

        public MainWindow()
        {
            InitializeComponent();

            ResetList();

            ScottGraph.Plot.ShowLegend(Edge.Bottom);
        }

        /// <summary>
        /// Resets the list of countries
        /// </summary>
        public void ResetList()
        {
            // Refilling the list box with country's names
            List<CountryPopulation> countryPop = ReadCsvFile();
            listboxNames.Items.Clear();
            countryPop.ForEach(country => listboxNames.Items.Add(country.Name));

            // Clear and refreshing the graph
            ScottGraph.Plot.Clear();
            ScottGraph.Refresh();
        }

        /// <summary>
        /// Clears the search and the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClearGraph(object sender, RoutedEventArgs e)
        {
            listboxNamesChosen.Items.Clear();
            ResetList();
        }

        /// <summary>
        /// Creates the graph with the choosen countries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CreateGraph(object sender, RoutedEventArgs e)
        {
            List<CountryPopulation> countryPop = ReadCsvFile();

            int StartYear;
            int EndYear;

            ScottGraph.Plot.Clear();
            // Add labels in the graph
            ScottGraph.Plot.XLabel("Year");
            ScottGraph.Plot.YLabel("Population");

            // Try catch to get the values of the start and end year
            try
            {
                StartYear = StartYearValue.Text != "" ? Int32.Parse(StartYearValue.Text): listOfYears.Min();
                EndYear = EndYearValue.Text != "" ? Int32.Parse(EndYearValue.Text): listOfYears.Max();

                // If the end year is smaller or equal than the start year
                if (StartYear >= EndYear) throw new Exception();

                // This variable allows to get a little gap between the first point and the edge of the graph
                int gapYear = (int)Math.Ceiling((double)(EndYear - StartYear) / 20);
                ScottGraph.Plot.Axes.SetLimits(StartYear - gapYear, EndYear + gapYear);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your year values aren't a number or the end Year is smaller or equal than the start year", "years error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            countryPop.ForEach(country =>
            {
                foreach (string chosen in listboxNamesChosen.Items)
                {
                    if (chosen == country.Name)
                    {
                        // This list allows to know which dates to filter
                        List<int> chosenIndex = new List<int>();
                        int index = 0;
                        int[] dataX = new int[listOfYears.Count + 1];
                        listOfYears.ForEach(year =>
                        {
                            if(year <= EndYear && year >= StartYear)
                            {
                                chosenIndex.Add(index);
                            }
                            dataX[index] = year;
                            index++;
                        });

                        index = 0;
                        // Getting the final datas after filtering
                        int[] finalDataX = new int[chosenIndex.Count];
                        int[] finalDataY = new int[chosenIndex.Count];
                        chosenIndex.ForEach(i =>
                        {
                            finalDataX[index] = dataX[i];
                            finalDataY[index] = country.PopulationData[i];
                            index++;
                        });
                        ScottGraph.Plot.Add.Scatter(finalDataX, finalDataY)
                                            .LegendText = country.Name;
                        // Scalles both axes
                        //ScottGraph.Plot.Axes.AutoScale();

                        // Scalles only the Y axe
                        ScottGraph.Plot.Axes.AutoScaleY();
                    }
                }
                
            });
            ScottGraph.Refresh();
        }

        /// <summary>
        /// Add the country in another listbox that will be used to display the stats in the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddCountry(object sender, SelectionChangedEventArgs e)
        {
            if(listboxNames.SelectedItem != null)
            {
                listboxNamesChosen.Items.Add(listboxNames.SelectedItem.ToString());

                int index = listboxNames.SelectedIndex;

                // Removing in the original listBox
                if (listboxNames.SelectedIndex >= 0)
                {
                    listboxNames.Items.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// This method will read the csv file and return a list of class with the results
        /// </summary>
        public List<CountryPopulation> ReadCsvFile()
        {
            // Path where the scv file locates (Will be removed when the implementation of client put his own csv file will be done)
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\world_population.csv";

            try
            {
                string[] lines = File.ReadAllLines(path);

                List<CountryPopulation> country = new List<CountryPopulation>();
                lines
                    .Select((line, index) => new { line, index })
                    .ToList()
                    .ForEach(l =>
                    {
                        string[] values = l.line.Split(',');

                        // If it's the first line of the csv, it should contain only the years and then the countries data
                        CountryPopulation c = l.index == 0 ? AddYears(values) : CreateCountry(values);

                        if(c != null)
                        {
                            country.Add(c);
                        }
                    });
                return country;
            }
            catch
            {
                MessageBox.Show("Unable to read file, check if there is an error in the file.", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Creates a list with all the years that are in the csv file 
        /// </summary>
        /// <param name="values">Array that will contain the years</param>
        /// <returns></returns>
        public CountryPopulation AddYears(string[] values)
        {
            // This condition fixes an issue where the years started to add in each other
            if(listOfYears.Count == 0)
            {
                values.ToList().ForEach((value) =>
                {
                    listOfYears.Add(Int32.Parse(value));
                });
            }
            return null;
        }

        /// <summary>
        /// Method that will create the countries data when the csv is already done
        /// </summary>
        /// <param name="values">Array that will contain the data from the country</param>
        /// <returns></returns>
        public CountryPopulation CreateCountry(string[] values)
        {
            List<int> popValues = new List<int>();

            // Converting string to int, if not done can do typing errors
            Int32.TryParse(values[0], out int rank);
            Int32.TryParse(values[5], out int p1);
            Int32.TryParse(values[6], out int p2);
            Int32.TryParse(values[7], out int p3);
            Int32.TryParse(values[8], out int p4);
            Int32.TryParse(values[9], out int p5);
            Int32.TryParse(values[10], out int p6);
            Int32.TryParse(values[11], out int p7);
            Int32.TryParse(values[12], out int p8);

            CountryPopulation c = new CountryPopulation
            {
                Rank = rank,
                CCA3 = values[1],
                Name = values[2],
                Capital = values[3],
                Continent = values[4],
                PopulationData = new List<int> { p1, p2, p3, p4, p5, p6, p7, p8}
            };
            return c;
        } 

        /// <summary>
        /// Creating the class that will contain the csv
        /// </summary>
        public class CountryPopulation
        {
            public int Rank { get; set; }
            public string CCA3 { get; set; }
            public string Name { get; set; }
            public string Capital { get; set; }
            public string Continent { get; set; }
            public List<int> PopulationData { get; set; }
        }
    }
}
