using UnityEngine;

namespace Vigilia
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PrototypeAudio : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        private AudioSource source;
        private AudioClip knock, agony, victory, defeat;
        private void Awake()
        {
            source=GetComponent<AudioSource>();source.playOnAwake=false;source.volume=.25f;
            knock=Tone("Knock",.5f,100f,true);agony=Tone("AgonyPlaceholder",.9f,170f,false);
            victory=Tone("Victory",.8f,650f,false);defeat=Tone("Defeat",.9f,65f,false);
        }
        private void OnEnable(){if(game!=null)game.StateChanged+=StateChanged;}
        private void OnDisable(){if(game!=null)game.StateChanged-=StateChanged;}
        private void StateChanged(GameState state)
        {if(state==GameState.Victory)Play(victory);else if(state==GameState.Defeat)Play(defeat);}
        public void Knock()=>Play(knock);
        public void Agony()=>Play(agony);
        private void Play(AudioClip clip){if(source!=null&&clip!=null)source.PlayOneShot(clip);}
        private static AudioClip Tone(string name,float duration,float frequency,bool pulses)
        {
            int rate=22050;var data=new float[Mathf.CeilToInt(duration*rate)];
            for(int i=0;i<data.Length;i++)
            {
                float t=(float)i/rate;
                float envelope=pulses?Mathf.Exp(-35f*(t%.18f)):Mathf.Sin(Mathf.PI*t/duration)*.35f;
                data[i]=Mathf.Sin(2f*Mathf.PI*frequency*t+Mathf.Sin(t*35f)*2f)*envelope*.5f;
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        private void OnDestroy(){foreach(var clip in new[]{knock,agony,victory,defeat})if(clip!=null)Destroy(clip);}
    }
}
