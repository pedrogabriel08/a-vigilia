using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    public sealed class DoorAttack : AttackEvent
    {
        [SerializeField] private QTEManager qte;
        [SerializeField] private PrototypeAudio audioFeedback;
        [SerializeField] private Text visitorText;
        [SerializeField] private GameObject motherVisual;
        [SerializeField, Range(0f,1f)] private float witchChance=.6f;
        [SerializeField, Min(.1f)] private float holdDuration=4f;
        [SerializeField, Min(.1f)] private float weakenedHoldDuration=2.5f;
        [SerializeField, Min(.1f)] private float responseTime=7f;
        [SerializeField, Min(0f)] private float damagePerSecond=5f;
        private bool witch, weakened, resisting, visiting;
        private float visitRemaining;
        protected override void OnPreAttackStarted()
        {
            witch=Random.value<witchChance;weakened=resisting=visiting=false;
            if(visitorText!=null)visitorText.text="Tudo bem aí dentro?\nVou entrar.";
            if(audioFeedback!=null)audioFeedback.Knock();
        }
        public void RevealWithBroom()
        {
            if(Phase!=AttackPhase.PreAttack||!witch)return;
            weakened=true;
            if(audioFeedback!=null)audioFeedback.Agony();
            if(visitorText!=null)visitorText.text="Um gemido vem\nde trás da porta...";
        }
        public void BeginResistance()
        {
            if(Phase!=AttackPhase.PreAttack||Game==null||!Game.IsPlaying)return;
            resisting=true;BeginAttack();
        }
        protected override void OnAttackStarted()
        {
            if(Game.Baby.IsProtected){Finish();return;}
            if(!resisting){if(witch)Invade();else Visit();return;}
            if(qte==null||!qte.StartHoldQTE(witch&&weakened?weakenedHoldDuration:holdDuration,responseTime,OnResult,Damage))Finish();
        }
        private void Damage(float seconds)
        {if(witch&&Game!=null&&Game.IsPlaying)Game.Baby.TakeDamage(damagePerSecond*seconds);}
        private void OnResult(QTEResult result)
        {
            if(Phase!=AttackPhase.Active)return;
            if(result==QTEResult.Failure){if(witch)Invade();else Visit();}
            else Finish();
        }
        private void Invade()
        {
            if(Game!=null&&Game.IsPlaying)Game.Baby.TakeDamage(Game.Baby.MaxHealth);
            Finish();
        }
        private void Visit()
        {
            visiting=true;visitRemaining=1.5f;
            if(motherVisual!=null)motherVisual.SetActive(true);
            if(visitorText!=null)visitorText.text="Está tudo bem.\nJá volto, meu filho.";
        }
        private void Update()
        {
            if(!visiting||Phase!=AttackPhase.Active)return;
            visitRemaining-=Time.deltaTime;if(visitRemaining<=0f)Finish();
        }
        protected override void OnCleanup()
        {
            if(qte!=null)qte.Cancel();visiting=false;
            if(visitorText!=null)visitorText.text="";
            if(motherVisual!=null)motherVisual.SetActive(false);
        }
    }
}
