using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    [SerializeField] private Button play;

    void Start()
    {
        play.onClick.AddListener(SubmitForm);
    }

    private void SubmitForm()
    {
        SceneManager.LoadScene("Choose Category");
    }
}