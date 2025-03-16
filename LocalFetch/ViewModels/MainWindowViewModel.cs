using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LocalFetch.Shared.Settings.Builds;

namespace LocalFetch.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<BuildSettings> Builds { get; } = new ();

        public MainWindowViewModel()
        {
            LoadBuilds();
        }
        
        private void LoadBuilds()
        {
            var builds = BuildSettings.LoadAll();
            foreach (var build in builds)
            {
                Builds.Add(build);
                _ = build.Save();
            }

            if (!(Builds.Count > 50))
            {
                LoadBuilds();
            }
        }
    }
}