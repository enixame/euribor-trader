using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using Trading.Client.Wpf.ViewModels;
using Trading.Infrastructure.Configuration;

namespace Trading.Client.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var config = LoadConfig();
            DataContext = new MainViewModel(config);
        }

        private static TradingConfig LoadConfig()
        {
            // Attempt to find appsettings.json in the parent directory of the exe
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDir, "appsettings.json");
            if (!File.Exists(path))
            {
                // Fallback for development: look two directories up (project root)
                path = Path.Combine(baseDir, "..", "appsettings.json");
            }
            if (!File.Exists(path))
            {
                return new TradingConfig
                {
                    Products = new() { "EURIBOR-3M", "EURIBOR-6M" },
                    Venues = new()
                };
            }
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<TradingConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return config ?? new TradingConfig();
        }
    }
}