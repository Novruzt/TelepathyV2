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

            // Notification after clicking
            TextObject textObject = new TextObject("{=TelepathyV2_Hero_Will_Talk}{HeroName} will talk to you soon...", null);
            textObject.SetTextVariable("HeroName", this._hero.Name);

            // Refresh UI properties
            base.OnPropertyChanged("WillNotTalk");
            base.OnPropertyChanged("CallToTalkText");
        }

        [DataSourceProperty]
        public string CallToTalkText => this._btnText;

        [DataSourceProperty]
        public int TalkCost
        {
            get
            {
                if (HeroHelper.Settings.PigeonPostMode && HeroHelper.Settings.PigeonCostsMoney)
                {
                    return HeroHelper.CalculatePigeonCost(this._hero);
                }
                return 0;
            }
        }

        [DataSourceProperty]
        public bool IsCostVisible => TalkCost > 0;

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
                // 1. Check if hero is already in the call queue
                if (TelepathyBehaviour.IsAlreadyQueued(this._hero))
                    return false;

                // 2. If pigeon cost is active, check if the player has enough gold
                if (HeroHelper.Settings.PigeonPostMode && HeroHelper.Settings.PigeonCostsMoney)
                {
                    int cost = HeroHelper.CalculatePigeonCost(this._hero);
                    if (Hero.MainHero.Gold < cost)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}