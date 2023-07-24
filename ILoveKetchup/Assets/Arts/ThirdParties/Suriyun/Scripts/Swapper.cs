using UnityEngine;
using System.Collections;

public class Swapper : MonoBehaviour
{

    public GameObject[] character;
    int m_Index;
    void Awake()
    {
        foreach (GameObject c in character)
        {
            c.SetActive(false);
        }

        m_Index = Random.Range(0, character.Length);
        character[m_Index].SetActive(true);
    }

    public Vector3 CharacterPosition()
    {
        return character[m_Index].transform.position;
    }
}
