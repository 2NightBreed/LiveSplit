using System;
using System.Globalization;

namespace LiveSplit.TimeFormatters;

public class GeneralTimeFormatter : ITimeFormatter
{
    static readonly private CultureInfo ic = CultureInfo.InvariantCulture;

    public TimeAccuracy Accuracy { get; set; }

    public DigitsFormat DigitsFormat { get; set; }

    /// <summary>
    /// How to display null times
    /// </summary>
    public NullFormat NullFormat { get; set; }

    /// <summary>
    /// If true, for example show "1d 23:59:10" instead of "47:59:10". For durations of 24 hours or more, 
    /// </summary>
    public bool ShowDays { get; set; }

    /// <summary>
    /// If true, include a "+" for positive times (excluding zero)
    /// </summary>
    public bool ShowPlus { get; set; }

    /// <summary>
    /// If true, don't display decimals if absolute time is 1 minute or more
    /// </summary>
    public bool DropDecimals { get; set; }

    /// <summary>
    /// If true, don't display trailing zero decimal places
    /// </summary>
    public bool AutomaticPrecision { get; set; } = false;

    /// <summary>
    /// If true, uses a custom formatter on delta times
    /// The custom formatting rules are as follows:
    /// If delta time is between -10 seconds ahead to +10 seconds behind, display hundredths place                                                      ie. +4.67
    /// If delta time is between -60 seconds ahead to -10 seconds ahead or between +60 seconds behind to +10 seconds behind, display tenths place       ie. -34.7
    /// If delta time is anything else, display no decimal place                                                                                        ie. +3:54
    /// This formatter essentially keeps delta times the same length by omitting decimal places from the time as it either increases or decreases
    /// </summary>
    public bool UseCustomDeltaTimeFormatter { get; set; }

    public GeneralTimeFormatter()
    {
        DigitsFormat = DigitsFormat.SingleDigitSeconds;
        NullFormat = NullFormat.Dash;
    }

    public string Format(TimeSpan? timeNullable)
    {
        bool isNull = !timeNullable.HasValue;
        if (isNull)
        {
            if (NullFormat == NullFormat.Dash)
            {
                return TimeFormatConstants.DASH;
            }
            else if (NullFormat == NullFormat.ZeroWithAccuracy)
            {
                return ZeroWithAccuracy();
            }
            else if (NullFormat == NullFormat.ZeroDotZeroZero)
            {
                return "0.00";
            }
            else if (NullFormat is NullFormat.ZeroValue or NullFormat.Dashes)
            {
                timeNullable = TimeSpan.Zero;
            }
        }

        TimeSpan time = timeNullable.Value;

        string minusString;
        if (time == TimeSpan.Zero)
        {
            minusString = "";
        }
        else if (time < TimeSpan.Zero)
        {
            minusString = TimeFormatConstants.MINUS;
            time = -time;
        }
        else
        {
            minusString = ShowPlus ? "+" : "";
        }

        string decimalFormat = "";
        if (AutomaticPrecision)
        {
            double totalSeconds = time.TotalSeconds;
            if (Accuracy == TimeAccuracy.Seconds || totalSeconds % 1 == 0)
            {
                decimalFormat = "";
            }
            else if (Accuracy == TimeAccuracy.Tenths || 10 * totalSeconds % 1 == 0)
            {
                decimalFormat = @"\.f";
            }
            else if (Accuracy == TimeAccuracy.Hundredths || 100 * totalSeconds % 1 == 0)
            {
                decimalFormat = @"\.ff";
            }
            else
            {
                decimalFormat = @"\.fff";
            }
        }
        else
        {
            decimalFormat = GetDecimalFormat(time);
        }

        string formatted;
        if (time.TotalDays >= 1)
        {
            if (ShowDays)
            {
                formatted = minusString + time.ToString(@"d\d\ " + (DigitsFormat == DigitsFormat.DoubleDigitHours ? "hh" : "h") + @"\:mm\:ss" + decimalFormat, ic);
            }
            else
            {
                formatted = minusString + (int)time.TotalHours + time.ToString(@"\:mm\:ss" + decimalFormat, ic);
            }
        }
        else if (DigitsFormat == DigitsFormat.DoubleDigitHours)
        {
            formatted = minusString + time.ToString(@"hh\:mm\:ss" + decimalFormat, ic);
        }
        else if (time.TotalHours >= 1 || DigitsFormat == DigitsFormat.SingleDigitHours)
        {
            formatted = minusString + time.ToString(@"h\:mm\:ss" + decimalFormat, ic);
        }
        else if (DigitsFormat == DigitsFormat.DoubleDigitMinutes)
        {
            formatted = minusString + time.ToString(@"mm\:ss" + decimalFormat, ic);
        }
        else if (time.TotalMinutes >= 1 || DigitsFormat == DigitsFormat.SingleDigitMinutes)
        {
            formatted = minusString + time.ToString(@"m\:ss" + decimalFormat, ic);
        }
        else if (DigitsFormat == DigitsFormat.DoubleDigitSeconds)
        {
            formatted = minusString + time.ToString(@"ss" + decimalFormat, ic);
        }
        else
        {
            formatted = minusString + time.ToString(@"%s" + decimalFormat, ic);
        }

        if (isNull && NullFormat == NullFormat.Dashes)
        {
            formatted = formatted.Replace('0', '-');
        }

        return formatted;
    }

    private string GetDecimalFormat(TimeSpan time)
    {
        if (UseCustomDeltaTimeFormatter)
        {
            if (time.TotalSeconds >= 60)
            {
                return "";
            }
            else if (time.TotalSeconds >= 10)
            {
                return @"\.f";
            }
            else
            {
                return @"\.ff";
            }
        }
        else
        {
            if (DropDecimals && time.TotalMinutes >= 1)
            {
                return "";
            }
            else if (Accuracy == TimeAccuracy.Seconds)
            {
                return "";
            }
            else if (Accuracy == TimeAccuracy.Tenths)
            {
                return @"\.f";
            }
            else if (Accuracy == TimeAccuracy.Hundredths)
            {
                return @"\.ff";
            }
            else if (Accuracy == TimeAccuracy.Milliseconds)
            {
                return @"\.fff";
            }
        }

        return @"\.ff";
    }

    private string ZeroWithAccuracy()
    {
        if (AutomaticPrecision || Accuracy == TimeAccuracy.Seconds)
        {
            return "0";
        }
        else if (Accuracy == TimeAccuracy.Tenths)
        {
            return "0.0";
        }
        else if (Accuracy == TimeAccuracy.Milliseconds)
        {
            return "0.000";
        }
        else
        {
            return "0.00";
        }
    }
}
