using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPage : MonoBehaviour
{
    public MyButton StartButton;
    public MyButton EndButton;


    private void Start()
    {
        StartButton.OnDoubleClick.AddListener(OnStartButtonDoubleClick);
        EndButton.OnDoubleClick.AddListener(OnEndButtonDoubleClick);
    }

    private void OnStartButtonDoubleClick()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnEndButtonDoubleClick()
    {
        Application.Quit();
    }
}
