using System.Collections.Generic;
using System.Linq;
using Serilog;
using Streamer.bot.Plugin.Interface;

class CamRefresh : CPHInlineBase
{
    // delay
    public bool Execute()
    {
        int delay = 3000;
        if (CPH.TryGetArg<int>("TimedActionDelay", out var arg))
        {
            CPH.LogDebug("Found TimedActionDelay arg");
            delay = arg;
        }

        if (CPH.TryGetArg<string>("triggerName", out var triggerName))
        {
            if (
                string.Equals(
                    "Timed Actions",
                    triggerName,
                    System.StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                CPH.LogDebug($"Action triggered by a timed action delaying for {delay}ms");
		CPH.SetArgument("DelayDebug",delay);
                CPH.Wait(delay);
            }
        }
        return true;
    }
}
