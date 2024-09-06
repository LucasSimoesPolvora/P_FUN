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

namespace WorldPopulation
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<CountryPopulation> countryPop = ReadCsvFile();

            Loaded += (s, e) =>
            {
                double[] dataX = { 1, 2, 3, 4, 5 };
                double[] dataY = { 1, 4, 9, 16, 25 };
                WpfPlot1.Plot.Add.Scatter(dataX, dataY);
                WpfPlot1.Refresh();
                
            };
        }

        /// <summary>
        /// This method will read the csv file and return a list of class with the results
        /// </summary>
        public List<CountryPopulation> ReadCsvFile()
        {
            // 
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\world_population.csv";
            
            using(StreamReader reader = new StreamReader(path))
            {
                List<CountryPopulation> country = new List<CountryPopulation>();
                try
                {
                    // Reading the csv file
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(',');

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
                            Country = values[2],
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
                        country.Add(c);
                    }
                }
                catch 
                {
                    MessageBox.Show("Unable to read file, check if there is an error in the file.", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Console.ReadLine();

                return country;
            }
        }

        // Creating the class that will contain the csv
        public class CountryPopulation
        {
            public int Rank { get; set; }
            public string CCA3 { get; set; }
            public string Country { get; set; }
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
