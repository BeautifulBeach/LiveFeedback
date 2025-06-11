using System;
using Avalonia.Threading;

namespace LiveFeedback.Core;

public class WindowTools
{
    public static Action Throttle(TimeSpan delay, Action action)
    {
        DispatcherTimer? timer = null;

        return () =>
        {
            if (timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = delay
                };
                timer.Tick += (s, e) =>
                {
                    timer!.Stop();
                    action();
                };
            }

            timer.Stop();
            timer.Start();
        };
    }
}