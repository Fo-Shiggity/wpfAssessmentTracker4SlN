/* Title: Wpf Assessment Tracker
 * 
 * Description: A simple wpf application that allows users to add and delete assessments to keep track of them
 * Assessment Tracker will allow user to input due date,Assessment Name, Unit Name and Assessment Type 
 * 
 * 
 * Author: Victoria Veltman
 */



using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfAssessmentTracker4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Assessment
    {
        public DateTime DueDate { get; set; } = DateTime.Now;
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = "NYM"; // NYM, NYS, or S
    }

    public class CompletedAssessment
    {
        public DateTime DueDate { get; set; } = DateTime.Now;
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Grade { get; set; } = "S";
        public DateTime DateCompleted { get; set; } = DateTime.Now;

    }

    public partial class MainWindow : Window
    {

        private ObservableCollection<Assessment> IncompleteAssessments = new();
        private ObservableCollection<CompletedAssessment> completedAssessments = new();

        private const string IncompleteFile = "incomplete_assessments.txt";
        private const string CompletedFile = "completed_assessments.txt";



        public MainWindow()
        {
            InitializeComponent();


            // Bind item collections to DataGrids
            Incomplete_Assessments.ItemsSource = IncompleteAssessments;
            Completed_Assessments.ItemsSource = completedAssessments;

            // Auto load on app initialization
            LoadDataFromFile();

                       
        }
    
        
        
        
        
        private void SaveDataToFile()
        {

            try
            {
                // Save Current Assessments
                using (StreamWriter writer = new StreamWriter(IncompleteFile, false))
                {
                    foreach (var item in IncompleteAssessments)
                    {
                        if(item.Grade == "S")
                        {
                            completedAssessments.Add(new CompletedAssessment
                            {
                                DueDate = item.DueDate,
                                Unit = item.Unit,
                                Type = item.Type,
                                Name = item.Name,
                                Grade = "S",
                                DateCompleted = DateTime.Now
                            });
                            continue;
                        }
                        writer.WriteLine($"{item.DueDate},{item.Unit},{item.Type},{item.Name},{item.Grade}");
                    }
                }

                // Save Completed Assessments
                using (StreamWriter writer = new StreamWriter(CompletedFile, false))
                {
                    foreach (var item in completedAssessments)
                    {
                        writer.WriteLine($"{item.DueDate},{item.Unit},{item.Type},{item.Name},{item.Grade},{item.DateCompleted:yyyy-MM-dd}");
                    }
                }
                LoadDataFromFile();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadDataFromFile()
        {
            try
            {
                IncompleteAssessments.Clear();
                completedAssessments.Clear();

                // Load Current Assessments
                if (File.Exists(IncompleteFile))
                {
                    using (StreamReader reader = new StreamReader(IncompleteFile))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');

                            if (parts.Length >= 5)
                            {
                                IncompleteAssessments.Add(new Assessment 
                                {
                                    Unit = parts[1],
                                    Type = parts[2], 
                                    Name = parts[3], 
                                    Grade = parts[4], 
                                    DueDate = DateTime.Parse(parts[0])  
                                });
                            }
                        }
                    }
                }

                // Load Completed Assessments
                if (File.Exists(CompletedFile))
                {
                    using (StreamReader reader = new StreamReader(CompletedFile))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 6)
                            {
                                completedAssessments.Add(new CompletedAssessment
                                {
                                    DueDate = DateTime.Parse(parts[0]),
                                    Unit = parts[1],
                                    Type = parts[2],
                                    Name = parts[3],
                                    Grade = parts[4],
                                    DateCompleted = DateTime.TryParse(parts[5], out var dt) ? dt : DateTime.Now
                                   
                                });
                            }
                        }
                    }
                }
                RefreshFilter();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile();
            MessageBox.Show("Data successfully saved to file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadDataFromFile();
            MessageBox.Show("Data successfully reloaded from file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

       


        private void IncompleteAssessments_Edit(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Assessment editedAssessment)
            {
                // Use Dispatcher to let the bindings update before verifying the status shift
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (editedAssessment.Grade == "S")
                    {
                        // Transfer to Completed collection
                        completedAssessments.Add(new CompletedAssessment
                        {
                            DueDate = editedAssessment.DueDate,
                            Unit = editedAssessment.Unit,
                            Type = editedAssessment.Type,
                            Name = editedAssessment.Name,
                            Grade = "S",
                            DateCompleted = DateTime.Now
                        });

                        // Delete from incomplete assessments
                        IncompleteAssessments.Remove(editedAssessment);
                        RefreshFilter();
                       
                    }
                }));
            }
        }


       

        private void FilterChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilter();
           
        }

        private void RefreshFilter()
        {
            // Apply filter to Incomplete Assessments
            ICollectionView incompleteView = CollectionViewSource.GetDefaultView(IncompleteAssessments);
            if (incompleteView != null)
            {
                incompleteView.Filter = AssessmentFilter;
            }

        
        }
        private bool AssessmentFilter(object item)
        {
            if (item is not Assessment assessment) return false;

            bool matchesUnit = string.IsNullOrEmpty(TxtFilterUnit.Text) || assessment.Unit.Contains(TxtFilterUnit.Text, StringComparison.OrdinalIgnoreCase);
            bool matchesType = string.IsNullOrEmpty(TxtFilterType.Text) || assessment.Type.Contains(TxtFilterType.Text, StringComparison.OrdinalIgnoreCase);
            bool matchesName = string.IsNullOrEmpty(TxtFilterName.Text) || assessment.Name.Contains(TxtFilterName.Text, StringComparison.OrdinalIgnoreCase);

            return matchesUnit && matchesType && matchesName;
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            TxtFilterUnit.Clear();
            TxtFilterType.Clear();
            TxtFilterName.Clear();
            RefreshFilter();
            
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Incomplete_Assessments.CommitEdit(DataGridEditingUnit.Row, true);

            SaveDataToFile();
          

                // Display a dialog box with Yes and No buttons
                MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Close",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // If the user clicks No, cancel the closing process
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}