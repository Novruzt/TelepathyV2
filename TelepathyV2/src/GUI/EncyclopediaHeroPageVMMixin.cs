using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TelepathyV2
{
    [ViewModelMixin("RefreshValues")]
    public sealed class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private readonly Hero _hero;
        private readonly string _btnText;

        public EncyclopediaHeroPageVMMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            _hero = vm.Obj as Hero;
            _btnText = new TextObject("{=TelepathyV2_TalkToMe}Talk to me!").ToString();
        }

        [DataSourceMethod]
        public void CallToTalk()
        {
            if (_hero == null)
                return;

            TelepathyBehaviour.CallToTalk(_hero);

            var msg = new TextObject("{=TelepathyV2_Queued}{HeroName} will talk to you soon...");
            msg.SetTextVariable("HeroName", _hero.Name);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString()));

            OnPropertyChanged(nameof(WillNotTalk));
        }

        [DataSourceProperty]
        public string CallToTalkText => _btnText;

        [DataSourceProperty]
        public bool CanTalkTo => _hero != null && _hero.CanTalkTo();

        [DataSourceProperty]
        public bool WillNotTalk => _hero != null && TelepathyBehaviour.IsAlreadyQueued(_hero);
    }
}
