namespace CoverShooter
{
    public enum Tool
    {
        none = 0,
        radio,
        phone,
        flashlight
    }

    public struct ToolUseDescription
    {
        /// <summary>
        /// Signifies if a character needs to aim while using the tool.
        /// </summary>
        public bool HasAiming;

        /// <summary>
        /// Is the use animation continuous instead of a single action.
        /// </summary>
        public bool IsContinuous;

        public ToolUseDescription(bool hasAiming, bool isContinuous)
        {
            HasAiming = hasAiming;
            IsContinuous = isContinuous;
        }
    }

    public struct ToolDescription
    {
        /// <summary>
        /// Settings for tool main use.
        /// </summary>
        public ToolUseDescription Main;

        /// <summary>
        /// Settings for tool alternate use.
        /// </summary>
        public ToolUseDescription Alternate;

        public static ToolDescription[] Defaults = GetDefaults();

        public ToolDescription(ToolUseDescription main)
        {
            Main = main;
            Alternate = new ToolUseDescription(false, false);
        }

        public ToolDescription(ToolUseDescription main, ToolUseDescription alternate)
        {
            Main = main;
            Alternate = alternate;
        }

        public bool HasAiming(bool isAlternate)
        {
            return isAlternate ? Alternate.HasAiming : Main.HasAiming;
        }

        public bool IsContinuous(bool isAlternate)
        {
            return isAlternate ? Alternate.IsContinuous : Main.IsContinuous;
        }

        public static ToolDescription[] GetDefaults()
        {
            var descriptions = new ToolDescription[4];

            descriptions[1] = new ToolDescription(new ToolUseDescription(false, false));
            descriptions[2] = new ToolDescription(new ToolUseDescription(true, true), new ToolUseDescription(false, false));
            descriptions[3] = new ToolDescription(new ToolUseDescription(true, true));

            return descriptions;
        }
    }
}
