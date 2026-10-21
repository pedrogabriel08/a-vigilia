using UnityEngine;
using UnityEngine.UI;

namespace Vigilia
{
    // Informação só de desenvolvimento: não é um alerta da interface final.
    public sealed class AttackDebugView : MonoBehaviour
    {
        [SerializeField] private AttackManager attacks;
        [SerializeField] private Text statusText;

        private void Update()
        {
            if (!Debug.isDebugBuild || attacks == null || statusText == null)
                return;

            string prefix = "DESENVOLVIMENTO · ";
            if (!attacks.IsRunning)
                statusText.text = prefix + "AGUARDANDO PARTIDA";
            else if (attacks.CurrentAttack == null)
                statusText.text = prefix + $"PRÓXIMO EVENTO EM {Mathf.CeilToInt(attacks.IntervalRemaining)}s";
            else if (attacks.CurrentAttack.Phase == AttackPhase.PreAttack)
                statusText.text = prefix + $"PRÉ-ATAQUE {Mathf.CeilToInt(attacks.PreAttackRemaining)}s";
            else statusText.text = prefix + "ATAQUE EM ANDAMENTO";
        }
    }
}
