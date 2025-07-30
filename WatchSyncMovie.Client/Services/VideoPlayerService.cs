using Microsoft.JSInterop;

namespace WatchSyncMovie.Client.Services;

public class VideoPlayerService
{
    private readonly IJSRuntime _jsRuntime;

    public VideoPlayerService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializePlayerAsync(string videoElementId, string videoUrl)
    {
        // Retry logic for DOM element availability
        const int maxRetries = 5;
        const int retryDelayMs = 200;
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var success = await _jsRuntime.InvokeAsync<bool>("videoPlayer.initialize", videoElementId, videoUrl);
                if (success)
                {
                    return; // Success, exit retry loop
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Video player initialization attempt {i + 1} failed: {ex.Message}");
            }
            
            if (i < maxRetries - 1) // Don't wait after the last attempt
            {
                await Task.Delay(retryDelayMs);
            }
        }
        
        Console.WriteLine($"Failed to initialize video player after {maxRetries} attempts for element: {videoElementId}");
        throw new InvalidOperationException($"Could not initialize video player for element '{videoElementId}' after {maxRetries} attempts. The DOM element may not be available.");
    }

    public async Task PlayAsync()
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.play");
    }

    public async Task PauseAsync()
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.pause");
    }

    public async Task SetCurrentTimeAsync(double seconds)
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.setCurrentTime", seconds);
    }

    public async Task<double> GetCurrentTimeAsync()
    {
        return await _jsRuntime.InvokeAsync<double>("videoPlayer.getCurrentTime");
    }

    public async Task<double> GetDurationAsync()
    {
        return await _jsRuntime.InvokeAsync<double>("videoPlayer.getDuration");
    }

    public async Task<bool> IsPlayingAsync()
    {
        return await _jsRuntime.InvokeAsync<bool>("videoPlayer.isPlaying");
    }

    public async Task SetVolumeAsync(double volume)
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.setVolume", volume);
    }

    public async Task<double> GetVolumeAsync()
    {
        return await _jsRuntime.InvokeAsync<double>("videoPlayer.getVolume");
    }

    public async Task SetMutedAsync(bool muted)
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.setMuted", muted);
    }

    public async Task ToggleFullscreenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.toggleFullscreen");
    }

    public async Task LoadVideoAsync(string videoUrl)
    {
        await _jsRuntime.InvokeVoidAsync("videoPlayer.loadVideo", videoUrl);
    }


} 