using System;
using System.Threading;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using LocalFetch.Converters;

namespace LocalFetch.Windows.Animation;

// Only done for Loading status
public class StatusBarAnimation
{
    private readonly Border _loadingBorder;
    private CancellationTokenSource _animationTokenSource;
    private SolidColorBrush? _initialColor;

    public StatusBarAnimation(Border loadingBorder)
    {
        _loadingBorder = loadingBorder ?? throw new ArgumentNullException(nameof(loadingBorder));
    }
    
    public void StartLoadingAnimation()
    {
        if (_loadingBorder == null || _animationTokenSource != null)
            return;

        // Save the initial color of the border before starting the animation
        _initialColor = _loadingBorder.Background as SolidColorBrush;

        _animationTokenSource = new CancellationTokenSource();
        var token = _animationTokenSource.Token;

        string keyframeMainColor = "#ffb300";
        string keyframeTransitionColor = "#ffa200";

        // Create the color animation with smooth transitions
        var animation = new Avalonia.Animation.Animation
        {
            // I want to change this to 3 seconds, it's such a small detail, I'm not final
            Duration = TimeSpan.FromSeconds(1),
            Easing = new SineEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = 
                    {
                        new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.Parse(keyframeMainColor))) // Initial color
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.5),
                    Setters = 
                    {
                        new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.Parse(keyframeTransitionColor))) // Target color
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = 
                    {
                        new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.Parse(keyframeMainColor))) // Reset to initial color
                    }
                }
            }
        };

        RunLoopingAnimation(animation, token);
    }

    public void StopLoadingAnimation(EApplicationStatus status)
    {
        _animationTokenSource?.Cancel();
        _animationTokenSource?.Dispose();
        _animationTokenSource = null;

        // Apply the status color based on the current status
        ApplyStatusColor(status);
    }

    // Get the status color from the StatusColorConverter
    private void ApplyStatusColor(EApplicationStatus status)
    {
        var converter = new StatusColorConverter();
        _loadingBorder.Background = (Brush)converter.Convert(status, typeof(Brush), null, null);
    }

    private async void RunLoopingAnimation(Avalonia.Animation.Animation animation, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await animation.RunAsync(_loadingBorder, token);
            }
        }
        catch (Exception e)
        {
        }
    }
}
