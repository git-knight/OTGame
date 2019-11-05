using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    InputField inputField;
    GameObject content;

    void Start()
    {
        inputField = transform.Find("InputField").GetComponent<InputField>();
        content = transform.Find("Scroll View/Viewport/Content").gameObject;

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(SendMessage);

        foreach (Transform obj in content.transform)
            if (obj.gameObject.activeSelf)
                Destroy(obj.gameObject);
    }

    void SendMessage()
    {
        if (inputField.text != "")
            GameHub.Invoke("SendChatMessage", inputField.text);

        inputField.text = "";
    }

    public void ReceiveMessage(JSONObject message)
    {
        var msg = Instantiate(content.transform.Find("message"), content.transform, false);
        msg.gameObject.SetActive(true);
        msg.GetComponent<Text>().text = "<color=green>" + message["sender"].AsString + "</color>: " + message["message"].AsString;
    }
}
