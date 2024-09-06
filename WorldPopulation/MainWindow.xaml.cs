﻿using System;
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

            List<CountryPopulation> countryPop = ReadCsvFile();            

            WpfPlot1.Refresh();

            if(countryPop != null)
            {   
                Loaded += (s, e) =>
                {
                    // Add labels in the graph
                    WpfPlot1.Plot.XLabel("Year");
                    WpfPlot1.Plot.YLabel("Population");

                    foreach (CountryPopulation country in countryPop)
                    {
                        int index = 0;
                        int[] dataX = new int[listOfYears.Count + 1];
                        listOfYears.ForEach(year =>
                        {
                            dataX[index] = year;
                            index++;
                        });

                        int[] dataY = new int[]{ country.Population2022, country.Population2020, country.Population2015, country.Population2010, country.Population2000, country.Population1990, country.Population1980, country.Population1970 };

                        WpfPlot1.Plot.Add.Scatter(dataX, dataY);
                        WpfPlot1.Plot.Axes.AutoScale();
                        WpfPlot1.Refresh();
                    }
                };
            }
        }

        /// <summary>
        /// This method will read the csv file and return a list of class with the results
        /// </summary>
        public List<CountryPopulation> ReadCsvFile()
        {
            // Path where the scv file locates (Will be removed when the implementation of client put his own csv file will be done)
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\world_population.csv";

            int counter = 0;
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

                    if (l.index == 0)
                    {
                        foreach (string value in values)
                        {
                            listOfYears.Add(Int32.Parse(value));
                        }
                        counter++;
                    }
                    else
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

        // Creating the class that will contain the csv
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
