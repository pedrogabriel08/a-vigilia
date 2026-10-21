using UnityEngine;

namespace Vigilia
{
    public sealed class ScareFeedback : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private GameObject panel;
        [SerializeField, Min(.1f)] private float duration=1f;
        private float remaining;
        private void OnEnable(){if(game!=null)game.StateChanged+=OnState;}
        private void OnDisable(){if(game!=null)game.StateChanged-=OnState;if(panel!=null)panel.SetActive(false);}
        private void OnState(GameState state){remaining=state==GameState.Defeat?duration:0f;panel.SetActive(remaining>0f);}
        private void Update(){if(remaining<=0f)return;remaining-=Time.unscaledDeltaTime;if(remaining<=0f)panel.SetActive(false);}
    }
}
