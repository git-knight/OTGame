using NotSoSimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestsView : MonoBehaviour
{
    List<Transform> questButtons;

    GameObject questList;
    GameObject questSelection;
    Text questDescription;

    // Start is called before the first frame update
    void Start()
    {
        UICommon.FadeIn(gameObject);
    }


    public void Initialize(JSONArray quests)
    {
        questList = transform.Find("background/Scroll View/Viewport/Content").gameObject;
        questSelection = transform.Find("background/Scroll View/Viewport/Selection").gameObject;
        questDescription = transform.Find("background/QuestDescription/Text").GetComponent<Text>();

        for (int i = 2; i < questList.transform.childCount; i++)
            Destroy(questList.transform.GetChild(2).gameObject);

        int questIndex = 0;
        int uncompletedQuestIndex = 0;
        var listItemTemplate = questList.transform.Find("QuestNameTemplate");

        questButtons = new List<Transform>();

        foreach (var quest in quests)
        {
            var questButton = Instantiate(listItemTemplate, questList.transform, false);
            questButton.name = "Quest [" + quest["title"].AsString + "]";
            questButton.GetComponent<Text>().text = quest["title"].AsString;

            questButton.GetComponent<Button>().onClick.AddListener(() => { ShowQuest(questIndex, quest); });

            if (quest["isCompleted"].AsBool == false)
                questButton.transform.SetSiblingIndex(uncompletedQuestIndex++);

            questButtons.Add(questButton);
            questIndex++;
        }

        listItemTemplate.gameObject.SetActive(false);

        if (questIndex > 0)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(questList.transform as RectTransform);
            ShowQuest(0, quests[0]);
        }
        else ShowNoQuests();
    }

    void ShowNoQuests()
    {
        questDescription.text = "(no quests yet)";
        questDescription.fontStyle = FontStyle.Italic;
        questDescription.color = Color.gray;

        questSelection.SetActive(false);
    }

    public void ShowQuest(int ind, JSONNode quest)
    {
        var questButton = questButtons[ind];

        var selLoc = questSelection.transform as RectTransform;
        selLoc.localPosition = new Vector3(selLoc.localPosition.x, questButton.localPosition.y, selLoc.localPosition.z);

        questDescription.text = quest["description"].AsString + "\n\n"
            + string.Join("\n", quest["completionInfo"].AsArray.Select(x => "<color=" + (x["isCompleted"].AsBool.Value ? "green" : "red") + ">" + x["statusText"].AsString + "</color>"));
    }
}
