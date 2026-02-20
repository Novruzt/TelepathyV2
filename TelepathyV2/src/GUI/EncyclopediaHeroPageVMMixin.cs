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
            this._hero = (vm.Obj as Hero);
            this._btnText = new TextObject("{=TelepathyV2_Talk_To_Me}Talk to me!", null).ToString();
            vm.RefreshValues();
        }

        [DataSourceMethod]
        public void CallToTalk()
        {
            TelepathyBehaviour.CallToTalk(this._hero);
            TextObject textObject = new TextObject("{=TelepathyV2_Hero_Will_Talk}{HeroName} will talk to you soon...", null);
            textObject.SetTextVariable("HeroName", this._hero.Name);
            base.OnPropertyChanged("WillNotTalk");
        }

        [DataSourceProperty]
        public string CallToTalkText
        {
            get
            {
                return this._btnText;
            }
        }

        [DataSourceProperty]
        public bool CanTalkTo
        {
            get
            {
                return this._hero.CanTalkTo();
            }
        }

        [DataSourceProperty]
        public bool WillNotTalk
        {
            get
            {
                return !TelepathyBehaviour.IsAlreadyQueued(this._hero);
            }
        }
    }
}
