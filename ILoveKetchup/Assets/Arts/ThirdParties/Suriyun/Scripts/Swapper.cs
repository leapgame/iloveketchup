using UnityEngine;
using System.Collections;

    public class Swapper : MonoBehaviour
    {

        public GameObject[] character;
        void Awake()
        {
            foreach (GameObject c in character)
            {
                c.SetActive(false);
            }
            character[Random.Range(0, character.Length)].SetActive(true);
        }
    }
