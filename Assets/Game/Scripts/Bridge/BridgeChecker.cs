using UnityEngine;
using System.Collections;

public class BridgeChecker : MonoBehaviour
{
    [SerializeField] private Bridge bridge;
    [SerializeField] private float buildDelay = 0.08f;

    private Character currentBuilder;
    private bool isBuilding;

    public void HandleCharacter(Character character)
    {
        if (bridge.IsFull()) return;

        
        if (currentBuilder == null)
        {
            currentBuilder = character;
        }

        if (currentBuilder != character) return;

        if (!isBuilding)
        {
            StartCoroutine(BuildLoop(character));
        }
    }

    private IEnumerator BuildLoop(Character character)
    {
        isBuilding = true;

        while (true)
        {
            if (character != currentBuilder) break;

            if (bridge.IsFull()) break;

            if (character.BrickCount > 0)
            {
                BuildStep(character);
            }
            else
            {
                ReleaseBuilder();

                Rigidbody rb = character.GetComponent<Rigidbody>();
                if (rb != null)
                    character.Block(rb);

                break;
            }

            yield return new WaitForSeconds(buildDelay);
        }

        isBuilding = false;
    }

    private void BuildStep(Character character)
    {
        Vector3 pos = bridge.GetBuildPosition();

        character.RemoveBrick();

        GameObject brick = SimplePool.Spawn("BridgeBrick", pos, Quaternion.identity);

        foreach (var mr in brick.GetComponentsInChildren<MeshRenderer>())
        {
            mr.material = new Material(mr.material);
            mr.material.color = character.characterColor;
        }

        bridge.NextStep();
        
        transform.position = bridge.GetBuildPosition();
    }

    public void ReleaseBuilder()
    {
        currentBuilder = null;
    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if (character != null && character == currentBuilder)
        {
            ReleaseBuilder();
        }
    }
}