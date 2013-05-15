using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for VSSCreen.xaml
    /// </summary>
    public partial class VSTable : UserControl
    {
        private VolumeStore _volumeStore;
        private int _interval;
        private DateTime _startDate;
        private DateTime _endDate;

        public VSTable(VolumeStore volumeStore, int interval, DateTime startDate, DateTime endDate)
        {
            // TODO: Complete member initialization
            this._volumeStore = volumeStore;
            this._interval = interval;
            this._startDate = startDate;
            this._endDate = endDate;
            InitializeComponent();

            table.ItemsSource = Volume.GetVolumeData();
        }
    }

    public class Volume
    {
        public int min00 {get; set;}
        public int min01 {get; set;}
        public int min02 {get; set;}
        public int min03 {get; set;}
        public int min04 {get; set;}
        public int min05 {get; set;}
        public int min06 {get; set;}
        public int min07 {get; set;}
        public int min08 {get; set;}
        public int min09 {get; set;}

        public Volume()
        {
            Random rnd = new Random();
            this.min00 = rnd.Next();
            this.min01 = rnd.Next();
            this.min02 = rnd.Next();
            this.min03 = rnd.Next();
            this.min04 = rnd.Next();
            this.min05 = rnd.Next();
            this.min06 = rnd.Next();
            this.min07 = rnd.Next();
            this.min08 = rnd.Next();
            this.min09 = rnd.Next();
        }

        public static List<Volume> GetVolumeData()
        {
            List<Volume> volumes = new List<Volume>(new Volume[4] {
                new Volume(), 
                new Volume(),
                new Volume(),
                new Volume()
            });
            return volumes;
        }

    }
}