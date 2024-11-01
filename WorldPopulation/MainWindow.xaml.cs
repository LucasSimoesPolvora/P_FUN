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
using System.Windows.Markup;
using ScottPlot.TickGenerators.TimeUnits;

namespace WorldPopulation
{
    /// <summary>
    /// logic interaction for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List that will contain all the years in the csv
        List<int> listOfYears = new List<int>();

        string filePath = "";

        bool isImported = false;

        public MainWindow()
        {
            InitializeComponent();

            ScottGraph.Plot.ShowLegend(Edge.Bottom);
        }

        /// <summary>
        /// Resets the list of countries
        /// </summary>
        public void ResetList()
        {
            if (isImported)
            {
                // Refilling the list box with country's names
                List<CountryPopulation> countryPop = GiveValues(filePath);
                listboxNames.Items.Clear();
                countryPop.ForEach(country => listboxNames.Items.Add(country.Name));

                // Clear and refreshing the graph
                ScottGraph.Plot.Clear();
                ScottGraph.Refresh();
            }
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
            List<CountryPopulation> countryPop = GiveValues(filePath);

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

                try
                {
                    if (StartYear < listOfYears.Min() || EndYear > listOfYears.Max() || StartYear > listOfYears.Max() || EndYear < listOfYears.Min()) throw new Exception();
                }
                catch
                {
                    MessageBox.Show($"Your years are too big or too small for the data, it should be between {listOfYears.Min()} and {listOfYears.Max()}", "years error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // This variable allows to get a little gap between the first point and the edge of the graph
                int gapYear = (int)Math.Ceiling((double)(EndYear - StartYear) / 20);
                ScottGraph.Plot.Axes.SetLimits(StartYear - gapYear, EndYear + gapYear);
            }
            catch
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
                        int[] dataX = new int[listOfYears.Count];
                        listOfYears.ForEach(year =>
                        {
                            if(year <= EndYear && year >= StartYear)
                            {
                                chosenIndex.Add(index);
                            }
                            dataX[index] = year;
                            index++;
                        });

                        // Getting the final datas after filtering

                        List<int> finalDataX = new List<int>() { 0,0,0,0,0,0,0,0,0,0,0 };
                        List<int> finalDataY = new List<int>() { 0,0,0,0,0,0,0,0,0,0,0 };


                        void CalculatePopulation(int year, bool isItTheStartYear)
                        {
                            int dataIndex = isItTheStartYear ? 0 : chosenIndex.Count + 1;

                            finalDataX[dataIndex] = year;

                            // Getting the closest year to the chosen year
                            int[] closest = new int[2] { listOfYears.Contains(year) ? year : listOfYears.Aggregate((x, y) => Math.Abs(x - year) < Math.Abs(y - year) ? x : y), 0 };
                            closest[1] = listOfYears.FindIndex(x => x == closest[0]);

                            if (closest[0] == year)
                                return;

                            // Getting the closest on the opposite of the chosen year
                            int[] secondClosest = new int[2] { closest[0] < year ? listOfYears[closest[1] - 1] : listOfYears[closest[1] + 1], 0 };
                            secondClosest[1] = listOfYears.FindIndex(x => x == secondClosest[0]);

                            int ClosestyearDistance = secondClosest[0] < closest[0] ? closest[0] - secondClosest[0] : secondClosest[0] - closest[0];

                            // gets the values of the smaller and bigger year
                            int[] smaller = new int[2] { secondClosest[0] < closest[0] ? secondClosest[0] : closest[0], secondClosest[0] < closest[0] ? secondClosest[1] : closest[1] };
                            int[] bigger = new int[2] { secondClosest[0] < closest[0] ? closest[0] : secondClosest[0], secondClosest[0] < closest[0] ? closest[1] : secondClosest[1] };

                            // Calculates the gap between the years
                            int gap = year - smaller[0];
                            double multiplicator = (ClosestyearDistance * gap) / 100.0;

                            int biggestYearPopulation = country.PopulationData[smaller[1]] < country.PopulationData[bigger[1]] ? country.PopulationData[bigger[1]] : country.PopulationData[smaller[1]];
                            int smallestYearPopulation = country.PopulationData[smaller[1]] < country.PopulationData[bigger[1]] ? country.PopulationData[smaller[1]] : country.PopulationData[bigger[1]];
                            int differencePopulation = biggestYearPopulation - smallestYearPopulation;
                            
                            finalDataY[dataIndex] = (int)Math.Floor(country.PopulationData[smaller[1]] + differencePopulation * (multiplicator + 1));
                        };
                        
                        if(StartYearValue.Text != "")
                        {
                            CalculatePopulation(StartYear, true);
                        }

                        if (EndYearValue.Text != "")
                        {
                            CalculatePopulation(EndYear, false);
                        }

                        index = 0;
                        bool shouldBreak = false;
                        chosenIndex.ForEach(i =>
                        {
                            if (shouldBreak)
                                return;

                            void setNormalValues()
                            {
                                if(country.PopulationData[i] != 0)
                                {
                                    finalDataX.Add(dataX[i]);
                                    finalDataY.Add(country.PopulationData[i]);
                                    index++;
                                }
                            };

                            if (index != chosenIndex.Count + 1)
                                setNormalValues();
                            else
                                shouldBreak = true;
                        });

                        List<int> graphDataX = new List<int>();
                        List<int> graphDataY = new List<int>();

                        // Delete useless data taht could bug the chart
                        finalDataX.Where(data => data != 0).ToList().ForEach(data =>
                        {
                            if (!graphDataX.Contains(data))
                            {
                                graphDataX.Add(data);
                            }
                        });

                        // Delete useless data that could bug the chart
                        finalDataY.Where(dataY => dataY != 0).ToList().ForEach(dataY =>
                        {
                            if (!graphDataY.Contains(dataY))
                            {
                                graphDataY.Add(dataY);
                            }
                        });
                        
                        // Sorting the data from smallest to biggest
                        graphDataX.Sort();
                        graphDataY.Sort();

                        ScottGraph.Plot.Add.Scatter(graphDataX.ToArray(), graphDataY.ToArray())
                                            .LegendText = country.Name;
                        // Scalles both axes
                        ScottGraph.Plot.Axes.AutoScale();

                        // Scalles only the Y axe
                        //ScottGraph.Plot.Axes.AutoScaleY();
                    }
                }
                
            });
            ScottGraph.Refresh();
        }

        public void ReadImportPath(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            bool? response = openFileDialog.ShowDialog();

            if (response == true)
            {
                filePath = openFileDialog.FileName;

                GiveValues(filePath);
                ResetList();
                return;
            }
        }

        /// <summary>
        /// Add the country in another listbox that will be used to display the stats in the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddCountry(object sender, SelectionChangedEventArgs e)
        {
            if(listboxNames.SelectedItem != null && listboxNamesChosen.Items.Count < 15)
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
        /// Removes the country of the listbox that will display the stats in the graph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteCountry(object sender, SelectionChangedEventArgs e)
        {
            if (listboxNamesChosen.SelectedItem != null)
            {
                listboxNames.Items.Add(listboxNamesChosen.SelectedItem.ToString());
                int index = listboxNamesChosen.SelectedIndex;
                // Removing in the original listBox
                if (listboxNamesChosen.SelectedIndex >= 0)
                {
                    listboxNamesChosen.Items.RemoveAt(index);
                }

                listboxNames.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("",
                    System.ComponentModel.ListSortDirection.Ascending));
            }
        }

        /// <summary>
        /// This method will read the csv file and return a list of class with the results
        /// </summary>
        public List<CountryPopulation> GiveValues(string filepath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

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
                isImported = true;
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