using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommonPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private TextMeshProUGUI button1Text;
    [SerializeField] private TextMeshProUGUI button2Text;

    private AnimatedButton animatedButton1;
    private AnimatedButton animatedButton2;
    private Action button1Callback;
    private Action button2Callback;

    private void Awake()
    {
        animatedButton1 = button1.GetComponent<AnimatedButton>();
        animatedButton2 = button2.GetComponent<AnimatedButton>();

        animatedButton1.onClick.AddListener(OnButton1Click);
        animatedButton2.onClick.AddListener(OnButton2Click);
    }

    private void OnButton1Click() => button1Callback?.Invoke();
    private void OnButton2Click() => button2Callback?.Invoke();

    public void ShowPopup(string title,
                        string btn1Text, Action btn1Action,
                        string btn2Text, Action btn2Action)
    {
        titleText.text = title;

        button1Text.text = btn1Text;
        button1Callback = btn1Action;

        button2Text.text = btn2Text;
        button2Callback = btn2Action;

        gameObject.SetActive(true);
    }

    public void HidePopup()
    {
        gameObject.SetActive(false);
        button1Callback = null;
        button2Callback = null;
    }
}