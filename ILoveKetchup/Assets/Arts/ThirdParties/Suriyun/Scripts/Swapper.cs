using UnityEngine;
using System.Collections;

public class Swapper : MonoBehaviour
{
    public GameObject[] character;
    private int m_Index = 0;
    public GameObject ActiveCharacter => this.character[m_Index];
    public GameObject HeadBone { get; private set; }
    void Awake()
    {
        m_Index = Random.Range(0, character.Length);
        SetActiveIndex(m_Index);
    }

    void SetActiveIndex(int index)
    {
        foreach (GameObject c in character)
        {
            c.SetActive(false);
        }
        try
        {
            GameObject charObj = character[m_Index];
            charObj.SetActive(true);
            HeadBone = CommonUtils.FindInChildren(charObj, "Head");
        }
        catch
        {
            Development.LogError("SetActiveIndex ERROR " + index);
        }
    }

    public Vector3 CharacterPosition()
    {
        return character[m_Index].transform.position;
    }
}
