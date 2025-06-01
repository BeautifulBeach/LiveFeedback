using System;
using LiveFeedback.Models;
using LiveFeedback.Services;
using LiveFeedback.Shared.Models;

namespace LiveFeedback.Core;

public static class Calculator
{
    // This is the point where calculation of sensitivity settings happens
    public static ComprehensibilityInformation CalculateComprehensibilityWithSensitivity(ComprehensibilityInformation info, AppState appState)
    {
        // how many individual ratings we have
        int count = info.IndividualRatings?.Length ?? 0;

        // maximum relative boost at count == 0:
        //   Low    =  0%
        //   Medium = +20%
        //   High   = +40%
        double maxBoost = appState.Sensitivity switch
        {
            Sensitivity.Low => 0.0,
            Sensitivity.Medium => 0.2,
            Sensitivity.High => 0.40,
            _ => 0.0
        };

        // threshold at which boost has dropped to 50%
        const double threshold = 10.0;

        // diminishing factor: at count=0 → 1.0, at count=threshold → 0.5, →0 as count→∞
        double diminishingFactor = threshold / (threshold + count);

        // final amplification factor around the midpoint (1.0 = no change)
        double amplification = 1.0 + maxBoost * diminishingFactor;

        // compute offset from neutral midpoint (50), amplify it, then re-center
        double offset = info.OverallRating - 50.0;
        double scaledOffset = offset * amplification;
        double adjusted = 50.0 + scaledOffset;

        // clamp into [0…100] and round
        adjusted = Math.Max(0.0, Math.Min(100.0, adjusted));
        info.OverallRating = (ushort)Math.Round(adjusted);

        return info;
    }
}