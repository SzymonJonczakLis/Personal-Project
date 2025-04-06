using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private GameObject basicSpell;
    [SerializeField]
    private Transform playerEyes;

    private bool isSpellReady = true;
    private float spellCooldown = .5f;

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput() {
        // cast a spell
        if(Input.GetMouseButton(0) && isSpellReady) {
            isSpellReady = false;
            CastSpell(basicSpell);

            Invoke("ResetSpellCooldown", spellCooldown);
        }
    }

    private void CastSpell(GameObject spell) {
        Instantiate(spell, playerEyes.position + playerEyes.forward, playerEyes.rotation);
    }

    private void ResetSpellCooldown() {
        isSpellReady = true;
    }
}
