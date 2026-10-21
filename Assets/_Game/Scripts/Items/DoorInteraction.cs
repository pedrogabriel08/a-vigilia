using UnityEngine;
using UnityEngine.EventSystems;

namespace Vigilia
{
    public sealed class DoorInteraction : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private DoorAttack door;
        public void OnPointerClick(PointerEventData e)
        {if(e.button==PointerEventData.InputButton.Left&&door!=null)door.BeginResistance();}
    }
}
