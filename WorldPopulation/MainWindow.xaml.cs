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
        List<int> listOfYears = new List<int>();

        public MainWindow()
        {
            InitializeComponent();

            ResetList();

            ScottGraph.Plot.ShowLegend(Edge.Bottom);


            /*countryPop.ForEach(country => listboxNames.Items.Add(country.Name));

            if(countryPop != null)
            {   
                ShowChart(countryPop);
            }*/
        }

        /// <summary>
        /// Resets the list of countries
        /// </summary>
        public void ResetList()
        {
            List<CountryPopulation> countryPop = ReadCsvFile();
            listboxNames.Items.Clear();
            countryPop.ForEach(country => listboxNames.Items.Add(country.Name));

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

            try
            {
                StartYear = StartYearValue.Text != "" ? Int32.Parse(StartYearValue.Text): listOfYears.Min();

                EndYear = EndYearValue.Text != "" ? Int32.Parse(EndYearValue.Text): listOfYears.Max();

                int gapYear = (int)Math.Ceiling((double)(EndYear - StartYear) / 20);
                ScottGraph.Plot.Axes.SetLimits(StartYear - gapYear, EndYear + gapYear);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Your year values aren't a number", "years error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            countryPop.ForEach(country =>
            {
                foreach (string chosen in listboxNamesChosen.Items)
                {
                    if (chosen == country.Name)
                    {
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
                        int[] dataY = new int[] { country.Population2022, country.Population2020, country.Population2015, country.Population2010, country.Population2000, country.Population1990, country.Population1980, country.Population1970 };

                        index = 0;
                        int[] finalDataX = new int[chosenIndex.Count];
                        int[] finalDataY = new int[chosenIndex.Count];
                        chosenIndex.ForEach(i =>
                        {
                            finalDataX[index] = dataX[i];
                            finalDataY[index] = dataY[i];
                            index++;
                        });
                        ScottGraph.Plot.Add.Scatter(finalDataX, finalDataY);
                        ScottGraph.Plot.Add.Scatter(finalDataX, finalDataY).LegendText = country.Name;
                        //ScottGraph.Plot.Axes.AutoScale();
                        ScottGraph.Plot.Axes.AutoScaleY();
                    }
                }
                
            });
            ScottGraph.Refresh();
        }

        public void ScatterInGraph()
        {

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
        /// <param name="values"></param>
        /// <returns></returns>
        public CountryPopulation AddYears(string[] values)
        {
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
        /// <param name="values"></param>
        /// <returns></returns>
        public CountryPopulation CreateCountry(string[] values)
        {
            // Converting string to int, if not done can do types errors
            Int32.TryParse(values[0], out int rank);
            Int32.TryParse(values[5], out int p22);
            Int32.TryParse(values[6], out int p20);
            Int32.TryParse(values[7], out int p15);
            Int32.TryParse(values[8], out int p10);
            Int32.TryParse(values[9], out int p00);
            Int32.TryParse(values[10], out int p90);
            Int32.TryParse(values[11], out int p80);
            Int32.TryParse(values[12], out int p70);

            CountryPopulation c = new CountryPopulation
            {
                Rank = rank,
                CCA3 = values[1],
                Name = values[2],
                Capital = values[3],
                Continent = values[4],
                Population2022 = p22,
                Population2020 = p20,
                Population2015 = p15,
                Population2010 = p10,
                Population2000 = p00,
                Population1990 = p90,
                Population1980 = p80,
                Population1970 = p70,
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
            public int Population2022 { get; set; }
            public int Population2020 { get; set; }
            public int Population2015 { get; set; }
            public int Population2010 { get; set; }
            public int Population2000 { get; set; }
            public int Population1990 { get; set; }
            public int Population1980 { get; set; }
            public int Population1970 { get; set; }
        }
    }
}
