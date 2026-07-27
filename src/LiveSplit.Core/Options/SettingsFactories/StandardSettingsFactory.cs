using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Model.Input;
using LiveSplit.UI.Components;
using LiveSplit.Web.SRL.RaceViewers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.Options.SettingsFactories;

public class StandardSettingsFactory : ISettingsFactory
{
    public ISettings Create()
    {
        return new Settings()
        {
            HotkeyProfiles = new Dictionary<string, HotkeyProfile>()
            {
                {HotkeyProfile.DefaultHotkeyProfileName, new HotkeyProfile()
                    {
                        SplitKey = new KeyOrButton(Keys.ControlKey),
                        ResetKey = new KeyOrButton(Keys.Delete),
                        UndoKey = new KeyOrButton(Keys.Left),
                        SkipKey = new KeyOrButton(Keys.Right),
                        SwitchComparisonPrevious = new KeyOrButton(Keys.Up),
                        SwitchComparisonNext = new KeyOrButton(Keys.Down),
                        PauseKey = null,
                        ToggleGlobalHotkeys = new KeyOrButton(Keys.Oemtilde),
                        GlobalHotkeysEnabled = true,
                        DeactivateHotkeysForOtherPrograms = false,
                        DoubleTapPrevention = true,
                        AllowGamepadsAsHotkeys = false,
                        HotkeyDelay = 0f
                    }
                }
            },
            WarnOnReset = true,
            LastComparison = Run.PersonalBestComparisonName,
            RaceViewer = new SRLRaceViewer(),
            AgreedToSRLRules = false,
            SimpleSumOfBest = false,
            RaceProvider = [.. ComponentManager.RaceProviderFactories.Values.ToList().Select(x => x.CreateSettings())],
            RefreshRate = 60,
            ServerPort = 16834,
            ServerStartup = ServerStartupType.Off,
            ServerState = ServerStateType.Off,
            EnableDPIAwareness = false,
            UILanguage = string.Empty,
            ComparisonGeneratorStates = new Dictionary<string, bool>()
            {
                { BestSegmentsComparisonGenerator.ComparisonName, true },
                { BestSplitTimesComparisonGenerator.ComparisonName, true },
                { AverageSegmentsComparisonGenerator.ComparisonName, true },
                { MedianSegmentsComparisonGenerator.ComparisonName, true },
                { WorstSegmentsComparisonGenerator.ComparisonName, true},
                { PercentileComparisonGenerator.ComparisonName, true },
                { LatestRunComparisonGenerator.ComparisonName, true },
                { HCPComparisonGenerator.ComparisonName, true },
                { NoneComparisonGenerator.ComparisonName, true }
            },
            HcpHistorySize = 20,
            HcpNBestRuns = 8
        };
    }
}
