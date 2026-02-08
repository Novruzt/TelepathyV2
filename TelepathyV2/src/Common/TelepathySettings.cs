using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TelepathyV2
{
    public sealed class TelepathySettings : AttributeGlobalSettings<TelepathySettings>
    {
        public override string DisplayName => "TelepathyV2";
        public override string FolderName => "TelepathyV2";
        public override string Id => "TelepathyV2";
        public override string FormatType => "json2";

        // =========================
        // Realism / Rules
        // =========================

        [SettingPropertyGroup("Distant conversation realism", GroupOrder = (int)GroupOrder.DistantConversationRealism)]
        [SettingPropertyBool("Prevent talking to dead",
            HintText = "Prevents distant conversations with dead heroes.",
            Order = 0, RequireRestart = false)]
        public bool PreventTalkingToDead { get; set; } = true;

        [SettingPropertyGroup("Distant conversation realism", GroupOrder = (int)GroupOrder.DistantConversationRealism)]
        [SettingPropertyBool("Prevent talking to heroes you haven't met",
            HintText = "Prevents distant conversations with heroes you have never met.",
            Order = 1, RequireRestart = false)]
        public bool PreventTalkingToHeroesHaveNotMetBefore { get; set; } = false;

        [SettingPropertyGroup("Distant conversation realism", GroupOrder = (int)GroupOrder.DistantConversationRealism)]
        [SettingPropertyBool("Hide quest dialog lines",
            HintText = "Hides quest-related conversation lines during distant conversations.",
            Order = 2, RequireRestart = false)]
        public bool HideQuestDialogLines { get; set; } = false;

        // =========================
        // Relationship Gate
        // =========================

        [SettingPropertyGroup("Relationship requirement", GroupOrder = (int)GroupOrder.RelationshipRequirement)]
        [SettingPropertyBool("Require minimum relationship",
            HintText = "If enabled, heroes below the minimum relationship value will refuse distant conversations.",
            Order = 0, RequireRestart = false)]
        public bool RequireMinimumRelationship { get; set; } = false;


        [SettingPropertyGroup("Relationship requirement", GroupOrder = (int)GroupOrder.RelationshipRequirement)]
        [SettingPropertyInteger("Minimum relationship to talk", -100, 100,
           HintText = "Heroes below this relationship value will refuse distant conversations.",
           Order = 1, RequireRestart = false)]
        public int MinimumRelationshipToTalk { get; set; } = 0;

        [SettingPropertyGroup("Relationship requirement", GroupOrder = (int)GroupOrder.RelationshipRequirement)]
        [SettingPropertyBool("Apply relationship rule to pigeon post",
            HintText = "If disabled, relationship gate applies only to direct telepathy calls.",
            Order = 2, RequireRestart = false)]
        public bool ApplyRelationshipRuleToPigeon { get; set; } = false;

        // =========================
        // Pigeon Post mode
        // =========================

        [SettingPropertyGroup("Pigeon post mode", GroupOrder = (int)GroupOrder.PigeonPostMode)]
        [SettingPropertyBool("Enable pigeon post mode",
            HintText = "Message delivery takes time depending on distance.",
            Order = 0, RequireRestart = false)]
        public bool PigeonPostMode { get; set; } = false;

        [SettingPropertyGroup("Pigeon post mode", GroupOrder = (int)GroupOrder.PigeonPostMode)]
        [SettingPropertyInteger("Minimum delay (hours)", 0, 168,
            HintText = "Minimum waiting time before a call can become ready.",
            Order = 1, RequireRestart = false)]
        public int MinDelayHours { get; set; } = 0;

        [SettingPropertyGroup("Pigeon post mode", GroupOrder = (int)GroupOrder.PigeonPostMode)]
        [SettingPropertyInteger("Pigeon speed per hour", 1, 300,
            HintText = "Higher value means faster delivery.",
            Order = 2, RequireRestart = false)]
        public int PigeonSpeedPerHour { get; set; } = 30;
    }
}
