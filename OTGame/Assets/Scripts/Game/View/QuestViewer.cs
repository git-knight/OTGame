using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum QuestStatus
{
    None,
    Available,
    Completed
}

public class QuestViewer : MonoBehaviour
{
    public GameObject QuestButtonPrefab;

    string currentQuestId;
    QuestStatus currentQuestStatus;

    GameObject questList;
    GameObject questDialogue;

    void Start()
    {
        transform.Find("background/QuestDialogue/Text2_Ok").GetComponent<Button>().onClick.AddListener(() => { ChooseAnswer(1); });
        transform.Find("background/QuestDialogue/Text2_No").GetComponent<Button>().onClick.AddListener(() => { ChooseAnswer(2); });

        UICommon.FadeIn(gameObject);
    }

    public void Initialize(JSONNode quests)
    {
        questList = transform.Find("background/QuestList").gameObject;
        questDialogue = transform.Find("background/QuestDialogue").gameObject;

        for (int i = questList.transform.childCount; i > 1; i--)
            Destroy(questList.transform.GetChild(1).gameObject);

        var text1Loc = transform.Find("background/QuestDialogue/Text1").localPosition;

        int questIndex = 0;
        foreach (var quest in quests.AsArray)
        {
            var questButton = Instantiate(QuestButtonPrefab, questList.transform, false);
            questButton.name = "Quest [" + quest["title"].AsString + "]";
            questButton.transform.Find("QuestTitle").GetComponent<Text>().text = quest["title"].AsString;

            questButton.transform.localPosition = new Vector3(text1Loc.x, text1Loc.y - 30 * questIndex, 0);

            questButton.GetComponent<Button>().onClick.AddListener(() => { ChooseQuest(quest); });

            questIndex++;
        }

        questList.SetActive(true);
        questDialogue.SetActive(false);
    }

    public void ChooseQuest(JSONNode quest)
    {
        questList.SetActive(false);
        questDialogue.SetActive(true);

        currentQuestId = quest["id"].AsString;
        questDialogue.transform.Find("Text_QuestName").GetComponent<Text>().text = quest["title"].AsString;

        currentQuestStatus = (QuestStatus)quest["status"].AsInt.Value;

        var options = quest["dialogues"].AsString.Split('$');
        var texts = options[currentQuestStatus == QuestStatus.Completed ? 1 : 0].Split('|');
        questDialogue.transform.Find("Text1").GetComponent<Text>().text = texts[0];

        if (currentQuestStatus == QuestStatus.Available)
        {
            questDialogue.transform.Find("Text2_Ok").gameObject.SetActive(true);
            questDialogue.transform.Find("Text2_Ok").GetComponent<Text>().text = texts[1];
            questDialogue.transform.Find("Text2_No").GetComponent<Text>().text = texts[2];
        }
        else
        {
            questDialogue.transform.Find("Text2_Ok").gameObject.SetActive(false);
            questDialogue.transform.Find("Text2_No").GetComponent<Text>().text = texts[1];
        }
    }

    public void ChooseAnswer(int answer)
    {
        questDialogue.SetActive(false);
        questList.SetActive(false);
        if (answer == 2 && currentQuestStatus == QuestStatus.Available)
        {
            questList.SetActive(true);
            return;
        }

        GameHub.Invoke("ProceedQuest", currentQuestId);
    }
}
