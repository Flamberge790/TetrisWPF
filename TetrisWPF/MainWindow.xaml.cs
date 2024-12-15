using System;
using System.Globalization;
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

namespace TetrisWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TetrisModel Model { get; set; }

        public MainWindow()
        {
            Model = new TetrisModel();
            InitializeComponent();
            MainFrame.Navigate(new MainMenu());
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    public class PolygonPointConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                if (values.Length != 2 ||
                    !double.TryParse(values[0]?.ToString(), out double width) ||
                    !double.TryParse(values[1]?.ToString(), out double height))
                {
                    return null;
                }

            return new PointCollection([new Point(0, 0), new Point(width, 0), new Point(0, height)]);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    public class PolygonPointConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 ||
                !double.TryParse(values[0]?.ToString(), out double width) ||
                !double.TryParse(values[1]?.ToString(), out double height))
            {
                return null;
            }


            return new PointCollection([new Point(width, height), new Point(width, 0), new Point(0, height)]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}